using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TextSearch
{
	internal sealed class IndexedDirectory : IDisposable
	{
		private readonly DirectoryInfo directoryInfo;
		private readonly IEnumerable<IDisposable> indexedFiles;
		private DisposeState disposeState; // mutable struct

		public IndexedDirectory(DirectoryInfo directoryInfo, IReadOnlyCollection<IDisposable> indexedFiles)
		{
			if (directoryInfo == null) throw new ArgumentNullException(nameof(directoryInfo));
			if (indexedFiles == null) throw new ArgumentNullException(nameof(indexedFiles));
			if (indexedFiles.Any(indexedFile => indexedFile == null)) throw new ArgumentException("Array element can't be null.", nameof(indexedFiles));

			this.directoryInfo = directoryInfo;
			this.indexedFiles = indexedFiles;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if (!this.disposeState.TrySetDisposing())
			{
				return; // already disposed
			}

			foreach (var indexedFile in this.indexedFiles)
			{
				indexedFile.Dispose();
			}

			this.disposeState.SetDisposed();
		}

		/// <inheritdoc />
		public override string ToString() => $"Indexed Directory: {this.directoryInfo.Name}, Disposed: {this.disposeState.ToString()}";
	}
}