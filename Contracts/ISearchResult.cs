namespace Contracts
{
    public interface ISearchResult
    {
        IDocument Match { get; set; }
    }
}