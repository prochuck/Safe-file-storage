using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
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


            _fileBrowserModel = new FileBrowserModel(new LntfsSecureFileWorker(new config(), new AesCryptoService(new cryptoConfig())));
            this.DataContext = new FileBrowserViewModel(_fileBrowserModel);


            Console.WriteLine();
        }

        struct config : ILntfsConfiguration
        {
            public string FilePath => "123.bin";

            public int MFTRecordSize => 1024;

            public int ClusterSize => 1024;

            public int MFTZoneSize => 1024 * 50;

            public int AttributeHeaderSize => 200;

            public int FileSize => 1024 * 400;
        }
        struct cryptoConfig : IAesConfigureation
        {
            public SecureString Password
            {
                get
                {
                    SecureString res = new SecureString();

                    foreach (var item in "123")
                    {
                        res.AppendChar(item);
                    }

                    return res;
                }
            }


            public byte[] PasswordSalt => BitConverter.GetBytes(123);

            public byte[] IV => MD5.HashData(BitConverter.GetBytes(123));

            public HashAlgorithm HashAlgorithm => SHA256.Create();
        }
        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {




        }
    }
}
