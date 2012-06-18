using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.WindowsCE.Forms;

namespace Linn.Kinsky
{
    public class DisplayManager
    {
        private static ScreenOrientation iOrientation;
        private static SizeF? iScaleFactorOriented = null;
        private static SizeF? iScaleFactor = null;

        public static SizeF ScaleFactor
        {
            get
            {
                Initialize();

                return iScaleFactor.Value;
            }
        }

        public static SizeF ScaleFactorOriented
        {
            get
            {
                Initialize();

                return iScaleFactorOriented.Value;
            }
        }
        
        public static int ScaleHeight(int aHeight)
        {
            Initialize();

            return (int)(aHeight * iScaleFactor.Value.Width);
        }

        public static int ScaleWidth(int aWidth)
        {
            Initialize();

            return (int)(aWidth * iScaleFactor.Value.Width);
        }

        public static Rectangle ScaleRectangle(int aX, int aY, int aWidth, int aHeight)
        {
            Initialize();

            return new Rectangle((int)(aX * iScaleFactor.Value.Width),
                (int)(aY * iScaleFactor.Value.Width),
                (int)(aWidth * iScaleFactor.Value.Width),
                (int)(aHeight * iScaleFactor.Value.Width));
        }

        public static Rectangle ScaleRectangle(Rectangle aRectangle)
        {
            Initialize();

            //scale for other resolutions other than 480 * 640
            aRectangle.X = (int)(iScaleFactor.Value.Width * aRectangle.X);
            aRectangle.Y = (int)(iScaleFactor.Value.Width * aRectangle.Y);
            aRectangle.Width = (int)(iScaleFactor.Value.Width * aRectangle.Width);
            aRectangle.Height = (int)(iScaleFactor.Value.Width * aRectangle.Height);

            return aRectangle;
        }

        public static Size ScaleSize(int aWidth, int aHeight)
        {
            Initialize();

            return new Size((int)(aWidth * iScaleFactor.Value.Width), (int)(aHeight * iScaleFactor.Value.Width));
        }

        public static void Initialize()
        {
            Screen screen = Screen.PrimaryScreen;
            ScreenOrientation orientation = SystemSettings.ScreenOrientation;

            if (iScaleFactor == null)
            {
                if (orientation == ScreenOrientation.Angle0)
                {
                    iScaleFactor = new SizeF(screen.Bounds.Width / 480.0f, screen.Bounds.Height / 640.0f);
                }
                else
                {
                    iScaleFactor = new SizeF(screen.Bounds.Height / 480.0f, screen.Bounds.Width / 640.0f);
                }
            }

            if (orientation != iOrientation || iScaleFactorOriented == null)
            {
                iScaleFactorOriented = new SizeF(screen.Bounds.Width / 480.0f, screen.Bounds.Height / 640.0f);
                iOrientation = orientation;
            }
        }
    }
}
