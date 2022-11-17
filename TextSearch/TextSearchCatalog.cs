using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TextSearch.ChangeTracking;

namespace TextSearch
{
	public sealed class TextSearchCatalog<WordT> : ITextSearchCatalog<WordT>
	{
		private readonly ITextLexerFactory<WordT> textLexerFactory;
		private readonly IWordIndexBuilderFactory<WordT> wordIndexBuilderFactory;
		private readonly ConcurrentDictionary<string, IndexedFile<WordT>> indexedFiles;
		private readonly ISubject<TextSearchCatalogEvent> events;
		private readonly ILogger logger;
		private DisposeState disposeState; // mutable struct

		/// <inheritdoc />
		public IObservable<TextSearchCatalogEvent> Events => this.events.AsObservable();

		public TextSearchCatalog(
			ITextLexerFactory<WordT> textLexerFactory,
			IWordIndexBuilderFactory<WordT> wordIndexBuilderFactory,
			ILogger logger)
		{
			if (textLexerFactory == null) throw new ArgumentNullException(nameof(textLexerFactory));
			if (wordIndexBuilderFactory == null) throw new ArgumentNullException(nameof(wordIndexBuilderFactory));
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this.textLexerFactory = textLexerFactory;
			this.wordIndexBuilderFactory = wordIndexBuilderFactory;
			this.logger = logger;

			var fileSystemPathComparer = StringComparer.Ordinal; // assuming we are on Windows, worst case - on Posix OS one file could be indexed multiple times;

			this.events = new Subject<TextSearchCatalogEvent>();
			this.indexedFiles = new ConcurrentDictionary<string, IndexedFile<WordT>>(fileSystemPathComparer);
		}

		/// <inheritdoc />
		public async Task<IDisposable> IndexFileAsync(FileInfo fileInfo, bool trackChanges)
		{
			if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

			this.ThrowIfDisposed();

			var indexedFile = default(IDisposable);
			if (trackChanges)
			{
				indexedFile = await this.IndexAndTrackFile(fileInfo).ConfigureAwait(false);
			}
			else
			{
				indexedFile = await this.IndexFile(fileInfo).ConfigureAwait(false);
			}

			if (this.disposeState.IsDisposed)
			{
				indexedFile.Dispose();
				this.CleanupIndexedFiles();
				this.ThrowIfDisposed();
			}

			return indexedFile;
		}

		/// <inheritdoc />
		public async Task<IDisposable> IndexDirectoryAsync(DirectoryInfo directoryInfo, bool trackChanges)
		{
			if (directoryInfo == null) throw new ArgumentNullException(nameof(directoryInfo));

			this.ThrowIfDisposed();

			var indexedDirectory = default(IDisposable);
			if (trackChanges)
			{
				indexedDirectory = await this.IndexAndTrackDirectory(directoryInfo).ConfigureAwait(false);
			}
			else
			{
				indexedDirectory = await this.IndexDirectory(directoryInfo).ConfigureAwait(false);
			}

			if (this.disposeState.IsDisposed)
			{
				indexedDirectory.Dispose();
				this.CleanupIndexedFiles();
				this.ThrowIfDisposed();
			}

			return indexedDirectory;
		}

		/// <inheritdoc />
		public IEnumerable<FileInfo> Search(WordT word)
		{
			this.ThrowIfDisposed();

			foreach (var indexedFileByName in this.indexedFiles)
			{
				var indexedFile = indexedFileByName.Value;
				if (indexedFile.HasWord(word))
				{
					yield return indexedFile.FileInfo;
				}
			}
		}

		private async Task<IDisposable> IndexAndTrackFile(FileInfo fileInfo)
		{
			if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

			var trackedFile = new TrackedFile<WordT>(fileInfo, this.IndexFile, this.logger);

			await trackedFile.StartTrackingAsync().ConfigureAwait(false);

			return trackedFile;
		}
		private async Task<IndexedFile<WordT>> IndexFile(FileInfo fileInfo)
		{
			if (fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));

			while (true)
			{
				var fullName = fileInfo.FullName;
				if (this.indexedFiles.TryGetValue(fullName, out var indexedFile))
				{
					indexedFile.IncrementRefCount();
					await indexedFile.LexingCompletion.ConfigureAwait(false);
					return indexedFile;
				}

				indexedFile = new IndexedFile<WordT>(
					fileInfo, 
					this.textLexerFactory, 
					this.wordIndexBuilderFactory, 
					this.OnFileIndexUpdated,
					this.OnFileIndexDisposed, 
					this.logger
				);
				if (!this.indexedFiles.TryAdd(fullName, indexedFile))
				{
					await Task.Yield();
					continue;
				}

				this.logger.LogInformation($"A new file '{fullName}' is added for indexing.");
				this.events.OnNext(new TextSearchCatalogFileAdded(fileInfo));

				indexedFile.IncrementRefCount();
				await indexedFile.UpdateIndexAsync().ConfigureAwait(false);

				this.logger.LogInformation($"Indexing of file '{fullName}' is completed. Word count: {indexedFile.WordCount}.");

				return indexedFile;
			}
		}

		private async Task<IDisposable> IndexDirectory(DirectoryInfo directoryInfo)
		{
			if (directoryInfo == null) throw new ArgumentNullException(nameof(directoryInfo));

			var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
			var indexedFiles = await Task.WhenAll(
				Array.ConvertAll(files, this.IndexFile)
			).ConfigureAwait(false);

			var indexedDirectory = new IndexedDirectory(directoryInfo, indexedFiles);
			return indexedDirectory;
		}
		private async Task<IDisposable> IndexAndTrackDirectory(DirectoryInfo directoryInfo)
		{
			if (directoryInfo == null) throw new ArgumentNullException(nameof(directoryInfo));

			var trackedDirectory = new TrackedDirectory<WordT>(directoryInfo, this.IndexFile, this.logger);
			await trackedDirectory.StartTrackingAsync().ConfigureAwait(false);
			return trackedDirectory;
		}

		private void ThrowIfDisposed()
		{
			if (this.disposeState.IsDisposed)
			{
				throw new ObjectDisposedException(this.GetType().FullName);
			}
		}

		private void OnFileIndexUpdated(FileInfo fileInfo)
		{
			this.events.OnNext(new TextSearchCatalogFileChanged(fileInfo));
		}
		private void OnFileIndexDisposed(FileInfo fileInfo)
		{
			if (this.indexedFiles.TryRemove(fileInfo.FullName, out _))
			{
				this.logger.LogInformation($"A file '{fileInfo.FullName}' is removed from indexing.");
				this.events.OnNext(new TextSearchCatalogFileRemoved(fileInfo));
			}
		}
		private void CleanupIndexedFiles()
		{
			while (this.indexedFiles.Count > 0)
			{
				foreach (var indexedFile in this.indexedFiles)
				{
					indexedFile.Value.ZeroRefCount();
					indexedFile.Value.Dispose(); // it will remove itself from collection via RemovedIndexedFile() callback
				}
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if (!this.disposeState.TrySetDisposing())
			{
				return; // already disposed or disposing
			}

			this.CleanupIndexedFiles();

			this.disposeState.SetDisposed();
		}
	}
}
