using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace Linn.Songcast
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

        internal static Icon GetObjectIcon(string aName)
        {
            string filename = aName + ".ico";
            string fullpath = System.IO.Path.Combine("../../share/Linn/Resources/Songcaster", filename);
            if (!System.IO.File.Exists(fullpath))
            {
                fullpath = System.IO.Path.Combine("Resources", filename);
            }

            return new Icon(fullpath);
        }

        private static BitmapImage iImageIcon;
        internal static BitmapImage Icon
        {
            get
            {
                if (iImageIcon == null)
                {
                    iImageIcon = GetObject(Linn.Songcast.Properties.Resources.IconLarge);
                }
                return iImageIcon;
            }
        }

        private static BitmapImage iImageIconLarge;
        internal static BitmapImage IconLarge
        {
            get
            {
                if (iImageIconLarge == null)
                {
                    iImageIconLarge = GetObject(Linn.Songcast.Properties.Resources.IconXLarge);
                }
                return iImageIconLarge;
            }
        }

        private static BitmapImage iImageConnected;
        internal static BitmapImage Connected
        {
            get
            {
                if (iImageConnected == null)
                {
                    iImageConnected = GetObject(Linn.Songcast.Properties.Resources.Green);
                }
                return iImageConnected;
            }
        }

        private static BitmapImage iImageDisconnected;
        internal static BitmapImage Disconnected
        {
            get
            {
                if (iImageDisconnected == null)
                {
                    iImageDisconnected = GetObject(Linn.Songcast.Properties.Resources.Red);
                }
                return iImageDisconnected;
            }
        }

        private static BitmapImage iImageError;
        internal static BitmapImage Error
        {
            get
            {
                if (iImageError == null)
                {
                    iImageError = GetObject(Linn.Songcast.Properties.Resources.Error);
                }
                return iImageError;
            }
        }

        internal static Icon IconSongcaster
        {
            get
            {
                return Linn.Songcast.Properties.Resources.Songcaster;
            }
        }

        internal static Icon SysTrayIconOff
        {
            get
            {
                return Linn.Songcast.Properties.Resources.SysTrayIconOff;
            }
        }

        internal static Icon SysTrayIconOn
        {
            get
            {
                return Linn.Songcast.Properties.Resources.SysTrayIconOn;
            }
        }

        private static BitmapImage iImageRotaryControl;
        internal static BitmapImage ImageRotaryControl
        {
            get
            {
                if (iImageRotaryControl == null)
                {
                    iImageRotaryControl = GetObject(Linn.Songcast.Properties.Resources.ImageRotaryControl);
                }
                return iImageRotaryControl;
            }
        }

        private static BitmapImage iImageRockerControl;
        internal static BitmapImage ImageRockerControl
        {
            get
            {
                if (iImageRockerControl == null)
                {
                    iImageRockerControl = GetObject(Linn.Songcast.Properties.Resources.ImageRockerControl);
                }
                return iImageRockerControl;
            }
        }
    }
}

