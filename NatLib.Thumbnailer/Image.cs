using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NatLib.Thumbnailer
{
    public class Image: IDisposable
    {
        private bool _disposed = false;

        #region Properties
        public double ScaleFactor { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }

        #endregion

        public Image(string sourcePath, string targetPath)
        {
            SourcePath = sourcePath;
            TargetPath = targetPath;
            ScaleFactor = 0.10;
        }

        public void Generate()
        {
            using (var image = System.Drawing.Image.FromFile(SourcePath))
            {
                var newWidth = (int)(image.Width * ScaleFactor);
                var newHeight = (int)(image.Height * ScaleFactor);
                var thumbnailImg = new Bitmap(newWidth, newHeight);
                var thumbGraph = Graphics.FromImage(thumbnailImg);
                thumbGraph.CompositingQuality = CompositingQuality.HighQuality;
                thumbGraph.SmoothingMode = SmoothingMode.HighQuality;
                thumbGraph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var imageRectangle = new Rectangle(0, 0, newWidth, newHeight);
                thumbGraph.DrawImage(image, imageRectangle);
                thumbnailImg.Save(TargetPath, image.RawFormat);
            }
        }



        #region Dispose Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {

            }
        }

        ~Image()
        {

        } 
        #endregion
    }
}
