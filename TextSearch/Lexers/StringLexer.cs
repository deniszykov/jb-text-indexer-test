using System;
using System.Buffers;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TextSearch.Lexers
{
	public class StringLexer : ITextLexer<string>
	{
		private const int BUFFER_SIZE = 4096;

		private readonly StreamReader streamReader;
		private ArraySegment<char> charBuffer;

		public string? CurrentLexeme { get; private set; }

		public StringLexer(Stream stream, Encoding encoding)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			if (encoding == null) throw new ArgumentNullException(nameof(encoding));
			if (!stream.CanRead) throw new ArgumentException("Readable stream expected.", nameof(stream));

			this.streamReader = new StreamReader(stream, encoding);
			this.charBuffer = new ArraySegment<char>(ArrayPool<char>.Shared.Rent(BUFFER_SIZE), 0, 0);
		}

		/// <inheritdoc />
		public async ValueTask<bool> NextLexemeAsync()
		{
			var endOfFile = false;
			var extraLongWord = false;
			do
			{
				this.SkipNonLetters(ref this.charBuffer);
				var nextSeparatorIndex = IndexOfNonLetter(this.charBuffer);
				if (nextSeparatorIndex < 0 && endOfFile)
				{
					nextSeparatorIndex = this.charBuffer.Count; // last word in file
				}

				if (nextSeparatorIndex < 0 && this.charBuffer.Count == this.charBuffer.Array!.Length)
				{
					this.charBuffer = new ArraySegment<char>(this.charBuffer.Array, 0, 0); // discard buffer
					extraLongWord = true;
					continue;
				}

				if (nextSeparatorIndex >= 0)
				{
					var startIndex = this.charBuffer.Offset;
					var wordLength = nextSeparatorIndex;
					var endIndex = startIndex + wordLength;

					this.charBuffer = new ArraySegment<char>(this.charBuffer.Array!, endIndex, this.charBuffer.Count - wordLength);

					if (extraLongWord)
					{
						extraLongWord = false;
						continue; // skip extra long words
					}


					if (IsProperWord(this.charBuffer.Array!.AsSpan(startIndex, wordLength)))
					{
						this.CurrentLexeme = new string(this.charBuffer.Array!, startIndex, wordLength);
						return true;
					}
				}
				else
				{
					this.CopyTailToBeginning(ref this.charBuffer);

					var toRead = this.charBuffer.Array!.Length - this.charBuffer.Count - this.charBuffer.Offset;
					var read = await this.streamReader.ReadAsync(this.charBuffer.Array!, this.charBuffer.Offset + this.charBuffer.Count, toRead).ConfigureAwait(false);
					endOfFile = read == 0;
					this.charBuffer = new ArraySegment<char>(this.charBuffer.Array!, this.charBuffer.Offset, this.charBuffer.Count + read);
				}
			} while (!endOfFile || this.charBuffer.Count > 0);

			return false; // end of file
		}

		private void SkipNonLetters(ref ArraySegment<char> charsSegment)
		{
			var chars = charsSegment.Array!;
			var count = charsSegment.Count;
			var start = charsSegment.Offset;
			var end = start + count;
			for (var i = start; i < end; i++, count--)
			{
				if (!IsLetter(chars[i])) continue;

				charsSegment = new ArraySegment<char>(chars, i, count);
				return;
			}

			charsSegment = new ArraySegment<char>(chars, 0, 0); // all chars are whitespaces
		}
		private static int IndexOfNonLetter(Span<char> chars)
		{
			for (var i = 0; i < chars.Length; i++)
			{
				if (IsLetter(chars[i]))
				{
					continue;
				}

				return i;
			}

			return -1;
		}
		private static bool IsLetter(char ch)
		{
			switch (char.GetUnicodeCategory(ch))
			{
				case UnicodeCategory.LowercaseLetter:
				case UnicodeCategory.ModifierLetter:
				case UnicodeCategory.OtherLetter:
				case UnicodeCategory.TitlecaseLetter:
				case UnicodeCategory.UppercaseLetter:
				case UnicodeCategory.ModifierSymbol:
				case UnicodeCategory.ConnectorPunctuation:
					return true;
			}
			return false;
		}
		private static bool IsProperWord(Span<char> chars)
		{
			if (chars.Length <= 1)
			{
				return false;
			}

			return true;
		}

		private void CopyTailToBeginning<T>(ref ArraySegment<T> arraySegment) where T : unmanaged
		{
			if (arraySegment.Offset <= 0) return;
			if (arraySegment.Count == 0)
			{
				arraySegment = new ArraySegment<T>(arraySegment.Array!);
				return;
			}
			Array.ConstrainedCopy(arraySegment.Array!, arraySegment.Offset, arraySegment.Array!, 0, arraySegment.Count);
			arraySegment = new ArraySegment<T>(arraySegment.Array!, 0, arraySegment.Count);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			var charBuffer = this.charBuffer.Array;
			this.charBuffer = new ArraySegment<char>(Array.Empty<char>(), 0, 0);
			if (charBuffer != null && charBuffer.Length != 0)
			{
				ArrayPool<char>.Shared.Return(charBuffer);
			}
		}
	}
}