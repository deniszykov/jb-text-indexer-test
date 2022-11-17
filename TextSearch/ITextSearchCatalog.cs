using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TextSearch
{
	/// <summary>
	/// Service providing text indexing and search functionality.
	/// Used in pair with <see cref="ITextLexer{WordT}"/> and <see cref="IWordIndex{WordT}"/> to build text search catalog.
	/// </summary>
	/// <typeparam name="WordT">Type of word object which is indexed.</typeparam>
	public interface ITextSearchCatalog<in WordT> : IDisposable 
	{
		/// <summary>
		/// Events related to updates/changes in text search catalog.
		/// </summary>
		IObservable<TextSearchCatalogEvent> Events { get; }
		
		/// <summary>
		/// Add specified file to text search index. Data would be available after returned <see cref="Task{T}"/> is finished.
		/// </summary>
		/// <param name="fileInfo">File to index.</param>
		/// <param name="trackChanges">True to enable file status tracking. Removal, re-creation and change events are observed and reacted.</param>
		/// <returns>Task returning <see cref="IDisposable"/> which controls lifetime of indexed file.
		/// Calling <see cref="IDisposable.Dispose()"/> would release indexed file.</returns>
		Task<IDisposable> IndexFileAsync(FileInfo fileInfo, bool trackChanges);

		/// <summary>
		/// Add specified directory and it's subdirectories to text search index. Data would be available after returned <see cref="Task{T}"/> is finished.
		/// </summary>
		/// <param name="directoryInfo">Directory to index.</param>
		/// <param name="trackChanges">True to enable directory status tracking. Removal, re-creation and change events are observed and reacted.</param>
		/// <returns>Task returning <see cref="IDisposable"/> which controls lifetime of indexed directory and all it's files.
		/// Calling <see cref="IDisposable.Dispose()"/> would release indexed directory.</returns>
		Task<IDisposable> IndexDirectoryAsync(DirectoryInfo directoryInfo, bool trackChanges);
		
		/// <summary>
		/// Lookup for <paramref name="word"/> in indexed files and directories and return file names contains this <paramref name="word"/>.
		/// </summary>
		/// <param name="word">Word to lookup.</param>
		/// <returns>Found files which contains <paramref name="word"/>. Could be empty enumerable.</returns>
		IEnumerable<FileInfo> Search(WordT word);
	}
}
