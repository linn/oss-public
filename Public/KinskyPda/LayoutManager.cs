using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

using Linn.Kinsky;

namespace KinskyPda
{
    //values are based upon a resolution of 640 * 480
    internal class LayoutManager
    {
        private static LayoutManager iInstance;

        internal static LayoutManager Instance
        {
            get
            {
                if (iInstance == null)
                {
                    iInstance = new LayoutManager();
                }
                return iInstance;
            }
        }

        private LayoutManager() { }

        public Rectangle TopBounds
        {
            get { return new Rectangle(0, 0, 480, 47); }
        }

        public Rectangle CentreBounds
        {
            get { return new Rectangle(0, 47, 480, 401); }
        }

        public Rectangle BottomBounds
        {
            get { return new Rectangle(0, 448, 480, 140); }
        }

        public Rectangle AllBounds
        {
            get { return new Rectangle(0, 0, 480, 588); }
        }

        public Rectangle PanelToolsBounds
        {
            // the bounds of the panel tools relative to its parent (BottomBounds)
            get { return new Rectangle(106, 0, 266, 140); }
        }

        public Rectangle ButtonVolumeDownBounds
        {
            // the bounds of the button relative to its parent (BottomRect)
            get { return new Rectangle(4, 20, 100, 100); }
        }

        public Rectangle ButtonVolumeDownHitBounds
        {
            // the actual hit bounds (the bounds of the button control) relative to the button bounds
            get { return new Rectangle(0, 0, 100, 100); }
        }

        public Rectangle ButtonVolumeUpBounds
        {
            // the bounds of the button relative to its parent (BottomBounds)
            get { return new Rectangle(376, 20, 100, 100); }
        }

        public Rectangle ButtonVolumeUpHitBounds
        {
            // the actual hit bounds (the bounds of the button control) relative to the button bounds
            get { return new Rectangle(0, 0, 100, 100); }
        }

        public Rectangle ButtonLeftBackgroundBounds
        {
            // the bounds of the button relative to the background image
            get { return new Rectangle(112, 20, 100, 100); }
        }

        public Rectangle ButtonLeftBounds
        {
            // the bounds of the button relative to its parent (PanelToolsBounds)
            get { return new Rectangle(6, 20, 100, 100); }
        }

        public Rectangle ButtonLeftHitBounds
        {
            // the actual hit bounds (the bounds of the button control) relative to the button bounds
            get { return new Rectangle(0, 12, 76, 76); }
        }

        public Rectangle ButtonCentreBackgroundBounds
        {
            // the bounds of the button relative to the background image
            get { return new Rectangle(164, 0, 150, 140); }
        }

        public Rectangle ButtonCentreBounds
        {
            // the bounds of the button relative to its parent (PanelToolsBounds)
            get { return new Rectangle(58, 0, 150, 140); }
        }

        public Rectangle ButtonCentreHitBounds
        {
            // the actual hit bounds (the bounds of the button control) relative to the button bounds
            get { return new Rectangle(26, 20, 100, 100); }
        }

        public Rectangle ButtonRightBackgroundBounds
        {
            // the bounds of the button relative to the background image
            get { return new Rectangle(266, 20, 100, 100); }
        }

        public Rectangle ButtonRightBounds
        {
            // the bounds of the button relative to its parent (PanelToolsBounds)
            get { return new Rectangle(160, 20, 100, 100); }
        }

        public Rectangle ButtonRightHitBounds
        {
            // the actual hit bounds (the bounds of the button control) relative to the button bounds
            get { return new Rectangle(24, 12, 76, 76); }
        }

        public Size SmallImageSize
        {
            get { return DisplayManager.ScaleSize(65, 50); }
        }

        public Size BrowseIconImageSize
        {
            get { return DisplayManager.ScaleSize(64, 64); }
        }
    }
}