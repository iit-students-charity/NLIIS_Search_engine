using System.Collections.Generic;
using System.Linq;
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
                ListView_FoundDocuments.ItemsSource = _searchService
                    .Find(TextBox_Search.Text)?
                    .Select(result => new ListViewItem
                    {
                        Content = 
                            result.Match.Id.ToString().Substring(0, 10) + "...\t| " 
                            + result.Match.CreatedAt.ToString("dd/MM/yyyy") + " | "
                            + result.Match.Title,
                    });
                ListView_FoundDocuments.Visibility = ListView_FoundDocuments.ItemsSource == null
                    ? Visibility.Hidden
                    : Visibility.Visible;
                Label_NoFoundDocuments.Visibility = ListView_FoundDocuments.Visibility == Visibility.Visible
                    ? Visibility.Hidden
                    : Visibility.Visible;
            }
        } 
    }
}