using System;

namespace TextSearch
{
	/// <summary>
	/// Collection of words and interface for containment check.
	/// </summary>
	public interface IWordIndex<in WordT> : IDisposable
	{
		/// <summary>
		/// Total non-unique words added into index.
		/// </summary>
		int Count { get; }
		
		/// <summary>
		/// Check if word present in index.
		/// </summary>
		/// <param name="word">Word to check.</param>
		bool Contains(WordT word);
	}
}