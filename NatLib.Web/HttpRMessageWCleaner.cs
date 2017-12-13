using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NatLib.Web
{
    public class HttpRMessageWCleaner: HttpResponseMessage
    {
        public List<string> FileToClean { get; set; }

        public HttpRMessageWCleaner()
        {
            FileToClean = new List<string>();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Content?.Dispose();
            foreach (var path in FileToClean)
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }
}
