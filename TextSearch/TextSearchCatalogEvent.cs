using System.IO;

namespace TextSearch
{
	public abstract record TextSearchCatalogEvent;

	public record TextSearchCatalogFileAdded(FileInfo File) : TextSearchCatalogEvent;
	public record TextSearchCatalogFileRemoved(FileInfo File) : TextSearchCatalogEvent;
	public record TextSearchCatalogFileChanged(FileInfo File) : TextSearchCatalogEvent;

}