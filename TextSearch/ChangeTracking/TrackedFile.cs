using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TextSearch.ChangeTracking
{
	internal sealed class TrackedFile<WordT> : IDisposable
	{
		private readonly FileInfo fileInfo;
		private readonly Func<FileInfo, Task<IndexedFile<WordT>>> fileIndexer;
		private readonly ILogger logger;
		private readonly FileSystemWatcher? watcher;
		private readonly SemaphoreSlim updateLock;
		private IndexedFile<WordT>? indexedFile;
		private DisposeState disposeState; // mutable struct

		public TrackedFile(FileInfo fileInfo, Func<FileInfo, Task<IndexedFile<WordT>>> fileIndexer, ILogger logger)
		{
			if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
			if (fileIndexer == null) throw new ArgumentNullException(nameof(fileIndexer));
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this.fileInfo = fileInfo;
			this.fileIndexer = fileIndexer;
			this.logger = logger;
			this.updateLock = new SemaphoreSlim(1, 1);
			var directoryPath = fileInfo.DirectoryName;

			if (string.IsNullOrEmpty(directoryPath)) return;

			this.watcher = new FileSystemWatcher(directoryPath, fileInfo.Name)
			{
				IncludeSubdirectories = false,
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
				}

				this.indexedFile = await this.fileIndexer(this.fileInfo).ConfigureAwait(false);
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
				this.logger.LogInformation($"File '{e.FullPath}' has been changed and re-indexing scheduled.");

				await this.updateLock.WaitAsync().ConfigureAwait(false);
			}
			catch (ObjectDisposedException)
			{
				return; // failed to take lock because TrackedDirectory is disposed
			}

			try
			{
				this.indexedFile?.UpdateIndexAsync();
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
				this.logger.LogInformation($"File '{e.FullPath}' is deleted and will be deleted from index.");

				this.indexedFile?.ZeroRefCount();
				this.indexedFile?.Dispose(); // release indexed file
				this.indexedFile = null;
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
				if (this.indexedFile != null)
				{
					return; // already found
				}

				var newFileInfo = new FileInfo(e.FullPath);
				this.indexedFile = await this.fileIndexer(newFileInfo).ConfigureAwait(false);

				this.logger.LogInformation($"A new file '{e.FullPath}' is discovered and added to index.");

			}
			finally
			{
				this.updateLock.Release();
			}
		}

		private void FileRenamed(object sender, RenamedEventArgs e)
		{
			if (string.Equals(e.OldFullPath, this.fileInfo.FullName))
			{
				this.FileDeleted(sender, new FileSystemEventArgs(WatcherChangeTypes.Deleted, this.fileInfo.DirectoryName!, e.OldName));
			}
			else if (string.Equals(e.FullPath, this.fileInfo.FullName))
			{
				this.FileCreated(sender, new FileSystemEventArgs(WatcherChangeTypes.Created, this.fileInfo.DirectoryName!, e.Name));
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

				this.indexedFile?.Dispose();
				this.indexedFile = null;
				this.disposeState.SetDisposed();
			}, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
		}

		/// <inheritdoc />
		public override string ToString() => $"Tracked File: {this.fileInfo.Name}, Exists: {this.fileInfo.Exists}, Disposed: {this.disposeState.ToString()}";
	}
}
