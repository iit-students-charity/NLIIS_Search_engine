using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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
            Label_NoFoundDocuments.Content = "No documents found";
            
            if (string.IsNullOrEmpty(TextBox_Search.Text))
            {
                ListView_FoundDocuments.ItemsSource = null;
                ListView_FoundDocuments.Visibility = Visibility.Hidden;
                Label_NoFoundDocuments.Visibility = Visibility.Visible;
            }
            else
            {
                var searchStrings = FormQuery(TextBox_Search.Text);
                
                ListView_FoundDocuments.ItemsSource = _searchService
                    .Find(searchStrings)?
                    .Select(result => new ListViewItem
                    {
                        Content = 
                            result.Match.Id.ToString().Substring(0, 11) + "...\t " +
                            result.Match.CreatedAt.ToString("dd/MM/yyyy") + "\t " +
                            result.Occurrences + "\t\t " +
                            (result.Match.Title.Length > 20
                                ? result.Match.Title.Substring(0, 20) + "..."
                                : result.Match.Title),
                    });
                ListView_FoundDocuments.Visibility = ListView_FoundDocuments.ItemsSource == null
                    ? Visibility.Hidden
                    : Visibility.Visible;
                Label_NoFoundDocuments.Visibility = ListView_FoundDocuments.Visibility == Visibility.Visible
                    ? Visibility.Hidden
                    : Visibility.Visible;
                Label_TableHeader.Visibility = ListView_FoundDocuments.Visibility;
            }
        }

        private IEnumerable<string> FormQuery(string query)
        {
            var searchStrings = new List<string>();
            var atoms = GetAtoms(query);
            var valueSets = GetValueSets(atoms);
            var positiveResultValueSets = GetPositiveResultValueSets(valueSets, query, atoms);

            foreach (var setNumber in positiveResultValueSets)
            {
                searchStrings.AddRange(atoms.Where(
                    (atom, index) => valueSets.ElementAt(setNumber).ElementAt(index)));
            }
            
            return searchStrings;
        }

        private bool IsQueryValid(string query)
        {
            if (!(Regex.Match(query, @"([|&~]|->)", RegexOptions.Compiled).Length != 0 ||
                (Regex.Match(query, @"\)\(", RegexOptions.Compiled).Length == 0 &&
                 Regex.Match(query, @"[A-Z01]([^|&~]|(?!->))[A-Z01]", RegexOptions.Compiled).Length == 0 &&
                 Regex.Match(query, @"[^(]![A-Z01]", RegexOptions.Compiled).Length == 0 &&
                 Regex.Match(query, @"![A-Z01][^)]", RegexOptions.Compiled).Length == 0 &&
                 Regex.Match(query, @"\([A-Z01]\)", RegexOptions.Compiled).Length == 0)))
            {
                return false;
            }
            
            var openedBraces = query.Split('(').Length - 1;
            var closedBraces = query.Split(')').Length - 1;

            if (openedBraces != closedBraces)
            {
                return false;
            }
            
            var queryCopy = new string(query);
            var replacement = "A";

            while (Regex.Match(queryCopy, @"([|&~]|->)", RegexOptions.Compiled).Length != 0 ||
                   Regex.Match(queryCopy, @"^[" + replacement + "()]+$", RegexOptions.Compiled).Length == 0)
            {
                var previousCopy = new string(queryCopy);

                queryCopy = Regex.Replace(queryCopy, @"\(![A-Z01]\)", replacement);
                queryCopy = Regex.Replace(queryCopy, @"\([A-Z01]([|&~]|->)[A-Z01]\)", replacement);

                if (queryCopy.Equals(previousCopy))
                {
                    return false;
                }
            }

            return queryCopy.Equals(replacement);
        }

        private IEnumerable<int> GetPositiveResultValueSets(
            IEnumerable<IEnumerable<bool>> sets,
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
                    regex.Replace(queryWithValues, sets.ElementAt(valueSetNumber).ElementAt(atomIndex) ? "1" : "0");
                }

                var queryResult = CalculateBooleanQueryResult(queryWithValues);

                if (queryResult)
                {
                    positiveResultValueSets.Add(valueSetNumber);
                }
            }

            return positiveResultValueSets;
        }
        
        private ISet<string> GetAtoms(string query)
        {
            var strings = query
                .Split('!', '&', '|')
                .Where(atom => !atom.Equals(string.Empty))
                .Select(atom => atom.StartsWith("\"") ? atom : atom.Trim());
            var atoms = new HashSet<string>(strings);

            return atoms;
        }

        private IEnumerable<IEnumerable<bool>> GetValueSets(IEnumerable<string> atoms)
        {
            var sets = new List<List<bool>>();

            for (var row = 0; row < Math.Pow(2, atoms.Count()); row++)
            {
                var newSet = new List<bool>();
                sets.Add(newSet);
                var binaryNumber = Convert.ToString(row, 2)
                    .Split(string.Empty)
                    .Where(digit => !digit.Equals(string.Empty))
                    .Select(digit => digit.Equals("1"));

                if (binaryNumber.Count() < atoms.Count())
                {
                    var zerosToAdd = atoms.Count() - binaryNumber.Count();

                    for (var zeroIndex = 0; zeroIndex < zerosToAdd; zeroIndex++)
                    {
                        newSet.Add(false);
                    }
                }
                
                newSet.AddRange(binaryNumber);
            }

            return sets;
        }

        private bool CalculateBooleanQueryResult(string queryWithValues)
        {
            while (Regex.Match(queryWithValues, "[!|&~]|->").Length != 0) {
                queryWithValues = Regex.Replace(queryWithValues, @"\(?!0\)?", "1");
                queryWithValues = Regex.Replace(queryWithValues, @"\(?!1\)?", "0");

                queryWithValues = Regex.Replace(queryWithValues, @"(\([10]\|1\))|(\(1\|[10]\))", "1");
                queryWithValues = Regex.Replace(queryWithValues, @"(\(0\|0\))", "0");
        
                queryWithValues = Regex.Replace(queryWithValues, @"(\([10]\&0\))|(\(0\&[10]\))", "0");
                queryWithValues = Regex.Replace(queryWithValues, @"(\(1\&1\))", "1");

                queryWithValues = Regex.Replace(queryWithValues, @"\(1->0\)", "0");
                queryWithValues = Regex.Replace(queryWithValues, @"\([10]->[10]\)", "1");
        
                queryWithValues = Regex.Replace(queryWithValues, @"\(0~0\)|\(1~1\)", "1");
                queryWithValues = Regex.Replace(queryWithValues, @"\(([10])~[10]\)", "0");
            }

            return queryWithValues.Equals("1");
        }
    }
}
