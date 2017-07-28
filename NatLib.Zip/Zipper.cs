using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zip;

namespace NatLib.Zip
{
    public class Zipper : IDisposable
    {
        #region Fields
        private bool _disposed;
        private bool _saved;

        #endregion

        #region Properties
        public ZipFile ZipFile { get; set; }
        public string FileName { get; set; }
        public string DirectoryName { get; set; }
        public string FilePath { get; set; }
        #endregion

        #region Constructors

        public Zipper()
        {
            ZipFile = new ZipFile();
            FileName = FileName ?? Guid.NewGuid().ToString() + ".zip";
        }

        public Zipper(string fileName) : this()
        {
            FileName = fileName;
        }
        #endregion

        #region Methods

        public void AddFiles(List<string> files, bool isPathRetain = false)
        {
            if (isPathRetain)
                ZipFile.AddFiles(files, "");
            else
                ZipFile.AddFiles(files);
        }

        public void AddFile(string file, bool isPathRetain)
        {
            if (isPathRetain)
                ZipFile.AddFile(file, "");
            else
                ZipFile.AddFile(file);
        }
        public void Save(string path = "")
        {
            SetFilePath(path);
            if (_saved) File.Delete(FilePath);
            ZipFile.Save(FilePath);
            _saved = true;
        }

        private void SetFilePath(string path)
        {
            var dir = DirectoryName;
            var file = FileName;

            if (path != "")
            {
                dir = Path.GetDirectoryName(path);
                file = Path.GetFileName(path);
            }

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            FilePath = Path.Combine(dir, file);
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
        public static PackageFile ZipFiles(IEnumerable<string> fileNames, string outputFile,
                                string directoryName = null, IEnumerable<string> subDirectories = null)
        {
            //string result = null;
            var result = new PackageFile();
            var copyLocation = Path.Combine(Path.GetDirectoryName(outputFile), Guid.NewGuid().ToString());
            if (!Directory.Exists(copyLocation))
                Directory.CreateDirectory(copyLocation);
            try
            {
                using (var zip = new ZipFile())
                {
                    var files = fileNames.ToList();
                    var subDir = subDirectories?.ToList();
                    var fileCounter = 0;
                    var isSubApply = false;

                    if (subDir != null && subDir.Count() == fileNames.Count())
                    {
                        isSubApply = true;
                        foreach (var newPath in subDir.Where(r => r != null).Select(sub => Path.Combine(copyLocation, sub)).ToList())
                            Directory.CreateDirectory(newPath);
                    }

                    if (directoryName != null)
                    {
                        files.Clear();
                        files.AddRange(fileNames.Select(
                            fileName => Path.Combine(directoryName, fileName))
                            .Where(File.Exists));
                    }
                    for (int i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        var fileName = Path.GetFileName(file);

                        var fileCopy = Path.Combine(copyLocation, (isSubApply ? subDir[i] : "") ?? "", fileName);

                        if (!File.Exists(file)) continue;
                        File.Copy(file, fileCopy);
                        fileCounter++;
                    }

                    if (fileCounter == 0) throw new FileNotFoundException();
                    result.PackagePath = outputFile;
                    result.FileList = Directory.GetFiles(copyLocation).ToList();
                    result.FileCount = fileCounter;

                    zip.AddDirectory(copyLocation);
                    zip.Save(outputFile);

                }

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Directory.Delete(copyLocation, true);
            }
            return result;
        }



        public static string ZipFolder(string folderPath, string zipFile = null)
        {
            string result = null;
            var copyPath = Path.Combine(Directory.GetParent(folderPath).FullName, Guid.NewGuid().ToString());

            using (var zip = new ZipFile())
            {
                var filePath = zipFile ?? folderPath + ".zip";

                DirectoryCopy(folderPath, copyPath, true);

                zip.AddDirectory(copyPath);

                result = filePath;
                zip.Save(filePath);
            }

            Directory.Delete(copyPath, true);

            return result;
        }



        public static void ExtractZipFiles(string zipFilePath, string outputDirectory)
        {
            using (var zip = Ionic.Zip.ZipFile.Read(zipFilePath))
            {
                if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);
                var copyDir = Path.Combine(Directory.GetParent(outputDirectory).FullName, Guid.NewGuid().ToString());

                foreach (var entry in zip.Entries)
                    entry.Extract(copyDir, ExtractExistingFileAction.OverwriteSilently);

                DirectoryCopy(copyDir, outputDirectory, true);
                Thread.Sleep(3000);
                Directory.Delete(copyDir, true);
            }
        }



        #endregion

        #region Events


        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed) return;

            if (disposing)
            {
                ZipFile?.Dispose();
            }
            else
            {
                //if (File.Exists(FilePath) && _saved)
                //    File.Delete(FileName);
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Zipper()
        {
            Dispose(false);
        }
    }
}
