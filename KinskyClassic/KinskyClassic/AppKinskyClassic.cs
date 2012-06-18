/*using System;

using Linn;
using Linn.Kinsky;

namespace KinskyClassic
{
    public class HelperKinskyClassic : HelperKinsky
    {
        public HelperKinskyClassic()
        {
        }

        protected override void OnStart()
        {
            base.OnStart();

            UserOptionsDialog.Remove(iUserOptionsLocalPlaylist.OptionsPage);

            iUserOptions = new UserOptions(BasePath);
            UserOptionsDialog.Add(iUserOptions.OptionsPageGeneral);

            iUserOptions.EventAutoDetectChanged += AutoDetectChanged;
            iUserOptions.EventFullscreenChanged += FullscreenChanged;
            iUserOptions.EventHideMouseCursorChanged += HideMouseCursorChanged;
            iUserOptions.EventSkinChanged += SkinChanged;
        }

        public bool AutoDetect
        {
            get
            {
                return iUserOptions.AutoDetect;
            }
        }

        public bool HideMouseCursor
        {
            get
            {
                return iUserOptions.HideMouseCursor;
            }
        }

        public bool Fullscreen
        {
            get
            {
                return iUserOptions.Fullscreen;
            }
        }

        public int Skin
        {
            get
            {
                return iUserOptions.Skin;
            }
        }

        public void SetAutoDetect(bool aAutoDetect)
        {
            iUserOptions.SetAutoDetect(aAutoDetect);
        }

        public void SetHideMouseCursor(bool aHideMouseCursor)
        {
            iUserOptions.SetHideMouseCursor(aHideMouseCursor);
        }

        public void SetFullscreen(bool aFullscreen)
        {
            iUserOptions.SetFullscreen(aFullscreen);
        }

        public void SetSkin(int aSkin)
        {
            iUserOptions.SetSkin(aSkin);
        }

        public event EventHandler<EventArgs> EventAutoDetectChanged;
        public event EventHandler<EventArgs> EventHideMouseCursorChanged;
        public event EventHandler<EventArgs> EventFullscreenChanged;
        public event EventHandler<EventArgs> EventSkinChanged;

        private void AutoDetectChanged(object sender, EventArgs e)
        {
            if (EventAutoDetectChanged != null)
            {
                EventAutoDetectChanged(this, EventArgs.Empty);
            }
        }

        private void FullscreenChanged(object sender, EventArgs e)
        {
            if (EventFullscreenChanged != null)
            {
                EventFullscreenChanged(this, EventArgs.Empty);
            }
        }

        private void HideMouseCursorChanged(object sender, EventArgs e)
        {
            if (EventHideMouseCursorChanged != null)
            {
                EventHideMouseCursorChanged(this, EventArgs.Empty);
            }
        }

        private void SkinChanged(object sender, EventArgs e)
        {
            if (EventSkinChanged != null)
            {
                EventSkinChanged(this, EventArgs.Empty);
            }
        }

        private UserOptions iUserOptions;
    }
} // KinskyClassic*/
