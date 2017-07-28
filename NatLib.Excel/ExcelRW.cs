using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;

namespace NatLib.Excel
{
    public class ExcelRW : IDisposable
    {
        public OleDbConnection Document { get; set; }

        public string FileName { get; set; }

        public string WorkSheetName { get; set; }

        public bool DeleteFileOnDispose { get; set; }

        public int RowNumber { get; set; }

        public ExcelRW(string fileName, bool hdr = false)
        {
            try
            {
                FileName = fileName;
                RowNumber = 0;

                Document = new OleDbConnection($"Provider=Microsoft.ACE.OLEDB.12.0;" +
                                               $"Data Source={fileName}" +
                                               $";Extended Properties=\"Excel 12.0 Xml;HDR={(hdr ? "YES" : "NO")}\"");
                Document.Open();
            }
            catch (Exception)
            {
                throw ;
            }
        }

        public void WriteCommand(Action<OleDbCommand> todo)
        {
            try
            {
                using (var com = new OleDbCommand())
                {
                    com.Connection = Document;
                    com.CommandType = CommandType.Text;
                    todo(com);
                    com.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void WriteTable(DataTable dt, string sheetName, int startRow, int startColumn, bool update = true)
        {
            var columns = dt.Columns;
            var rows = dt.Rows;
            var colName = from DataColumn col in columns select "[" + col.ColumnName + "]";

            try
            {
                foreach (DataRow row in rows)
                {
                    var letter = (startColumn - 1).ToLetter();
                    var rowNum = startRow.ToString();
                    var index = 0;
                    var range = "";
                    var valList = row.ItemArray.Select(r =>
                    {
                        index++;
                        if (update)
                            return "F" + index.ToString() + "='" + r.ToString() + "'";
                        else
                        {
                            return "'" + r.ToString() + "'";
                        }
                    }).ToList();

                    range = letter + rowNum + ":" + ((startColumn + columns.Count) - 2).ToLetter() + rowNum;
                    //"Insert into [Sheet1$] ([ColumnName]) values('Value')"

                    WriteCommand(r =>
                    {
                        var comm = "";
                        if (update)
                            comm = $"UPDATE [{sheetName}${range}] SET {valList.Fuse()}";
                        else
                            comm = $"INSERT INTO [{sheetName}$] ({colName.Fuse()}) " +
                                   $"VALUES ({valList.Fuse()})";
                        r.CommandText = comm;
                    });

                    startRow++;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void SetColumn()
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (DeleteFileOnDispose)
                File.Delete(FileName);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ExcelRW()
        {
            Dispose(false);
        }

    }
}