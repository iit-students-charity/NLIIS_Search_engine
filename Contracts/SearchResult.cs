using System;

namespace Contracts
{
    public class SearchResult : ISearchResult
    {
        public IDocument Match { get; set; }
        public int Occurrences { get; set; }
    }
}