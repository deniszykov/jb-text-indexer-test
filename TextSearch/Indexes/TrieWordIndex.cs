using System;
using System.Linq;
using Gma.DataStructures.StringSearch;

namespace TextSearch.Indexes
{
	public class TrieWordIndex : IWordIndex<string>
	{
		private readonly ITrie<string> trie;
		private readonly int wordCount;

		/// <inheritdoc />
		public int Count => this.wordCount;

		public TrieWordIndex(ITrie<string> trie, int wordCount)
		{
			if (trie == null) throw new ArgumentNullException(nameof(trie));

			this.trie = trie;
			this.wordCount = wordCount;
		}

		/// <inheritdoc />
		public bool Contains(string word)
		{
			return this.trie.Retrieve(word).Any();
		}

		/// <inheritdoc />
		public void Dispose()
		{

		}
	}
}