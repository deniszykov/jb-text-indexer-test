using System;
using System.Collections.Concurrent;

namespace TextSearch.Indexes
{
	public class ConcurrentDictionaryIndex : IWordIndex<string>
	{
		private readonly ConcurrentDictionary<string, string> dictionary;
		private readonly int wordCount;

		/// <inheritdoc />
		public int Count => this.wordCount;

		public ConcurrentDictionaryIndex(ConcurrentDictionary<string, string> dictionary, int wordCount)
		{
			if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

			this.dictionary = dictionary;
			this.wordCount = wordCount;
		}

		/// <inheritdoc />
		public bool Contains(string word)
		{
			return this.dictionary.ContainsKey(word);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			this.dictionary.Clear();
		}
	}
}