using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Linn.Kinsky;

namespace KinskyPda
{
    public class ViewSupport
    {
        private static ViewSupport iInstance;

        public static ViewSupport Instance
        {
            get
            {
                if (iInstance == null)
                {
                    iInstance = new ViewSupport();
                }
                return iInstance;
            }
        }

        public ViewSupport()
        {
            iBackColour = System.Drawing.Color.FromArgb((22), (22), (22));
            iForeColour = Color.FromArgb(176, 196, 196);

            iHighlightBackColour = Color.FromArgb(0, 189, 255);
            iHighlightForeColour = Color.Black;

            iForeColourBright = Color.White;
            iForeColourMuted = Color.White;

            iFontLarge = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            iFontMedium = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular);
            iFontSmall = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Regular);
        }

        public Color BackColour
        {
            get { return iBackColour; }
        }

        public Color ForeColour
        {
            get { return iForeColour; }
        }

        public Color HighlightBackColour
        {
            get { return iHighlightBackColour; }
        }

        public Color HighlightForeColour
        {
            get { return iHighlightForeColour; }
        }

        public Color ForeColourBright
        {
            get { return iForeColourBright; }
        }

        public Color ForeColourMuted
        {
            get { return iForeColourMuted; }
        }

        public Font FontLarge
        {
            get { return iFontLarge; }
        }

        public Font FontMedium
        {
            get { return iFontMedium; }
        }

        public Font FontSmall
        {
            get { return iFontSmall; }
        }

        private Color iBackColour;
        private Color iForeColour;
        private Color iHighlightBackColour;
        private Color iHighlightForeColour;
        private Color iForeColourBright;
        private Color iForeColourMuted;
        private Font iFontLarge;
        private Font iFontMedium;
        private Font iFontSmall;
    }
}
