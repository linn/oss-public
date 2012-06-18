using System;

using MonoMac.AppKit;

namespace MagicMouseControl
{
    public abstract class InputTracker : NSResponder
    {
        public InputTracker()
        {
            iEnabled = true;
        }

        public bool Enabled
        {
            get
            {
                return iEnabled;
            }
            set
            {
                iEnabled = value;
            }
        }

        public NSView View
        {
            get
            {
                return iView;
            }
            set
            {
                iView = value;
            }
        }

        public abstract void CancelTracking();

        private bool iEnabled;
        private NSView iView;
    }
}

