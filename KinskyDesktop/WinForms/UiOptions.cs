
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

using Linn;


namespace KinskyDesktop
{
    public class UiOptions
    {
        public UiOptions(IHelper aHelper)
        {
            iWindowWidth = new OptionInt("windowwidth", "WindowWidth", "", 1024);
            iWindowHeight = new OptionInt("windowheight", "WindowHeight", "", 600);
            iWindowLocationX = new OptionInt("windowx", "WindowLocationX", "", -1);
            iWindowLocationY = new OptionInt("windowy", "WindowLocationY", "", -1);
            iSplitterLocation = new OptionInt("splitterx", "SplitterLocation", "", 502);
            iFullscreen = new OptionBool("fullscreen", "Fullscreen", "", false);
            iMiniMode = new OptionBool("minimode", "MiniMode", "", false);
            iMiniModeWidth = new OptionInt("minimodewidth", "MiniModeWidth", "", Screen.PrimaryScreen.WorkingArea.Width / 2);
            iMiniModeLocationX = new OptionInt("minimodex", "MiniModeLocationX", "", Screen.PrimaryScreen.WorkingArea.Width / 2);
            iMiniModeLocationY = new OptionInt("minimodey", "MiniModeLocationY", "", Screen.PrimaryScreen.WorkingArea.Height - 100);
            iAlbumView = new OptionUint("albumview", "AlbumView", "", 0);
            iContainerView = new OptionUint("containerview", "ContainerView", "", 0);
            iContainerViewSize = new OptionUint("containerviewsize", "ContainerViewSize", "", 0);

            aHelper.AddOption(iWindowWidth);
            aHelper.AddOption(iWindowHeight);
            aHelper.AddOption(iWindowLocationX);
            aHelper.AddOption(iWindowLocationY);
            aHelper.AddOption(iSplitterLocation);
            aHelper.AddOption(iFullscreen);
            aHelper.AddOption(iMiniMode);
            aHelper.AddOption(iMiniModeWidth);
            aHelper.AddOption(iMiniModeLocationX);
            aHelper.AddOption(iMiniModeLocationY);
            aHelper.AddOption(iAlbumView);
            aHelper.AddOption(iContainerView);
            aHelper.AddOption(iContainerViewSize);
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
                iWindowLocationX.Native  = value.X;
                iWindowLocationY.Native = value.Y;
            }
        }

        public int SplitterLocation
        {
            get
            {
                return iSplitterLocation.Native;
            }
            set
            {
                iSplitterLocation.Native = value;
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

        public int MiniModeWidth
        {
            get
            {
                return iMiniModeWidth.Native;
            }
            set
            {
                iMiniModeWidth.Native = value;
            }
        }

        public Point MiniModeLocation
        {
            get
            {
                return new Point(iMiniModeLocationX.Native, iMiniModeLocationY.Native);
            }
            set
            {
                iMiniModeLocationX.Native = value.X;
                iMiniModeLocationY.Native = value.Y;
            }
        }

        public uint AlbumView
        {
            get
            {
                return iAlbumView.Native;
            }
            set
            {
                iAlbumView.Native = value;
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

        public uint ContainerViewSize
        {
            get
            {
                return iContainerViewSize.Native;
            }
            set
            {
                iContainerViewSize.Native = value;
            }
        }

        private OptionInt iWindowWidth;
        private OptionInt iWindowHeight;
        private OptionInt iWindowLocationX;
        private OptionInt iWindowLocationY;
        private OptionInt iSplitterLocation;
        private OptionBool iFullscreen;
        private OptionBool iMiniMode;
        private OptionInt iMiniModeWidth;
        private OptionInt iMiniModeLocationX;
        private OptionInt iMiniModeLocationY;
        private OptionUint iAlbumView;
        private OptionUint iContainerView;
        private OptionUint iContainerViewSize;
    }
}