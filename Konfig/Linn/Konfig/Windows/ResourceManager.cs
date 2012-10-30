using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace Linn.Konfig
{
    internal class ResourceManager
    {
        internal ResourceManager()
        {
        }

        internal static BitmapImage GetObject(Bitmap aBitmap)
        {
            MemoryStream ms = new MemoryStream();
            aBitmap.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = ms;
            bitmap.EndInit();

            return bitmap;
        }

        private static BitmapImage iImageIconLarge;
        internal static BitmapImage IconLarge
        {
            get
            {
                if (iImageIconLarge == null)
                {
                    iImageIconLarge = GetObject(Konfig.Properties.Resources.IconLarge);
                }
                return iImageIconLarge;
            }
        }

        private static BitmapImage iImageIconXLarge;
        internal static BitmapImage IconXLarge
        {
            get
            {
                if (iImageIconXLarge == null)
                {
                    iImageIconXLarge = GetObject(Konfig.Properties.Resources.IconXLarge);
                }
                return iImageIconXLarge;
            }
        }

        internal static Icon Icon
        {
            get
            {
                return Konfig.Properties.Resources.Icon;
            }
        }
    }
}

