using System;
using System.IO;
using System.Text;

namespace TextSearch.Lexers
{
	public sealed class StringLexerFactory : ITextLexerFactory<string>
	{
		private readonly Encoding encoding;

		public StringLexerFactory(Encoding encoding)
		{
			if (encoding == null) throw new ArgumentNullException(nameof(encoding));

			this.encoding = encoding;
		}

		/// <inheritdoc />
		public ITextLexer<string> Create(Stream textStream)
		{
			return new StringLexer(textStream, this.encoding);
		}
	}
}
