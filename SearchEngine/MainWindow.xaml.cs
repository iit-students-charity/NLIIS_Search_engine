using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Contracts;
using Services;

namespace SearchEngine
{
    public partial class MainWindow
    {
        private readonly ISearchService _searchService;
        
        private readonly TextBox _searchTextBox;
        
        public MainWindow()
        {
            _searchService = new LinearSearchService();
            
            InitializeComponent();
            
            _searchTextBox = (TextBox)FindName("TextBox_Search");
        }
        
        private void FindDocuments(object sender, RoutedEventArgs e)
        {
            FoundDocumentsList.ItemsSource = _searchService
                .Find(_searchTextBox.Text)
                .Select(document => new ListViewItem
                {
                    Content = document.Match.Text
                });
            
            MessageBox.Show("Button is clicked!"); 
        } 
    }
}