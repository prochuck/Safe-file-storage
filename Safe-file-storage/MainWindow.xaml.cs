using Safe_file_storage.Models;
using Safe_file_storage.Models.FileAtributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Configuration.Internal;
using System.IO;
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

namespace Safe_file_storage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        List<string> strings;

        ReadOnlyCollection<string> Strings { get { return new ReadOnlyCollection<string>(strings); } }

        ObservableCollection<string> Strings2;

        public MainWindow()
        {
            InitializeComponent();


            strings = new List<string>();
            strings.Add("123");
            strings.Add("124");

            Strings2=new ObservableCollection<string>(Strings);
            this.DataContext = Strings2;
            
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            strings.Add("321");
           // strings = new List<string>(new string[] { "323","321"});
        }
    }
}
