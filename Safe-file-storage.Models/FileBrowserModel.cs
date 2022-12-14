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
    public class FileBrowserModel
    {


        IFileWorker _fileWorker;
        public FileModel CurrentDirectory { get; private set; }
        ObservableCollection<FileModel> _files;
        public ReadOnlyObservableCollection<FileModel> FilesInDirectory { get; private set; }

        public FileBrowserModel(IFileWorker fileWorker)
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

        public void ExportDirectory(string targetPath, int mftNo)
        {
            _fileWorker.ExportFile(mftNo, Path.Combine(targetPath, _fileWorker.ReadFileAttribute<FileNameAttribute>(mftNo).Name));
        }
        public void ImportToCurrentDirectory(string filePath)
        {
            _fileWorker.ImportFile(filePath, CurrentDirectory.MFTRecordNo);
            UpdateFileList();
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
            if (directoryName is null)
            {
                return;
            }
            FileModel newDirecory = _fileWorker.CreateDirectory(directoryName,CurrentDirectory);
            UpdateFileList();
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

        

    }
}
