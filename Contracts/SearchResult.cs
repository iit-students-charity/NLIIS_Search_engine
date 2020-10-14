using System;

namespace Contracts
{
    public class SearchResult : ISearchResult
    {
        public TextDocument Match { get; set; }
        public int Occurrences { get; set; }
        public string SearchedWords { get; set; }
    }
}