using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace NatLib.Thumbnailer
{
    /// <summary>
    /// Dependent on ffmpeg, please make sure that the ffmpeg.exe is located in the FfmpegLocation
    /// </summary>
    public class Video: IDisposable
    {
        /*Fields*/
        private bool _disposed = false;
        private readonly string _input;
        private string _output;
        
        //private AutoResetEvent _generateHandle = new AutoResetEvent(false);

        /*Properties*/

        public string Output => _output;
        public string FileName { get; set; }
        public string StartTime { get; set; }        
        public string OutputDirectory { get; set; }
        public int OutputLength { get; set; }
        public int FrameRate { get; set; }
        public string Size { get; set; }
        public string CurrentDirectory { get; set; }
        public ProcessStartInfo ProcessStartInfo { get; set; }
        public Process Process { get; set; }
        public string FfmpegLocation { get; set; }


        public Video(string input)
        {
            _input = input;

            CurrentDirectory = HostingEnvironment.IsHosted ? HttpContext.Current.Server.MapPath("~") : Path.GetDirectoryName(_input);
            OutputDirectory = Path.Combine(CurrentDirectory, "thumbnails");
            FfmpegLocation = Path.Combine(CurrentDirectory, "ffmpeg.exe");

            if (!Directory.Exists(OutputDirectory)) Directory.CreateDirectory(OutputDirectory);

            FileName = Guid.NewGuid().ToString() + ".webm";
            
            StartTime = "00:00:05";
            Size = "176x144"; //"256x144";
            FrameRate = 25; //25; //10;
            OutputLength = 5;
        }


        /*Methods*/

        public void Generate(string outputFile = "")
        {
            _output = outputFile == "" ? Path.Combine(OutputDirectory, FileName) : outputFile;

            var ffmpegArgs = $"-i {_input} -ss {StartTime}  -s {Size}  -t {OutputLength} -r {FrameRate} -an {_output}";

            ProcessStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe", //FfmpegLocation, 
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                //Arguments = ffmpegArgs
            };

            


            Process = new Process
            {
                StartInfo = ProcessStartInfo                
            };



            //var output = new StringBuilder();
            //var error = new StringBuilder();
            var output = new List<string>();
            var error = new List<string>();


            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
            using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
            {
                DataReceivedEventHandler processOnOutputDataReceived = (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        outputWaitHandle.Set();
                    }
                    else
                    {
                        //output.AppendLine(e.Data);
                        output.Add(e.Data);
                    }
                };

                DataReceivedEventHandler processOnErrorDataReceived = (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        errorWaitHandle.Set();
                    }
                    else
                    {
                        //error.AppendLine(e.Data);
                        error.Add(e.Data);
                    }
                };


                Process.OutputDataReceived += processOnOutputDataReceived;
                Process.ErrorDataReceived += processOnErrorDataReceived;

                Process.Start();

                /*nat 20170720*/

                var standardInput = Process.StandardInput;
                standardInput.WriteLine("@echo off");
                standardInput.WriteLine($"{FfmpegLocation} {ffmpegArgs}");
                standardInput.Flush();
                standardInput.Close();
                /*................*/


                Process.BeginOutputReadLine();
                Process.BeginErrorReadLine();
                var timeout = 50000; //5000

                if (Process.WaitForExit(timeout) &&
                    outputWaitHandle.WaitOne(timeout) &&
                    errorWaitHandle.WaitOne(timeout))
                {
                    // Process completed. Check process.ExitCode here.
                    if (error.Any(r => r.Contains("Output file is empty")))
                    {
                        throw new Exception("Thumbnail did not created successfully.");
                    }
                }
                else
                {
                    // Timed out.
                }
            }

        }

        /*
                private void Generate_End(object sender, EventArgs e)
                {
                    _generateHandle.Set();
                }
        */


        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                Process.Dispose();              
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        ~Video()
        {
            Dispose(false);
        }
    }
}
