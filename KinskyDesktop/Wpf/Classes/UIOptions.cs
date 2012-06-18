using System.Windows;
using Linn;
using System.IO;
namespace KinskyDesktopWpf
{
    public class UiOptions
    {
        public UiOptions(IHelper aHelper)
        {
            iWindowWidth = new OptionDouble("windowwidth", "WindowWidth", "", 1024);
            iWindowHeight = new OptionDouble("windowheight", "WindowHeight", "", 600);
            iWindowLocationX = new OptionDouble("windowx", "WindowLocationX", "", -1);
            iWindowLocationY = new OptionDouble("windowy", "WindowLocationY", "", -1);
            iDetailsWindowWidth = new OptionDouble("detailswindowwidth", "DetailsWindowWidth", "", 320);
            iDetailsWindowHeight = new OptionDouble("detailswindowheight", "DetailsWindowHeight", "", 450);

            iBrowserSplitterLocationLeft = new OptionInt("browsersplitterleft", "BrowserSplitterLocationLeft", "", 652);
            iBrowserSplitterLocationRight = new OptionInt("browsersplitterright", "BrowserSplitterLocationRight", "", 404);
            iFullscreen = new OptionBool("fullscreen", "Fullscreen", "", false);
            iMiniMode = new OptionBool("minimode", "MiniMode", "", false);
            iContainerView = new OptionUint("containerview", "ContainerView", "", 0);
            iContainerViewSizeThumbsView = new OptionDouble("containerviewsizethumbs", "ContainerViewSizeThumbs", "", 150);
            iContainerViewSizeListView = new OptionDouble("containerviewsizelist", "ContainerViewSizeList", "", 100);

            aHelper.AddOption(iWindowWidth);
            aHelper.AddOption(iWindowHeight);
            aHelper.AddOption(iDetailsWindowWidth);
            aHelper.AddOption(iDetailsWindowHeight);
            aHelper.AddOption(iWindowLocationX);
            aHelper.AddOption(iWindowLocationY);
            aHelper.AddOption(iBrowserSplitterLocationLeft);
            aHelper.AddOption(iBrowserSplitterLocationRight);
            aHelper.AddOption(iFullscreen);
            aHelper.AddOption(iMiniMode);
            aHelper.AddOption(iContainerView);
            aHelper.AddOption(iContainerViewSizeThumbsView);
            aHelper.AddOption(iContainerViewSizeListView);
        }

        public Size DetailsWindowSize
        {
            get
            {
                return new Size(iDetailsWindowWidth.Native, iDetailsWindowHeight.Native);
            }
            set
            {
                iDetailsWindowWidth.Native = value.Width;
                iDetailsWindowHeight.Native = value.Height;
            }
        }

        public Size WindowSize
        {
            get
            {
                return new Size(iWindowWidth.Native, iWindowHeight.Native);
            }
            set
            {
                iWindowWidth.Native = value.Width;
                iWindowHeight.Native = value.Height;
            }
        }

        public Point WindowLocation
        {
            get
            {
                return new Point(iWindowLocationX.Native, iWindowLocationY.Native);
            }
            set
            {
                iWindowLocationX.Native = value.X;
                iWindowLocationY.Native = value.Y;
            }
        }

        public int BrowserSplitterLocationLeft
        {
            get
            {
                return iBrowserSplitterLocationLeft.Native;
            }
            set
            {
                iBrowserSplitterLocationLeft.Native = value;
            }
        }

        public int BrowserSplitterLocationRight
        {
            get
            {
                return iBrowserSplitterLocationRight.Native;
            }
            set
            {
                iBrowserSplitterLocationRight.Native = value;
            }
        }

        public bool Fullscreen
        {
            get
            {
                return iFullscreen.Native;
            }
            set
            {
                iFullscreen.Native = value;
            }
        }

        public bool MiniMode
        {
            get
            {
                return iMiniMode.Native;
            }
            set
            {
                iMiniMode.Native = value;
            }
        }

        public uint ContainerView
        {
            get
            {
                return iContainerView.Native;
            }
            set
            {
                iContainerView.Native = value;
            }
        }

        public double ContainerViewSizeListView
        {
            get
            {
                return iContainerViewSizeListView.Native;
            }
            set
            {
                iContainerViewSizeListView.Native = value;
            }
        }

        public double ContainerViewSizeThumbsView
        {
            get
            {
                return iContainerViewSizeThumbsView.Native;
            }
            set
            {
                iContainerViewSizeThumbsView.Native = value;
            }
        }

        private OptionDouble iWindowWidth;
        private OptionDouble iWindowHeight;
        private OptionDouble iDetailsWindowWidth;
        private OptionDouble iDetailsWindowHeight;
        private OptionDouble iWindowLocationX;
        private OptionDouble iWindowLocationY;
        private OptionInt iBrowserSplitterLocationLeft;
        private OptionInt iBrowserSplitterLocationRight;
        private OptionBool iFullscreen;
        private OptionBool iMiniMode;
        private OptionUint iContainerView;
        private OptionDouble iContainerViewSizeThumbsView;
        private OptionDouble iContainerViewSizeListView;
    }
}