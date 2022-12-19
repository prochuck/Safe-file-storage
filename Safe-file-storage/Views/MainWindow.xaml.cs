using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Safe_file_storage.Configurations;
using Safe_file_storage.Models;
using Safe_file_storage.Models.FileAtributes;
using Safe_file_storage.Models.Interfaces;
using Safe_file_storage.Models.Services;
using Safe_file_storage.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Configuration.Internal;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
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



        FileBrowserModel _fileBrowserModel;
        public MainWindow()
        {
            InitializeComponent();


           
            this.DataContext = new FileBrowserViewModel();

          
        }
    }
}
