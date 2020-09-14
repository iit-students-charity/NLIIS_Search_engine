﻿using System.Collections.Generic;
using Contracts;

namespace Services
{
    public interface ISearchService
    {
        IEnumerable<SearchResult> Find(string text);
    }
}