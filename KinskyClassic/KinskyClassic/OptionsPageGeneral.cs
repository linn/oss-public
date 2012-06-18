using System;

using Linn;

namespace KinskyClassic
{
    public class OptionPageGeneral : OptionPage
    {
        public const string kAutoDetect = "Auto detect";
        public const string k800x480Touch = "800x480 for touchscreens";
        public const string k800x480Laptop = "800x480 for laptops";
        public const string k1024x600Touch = "1024x600 for touchscreens";
        public const string k1024x600Laptop = "1024x600 for laptops";

        public OptionPageGeneral()
            : base("General")
        {
            iHideMouseCursor = new OptionBool("hidemouse", "Hide mouse cursor", "Hide the mouse cursor", false);
            Add(iHideMouseCursor);

            iFullscreen = new OptionBool("fullscreen", "Fullscreen", "Fullscreen", false);
            Add(iFullscreen);

            iSkin = new OptionEnum("skin", "Skin", "Skin to use");
            iSkin.AddDefault(kAutoDetect);
            iSkin.Add(k800x480Touch);
            iSkin.Add(k800x480Laptop);
            iSkin.Add(k1024x600Touch);
            iSkin.Add(k1024x600Laptop);
            Add(iSkin);
        }

        public bool HideMouseCursor
        {
            get { return iHideMouseCursor.Native; }
            set { iHideMouseCursor.Native = value; }
        }

        public bool Fullscreen
        {
            get { return iFullscreen.Native; }
            set { iFullscreen.Native = value; }
        }

        public string Skin
        {
            get { return iSkin.Value; }
            set { iSkin.Set(value); }
        }

        public event EventHandler<EventArgs> EventHideMouseCursorChanged
        {
            add { iHideMouseCursor.EventValueChanged += value; }
            remove { iHideMouseCursor.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventFullscreenChanged
        {
            add { iFullscreen.EventValueChanged += value; }
            remove { iFullscreen.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventSkinChanged
        {
            add { iSkin.EventValueChanged += value; }
            remove { iSkin.EventValueChanged -= value; }
        }

        private OptionBool iHideMouseCursor;
        private OptionBool iFullscreen;
        private OptionEnum iSkin;
    }
}
