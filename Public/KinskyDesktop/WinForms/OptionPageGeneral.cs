
using System;

using Linn;


namespace KinskyDesktop
{
    public class OptionPageGeneral : OptionPage
    {
        public OptionPageGeneral()
            : base("General")
        {
            bool isUnix = (Environment.OSVersion.Platform == PlatformID.Unix);

            iTransparency = new OptionBool("windowtransparency", "Window transparency", "Enable transparency in the application", !isUnix);
            Add(iTransparency);

            iWindowBorder = new OptionBool("windowborder", "Window border", "Show the main window border", isUnix);
            Add(iWindowBorder);

            iHideMouseCursor = new OptionBool("hidemouse", "Hide mouse cursor", "Hide the mouse cursor", false);
            Add(iHideMouseCursor);

            iShowTrackInfo = new OptionBool("trackinfo", "Show technical track information", "Show technical information about the currently playing track", true);
            Add(iShowTrackInfo);

            iUseRotaryControls = new OptionBool("rotarycontrols", "Use rotary controls for volume and seeking", "Use rotary controls for volume and seeking", true);
            Add(iUseRotaryControls);

            iShowToolTips = new OptionBool("tooltips", "Show Tool Tips", "Enable tool tips for the application", true);
            Add(iShowToolTips);
        }

        public bool Transparency
        {
            get { return iTransparency.Native; }
        }

        public bool WindowBorder
        {
            get { return iWindowBorder.Native; }
        }

        public bool HideMouseCursor
        {
            get { return iHideMouseCursor.Native; }
        }

        public bool ShowTrackInfo
        {
            get { return iShowTrackInfo.Native; }
        }

        public bool UseRotaryControls
        {
            get { return iUseRotaryControls.Native; }
        }

        public bool ShowToolTips
        {
            get { return iShowToolTips.Native; }
        }

        public event EventHandler<EventArgs> EventTransparencyChanged
        {
            add { iTransparency.EventValueChanged += value; }
            remove { iTransparency.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventWindowBorderChanged
        {
            add { iWindowBorder.EventValueChanged += value; }
            remove { iWindowBorder.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventHideMouseCursorChanged
        {
            add { iHideMouseCursor.EventValueChanged += value; }
            remove { iHideMouseCursor.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventShowTrackInfoChanged
        {
            add { iShowTrackInfo.EventValueChanged += value; }
            remove { iShowTrackInfo.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventUseRotaryControlsChanged
        {
            add { iUseRotaryControls.EventValueChanged += value; }
            remove { iUseRotaryControls.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventShowToolTipsChanged
        {
            add { iShowToolTips.EventValueChanged += value; }
            remove { iShowToolTips.EventValueChanged -= value; }
        }

        private OptionBool iTransparency;
        private OptionBool iWindowBorder;
        private OptionBool iHideMouseCursor;
        private OptionBool iShowTrackInfo;
        private OptionBool iUseRotaryControls;
        private OptionBool iShowToolTips;
    }
}


