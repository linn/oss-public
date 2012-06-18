
using System;
using System.Windows.Forms;

using Linn;
using Linn.Kinsky;


namespace KinskyPda.Widgets
{
    internal class WidgetPlayMode : IViewWidgetPlayMode
    {
        public WidgetPlayMode(Control aParent, KinskyPda.Views.ContextMenuPlay aContextMenu)
        {
            iParent = aParent;
            iContextMenu = aContextMenu;
            iContextMenu.MenuItemRepeat.Click += MenuItemRepeatClick;
            iContextMenu.MenuItemShuffle.Click += MenuItemShuffleClick;
        }

        private delegate void DOpen();
        public void Open()
        {
            if (iParent.InvokeRequired)
            {
                iParent.BeginInvoke(new DOpen(Open));
            }
            else
            {
                iContextMenu.MenuItemShuffle.Enabled = true;
                iContextMenu.MenuItemRepeat.Enabled = true;
            }
        }

        private delegate void DClose();
        public void Close()
        {
            if (iParent.InvokeRequired)
            {
                iParent.BeginInvoke(new DClose(Close));
            }
            else
            {
                iContextMenu.MenuItemShuffle.Enabled = false;
                iContextMenu.MenuItemRepeat.Enabled = false;
            }
        }

        public void Initialised()
        {
        }

        private delegate void DSetShuffle(bool aShuffle);
        public void SetShuffle(bool aShuffle)
        {
            if (iParent.InvokeRequired)
            {
                iParent.BeginInvoke(new DSetShuffle(SetShuffle), new object[] { aShuffle });
            }
            else
            {
                iContextMenu.MenuItemShuffle.Checked = aShuffle;
            }
        }

        private delegate void DSetRepeat(bool aRepeat);
        public void SetRepeat(bool aRepeat)
        {
            if (iParent.InvokeRequired)
            {
                iParent.BeginInvoke(new DSetRepeat(SetRepeat), new object[] { aRepeat });
            }
            else
            {
                iContextMenu.MenuItemRepeat.Checked = aRepeat;
            }
        }

        public event EventHandler<EventArgs> EventToggleShuffle;
        public event EventHandler<EventArgs> EventToggleRepeat;

        private void MenuItemShuffleClick(object sender, System.EventArgs e)
        {
            if (EventToggleShuffle != null)
            {
                EventToggleShuffle(this, new EventArgs());
            }

            iContextMenu.MenuItemShuffle.Checked = !iContextMenu.MenuItemShuffle.Checked;
        }

        private void MenuItemRepeatClick(object sender, System.EventArgs e)
        {
            if (EventToggleRepeat != null)
            {
                EventToggleRepeat(this, new EventArgs());
            }

            iContextMenu.MenuItemRepeat.Checked = !iContextMenu.MenuItemRepeat.Checked;
        }

        private Control iParent;
        private KinskyPda.Views.ContextMenuPlay iContextMenu;
    }
}


