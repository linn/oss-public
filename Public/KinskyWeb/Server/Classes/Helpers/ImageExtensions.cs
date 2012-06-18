
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
namespace KinskyWeb.Helpers
{
    namespace Extensions
    {
        public static class ImageExtensions
        {
            public static Stream GetStream(this Image aImage, ImageFormat aFormat)
            {
                Stream s = new MemoryStream();
                aImage.Save(s, aFormat);
                s.Position = 0;
                return s;
            }

        }
    }
}