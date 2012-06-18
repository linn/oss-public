using System.Windows.Forms;
using System;

using Linn.Kinsky;

namespace OssKinskyMppLastFm
{
    internal class ViewContextMenu
    {
        public ViewContextMenu()
        {
            Initialise();
        }

        public ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return iContextMenu;
            }
        }

        public void SetPlayEnabled(bool aValue)
        {
            if (iContextMenu.InvokeRequired)
            {
                iContextMenu.BeginInvoke((MethodInvoker)delegate()
                {
                    iContextMenuItemPlayNow.Enabled = aValue;
                    iContextMenuItemPlayNext.Enabled = aValue;
                    iContextMenuItemPlayLater.Enabled = aValue;
                });
            }
            else
            {
                iContextMenuItemPlayNow.Enabled = aValue;
                iContextMenuItemPlayNext.Enabled = aValue;
                iContextMenuItemPlayLater.Enabled = aValue;
            }
        }

        public EventHandler<EventArgs> EventPlayNowClicked;
        public EventHandler<EventArgs> EventPlayNextClicked;
        public EventHandler<EventArgs> EventPlayLaterClicked;

        private void Initialise()
        {
            iContextMenu = new ContextMenuStrip();
            iContextMenu.Name = "ContextMenuStripLibrary";
            iContextMenu.Size = new System.Drawing.Size(166, 176);

            ToolStripMenuItem toolStripMenuItem = null;


            // PlayNow
            toolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem.Enabled = false;
            toolStripMenuItem.Name = "ToolStripMenuItemPlayNow";
            toolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            toolStripMenuItem.Text = "Play Now";
            iContextMenuItemPlayNow = toolStripMenuItem;
            iContextMenu.Items.Add(toolStripMenuItem);

            iContextMenuItemPlayNow.Click += EventPlayNow_Click;
            
            // PlayNext
            toolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem.Enabled = false;
            toolStripMenuItem.Name = "ToolStripMenuItemPlayNext";
            toolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            toolStripMenuItem.Text = "Play Next";
            iContextMenuItemPlayNext = toolStripMenuItem;
            iContextMenu.Items.Add(toolStripMenuItem);

            iContextMenuItemPlayNext.Click += EventPlayNext_Click;

            // PlayLater
            toolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem.Enabled = false;
            toolStripMenuItem.Name = "ToolStripMenuItemPlayLater";
            toolStripMenuItem.Size = new System.Drawing.Size(165, 22);
            toolStripMenuItem.Text = "Play Later";
            iContextMenuItemPlayLater = toolStripMenuItem;
            iContextMenu.Items.Add(toolStripMenuItem);

            iContextMenuItemPlayLater.Click += EventPlayLater_Click;
        }

        private void EventPlayNow_Click(object sender, EventArgs e)
        {
            if (EventPlayNowClicked != null)
            {
                EventPlayNowClicked(this, EventArgs.Empty);
            }
        }

        private void EventPlayNext_Click(object sender, EventArgs e)
        {
            if (EventPlayNextClicked != null)
            {
                EventPlayNextClicked(this, EventArgs.Empty);
            }
        }

        private void EventPlayLater_Click(object sender, EventArgs e)
        {
            if (EventPlayLaterClicked != null)
            {
                EventPlayLaterClicked(this, EventArgs.Empty);
            }
        }

        private ContextMenuStrip iContextMenu;
        private ToolStripMenuItem iContextMenuItemPlayNow;
        private ToolStripMenuItem iContextMenuItemPlayNext;
        private ToolStripMenuItem iContextMenuItemPlayLater;
    }
} // OssKinskyMppLibrary