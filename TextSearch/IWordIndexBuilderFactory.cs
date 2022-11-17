namespace TextSearch
{
	/// <summary>
	/// Factory providing new instances of <see cref="IWordIndexBuilder{WordT}"/>
	/// </summary>
	public interface IWordIndexBuilderFactory<in WordT>
	{
		/// <summary>
		/// Create new instance of <see cref="IWordIndexBuilder{WordT}"/>
		/// </summary>
		IWordIndexBuilder<WordT> Create();
	}
}