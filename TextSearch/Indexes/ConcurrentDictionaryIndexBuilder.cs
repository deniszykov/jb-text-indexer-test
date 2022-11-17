using System;
using System.Collections.Concurrent;

namespace TextSearch.Indexes
{
	public class ConcurrentDictionaryIndexBuilder : IWordIndexBuilder<string>
	{
		private readonly ConcurrentDictionary<string, string> dictionary = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		private int wordCount;

		/// <inheritdoc />
		public void Add(string word)
		{
			this.wordCount++;
			this.dictionary.TryAdd(word, word);
		}
		/// <inheritdoc />
		public void WithLexer(ITextLexer<string> lexer)
		{

		}
		/// <inheritdoc />
		public IWordIndex<string> Build()
		{
			return new ConcurrentDictionaryIndex(this.dictionary, this.wordCount);
		}
	}
}