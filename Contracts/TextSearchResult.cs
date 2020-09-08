using System;

namespace Contracts
{
    public class TextSearchResult
    {
        public IDocument Document { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public uint Occurrences { get; set; }
    }
}