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
using System.Windows.Shapes;

namespace Safe_file_storage.Views
{
    /// <summary>
    /// Interaction logic for DirectoryNameEnterView.xaml
    /// </summary>
    public partial class DirectoryNameEnterView : Window
    {
        public DirectoryNameEnterView()
        {
            InitializeComponent();
        }
        public string Input
        {
            get
            {
                return textBox.Text;
            }
        }
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
