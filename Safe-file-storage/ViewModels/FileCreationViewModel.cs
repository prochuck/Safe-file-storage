using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Safe_file_storage.ViewModels
{
    internal class FileCreationViewModel : INotifyPropertyChanged
    {
        public FileCreationViewModel()
        {
            SelectDirectoryCommand = new Command(e => SelectDirectoryDialog(), null);

        }

        public string Password { get; set; }
        public long Size { get; set; }

        string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                OnProperyChanged(nameof(Path));
            }
        }
        
        public string FileName { get; set; }

        public ICommand SelectDirectoryCommand { get; set; }


        void SelectDirectoryDialog()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog.IsFolderPicker = true;
            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Path=openFileDialog.FileName;
            }
            openFileDialog.Dispose();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnProperyChanged(string propertyChangedName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyChangedName));
        }
    }
}
