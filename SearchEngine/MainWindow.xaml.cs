using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Contracts;
using Services;

namespace SearchEngine
{
    public partial class MainWindow
    {
        private readonly ISearchService _searchService;
        
        public MainWindow()
        {
            _searchService = new LinearSearchService();
            
            InitializeComponent();

            ListView_FoundDocuments.Visibility = Visibility.Hidden;
            Label_TableHeader.Visibility = Visibility.Hidden;
            ListView_FoundDocuments.ItemsSource = null;
        }
        
        private void FindDocuments(object sender, RoutedEventArgs e)
        {
            Label_Information.Content = "No documents found";
            
            if (string.IsNullOrEmpty(TextBox_Search.Text))
            {
                ListView_FoundDocuments.ItemsSource = null;
                ListView_FoundDocuments.Visibility = Visibility.Hidden;
                Label_Information.Visibility = Visibility.Visible;
            }
            else
            {
                var query = TextBox_Search.Text;
                
                if (!IsQueryValid(query))
                {
                    Label_Information.Content = "Enter a valid query";
                    Label_Information.Visibility = Visibility.Visible;
                    ListView_FoundDocuments.Visibility = Visibility.Hidden;
                    Label_TableHeader.Visibility = ListView_FoundDocuments.Visibility;

                    return;
                }
                
                var searchStringsSequences = FormQuery(TextBox_Search.Text);
                var items = new List<ListViewItem>();

                foreach (var searchStrings in searchStringsSequences)
                {
                    var found = _searchService.Find(searchStrings);

                    if (found == null)
                    {
                        continue;
                    }

                    items.Add(new ListViewItem
                    {
                        Content = "\n" + String.Join(" • ", searchStrings) + "\n",
                    });
                    
                    items.AddRange(found
                        .OrderByDescending(d => d.Occurrences)
                        .Select(result => new ListViewItem
                        {
                            Content = 
                                result.Match.Id + "\t " +
                                result.Match.CreatedAt.ToString("dd/MM/yyyy") + "\t " +
                                result.Occurrences + "\t\t " +
                                result.Match.Title,
                        }));
                }

                ListView_FoundDocuments.ItemsSource = items.Any() ? items : null;
                ListView_FoundDocuments.Visibility = ListView_FoundDocuments.ItemsSource == null
                    ? Visibility.Hidden
                    : Visibility.Visible;
                Label_Information.Visibility = ListView_FoundDocuments.Visibility == Visibility.Visible
                    ? Visibility.Hidden
                    : Visibility.Visible;
                Label_TableHeader.Visibility = ListView_FoundDocuments.Visibility;
            }
        }
        
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Type words in search box, then press Search\n" +
                "button to find proper documents.\n" +
                "Boolean operators can be used: &, |, and brackets. Example query:\n" +
                @"we | (are & ""the champions"")",
                "Help");
        }
        
        private void Authors_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Group 721701:\nSemenikhin Nirita,\nStryzhych Angelika",
                "Authors");
        }
        
        private IEnumerable<IEnumerable<string>> FormQuery(string query)
        {
            var atoms = GetAtoms(query);
            var valueSets = GetValueSets(atoms);
            var positiveResultValueSets = GetPositiveResultValueSets(valueSets, query, atoms);

            return positiveResultValueSets
                .Select(setNumber => 
                    atoms.Where((atom, index) => valueSets.ElementAt(setNumber).ElementAt(index) == "1").ToList())
                .ToList();
        }

        private bool IsQueryValid(string query)
        {
            var quotesReplace = new Regex("\"[^\"]+\"", RegexOptions.Compiled);
            query = quotesReplace.Replace(query, "QUOTES");
            
            static bool IsQueryValidGeneral(string queryToValidate)
            {
                var onlyOperators = Regex.Match(queryToValidate, @"^[!|&]+$", RegexOptions.Compiled).Length != 0;
                var flippedBraces = Regex.Match(queryToValidate, @"\)\(", RegexOptions.Compiled).Length != 0;
                var moreThanTwoOperandsWithoutBraces = Regex.Match(queryToValidate, @"[^!|&]+\s+[^!|&]\s+[^!|&]+", RegexOptions.Compiled).Length != 0;
                var noBraceBeforeNegation = Regex.Match(queryToValidate, @"[^(]\s*!", RegexOptions.Compiled).Length != 0;
                var atomInBraces = Regex.Match(queryToValidate, @"\(\s*[^!|&]+\s*\)", RegexOptions.Compiled).Length != 0;
                
                return !(onlyOperators ||
                         flippedBraces ||
                         moreThanTwoOperandsWithoutBraces ||
                         noBraceBeforeNegation ||
                         atomInBraces);
            }

            if (!IsQueryValidGeneral(query))
            {
                return false;
            }
            
            var openedBraces = query.Split('(').Length - 1;
            var closedBraces = query.Split(')').Length - 1;

            if (openedBraces != closedBraces)
            {
                return false;
            }
            
            var queryCopy = new string(query).Insert(query.Length, ")").Insert(0, "(");
            var replacement = "•";

            while (Regex.Match(queryCopy, @"[|&~]", RegexOptions.Compiled).Length != 0 ||
                   Regex.Match(queryCopy, @"^[" + replacement + "()]+$", RegexOptions.Compiled).Length == 0)
            {
                var previousCopy = new string(queryCopy);

                queryCopy = Regex.Replace(queryCopy, @"\(\s*!?[^|&]+\s*\)", replacement);
                queryCopy = Regex.Replace(queryCopy, @"\(\s*[^|&()]+\s+[|&]\s+[^|&()]+\s*\)", replacement);

                if (queryCopy.Equals(previousCopy))
                {
                    return false;
                }
            }

            return queryCopy.Equals(replacement);
        }

        private IEnumerable<int> GetPositiveResultValueSets(
            IEnumerable<IEnumerable<string>> sets,
            string query,
            IEnumerable<string> atoms)
        {
            var positiveResultValueSets = new List<int>();

            for (var valueSetNumber = 0; valueSetNumber < sets.Count(); valueSetNumber++)
            {
                var queryWithValues = new string(query);

                for (var atomIndex = 0; atomIndex < atoms.Count(); atomIndex++)
                {
                    var regex = new Regex(atoms.ElementAt(atomIndex), RegexOptions.Compiled);
                    queryWithValues = regex.Replace(queryWithValues, sets.ElementAt(valueSetNumber).ElementAt(atomIndex));
                }

                var queryResult = CalculateBooleanQueryResult(queryWithValues);

                if (queryResult == "1")
                {
                    positiveResultValueSets.Add(valueSetNumber);
                }
            }

            return positiveResultValueSets;
        }
        
        private ISet<string> GetAtoms(string query)
        {
            var strings = query
                .Split('!', '&', '|', ')', '(')
                .Select(atom => atom.Trim())
                .Where(atom => !atom.Equals(string.Empty));
            var atoms = new HashSet<string>(strings);

            return atoms;
        }

        private IEnumerable<IEnumerable<string>> GetValueSets(IEnumerable<string> atoms)
        {
            var sets = new List<List<string>>();

            for (var row = 0; row < Math.Pow(2, atoms.Count()); row++)
            {
                var newSet = new List<string>();
                var binaryNumber = Convert.ToString(row, 2).ToCharArray();

                if (binaryNumber.Length < atoms.Count())
                {
                    var zerosToAdd = atoms.Count() - binaryNumber.Count();

                    for (var zeroIndex = 0; zeroIndex < zerosToAdd; zeroIndex++)
                    {
                        newSet.Add("0");
                    }
                }
                
                newSet.AddRange(binaryNumber.Select(digit => digit.ToString()));
                sets.Add(newSet);
            }

            return sets;
        }

        private string CalculateBooleanQueryResult(string queryWithValues)
        {
            queryWithValues = PrepareQueryToCalculation(queryWithValues);
            
            while (Regex.Match(queryWithValues, "[!|&~]|->").Length != 0) {
                queryWithValues = Regex.Replace(queryWithValues, @"\(?!0\)?", "1");
                queryWithValues = Regex.Replace(queryWithValues, @"\(?!1\)?", "0");

                queryWithValues = Regex.Replace(queryWithValues, @"(\([10]\|1\))|(\(1\|[10]\))", "1");
                queryWithValues = Regex.Replace(queryWithValues, @"(\(0\|0\))", "0");
        
                queryWithValues = Regex.Replace(queryWithValues, @"(\([10]\&0\))|(\(0\&[10]\))", "0");
                queryWithValues = Regex.Replace(queryWithValues, @"(\(1\&1\))", "1");
            }

            return queryWithValues
                .Replace("(", string.Empty)
                .Replace(")", string.Empty);
        }

        private string PrepareQueryToCalculation(string queryWithValues)
        {
            return queryWithValues
                .Insert(queryWithValues.Length, ")")
                .Insert(0 , "(")
                .Replace(" ", string.Empty);
        }
    }
}
