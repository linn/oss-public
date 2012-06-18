using Linn;
using Linn.Kinsky;

namespace KinskyDesktop
{
	public class HelperKinskyDesktop : HelperKinsky
	{
		public HelperKinskyDesktop(string[] aArgs, Rect aScreenRect, IInvoker aInvoker)
			: base(aArgs, aInvoker)
        {
            float goldenRatio = 0.6180339887f;
            float windowWidth = aScreenRect.Width * goldenRatio;
            float windowHeight = windowWidth * goldenRatio;

			iOptionPageGeneral = new OptionPageGeneral("General");
			AddOptionPage(iOptionPageGeneral);

            iOptionWindowX = new OptionFloat("windowx", "WindowX", "Main window's x position", aScreenRect.Origin.X + (aScreenRect.Width - windowWidth)*0.5f);
            AddOption(iOptionWindowX);

            iOptionWindowY = new OptionFloat("windowy", "WindowY", "Main window's y position", aScreenRect.Origin.Y + (aScreenRect.Height - windowHeight)*0.5f);
            AddOption(iOptionWindowY);
			
			iOptionWindowWidth = new OptionFloat("windowwidth", "WindowWidth", "Main window's width", windowWidth);
			AddOption(iOptionWindowWidth);
			
			iOptionWindowHeight = new OptionFloat("windowheight", "WindowHeight", "Main window's height", windowHeight);
			AddOption(iOptionWindowHeight);

            iOptionSplitterFraction = new OptionFloat("splitterfraction", "SplitterFraction", "Splitter position in range [0, 1]", goldenRatio);
            AddOption(iOptionSplitterFraction);

            iOptionFullscreen = new OptionBool("fullscreen", "Fullscreen", "Fullscreen mode", false);
            AddOption(iOptionFullscreen);

            iOptionKompactMode = new OptionBool("kompactmode", "KompactMode", "Kompact mode", false);
            AddOption(iOptionKompactMode);

            iOptionContainerView = new OptionUint("containerview", "ContainerView", "Container View", 0);
            AddOption(iOptionContainerView);

            iOptionContainerSizeThumbs = new OptionFloat("containersizethumbs", "ContainerSizeThumbs", "Image size for browser thumbnail view", 0.3f);
            AddOption(iOptionContainerSizeThumbs);

            iOptionContainerSizeList = new OptionFloat("containersizelist", "ContainerSizeList", "Image size for browser list view", 0.3f);
            AddOption(iOptionContainerSizeList);

            iOptionPageUpdates = new OptionPageUpdates(this);
            AddOptionPage(iOptionPageUpdates);
		}

        public OptionPageUpdates OptionPageUpdates
        {
            get
            {
                return iOptionPageUpdates;
            }
        }

		public OptionBool ShowToolTips
		{
			get
			{
				return iOptionPageGeneral.ShowToolTips;
			}
		}
		
		public OptionBool ShowTechnicalInfo
		{
			get
			{
				return iOptionPageGeneral.ShowTechnicalInfo;
			}
		}

        public OptionBool EnableRocker
        {
            get
            {
                return iOptionPageGeneral.EnableRocker;
            }
        }

        public OptionBool PlaylistGrouping
        {
            get
            {
                return iOptionPageGeneral.PlaylistGrouping;
            }
        }

        public OptionFloat WindowX
        {
            get
            {
                return iOptionWindowX;
            }
        }

        public OptionFloat WindowY
        {
            get
            {
                return iOptionWindowY;
            }
        }
		
		public OptionFloat WindowWidth
		{
			get
			{
				return iOptionWindowWidth;
			}
		}
		
		public OptionFloat WindowHeight
		{
			get
			{
				return iOptionWindowHeight;
			}
		}

        public OptionFloat SplitterFraction
        {
            get
            {
                return iOptionSplitterFraction;
            }
        }

        public OptionBool Fullscreen
        {
            get
            {
                return iOptionFullscreen;
            }
        }

        public OptionBool KompactMode
        {
            get
            {
                return iOptionKompactMode;
            }
        }

        public OptionUint ContainerView
        {
            get
            {
                return iOptionContainerView;
            }
        }

        public OptionFloat ContainerSizeThumbs
        {
            get
            {
                return iOptionContainerSizeThumbs;
            }
        }

        public OptionFloat ContainerSizeList
        {
            get
            {
                return iOptionContainerSizeList;
            }
        }

		private OptionPageGeneral iOptionPageGeneral;
        private OptionPageUpdates iOptionPageUpdates;

        private OptionFloat iOptionWindowX;
        private OptionFloat iOptionWindowY;
		private OptionFloat iOptionWindowWidth;
		private OptionFloat iOptionWindowHeight;
        private OptionFloat iOptionSplitterFraction;
        private OptionBool iOptionFullscreen;
        private OptionBool iOptionKompactMode;
        private OptionUint iOptionContainerView;
        private OptionFloat iOptionContainerSizeThumbs;
        private OptionFloat iOptionContainerSizeList;
	}
}
