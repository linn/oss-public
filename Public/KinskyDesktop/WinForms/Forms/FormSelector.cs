using System;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Drawing;

using Linn;
using Linn.Kinsky;
using Linn.Topology;

namespace KinskyDesktop
{
    public partial class FormSelector : FormThemed
    {
        public class EventArgsRoomImageUpdated : EventArgs
        {
            public EventArgsRoomImageUpdated(Linn.Topology.Room aRoom, Image aImage)
            {
                Room = aRoom;
                Image = aImage;
            }

            public Linn.Topology.Room Room;
            public Image Image;
        }

        private const uint kRefreshTime = 20;

        private Ticker iTicker;
        private System.Threading.Timer iTimer;
        private bool iClosing;
        private ListViewItem iSelected;

        public FormSelector()
        {
            iTimer = new System.Threading.Timer(TimerElapsed);
            iTicker = new Ticker();

            InitializeComponent();
        }

        public ListViewItem Selected
        {
            get
            {
                return iSelected;
            }
        }

        public event EventHandler<EventArgsRoomImageUpdated> EventRoomImageUpdated;

        private void tableLayoutPanel1_MouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        private void tableLayoutPanel1_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        private void tableLayoutPanel1_MouseUp(object sender, MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        private void TimerElapsed(object sender)
        {
            if (!iClosing)
            {
                double o = iTicker.MilliSeconds * 0.01f;
                BeginInvoke((MethodInvoker)delegate()
                {
                    double op = Opacity + o;
                    if (op > 1)
                    {
                        op = 1;
                    }
                    Opacity = op;

                    if (Opacity < 1)
                    {
                        iTimer.Change(kRefreshTime, Timeout.Infinite);
                        iTicker.Reset();
                    }
                });
            }
        }

        private void ListViewSelector_ItemActivate(object sender, EventArgs e)
        {
            iClosing = true;
            Opacity = 0;
            DialogResult = DialogResult.OK;
            if (ListViewSelector.SelectedIndices.Count > 0)
            {
                int index = ListViewSelector.SelectedIndices[0];
                iSelected = ListViewSelector.Items[index];
            }
            else
            {
                iSelected = null;
            }
            Close();
        }

        private void FormSelector_Shown(object sender, EventArgs e)
        {
            iClosing = false;
            iTimer.Change(kRefreshTime, Timeout.Infinite);
            iTicker.Reset();

            ListViewSelector.UpdateGroupHeaderColour();
        }

        private void FormSelector_FormClosing(object sender, FormClosingEventArgs e)
        {
            iClosing = true;
            Opacity = 0;
        }

        private void ListViewSelector_DragOver(object sender, DragEventArgs e)
        {
            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                    foreach (string s in files)
                    {
                        try
                        {
                            Image i = Image.FromFile(s);
                            i.Dispose();
                            e.Effect = DragDropEffects.Copy;
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        private void ListViewSelector_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Effect == DragDropEffects.Copy)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                    foreach (string s in files)
                    {
                        try
                        {
                            Image image = Image.FromFile(s);
                            //File.Delete(Path.GetFileName(s));
                            image.Save(Path.GetFileName(s));
                            image.Dispose();

                            foreach (Widgets.ListViewKinsky.Item i in ListViewSelector.Items)
                            {
                                Linn.Topology.Room room = i.Tag as Linn.Topology.Room;
                                if (room.Name == Path.GetFileNameWithoutExtension(s))
                                {
                                    image = new Bitmap(File.Open(Path.GetFileName(s), FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                                    i.Icon = image;
                                    i.IconSelected = image;

                                    if (EventRoomImageUpdated != null)
                                    {
                                        EventRoomImageUpdated(this, new EventArgsRoomImageUpdated(room, image));
                                    }
                                }
                            }
                        }
                        catch (Exception) { }
                    }
                    ListViewSelector.Invalidate();
                }
            }
        }
    }
}
