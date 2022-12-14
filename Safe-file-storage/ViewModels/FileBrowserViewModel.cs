using Safe_file_storage.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Safe_file_storage.ViewModels
{
    internal class FileBrowserViewModel:INotifyPropertyChanged
    {
        FileBrowserModel _fileBrowser;


        FileModel _selectedDirectory;
        public FileModel SelectedDirectory
        {
            get { return _selectedDirectory; }
            set
            {
                _selectedDirectory = value;
                OnProperyChanged(nameof(SelectedDirectory));
            }
        }

        public ReadOnlyObservableCollection<FileModel> FilesInDirectory
        {
            get
            {
                return _fileBrowser.FilesInDirectory;
            }
        }


        public FileBrowserViewModel(FileBrowserModel fileBrowser)
        {
            _fileBrowser = fileBrowser;

            MoveToSelectedDirectory = new Command(e => _fileBrowser.MoveToDirectory(_selectedDirectory), null);
            MoveToParentDirectory = new Command(e => _fileBrowser.MoveToDirectory(_fileBrowser.CurrentDirectory.ParentDirectoryRecordNo), null);

        }

        public ICommand MoveToSelectedDirectory { get; private set; }

        public ICommand MoveToParentDirectory { get; private set; }


        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnProperyChanged(string propertyChangedName)
        {
            PropertyChanged.Invoke(this,new PropertyChangedEventArgs(propertyChangedName));
        }
    }
}
