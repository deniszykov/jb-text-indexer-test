namespace TextSearch
{
	/// <summary>
	/// Type used for building <see cref="IWordIndex{WordT}"/>.
	/// </summary>
	public interface IWordIndexBuilder<in WordT>
	{
		/// <summary>
		/// Add new <paramref name="word"/> to index.
		/// </summary>
		void Add(WordT word);
		/// <summary>
		/// Hint builder about used lexer. Lexer and index builder could share state in this method.
		/// </summary>
		/// <param name="lexer">Lexer used for building index.</param>
		void WithLexer(ITextLexer<WordT> lexer);

		/// <summary>
		/// Finish building index and return <see cref="IWordIndex{WordT}"/> instance used to access added words.
		/// Instance of <see cref="IWordIndexBuilder{WordT}"/> should be discarded after this call.
		/// </summary>
		IWordIndex<WordT> Build();
	}
}