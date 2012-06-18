
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Linn;
using Linn.Kinsky;

using KinskyPda;
using KinskyPda.Widgets;

namespace KinskyPda.Views
{
    public class UserControlPlay : UserControl
    {
        public ContextMenu PContextMenu
        {
            get { return iContextMenu; }
        }

        public IViewWidgetTrack ViewWidgetTrack
        {
            get { return iWidgetNowPlayingDsRadio; }
        }

        public IViewWidgetPlaylistRadio ViewWidgetPlaylistRadio
        {
            get { return iWidgetPlaylistRadio; }
        }

        public IViewWidgetPlaylistAux ViewWidgetPlaylistAux
        {
            get { return iWidgetNowPlayingAux; }
        }

        public IViewWidgetMediaTime ViewWidgetMediaTime
        {
            get { return iWidgetPlayControls.ViewWidgetMediaTime; }
        }

        public IViewWidgetVolumeControl ViewWidgetVolumeControl
        {
            get { return iWidgetPlayControls.ViewWidgetVolumeControl; }
        }

        public IViewWidgetTransportControl ViewWidgetTransportControlPause
        {
            get { return iWidgetPlayControls.ViewWidgetTransportControlPause; }
        }

        public IViewWidgetTransportControl ViewWidgetTransportControlStop
        {
            get { return iWidgetPlayControls.ViewWidgetTransportControlStop; }
        }

        public IViewWidgetPlaylist ViewWidgetPlaylist
        {
            get { return iWidgetPlaylistDs; }
        }

        public IViewWidgetPlaylistReceiver ViewWidgetPlaylistReceiver
        {
            get { return iWidgetPlaylistReceiver; }
        }

        public IViewWidgetPlayMode ViewWidgetPlayMode
        {
            get { return iWidgetPlayMode; }
        }

        public UserControlPlay()
        {
            // create the "Now Playing" control for DS sources
            iWidgetNowPlayingDsRadio = new WidgetTrack();
            iWidgetNowPlayingDsRadio.BackColor = ViewSupport.Instance.BackColour;
            iWidgetNowPlayingDsRadio.Dock = DockStyle.Fill;

            // create the "Now Playing" control for aux sources
            iWidgetNowPlayingAux = new WidgetPlaylistAux();
            iWidgetNowPlayingAux.BackColor = ViewSupport.Instance.BackColour;
            iWidgetNowPlayingAux.Dock = DockStyle.Fill;

            // create the parent control for the playlists
            iWidgetPlaylistParent = new Panel();
            iWidgetPlaylistParent.BackColor = ViewSupport.Instance.BackColour;
            iWidgetPlaylistParent.Dock = DockStyle.Fill;

            // create the DS playlist control
            iWidgetPlaylistDs = new WidgetPlaylist();
            iWidgetPlaylistDs.Dock = DockStyle.Fill;
            iWidgetPlaylistDs.Visible = false;
            iWidgetPlaylistDs.Parent = iWidgetPlaylistParent;

            // create the radio playlist control
            iWidgetPlaylistRadio = new WidgetPlaylistRadio();
            iWidgetPlaylistRadio.Dock = DockStyle.Fill;
            iWidgetPlaylistRadio.Visible = false;
            iWidgetPlaylistRadio.Parent = iWidgetPlaylistParent;

            // create the receiver playlist control
            iWidgetPlaylistReceiver = new WidgetPlaylistReceiver();
            iWidgetPlaylistReceiver.Dock = DockStyle.Fill;
            iWidgetPlaylistReceiver.Visible = false;
            iWidgetPlaylistReceiver.Parent = iWidgetPlaylistParent;

            // create the control for the transport, volume controls etc...
            iWidgetPlayControls = new WidgetVolume();
            iWidgetPlayControls.BackColor = ViewSupport.Instance.BackColour;
            iWidgetPlayControls.Bounds = LayoutManager.Instance.BottomBounds;
            iWidgetPlayControls.Dock = DockStyle.Bottom;

            // create the busy panel
            iPanelBusy = new PanelBusy();

            // add the controls to the parent
            this.SuspendLayout();

            this.BackColor = ViewSupport.Instance.BackColour;
            this.AutoScaleDimensions = new SizeF(192F, 192F);
            this.AutoScaleMode = AutoScaleMode.Dpi;

            iWidgetNowPlayingDsRadio.Parent = this;
            iWidgetNowPlayingAux.Parent = this;
            iWidgetPlaylistParent.Parent = this;
            iWidgetPlayControls.Parent = this;
            iPanelBusy.Parent = this;

            this.ResumeLayout(false);

            // create the context menu
            iContextMenu = new ContextMenuPlay();
            iContextMenuHelper = new ContextMenuHelper(iContextMenu);

            GotFocus += EventGotFocus;

            // create the play mode widget
            iWidgetPlayMode = new WidgetPlayMode(this, iContextMenu);

            // setup the eventing from the child widgets
            iWidgetNowPlayingAux.EventOpen += WidgetPlaylistAuxOpen;
            iWidgetNowPlayingAux.EventClose += WidgetPlaylistAuxClose;
            iWidgetPlaylistRadio.EventOpen += WidgetPlaylistRadioOpen;
            iWidgetPlaylistRadio.EventClose += WidgetPlaylistRadioClose;
            iWidgetPlaylistDs.EventOpen += WidgetPlaylistDsOpen;
            iWidgetPlaylistDs.EventClose += WidgetPlaylistDsClose;
            iWidgetPlaylistReceiver.EventOpen += WidgetPlaylistReceiverOpen;
            iWidgetPlaylistReceiver.EventClose += WidgetPlaylistReceiverClose;

            iCurrentSource = ESource.eNone;
        }

        public void Load(OptionBool aOptionPlaylistInfo, OptionEnum aOptionVolumeStep)
        {
            this.ContextMenu = iContextMenu;
            iPanelBusy.ContextMenu = iContextMenu;
            iWidgetNowPlayingDsRadio.ContextMenu = iContextMenu;
            iWidgetNowPlayingAux.ContextMenu = iContextMenu;
            iWidgetPlaylistParent.ContextMenu = iContextMenu;
            iWidgetPlaylistRadio.ContextMenu = iContextMenu;
            iWidgetPlaylistReceiver.ContextMenu = iContextMenu;
            iWidgetPlaylistDs.Load(iContextMenu, aOptionPlaylistInfo);
            iWidgetPlayControls.Load(iContextMenu, aOptionVolumeStep);

            // set bounds of the busy panel
            Rectangle bounds = new Rectangle(0, 0,
                                             Bounds.Width,
                                             iPanelBusy.MinimumSize.Height);
            bounds.Offset(0, (Bounds.Height - bounds.Height) / 2);
            bounds.Inflate(-50, 0);
            iPanelBusy.Bounds = bounds;
            iPanelBusy.BringToFront();

            SetPlayState("playing");
        }

        public void SetPlayState(string aPlayState)
        {
            iPlayState = aPlayState;

            SuspendLayout();

            // show or hide the busy panel
            if (iCurrentSource == ESource.eNone)
            {
                iPanelBusy.StartBusy("Please wait...");
            }
            else
            {
                iPanelBusy.StopBusy();
            }

            if (aPlayState == "playing")
            {
                // "now playing" screen
                iWidgetPlaylistParent.Visible = false;
                iWidgetNowPlayingAux.Visible = (iCurrentSource == ESource.eAux);
                iWidgetNowPlayingDsRadio.Visible = (iCurrentSource == ESource.eDs || iCurrentSource == ESource.eRadio || iCurrentSource == ESource.eReceiver);
            }
            else
            {
                // "playlist" screen
                iWidgetNowPlayingDsRadio.Visible = false;
                iWidgetNowPlayingAux.Visible = false;
                iWidgetPlaylistParent.Visible = true;
            }

            // configure the context menu
            List<int> itemsToHide = new List<int>();
            switch (iCurrentSource)
            {
                case ESource.eNone:
                    itemsToHide.Add(ContextMenuPlay.kMuteTrackMenuPosition);
                    itemsToHide.Add(ContextMenuPlay.kSeperator1MenuPosition);
                    itemsToHide.Add(ContextMenuPlay.kRepeatTrackMenuPosition);
                    itemsToHide.Add(ContextMenuPlay.kShuffleTrackMenuPosition);
                    itemsToHide.Add(ContextMenuPlay.kSeperator2MenuPosition);
                    itemsToHide.Add(ContextMenuPlay.kDeleteTrackMenuPosition);
                    itemsToHide.Add(ContextMenuPlay.kDeleteAllTrackMenuPosition);
                    itemsToHide.Add(ContextMenuPlay.kSeperator3MenuPosition);
                    break;

                case ESource.eAux:
                case ESource.eRadio:
                case ESource.eReceiver:
                    itemsToHide.Add(ContextMenuPlay.kRepeatTrackMenuPosition);
                    itemsToHide.Add(ContextMenuPlay.kShuffleTrackMenuPosition);
                    itemsToHide.Add(ContextMenuPlay.kSeperator2MenuPosition);
                    itemsToHide.Add(ContextMenuPlay.kDeleteTrackMenuPosition);
                    itemsToHide.Add(ContextMenuPlay.kDeleteAllTrackMenuPosition);
                    itemsToHide.Add(ContextMenuPlay.kSeperator3MenuPosition);
                    break;

                case ESource.eDs:
                    if (aPlayState == "playing")
                    {
                        itemsToHide.Add(ContextMenuPlay.kDeleteTrackMenuPosition);
                    }
                    break;
            }

            iContextMenuHelper.ShowAllItems();
            iContextMenuHelper.HideItems(itemsToHide);

            ResumeLayout(false);
        }


        private void WidgetPlaylistAuxOpen()
        {
            iCurrentSource = ESource.eAux;
            SetPlayState(iPlayState);
        }

        private void WidgetPlaylistAuxClose()
        {
            iCurrentSource = ESource.eNone;
            SetPlayState(iPlayState);
        }

        private void WidgetPlaylistRadioOpen()
        {
            iCurrentSource = ESource.eRadio;
            SetPlayState(iPlayState);
        }

        private void WidgetPlaylistRadioClose()
        {
            iCurrentSource = ESource.eNone;
            SetPlayState(iPlayState);
        }

        private void WidgetPlaylistDsOpen()
        {
            iCurrentSource = ESource.eDs;
            SetPlayState(iPlayState);
        }

        private void WidgetPlaylistDsClose()
        {
            iCurrentSource = ESource.eNone;
            SetPlayState(iPlayState);
        }

        private void WidgetPlaylistReceiverOpen()
        {
            iCurrentSource = ESource.eReceiver;
            SetPlayState(iPlayState);
        }

        private void WidgetPlaylistReceiverClose()
        {
            iCurrentSource = ESource.eNone;
            SetPlayState(iPlayState);
        }

        private void EventGotFocus(object sender, EventArgs e)
        {
            if (Focused)
            {
                if (iWidgetPlaylistRadio.Visible)
                {
                    iWidgetPlaylistRadio.Focus();
                }
                if (iWidgetPlaylistDs.Visible)
                {
                    iWidgetPlaylistDs.Focus();
                }
                if (iWidgetPlaylistReceiver.Visible)
                {
                    iWidgetPlaylistReceiver.Focus();
                }
            }
        }

        private enum ESource
        {
            eNone,
            eAux,
            eDs,
            eRadio,
            eReceiver
        }

        private WidgetVolume iWidgetPlayControls;
        private Panel iWidgetPlaylistParent;
        private WidgetPlaylist iWidgetPlaylistDs;
        private WidgetPlaylistRadio iWidgetPlaylistRadio;
        private WidgetPlaylistReceiver iWidgetPlaylistReceiver;
        private WidgetTrack iWidgetNowPlayingDsRadio;
        private WidgetPlaylistAux iWidgetNowPlayingAux;
        private WidgetPlayMode iWidgetPlayMode;
        private PanelBusy iPanelBusy;
        private ESource iCurrentSource;
        private string iPlayState;
        private ContextMenuPlay iContextMenu;
        private ContextMenuHelper iContextMenuHelper;
    }


    internal class ContextMenuPlay : ContextMenu
    {
        private MenuItem AddMenuItem(string aText)
        {
            MenuItem item = new MenuItem();
            item.Text = aText;
            this.MenuItems.Add(item);
            return item;
        }

        public ContextMenuPlay()
        {
            MenuItemMute        = AddMenuItem("Mute");
            Separator1          = AddMenuItem("-");
            MenuItemRepeat      = AddMenuItem("Repeat");
            MenuItemShuffle     = AddMenuItem("Shuffle");
            Separator2          = AddMenuItem("-");
            MenuItemDeleteTrack = AddMenuItem("Delete Track");
            MenuItemDeleteAll   = AddMenuItem("Delete All");
            Separator3          = AddMenuItem("-");
            MenuItemRoomSource  = AddMenuItem("Room/Source");
            Separator4          = AddMenuItem("-");
            MenuItemOptions     = AddMenuItem("Options...");
            MenuItemAbout       = AddMenuItem("About");
            MenuItemDebug       = AddMenuItem("Debug");
            Separator5          = AddMenuItem("-");
            MenuItemExit        = AddMenuItem("Exit");
        }

        public const int kMuteTrackMenuPosition = 0;
        public const int kSeperator1MenuPosition = 1;
        public const int kRepeatTrackMenuPosition = 2;
        public const int kShuffleTrackMenuPosition = 3;
        public const int kSeperator2MenuPosition = 4;
        public const int kDeleteTrackMenuPosition = 5;
        public const int kDeleteAllTrackMenuPosition = 6;
        public const int kSeperator3MenuPosition = 7;
        public const int kRoomSourceTrackMenuPosition = 8;
        public const int kOptionsMenuPosition = 10;
        public const int kAboutMenuPosition = 11;
        public const int kDebugMenuPosition = 12;
        public const int kExitMenuPosition = 14;

        public MenuItem MenuItemMute;
        public MenuItem Separator1;
        public MenuItem MenuItemRepeat;
        public MenuItem MenuItemShuffle;
        public MenuItem Separator2;
        public MenuItem MenuItemDeleteTrack;
        public MenuItem MenuItemDeleteAll;
        public MenuItem Separator3;
        public MenuItem MenuItemRoomSource;
        public MenuItem Separator4;
        public MenuItem MenuItemOptions;
        public MenuItem MenuItemAbout;
        public MenuItem MenuItemDebug;
        public MenuItem Separator5;
        public MenuItem MenuItemExit;
    }
}
