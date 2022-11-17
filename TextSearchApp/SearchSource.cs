using System;
using System.Threading;
using System.Threading.Tasks;

namespace TextSearchApp
{
	public class SearchSource : IDisposable
	{
		private readonly Task<IDisposable> indexingTask;

		public string Path { get; }
		public string Status => this.indexingTask.IsCompleted ? "Indexed" : "Pending";

		public SearchSource(Task<IDisposable> indexingTask, string name)
		{
			if (indexingTask == null) throw new ArgumentNullException(nameof(indexingTask));
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.indexingTask = indexingTask;
			this.Path = name;
		}

		public void Dispose()
		{
			this.indexingTask.ContinueWith(
				indexingTask => indexingTask.Result.Dispose(),
				CancellationToken.None,
				TaskContinuationOptions.ExecuteSynchronously,
				TaskScheduler.Default
			);
		}

		/// <inheritdoc />
		public override string ToString() => $"[{this.Status}] " + this.Path;
	}
}
