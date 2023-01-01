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
    /// Interaction logic for WaitView.xaml
    /// </summary>
    public partial class WaitView : Window
    {
        private Task _task;
        public WaitView(Task task)
        {
            InitializeComponent();

            _task = task;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await _task;
            Close();
        }
    }
}
