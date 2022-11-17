namespace TextSearch.Indexes
{
	internal class EmptyWordIndex<T> : IWordIndex<T>
	{
		public static IWordIndex<T> Instance => new EmptyWordIndex<T>();

		/// <inheritdoc />
		public int Count => 0;

		/// <inheritdoc />
		public bool Contains(T word)
		{
			return false;
		}

		/// <inheritdoc />
		public void Dispose()
		{

		}
	}
}
