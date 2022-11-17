using System.Threading;

namespace TextSearch
{
	internal struct DisposeState
	{
		private const int DISPOSE_STATE_ALIVE = 0;
		private const int DISPOSE_STATE_DISPOSING = 1;
		private const int DISPOSE_STATE_DISPOSED = 2;

		private volatile int disposeState;

		public bool IsDisposed => this.disposeState >= DISPOSE_STATE_DISPOSING;

		public bool TrySetDisposing()
		{
			return Interlocked.CompareExchange(ref this.disposeState, DISPOSE_STATE_DISPOSING, DISPOSE_STATE_ALIVE) == DISPOSE_STATE_ALIVE;
		}
		public void SetDisposed()
		{
			Interlocked.Exchange(ref this.disposeState, DISPOSE_STATE_DISPOSED);
		}

		/// <inheritdoc />
		public override string ToString() => this.disposeState switch
		{
			DISPOSE_STATE_ALIVE => "Alive",
			DISPOSE_STATE_DISPOSING => "Disposing",
			DISPOSE_STATE_DISPOSED => "Disposed",
			_ => "Unknown"
		};
	}
}