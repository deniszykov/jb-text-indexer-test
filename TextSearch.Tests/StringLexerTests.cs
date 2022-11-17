using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TextSearch.Lexers;
using Xunit;

namespace TextSearch.Tests
{
	public class StringLexerTests
	{
		[Theory]
		[InlineData("aaa ", new[] { "aaa" })]
		[InlineData(" aaa ", new[] { "aaa" })]
		[InlineData(" aaa", new[] { "aaa" })]
		[InlineData(" aaa bbb", new[] { "aaa", "bbb" })]
		[InlineData(" aa bb  ", new[] { "aa", "bb" })]
		[InlineData(" aa bb cc", new[] { "aa", "bb", "cc" })]
		[InlineData(" aa\tbb,cc", new[] { "aa", "bb", "cc" })]
		[InlineData(" aa\nbb\rcc", new[] { "aa", "bb", "cc" })]
		[InlineData("\r\naa\r\nbb\r\rcc", new[] { "aa", "bb", "cc" })]
		[InlineData("\0aa&bb%cc", new[] { "aa", "bb", "cc" })]
		[InlineData("\0aa&b%cc", new[] { "aa", "cc" })]
		public async Task NextLexemeAsyncTest(string text, string[] expectedLexemes)
		{
			var textBytes = Encoding.UTF8.GetBytes(text);
			var textStream = new MemoryStream(textBytes);
			var lexer = new StringLexer(textStream, Encoding.UTF8);

			var actualLexemes = new List<string>();
			while (await lexer.NextLexemeAsync())
			{
				actualLexemes.Add(lexer.CurrentLexeme);
			}

			Assert.Equal(expectedLexemes, actualLexemes.ToArray());
		}
	}
}
