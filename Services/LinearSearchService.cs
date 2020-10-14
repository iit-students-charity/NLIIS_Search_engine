using System;
using System.Collections.Generic;
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
        
        public IEnumerable<SearchResult> Find(IEnumerable<string> searchStrings)
        {
            var foundDocs = new List<SearchResult>();
            
            foreach (var document in MongoDBConnector.GetAll<TextDocument>("text_documents"))
            {
                var matches = searchStrings
                    .Select(searchString => Regex.Matches(
                        document.Text,
                        searchString.StartsWith("\"")
                            ? searchString.Replace("\"", string.Empty)
                            : searchString,
                        RegexOptions.Compiled))
                    .ToList();

                if (matches.All(match => match.Count > 0))
                {
                    foundDocs.Add(new SearchResult
                    {
                        Match = document,
                        Occurrences = matches.Sum(match => match.Count),
                        SearchedWords = String.Join(" • ", searchStrings)
                    });
                }
            }

            return foundDocs.Any() ? foundDocs : null;
        }
    }
}