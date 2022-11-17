using System.IO;

namespace TextSearch
{
	/// <summary>
	/// Factory providing new instances of <see cref="ITextLexer{WordT}"/> for specified <see cref="Stream"/>.
	/// </summary>
	public interface ITextLexerFactory<out WordT>
	{
		/// <summary>
		/// Create new instance of <see cref="ITextLexer{WordT}"/> for specified <see cref="Stream"/>.
		/// </summary>
		/// <param name="textStream">Readable stream used for lexing. Lexer doesn't own passed <see cref="Stream"/> and will not dispose it upon completion.</param>
		/// <returns></returns>
		ITextLexer<WordT> Create(Stream textStream);
	}
}