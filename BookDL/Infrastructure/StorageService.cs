using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BookDL.Infrastructure
{
    public interface IStorageService
    {
        void CreateDirectory(string dir);
        string GetProfilePath(params string[] segments);
        Stream OpenWriteSteam(string path);
    }
    public class StorageService : IStorageService
    {
        public void CreateDirectory(string dir)
        {
            Directory.CreateDirectory(dir);
        }

        public string GetProfilePath(params string[] args)
        {
            var localAppPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var segmentList = new List<string>()
            {
                localAppPath,
                "Hernian",
                "BookDL"
            };
            segmentList.AddRange(args);
            var targetPath = Path.Combine(segmentList.ToArray());
            return targetPath;
        }

        public Stream OpenWriteSteam(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (dir != null)
            {
                Directory.CreateDirectory(dir);
            }
            return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        }
    }
}
