using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Access;
using Contracts;

namespace Services
{
    public class LinearSearchService : ISearchService
    {
        public LinearSearchService()
        {
            MongoDBConnector.CreateSession("nliis");
        }
        
        public IEnumerable<SearchResult> Find(string text)
        {
            var foundDocs = MongoDBConnector.GetAll<TextDocument>()
                .Select(document => new SearchResult
                {
                    Match = document,
                    Occurrences = Regex.Matches(document.Text, text).Count,
                })
                .AsEnumerable();

            return foundDocs;
        }
    }
}