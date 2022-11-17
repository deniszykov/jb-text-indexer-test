using System;
using System.Collections.Generic;

namespace TextSearch.Indexes
{
	internal class HashSetIndexBuilder : IWordIndexBuilder<string>
	{
		private readonly HashSet<string> list = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private int wordCount;

		/// <inheritdoc />
		public void Add(string word)
		{
			this.wordCount++;
			this.list.Add(word);
		}
		/// <inheritdoc />
		public void WithLexer(ITextLexer<string> lexer)
		{
			
		}
		/// <inheritdoc />
		public IWordIndex<string> Build()
		{
			return new HashSetIndex(this.list, this.wordCount);
		}
	}
}