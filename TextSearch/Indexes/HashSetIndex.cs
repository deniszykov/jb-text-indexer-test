using System.Collections.Generic;

namespace TextSearch.Indexes
{
	internal class HashSetIndex : IWordIndex<string>
	{
		private readonly HashSet<string> wordSet;
		private readonly int wordCount;

		/// <inheritdoc />
		public int Count => this.wordCount;

		public HashSetIndex(HashSet<string> wordSet, int wordCount)
		{
			this.wordSet = wordSet;
			this.wordCount = wordCount;
		}

		/// <inheritdoc />
		public bool Contains(string word)
		{
			return this.wordSet.Contains(word);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			this.wordSet.Clear();
		}
	}
}