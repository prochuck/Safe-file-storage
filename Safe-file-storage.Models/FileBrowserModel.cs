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



        public FileModel CurrentDirectory { get; private set; }
        public ObservableCollection<FileModel> _files;
        public ReadOnlyObservableCollection<FileModel> FilesInDirectory { get; private set; }

        public FileBrowserModel(IFileWorker fileWorker)
        {
            
            CurrentDirectory = fileWorker.ReadFileHeaderAndAttributes(1);

            _files = new ObservableCollection<FileModel>(CurrentDirectory.DirectoryAttribute.Files);
            FilesInDirectory = new ReadOnlyObservableCollection<FileModel>(_files);

        }


        public void MoveToDirectory(FileModel directory)
        {
            if (directory is not null && directory.IsDirectory)
            {
                CurrentDirectory = directory;
            }
        }

        private void UpdateFileList()
        {
            _files.Clear();
            foreach (var item in CurrentDirectory.DirectoryAttribute.Files)
            {
                _files.Add(item);
            }
        }


    }
}
