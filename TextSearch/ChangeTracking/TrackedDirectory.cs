using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TextSearch.ChangeTracking
{
	internal sealed class TrackedDirectory<WordT> : IDisposable
	{
		private readonly DirectoryInfo directoryInfo;
		private readonly Func<FileInfo, Task<IndexedFile<WordT>>> fileIndexer;
		private readonly ConcurrentDictionary<string, IndexedFile<WordT>> indexedFiles;
		private readonly SemaphoreSlim updateLock;
		private readonly FileSystemWatcher? watcher;
		private readonly ILogger logger;
		private DisposeState disposeState; // mutable struct

		public TrackedDirectory(DirectoryInfo directoryInfo, Func<FileInfo, Task<IndexedFile<WordT>>> fileIndexer, ILogger logger)
		{
			if (directoryInfo == null) throw new ArgumentNullException(nameof(directoryInfo));
			if (fileIndexer == null) throw new ArgumentNullException(nameof(fileIndexer));
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this.indexedFiles = new ConcurrentDictionary<string, IndexedFile<WordT>>(StringComparer.Ordinal);
			this.updateLock = new SemaphoreSlim(1, 1);
			this.directoryInfo = directoryInfo;
			this.fileIndexer = fileIndexer;
			this.logger = logger;
			var directoryPath = directoryInfo.FullName;

			if (string.IsNullOrEmpty(directoryPath)) return;

			this.watcher = new FileSystemWatcher(directoryPath)
			{
				IncludeSubdirectories = true,
				NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
			};
			this.watcher.Deleted += this.FileDeleted;
			this.watcher.Changed += this.FileChanged;
			this.watcher.Created += this.FileCreated;
			this.watcher.Renamed += this.FileRenamed;
		}


		public async Task StartTrackingAsync()
		{
			await this.updateLock.WaitAsync(CancellationToken.None).ConfigureAwait(false);
			try
			{
				if (this.watcher != null)
				{
					this.watcher.EnableRaisingEvents = true;
					this.logger.LogWarning($"Watching for changes in '{this.directoryInfo.FullName}'.");
				}
				else
				{
					this.logger.LogWarning($"Unable to watch for changes in '{this.directoryInfo.Name}' because full path is unavailable.");
				}

				var files = this.directoryInfo.GetFiles("*", SearchOption.AllDirectories);
				var indexedFiles = await Task.WhenAll(
					Array.ConvertAll(files, fileInfo => this.fileIndexer(fileInfo))
				).ConfigureAwait(false);

				foreach (var indexedFile in indexedFiles)
				{
					var fileName = indexedFile.FileInfo.FullName;
					this.indexedFiles.TryAdd(fileName, indexedFile);
				}
			}
			finally
			{
				this.updateLock.Release();
			}
		}

		private async void FileChanged(object sender, FileSystemEventArgs e)
		{
			try
			{
				await this.updateLock.WaitAsync().ConfigureAwait(false);
			}
			catch (ObjectDisposedException)
			{
				return; // failed to take lock because TrackedDirectory is disposed
			}
			try
			{
				this.logger.LogInformation($"File '{e.FullPath}' has been changed and re-indexing scheduled.");

				if (!this.indexedFiles.TryGetValue(e.FullPath, out var indexedFile))
				{
					return;
				}

#pragma warning disable 4014 // intended to be executed in background
				indexedFile.UpdateIndexAsync();
#pragma warning restore 4014
			}
			finally
			{
				this.updateLock.Release();
			}
		}
		private async void FileDeleted(object sender, FileSystemEventArgs e)
		{
			try
			{
				await this.updateLock.WaitAsync().ConfigureAwait(false);
			}
			catch (ObjectDisposedException)
			{
				return; // failed to take lock because TrackedDirectory is disposed
			}
			try
			{
				if (!this.indexedFiles.TryRemove(e.FullPath, out var indexedFile))
				{
					return;
				}

				this.logger.LogInformation($"File '{e.FullPath}' is deleted and will be deleted from index.");

				indexedFile.ZeroRefCount();
				indexedFile.Dispose(); // release indexed file
			}
			finally
			{
				this.updateLock.Release();
			}
		}
		private async void FileCreated(object sender, FileSystemEventArgs e)
		{
			try
			{
				await this.updateLock.WaitAsync().ConfigureAwait(false);
			}
			catch (ObjectDisposedException)
			{
				return; // failed to take lock because TrackedDirectory is disposed
			}
			try
			{
				if (this.indexedFiles.ContainsKey(e.FullPath))
				{
					return; // already indexed
				}

				var newFileInfo = new FileInfo(e.FullPath);
				var indexedFile = await this.fileIndexer(newFileInfo).ConfigureAwait(false);
				if (!this.indexedFiles.TryAdd(e.FullPath, indexedFile))
				{
					indexedFile.Dispose();
					return;
				}

				this.logger.LogInformation($"A new file '{e.FullPath}' is discovered and added to index.");
			}
			finally
			{
				this.updateLock.Release();
			}
		}
		private void FileRenamed(object sender, RenamedEventArgs e)
		{
			var oldDirectoryName = Path.GetDirectoryName(e.OldFullPath);
			if (oldDirectoryName != null && oldDirectoryName.StartsWith(this.directoryInfo.FullName, StringComparison.Ordinal))
			{
				this.FileDeleted(sender, new FileSystemEventArgs(WatcherChangeTypes.Deleted, oldDirectoryName, e.OldName));
			}
			var newDirectoryName = Path.GetDirectoryName(e.FullPath);
			if (newDirectoryName != null && newDirectoryName.StartsWith(this.directoryInfo.FullName, StringComparison.Ordinal))
			{
				this.FileCreated(sender, new FileSystemEventArgs(WatcherChangeTypes.Created, newDirectoryName, e.Name));
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if (!this.disposeState.TrySetDisposing())
			{
				return; // already disposed
			}

			this.watcher?.Dispose();
			this.updateLock.WaitAsync().ContinueWith(waitLockTask =>
			{
				using var _ = this.updateLock; // make sure update lock is disposed

				foreach (var indexedFileByPath in this.indexedFiles)
				{
					var indexedFile = indexedFileByPath.Value;
					indexedFile?.Dispose();
					indexedFile = null;
				}

				this.disposeState.SetDisposed();
			}, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
		}

		/// <inheritdoc />
		public override string ToString() => $"Tracked Directory: {this.directoryInfo.Name}, Exists: {this.directoryInfo.Exists}, Disposed: {this.disposeState.ToString()}";
	}
}
