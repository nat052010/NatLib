using System.Collections.Generic;

namespace NatLib.Zip
{
    public class PackageFile
    {
        public string PackagePath { get; set; }
        public int FileCount { get; set; }
        public List<string> FileList { get; set; }
    }
}