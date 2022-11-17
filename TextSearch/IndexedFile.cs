using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TextSearch.Indexes;

namespace TextSearch
{
	internal class IndexedFile<WordT> : IDisposable
	{
		private readonly ITextLexerFactory<WordT> textLexerFactory;
		private readonly IWordIndexBuilderFactory<WordT> wordIndexBuilderFactory;
		private readonly Action<FileInfo> indexRebuildCallback;
		private readonly Action<FileInfo> disposeCallback;
		private readonly LexingProcessReporter lexingProcessReporter;
		private readonly ILogger logger;
		private volatile IWordIndex<WordT> index;
		private int referenceCount;
		private DisposeState disposeState; // mutable struct

		public int WordCount => this.index.Count;
		public Task LexingCompletion => this.lexingProcessReporter.Completion;
		public FileInfo FileInfo { get; private set; }

		public IndexedFile
		(
			FileInfo fileFileInfo,
			ITextLexerFactory<WordT> textLexerFactory,
			IWordIndexBuilderFactory<WordT> wordIndexBuilderFactory,
			Action<FileInfo> indexRebuildCallback,
			Action<FileInfo> disposeCallback,
			ILogger logger)
		{
			if (fileFileInfo == null) throw new ArgumentNullException(nameof(fileFileInfo));
			if (textLexerFactory == null) throw new ArgumentNullException(nameof(textLexerFactory));
			if (wordIndexBuilderFactory == null) throw new ArgumentNullException(nameof(wordIndexBuilderFactory));
			if (indexRebuildCallback == null) throw new ArgumentNullException(nameof(indexRebuildCallback));
			if (disposeCallback == null) throw new ArgumentNullException(nameof(disposeCallback));
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this.referenceCount = 0;
			this.FileInfo = fileFileInfo;
			this.textLexerFactory = textLexerFactory;
			this.wordIndexBuilderFactory = wordIndexBuilderFactory;
			this.indexRebuildCallback = indexRebuildCallback;
			this.disposeCallback = disposeCallback;
			this.logger = logger;
			this.lexingProcessReporter = new LexingProcessReporter();
			this.index = EmptyWordIndex<WordT>.Instance;
		}

		public bool HasWord(WordT word)
		{
			return this.index.Contains(word);
		}

		public async Task UpdateIndexAsync()
		{
			if (this.disposeState.IsDisposed)
			{
				return; // IndexedFile has been disposed and we could ignore further UpdateIndexAsync() calls
			}

			try
			{
				using var processToken = this.lexingProcessReporter.StartNew(); // each call interrupts the previous one process
				using var fileStream = new FileStream(this.FileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 4096,
					FileOptions.SequentialScan | FileOptions.Asynchronous);
				using var lexer = this.textLexerFactory.Create(fileStream);

				this.logger.LogInformation($"Start building index process #{processToken} for file '{this.FileInfo.Name}'.");

				var newIndexBuilder = this.wordIndexBuilderFactory.Create();
				newIndexBuilder.WithLexer(lexer);
				
				var lexingTime = Stopwatch.StartNew();
				while (await lexer.NextLexemeAsync().ConfigureAwait(false))
				{
					var word = lexer.CurrentLexeme!;
					if (processToken.IsInterrupted || this.disposeState.IsDisposed)
					{
						this.logger.LogInformation($"Index building process #{processToken} for file '{this.FileInfo.Name}' interrupted.");

						return;
					}
					newIndexBuilder.Add(word);

					// processToken.ReportProgress(...)
				}

				var newIndex = newIndexBuilder.Build();

				Interlocked.Exchange(ref this.index, newIndex);

				processToken.MarkSuccessful();

				this.logger.LogInformation($"Index building process #{processToken} for file '{this.FileInfo.Name}' finished successfully " +
					$"in {lexingTime.Elapsed.TotalMilliseconds:F2}ms. Word Count: {newIndex.Count}, File Size: {fileStream.Length / 1024.0:F2} KiB.");
				this.indexRebuildCallback(this.FileInfo);
			}
			catch (Exception lexingError)
			{
				this.logger.LogWarning($"Failed to build index for file '{this.FileInfo.Name}' due error: " + lexingError.Message);
			}

			if (this.disposeState.IsDisposed)
			{
				this.Cleanup();
			}
		}
		private void Cleanup()
		{
			var originalIndex = Interlocked.Exchange(ref this.index, EmptyWordIndex<WordT>.Instance);
			originalIndex.Dispose();
		}

		public void IncrementRefCount()
		{
			Interlocked.Increment(ref this.referenceCount);
		}
		public void ZeroRefCount()
		{
			Interlocked.Exchange(ref this.referenceCount, 0);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if (Interlocked.Decrement(ref this.referenceCount) > 0)
			{
				return; // still used by some one else
			}

			if (!this.disposeState.TrySetDisposing())
			{
				return; // already disposed or disposing
			}

			this.disposeCallback(this.FileInfo);
			this.Cleanup();
			this.lexingProcessReporter.SetCompleted(); // release threads waiting for LexingCompletion task

			this.disposeState.SetDisposed();
		}

		/// <inheritdoc />
		public override string ToString() => $"Tracked File: {this.FileInfo.Name}, Word Count: {this.index.Count}, Ref Count: {this.referenceCount}, Disposed: {this.disposeState.ToString()}";
	}
}