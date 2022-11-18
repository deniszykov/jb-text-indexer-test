# Вступление
Разбиение на слова не может быть сделано правильно, по причине того что в мире ~7000 языков со своими правилами и грамматиками. С программной стороны еще есть сотня кодировок и целый ворох знаний в Unicode. 
В этой реализации "слово" это:
 - последовательность Letters, ConnectorPunctuation, ModifierSymbol в классификации Unicode 
 - длиннее 2-х символов 
 - короче 4096-х символов
 - разделенные другими не-Letter символами

# Text Search App
Демонстрационное приложение на WinForms находится в папке TextSearchApp

# Библиотека
"Библиотека" находится в папке TextSearch.
Основной интерфейс через которые добавляются файлы/директории в каталог выглядит так:
```cs
	/// <summary>
	/// Service providing text indexing and search functionality.
	/// Used in pair with <see cref="ITextLexer{WordT}"/> and <see cref="IWordIndex{WordT}"/> to build text search catalog.
	/// </summary>
	public interface ITextSearchCatalog<in WordT> : IDisposable 
	{
		/// <summary>
		/// Events related to updates/changes in text search catalog.
		/// </summary>
		IObservable<TextSearchCatalogEvent> Events { get; }
		
		/// <summary>
		/// Add specified file to text search index. Data would be available after returned <see cref="Task{T}"/> is finished.
		/// </summary>
		Task<IDisposable> IndexFileAsync(FileInfo fileInfo, bool trackChanges);

		/// <summary>
		/// Add specified directory and it's subdirectories to text search index. Data would be available after returned <see cref="Task{T}"/> is finished.
		/// </summary>
		Task<IDisposable> IndexDirectoryAsync(DirectoryInfo directoryInfo, bool trackChanges);

		/// <summary>
		/// Lookup for <paramref name="word"/> in indexed files and directories and return file names contains this <paramref name="word"/>.
		/// </summary>
		IEnumerable<FileInfo> Search(WordT word);
	}
```

## Кастомизация
Интерфейса каталога реализован в ```TextSearchCatalog```, который требует еще пару сервисов:

Фабрика лексеров, для кастомизации лексеров.
```cs

	public interface ITextLexerFactory<out WordT>
	{
		ITextLexer<WordT> Create(Stream textStream);
	}
```
По дефолту есть ```StringLexer``` который разбирает utf-8 поток символов по словам и возвращает слова в виде строк. Его можно заменить на лексер который не алоцирует память в хипе или даже скрестить лексер с тезаурусом и возвращать разобранное слово.

И фабрику строителей индекса для кастомизации построения и доступа к индексу:
```cs
	public interface IWordIndexBuilderFactory<in WordT>
	{
		IWordIndexBuilder<WordT> Create();
	}
```

По дефолту используется ```HashSetIndexBuilderFactory``` который показывает лучший перфоманс из всех реализаций (там есть и Trie), 
но в демо приложении используется ```ConcurrentDictionaryIndexBuilderFactory``` по тому, что он не аллоцирует большие колекции и они не забивает LOH.
