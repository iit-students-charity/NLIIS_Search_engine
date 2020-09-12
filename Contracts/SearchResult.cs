using System;

namespace Contracts
{
    public class SearchResult
    {
        public IDocument Document { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public uint Occurrences { get; set; }
    }
}