namespace TextSearch.Indexes
{
	public class ConcurrentDictionaryIndexBuilderFactory : IWordIndexBuilderFactory<string>
	{
		/// <inheritdoc />
		public IWordIndexBuilder<string> Create()
		{
			return new ConcurrentDictionaryIndexBuilder();
		}
	}
}
