using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Diagnostics;
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

        public static void Log(this string error, string prefix = "Err_", string folder = "Error", bool useDateFolder = false)
        {
            var isHosted = HostingEnvironment.IsHosted;
            var location = isHosted ? HttpContext.Current.Server.MapPath($"~/{folder}") : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder);
            var dt = DateTime.Now;
            if (useDateFolder)
            {                
                var df = dt.Year.ToString() + dt.Month.ToString() + dt.Day.ToString();
                location = isHosted ? $"{location}/{df}" : $"{Path.Combine(location, df)}";
            }

            if (!Directory.Exists(location))
                Directory.CreateDirectory(location);

            var fileName = prefix + DateTime.Today.ToShortDateString().Replace("/", "-") + ".txt";
            var path = Path.Combine(location, fileName);

            using (var file = new StreamWriter(path, true))
            {
                file.WriteLine(dt.ToShortTimeString() + ", " + error);
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

        public static List<Dictionary<string, object>> JsonItems(this DataRowCollection dRows)
        {
            
            var items = new List<Dictionary<string, object>>();
            if (dRows.Count > 0){
                var cols = dRows[0].Table.Columns;
                foreach (DataRow row in dRows)
                {
                    var value = (from DataColumn col in cols select col.ColumnName).ToDictionary(colName => colName, colName => row[colName]);
                    items.Add(value);
                }
            }

            return items;
        }


        public static List<Dictionary<string, object>> JsonItems(this DataTable dt)
        {
/*            var columnNames = dt.Columns.Cast<DataColumn>()
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

            return items;*/
            return dt.Rows.JsonItems();
        }

        public static List<List<Dictionary<string, object>>> JsonItems(this DataTableCollection dts)
        {
            return (from DataTable dt in dts select dt.JsonItems()).ToList();
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

        public static void RunCmd(this string pthurl, bool waitforexit = false)
        {
            var cmd = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            cmd.Start();
            cmd.StandardInput.WriteLine("@echo off");
            cmd.StandardInput.WriteLine(pthurl);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();

            if (waitforexit)
                cmd.WaitForExit();
        }

        public static string GetSetting(string item)
        {           
            return ConfigurationManager.AppSettings.Get(item);
        }

    }
}