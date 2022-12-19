using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Safe_file_storage.ViewModels
{
    internal class FileSelectionViewModel : INotifyPropertyChanged
    {
        public FileSelectionViewModel()
        {
            SelectFileCommand = new Command(e => SelectFileDialog(), null);
        }

        public string Password { get; set; }

        string _filePath;
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value;
                OnProperyChanged(nameof(FilePath));
            }
        }


        public ICommand SelectFileCommand { get; set; }


        void SelectFileDialog()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                FilePath = openFileDialog.FileName;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnProperyChanged(string propertyChangedName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyChangedName));
        }
    }
}
