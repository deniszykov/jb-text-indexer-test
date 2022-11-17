using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace TextSearch
{
	internal class LexingProcessReporter
	{
		private TaskCompletionSource<object?> completionSource;
		private CancellationTokenSource cancellationSource;

		public Task Completion => this.completionSource.Task;

		public LexingProcessReporter()
		{
			this.completionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
			this.cancellationSource = new CancellationTokenSource();
		}

		[MethodImpl(MethodImplOptions.Synchronized | MethodImplOptions.NoInlining)]
		public ProcessToken StartNew()
		{
			this.cancellationSource.Cancel(); // cancel previous process
			this.cancellationSource.Dispose();

			this.cancellationSource = new CancellationTokenSource();
			if (this.completionSource.Task.IsCompleted)
			{
				this.completionSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
			}

			return new ProcessToken(this.completionSource, this.cancellationSource);
		}

		public void SetCompleted()
		{
			this.completionSource.TrySetResult(default);
		}

		internal struct ProcessToken : IDisposable
		{
			private static int LastProcessNumber = 1;

			private readonly int number;
			private readonly TaskCompletionSource<object?>? completionSource;
			private readonly CancellationTokenSource? cancellationSource;
			private bool isSuccessful;

			public bool IsInterrupted => this.cancellationSource?.IsCancellationRequested ?? false;

			public ProcessToken(TaskCompletionSource<object?> completionSource, CancellationTokenSource cancellationSource)
			{
				if (completionSource == null) throw new ArgumentNullException(nameof(completionSource));
				if (cancellationSource == null) throw new ArgumentNullException(nameof(cancellationSource));

				this.number = Interlocked.Increment(ref LastProcessNumber);
				this.completionSource = completionSource;
				this.cancellationSource = cancellationSource;
				this.isSuccessful = false;
			}

			public void MarkSuccessful()
			{
				this.isSuccessful = true;
			}

			private void Finish()
			{
				if (this.isSuccessful)
				{
					this.completionSource?.TrySetResult(default);
				}
			}

			/// <inheritdoc />
			void IDisposable.Dispose()
			{
				this.Finish();
			}

			/// <inheritdoc />
			public override string ToString() => this.number.ToString();
		}
	}
}
