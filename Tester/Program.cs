using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
//using NatLib;
using NatLib.DB;
//using NatLib.Debug;
//using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNet.SignalR.Client;
//using Microsoft.Azure;
using NatLib.Zip;
using NatLib.Web;

//using Microsoft.WindowsAzure;

//using NatLib.Thumbnailer;

namespace Tester
{
    internal class Program
    {
        private string _zipFile;
        public FileSystemWatcher Watcher { get; set; }

        private static void Main(string[] args)
        {
            /*
                        Test();
                        Console.WriteLine("Today is " + DateTime.Today);
            */
            //Test2();
            //Zipper.ZipFolder(Path.Combine(Directory.GetCurrentDirectory(), "Error"));           
            //Zipper.ExtractZipFiles(Path.Combine(Directory.GetCurrentDirectory(), "f7433f75-e1cf-40a9-b12e-0a7e2ced78a5.zip"), Directory.GetCurrentDirectory());
            //Zipper.ZipFiles(new string[] {"output2.3.webm", "output2.2.webm"},
            //    Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString() + ".zip"),
            //    Directory.GetCurrentDirectory(),
            //    new string[] {"Test", null});
            //Test4();
            //var stream = new StreamReader(new FileStream(result));
            //File.Delete(result);
            //Test4();
            //var Watcher = new FileSystemWatcher
            //{
            //    Path = Directory.GetCurrentDirectory(),
            //    Filter = "*.*",
            //    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size
            //};

            //Watcher.Changed += Watcher_Changed;
            //Watcher.EnableRaisingEvents = true;

            //Test2();
            //Console.ReadLine();
            //var sql = new MsSqlServer();
            try
            {
                /*
                                var message = new MailMessage("servicedelivery@intellismartinc.com", "jonathan_lumapas@yahoo.com")
                                {
                                    Body = "This is a test;",
                                    Subject = "Test"
                                };
                                var client = new SmtpClient
                                {
                                    Host = "mail.intellismartinc.com",
                                    EnableSsl = false,   
                                    Port = 587,
                                    UseDefaultCredentials = false, 
                                    Credentials = new NetworkCredential("servicedelivery@intellismartinc.com", "service001")
                                };

                                client.Send(message);
                */

                //var sqlLiteLog = new SqLite { DataSource = MapPath("log.db3"), Location = "" };
                //var logs = sqlLiteLog.SqlExecCommand($"SELECT * FROM tLogs").Tables[0].JsonItems();
                //HttpClientMultipartFormPostAsync();
                //if (!res.IsSuccessStatusCode && res.StatusCode == HttpStatusCode.InternalServerError)
                //    throw new HttpException("Server Error");
                //ZIpperAppendFile();
                //Zipper.ExtractZipFiles(Path.Combine(Directory.GetCurrentDirectory(), "UPD-20170831-15-2.InSysPackage"), Directory.GetCurrentDirectory());
                //GeneratateRss();
                CloudGetSetting();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private static void CloudGetSetting()
        {
            //Microsoft.Azure.CloudConfigurationManager.GetSetting("fdsafsda");
            ConfigurationManager.AppSettings.Get("");
        }

        private static void GeneratateRss()
        {
            var rss = new Rss
            {
                Style =
                {
                    IsFlowVertical = false,
                    Width = 1000,
                    DescriptionWidth = 400,
                    Height = 300,
                    DescriptionHeight = 300,
                    DescriptionLines = 12
                }
            };
            var rssHtml = rss.ParseFeed("https://news.google.com/news/rss/?ned=en_ph&hl=en");
        }

        private static void ZIpperAppendFile()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var zipFile = Path.Combine(baseDirectory, "setup1.zip");
            var fileToAdd = Path.Combine(baseDirectory, "output1.webm");
            Zipper.AppendFile(zipFile, fileToAdd, true);
        }

        private static void HttpClientMultipartFormPostAsync()
        {
            var logs = new List<Dictionary<string, object>> {new Dictionary<string, object> {{"Test", "Result"}}};
            var content = new MultipartFormDataContent
            {
                {new StringContent(JsonConvert.SerializeObject(logs)), "data"},
                {new StringContent("fdasfdsafdsa"), "test"}
            };

            var client = new HttpClient();
            var address = "http://localhost:20063";
            address = address.Substring(address.Length - 1) == "/" ? address : address + "/";
            var location = "api/player/SaveLogs";
            var uri = new Uri(Path.Combine(address, location));
            var res = client.PostAsync(uri, content).Result;
        }

        private static void HttpClientPostAsync()
        {
            var logs = new List<Dictionary<string, object>>();
            logs.Add(new Dictionary<string, object> {{"Test", "Result"}});
            var content = new StringContent(JsonConvert.SerializeObject(logs), Encoding.UTF8, "application/json");
            var client = new HttpClient();
            var address = "http://localhost:20063";
            address = address.Substring(address.Length - 1) == "/" ? address : address + "/";
            var location = "api/player/SaveLogs";
            var uri = new Uri(Path.Combine(address, location));
            var res = client.PostAsync(uri, content).Result;
        }

        private async static void HttpClientGetAsync()
        {
            var client = new HttpClient();
            var response = await client.GetAsync("http://localhost:20061/api/action/downloadfile?mediaFiles=1,2,3");
            var file = response.Content.Headers.ContentDisposition?.FileName ?? Guid.NewGuid().ToString() + ".zip";

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error Occur");
                return;
            }
            var result = await response.Content.ReadAsStreamAsync();

            var dir = Directory.GetCurrentDirectory();
            var zipFile = Path.Combine(dir, file);
            var stream = File.Create(zipFile);

            result.CopyTo(stream);

            stream.Dispose();
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            var zipFile = e.FullPath;
            var ext = Path.GetExtension(zipFile);
            var result = ext != null && Regex.IsMatch(ext, @"\.zip|\.rar", RegexOptions.IgnoreCase);

            if (result)
            {
                Zipper.ExtractZipFiles(zipFile, Path.GetDirectoryName(zipFile));
                File.Delete(zipFile);
            }
            Console.WriteLine("File Changed: " + result.ToString());
        }


        public static void Test4()
        {
            var dir = Directory.GetCurrentDirectory();
            var outputDir = Path.Combine(dir, "output");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);


            var output = Path.Combine(outputDir, Guid.NewGuid().ToString() + ".zip");
            //var zipper = new Zipper();
            var file = Zipper.ZipFiles(new List<string> { "input1.mp4", "input2.mkv" }, output, dir);
            //File.Delete(file);
            //Zipper.ZipFolder(Path.Combine(dir, "Error2"), output);

        }

        private static void Main2(string[] args)
        {
            //var ser = new System.Web.
            //"Error Sample".Log();
            //Console.WriteLine("Writing Error".ToSqlCharacter());
            //Console.ReadLine();
            /*
                        var sql = new MsSqlServer();
                        var test = sql.SqlExecCommand("pTest", new Dictionary<string, object> { {"test1", 5}, {"test2", true} });
            */

            //MutableStructHolder holder = new MutableStructHolder();
            //// Affects the value of holder.Field
            //holder.Field.SetValue(10);
            //// Retrieves holder.Property as a copy and changes the copy
            //holder.Property.SetValue(10);

            //Console.WriteLine(holder.Field.Value);
            //Console.WriteLine(holder.Property.Value);
            //var a = new JavaScriptSerializer();
            //var result = a.Serialize();
            //var a = new JsonSerializer();
            //var b = "";
            //var c = new StringReader("");
            
            //var b = "Test";
            //var result = JsonConvert.DeserializeObject("{'test': 'yey'}");
            //Console.ReadLine();
            //var a = new StringWriter();
            //var dt = new DataTable();
            //var dc1 = new DataColumn("ID", typeof(int));
            //var dc2 = new DataColumn("Name", typeof(string));
            //dt.Columns.Add(dc1);
            //dt.Columns.Add(dc2);
            //var row = dt.NewRow();
            //row["ID"] = 1;
            //row["Name"] = "test";
            //dt.Rows.Add(row);
            //var result = JsonConvert.SerializeObject(dt);
            //var str = "fdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsafdsafdsafdsafdsafdsafdsafdsafsdf324dfsdfds)*#)$*)#$*@)*#$0)*$#)*$#)@*$fdsfsafsdafsdfdsa";
            //var test =
            //    LZString.compressToBase64(
            //        str);
            //var dec = LZString.decompressFromBase64(test);

            //var str = new StringBuilder();
            //str.Append("test1").Append("fdsafdsa");
            //str.Append("test2");
            //str.Append("test3");
            //str.Append("test4");
            //str.Append("test5");
            //str.Append("test6");
            //str.Append("test7");
            //str.Append("test8");
            //str.Append("test9");
            //str.Append("test10");
            //Console.WriteLine(str.ToString());
            //var sw = new Stopwatch();

            //sw.Start();

            //Thread.Sleep(3000);

            //sw.Stop();

            //var ts = Stopwatch.StartNew();
            //Thread.Sleep(3000);
            //ts.Stop();
            //Console.WriteLine(ts.Elapsed);
/*
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var video = Path.Combine(path, "XML and XSLT Transformation Explained.mp4");
            var thumbnail = Path.Combine(path, "thumbnail", "test.jpg");

            Console.WriteLine(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            //GetThumbnail(video, thumbnail);

            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(path, "ffmpeg.exe"),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = "-i input2.mkv -ss 00:02:00  -s 256x144  -t 5 -r 10 -an output2.3.webm"
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit(5000);
*/
/*
            var thumbnail = new Video(Directory.GetCurrentDirectory() + @"\input2.mkv");
            var sw = new Stopwatch();
            sw.Start();
            thumbnail.Generate(Directory.GetCurrentDirectory() + @"\thumbnails\test2.webm");
            sw.Stop();
*/
            //Console.WriteLine("Conversion End at " + sw.Elapsed.ToString());

            //Console.WriteLine("Connecting to Hub.....");
                
        }

        private async static void Test()
        {
            var param = new Dictionary<string, string>();
            param.Add("Screen", "True");
            var hubConnection = new HubConnection("http://localhost:20061", param);
            var screenProxy = hubConnection.CreateHubProxy("Screen");
            screenProxy.On("hello", (r) =>
            {
                Console.WriteLine(r.ToString());
            });

            await hubConnection.Start();
            //Thread.Sleep(2000);
            //hubConnection.Stop();         
        }


        public static Bitmap GetThumbnail(string video, string thumbnail)
        {
            var cmd = "ffmpeg  -itsoffset -1  -i " + '"' + video + '"' + " -vcodec mjpeg -vframes 1 -an -f rawvideo -s 320x240 " + '"' + thumbnail + '"';

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/C " + cmd
            };

            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
            process.WaitForExit(5000);

            return LoadImage(thumbnail);
        }

        static Bitmap LoadImage(string path)
        {
            var ms = new MemoryStream(File.ReadAllBytes(path));
            return (Bitmap)Image.FromStream(ms);
        }
    }

    struct MutableStruct
    {
        public int Value { get; set; }

        public void SetValue(int newValue)
        {
            Value = newValue;
        }
    }

    class MutableStructHolder
    {
        public MutableStruct Field;
        public MutableStruct Property { get; set; }
    }
}