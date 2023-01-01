using Safe_file_storage.Models.FileAtributes;
using Safe_file_storage.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safe_file_storage.Models
{
    public class FileBrowserModel : IDisposable
    {


        IFileSystemService _fileWorker;
        public FileModel CurrentDirectory { get; private set; }
        ObservableCollection<FileModel> _files;
        public ReadOnlyObservableCollection<FileModel> FilesInDirectory { get; private set; }

        internal FileBrowserModel(IFileSystemService fileWorker)
        {

            _fileWorker = fileWorker;
            CurrentDirectory = fileWorker.RootDirectory;
            _files = new ObservableCollection<FileModel>(CurrentDirectory.DirectoryAttribute.Files);

            foreach (var item in _files)
            {
                item.FileNameAttribute = _fileWorker.ReadFileAttribute<FileNameAttribute>(item.MFTRecordNo);
                item.HistoryAttribute = _fileWorker.ReadFileAttribute<HistoryAttribute>(item.MFTRecordNo);
            }

            FilesInDirectory = new ReadOnlyObservableCollection<FileModel>(_files);
        }

        public async Task ExportFileAsync(string targetPath, int mftNo)
        {
            await Task.Run(() => _fileWorker.ExportFile(mftNo, Path.Combine(targetPath, _fileWorker.ReadFileAttribute<FileNameAttribute>(mftNo).Name)));
        }
        public async Task<bool> ImportToCurrentDirectoryAsync(string filePath)
        {
            Task<FileModel> task = Task.Run(() => _fileWorker.ImportFile(filePath, CurrentDirectory.MFTRecordNo));
            await task;
            if (task.Result is null)
            {
                return false;
            }

            UpdateFileList();
            return true;
        }
        public void MoveToDirectory(int mftRecordNo)
        {
            FileModel file = _fileWorker.ReadFileHeader(mftRecordNo);
            MoveToDirectory(file);
        }
        public void MoveToDirectory(FileModel directory)
        {
            if (directory is not null && directory.IsDirectory)
            {
                CurrentDirectory = directory;
                UpdateFileList();
            }
        }
        public void CreateDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
            {
                return;
            }
            FileModel newDirecory = _fileWorker.CreateDirectory(directoryName, CurrentDirectory);
            UpdateFileList();
        }
        public void DeleteFile(FileModel file)
        {
            _fileWorker.DeleteFile(file.MFTRecordNo);
            UpdateFileList();
        }

        public ReadOnlyCollection<HistoryRecord> GetFileHistory(FileModel fileModel)
        {
            return _fileWorker.ReadFileAttribute<HistoryAttribute>(fileModel.MFTRecordNo).HistoryRecords;
        }


        private void UpdateFileList()
        {
            _files.Clear();
            CurrentDirectory.DirectoryAttribute = _fileWorker.ReadFileAttribute<DirectoryAttribute>(CurrentDirectory.MFTRecordNo);
            foreach (var item in CurrentDirectory.DirectoryAttribute.Files)
            {
                item.FileNameAttribute = _fileWorker.ReadFileAttribute<FileNameAttribute>(item.MFTRecordNo);
                item.HistoryAttribute = _fileWorker.ReadFileAttribute<HistoryAttribute>(item.MFTRecordNo);
                _files.Add(item);
            }
        }

        public void Dispose()
        {
            _fileWorker.Dispose();

        }
    }
}
