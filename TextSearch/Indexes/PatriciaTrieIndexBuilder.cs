using Gma.DataStructures.StringSearch;

namespace TextSearch.Indexes
{
	public class PatriciaTrieIndexBuilder : IWordIndexBuilder<string>
	{
		private readonly PatriciaTrie<string> trie = new PatriciaTrie<string>();
		private int wordCount;

		/// <inheritdoc />
		public void Add(string word)
		{
			this.wordCount++;
			this.trie.Add(word, word);
		}
		/// <inheritdoc />
		public void WithLexer(ITextLexer<string> lexer)
		{

		}
		/// <inheritdoc />
		public IWordIndex<string> Build()
		{
			return new TrieWordIndex(this.trie, this.wordCount);
		}
	}
}