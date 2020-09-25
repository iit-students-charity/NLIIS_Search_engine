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
        
        public IEnumerable<SearchResult> Find(string text)
        {
            var foundDocs = new List<SearchResult>();
            
            foreach (var document in MongoDBConnector.GetAll<TextDocument>("text_documents"))
            {
                var matches = Regex.Matches(document.Text, text);

                if (matches.Count > 0)
                {
                    foundDocs.Add(new SearchResult
                    {
                        Match = document,
                        Occurrences = matches.Count,
                    });
                }
            }

            return foundDocs.Any() ? foundDocs : null;
        }
    }
}