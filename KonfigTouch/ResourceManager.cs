using System;
using System.IO;

using MonoTouch.UIKit;

namespace KonfigTouch.Properties
{
    internal class ResourceManager
    {
        internal ResourceManager() {
        }

        internal static UIImage GetObject(string aName) {
            string filename = aName + ".png";
            return UIImage.FromFile(filename);
        }

        internal static UIImage DeviceImagePlaceholder {
            get {
                if (iImageIcon == null) {
                    iImageIcon = GetObject("DeviceImagePlaceholder");
                }
                return iImageIcon;
            }
        }
		
		private static UIImage iImageIcon;
    }
}

