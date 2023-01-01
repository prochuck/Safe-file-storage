using Microsoft.WindowsAPICodePack.Dialogs;
using Safe_file_storage.Configurations;
using Safe_file_storage.Models;
using Safe_file_storage.Models.Services;
using Safe_file_storage.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Safe_file_storage.ViewModels
{
    internal class FileBrowserViewModel : INotifyPropertyChanged, IDisposable
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


        public ReadOnlyObservableCollection<FileModel> FilesInDirectory
        {
            get
            {
                return _fileBrowser?.FilesInDirectory;
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
                return _fileBrowser?.GetFileHistory(SelectedFile).Select(e => e.ToString()).ToList();
            }
        }

        public FileBrowserViewModel()
        {
            MoveToSelectedDirectoryCommand = new Command(e => _fileBrowser.MoveToDirectory(_selectedFile), e => _selectedFile is not null);
            MoveToParentDirectoryCommand = new Command(e => _fileBrowser.MoveToDirectory(_fileBrowser.CurrentDirectory.ParentDirectoryRecordNo), e => (_fileBrowser is not null));

            ImportFileCommand = new Command(e => ImportDirectoryDilaog(), e => (_fileBrowser is not null));
            ExportFileCommand = new Command(e => ExportDirectoryDilaog(_selectedFile.MFTRecordNo), e => _selectedFile is not null);
            CreateDirectoryCommand = new Command(e => _fileBrowser.CreateDirectory(OpenDirectoryNameEnterDialog()), e => (_fileBrowser is not null));
            DeleteFileCommand = new Command(e => { _fileBrowser.DeleteFile(_selectedFile); _selectedFile = null; }, e => _selectedFile is not null);
            CreateFileCommand = new Command(e => OpenFileCreationDialog(), null);
            OpenFileCommand = new Command(e => OpenFileSelectionDialog(), null);

        }

        public ICommand MoveToSelectedDirectoryCommand { get; private set; }

        public ICommand MoveToParentDirectoryCommand { get; private set; }
        public ICommand ImportFileCommand { get; private set; }
        public ICommand ExportFileCommand { get; private set; }
        public ICommand CreateDirectoryCommand { get; private set; }
        public ICommand DeleteFileCommand { get; private set; }

        public ICommand OpenFileCommand { get; private set; }
        public ICommand CreateFileCommand { get; private set; }


        string OpenDirectoryNameEnterDialog()
        {
            DirectoryNameEnterView directoryNameEnterView = new DirectoryNameEnterView();
            if ((bool)directoryNameEnterView.ShowDialog())
            {
                return directoryNameEnterView.Input;
            }
            else
            {
                return "";
            }
        }
        void OpenFileCreationDialog()
        {
            FileCreationView openFileDialog = new FileCreationView();

            _fileBrowser?.Dispose();
            try
            {
                if ((bool)openFileDialog.ShowDialog())
                {
                    FileCreationViewModel fileCreationViewModel = openFileDialog.DataContext as FileCreationViewModel;
                    if (fileCreationViewModel.Password is null || fileCreationViewModel.Password.Length < 5)
                    {
                        MessageBox.Show("Пароль должен содержать как минимум 6 знаков");
                        return;
                    }
                    if (fileCreationViewModel.Path is null || !Directory.Exists(fileCreationViewModel.Path))
                    {
                        MessageBox.Show("Неверный путь к папке");
                        return;
                    }
                    if (fileCreationViewModel.FileName is null || File.Exists(Path.Combine(fileCreationViewModel.Path, fileCreationViewModel.FileName)))
                    {
                        MessageBox.Show("Файл с таким именем уже существует");
                        return;
                    }
                    if (fileCreationViewModel.Size < 100 || fileCreationViewModel.Size > int.MaxValue)
                    {
                        MessageBox.Show("Файл должен быть больше 1000 КБ");
                        return;
                    }


                    LntfsConfiguration lntfsConfiguration = new LntfsConfiguration()
                    {
                        FilePath = Path.Combine(fileCreationViewModel.Path, fileCreationViewModel.FileName),
                        AttributeHeaderSize = 200,
                        ClusterSize = 1024,
                        MFTZoneSize = ((int)fileCreationViewModel.Size * 1024) / 20,
                        FileSize = (int)fileCreationViewModel.Size * 1024,
                        MFTRecordSize = 1024
                    };


                    AesCryptoConfiguration aesCryptoConfiguration = new AesCryptoConfiguration()
                    {
                        IV = Encoding.ASCII.GetBytes(Path.Combine(fileCreationViewModel.Path, fileCreationViewModel.FileName)),
                        PasswordSalt = Encoding.ASCII.GetBytes(Path.Combine(fileCreationViewModel.Path, fileCreationViewModel.FileName)),
                        Password = fileCreationViewModel.Password

                    };

                    _fileBrowser = FileBrowserFactory.CreateInstanceFromFile(
                        lntfsConfiguration.FilePath,
                        lntfsConfiguration.FileSize,
                        lntfsConfiguration,
                        aesCryptoConfiguration
                        );

                    _selectedFile = null;



                    // Обновить все байдинги
                    OnProperyChanged(String.Empty);
                }
            }
            finally
            {
            }
        }
        void OpenFileSelectionDialog()
        {

            FileSelectionView openFileDialog = new FileSelectionView();

            _fileBrowser?.Dispose();
            try
            {
                if ((bool)openFileDialog.ShowDialog())
                {
                    FileSelectionViewModel fileSelectionViewModel = openFileDialog.DataContext as FileSelectionViewModel;
                    if (fileSelectionViewModel.Password is null || fileSelectionViewModel.Password.Length < 5)
                    {
                        MessageBox.Show("Пароль должен содержать как минимум 6 знаков");
                        return;
                    }
                    if (fileSelectionViewModel.FilePath is null || !File.Exists(fileSelectionViewModel.FilePath))
                    {
                        MessageBox.Show("Неверный путь к папке");
                        return;
                    }


                    LntfsConfiguration lntfsConfiguration = new LntfsConfiguration()
                    {
                        FilePath = fileSelectionViewModel.FilePath,
                        AttributeHeaderSize = 200,
                        ClusterSize = 1024,
                        MFTZoneSize = ((int)new FileInfo(fileSelectionViewModel.FilePath).Length) / 20,
                        FileSize = (int)new FileInfo(fileSelectionViewModel.FilePath).Length,
                        MFTRecordSize = 1024
                    };


                    AesCryptoConfiguration aesCryptoConfiguration = new AesCryptoConfiguration()
                    {
                        IV = Encoding.ASCII.GetBytes(fileSelectionViewModel.FilePath),
                        PasswordSalt = Encoding.ASCII.GetBytes(fileSelectionViewModel.FilePath),
                        Password = fileSelectionViewModel.Password
                    };

                    _fileBrowser = FileBrowserFactory.CreateInstanceFromFile(
                        lntfsConfiguration.FilePath,
                        lntfsConfiguration.FileSize,
                        lntfsConfiguration,
                        aesCryptoConfiguration
                        );
                    _selectedFile = null;

                    // Обновить все байдинги
                    OnProperyChanged(null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("При чтении файла произошла ошибка. Возможно пароль не верен.");
            }
            finally
            {
            }
        }
        void ExportDirectoryDilaog(int mftNo)
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog.IsFolderPicker = true;
            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                WaitView waitView = new WaitView(_fileBrowser.ExportFileAsync(openFileDialog.FileName, mftNo));
                waitView.ShowDialog();

            }
        }
        void ImportDirectoryDilaog()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
            openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            openFileDialog.IsFolderPicker = true;
            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                WaitView waitView = new WaitView(_fileBrowser.ImportToCurrentDirectoryAsync(openFileDialog.FileName));
                waitView.ShowDialog();

            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnProperyChanged(string propertyChangedName)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyChangedName));
            FileCreationView fileCreationView = new FileCreationView();
        }

        public void Dispose()
        {
            _fileBrowser?.Dispose();
        }
    }
}
