namespace TextSearch.Indexes
{
	public class HashSetIndexBuilderFactory : IWordIndexBuilderFactory<string>
	{
		/// <inheritdoc />
		public IWordIndexBuilder<string> Create()
		{
			return new HashSetIndexBuilder();
		}
	}
}
