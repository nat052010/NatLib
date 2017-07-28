using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.AccessControl;
using System.Web;
using System.Web.Hosting;

namespace NatLib.Debug
{
    public class Log:IDisposable
    {
        #region Fields
        private bool _disposed = false;
        #endregion

        #region Properties
        public string FileName { get; set; }
        public string Location { get; set; }
        #endregion


        #region Constructor
        public Log()
        {
            if (HostingEnvironment.IsHosted)
                Location = HttpContext.Current.Server.MapPath("~/Error");
            else
                Location = Path.Combine(Directory.GetCurrentDirectory(), "Error");

            FileName = "Err_" + DateTime.Today.ToShortDateString().Replace("/", "-");
        }

        #endregion


        #region Methods


        public void Write(string error)
        {
            var path = Path.Combine(Location, FileName);
            if (!Path.HasExtension(path)) path = path + ".txt";
            if (!Directory.Exists(Location)) Directory.CreateDirectory(Location);
            using (var file = new StreamWriter(path, true))
            {
                file.WriteLine(DateTime.Now.ToShortTimeString() + ", " + error);
            }            
        }
        #endregion


        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                    
            }
            // Release unmanaged resources.
            // Set large fields to null.                
            _disposed = true;
        }

        public void Dispose() // Implement IDisposable
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Log() // the finalizer
        {
            Dispose(false);
        }
    }
}
