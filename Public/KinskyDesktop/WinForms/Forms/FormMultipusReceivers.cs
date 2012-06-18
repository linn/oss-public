using System;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

using Linn;
using Linn.Kinsky;
using Linn.Topology;

using Upnp;

namespace KinskyDesktop
{
    partial class FormReceivers : FormThemed, IViewWidgetReceivers
    {
        private const uint kRefreshTime = 20;

        private Ticker iTicker;
        private System.Threading.Timer iTimer;
        private bool iClosing;

        private Form iForm;

        private IViewSupport iViewSupport;

        private ReceiverSourceList iSourceList;
        private ModelSender iSender;

        public FormReceivers(FormKinskyDesktop aForm, ReceiverSourceList aSourceList, IViewSupport aViewSupport)
        {
            iTimer = new System.Threading.Timer(TimerElapsed);
            iTicker = new Ticker();

            iForm = aForm;

            iSourceList = aSourceList;
            iSourceList.EventReceiverSourceAdded += ReceiverSourceAdded;
            iSourceList.EventReceiverSourceRemoved += ReceiverSourceRemoved;
            iSourceList.EventReceiverSourceChanged += ReceiverSourceChanged;

            iViewSupport = aViewSupport;
            iViewSupport.EventSupportChanged += ViewSupportChanged;

            InitializeComponent();

            ViewSupportChanged(null, EventArgs.Empty);
        }

        public void Open()
        {
            if (!Visible)
            {
                iClosing = false;
                ShowDialog(iForm);
            }
        }

        void IViewWidgetReceivers.Close()
        {
            Hide();
        }

        public void SetSender(ModelSender aSender)
        {
            if (ListViewSelector.InvokeRequired)
            {
                ListViewSelector.BeginInvoke((MethodInvoker)delegate()
                {
                    DoSetSender(aSender);
                });
            }
            else
            {
                DoSetSender(aSender);
            }
        }

        private void DoSetSender(ModelSender aSender)
        {
            iSender = aSender;

            /*foreach (ListViewItem i in ListViewSelector.Items)
            {
                ModelSourceMultipus s = i.Tag as ModelSourceMultipus;
                i.Checked = IsPlayingSender(s);
            }*/
            DoSourceListChanged();
        }

        private void ViewSupportChanged(object sender, EventArgs e)
        {
            ListViewSelector.BackColor = iViewSupport.BackColour;
            ListViewSelector.ForeColor = iViewSupport.ForeColour;
            ListViewSelector.ForeColorBright = iViewSupport.ForeColourBright;
            ListViewSelector.ForeColorMuted = iViewSupport.ForeColourMuted;
            ListViewSelector.HighlightBackColour = iViewSupport.HighlightBackColour;
            ListViewSelector.HighlightForeColour = iViewSupport.HighlightForeColour;
            ListViewSelector.Font = iViewSupport.FontSmall;
        }

        private void ReceiverSourceAdded(object sender, EventArgsReceiverSource e)
        {
            if (ListViewSelector.InvokeRequired)
            {
                ListViewSelector.BeginInvoke((MethodInvoker)delegate()
                {
                    DoSourceListChanged();
                });
            }
            else
            {
                DoSourceListChanged();
            }
        }

        private void ReceiverSourceRemoved(object sender, EventArgsReceiverSource e)
        {
            if (ListViewSelector.InvokeRequired)
            {
                ListViewSelector.BeginInvoke((MethodInvoker)delegate()
                {
                    DoSourceListChanged();
                });
            }
            else
            {
                DoSourceListChanged();
            }
        }

        private void ReceiverSourceChanged(object sender, EventArgsReceiverSource e)
        {
            if (ListViewSelector.InvokeRequired)
            {
                ListViewSelector.BeginInvoke((MethodInvoker)delegate()
                {
                    DoSourceListChanged();
                });
            }
            else
            {
                DoSourceListChanged();
            }
        }

        private void DoSourceListChanged()
        {
            Trace.WriteLine(Trace.kKinskyDesktop, "SourceListChanged");

            IList<ModelSourceReceiver> sources = iSourceList.Sources;

            ListViewSelector.BeginUpdate();

            ListViewSelector.Items.Clear();

            foreach (ModelSourceReceiver s in sources)
            {
                if (iSender != null && s.Source.Device.Udn != iSender.Udn)
                {
                    Widgets.ListViewKinsky.Item item = new Widgets.ListViewKinsky.Item();
                    ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
                    subItem.Name = "Title";
                    subItem.Text = string.Format("{0}:{1}", s.Source.Room.Name, s.Source.FullName);
                    item.SubItems.Add(subItem);
                    //item.Icon = image;
                    //item.IconSelected = image;
                    item.Tag = s;
                    item.Checked = s.IsPlayingSender(iSender);

                    int index = 0;
                    for (int i = 0; i < ListViewSelector.Items.Count; ++i, ++index)
                    {
                        ListViewItem testItem = ListViewSelector.Items[i];
                        ModelSourceReceiver source = testItem.Tag as ModelSourceReceiver;
                        string name = string.Format("{0}:{1}", source.Source.Room.Name, source.Source.FullName);
                        if (name.CompareTo(subItem.Text) > 0)
                        {
                            break;
                        }
                    }

                    ListViewSelector.Items.Insert(index, item);
                }
            }

            ListViewSelector.EndUpdate();
        }

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
            Console.WriteLine(ListViewSelector.Items.Count + " " + ListViewSelector.SelectedIndices.Count);
            if (ListViewSelector.SelectedIndices.Count > 0)
            {
                ModelSourceReceiver s = ListViewSelector.Items[ListViewSelector.SelectedIndices[0]].Tag as ModelSourceReceiver;
                if (s.IsPlayingSender(iSender))
                {
                    s.Stop();
                }
                else
                {
                    s.PlayNow(iSender.Metadata);
                }
            }
        }

        private void FormSelector_Shown(object sender, EventArgs e)
        {
            iClosing = false;
            iTimer.Change(kRefreshTime, Timeout.Infinite);
            iTicker.Reset();
        }

        private void FormSelector_FormClosing(object sender, FormClosingEventArgs e)
        {
            iClosing = true;
            Opacity = 0;
        }
    }
}
