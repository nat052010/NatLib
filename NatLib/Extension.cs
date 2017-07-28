using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Hosting;


namespace NatLib
{
    public static class Extension
    {
        public static string ToLetter(this int index)
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var value = "";

            if (index >= letters.Length)
                value += letters[index / letters.Length - 1];

            value += letters[index % letters.Length];

            return value;
        }

        public static string Fuse<T>(this IEnumerable<T> list, string delimeter = ", ")
        {
            return string.Join(delimeter, list);
        }

        public static void Log(this string error, string prefix = "Err_", string folder = "Error")
        {
            var location = HostingEnvironment.IsHosted ? HttpContext.Current.Server.MapPath($"~/{folder}") : Path.Combine(Directory.GetCurrentDirectory(), folder);

            if (!Directory.Exists(location))
                Directory.CreateDirectory(location);

            var fileName = prefix + DateTime.Today.ToShortDateString().Replace("/", "-") + ".txt";
            var path = Path.Combine(location, fileName);

            using (var file = new StreamWriter(path, true))
            {
                file.WriteLine(DateTime.Now.ToShortTimeString() + ", " + error);
                file.AutoFlush = true;
            }
        }

        public static List<object> Items(this DataRowCollection dRows)
        {
            var drow = (from DataRow item in dRows select item.ItemArray.ToList()).Cast<object>().ToList();
            return drow;
        }

        public static List<object> Items(this DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>()
                                 .Select(x => x.ColumnName)
                                 .ToArray();

            var items = new List<object>();

            foreach (DataRow item in dt.Rows)
            {
                var value = new Dictionary<string, object>();
                foreach (var column in columnNames)
                {
                    value.Add(column, item[column]);
                }
                items.Add(value);
            }

            return items;
        }

        public static List<List<object>> Items(this DataTableCollection dts)
        {
            var items = new List<List<object>>();
            foreach (DataTable dt in dts)
            {
                items.Add(dt.Items());
            }

            return items;
        }

        public static List<Dictionary<string, object>> JsonItems(this DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>()
                                 .Select(x => x.ColumnName)
                                 .ToArray();

            var items = new List<Dictionary<string, object>>();

            foreach (DataRow item in dt.Rows)
            {
                var value = new Dictionary<string, object>();
                foreach (var column in columnNames)
                {
                    value.Add(column, item[column]);
                }
                items.Add(value);
            }

            return items;
        }

        public static List<List<Dictionary<string, object>>> JsonItems(this DataTableCollection dts)
        {
            var items = new List<List<Dictionary<string, object>>>();
            foreach (DataTable dt in dts)
            {
                items.Add(dt.JsonItems());
            }
            return items;
        }

        public static List<Dictionary<string, object>> JsonItems(this DataRowCollection dRows)
        {
            var cols = dRows[0].Table.Columns;
            var items = new List<Dictionary<string, object>>();

            foreach (DataRow row in dRows)
            {
                var value = (from DataColumn col in cols select col.ColumnName).ToDictionary(colName => colName, colName => row[colName]);
                items.Add(value);
            }

            return items;
        }


        public static bool IsValidImage(this string filename)
        {

            try
            {
                using (var image = Image.FromFile(filename))
                { }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool IsImageVideoFile(this string file)
        {
            string[] mediaExtensions = {
                        ".PNG", ".JPG", ".JPEG", ".BMP", ".GIF", //etc
                        ".AVI", ".MP4", ".DIVX", ".WMV", ".MKV", //etc
                    };
            var ext = Path.GetExtension(file).ToUpper();
            return Array.IndexOf(mediaExtensions, ext) != -1;
        }

    }
}