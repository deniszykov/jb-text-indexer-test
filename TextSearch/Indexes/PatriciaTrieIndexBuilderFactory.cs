namespace TextSearch.Indexes
{
	public class PatriciaTrieIndexBuilderFactory : IWordIndexBuilderFactory<string>
	{
		/// <inheritdoc />
		public IWordIndexBuilder<string> Create()
		{
			return new PatriciaTrieIndexBuilder();
		}
	}
}
