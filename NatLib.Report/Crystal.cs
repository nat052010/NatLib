using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using NatLib.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;

namespace NatLib.Report
{
    /// <summary>
    /// nat20161116 dependent on crystal report engine
    /// </summary>
    public class Crystal : IDisposable
    {
        //Fields
        private bool _disposed = false;
        private string _mediaType = "pdf";
        private ExportFormatType _exportFormatType = ExportFormatType.PortableDocFormat;


        //Properties
        public string Data { get; set; }
        public string FileName { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public DbConString ConString { get; set; }
        public ReportDocument Document { get; set; }
        public bool DeleteTempReport { get; set; }
        public string ReportPath { get; set; }
        protected bool DbConnectionReady { get; set; } = false;
        //public DbConString DbConString { get; set; }

        //Constructor
        public Crystal(bool deleteTempReport = false)
        {
            Document = new ReportDocument();
            DeleteTempReport = deleteTempReport;
        }

        public Crystal(DbConString conString, bool deleteTempReport = false)
        {
            Document = new ReportDocument();
            ConString = conString;
            DeleteTempReport = deleteTempReport;
        }

        //Methods
        private void SetDatabase()
        {
            var db = ConString;
            if (db == null) return;
            try
            {
                Document.SetDatabaseLogon(db.UserId, db.Password, db.Database, db.DataSource);
                DbConnectionReady = true;
            }
            catch (Exception ex)
            {
                ex.Message.Log();
                throw;
            }
        }

        public void SetExportFormat(ExportFormatType format )
        {
            switch (format)
            {
                case ExportFormatType.PortableDocFormat:
                    _mediaType = "pdf";
                    break;
                case ExportFormatType.Excel:
                    _mediaType = "vnd.ms-excel";
                    break;
            }

            _exportFormatType = format;
        }

        private void PreLoad()
        {
            var fn = FileName;
            if (!File.Exists(fn)) throw new Exception("Cannot find the Report File");
            ReportPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + Path.GetExtension(fn));
            File.Copy(fn, ReportPath);
            Document.Load(ReportPath);
            SetDatabase();

            if (Document.Subreports.Count > 0 && DbConnectionReady)
                throw new Exception("Database Connection must be set first");

            foreach (var dsc in from ReportDocument ireport in Document.Subreports from IConnectionInfo dsc in ireport.DataSourceConnections select dsc)
                dsc.SetConnection(ConString.DataSource, ConString.Database, ConString.UserId, ConString.Password);

            if (Parameters != null)
            {
                foreach (var param in Parameters)
                    Document.SetParameterValue(param.Key, param.Value);
            }
        }

        private void PostLoad()
        {
/*
            using (var ms = new MemoryStream())
            {
                try
                {
                    Document.ExportToStream(_exportFormatType);
                    ms.Position = 0;
                    var buffer = ms.ToArray();                    
                    Data = $"data:application/{_mediaType};base64,{Convert.ToBase64String(buffer)}";
                }
                catch (Exception ex)
                {
                    ex.Message.Log();
                    throw;
                }
            }
*/

            using (var stream = Document.ExportToStream(_exportFormatType))
            {
                try
                {
                    var buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, Convert.ToInt32(stream.Length - 1));
                    Data = $"data:application/{_mediaType};base64,{Convert.ToBase64String(buffer)}";
                }
                catch (Exception ex)
                {
                    ex.Message.Log();
                    throw;
                }
            }
        }

        public void ExportToDisk(DataTable dt, ExportFormatType format, string fileName, Action<ReportDocument> rptDoc = null)
        {
            PreLoad();
            Document.SetDataSource(dt);
            rptDoc?.Invoke(Document);
            Document.ExportToDisk(format, fileName);
        }

        public void Load(DataTable dataSource, Action<ReportDocument> rptDoc = null)
        {
            if (dataSource == null) throw new Exception("Please provide report DataSource");
            PreLoad();
            rptDoc?.Invoke(Document);
            Document.SetDataSource(dataSource);
            PostLoad();
        }

        public void Load(Action<ReportDocument> rptDoc = null)
        {
            PreLoad();
            rptDoc?.Invoke(Document);
            PostLoad();
        }

        public void AddParameter(string key, object value)
        {
            Parameters.Add(key, value);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (!_disposed)
            {
                if (DeleteTempReport && File.Exists(ReportPath))
                    File.Delete(ReportPath);
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Crystal()
        {
            Dispose(false);            
        }
    }
}