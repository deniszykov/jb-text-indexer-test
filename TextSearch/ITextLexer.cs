using System;
using System.Threading.Tasks;

namespace TextSearch
{
	/// <summary>
	/// Lexer used to split character/binary stream of data into <typeparamref name="WordT"/> lexemes.
	/// </summary>
	/// <typeparam name="WordT">Type of lexemes. Could be <see cref="string"/> or any custom type.</typeparam>
	public interface ITextLexer<out WordT> : IDisposable
	{
		/// <summary>
		/// Current lexeme. Should be not null if <see cref="NextLexemeAsync()"/> returned <value>true</value>.
		/// </summary>
		WordT? CurrentLexeme { get; }

		/// <summary>
		/// Advance lexer to next lexeme.
		/// </summary>
		/// <returns>If <value>true</value> is returned then <see cref="CurrentLexeme"/> contains new lexeme else end of stream is reached.</returns>
		ValueTask<bool> NextLexemeAsync();
	}
}