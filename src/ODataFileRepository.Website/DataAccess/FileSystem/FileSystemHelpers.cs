using System;
using System.IO;

namespace ODataFileRepository.Website.DataAccess.FileSystem
{
    public static class FileSystemHelpers
    {
        private static readonly DirectoryInfo _appDataDirectory = new DirectoryInfo(AppDomain.CurrentDomain.GetData("DataDirectory").ToString());

        public static DirectoryInfo AppDataDirectory
        {
            get { return _appDataDirectory; }
        }
    }
}