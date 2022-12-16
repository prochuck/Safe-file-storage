using Microsoft.WindowsAPICodePack.Dialogs;
using Safe_file_storage.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Safe_file_storage.ViewModels
{
    internal class FileBrowserViewModel : INotifyPropertyChanged
    {
        FileBrowserModel _fileBrowser;


        FileModel _selectedFile;
        public FileModel SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                _selectedFile = value;
                OnProperyChanged(nameof(SelectedFile));
                OnProperyChanged(nameof(FileHistory));
            }
        }

        public string DirectoryToCreateName { get; set; }
        public ReadOnlyObservableCollection<FileModel> FilesInDirectory
        {
            get
            {
                return _fileBrowser.FilesInDirectory;
            }
        }

        public List<string> FileHistory
        {
            get
            {
                if (SelectedFile is null)
                {
                    return new List<string>();
                }
                return _fileBrowser.GetFileHistory(SelectedFile).Select(e => e.ToString()).ToList();
            }
        }

        public FileBrowserViewModel(FileBrowserModel fileBrowser)
        {
            _fileBrowser = fileBrowser;

            MoveToSelectedDirectory = new Command(e => _fileBrowser.MoveToDirectory(_selectedFile), e => _selectedFile != null);
            MoveToParentDirectory = new Command(e => _fileBrowser.MoveToDirectory(_fileBrowser.CurrentDirectory.ParentDirectoryRecordNo), null);

            ImportFile = new Command(e => ImportDirectoryDilaog(), null);
            ExportFile = new Command(e => ExportDirectoryDilaog(_selectedFile.MFTRecordNo), e => _selectedFile != null);
            CreateDirectory = new Command(e => _fileBrowser.CreateDirectory(DirectoryToCreateName), null);
            DeleteFile=new  Command(e => { _fileBrowser.DeleteFile(_selectedFile); _selectedFile = null; }, e => _selectedFile != null);


        }

        public ICommand MoveToSelectedDirectory { get; private set; }

        public ICommand MoveToParentDirectory { get; private set; }
        public ICommand ImportFile { get; private set; }
        public ICommand ExportFile { get; private set; }
        public ICommand CreateDirectory { get; private set; }
        public ICommand DeleteFile { get; private set; }

        void ExportDirectoryDilaog(int mftNo)
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog.IsFolderPicker = true;
            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _fileBrowser.ExportFile(openFileDialog.FileName, mftNo);
            }
        }
        void ImportDirectoryDilaog()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog.IsFolderPicker = true;
            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                _fileBrowser.ImportToCurrentDirectory(openFileDialog.FileName);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnProperyChanged(string propertyChangedName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyChangedName));
        }
    }
}
