using System;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.IO;

using Upnp;

using Linn;
using Linn.ControlPoint.Upnp;
using Linn.Kinsky;
using Linn.Topology;

using KinskyDesktop.Widgets;

namespace KinskyDesktop
{
    class View : IView
    {
        public View(FormKinskyDesktop aForm, IArtworkCache aArtworkCache, IViewSupport aViewSupport, IPlaylistSupport aPlaySupport, IViewSaveSupport aSaveSupport, DropConverter aDropConverter, ReceiverSourceList aSourceList, ModelSenders aMultipusSenders)
        {
            ViewWidgetPlaylistMediaRenderer viewWidgetplaylist = new ViewWidgetPlaylistMediaRenderer(aForm, aArtworkCache, aDropConverter, aViewSupport, aPlaySupport, aSaveSupport);

            ViewWidgetPlaylistMaster viewWidgetPlaylistMaster = new ViewWidgetPlaylistMaster();
            viewWidgetPlaylistMaster.Add(viewWidgetplaylist);
            viewWidgetPlaylistMaster.Add(aForm.ViewStatus);

            ViewWidgetPlaylistRadio viewWidgetPlaylistRadio = new ViewWidgetPlaylistRadio(aForm, aArtworkCache, aViewSupport, aPlaySupport, aSaveSupport);

            ViewWidgetPlaylistRadioMaster viewWidgetPlaylistRadioMaster = new ViewWidgetPlaylistRadioMaster();
            viewWidgetPlaylistRadioMaster.Add(viewWidgetPlaylistRadio);
            viewWidgetPlaylistRadioMaster.Add(aForm.ViewStatus);

            ViewWidgetPlaylistReceiver viewWidgetPlaylistReceiver = new ViewWidgetPlaylistReceiver(aForm, aArtworkCache, aDropConverter, aViewSupport, aPlaySupport, aSaveSupport);

            ViewWidgetPlaylistReceiverMaster viewWidgetPlaylistReceiverMaster = new ViewWidgetPlaylistReceiverMaster();
            viewWidgetPlaylistReceiverMaster.Add(viewWidgetPlaylistReceiver);
            viewWidgetPlaylistReceiverMaster.Add(aForm.ViewStatus);

            ViewWidgetMediaTime viewWidgetMediaTime = new ViewWidgetMediaTime(aForm);

            ViewWidgetMediaTimeMaster viewWidgetMediaTimeMaster = new ViewWidgetMediaTimeMaster();
            viewWidgetMediaTimeMaster.Add(viewWidgetMediaTime);
            viewWidgetMediaTimeMaster.Add(aForm.ViewStatus);

            ViewSysTrayContextMenu sysTrayContextMenuPause = new ViewSysTrayContextMenuPause(aForm);
            ViewWidgetTransportControl viewWidgetTransportControlPause = new ViewWidgetTransportControlPause(aForm, aDropConverter, aPlaySupport);

            ViewWidgetTransportControlMaster viewWidgetTransportControlMasterPause = new ViewWidgetTransportControlMaster();
            viewWidgetTransportControlMasterPause.Add(viewWidgetTransportControlPause);
            viewWidgetTransportControlMasterPause.Add(sysTrayContextMenuPause);

            ViewSysTrayContextMenu sysTrayContextMenuStop = new ViewSysTrayContextMenuStop(aForm);
            ViewWidgetTransportControl viewWidgetTransportControlStop = new ViewWidgetTransportControlStop(aForm, aDropConverter, aPlaySupport);

            ViewWidgetTransportControlMaster viewWidgetTransportControlMasterStop = new ViewWidgetTransportControlMaster();
            viewWidgetTransportControlMasterStop.Add(viewWidgetTransportControlStop);
            viewWidgetTransportControlMasterStop.Add(sysTrayContextMenuStop);

            ViewSysTrayContextMenu sysTrayContextMenuStopNoSeek = new ViewSysTrayContextMenuStopNoSeek(aForm);
            ViewWidgetTransportControl viewWidgetTransportControlStopNoSeek = new ViewWidgetTransportControlStopNoSeek(aForm, aDropConverter, aPlaySupport);

            ViewWidgetTransportControlMaster viewWidgetTransportControlMasterStopNoSeek = new ViewWidgetTransportControlMaster();
            viewWidgetTransportControlMasterStopNoSeek.Add(viewWidgetTransportControlStopNoSeek);
            viewWidgetTransportControlMasterStopNoSeek.Add(sysTrayContextMenuStopNoSeek);

            ViewWidgetVolumeControl vewWidgetVolumeControl = new ViewWidgetVolumeControl(aForm);

            ViewWidgetVolumeControlMaster viewWidgetVolumeControlMaster = new ViewWidgetVolumeControlMaster();
            viewWidgetVolumeControlMaster.Add(vewWidgetVolumeControl);
            viewWidgetVolumeControlMaster.Add(sysTrayContextMenuPause);

            ViewWidgetPlayMode viewWidgetPlayMode = new ViewWidgetPlayMode(aForm);

            ViewWidgetPlayModeMaster viewWidgetPlayModeMaster = new ViewWidgetPlayModeMaster();
            viewWidgetPlayModeMaster.Add(viewWidgetPlayMode);
            viewWidgetPlayModeMaster.Add(sysTrayContextMenuPause);

            ViewWidgetTrack viewWidgetTrack = new ViewWidgetTrack(aForm, aViewSupport);

            ViewWidgetTrackMaster viewWidgetTrackMaster = new ViewWidgetTrackMaster();
            viewWidgetTrackMaster.Add(viewWidgetTrack);
            viewWidgetTrackMaster.Add(aForm.ViewStatus);

            iViewWidgetReceivers = new FormReceivers(aForm, aSourceList, aViewSupport);
            iViewWidgetSelectorRoom = new ViewWidgetSelectorRoom(aForm, aViewSupport);
            iViewWidgetButtonStandby = new ViewWidgetButtonStandby(aForm);
            iViewWidgetSelectorSource = new ViewWidgetSelectorSource(aForm, aViewSupport, iViewWidgetReceivers, aMultipusSenders);
            iViewWidgetVolumeControl = viewWidgetVolumeControlMaster;
            iViewWidgetMediaTime = viewWidgetMediaTimeMaster;
            iViewWidgetTransportControlPause = viewWidgetTransportControlMasterPause;
            iViewWidgetTransportControlStop = viewWidgetTransportControlMasterStop;
            iViewWidgetTrack = viewWidgetTrackMaster;
            iViewWidgetPlayMode = viewWidgetPlayModeMaster;
            iViewWidgetPlaylist = viewWidgetPlaylistMaster;
            iViewWidgetPlaylistRadio = viewWidgetPlaylistRadioMaster;
            iViewWidgetPlaylistReceiver = viewWidgetPlaylistReceiverMaster;
            iViewWidgetPlaylistAux = new ViewWidgetPlaylistAux(aForm, aViewSupport);
            iViewWidgetPlaylistDiscPlayer = new ViewWidgetPlaylistDiscPlayer(aForm, aViewSupport);
            iViewWidgetButtonSave = new ViewWidgetButtonSave(aForm, aDropConverter, aSaveSupport);
            iViewWidgetButtonWasteBin = new ViewWidgetButtonWasteBin(aForm, aDropConverter);
            iViewWidgetButtonReceivers = new ViewWidgetButtonReceivers(aForm);
        }

        public IViewWidgetSelector<Linn.Kinsky.Room> ViewWidgetSelectorRoom
        {
            get
            {
                return iViewWidgetSelectorRoom;
            }
        }

        public IViewWidgetButton ViewWidgetButtonStandby
        {
            get
            {
                return iViewWidgetButtonStandby;
            }
        }

        public IViewWidgetSelector<Linn.Kinsky.Source> ViewWidgetSelectorSource
        {
            get
            {
                return iViewWidgetSelectorSource;
            }
        }

        public IViewWidgetVolumeControl ViewWidgetVolumeControl
        {
            get
            {
                return iViewWidgetVolumeControl;
            }
        }

        public IViewWidgetMediaTime ViewWidgetMediaTime
        {
            get
            {
                return iViewWidgetMediaTime;
            }
        }

        public IViewWidgetTransportControl ViewWidgetTransportControlMediaRenderer
        {
            get
            {
                return iViewWidgetTransportControlPause;
            }
        }

        public IViewWidgetTransportControl ViewWidgetTransportControlDiscPlayer
        {
            get
            {
                return iViewWidgetTransportControlStop;
            }
        }

        public IViewWidgetTransportControl ViewWidgetTransportControlRadio
        {
            get
            {
                return iViewWidgetTransportControlStop;
            }
        }

        public IViewWidgetTrack ViewWidgetTrack
        {
            get
            {
                return iViewWidgetTrack;
            }
        }

        public IViewWidgetPlayMode ViewWidgetPlayMode
        {
            get
            {
                return iViewWidgetPlayMode;
            }
        }

        public IViewWidgetPlaylist ViewWidgetPlaylist
        {
            get
            {
                return iViewWidgetPlaylist;
            }
        }

        public IViewWidgetPlaylistRadio ViewWidgetPlaylistRadio
        {
            get
            {
                return iViewWidgetPlaylistRadio;
            }
        }

        public IViewWidgetPlaylistReceiver ViewWidgetPlaylistReceiver
        {
            get
            {
                return iViewWidgetPlaylistReceiver;
            }
        }

        public IViewWidgetPlaylistAux ViewWidgetPlaylistAux
        {
            get
            {
                return iViewWidgetPlaylistAux;
            }
        }

        public IViewWidgetPlaylistDiscPlayer ViewWidgetPlaylistDiscPlayer
        {
            get
            {
                return iViewWidgetPlaylistDiscPlayer;
            }
        }

        public IViewWidgetButton ViewWidgetButtonSave
        {
            get
            {
                return iViewWidgetButtonSave;
            }
        }

        public IViewWidgetButton ViewWidgetButtonWasteBin
        {
            get
            {
                return iViewWidgetButtonWasteBin;
            }
        }

        public IViewWidgetReceivers ViewWidgetReceivers
        {
            get
            {
                return iViewWidgetReceivers;
            }
        }

        public IViewWidgetButton ViewWidgetButtonReceivers
        {
            get
            {
                return iViewWidgetButtonReceivers;
            }
        }

        private IViewWidgetSelector<Linn.Kinsky.Room> iViewWidgetSelectorRoom;
        private IViewWidgetButton iViewWidgetButtonStandby;
        private IViewWidgetSelector<Linn.Kinsky.Source> iViewWidgetSelectorSource;
        private IViewWidgetVolumeControl iViewWidgetVolumeControl;
        private IViewWidgetMediaTime iViewWidgetMediaTime;
        private IViewWidgetTransportControl iViewWidgetTransportControlPause;
        private IViewWidgetTransportControl iViewWidgetTransportControlStop;
        private IViewWidgetTrack iViewWidgetTrack;
        private IViewWidgetPlayMode iViewWidgetPlayMode;
        private IViewWidgetPlaylist iViewWidgetPlaylist;
        private IViewWidgetPlaylistRadio iViewWidgetPlaylistRadio;
        private IViewWidgetPlaylistReceiver iViewWidgetPlaylistReceiver;
        private IViewWidgetPlaylistAux iViewWidgetPlaylistAux;
        private IViewWidgetPlaylistDiscPlayer iViewWidgetPlaylistDiscPlayer;
        private IViewWidgetButton iViewWidgetButtonSave;
        private IViewWidgetButton iViewWidgetButtonWasteBin;
        private IViewWidgetReceivers iViewWidgetReceivers;
        private IViewWidgetButton iViewWidgetButtonReceivers;
    }

    class ViewWidgetSelectorRoom : IViewWidgetSelector<Linn.Kinsky.Room>
    {
        public ViewWidgetSelectorRoom(FormKinskyDesktop aForm, IViewSupport aViewSupport)
        {
            iMutex = new Mutex(false);
            iOpen = false;

            iForm = aForm;
            iViewSupport = aViewSupport;
            iViewSupport.EventSupportChanged += SupportChanged;

            iFormSelector = new FormSelector();
            iFormSelector.Text = "Rooms";
            iFormSelector.ListViewSelector.BackColor = iViewSupport.BackColour;
            iFormSelector.ListViewSelector.ForeColor = iViewSupport.ForeColour;
            iFormSelector.ListViewSelector.ForeColorBright = iViewSupport.ForeColourBright;
            iFormSelector.ListViewSelector.ForeColorMuted = iViewSupport.ForeColourMuted;
            iFormSelector.ListViewSelector.HighlightBackColour = iViewSupport.HighlightBackColour;
            iFormSelector.ListViewSelector.HighlightForeColour = iViewSupport.HighlightForeColour;
            iFormSelector.ListViewSelector.Font = iViewSupport.FontSmall;
            iFormSelector.ListViewSelector.LargeIconSize = new Size(64, 64);
            iFormSelector.ListViewSelector.AllowDrop = true;

            iButtonSelector = aForm.ButtonRoomSelector;

            iButtonSelector.MouseEnter += MouseEnter;
            iButtonSelector.MouseLeave += MouseLeave;
        }

        public void Open()
        {
            iMutex.WaitOne();

            Assert.Check(!iOpen);

            iFormSelector.EventRoomImageUpdated += RoomImageUpdated;
            iButtonSelector.Click += Click;

            iOpen = true;

            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iButtonSelector.Text = "Select a room";
            });

            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            if (iOpen)
            {
                iFormSelector.EventRoomImageUpdated -= RoomImageUpdated;
                iButtonSelector.Click -= Click;
            }

            iOpen = false;

            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iFormSelector.ListViewSelector.Items.Clear();
            });

            iMutex.ReleaseMutex();
        }


        public void InsertItem(int aIndex, Linn.Kinsky.Room aItem)
        {
            Widgets.ListViewKinsky.Item item = new Widgets.ListViewKinsky.Item();
            ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
            subItem.Name = "Title";
            subItem.Text = aItem.Name;
            item.SubItems.Add(subItem);
            item.Icon = kIconRoom;
            item.IconSelected = kIconRoom;
            item.Tag = aItem;


            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                Trace.WriteLine(Trace.kKinskyDesktop, "RoomAdded: " + aItem.Name);

                iMutex.WaitOne();
                if (iOpen)
                {
                    iMutex.ReleaseMutex();
                    //iFormSelector.ListViewSelector.BeginUpdate();
                    iFormSelector.ListViewSelector.Items.Insert(aIndex, item);
                    //iFormSelector.ListViewSelector.EndUpdate();
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            });
        }

        public void RemoveItem(Linn.Kinsky.Room aItem)
        {
            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                Trace.WriteLine(Trace.kKinskyDesktop, "RoomRemoved: " + aItem.Name);

                foreach (ListViewItem i in iFormSelector.ListViewSelector.Items)
                {
                    if (i.Tag == aItem)
                    {
                        iFormSelector.ListViewSelector.BeginUpdate();
                        iFormSelector.ListViewSelector.Items.Remove(i);
                        iFormSelector.ListViewSelector.EndUpdate();
                        break;
                    }
                }

                if (aItem == iButtonSelector.Tag)
                {
                    SetSelected(null);
                }
            });
        }

        public void ItemChanged(Linn.Kinsky.Room aItem)
        {
            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                Trace.WriteLine(Trace.kKinskyDesktop, "RoomChanged: " + aItem.Name);
                foreach (ListViewItem i in iFormSelector.ListViewSelector.Items)
                {
                    if (i.Tag == aItem)
                    {
                        i.SubItems[0].Text = aItem.Name;
                        break;
                    }
                }
            });
        }

        public void SetSelected(Linn.Kinsky.Room aItem)
        {
            iButtonSelector.BeginInvoke((MethodInvoker)delegate()
            {
                iButtonSelector.Image = kIconRoom;
                if (aItem == null)
                {
                    iButtonSelector.Text = "Select a room";
                }
                else
                {
                    iButtonSelector.Text = aItem.Name;
                }
                iButtonSelector.Tag = aItem;
                iButtonSelector.Invalidate();
            });

        }

        public event EventHandler<EventArgsSelection<Linn.Kinsky.Room>> EventSelectionChanged;

        private void RoomImageUpdated(object sender, FormSelector.EventArgsRoomImageUpdated e)
        {
            if (e.Room == iButtonSelector.Tag)
            {
                iButtonSelector.Image = e.Image;
                iButtonSelector.Invalidate();
            }
        }

        private void MouseEnter(object sender, EventArgs e)
        {
            DoubleBufferedButton button = sender as DoubleBufferedButton;
            button.ForeColor = iViewSupport.ForeColourBright;
        }

        private void MouseLeave(object sender, EventArgs e)
        {
            DoubleBufferedButton button = sender as DoubleBufferedButton;
            button.ForeColor = iViewSupport.ForeColourMuted;
        }

        private void Click(object sender, EventArgs e)
        {
            if (iFormSelector.ShowDialog(iForm) == DialogResult.OK)
            {
                if (iFormSelector.Selected != null)
                {
                    Widgets.ListViewKinsky.Item item = iFormSelector.Selected as Widgets.ListViewKinsky.Item;
                    SetSelected(item.Tag as Linn.Kinsky.Room);
                    if (EventSelectionChanged != null)
                    {
                        EventSelectionChanged(this, new EventArgsSelection<Linn.Kinsky.Room>(item.Tag as Linn.Kinsky.Room));
                    }
                }
            }
        }

        private void SupportChanged(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            iFormSelector.ListViewSelector.BackColor = iViewSupport.BackColour;
            iFormSelector.ListViewSelector.ForeColor = iViewSupport.ForeColour;
            iFormSelector.ListViewSelector.ForeColorBright = iViewSupport.ForeColourBright;
            iFormSelector.ListViewSelector.ForeColorMuted = iViewSupport.ForeColourMuted;
            iFormSelector.ListViewSelector.HighlightBackColour = iViewSupport.HighlightBackColour;
            iFormSelector.ListViewSelector.HighlightForeColour = iViewSupport.HighlightForeColour;
            iFormSelector.ListViewSelector.Font = iViewSupport.FontSmall;

            iMutex.ReleaseMutex();
        }

        private readonly Image kIconRoom = Properties.Resources.IconRoom;

        private bool iOpen;
        private Mutex iMutex;

        private Form iForm;
        private IViewSupport iViewSupport;
        private FormSelector iFormSelector;
        private DoubleBufferedButton iButtonSelector;
       

    }

    abstract class ViewWidgetButton : IViewWidgetButton
    {
        protected ViewWidgetButton(Widgets.ButtonControl aButton, ToolTip aToolTip, string aToolTipText)
        {
            iOpen = false;
            iToolTip = aToolTip;
            iToolTipText = aToolTipText;
            iButton = aButton;
        }

        private delegate void DOpen();
        public void Open()
        {
            if (iButton.InvokeRequired)
            {
                iButton.BeginInvoke(new DOpen(Open));
            }
            else
            {
                Assert.Check(!iOpen);

                iButton.EventClick += EventButton_Click;
                SetEnabled(true);
                iToolTip.SetToolTip(iButton, iToolTipText);

                OnOpen();

                iOpen = true;
            }
        }

        private delegate void DClose();
        public void Close()
        {
            if (iButton.InvokeRequired)
            {
                iButton.BeginInvoke(new DClose(Close));
            }
            else
            {
                if (iOpen)
                {
                    iButton.EventClick -= EventButton_Click;
                    SetEnabled(false);
                    iToolTip.SetToolTip(iButton, "");

                    OnClose();

                    iOpen = false;
                }
            }
        }

        private delegate void SetEnabledDelegate(bool aEnabled);
        private void SetEnabled(bool aEnabled)
        {
            if (iButton.InvokeRequired)
            {
                iButton.BeginInvoke(new SetEnabledDelegate(SetEnabled), new object[] { aEnabled });
            }
            else
            {
                iButton.Enabled = aEnabled;
            }
        }

        public event EventHandler<EventArgs> EventClick;

        protected virtual void OnOpen()
        {
        }

        protected virtual void OnClose()
        {
        }

        private void EventButton_Click(object sender, EventArgs e)
        {
            if (EventClick != null)
            {
                EventClick(this, EventArgs.Empty);
            }
        }

        private bool iOpen;
        private string iToolTipText;
        private ToolTip iToolTip;
        protected Widgets.ButtonControl iButton;
    }

    class ViewWidgetButtonStandby : ViewWidgetButton
    {
        public ViewWidgetButtonStandby(FormKinskyDesktop aForm)
            : base(aForm.ButtonStandby, aForm.ToolTip, "Put room into standby")
        {
        }
    }

    class ViewWidgetButtonReceivers : ViewWidgetButton
    {
        public ViewWidgetButtonReceivers(FormKinskyDesktop aForm)
            : base(aForm.ButtonReceivers, aForm.ToolTip, "Show receivers")
        {
        }
    }

    class ViewWidgetSelectorSource : IViewWidgetSelector<Linn.Kinsky.Source>
    {
        public ViewWidgetSelectorSource(FormKinskyDesktop aForm, IViewSupport aViewSupport, IViewWidgetReceivers aViewWidgetReceivers, ModelSenders aSenders)
        {
            iMutex = new Mutex(false);
            iOpen = false;

            iForm = aForm;
            iViewSupport = aViewSupport;
            iViewSupport.EventSupportChanged += SupportChanged;

            iFormSelector = new FormSelector();
            iFormSelector.ListViewSelector.ShowGroups = true;
            iFormSelector.Text = "Sources";
            iFormSelector.ListViewSelector.BackColor = iViewSupport.BackColour;
            iFormSelector.ListViewSelector.ForeColor = iViewSupport.ForeColour;
            iFormSelector.ListViewSelector.ForeColorBright = iViewSupport.ForeColourBright;
            iFormSelector.ListViewSelector.ForeColorMuted = iViewSupport.ForeColourMuted;
            iFormSelector.ListViewSelector.HighlightBackColour = iViewSupport.HighlightBackColour;
            iFormSelector.ListViewSelector.HighlightForeColour = iViewSupport.HighlightForeColour;
            iFormSelector.ListViewSelector.Font = iViewSupport.FontSmall;
            iFormSelector.ListViewSelector.LargeIconSize = new Size(64, 64);

            iButtonSelector = aForm.ButtonSourceSelector;

            iButtonSelector.MouseEnter += MouseEnter;
            iButtonSelector.MouseLeave += MouseLeave;
            iViewWidgetReceivers = aViewWidgetReceivers;
            iSenders = aSenders;
        }

        public void Open()
        {
            iMutex.WaitOne();

            Assert.Check(!iOpen);

            iButtonSelector.Click += Click;

            iOpen = true;

            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iButtonSelector.Text = "Select a source";
            });

            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            if (iOpen)
            {
                iButtonSelector.Click -= Click;
            }

            iOpen = false;

            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iFormSelector.ListViewSelector.Items.Clear();
            });


            iMutex.ReleaseMutex();
        }

        public void ItemChanged(Linn.Kinsky.Source aItem)
        {
            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iMutex.WaitOne();
                if (iOpen)
                {
                    Trace.WriteLine(Trace.kKinskyDesktop, "SourceChanged: " + aItem.Name);
                    iMutex.ReleaseMutex();
                        
                    foreach (ListViewItem i in iFormSelector.ListViewSelector.Items)
                    {
                        if (i.Tag == aItem)
                        {
                            i.SubItems[1].Text = aItem.Name;
                            break;
                        }
                    }
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            });

            iButtonSelector.BeginInvoke((MethodInvoker)delegate()
            {
                if (aItem == iButtonSelector.Tag)
                {
                    iButtonSelector.Text = aItem.Name;
                    iButtonSelector.Invalidate();
                }
            });
        }

        public void InsertItem(int aIndex, Linn.Kinsky.Source aItem)
        {
            Image image = SourceImage(aItem);

            Widgets.ListViewKinsky.Item item = new Widgets.ListViewKinsky.Item();
            ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
            subItem.Name = "Title";
            subItem.Text = aItem.Name;
            item.SubItems.Add(subItem);
            item.Icon = image;
            item.IconSelected = image;
            item.Tag = aItem;

            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                
                Trace.WriteLine(Trace.kKinskyDesktop, "SourceAdded: " + aItem.Name);

                iMutex.WaitOne();
                if (iOpen)
                {
                    iMutex.ReleaseMutex();

                    int index = aIndex;                   

                    iFormSelector.ListViewSelector.BeginUpdate();
                    iFormSelector.ListViewSelector.Items.Insert(index, item);
                    iFormSelector.ListViewSelector.EndUpdate();
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            });
        }
        

        public void RemoveItem(Linn.Kinsky.Source aItem)
        {
            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                Trace.WriteLine(Trace.kKinskyDesktop, "SourceRemoved: " + aItem.Name);

                foreach (ListViewItem i in iFormSelector.ListViewSelector.Items)
                {
                    if (i.Tag == aItem)
                    {
                        iFormSelector.ListViewSelector.BeginUpdate();
                        iFormSelector.ListViewSelector.Items.Remove(i);
                        iFormSelector.ListViewSelector.EndUpdate();
                        break;
                    }
                }

                if (aItem == iButtonSelector.Tag)
                {
                    SetSelected(null);
                }
            });
        }

        public void SetSelected(Linn.Kinsky.Source aItem)
        {
            iButtonSelector.BeginInvoke((MethodInvoker)delegate()
             {
                 iButtonSelector.Image = SourceImage(aItem);
                 if (aItem == null)
                 {
                     iButtonSelector.Text = "Select a source";
                 }
                 else
                 {
                     iButtonSelector.Text = aItem.Name;
                 }
                 iButtonSelector.Tag = aItem;
                 iButtonSelector.Invalidate();
             });

            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                for (int i = 0; i < iFormSelector.ListViewSelector.Items.Count; ++i)
                {
                    ListViewItem testItem = iFormSelector.ListViewSelector.Items[i];
                        testItem.Selected = testItem.Tag == aItem;
                } 
                //iFormSelector.ListViewSelector.Refresh();
            });

        }

        public event EventHandler<EventArgsSelection<Linn.Kinsky.Source>> EventSelectionChanged;

        private Image SourceImage(Linn.Kinsky.Source aSource)
        {
            if (aSource == null)
                return null;

            switch (aSource.Type)
            {
                case Linn.Kinsky.Source.kSourceAux:
                    return kIconAux;
                case Linn.Kinsky.Source.kSourceAnalog:
                    return kIconAux;
                case Linn.Kinsky.Source.kSourceDisc:
                    return kIconDisc;
                case Linn.Kinsky.Source.kSourceDs:
                    return kIconPlaylist;
                case Linn.Kinsky.Source.kSourceRadio:
                    return kIconRadio;
                case Linn.Kinsky.Source.kSourceSpdif:
                    return kIconSpdif;
                case Linn.Kinsky.Source.kSourceToslink:
                    return kIconToslink;
                case Linn.Kinsky.Source.kSourceTuner:
                    return kIconRadio;
                case Linn.Kinsky.Source.kSourceUpnpAv:
                    return kIconUpnpAv;
                case Linn.Kinsky.Source.kSourceReceiver:
                    return kIconMultipus;
                default:
                    return kIconAux;
            }
        }

        private void MouseEnter(object sender, EventArgs e)
        {
            DoubleBufferedButton button = sender as DoubleBufferedButton;
            button.ForeColor = iViewSupport.ForeColourBright;
        }

        private void MouseLeave(object sender, EventArgs e)
        {
            DoubleBufferedButton button = sender as DoubleBufferedButton;
            button.ForeColor = iViewSupport.ForeColourMuted;
        }

        private void Click(object sender, EventArgs e)
        {
            if (iFormSelector.ShowDialog(iForm) == DialogResult.OK)
            {
                if (iFormSelector.Selected != null)
                {
                    Widgets.ListViewKinsky.Item item = iFormSelector.Selected as Widgets.ListViewKinsky.Item;

                    if (EventSelectionChanged != null)
                    {
                        EventSelectionChanged(this, new EventArgsSelection<Linn.Kinsky.Source>(item.Tag as Linn.Kinsky.Source));
                    }
                }
            }
        }

        private void SupportChanged(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            iFormSelector.ListViewSelector.BackColor = iViewSupport.BackColour;
            iFormSelector.ListViewSelector.ForeColor = iViewSupport.ForeColour;
            iFormSelector.ListViewSelector.ForeColorBright = iViewSupport.ForeColourBright;
            iFormSelector.ListViewSelector.ForeColorMuted = iViewSupport.ForeColourMuted;
            iFormSelector.ListViewSelector.HighlightBackColour = iViewSupport.HighlightBackColour;
            iFormSelector.ListViewSelector.HighlightForeColour = iViewSupport.HighlightForeColour;
            iFormSelector.ListViewSelector.Font = iViewSupport.FontSmall;
            iFormSelector.ListViewSelector.UpdateGroupHeaderColour();

            iMutex.ReleaseMutex();
        }

        private readonly Image kIconAux = Properties.Resources.IconAuxSource;
        private readonly Image kIconDisc = Properties.Resources.IconDiscSource;
        private readonly Image kIconPlaylist = Properties.Resources.IconPlaylistSource;
        private readonly Image kIconRadio = Properties.Resources.IconRadioSource;
        private readonly Image kIconSpdif = Properties.Resources.IconSpdifSource;
        private readonly Image kIconToslink = Properties.Resources.IconTosLinkSource;
        private readonly Image kIconUpnpAv = Properties.Resources.IconUpnpAvSource;
        private readonly Image kIconMultipus = Properties.Resources.IconRoom;

        private bool iOpen;
        private Mutex iMutex;

        private Form iForm;
        private IViewSupport iViewSupport;
        private FormSelector iFormSelector;
        private DoubleBufferedButton iButtonSelector;
        private IViewWidgetReceivers iViewWidgetReceivers;
        private ModelSenders iSenders;

    }

    class ViewWidgetVolumeControl : IViewWidgetVolumeControl
    {
        public ViewWidgetVolumeControl(FormKinskyDesktop aForm)
        {
            iMutex = new Mutex(false);
            iOpen = false;

            iToolTip = aForm.ToolTip;
            iRotaryControlVolume = aForm.RotaryControlVolume;
            iRockerControlVolume = aForm.RockerControlVolume;
        }

        public void Open()
        {
            iMutex.WaitOne();

            Assert.Check(!iOpen);

            iRotaryControlVolume.EventClick += EventClick;
            iRotaryControlVolume.EventClockwiseStep += EventVolumeUp;
            iRotaryControlVolume.EventAntiClockwiseStep += EventVolumeDown;
            iRotaryControlVolume.Paint += EventPaint;

            iRockerControlVolume.EventClickMiddle += EventClick;
            iRockerControlVolume.EventClickLeft += EventVolumeDown;
            iRockerControlVolume.EventClickRight += EventVolumeUp;
            iRockerControlVolume.Paint += EventPaint;

            iOpen = true;

            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            if (iOpen)
            {
                iRotaryControlVolume.EventClick -= EventClick;
                iRotaryControlVolume.EventClockwiseStep -= EventVolumeUp;
                iRotaryControlVolume.EventAntiClockwiseStep -= EventVolumeDown;
                iRotaryControlVolume.Paint -= EventPaint;

                iRockerControlVolume.EventClickMiddle -= EventClick;
                iRockerControlVolume.EventClickLeft -= EventVolumeDown;
                iRockerControlVolume.EventClickRight -= EventVolumeUp;
                iRockerControlVolume.Paint -= EventPaint;
            }

            iOpen = false;

            iRotaryControlVolume.BeginInvoke((MethodInvoker)delegate()
            {
                iRotaryControlVolume.Enabled = false;
                iRotaryControlVolume.Text = "";
                iRotaryControlVolume.Value = 0;

                iToolTip.SetToolTip(iRotaryControlVolume, "");

                iRockerControlVolume.Enabled = false;
                iRockerControlVolume.Text = "";
                iRockerControlVolume.Value = 0;

                iToolTip.SetToolTip(iRockerControlVolume, "");
            });

            iMutex.ReleaseMutex();
        }

        public void Initialised()
        {
            iRotaryControlVolume.BeginInvoke((MethodInvoker)delegate()
            {
                iRotaryControlVolume.Enabled = true;
                iToolTip.SetToolTip(iRotaryControlVolume, "Volume control (click center to toggle mute, rotate ring to adjust volume)");

                iRockerControlVolume.Enabled = true;
                iToolTip.SetToolTip(iRockerControlVolume, "Volume control (click center to toggle mute, click left or right to adjust volume)");
            });
        }

        public void SetVolume(uint aVolume)
        {
            iRotaryControlVolume.BeginInvoke((MethodInvoker)delegate()
            {
                iMutex.WaitOne();
                if (iOpen)
                {
                    iMutex.ReleaseMutex();

                    iRotaryControlVolume.Text = aVolume.ToString();
                    iRotaryControlVolume.Value = aVolume;

                    iRockerControlVolume.Text = aVolume.ToString();
                    iRockerControlVolume.Value = aVolume;
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            });
        }

        public void SetMute(bool aMute)
        {
            iMutex.WaitOne();
            iMute = aMute;
            iMutex.ReleaseMutex();

            iRotaryControlVolume.Invalidate();
            iRockerControlVolume.Invalidate();
        }

        public void SetVolumeLimit(uint aVolumeLimit)
        {
            iRotaryControlVolume.BeginInvoke((MethodInvoker)delegate()
            {
                iMutex.WaitOne();
                if (iOpen)
                {
                    iMutex.ReleaseMutex();

                    iRotaryControlVolume.MaxValue = aVolumeLimit;
                    iRockerControlVolume.MaxValue = aVolumeLimit;
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            });
        }

        public event EventHandler<EventArgs> EventVolumeIncrement;
        public event EventHandler<EventArgs> EventVolumeDecrement;
        public event EventHandler<EventArgsVolume> EventVolumeChanged { add { } remove { } }
        public event EventHandler<EventArgsMute> EventMuteChanged;

        private void EventClick(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            if (iOpen)
            {
                iMutex.ReleaseMutex();
                if (EventMuteChanged != null)
                {
                    EventMuteChanged(this, new EventArgsMute(!iMute));
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventVolumeUp(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            if (iOpen)
            {
                iMutex.ReleaseMutex();
                if (EventVolumeIncrement != null)
                {
                    EventVolumeIncrement(this, EventArgs.Empty);
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventVolumeDown(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            if (iOpen)
            {
                iMutex.ReleaseMutex();
                if (EventVolumeDecrement != null)
                {
                    EventVolumeDecrement(this, EventArgs.Empty);
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventPaint(object sender, PaintEventArgs e)
        {
            Widgets.Kontrol kontrol = sender as Widgets.Kontrol;
            Assert.Check(kontrol != null);

            iMutex.WaitOne();
            if (iOpen)
            {
                bool mute = iMute;
                iMutex.ReleaseMutex();
                if (mute)
                {
                    Point location = new Point((int)(((kontrol.Width - kImageMuteOn.Width) * 0.5f)),
                                               (int)(((kontrol.Height - kImageMuteOn.Height) * 0.5f)));
                    e.Graphics.DrawImage(kImageMuteOn, location);
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private static Image kImageMuteOn = KinskyDesktop.Properties.Resources.WheelMute;

        private bool iOpen;
        private Mutex iMutex;
        private bool iMute;

        private ToolTip iToolTip;
        private Widgets.RotaryControl iRotaryControlVolume;
        private Widgets.RockerControl iRockerControlVolume;
    }

    class ViewWidgetMediaTime : IViewWidgetMediaTime
    {
        public ViewWidgetMediaTime(FormKinskyDesktop aForm)
        {
            iMutex = new Mutex(false);
            iDuration = new Time(0);

            iTicker = new Ticker();
            iTimer = new System.Threading.Timer(TimerElapsed);
            iTimer.Change(Timeout.Infinite, Timeout.Infinite);

            iToolTip = aForm.ToolTip;
            iRotaryControlTracker = aForm.RotaryControlTracker;
            iRockerControlTracker = aForm.RockerControlTracker;
        }

        public void Open()
        {
            iMutex.WaitOne();

            Assert.Check(!iOpen);

            iRotaryControlTracker.Paint += EventPaint;
            iRotaryControlTracker.EventClick += EventClick;
            iRotaryControlTracker.EventStartRotation += EventStartSeeking;
            iRotaryControlTracker.EventEndRotation += EventEndSeeking;
            iRotaryControlTracker.EventCancelRotation += EventCancelSeeking;
            iRotaryControlTracker.EventClockwiseStep += EventSeekForwards;
            iRotaryControlTracker.EventAntiClockwiseStep += EventSeekBackwards;

            iRockerControlTracker.Paint += EventPaint;
            iRockerControlTracker.EventClickMiddle += EventClick;
            iRockerControlTracker.EventClickLeftStarted += EventStartSeeking;
            iRockerControlTracker.EventClickRightStarted += EventStartSeeking;
            iRockerControlTracker.EventClickLeftFinished += EventEndSeeking;
            iRockerControlTracker.EventClickRightFinished += EventEndSeeking;
            iRockerControlTracker.EventClickCancelled += EventCancelSeeking;
            iRockerControlTracker.EventClickLeft += EventSeekBackwards;
            iRockerControlTracker.EventClickRight += EventSeekForwards;

            //iStopped = true;
            //iBuffering = false;
            iOpen = true;

            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            if (iOpen)
            {
                iRotaryControlTracker.Paint -= EventPaint;
                iRotaryControlTracker.EventClick -= EventClick;
                iRotaryControlTracker.EventStartRotation -= EventStartSeeking;
                iRotaryControlTracker.EventEndRotation -= EventEndSeeking;
                iRotaryControlTracker.EventCancelRotation -= EventCancelSeeking;
                iRotaryControlTracker.EventClockwiseStep -= EventSeekForwards;
                iRotaryControlTracker.EventAntiClockwiseStep -= EventSeekBackwards;

                iRockerControlTracker.Paint -= EventPaint;
                iRockerControlTracker.EventClickMiddle -= EventClick;
                iRockerControlTracker.EventClickLeftStarted -= EventStartSeeking;
                iRockerControlTracker.EventClickRightStarted -= EventStartSeeking;
                iRockerControlTracker.EventClickLeftFinished -= EventEndSeeking;
                iRockerControlTracker.EventClickRightFinished -= EventEndSeeking;
                iRockerControlTracker.EventClickCancelled -= EventCancelSeeking;
                iRockerControlTracker.EventClickLeft -= EventSeekBackwards;
                iRockerControlTracker.EventClickRight -= EventSeekForwards;
            }

            iOpen = false;

            iMutex.ReleaseMutex();

            SetDuration(0);

            iRotaryControlTracker.BeginInvoke((MethodInvoker)delegate()
            {
                iRotaryControlTracker.Enabled = false;
                iRotaryControlTracker.Value = 0;
                iRotaryControlTracker.Text = "";

                iToolTip.SetToolTip(iRotaryControlTracker, "");

                iRockerControlTracker.Enabled = false;
                iRockerControlTracker.Value = 0;
                iRockerControlTracker.Text = "";

                iToolTip.SetToolTip(iRockerControlTracker, "");
            });
        }

        public void Initialised()
        {
            iRotaryControlTracker.BeginInvoke((MethodInvoker)delegate()
            {
                //iRotaryControlTracker.Enabled = true;
                iToolTip.SetToolTip(iRotaryControlTracker, "Track control (Click center to toggle time display, rotate ring to seek within track)");

                //iRockerControlTracker.Enabled = true;
                iToolTip.SetToolTip(iRockerControlTracker, "Track control (Click center to toggle time display, click left and right to seek within track)");
            });

            iMutex.WaitOne();
            UpdateSeconds(iSeconds);
            iMutex.ReleaseMutex();
        }

        public void SetAllowSeeking(bool aAllowSeeking)
        {
            iRotaryControlTracker.BeginInvoke((MethodInvoker)delegate()
            {
                iRotaryControlTracker.OuterRingEnabled = aAllowSeeking;
                iRockerControlTracker.OuterRingEnabled = aAllowSeeking;
            });
        }

        public void SetTransportState(ETransportState aTransportState)
        {
            iMutex.WaitOne();

            SetBuffering(aTransportState == ETransportState.eBuffering);
            SetStopped(aTransportState == ETransportState.eStopped);
            iTransportState = aTransportState;

            UpdateSeconds(iSeconds);

            iMutex.ReleaseMutex();

            iRockerControlTracker.Invalidate();
            iRotaryControlTracker.Invalidate();
        }

        public void SetDuration(uint aDuration)
        {
            iRotaryControlTracker.BeginInvoke((MethodInvoker)delegate()
            {
                EventCancelSeeking(null, EventArgs.Empty);
            });

            iMutex.WaitOne();

            iDuration = new Time((int)aDuration);
            float seek = iDuration.SecondsTotal / 100.0f;
            iSeekAmountPerStep = (uint)Math.Round(seek + 0.5f, MidpointRounding.AwayFromZero);

            iMutex.ReleaseMutex();

            iRotaryControlTracker.BeginInvoke((MethodInvoker)delegate()
            {
                iRotaryControlTracker.MaxValue = aDuration;
                iRockerControlTracker.MaxValue = aDuration;

                iRotaryControlTracker.Enabled = (aDuration != 0);
                iRockerControlTracker.Enabled = (aDuration != 0);
            });
        }

        public void SetSeconds(uint aSeconds)
        {
            iMutex.WaitOne();
            iSeconds = aSeconds;
            iMutex.ReleaseMutex();

            UpdateSeconds(aSeconds);
        }

        public event EventHandler<EventArgsSeekSeconds> EventSeekSeconds;

        private void SetBuffering(bool aBuffering)
        {
            if (aBuffering)
            {
                iAngleRemainder = 0.0f;
                iAngle = 0.0f;
                iTicker.Reset();
                iTimer.Change(kUpdateRate, kUpdateRate);
            }
            else
            {
                iTimer.Change(Timeout.Infinite, Timeout.Infinite);
            }

            iMutex.WaitOne();
            iBuffering = aBuffering;
            iMutex.ReleaseMutex();

            iRotaryControlTracker.BeginInvoke((MethodInvoker)delegate()
            {
                if (aBuffering)
                {
                    iRotaryControlTracker.Value = 0;
                    iRotaryControlTracker.Text = string.Empty;

                    iRockerControlTracker.Value = 0;
                    iRockerControlTracker.Text = string.Empty;
                }
            });
        }

        private void SetStopped(bool aStopped)
        {
            iMutex.WaitOne();
            iStopped = aStopped;
            iMutex.ReleaseMutex();

            iRotaryControlTracker.BeginInvoke((MethodInvoker)delegate()
            {
                if (aStopped)
                {
                    iRotaryControlTracker.Value = 0;
                    iRotaryControlTracker.Text = string.Empty;

                    iRockerControlTracker.Value = 0;
                    iRockerControlTracker.Text = string.Empty;
                }
            });
        }

        private void UpdateSeconds(uint aSeconds)
        {
            iRotaryControlTracker.BeginInvoke((MethodInvoker)delegate()
            {
                iMutex.WaitOne();
                if (iOpen)
                {
                    if (!iBuffering && !iStopped)
                    {
                        Time duration = new Time(iDuration.SecondsTotal);
                        iMutex.ReleaseMutex();

                        iRotaryControlTracker.Value = aSeconds;
                        iRockerControlTracker.Value = aSeconds;

                        if (!iApplyTargetSeconds)
                        {
                            string t = string.Empty;
                            if (iShowTimeRemaining && duration.SecondsTotal > 0)
                            {
                                Time time = new Time((int)aSeconds - duration.SecondsTotal);
                                t = FormatTime(time, duration);
                            }
                            else
                            {
                                Time time = new Time((int)aSeconds);
                                t = FormatTime(time, duration);
                            }
                            iRotaryControlTracker.Text = t;
                            iRockerControlTracker.Text = t;
                        }
                    }
                    else
                    {
                        iMutex.ReleaseMutex();

                        iRotaryControlTracker.Value = 0;
                        iRotaryControlTracker.Text = string.Empty;

                        iRockerControlTracker.Value = 0;
                        iRockerControlTracker.Text = string.Empty;
                    }
                }
                else
                {
                    iRotaryControlTracker.Value = 0;
                    iRotaryControlTracker.Text = string.Empty;

                    iRockerControlTracker.Value = 0;
                    iRockerControlTracker.Text = string.Empty;

                    iMutex.ReleaseMutex();
                }
            });
        }

        private void UpdateTargetSeconds()
        {
            iRotaryControlTracker.BeginInvoke((MethodInvoker)delegate()
            {
                iMutex.WaitOne();
                if (iOpen)
                {
                    Time duration = new Time(iDuration.SecondsTotal);
                    bool buffering = iBuffering;
                    iMutex.ReleaseMutex();

                    if (!buffering)
                    {
                        if (iApplyTargetSeconds)
                        {
                            string t = string.Empty;
                            if (iShowTimeRemaining)
                            {
                                Time time = new Time((int)iTargetSeconds - duration.SecondsTotal);
                                t = FormatTime(time, duration);
                            }
                            else
                            {
                                Time time = new Time((int)iTargetSeconds);
                                t = FormatTime(time, duration);
                            }
                            iRotaryControlTracker.Text = t;
                            iRockerControlTracker.Text = t;
                        }
                    }
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            });
        }

        private void TimerElapsed(object sender)
        {
            iAngleRemainder += ((360 * kRevolutionsPerSecond) / 1000.0f) * iTicker.MilliSeconds;
            float angle = ((int)(iAngleRemainder / 45.0f)) * 45.0f;
            iAngleRemainder -= angle;
            iAngle += angle;
            if (iAngle > 360.0f)
            {
                iAngle -= 360.0f;
            }
            iTicker.Reset();

            iRotaryControlTracker.Invalidate();
            iRockerControlTracker.Invalidate();
        }

        private void EventPaint(object sender, PaintEventArgs e)
        {
            Widgets.Kontrol kontrol = sender as Widgets.Kontrol;
            Assert.Check(kontrol != null);

            iMutex.WaitOne();
            if (iOpen)
            {
                if (iDuration.SecondsTotal > 0 && kontrol.MaxValue > 0 && iApplyTargetSeconds)
                {
                    float innerCircleRadius = kontrol.InnerCircleRadius;
                    Rectangle rect = new Rectangle((int)((kontrol.Width * 0.5f) - innerCircleRadius), (int)((kontrol.Height * 0.5f) - innerCircleRadius), (int)(innerCircleRadius * 2) + 1, (int)(innerCircleRadius * 2) + 1);
                    float c = 360 * (kontrol.Value / kontrol.MaxValue);
                    float d = (360 * (iTargetSeconds / kontrol.MaxValue));
                    float a = d - c;

                    iMutex.ReleaseMutex();

                    if (Math.Abs(a) < 1.0f)
                    {
                        a = 0;
                    }
                    e.Graphics.DrawArc(kPenArc, rect, 90 + c, a);
                }
                else if (iBuffering)
                {
                    iMutex.ReleaseMutex();

                    e.Graphics.DrawImage(kImageBuffering, 0, 0);

                    using (TextureBrush b = new TextureBrush(kImageBufferingElement))
                    {
                        b.TranslateTransform(kImageBuffering.Width * 0.5f, kImageBuffering.Height * 0.5f);
                        b.RotateTransform(iAngle);
                        b.TranslateTransform(-kImageBuffering.Width * 0.5f, -kImageBuffering.Height * 0.5f);
                        e.Graphics.FillRectangle(b, 0, 0, kImageBuffering.Width, kImageBuffering.Height);
                    }
                }
                else
                {
                    iMutex.ReleaseMutex();
                }

                iMutex.WaitOne();
                if (iTransportState == ETransportState.ePaused)
                {
                    iMutex.ReleaseMutex();

                    Point location = new Point((int)(((kontrol.Width - kImageMuteOn.Width) * 0.5f)),
                                               (int)(((kontrol.Height - kImageMuteOn.Height) * 0.5f)));
                    e.Graphics.DrawImage(kImageMuteOn, location);
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventClick(object sender, EventArgs e)
        {
            iShowTimeRemaining = !iShowTimeRemaining;
            SetSeconds((uint)iRotaryControlTracker.Value);
        }

        private void EventStartSeeking(object sender, EventArgs e)
        {
            iApplyTargetSeconds = false;
        }

        private void EventEndSeeking(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            if (iOpen)
            {
                if (iStopped)
                {
                    iRotaryControlTracker.Text = string.Empty;
                    iRockerControlTracker.Text = string.Empty;
                }

                iMutex.ReleaseMutex();

                if (iApplyTargetSeconds)
                {
                    iSeconds = iTargetSeconds;

                    if (EventSeekSeconds != null)
                    {
                        EventSeekSeconds(this, new EventArgsSeekSeconds(iTargetSeconds));
                    }
                }
                iRotaryControlTracker.ForeColor = Color.White;
                iRockerControlTracker.ForeColor = Color.White;
                iApplyTargetSeconds = false;
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventCancelSeeking(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            if (iOpen)
            {
                if (iStopped)
                {
                    iRotaryControlTracker.Text = string.Empty;
                    iRockerControlTracker.Text = string.Empty;
                }
            }
            iMutex.ReleaseMutex();

            iRotaryControlTracker.ForeColor = Color.White;
            iRockerControlTracker.ForeColor = Color.White;
            iApplyTargetSeconds = false;
            iTargetSeconds = 0;

            iRotaryControlTracker.Invalidate();
            iRockerControlTracker.Invalidate();
        }

        private void EventSeekForwards(object sender, EventArgs e)
        {
            Widgets.Kontrol kontrol = sender as Widgets.Kontrol;
            Assert.Check(kontrol != null);

            iMutex.WaitOne();
            if (iOpen)
            {
                if (!iApplyTargetSeconds)
                {
                    iTargetSeconds = iSeconds;
                    iApplyTargetSeconds = true;
                    kontrol.ForeColor = Color.Yellow;
                }

                iTargetSeconds += iSeekAmountPerStep;

                if (iTargetSeconds > iDuration.SecondsTotal)
                {
                    iTargetSeconds = (uint)iDuration.SecondsTotal;
                }

                iMutex.ReleaseMutex();

                UpdateTargetSeconds();
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventSeekBackwards(object sender, EventArgs e)
        {
            Widgets.Kontrol kontrol = sender as Widgets.Kontrol;
            Assert.Check(kontrol != null);

            iMutex.WaitOne();
            if (iOpen)
            {
                if (!iApplyTargetSeconds)
                {
                    iTargetSeconds = iSeconds;
                    iApplyTargetSeconds = true;
                    kontrol.ForeColor = Color.Yellow;
                }

                if (iTargetSeconds > iSeekAmountPerStep)
                {
                    iTargetSeconds -= iSeekAmountPerStep;
                }
                else
                {
                    iTargetSeconds = 0;
                }

                iMutex.ReleaseMutex();

                UpdateTargetSeconds();
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private string FormatTime(Time aTime, Time aDuration)
        {
            string result = string.Empty;
            if (aDuration.Hours > 0)
            {
                int minutes = (aTime.Hours * 60) + aTime.Minutes;
                result = minutes + ":" + string.Format("{0:00}", aTime.Seconds);
            }
            else
            {
                result = aTime.Minutes + ":" + string.Format("{0:00}", aTime.Seconds);
            }
            if (iShowTimeRemaining || aTime.SecondsTotal < 0)
            {
                result = "-" + result;
            }
            return result;
        }

        private const string kEmptyTime = "\u2013:\u2013\u2013";

        private Pen kPenArc = new Pen(Color.Yellow, 3);
        private Image kImageBuffering = KinskyDesktop.Properties.Resources.BusyIcon;
        private Image kImageBufferingElement = KinskyDesktop.Properties.Resources.BusyIconElement;
        private Image kImageMuteOn = KinskyDesktop.Properties.Resources.WheelMute;

        private const uint kUpdateRate = 20;
        private const float kRevolutionsPerSecond = 1.5f;

        private Mutex iMutex;
        private bool iOpen;

        private System.Threading.Timer iTimer;
        private Ticker iTicker;
        private float iAngle;
        private float iAngleRemainder;

        private bool iShowTimeRemaining;

        private bool iApplyTargetSeconds;
        private uint iTargetSeconds;
        private uint iSeekAmountPerStep;

        private ETransportState iTransportState;
        private bool iBuffering;
        private bool iStopped;
        private Time iDuration;
        private uint iSeconds;

        private ToolTip iToolTip;
        private Widgets.RotaryControl iRotaryControlTracker;
        private Widgets.RockerControl iRockerControlTracker;
    }

    abstract class ViewWidgetTransportControl : IViewWidgetTransportControl
    {
        public ViewWidgetTransportControl(FormKinskyDesktop aForm, DropConverter aDropConverter, IPlaylistSupport aPlaylistSupport)
        {
            iMutex = new Mutex(false);
            iOpen = false;

            iPlaylistSupport = aPlaylistSupport;
            iDropConverter = aDropConverter;
            iToolTip = aForm.ToolTip;
            iThreekArrayControl = aForm.ThreekArrayControl;
        }

        public void Open()
        {
            iMutex.WaitOne();

            Assert.Check(!iOpen);

            OnOpen();

            iThreekArrayControl.EventClickLeft += EventThreekArrayControl_EventLeftClick;
            iThreekArrayControl.EventClickMiddle += EventThreekArrayControl_EventMiddleClick;
            iThreekArrayControl.EventClickRight += EventThreekArrayControl_EventRightClick;

            iThreekArrayControl.EventDragDropLeft += EventDragDropLeft;
            iThreekArrayControl.EventDragDropMiddle += EventDragDropMiddle;
            iThreekArrayControl.EventDragDropRight += EventDragDropRight;
            iThreekArrayControl.EventDragOverLeft += EventDragOver;
            iThreekArrayControl.EventDragOverMiddle += EventDragOver;
            iThreekArrayControl.EventDragOverRight += EventDragOver;

            SetDragging(iPlaylistSupport.IsDragging());

            iOpen = true;

            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            if (iOpen)
            {
                OnClose();

                iThreekArrayControl.EventClickLeft -= EventThreekArrayControl_EventLeftClick;
                iThreekArrayControl.EventClickMiddle -= EventThreekArrayControl_EventMiddleClick;
                iThreekArrayControl.EventClickRight -= EventThreekArrayControl_EventRightClick;

                iThreekArrayControl.EventDragDropLeft -= EventDragDropLeft;
                iThreekArrayControl.EventDragDropMiddle -= EventDragDropMiddle;
                iThreekArrayControl.EventDragDropRight -= EventDragDropRight;
                iThreekArrayControl.EventDragOverLeft -= EventDragOver;
                iThreekArrayControl.EventDragOverMiddle -= EventDragOver;
                iThreekArrayControl.EventDragOverRight -= EventDragOver;
            }

            iThreekArrayControl.BeginInvoke((MethodInvoker)delegate()
            {
                iThreekArrayControl.Enabled = false;
                iToolTip.SetToolTip(iThreekArrayControl, "");
            });

            iOpen = false;

            iMutex.ReleaseMutex();
        }

        public void Initialised()
        {
            iThreekArrayControl.BeginInvoke((MethodInvoker)delegate()
            {
                iThreekArrayControl.Enabled = true;
                iToolTip.SetToolTip(iThreekArrayControl, "Playback controls");
            });
        }

        public void SetPlayNowEnabled(bool aEnabled)
        {
            iThreekArrayControl.PlaylistMiddleEnabled = aEnabled;
        }

        public void SetPlayNextEnabled(bool aEnabled)
        {
            iThreekArrayControl.PlaylistRightEnabled = aEnabled;
        }

        public void SetPlayLaterEnabled(bool aEnabled)
        {
            iThreekArrayControl.PlaylistLeftEnabled = aEnabled;
        }

        public void SetDragging(bool aDragging)
        {
            iThreekArrayControl.BeginInvoke((MethodInvoker)delegate()
            {
                if (aDragging)
                {
                    iThreekArrayControl.Mode = Widgets.ThreekArrayControl.EMode.ePlaylist;
                }
                else
                {
                    iThreekArrayControl.Mode = Widgets.ThreekArrayControl.EMode.eControl;
                }
            });
        }

        public void SetTransportState(ETransportState aTransportState)
        {
            iThreekArrayControl.BeginInvoke((MethodInvoker)delegate()
            {
                switch (aTransportState)
                {
                    case ETransportState.eBuffering:
                    case ETransportState.ePlaying:
                        iThreekArrayControl.State = true;
                        break;

                    default:
                        iThreekArrayControl.State = false;
                        break;
                }
            });
        }

        public void SetDuration(uint aDuration)
        {
        }

        public void SetAllowSkipping(bool aAllowSkipping) { }
        public void SetAllowPausing(bool aAllowPausing) { }

        public abstract event EventHandler<EventArgs> EventPause;
        public abstract event EventHandler<EventArgs> EventPlay;
        public abstract event EventHandler<EventArgs> EventStop;

        public event EventHandler<EventArgs> EventPrevious;
        public event EventHandler<EventArgs> EventNext;

        public event EventHandler<EventArgsPlay> EventPlayNow;
        public event EventHandler<EventArgsPlay> EventPlayNext;
        public event EventHandler<EventArgsPlay> EventPlayLater;

        protected abstract void OnOpen();
        protected abstract void OnClose();

        protected virtual void OnLeftClick()
        {
            if (EventPrevious != null)
            {
                EventPrevious(this, EventArgs.Empty);
            }
        }

        protected abstract void OnMiddleClick();

        protected virtual void OnRightClick()
        {
            if (EventNext != null)
            {
                EventNext(this, EventArgs.Empty);
            }
        }

        private void EventThreekArrayControl_EventLeftClick(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            if (iOpen)
            {
                iMutex.ReleaseMutex();

                OnLeftClick();
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventThreekArrayControl_EventMiddleClick(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            if (iOpen)
            {
                iMutex.ReleaseMutex();

                OnMiddleClick();
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventThreekArrayControl_EventRightClick(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            if (iOpen)
            {
                iMutex.ReleaseMutex();

                OnRightClick();
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventDragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;

            MediaProviderDraggable draggable = iDropConverter.Convert(e.Data);
            if (draggable != null)
            {
                if (!iPlaylistSupport.IsInserting())
                {
                    if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                    {
                        e.Effect = DragDropEffects.Copy;
                    }
                    if (draggable.DragSource is IViewWidgetPlaylist)
                    {
                        if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                        {
                            e.Effect = DragDropEffects.Move;
                        }
                    }
                }
            }
        }

        private void EventDragDropLeft(object sender, DragEventArgs e)
        {
            if ((e.Effect & DragDropEffects.Copy) == DragDropEffects.Copy || (e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                MediaProviderDraggable draggable = iDropConverter.Convert(e.Data);
                if (draggable != null)
                {
                    if (EventPlayLater != null)
                    {
                        EventPlayLater(this, new EventArgsPlay(draggable));
                    }
                }
            }
        }

        private void EventDragDropMiddle(object sender, DragEventArgs e)
        {
            if ((e.Effect & DragDropEffects.Copy) == DragDropEffects.Copy || (e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                MediaProviderDraggable draggable = iDropConverter.Convert(e.Data);
                if (draggable != null)
                {
                    if (EventPlayNow != null)
                    {
                        EventPlayNow(this, new EventArgsPlay(draggable));
                    }
                }
            }
        }

        private void EventDragDropRight(object sender, DragEventArgs e)
        {
            if ((e.Effect & DragDropEffects.Copy) == DragDropEffects.Copy || (e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                MediaProviderDraggable draggable = iDropConverter.Convert(e.Data);
                if (draggable != null)
                {
                    if (EventPlayNext != null)
                    {
                        EventPlayNext(this, new EventArgsPlay(draggable));
                    }
                }
            }
        }

        protected Mutex iMutex;
        protected bool iOpen;

        protected IPlaylistSupport iPlaylistSupport;
        protected DropConverter iDropConverter;

        protected ToolTip iToolTip;
        protected Widgets.ThreekArrayControl iThreekArrayControl;
    }

    class ViewWidgetTransportControlPause : ViewWidgetTransportControl
    {
        public ViewWidgetTransportControlPause(FormKinskyDesktop aForm, DropConverter aDropConverter, IPlaylistSupport aPlaylistSupport)
            : base(aForm, aDropConverter, aPlaylistSupport)
        {
        }

        public override event EventHandler<EventArgs> EventPause;
        public override event EventHandler<EventArgs> EventPlay;
        public override event EventHandler<EventArgs> EventStop { add { } remove { } }

        protected override void OnOpen()
        {
            iThreekArrayControl.MiddleImageState2 = KinskyDesktop.Properties.Resources.Pause;
            iThreekArrayControl.MiddleMouseOverImageState2 = KinskyDesktop.Properties.Resources.PauseMouse;
            iThreekArrayControl.MiddleTouchImageState2 = KinskyDesktop.Properties.Resources.PauseTouch;

            iThreekArrayControl.ControlLeftEnabled = true;
            iThreekArrayControl.ControlMiddleEnabled = true;
            iThreekArrayControl.ControlRightEnabled = true;
        }

        protected override void OnClose()
        {
        }

        protected override void OnMiddleClick()
        {
            iMutex.WaitOne();

            if (iThreekArrayControl.State)
            {
                iMutex.ReleaseMutex();

                if (EventPause != null)
                {
                    EventPause(this, EventArgs.Empty);
                }
            }
            else
            {
                iMutex.ReleaseMutex();

                if (EventPlay != null)
                {
                    EventPlay(this, EventArgs.Empty);
                }
            }
        }
    }

    class ViewWidgetTransportControlStop : ViewWidgetTransportControl
    {
        public ViewWidgetTransportControlStop(FormKinskyDesktop aForm, DropConverter aDropConverter, IPlaylistSupport aPlaylistSupport)
            : base(aForm, aDropConverter, aPlaylistSupport)
        {
        }

        public override event EventHandler<EventArgs> EventPause { add { } remove { } }
        public override event EventHandler<EventArgs> EventPlay;
        public override event EventHandler<EventArgs> EventStop;

        protected override void OnOpen()
        {
            iThreekArrayControl.MiddleImageState2 = KinskyDesktop.Properties.Resources.Stop;
            iThreekArrayControl.MiddleMouseOverImageState2 = KinskyDesktop.Properties.Resources.StopMouse;
            iThreekArrayControl.MiddleTouchImageState2 = KinskyDesktop.Properties.Resources.StopTouch;

            iThreekArrayControl.ControlLeftEnabled = true;
            iThreekArrayControl.ControlMiddleEnabled = true;
            iThreekArrayControl.ControlRightEnabled = true;
        }

        protected override void OnClose()
        {
        }

        protected override void OnMiddleClick()
        {
            iMutex.WaitOne();

            if (iThreekArrayControl.State)
            {
                iMutex.ReleaseMutex();

                if (EventStop != null)
                {
                    EventStop(this, EventArgs.Empty);
                }
            }
            else
            {
                iMutex.ReleaseMutex();

                if (EventPlay != null)
                {
                    EventPlay(this, EventArgs.Empty);
                }
            }
        }
    }

    class ViewWidgetTransportControlStopNoSeek : ViewWidgetTransportControlStop
    {
        public ViewWidgetTransportControlStopNoSeek(FormKinskyDesktop aForm, DropConverter aDropConverter, IPlaylistSupport aPlaylistSupport)
            : base(aForm, aDropConverter, aPlaylistSupport)
        {
        }

        protected override void OnOpen()
        {
            iThreekArrayControl.MiddleImageState2 = KinskyDesktop.Properties.Resources.Stop;
            iThreekArrayControl.MiddleMouseOverImageState2 = KinskyDesktop.Properties.Resources.StopMouse;
            iThreekArrayControl.MiddleTouchImageState2 = KinskyDesktop.Properties.Resources.StopTouch;

            iThreekArrayControl.ControlLeftEnabled = false;
            iThreekArrayControl.ControlMiddleEnabled = true;
            iThreekArrayControl.ControlRightEnabled = false;
        }

        protected override void OnClose()
        {
        }

        protected override void OnLeftClick()
        {
            throw new NotSupportedException();
        }

        protected override void OnRightClick()
        {
            throw new NotSupportedException();
        }
    }

    class ViewWidgetTrack : IViewWidgetTrack
    {
        public ViewWidgetTrack(FormKinskyDesktop aForm, IViewSupport aViewSupport)
        {
            iMutex = new Mutex(false);
            iOpen = false;

            iTitle = string.Empty;
            iAlbum = string.Empty;
            iArtist = string.Empty;
            iShowExtendedInfo = false;

            iForm = aForm;
            iPanelTrackInfo = aForm.PanelTrackInfo;
            iPanelTrackInfo.Paint += EventPanelTrackInfo_Paint;

            iFormArtwork = new FormArtwork();
            iFormArtwork.Icon = iForm.Icon;

            iViewSupport = aViewSupport;
            iViewSupport.EventSupportChanged += EventSupportChanged;

            iShowExtendedInfo = iForm.OptionPageGeneral.ShowTrackInfo;

            SetViewSupport();

            iForm.OptionPageGeneral.EventShowTrackInfoChanged += EventShowTrackInfoChanged;
        }

        public void Open()
        {
            iMutex.WaitOne();

            Assert.Check(!iOpen);

            iPanelTrackInfo.MouseDown += EventPanelTrackInfo_MouseDown;
            iPanelTrackInfo.MouseMove += EventPanelTrackInfo_MouseMove;
            iPanelTrackInfo.MouseUp += EventPanelTrackInfo_MouseUp;

            iMouseDownArtwork = false;

            iOpen = true;

            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            if (iOpen)
            {
                iPanelTrackInfo.MouseDown -= EventPanelTrackInfo_MouseDown;
                iPanelTrackInfo.MouseMove -= EventPanelTrackInfo_MouseMove;
                iPanelTrackInfo.MouseUp -= EventPanelTrackInfo_MouseUp;

                SetNoArtwork();
                iTitle = string.Empty;
                iAlbum = string.Empty;
                iArtist = string.Empty;

                Update();
            }

            iOpen = false;

            iMutex.ReleaseMutex();
        }

        public void Initialised()
        {
            iPanelTrackInfo.BeginInvoke((MethodInvoker)delegate()
            {
                iPanelTrackInfo.Invalidate();
            });
        }

        public void SetItem(upnpObject aObject)
        {
            iMutex.WaitOne();

            if (aObject != null)
            {
                iTitle = DidlLiteAdapter.Title(aObject);
                iArtist = DidlLiteAdapter.Artist(aObject);
                iAlbum = DidlLiteAdapter.Album(aObject);

                iArtworkUri = DidlLiteAdapter.ArtworkUri(aObject);

                if (iArtworkUri == null)
                {
                    SetNoArtwork();
                }
                else
                {
                    DownloadArtwork(iArtworkUri);
                }
            }
            else
            {
                iTitle = string.Empty;
                iArtist = string.Empty;
                iAlbum = string.Empty;

                iCodec = string.Empty;
                iBitDepth = 0;
                iSampleRate = 0;
                iBitrate = 0;
                iLossless = false;

                iArtworkUri = null;
                SetNoArtwork();
            }

            iMutex.ReleaseMutex();
        }

        private void DownloadArtwork(Uri aAlbumArtUri)
        {
            try
            {
                HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(aAlbumArtUri);
                wreq.Proxy = new WebProxy();
                wreq.KeepAlive = false;
                wreq.Pipelined = false;
                wreq.ContentLength = 0;
                wreq.AllowWriteStreamBuffering = true;
                WebRequestPool.QueueJob(new JobGetResponse(GetResponseCallback, wreq));
            }
            catch (Exception e)
            {
                Console.WriteLine("ViewWidgetTrack.DownloadArtwork: " + e.Message);
            }
        }

        private void GetResponseCallback(object aResult)
        {
            HttpWebRequest wreq = aResult as HttpWebRequest;
            HttpWebResponse wresp = null;
            Stream stream = null;

            try
            {
                wresp = (HttpWebResponse)wreq.GetResponse();
                stream = wresp.GetResponseStream();

                if (stream != null)
                {

                    Bitmap image = new Bitmap(stream);

                    iMutex.WaitOne();

                    if (iArtworkUri != null && wreq.RequestUri.AbsoluteUri == iArtworkUri.AbsoluteUri)
                    {
                        iArtwork = image;
                        iFormArtwork.SetArtwork(iTitle, iArtwork);

                        iMutex.ReleaseMutex();

                        Update();
                    }
                    else
                    {
                        SetNoArtwork();

                        iMutex.ReleaseMutex();

                        image.Dispose();

                        Update();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ViewWidgetTrack.GetResponseCallback: " + e.Message);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }

                if (wresp != null)
                {
                    wresp.Close();
                }
            }
        }

        private void SetNoArtwork()
        {
            Image artwork = iArtwork;

            iArtwork = kNoAlbumArt;
            iFormArtwork.SetArtwork(iTitle, kNoAlbumArt);

            if (artwork != null && artwork != kNoAlbumArt)
            {
                artwork.Dispose();
            }
        }

        public void SetMetatext(upnpObject aObject)
        {
            iMutex.WaitOne();

            if (aObject != null)
            {
                iAlbum = DidlLiteAdapter.Title(aObject);
                iArtist = string.Empty;
            }

            iMutex.ReleaseMutex();
        }

        public void SetBitrate(uint aBitrate)
        {
            iMutex.WaitOne();
            iBitrate = aBitrate;
            iMutex.ReleaseMutex();
        }

        public void SetSampleRate(float aSampleRate)
        {
            iMutex.WaitOne();
            iSampleRate = aSampleRate;
            iMutex.ReleaseMutex();
        }

        public void SetBitDepth(uint aBitDepth)
        {
            iMutex.WaitOne();
            iBitDepth = aBitDepth;
            iMutex.ReleaseMutex();
        }

        public void SetCodec(string aCodec)
        {
            iMutex.WaitOne();
            iCodec = aCodec;
            iMutex.ReleaseMutex();
        }

        public void SetLossless(bool aLossless)
        {
            iMutex.WaitOne();
            iLossless = aLossless;
            iMutex.ReleaseMutex();
        }

        public void Update()
        {
            iPanelTrackInfo.BeginInvoke((MethodInvoker)delegate()
            {
                iPanelTrackInfo.Invalidate();
                iFormArtwork.Invalidate();
            });
        }

        private void SetViewSupport()
        {
            iFontLarge = new Font(iViewSupport.FontLarge.FontFamily, 14, FontStyle.Bold);
            iFontMedium = new Font(iViewSupport.FontMedium.FontFamily, 10, FontStyle.Regular);
            iFontSmall = new Font(iViewSupport.FontSmall.FontFamily, 8, FontStyle.Regular);
            iBrightForeColour = iViewSupport.ForeColourBright;
        }

        private void EventSupportChanged(object sender, EventArgs e)
        {
            iFontLarge.Dispose();
            iFontMedium.Dispose();
            iFontSmall.Dispose();

            SetViewSupport();

            iPanelTrackInfo.Invalidate();
        }

        private void EventShowTrackInfoChanged(object sender, EventArgs e)
        {
            iShowExtendedInfo = iForm.OptionPageGeneral.ShowTrackInfo;
            iPanelTrackInfo.Invalidate();
        }

        private void EventPanelTrackInfo_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            iMutex.WaitOne();
            if (iOpen)
            {
                iMutex.ReleaseMutex();

                int offset = 2;// (int)((125 - 122) * 0.5f);
                int size = iPanelTrackInfo.Height;// -(offset * 2) - (17 * 2);

                iMutex.WaitOne();

                if (iArtwork != null)
                {
                    try
                    {
                        SizeF imageSize = ImageSize(iArtwork, size);
                        RectangleF rect = new RectangleF(0, 0, imageSize.Width, imageSize.Height);
                        e.Graphics.DrawImage(iArtwork, rect);
                    }
                    catch (ArgumentException ex)
                    {
                        Trace.WriteLine(Trace.kKinskyDesktop, "ViewWidgetTrack.EventPanelAlbumArt_Paint: " + ex.Message);
                    }
                }

                iMutex.ReleaseMutex();

                TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter;

                float y = offset;// +17;
                int x = size + 5;// (17 * 2);
                int width = iPanelTrackInfo.Width - size - 5;// (17 * 3);

                iMutex.WaitOne();
                string temp = iSampleRate.ToString() + " kHz" + " / " + iBitDepth.ToString() + " bits";
                Size textSize = TextRenderer.MeasureText(temp, iFontSmall);

                if (iShowExtendedInfo)
                {
                    if (iCodec != string.Empty)
                    {
                        width -= (textSize.Width + 5);
                    }
                }

                iMutex.ReleaseMutex();

                if (width < 0)
                {
                    width = 0;
                }

                iMutex.WaitOne();
                textSize = TextRenderer.MeasureText(iTitle, iFontLarge);
                TextRenderer.DrawText(e.Graphics, iTitle, iFontLarge, new Rectangle(x, (int)y, width, (int)textSize.Height), iBrightForeColour, flags);
                iMutex.ReleaseMutex();
                y += textSize.Height + 2;

                iMutex.WaitOne();

                string albumAndArtist = iAlbum;
                if (iArtist != string.Empty)
                {
                    if (albumAndArtist != string.Empty)
                    {
                        albumAndArtist += " \u2013 ";
                    }
                    albumAndArtist += iArtist;
                }

                textSize = TextRenderer.MeasureText(albumAndArtist, iFontMedium);
                TextRenderer.DrawText(e.Graphics, albumAndArtist, iFontMedium, new Rectangle(x, (int)y, width, (int)textSize.Height), iBrightForeColour, flags);

                iMutex.ReleaseMutex();

                if (iShowExtendedInfo)
                {
                    iMutex.WaitOne();
                    if (iCodec != string.Empty)
                    {
                        iMutex.ReleaseMutex();
                        int infoWidth = iPanelTrackInfo.Width - (x + width);
                        /*if (infoWidth > 100)
                        {
                            infoWidth = 100;
                        }*/

                        y = offset;// +17;

                        iMutex.WaitOne();
                        string bitrateString = iBitrate.ToString() + " kbps";
                        iMutex.ReleaseMutex();
                        textSize = TextRenderer.MeasureText(bitrateString, iFontSmall);
                        TextRenderer.DrawText(e.Graphics, bitrateString, iFontSmall, new Rectangle(x + width, (int)y, infoWidth, (int)textSize.Height), iBrightForeColour, flags | TextFormatFlags.Right);
                        y += textSize.Height + 2;

                        iMutex.WaitOne();
                        string sampleRateAndBitDepth = iSampleRate.ToString() + " kHz";
                        if (iLossless)
                        {
                            sampleRateAndBitDepth += " / " + iBitDepth.ToString() + " bits";
                        }
                        textSize = TextRenderer.MeasureText(sampleRateAndBitDepth, iFontSmall);
                        iMutex.ReleaseMutex();
                        TextRenderer.DrawText(e.Graphics, sampleRateAndBitDepth, iFontSmall, new Rectangle(x + width, (int)y, infoWidth, (int)textSize.Height), iBrightForeColour, flags | TextFormatFlags.Right);
                        y += textSize.Height + 2;

                        iMutex.WaitOne();
                        textSize = TextRenderer.MeasureText(iCodec, iFontSmall);
                        TextRenderer.DrawText(e.Graphics, iCodec, iFontSmall, new Rectangle(x + width, (int)y, infoWidth, (int)textSize.Height), iBrightForeColour, flags | TextFormatFlags.Right);
                        iMutex.ReleaseMutex();
                        y += textSize.Height + 2;
                    }
                    else
                    {
                        iMutex.ReleaseMutex();
                    }
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventPanelTrackInfo_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.X <= iPanelTrackInfo.Height + 5)
                {
                    iMouseDownArtwork = true;
                }
            }
        }

        private void EventPanelTrackInfo_MouseMove(object sender, MouseEventArgs e)
        {
            iMouseDownArtwork = false;
        }

        private void EventPanelTrackInfo_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (iMouseDownArtwork)
                {
                    iMutex.WaitOne();
                    if (!iFormArtwork.Visible && iArtwork != null)
                    {
                        iMutex.ReleaseMutex();
                        iFormArtwork.Show();
                    }
                    else
                    {
                        iMutex.ReleaseMutex();
                    }
                    iMouseDownArtwork = false;
                }
            }
        }

        private SizeF ImageSize(Image aImage, int aSize)
        {
            SizeF size = new SizeF(aSize, aSize);
            if (aImage.Width > aImage.Height)
            {
                size.Height = aSize * (aImage.Height / (float)aImage.Width);
            }
            else if (aImage.Height > aImage.Width)
            {
                size.Width = aSize * (aImage.Width / (float)aImage.Height);
            }
            return size;
        }

        private static readonly Image kNoAlbumArt = Linn.Kinsky.Properties.Resources.NoAlbumArt;

        private Color iBrightForeColour;
        private Font iFontLarge;
        private Font iFontMedium;
        private Font iFontSmall;

        private Mutex iMutex;
        private bool iOpen;

        private Image iArtwork;
        private Uri iArtworkUri;
        private string iTitle;
        private string iArtist;
        private string iAlbum;

        private uint iBitrate;
        private float iSampleRate;
        private uint iBitDepth;
        private string iCodec;
        private bool iLossless;

        private bool iShowExtendedInfo;
        private bool iMouseDownArtwork;

        private FormKinskyDesktop iForm;
        private Panel iPanelTrackInfo;
        private FormArtwork iFormArtwork;

        private IViewSupport iViewSupport;
    }

    class ViewWidgetPlayMode : IViewWidgetPlayMode
    {
        public ViewWidgetPlayMode(FormKinskyDesktop aForm)
        {
            iMutex = new Mutex(false);
            iOpen = false;

            iToolTip = aForm.ToolTip;
            iButtonRepeat = aForm.ButtonRepeat;
            iButtonShuffle = aForm.ButtonShuffle;
        }

        public void Open()
        {
            iMutex.WaitOne();

            Assert.Check(!iOpen);

            iButtonRepeat.EventClick += EventButtonRepeat_Click;
            iButtonShuffle.EventClick += EventButtonShuffle_Click;

            iOpen = true;

            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            if (iOpen)
            {
                iButtonRepeat.EventClick -= EventButtonRepeat_Click;
                iButtonShuffle.EventClick -= EventButtonShuffle_Click;
            }

            iButtonRepeat.BeginInvoke((MethodInvoker)delegate()
            {
                iButtonRepeat.Enabled = false;
                iToolTip.SetToolTip(iButtonRepeat, "");
            });

            iButtonShuffle.BeginInvoke((MethodInvoker)delegate()
            {
                iButtonShuffle.Enabled = false;
                iToolTip.SetToolTip(iButtonShuffle, "");
            });

            iOpen = false;

            iMutex.ReleaseMutex();
        }

        public void Initialised()
        {
            iButtonRepeat.BeginInvoke((MethodInvoker)delegate()
            {
                iButtonRepeat.Enabled = true;
            });

            iButtonShuffle.BeginInvoke((MethodInvoker)delegate()
            {
                iButtonShuffle.Enabled = true;
            });
        }

        public void SetShuffle(bool aShuffle)
        {
            iButtonShuffle.BeginInvoke((MethodInvoker)delegate()
            {
                iMutex.WaitOne();
                if (iOpen)
                {
                    iButtonShuffle.State = aShuffle;
                    iToolTip.SetToolTip(iButtonShuffle, "Turn shuffle " + (aShuffle ? "off" : "on"));
                }
                iMutex.ReleaseMutex();
            });
        }

        public void SetRepeat(bool aRepeat)
        {
            iButtonRepeat.BeginInvoke((MethodInvoker)delegate()
            {
                iMutex.WaitOne();
                if (iOpen)
                {
                    iButtonRepeat.State = aRepeat;
                    iToolTip.SetToolTip(iButtonRepeat, "Turn repeat " + (aRepeat ? "off" : "on"));
                }
                iMutex.ReleaseMutex();
            });
        }

        public event EventHandler<EventArgs> EventToggleShuffle;
        public event EventHandler<EventArgs> EventToggleRepeat;

        private void EventButtonRepeat_Click(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            if (iOpen)
            {
                iMutex.ReleaseMutex();

                if (EventToggleRepeat != null)
                {
                    EventToggleRepeat(this, EventArgs.Empty);
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventButtonShuffle_Click(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            if (iOpen)
            {
                iMutex.ReleaseMutex();

                if (EventToggleShuffle != null)
                {
                    EventToggleShuffle(this, EventArgs.Empty);
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private Mutex iMutex;
        private bool iOpen;

        private ToolTip iToolTip;
        private Widgets.ButtonControlBiState iButtonRepeat;
        private Widgets.ButtonControlBiState iButtonShuffle;
    }

    class ViewWidgetButtonSave : ViewWidgetButton
    {
        public ViewWidgetButtonSave(FormKinskyDesktop aForm, DropConverter aDropConverter, IViewSaveSupport aSaveSupport)
            : base(aForm.ButtonSave, aForm.ToolTip, "Save playlist")
        {
            iSaveSupport = aSaveSupport;
            iDropConverter = aDropConverter;
        }

        protected override void OnOpen()
        {
            iButton.DragEnter += EventDragEnter;
            iButton.DragDrop += EventDragDrop;
        }

        protected override void OnClose()
        {
            iButton.DragEnter -= EventDragEnter;
            iButton.DragDrop -= EventDragDrop;
        }

        private void EventDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;

            MediaProviderDraggable draggable = iDropConverter.Convert(e.Data);
            if (draggable != null)
            {
                if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
        }

        private void EventDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(MediaProviderDraggable)))
            {
                if ((e.Effect & DragDropEffects.Copy) == DragDropEffects.Copy)
                {
                    MediaProviderDraggable draggable = iDropConverter.Convert(e.Data);
                    if (draggable != null)
                    {
                        iSaveSupport.Save(draggable.Media);
                    }
                }
            }
        }

        private IViewSaveSupport iSaveSupport;
        private DropConverter iDropConverter;
    }

    class ViewWidgetButtonWasteBin : ViewWidgetButton
    {
        public ViewWidgetButtonWasteBin(FormKinskyDesktop aForm, DropConverter aDropConverter)
            : base(aForm.ButtonWasteBin, aForm.ToolTip, "Delete dragged items (or click to clear playlist)")
        {
            iDropConverter = aDropConverter;
        }

        protected override void OnOpen()
        {
            iButton.DragEnter += EventDragEnter;
            iButton.DragDrop += EventDragDrop;
        }

        protected override void OnClose()
        {
            iButton.DragEnter -= EventDragEnter;
            iButton.DragDrop -= EventDragDrop;
        }

        private void EventDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;

            MediaProviderDraggable draggable = iDropConverter.Convert(e.Data);
            if (draggable != null)
            {
                if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move)
                {
                    e.Effect = DragDropEffects.Move;
                }
            }
        }

        private void EventDragDrop(object sender, DragEventArgs e)
        {
            // do nothing source of drag will do the delete operation
        }

        private DropConverter iDropConverter;
    }

    class ViewWidgetPlaylistMediaRenderer : IViewWidgetPlaylist
    {
        public ViewWidgetPlaylistMediaRenderer(FormKinskyDesktop aForm, IArtworkCache aArtworkCache, DropConverter aDropConverter, IViewSupport aViewSupport, IPlaylistSupport aPlaySupport, IViewSaveSupport aSaveSupport)
        {
            iMutex = new Mutex(false);
            iOpen = false;

            iForm = aForm;

            iTimerScroll = new System.Threading.Timer(TimerScrollElapsed, null, Timeout.Infinite, Timeout.Infinite);

            iDropConverter = aDropConverter;
            iPlaylistSupport = aPlaySupport;
            iSaveSupport = aSaveSupport;

            iPlaylistWidget = new PlaylistWidget<MrItem>(aArtworkCache, aViewSupport);
            iPlaylistWidget.ImagePlaying = KinskyDesktop.Properties.Resources.IconPlaying;
            iPlaylistWidget.ContextMenuStrip = aForm.ContextMenuStripPlaylist;
            iPlaylistWidget.AllowDrop = true;
            iPlaylistWidget.BackColor = aViewSupport.BackColour;

            iPanelPlaylist = aForm.PanelPlaylist;

            iToolStripMenuItemPlay = aForm.ToolStripMenuItemPlay;
            iToolStripMenuItemRemoveFromList = aForm.ToolStripMenuItemRemoveFromList;
            iToolStripMenuItemMoveUp = aForm.ToolStripMenuItemMoveUp;
            iToolStripMenuItemMoveDown = aForm.ToolStripMenuItemMoveDown;
            iToolStripMenuItemSavePlaylist = aForm.ToolStripMenuItemSavePlaylist;
            iToolStripMenuItemDeletePlaylist = aForm.ToolStripMenuItemDeletePlaylist;
        }

        public void Open()
        {
            iMutex.WaitOne();

            Assert.Check(!iOpen);

            iPanelPlaylist.StartBusy();
            iPanelPlaylist.SetMessage("Subscribing...");

            iPlaylistWidget.EventItemActivated += EventItemActivated;
            iPlaylistWidget.EventSelectedIndexChanged += EventSelectedIndexChanged;
            iPlaylistWidget.ItemDrag += EventItemDrag;
            iPlaylistWidget.DragOver += EventDragOver;
            iPlaylistWidget.DragLeave += EventDragLeave;
            iPlaylistWidget.DragDrop += EventDragDrop;

            iToolStripMenuItemPlay.Click += EventToolStripMenuItemPlay_Click;
            iToolStripMenuItemRemoveFromList.Click += EventToolStripMenuItemRemove_Click;
            iToolStripMenuItemMoveUp.Click += EventToolStripMenuItemMoveUp_Click;
            iToolStripMenuItemMoveDown.Click += EventToolStripMenuItemMoveDown_Click;
            iToolStripMenuItemSavePlaylist.Click += EventToolStripMenuItemSavePlaylist_Click;
            iToolStripMenuItemDeletePlaylist.Click += EventToolStripMenuItemDeletePlaylist_Click;

            iPlaylistSupport.EventIsInsertingChanged += EventIsInsertingChanged;

            iOpen = true;

            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            if (iOpen)
            {
                iPlaylistWidget.EventItemActivated -= EventItemActivated;
                iPlaylistWidget.EventSelectedIndexChanged -= EventSelectedIndexChanged;
                iPlaylistWidget.ItemDrag -= EventItemDrag;
                iPlaylistWidget.DragOver -= EventDragOver;
                iPlaylistWidget.DragLeave -= EventDragLeave;
                iPlaylistWidget.DragDrop -= EventDragDrop;

                iToolStripMenuItemPlay.Click -= EventToolStripMenuItemPlay_Click;
                iToolStripMenuItemRemoveFromList.Click -= EventToolStripMenuItemRemove_Click;
                iToolStripMenuItemMoveUp.Click -= EventToolStripMenuItemMoveUp_Click;
                iToolStripMenuItemMoveDown.Click -= EventToolStripMenuItemMoveDown_Click;
                iToolStripMenuItemSavePlaylist.Click -= EventToolStripMenuItemSavePlaylist_Click;
                iToolStripMenuItemDeletePlaylist.Click -= EventToolStripMenuItemDeletePlaylist_Click;

                iPlaylistSupport.EventIsInsertingChanged -= EventIsInsertingChanged;

                iPanelPlaylist.StopBusy();
            }

            iPanelPlaylist.BeginInvoke((MethodInvoker)delegate()
            {
                iPanelPlaylist.Controls.Remove(iPlaylistWidget);
                iPanelPlaylist.Invalidate();
            });

            iPlaylistWidget.Clear();
            iOpen = false;

            iMutex.ReleaseMutex();
        }

        public void Initialised()
        {
            iPanelPlaylist.BeginInvoke((MethodInvoker)delegate()
            {
                iPlaylistWidget.Dock = DockStyle.Fill;
                iPlaylistWidget.Size = iPanelPlaylist.Size;
                iPanelPlaylist.Controls.Add(iPlaylistWidget);

                iPanelPlaylist.StopBusy();
            });
        }

        public void SetPlaylist(IList<MrItem> aPlaylist)
        {
            iMutex.WaitOne();

            iPlaylistWidget.BeginUpdate();
            iPlaylistWidget.Clear();
            for (int i = 0; i < aPlaylist.Count; ++i)
            {
                iPlaylistWidget.Add(aPlaylist[i], PlaylistWidget<MrItem>.EAdornment.eOff);
            }
            iPlaylistWidget.EndUpdate();
            iPlaylistWidget.SetTrack(iTrack);

            iMutex.ReleaseMutex();

            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                if (aPlaylist.Count > 0)
                {
                    iToolStripMenuItemSavePlaylist.Enabled = true;
                    iToolStripMenuItemDeletePlaylist.Enabled = true;
                }
                else
                {
                    iToolStripMenuItemSavePlaylist.Enabled = false;
                    iToolStripMenuItemDeletePlaylist.Enabled = false;
                }
            });
        }

        public void SetTrack(MrItem aTrack)
        {
            iMutex.WaitOne();

            iTrack = aTrack;
            iPlaylistWidget.SetTrack(aTrack);

            iMutex.ReleaseMutex();
        }

        public void Save()
        {
            List<upnpObject> list = new List<upnpObject>();

            iMutex.WaitOne();
            foreach (MrItem i in iPlaylistWidget.Tracks)
            {
                upnpObject o = i.DidlLite[0];
                list.Add(o);
            }
            iMutex.ReleaseMutex();

            iSaveSupport.Save(list);
        }

        public void Delete()
        {
            if (EventPlaylistDeleteAll != null)
            {
                EventPlaylistDeleteAll(this, EventArgs.Empty);
            }
        }

        public event EventHandler<EventArgsSeekTrack> EventSeekTrack;
        public event EventHandler<EventArgsPlaylistInsert> EventPlaylistInsert;
        public event EventHandler<EventArgsPlaylistMove> EventPlaylistMove;
        public event EventHandler<EventArgsPlaylistDelete> EventPlaylistDelete;
        public event EventHandler<EventArgs> EventPlaylistDeleteAll;

        private void EventIsInsertingChanged(object sender, EventArgs e)
        {
            iPlaylistWidget.BeginInvoke((MethodInvoker)delegate()
            {
                if (iPlaylistSupport.IsInserting())
                {
                    iPlaylistWidget.Cursor = Cursors.AppStarting;
                }
                else
                {
                    iPlaylistWidget.Cursor = Cursors.Default;
                }
            });
        }

        private void EventItemActivated(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            if (iPlaylistWidget.SelectedIndices.Count > 0)
            {
                if (iOpen)
                {
                    uint track = (uint)iPlaylistWidget.SelectedIndices[0];
                    iMutex.ReleaseMutex();

                    if (EventSeekTrack != null)
                    {
                        EventSeekTrack(this, new EventArgsSeekTrack(track));
                    }

                    //iPlaylistWidget.SelectedIndices.Clear();
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventSelectedIndexChanged(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            int count = iPlaylistWidget.SelectedIndices.Count;
            bool upEnabled = true;
            bool downEnabled = true;
            if (count == 1)
            {
                if (iPlaylistWidget.SelectedIndices[0] == 0)
                {
                    upEnabled = false;
                }
                if (iPlaylistWidget.SelectedIndices[0] == iPlaylistWidget.Tracks.Count - 1)
                {
                    downEnabled = false;
                }
            }

            iMutex.ReleaseMutex();

            if (count > 0)
            {
                if (count == 1)
                {
                    iToolStripMenuItemMoveUp.Enabled = upEnabled;
                    iToolStripMenuItemMoveDown.Enabled = downEnabled;
                }
                else
                {
                    iToolStripMenuItemMoveUp.Enabled = false;
                    iToolStripMenuItemMoveDown.Enabled = false;
                }

                iToolStripMenuItemPlay.Enabled = true;
                iToolStripMenuItemRemoveFromList.Enabled = true;
            }
            else
            {
                iToolStripMenuItemMoveDown.Enabled = false;
                iToolStripMenuItemMoveUp.Enabled = false;
                iToolStripMenuItemPlay.Enabled = false;
                iToolStripMenuItemRemoveFromList.Enabled = false;
            }
        }

        private void EventToolStripMenuItemPlay_Click(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            if (iPlaylistWidget.SelectedIndices.Count > 0)
            {
                if (iOpen)
                {
                    uint track = (uint)iPlaylistWidget.SelectedIndices[0];

                    iMutex.ReleaseMutex();

                    if (EventSeekTrack != null)
                    {
                        EventSeekTrack(this, new EventArgsSeekTrack(track));
                    }
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventToolStripMenuItemRemove_Click(object sender, EventArgs e)
        {
            List<MrItem> tracks = new List<MrItem>();

            iMutex.WaitOne();

            if (iOpen)
            {
                foreach (int i in iPlaylistWidget.SelectedIndices)
                {
                    tracks.Add(iPlaylistWidget.Tracks[i]);
                }

                iMutex.ReleaseMutex();
            }
            else
            {
                iMutex.ReleaseMutex();
            }

            if (tracks.Count > 0)
            {
                if (EventPlaylistDelete != null)
                {
                    EventPlaylistDelete(this, new EventArgsPlaylistDelete(tracks));
                }
            }
        }

        private void EventToolStripMenuItemMoveUp_Click(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            if (iPlaylistWidget.SelectedIndices.Count == 1)
            {
                int index = iPlaylistWidget.SelectedIndices[0];
                MrItem p = iPlaylistWidget.Tracks[index];

                List<MrItem> items = new List<MrItem>();
                items.Add(p);

                uint afterId = 0;
                if (index > 1)
                {
                    p = iPlaylistWidget.Tracks[index - 2];
                    afterId = p.Id;
                }

                if (iOpen)
                {
                    iMutex.ReleaseMutex();

                    if (EventPlaylistMove != null)
                    {
                        EventPlaylistMove(this, new EventArgsPlaylistMove(afterId, items));
                    }
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventToolStripMenuItemMoveDown_Click(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            if (iPlaylistWidget.SelectedIndices.Count == 1)
            {
                int index = iPlaylistWidget.SelectedIndices[0];
                MrItem p = iPlaylistWidget.Tracks[index];

                List<MrItem> items = new List<MrItem>();
                items.Add(p);

                uint afterId = 0;
                if ((index + 1) < iPlaylistWidget.Tracks.Count)
                {
                    p = iPlaylistWidget.Tracks[index + 1];
                    afterId = p.Id;
                }

                if (iOpen)
                {
                    iMutex.ReleaseMutex();

                    if (EventPlaylistMove != null)
                    {
                        EventPlaylistMove(this, new EventArgsPlaylistMove(afterId, items));
                    }
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventToolStripMenuItemSavePlaylist_Click(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            if (iOpen)
            {
                iMutex.ReleaseMutex();

                Save();
            }
            else
            {
                iMutex.ReleaseMutex();
            }

        }

        private void EventToolStripMenuItemDeletePlaylist_Click(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            if (iOpen)
            {
                iMutex.ReleaseMutex();

                Delete();
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventItemDrag(object sender, ItemDragEventArgs e)
        {
            List<MrItem> tracks = new List<MrItem>();
            List<upnpObject> items = new List<upnpObject>();

            iMutex.WaitOne();
            for (int i = 0; i < iPlaylistWidget.SelectedIndices.Count; ++i)
            {
                MrItem track = iPlaylistWidget.Tracks[iPlaylistWidget.SelectedIndices[i]];
                items.Add(track.DidlLite[0]);
                tracks.Add(track);
            }
            MediaProviderDraggable draggable = new MediaProviderDraggable(new MediaRetrieverNoRetrieve(items), this);
            iMutex.ReleaseMutex();

            DataObject dataObject = new DataObject();
            dataObject.SetData(draggable);
            if (items.Count == 1)
            {
                if (items[0].Res.Count > 0)
                {
                    dataObject.SetData(items[0].Res[0].Uri);
                }
            }

            iPlaylistSupport.SetDragging(true);
            DragDropEffects result = iPlaylistWidget.DoDragDrop(dataObject, DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll);
            iPlaylistSupport.SetDragging(false);

            if (result == DragDropEffects.Move)
            {
                if (EventPlaylistDelete != null)
                {
                    EventPlaylistDelete(this, new EventArgsPlaylistDelete(tracks));
                }
            }

            iTimerScroll.Change(Timeout.Infinite, Timeout.Infinite);
            iPlaylistWidget.InsertionMark.Index = -1;
        }

        private void EventDragOver(object sender, DragEventArgs e)
        {
            Point p = iPlaylistWidget.PointToClient(new Point(e.X, e.Y));
            if (p.Y <= 35 && iPlaylistWidget.ScrollBar.Value > iPlaylistWidget.ScrollBar.Minimum)
            {
                iScrollDirection = false;
                iTimerScroll.Change(0, kScrollTime);
            }
            else if (p.Y >= iPlaylistWidget.ClientSize.Height - 35 && iPlaylistWidget.ScrollBar.Value < iPlaylistWidget.ScrollBar.Maximum)
            {
                iScrollDirection = true;
                iTimerScroll.Change(0, kScrollTime);
            }
            else
            {
                iTimerScroll.Change(Timeout.Infinite, Timeout.Infinite);
            }

            e.Effect = DragDropEffects.None;

            MediaProviderDraggable draggable = iDropConverter.Convert(e.Data);
            if (draggable != null)
            {
                // only perform a move operation if we are dragging onto the playlist from the playlist
                if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move && (draggable.DragSource == this))
                {
                    e.Effect = DragDropEffects.Move;
                }
                else
                {
                    if (!iPlaylistSupport.IsInserting())
                    {
                        if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                        {
                            e.Effect = DragDropEffects.Copy;
                        }
                        else if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
                        {
                            e.Effect = DragDropEffects.Link;
                        }
                    }
                }
            }

            if (e.Effect != DragDropEffects.None)
            {
                foreach (upnpObject o in draggable.DragMedia)
                {
                    if (o.Res.Count > 0)
                    {
                        if (System.IO.Path.GetExtension(o.Res[0].Uri) == PluginManager.kPluginExtension)
                        {
                            e.Effect = DragDropEffects.None;
                            break;
                        }
                    }
                }
            }

            if (e.Effect == DragDropEffects.None)
            {
                iPlaylistWidget.InsertionMark.Index = -1;
            }
            else
            {
                iPlaylistWidget.InsertionMark.Update(iPlaylistWidget.PointToClient(new Point(e.X, e.Y)));
                if (iPlaylistWidget.InsertionMark.Index == -1)
                {
                    e.Effect = DragDropEffects.None;
                }
            }
        }

        private void EventDragLeave(object sender, EventArgs e)
        {
            iTimerScroll.Change(Timeout.Infinite, Timeout.Infinite);
            iPlaylistWidget.InsertionMark.Index = -1;
        }

        private void EventDragDrop(object sender, DragEventArgs e)
        {
            int index = iPlaylistWidget.InsertionMark.Index;
            if (iPlaylistWidget.InsertionMark.AppearsAfterItem)
            {
                ++index;
            }

            uint afterId = 0;
            if (index > 0)
            {
                MrItem p = iPlaylistWidget.Tracks[index - 1];
                afterId = p.Id;
            }

            if ((e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                List<MrItem> items = new List<MrItem>();
                foreach (int i in iPlaylistWidget.SelectedIndices)
                {
                    MrItem p = iPlaylistWidget.Tracks[i];
                    items.Add(p);
                }

                if (EventPlaylistMove != null)
                {
                    EventPlaylistMove(this, new EventArgsPlaylistMove(afterId, items));
                }
            }
            else if ((e.Effect & DragDropEffects.Copy) == DragDropEffects.Copy || (e.Effect & DragDropEffects.Link) == DragDropEffects.Link)
            {
                MediaProviderDraggable draggable = iDropConverter.Convert(e.Data);
                if (draggable != null)
                {
                    if (EventPlaylistInsert != null)
                    {
                        EventPlaylistInsert(this, new EventArgsPlaylistInsert(afterId, draggable));
                    }
                }
            }

            iTimerScroll.Change(Timeout.Infinite, Timeout.Infinite);
            iPlaylistWidget.InsertionMark.Index = -1;
        }

        private void TimerScrollElapsed(object sender)
        {
            iPlaylistWidget.BeginInvoke((MethodInvoker)delegate()
            {
                if (iScrollDirection)
                {
                    iPlaylistWidget.ScrollBar.Value += iPlaylistWidget.ScrollBar.SmallChange;
                }
                else
                {
                    iPlaylistWidget.ScrollBar.Value -= iPlaylistWidget.ScrollBar.SmallChange;
                }
            });
        }

        private int kScrollTime = 200;

        private Mutex iMutex;
        private bool iOpen;

        private Form iForm;

        private bool iScrollDirection;
        private System.Threading.Timer iTimerScroll;

        private MrItem iTrack;

        private DropConverter iDropConverter;
        private IPlaylistSupport iPlaylistSupport;
        private IViewSaveSupport iSaveSupport;

        private PanelBusy iPanelPlaylist;
        private PlaylistWidget<MrItem> iPlaylistWidget;

        private ToolStripMenuItem iToolStripMenuItemPlay;
        private ToolStripMenuItem iToolStripMenuItemRemoveFromList;
        private ToolStripMenuItem iToolStripMenuItemMoveUp;
        private ToolStripMenuItem iToolStripMenuItemMoveDown;
        private ToolStripMenuItem iToolStripMenuItemSavePlaylist;
        private ToolStripMenuItem iToolStripMenuItemDeletePlaylist;
    }

    /*class ViewWidgetPlaylist : IViewWidgetPlaylist
    {
        public ViewWidgetPlaylist(FormKinskyDesktop aForm, IMediaProviderSupportV5 aSupport)
        {
            iMutex = new Mutex(false);
            iOpen = false;
            iListViewItemLookup = new Dictionary<PlaylistItem, ListViewKinsky.Item>();

            iPlaylistSupport = aSupport.PlaylistSupport;
            iSaveSupport = aSupport.SaveSupport;
            iDropConverter = aSupport.AppSupport.DropConverter;

            iListViewPlaylist = new ListViewKinsky();
            iListViewPlaylist.AllowDrop = true;
            iListViewPlaylist.BorderStyle = System.Windows.Forms.BorderStyle.None;
            iListViewPlaylist.ContextMenuStrip = aForm.ContextMenuStripPlaylist;
            iListViewPlaylist.FullRowSelect = true;
            iListViewPlaylist.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            iListViewPlaylist.LargeIconSize = new System.Drawing.Size(128, 128);
            iListViewPlaylist.Name = "ListViewPlaylist";
            iListViewPlaylist.ShowItemToolTips = true;
            iListViewPlaylist.UseCompatibleStateImageBehavior = false;
            iListViewPlaylist.View = System.Windows.Forms.View.Details;

            iListViewPlaylist.SmallIconSize = new Size(13, 15);

            iPanelPlaylist = aForm.PanelPlaylist;

            iViewSupport = aSupport.ViewSupport;
            iViewSupport.EventSupportChanged += EventSupportChanged;

            SetViewSupport();

            //iListViewPlaylist.InsertionMark.Color = aSupport.ForeColour;

            iToolStripMenuItemPlay = aForm.ToolStripMenuItemPlay;
            iToolStripMenuItemRemoveFromList = aForm.ToolStripMenuItemRemoveFromList;
            iToolStripMenuItemMoveUp = aForm.ToolStripMenuItemMoveUp;
            iToolStripMenuItemMoveDown = aForm.ToolStripMenuItemMoveDown;
            iToolStripMenuItemSavePlaylist = aForm.ToolStripMenuItemSavePlaylist;
            iToolStripMenuItemDeletePlaylist = aForm.ToolStripMenuItemDeletePlaylist;
        }

        public void Open()
        {
            iMutex.WaitOne();

            Assert.Check(!iOpen);

            iListViewPlaylist.ItemActivate += EventListViewPlaylist_ItemActive;
            iListViewPlaylist.SelectedIndexChanged += EventListViewPlaylist_SelectedIndexChanged;
            iListViewPlaylist.ItemDrag += EventListViewPlaylist_ItemDrag;
            iListViewPlaylist.DragOver += EventListViewPlaylist_DragOver;
            iListViewPlaylist.DragLeave += EventListViewPlaylist_DragLeave;
            iListViewPlaylist.DragDrop += EventListViewPlaylist_DragDrop;

            iToolStripMenuItemPlay.Click += EventToolStripMenuItemPlay_Click;
            iToolStripMenuItemRemoveFromList.Click += EventToolStripMenuItemRemove_Click;
            iToolStripMenuItemMoveUp.Click += EventToolStripMenuItemMoveUp_Click;
            iToolStripMenuItemMoveDown.Click += EventToolStripMenuItemMoveDown_Click;
            iToolStripMenuItemSavePlaylist.Click += EventToolStripMenuItemSavePlaylist_Click;
            iToolStripMenuItemDeletePlaylist.Click += EventToolStripMenuItemDeletePlaylist_Click;

            iPanelPlaylist.StartBusy();
            iPanelPlaylist.SetMessage("Subscribing...");

            iOpen = true;

            Trace.WriteLine(Trace.kKinsky, "ViewWidgetPlaylist opened");

            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            if (iOpen)
            {
                iListViewPlaylist.ItemActivate -= EventListViewPlaylist_ItemActive;
                iListViewPlaylist.SelectedIndexChanged -= EventListViewPlaylist_SelectedIndexChanged;
                iListViewPlaylist.ItemDrag -= EventListViewPlaylist_ItemDrag;
                iListViewPlaylist.DragOver -= EventListViewPlaylist_DragOver;
                iListViewPlaylist.DragLeave -= EventListViewPlaylist_DragLeave;
                iListViewPlaylist.DragDrop -= EventListViewPlaylist_DragDrop;

                iToolStripMenuItemPlay.Click -= EventToolStripMenuItemPlay_Click;
                iToolStripMenuItemRemoveFromList.Click -= EventToolStripMenuItemRemove_Click;
                iToolStripMenuItemMoveUp.Click -= EventToolStripMenuItemMoveUp_Click;
                iToolStripMenuItemMoveDown.Click -= EventToolStripMenuItemMoveDown_Click;
                iToolStripMenuItemSavePlaylist.Click -= EventToolStripMenuItemSavePlaylist_Click;
                iToolStripMenuItemDeletePlaylist.Click -= EventToolStripMenuItemDeletePlaylist_Click;

                iPanelPlaylist.StopBusy();
            }

            iListViewPlaylist.BeginInvoke((MethodInvoker)delegate()
            {
                iPanelPlaylist.Controls.Remove(iListViewPlaylist);

                iListViewPlaylist.Columns.Clear();
                iListViewPlaylist.Items.Clear();

                iToolStripMenuItemSavePlaylist.Enabled = false;

                Trace.WriteLine(Trace.kKinsky, "ViewWidgetPlaylist closed");
            });

            iListViewItemLookup.Clear();
            iTrack = null;
            iOpen = false;

            Trace.WriteLine(Trace.kKinsky, "ViewWidgetPlaylist closing");

            iMutex.ReleaseMutex();
        }

        public void Initialised()
        {
            iListViewPlaylist.BeginInvoke((MethodInvoker)delegate()
            {
                SetColumns();

                iListViewPlaylist.Dock = DockStyle.Fill;
                iListViewPlaylist.Size = iPanelPlaylist.Size;

                iPanelPlaylist.StopBusy();

                iPanelPlaylist.Controls.Add(iListViewPlaylist);

                iToolStripMenuItemSavePlaylist.Enabled = false;

                Trace.WriteLine(Trace.kKinsky, "ViewWidgetPlaylist initialised");
            });

            Trace.WriteLine(Trace.kKinsky, "ViewWidgetPlaylist initialising");
        }

        public void SetPlaylist(IList<PlaylistItem> aPlaylist)
        {
            List<ListViewKinsky.Item> list = new List<ListViewKinsky.Item>();
            ListViewKinsky.Item track = null;

            iMutex.WaitOne();
            if (iOpen)
            {
                iMutex.ReleaseMutex();

                uint index = 0;
                foreach (PlaylistItem i in aPlaylist)
                {
                    ListViewKinsky.Item item;
                    if (!iListViewItemLookup.TryGetValue(i, out item))
                    {
                        item = new ListViewKinsky.Item((index + 1).ToString());
                        item.SubItems[0].Name = "TrackNumber";
                        item.Tag = i;

                        ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem();
                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Title";
                        subItem.Text = DidlLiteAdapter.Title(i.DidlLite[0]);
                        item.SubItems.Add(subItem);                                    // title

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Duration";
                        subItem.Text = DidlLiteAdapter.Duration(i.DidlLite[0]);
                        item.SubItems.Add(subItem);                                    // length

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Artist";
                        subItem.Text = DidlLiteAdapter.Artist(i.DidlLite[0]);
                        item.SubItems.Add(subItem);                                    // artist

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Album";
                        subItem.Text = DidlLiteAdapter.Album(i.DidlLite[0]);
                        item.SubItems.Add(subItem);                                    // album

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Genre";
                        subItem.Text = DidlLiteAdapter.Genre(i.DidlLite[0]);
                        item.SubItems.Add(subItem);                                    // genre

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "ReleaseYear";
                        subItem.Text = DidlLiteAdapter.ReleaseYear(i.DidlLite[0]);
                        item.SubItems.Add(subItem);                                    // release year

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Contributor";
                        subItem.Text = DidlLiteAdapter.Contributor(i.DidlLite[0]);
                        item.SubItems.Add(subItem);                                    // contributing artist

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Conductor";
                        subItem.Text = DidlLiteAdapter.Conductor(i.DidlLite[0]);
                        item.SubItems.Add(subItem);                                    // conductor

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Composer";
                        subItem.Text = DidlLiteAdapter.Composer(i.DidlLite[0]);
                        item.SubItems.Add(subItem);                                    // composer

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Type";
                        subItem.Text = DidlLiteAdapter.ProtocolInfo(i.DidlLite[0]);
                        item.SubItems.Add(subItem);                                    // type

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Bitrate";
                        subItem.Text = DidlLiteAdapter.Bitrate(i.DidlLite[0]);
                        item.SubItems.Add(subItem);                                    // bitrate

                        subItem = new ListViewItem.ListViewSubItem();
                        subItem.Name = "Uri";
                        subItem.Text = DidlLiteAdapter.Uri(i.DidlLite[0]);
                        item.SubItems.Add(subItem);                                    // uri

                        iListViewItemLookup.Add(i, item);
                    }

                    list.Add(item);

                    iMutex.WaitOne();
                    if (i == iTrack)
                    {
                        iMutex.ReleaseMutex();

                        track = item;
                    }
                    else
                    {
                        iMutex.ReleaseMutex();
                    }

                    ++index;
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }

            iListViewPlaylist.BeginInvoke((MethodInvoker)delegate()
            {
                iListViewPlaylist.BeginUpdate();

                int index = 0;
                for (int i = 0; i < list.Count; ++i, ++index)
                {
                    if (i < iListViewPlaylist.Items.Count)
                    {
                        // if the upnp object at position i is not the same as what is currently at i replace it
                        if (list[i] != iListViewPlaylist.Items[i])
                        {
                            Trace.WriteLine(Trace.kMediaRenderer, "ViewWidgetPlaylist.SetPlaylist: Moving item at " + list[i].Index + " to " + i);
                            iListViewPlaylist.Items.Remove(list[i]);
                            iListViewPlaylist.Items.Insert(i, list[i]);
                        }
                        list[i].Text = (i + 1).ToString();
                    }
                    else
                    {
                        break;
                    }
                }

                // if we have less items than in the current playlist, delete the excess
                while (index < iListViewPlaylist.Items.Count)
                {
                    Trace.WriteLine(Trace.kMediaRenderer, "ViewWidgetPlaylist.SetPlaylist: Removing item at " + index);
                    iListViewItemLookup.Remove(iListViewPlaylist.Items[index].Tag as PlaylistItem);
                    iListViewPlaylist.Items.RemoveAt(index);
                }

                // if we still have items to add
                if (index < list.Count)
                {
                    Trace.WriteLine(Trace.kMediaRenderer, "ViewWidgetPlaylist.SetPlaylist: Adding items at " + index + ", count=" + (list.Count - index));
                    iListViewPlaylist.Items.AddRange(list.GetRange(index, list.Count - index).ToArray());
                }

                SetItemAsTrack(track);

                iListViewPlaylist.EndUpdate();

                if (list.Count > 0)
                {
                    iToolStripMenuItemSavePlaylist.Enabled = true;
                    iToolStripMenuItemDeletePlaylist.Enabled = true;
                }
                else
                {
                    iToolStripMenuItemSavePlaylist.Enabled = false;
                    iToolStripMenuItemDeletePlaylist.Enabled = false;
                }
            });
        }

        public void SetTrack(PlaylistItem aTrack)
        {
            iMutex.WaitOne();
            PlaylistItem oldTrack = iTrack;
            iTrack = aTrack;
            iMutex.ReleaseMutex();

            iListViewPlaylist.BeginInvoke((MethodInvoker)delegate()
            {
                iMutex.WaitOne();
                if (iOpen)
                {
                    iMutex.ReleaseMutex();

                    iListViewPlaylist.BeginUpdate();

                    Trace.WriteLine(Trace.kKinskyDesktop, "ViewWidgetPlaylist.SetTrack: iListViewPlaylist.Items.Count=" + iListViewPlaylist.Items.Count);

                    foreach (ListViewKinsky.Item i in iListViewPlaylist.Items)
                    {
                        if (i.Tag == oldTrack)
                        {
                            //i.BackColor = iViewSupport.BackColour;
                            //i.ForeColor = iViewSupport.ForeColour;
                            i.Icon = null;
                        }
                        if (i.Tag == aTrack)
                        {
                            SetItemAsTrack(i);
                        }
                    }

                    iListViewPlaylist.EndUpdate();
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            });
        }

        public void Save()
        {
            List<upnpObject> list = new List<upnpObject>();
            foreach (ListViewItem i in iListViewPlaylist.Items)
            {
                PlaylistItem p = i.Tag as PlaylistItem;
                upnpObject o = p.DidlLite[0];
                o.Restricted = false;
                list.Add(o);
            }
            iSaveSupport.Save(list);
        }

        public void Delete()
        {
            if (EventPlaylistDeleteAll != null)
            {
                EventPlaylistDeleteAll(this, EventArgs.Empty);
            }
        }

        public event EventHandler<EventArgsSeekTrack> EventSeekTrack;

        public event EventHandler<EventArgsPlaylistInsert> EventPlaylistInsert;
        public event EventHandler<EventArgsPlaylistMove> EventPlaylistMove;
        public event EventHandler<EventArgsPlaylistDelete> EventPlaylistDelete;

        public event EventHandler<EventArgs> EventPlaylistDeleteAll;

        private void SetItemAsTrack(ListViewKinsky.Item aItem)
        {
            //aItem.BackColor = iViewSupport.HighlightBackColour;
            //aItem.ForeColor = iViewSupport.HighlightForeColour;
            if (aItem != null)
            {
                aItem.Icon = KinskyDesktop.Properties.Resources.IconPlaying;
            }
        }

        private void SetColumns()
        {
            ColumnHeader column = new ColumnHeader();
            column.Name = "TrackNumber";
            column.Text = "";
            column.Width = 50;
            iListViewPlaylist.Columns.Add(column);

            column = new ColumnHeader();
            column.Name = "Title";
            column.Text = "Title";
            column.Width = 250;
            iListViewPlaylist.Columns.Add(column);

            column = new ColumnHeader();
            column.Name = "Duration";
            column.Text = "Duration";
            column.Width = 100;
            iListViewPlaylist.Columns.Add(column);

            column = new ColumnHeader();
            column.Name = "Artist";
            column.Text = "Artist";
            column.Width = 250;
            iListViewPlaylist.Columns.Add(column);

            column = new ColumnHeader();
            column.Name = "Album";
            column.Text = "Album";
            column.Width = 250;
            iListViewPlaylist.Columns.Add(column);

            column = new ColumnHeader();
            column.Name = "Genre";
            column.Text = "Genre";
            column.Width = 250;
            iListViewPlaylist.Columns.Add(column);

            column = new ColumnHeader();
            column.Name = "ReleaseYear";
            column.Text = "Release Year";
            column.Width = 250;
            iListViewPlaylist.Columns.Add(column);

            column = new ColumnHeader();
            column.Name = "Contributor";
            column.Text = "Contributing Artist";
            column.Width = 250;
            iListViewPlaylist.Columns.Add(column);

            column = new ColumnHeader();
            column.Name = "Conductor";
            column.Text = "Conductor";
            column.Width = 250;
            iListViewPlaylist.Columns.Add(column);

            column = new ColumnHeader();
            column.Name = "Composer";
            column.Text = "Composer";
            column.Width = 250;
            iListViewPlaylist.Columns.Add(column);

            column = new ColumnHeader();
            column.Name = "Type";
            column.Text = "Type";
            column.Width = 250;
            iListViewPlaylist.Columns.Add(column);

            column = new ColumnHeader();
            column.Name = "Bitrate";
            column.Text = "Bitrate";
            column.Width = 250;
            iListViewPlaylist.Columns.Add(column);

            column = new ColumnHeader();
            column.Name = "Uri";
            column.Text = "URI";
            column.Width = 250;
            iListViewPlaylist.Columns.Add(column);
        }

        private void EventListViewPlaylist_ItemActive(object sender, EventArgs e)
        {
            if (iListViewPlaylist.SelectedIndices.Count == 1)
            {
                iMutex.WaitOne();
                if (iOpen)
                {
                    iMutex.ReleaseMutex();

                    if (EventSeekTrack != null)
                    {
                        EventSeekTrack(this, new EventArgsSeekTrack((uint)iListViewPlaylist.SelectedIndices[0]));
                    }

                    iListViewPlaylist.SelectedIndices.Clear();
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            }
        }

        private void EventListViewPlaylist_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (iListViewPlaylist.SelectedIndices.Count > 0)
            {
                if (iListViewPlaylist.SelectedIndices.Count == 1)
                {
                    if (iListViewPlaylist.SelectedIndices[0] == 0)
                    {
                        iToolStripMenuItemMoveUp.Enabled = false;
                    }
                    else
                    {
                        iToolStripMenuItemMoveUp.Enabled = true;
                    }

                    if (iListViewPlaylist.SelectedIndices[0] == iListViewPlaylist.Items.Count - 1)
                    {
                        iToolStripMenuItemMoveDown.Enabled = false;
                    }
                    else
                    {
                        iToolStripMenuItemMoveDown.Enabled = true;
                    }
                }
                else
                {
                    iToolStripMenuItemMoveUp.Enabled = false;
                    iToolStripMenuItemMoveDown.Enabled = false;
                }

                iToolStripMenuItemPlay.Enabled = (iListViewPlaylist.SelectedIndices.Count == 1);
                iToolStripMenuItemRemoveFromList.Enabled = true;
            }
            else
            {
                iToolStripMenuItemMoveDown.Enabled = false;
                iToolStripMenuItemMoveUp.Enabled = false;
                iToolStripMenuItemPlay.Enabled = false;
                iToolStripMenuItemRemoveFromList.Enabled = false;
            }
        }

        private void EventListViewPlaylist_ItemDrag(object sender, ItemDragEventArgs e)
        {
            List<uint> indicies = new List<uint>();
            List<upnpObject> items = new List<upnpObject>();

            iMutex.WaitOne();
            for (int i = 0; i < iListViewPlaylist.SelectedIndices.Count; ++i)
            {
                ListViewItem item = iListViewPlaylist.Items[iListViewPlaylist.SelectedIndices[i]];
                PlaylistItem p = item.Tag as PlaylistItem;
                items.Add(p.DidlLite[0]);
                indicies.Add(p.Id);
            }
            MediaProviderDraggable draggable = new MediaProviderDraggable(new MediaRetrieverNoRetrieve(items));
            draggable.Tag = iListViewPlaylist;
            iMutex.ReleaseMutex();

            DataObject dataObject = new DataObject();
            dataObject.SetData(draggable);
            if (items.Count == 1)
            {
                if (items[0].Res.Count > 0)
                {
                    dataObject.SetData(items[0].Res[0].Uri);
                }
            }
            DragDropEffects result = iListViewPlaylist.DoDragDrop(dataObject, DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll);

            if (result == DragDropEffects.Move)
            {
                if (EventPlaylistDelete != null)
                {
                    EventPlaylistDelete(this, new EventArgsPlaylistDelete(indicies));
                }
            }

            DrawInsertionMark(iBrushBackColour, iPenBackColour);
        }

        private void EventListViewPlaylist_DragOver(object sender, DragEventArgs e)
        {
            DrawInsertionMark(iBrushBackColour, iPenBackColour);

            e.Effect = DragDropEffects.None;

            MediaProviderDraggable draggable = iDropConverter.Convert(e.Data);
            if (draggable != null)
            {
                // only perform a move operation if we are dragging onto the playlist from the playlist
                if ((e.AllowedEffect & DragDropEffects.Move) == DragDropEffects.Move && (draggable.Tag == iListViewPlaylist))
                {
                    e.Effect = DragDropEffects.Move;
                }
                else
                {
                    if (!iPlaylistSupport.IsInserting())
                    {
                        if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                        {
                            e.Effect = DragDropEffects.Copy;
                        }
                        else if ((e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
                        {
                            e.Effect = DragDropEffects.Link;
                        }
                    }
                }
            }

            int index = -1;
            Point point = iListViewPlaylist.PointToClient(new Point(e.X, e.Y));
            ListViewItem item = iListViewPlaylist.GetItemAt(point.X, point.Y);
            if (item != null)
            {
                index = item.Index;
            }
            else
            {
                if (point.Y > 3)
                {
                    index = iListViewPlaylist.Items.Count - 1;
                    if (index == -1)
                    {
                        index = 0;
                    }
                }
            }

            if (index > -1 && index < iListViewPlaylist.Items.Count)
            {
                Rectangle rect = iListViewPlaylist.GetItemRect(index);

                if (point.Y > rect.Top + (rect.Height * 0.5f))
                {
                    iInsertAfter = true;
                    //iListViewPlaylist.InsertionMark.AppearsAfterItem = true;
                    if (index + 1 < iListViewPlaylist.Items.Count)
                    {
                        iListViewPlaylist.EnsureVisible(index + 1);
                        Console.WriteLine("ensure visible = " + (index + 1));
                    }
                    else
                    {
                        iListViewPlaylist.EnsureVisible(index);
                        Console.WriteLine("ensure visible = " + index);
                    }
                }
                else
                {
                    iInsertAfter = false;
                    iListViewPlaylist.InsertionMark.AppearsAfterItem = false;
                    if (index > 0)
                    {
                        iListViewPlaylist.EnsureVisible(index - 1);
                        Console.WriteLine("ensure visible = " + (index - 1));
                    }
                    else
                    {
                        iListViewPlaylist.EnsureVisible(index);
                        Console.WriteLine("ensure visible = " + index);
                    }
                }
            }

            Console.WriteLine("index=" + index + ", NearestIndex=" + iListViewPlaylist.InsertionMark.NearestIndex(point) + ", insert after=" + iInsertAfter);

            iInsertIndex = index;
            //iListViewPlaylist.InsertionMark.Index = index;

            if (e.Effect != DragDropEffects.None)
            {
                DrawInsertionMark(iBrushForeColourBright, iPenForeColourBright);
            }
        }

        private void DrawInsertionMark(Brush aBrush, Pen aPen)
        {
            using (Graphics g = iListViewPlaylist.CreateGraphics())
            {
                if (iInsertIndex > -1 && iInsertIndex < iListViewPlaylist.Items.Count)
                {
                    int i = iInsertIndex;
                    Rectangle bounds = iListViewPlaylist.Items[i].GetBounds(ItemBoundsPortion.Entire);
                    int y = bounds.Top;
                    if (iInsertAfter)
                    {
                        y = bounds.Bottom;
                    }

                    g.DrawLine(aPen, bounds.Left, y, 35, y);

                    Point[] leftTriangle = new Point[3] {
                            new Point(bounds.Left, y - 4),
                            new Point(bounds.Left + 7, y),
                            new Point(bounds.Left, y + 4)
                        };
                    g.FillPolygon(aBrush, leftTriangle);
                    //Point[] rightTriangle = new Point[3] {
                    //        new Point(bounds.Right, y - 4),
                    //        new Point(bounds.Right - 8, y),
                    //        new Point(bounds.Right, y + 4)
                    //    };
                    //g.FillPolygon(aBrush, rightTriangle);
                }
            }
        }

        private void EventListViewPlaylist_DragLeave(object sender, EventArgs e)
        {
            DrawInsertionMark(iBrushBackColour, iPenBackColour);

            iInsertIndex = -1;
            iInsertAfter = false;
        }

        private void EventListViewPlaylist_DragDrop(object sender, DragEventArgs e)
        {
            int index = iInsertIndex;
            if (iInsertAfter && index < iListViewPlaylist.Items.Count)
            {
                ++index;
            }

            uint afterId = 0;
            if (index > 0)
            {
                ListViewItem item = iListViewPlaylist.Items[index - 1];
                PlaylistItem p = item.Tag as PlaylistItem;
                afterId = p.Id;
            }

            if ((e.Effect & DragDropEffects.Move) == DragDropEffects.Move)
            {
                List<PlaylistItem> items = new List<PlaylistItem>();
                foreach (int i in iListViewPlaylist.SelectedIndices)
                {
                    ListViewItem item = iListViewPlaylist.Items[i];
                    PlaylistItem p = item.Tag as PlaylistItem;
                    items.Add(p);
                }

                if (EventPlaylistMove != null)
                {
                    EventPlaylistMove(this, new EventArgsPlaylistMove(afterId, items));
                }
            }
            else if ((e.Effect & DragDropEffects.Copy) == DragDropEffects.Copy || (e.Effect & DragDropEffects.Link) == DragDropEffects.Link)
            {
                MediaProviderDraggable draggable = iDropConverter.Convert(e.Data);
                if (draggable != null)
                {
                    if (EventPlaylistInsert != null)
                    {
                        EventPlaylistInsert(this, new EventArgsPlaylistInsert(index, draggable));
                    }
                }
            }

            iInsertIndex = -1;
            iInsertAfter = false;
        }

        private void EventToolStripMenuItemPlay_Click(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            if (iOpen)
            {
                iMutex.ReleaseMutex();

                if (EventSeekTrack != null)
                {
                    EventSeekTrack(this, new EventArgsSeekTrack((uint)iListViewPlaylist.SelectedIndices[iListViewPlaylist.SelectedIndices.Count - 1]));
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventToolStripMenuItemRemove_Click(object sender, EventArgs e)
        {
            List<uint> ids = new List<uint>();
            foreach (int i in iListViewPlaylist.SelectedIndices)
            {
                ListViewItem item = iListViewPlaylist.Items[i];
                PlaylistItem p = item.Tag as PlaylistItem;
                ids.Add(p.Id);
            }
            if (EventPlaylistDelete != null)
            {
                EventPlaylistDelete(this, new EventArgsPlaylistDelete(ids));
            }
        }

        private void EventToolStripMenuItemMoveUp_Click(object sender, EventArgs e)
        {
            if (iListViewPlaylist.SelectedIndices.Count == 1)
            {
                int index = iListViewPlaylist.SelectedIndices[0];
                ListViewItem item = iListViewPlaylist.Items[index];
                PlaylistItem p = item.Tag as PlaylistItem;

                List<PlaylistItem> items = new List<PlaylistItem>();
                items.Add(p);

                uint afterId = 0;
                if (index > 0)
                {
                    item = iListViewPlaylist.Items[index - 2];
                    p = item.Tag as PlaylistItem;
                    afterId = p.Id;
                }

                iMutex.WaitOne();
                if (iOpen)
                {
                    iMutex.ReleaseMutex();

                    if (EventPlaylistMove != null)
                    {
                        EventPlaylistMove(this, new EventArgsPlaylistMove(afterId, items));
                    }
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            }
        }

        private void EventToolStripMenuItemMoveDown_Click(object sender, EventArgs e)
        {
            if (iListViewPlaylist.SelectedIndices.Count == 1)
            {
                int index = iListViewPlaylist.SelectedIndices[0];
                ListViewItem item = iListViewPlaylist.Items[index];
                PlaylistItem p = item.Tag as PlaylistItem;

                List<PlaylistItem> items = new List<PlaylistItem>();
                items.Add(p);

                uint afterId = 0;
                if ((index + 1) < iListViewPlaylist.Items.Count)
                {
                    item = iListViewPlaylist.Items[index + 1];
                    p = item.Tag as PlaylistItem;
                    afterId = p.Id;
                }

                iMutex.WaitOne();
                if (iOpen)
                {
                    iMutex.ReleaseMutex();

                    if (EventPlaylistMove != null)
                    {
                        EventPlaylistMove(this, new EventArgsPlaylistMove(afterId, items));
                    }
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            }
        }

        private void EventToolStripMenuItemSavePlaylist_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void EventToolStripMenuItemDeletePlaylist_Click(object sender, EventArgs e)
        {
            Delete();
        }

        private void SetViewSupport()
        {
            iListViewPlaylist.BackColor = iViewSupport.BackColour;
            iListViewPlaylist.ForeColor = iViewSupport.ForeColour;
            iListViewPlaylist.HighlightBackColour = iViewSupport.HighlightBackColour;
            iListViewPlaylist.HighlightForeColour = iViewSupport.HighlightForeColour;
            iListViewPlaylist.ForeColorBright = iViewSupport.ForeColourBright;
            iListViewPlaylist.ForeColorMuted = iViewSupport.ForeColourMuted;

            iListViewPlaylist.Font = iViewSupport.FontSmall;

            iBrushBackColour = new SolidBrush(iViewSupport.BackColour);
            iPenBackColour = new Pen(iViewSupport.BackColour);
            iBrushForeColourBright = new SolidBrush(iViewSupport.ForeColourBright);
            iPenForeColourBright = new Pen(iViewSupport.ForeColourBright);
        }

        private void EventSupportChanged(object sender, EventArgs e)
        {
            iBrushBackColour.Dispose();
            iPenBackColour.Dispose();
            iBrushForeColourBright.Dispose();
            iPenForeColourBright.Dispose();

            SetViewSupport();
        }

        private Brush iBrushBackColour;
        private Pen iPenBackColour;
        private Brush iBrushForeColourBright;
        private Pen iPenForeColourBright;

        private Mutex iMutex;
        private bool iOpen;

        private Dictionary<PlaylistItem, ListViewKinsky.Item> iListViewItemLookup;
        private PlaylistItem iTrack;

        private DropConverter iDropConverter;

        private ListViewKinsky iListViewPlaylist;
        private PanelBusy iPanelPlaylist;

        private int iInsertIndex;
        private bool iInsertAfter;

        private IViewSupport iViewSupport;
        private IPlaylistSupport iPlaylistSupport;
        private IViewSaveSupport iSaveSupport;

        private ToolStripMenuItem iToolStripMenuItemPlay;
        private ToolStripMenuItem iToolStripMenuItemRemoveFromList;
        private ToolStripMenuItem iToolStripMenuItemMoveUp;
        private ToolStripMenuItem iToolStripMenuItemMoveDown;
        private ToolStripMenuItem iToolStripMenuItemSavePlaylist;
        private ToolStripMenuItem iToolStripMenuItemDeletePlaylist;
    }*/

    class ViewWidgetPlaylistRadio : IViewWidgetPlaylistRadio
    {
        public ViewWidgetPlaylistRadio(FormKinskyDesktop aForm, IArtworkCache aArtworkCache, IViewSupport aViewSupport, IPlaylistSupport aPlaySupport, IViewSaveSupport aSaveSupport)
        {
            iMutex = new Mutex(false);
            iOpen = false;

            iPlaylistSupport = aPlaySupport;
            iSaveSupport = aSaveSupport;

            iPanelPlaylist = aForm.PanelPlaylist;

            iListView = new PlaylistWidget<MrItem>(aArtworkCache, aViewSupport);
            iListView.ImagePlaying = KinskyDesktop.Properties.Resources.IconPlaying;
            iListView.AllowDrop = true;
            iListView.BackColor = aViewSupport.BackColour;
            iListView.Dock = DockStyle.Fill;
        }

        public void Open()
        {
            iMutex.WaitOne();

            Assert.Check(!iOpen);

            iPanelPlaylist.StartBusy();
            iPanelPlaylist.SetMessage("Subscribing...");

            iPresetIndex = -1;

            iListView.EventItemActivated += EventItemActivate;
            iListView.ItemDrag += EventItemDrag;

            iOpen = true;
            Trace.WriteLine(Trace.kKinsky, "ViewWidgetPlaylistRadio Opened");

            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            if (iOpen)
            {
                iListView.EventItemActivated -= EventItemActivate;
                iListView.ItemDrag -= EventItemDrag;

                iPanelPlaylist.StopBusy();
            }

            iPanelPlaylist.BeginInvoke((MethodInvoker)delegate()
            {
                iPanelPlaylist.Controls.Remove(iListView);
                iPanelPlaylist.Invalidate();

                iListView.Clear();

                Trace.WriteLine(Trace.kKinsky, "ViewWidgetPlaylistRadio Closed");
            });

            iOpen = false;

            iMutex.ReleaseMutex();
        }

        public void Initialised()
        {
            iPanelPlaylist.BeginInvoke((MethodInvoker)delegate()
            {
                iPanelPlaylist.Controls.Add(iListView);

                iPanelPlaylist.StopBusy();

                Trace.WriteLine(Trace.kKinsky, "ViewWidgetPlaylistRadio Initialised");
            });
        }

        public void SetPresets(IList<MrItem> aPresets)
        {
            iMutex.WaitOne();

            iListView.BeginUpdate();

            iListView.Clear();

            for (int i = 0; i < aPresets.Count; ++i)
            {
                MrItem p = aPresets[i];

                iListView.Add(p, PlaylistWidget<MrItem>.EAdornment.eOff);
            }

            UpdatePreset(iPresetIndex);

            iListView.EndUpdate();

            iMutex.ReleaseMutex();
        }

        public void SetChannel(Channel aChannel)
        {
            iMutex.WaitOne();
            iChannel = aChannel;
            iMutex.ReleaseMutex();
        }

        public void SetPreset(int aPresetIndex)
        {
            iMutex.WaitOne();

            iListView.BeginUpdate();
            UpdatePreset(aPresetIndex);
            iListView.EndUpdate();

            iMutex.ReleaseMutex();
        }

        private void UpdatePreset(int aPresetIndex)
        {
            iMutex.WaitOne();

            if (aPresetIndex > -1 && aPresetIndex < iListView.Tracks.Count)
            {
                iListView.SetTrack(iListView.Tracks[aPresetIndex]);
            }
            else
            {
                iListView.SetTrack(null);
            }

            iPresetIndex = aPresetIndex;

            iMutex.ReleaseMutex();
        }

        public void Save()
        {
            List<upnpObject> list = new List<upnpObject>();

            iMutex.WaitOne();
            list.AddRange(iChannel.DidlLite);
            iMutex.ReleaseMutex();

            iSaveSupport.Save(list);
        }

        public event EventHandler<EventArgsSetPreset> EventSetPreset;
        public event EventHandler<EventArgsSetChannel> EventSetChannel { add { } remove { } }

        private void EventItemActivate(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            if (iListView.SelectedIndices.Count > 0)
            {
                if (iOpen)
                {
                    MrItem preset = iListView.Tracks[iListView.SelectedIndices[0]];
                    iMutex.ReleaseMutex();

                    if (EventSetPreset != null)
                    {
                        EventSetPreset(this, new EventArgsSetPreset(preset));
                    }
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventItemDrag(object sender, ItemDragEventArgs e)
        {
            List<MrItem> presets = new List<MrItem>();
            List<upnpObject> items = new List<upnpObject>();

            iMutex.WaitOne();
            for (int i = 0; i < iListView.SelectedIndices.Count; ++i)
            {
                MrItem preset = iListView.Tracks[iListView.SelectedIndices[i]];
                items.Add(preset.DidlLite[0]);
                presets.Add(preset);
            }
            MediaProviderDraggable draggable = new MediaProviderDraggable(new MediaRetrieverNoRetrieve(items), iListView);
            iMutex.ReleaseMutex();

            DataObject dataObject = new DataObject();
            dataObject.SetData(draggable);
            if (items.Count == 1)
            {
                if (items[0].Res.Count > 0)
                {
                    dataObject.SetData(items[0].Res[0].Uri);
                }
            }

            iPlaylistSupport.SetDragging(true);
            iListView.DoDragDrop(dataObject, DragDropEffects.Copy | DragDropEffects.Scroll);
            iPlaylistSupport.SetDragging(false);

            iListView.InsertionMark.Index = -1;
        }

        private Mutex iMutex;
        private bool iOpen;

        private Channel iChannel;
        private int iPresetIndex;

        private IPlaylistSupport iPlaylistSupport;
        private IViewSaveSupport iSaveSupport;

        private PanelBusy iPanelPlaylist;
        private PlaylistWidget<MrItem> iListView;
    }

    class ViewWidgetPlaylistReceiver : IViewWidgetPlaylistReceiver
    {
        public ViewWidgetPlaylistReceiver(FormKinskyDesktop aForm, IArtworkCache aArtworkCache, IDropConverter aDropConverter, IViewSupport aViewSupport, IPlaylistSupport aPlaySupport, IViewSaveSupport aSaveSupport)
        {
            iMutex = new Mutex(false);
            iOpen = false;

            iPlaylistSupport = aPlaySupport;
            iSaveSupport = aSaveSupport;

            iPanelPlaylist = aForm.PanelPlaylist;

            iListView = new PlaylistWidget<MrItem>(aArtworkCache, aViewSupport);
            iListView.ImagePlaying = KinskyDesktop.Properties.Resources.IconPlaying;
            iListView.ImageAdornmentRed = KinskyDesktop.Properties.Resources.AdornmentRed;
            iListView.ImageAdornmentAmber = KinskyDesktop.Properties.Resources.AdornmentAmber;
            iListView.ImageAdornmentGreen = KinskyDesktop.Properties.Resources.AdornmentGreen;
            iListView.AllowDrop = true;
            iListView.BackColor = aViewSupport.BackColour;
            iListView.Dock = DockStyle.Fill;
        }

        public void Open()
        {
            iMutex.WaitOne();

            Assert.Check(!iOpen);

            iPanelPlaylist.StartBusy();
            iPanelPlaylist.SetMessage("Subscribing...");

            iPresetIndex = -1;

            iListView.EventItemActivated += EventItemActivate;
            iListView.ItemDrag += EventItemDrag;

            iOpen = true;
            Trace.WriteLine(Trace.kKinsky, "ViewWidgetPlaylistMultipus Opened");

            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            if (iOpen)
            {
                iListView.EventItemActivated -= EventItemActivate;
                iListView.ItemDrag -= EventItemDrag;

                iPanelPlaylist.StopBusy();
            }

            iPanelPlaylist.BeginInvoke((MethodInvoker)delegate()
            {
                iPanelPlaylist.Controls.Remove(iListView);
                iPanelPlaylist.Invalidate();

                iMutex.WaitOne();

                iListView.Clear();

                iMutex.ReleaseMutex();

                Trace.WriteLine(Trace.kKinsky, "ViewWidgetPlaylistMultipus Closed");
            });

            iOpen = false;

            iMutex.ReleaseMutex();
        }

        public void Initialised()
        {
            iPanelPlaylist.BeginInvoke((MethodInvoker)delegate()
            {
                iPanelPlaylist.Controls.Add(iListView);

                iPanelPlaylist.StopBusy();

                Trace.WriteLine(Trace.kKinsky, "ViewWidgetPlaylistMultipus Initialised");
            });
        }

        public void SetSenders(IList<ModelSender> aSenders)
        {
            iPanelPlaylist.BeginInvoke((MethodInvoker)delegate()
            {
                iMutex.WaitOne();

                iListView.BeginUpdate();

                iListView.Clear();

                for (int i = 0; i < aSenders.Count; ++i)
                {
                    PlaylistWidget<MrItem>.EAdornment adornmentState = PlaylistWidget<MrItem>.EAdornment.eOff;

                    if (aSenders[i].Status == ModelSender.EStatus.eBlocked)
                    {
                        adornmentState = PlaylistWidget<MrItem>.EAdornment.eRed;
                    }
                    else
                    {
                        if (aSenders[i].Audio)
                        {
                            adornmentState = PlaylistWidget<MrItem>.EAdornment.eGreen;
                        }
                        else
                        {
                            adornmentState = PlaylistWidget<MrItem>.EAdornment.eAmber;
                        }
                    }

                    iListView.Add(new MrItem(0, "", aSenders[i].Metadata), adornmentState);
                }

                UpdateChannel(iChannel);

                iListView.EndUpdate();

                iMutex.ReleaseMutex();
            });
        }

        public void SetChannel(Channel aChannel)
        {
            iMutex.WaitOne();

            iListView.BeginUpdate();
            UpdateChannel(aChannel);
            iListView.EndUpdate();

            iMutex.ReleaseMutex();
        }

        private void UpdateChannel(Channel aChannel)
        {
            iMutex.WaitOne();

            int index = -1;
            if (aChannel != null)
            {
                for (int i = 0; i < iListView.Tracks.Count; ++i)
                {
                    if (iListView.Tracks[i].DidlLite[0].Title == aChannel.DidlLite[0].Title)
                    {
                        index = i;
                        break;
                    }
                }
            }

            if (index > -1)
            {
                iListView.SetTrack(iListView.Tracks[index]);
            }
            else
            {
                iListView.SetTrack(null);
            }

            iChannel = aChannel;

            iMutex.ReleaseMutex();
        }

        public void Save()
        {
            List<upnpObject> list = new List<upnpObject>();

            iMutex.WaitOne();
            list.AddRange(iChannel.DidlLite);
            iMutex.ReleaseMutex();

            iSaveSupport.Save(list);
        }

        public event EventHandler<EventArgsSetChannel> EventSetChannel;

        private void EventItemActivate(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            if (iListView.SelectedIndices.Count > 0)
            {
                if (iOpen)
                {
                    MrItem preset = iListView.Tracks[iListView.SelectedIndices[0]];
                    iMutex.ReleaseMutex();

                    List<upnpObject> items = new List<upnpObject>();
                    items.Add(preset.DidlLite[0]);

                    if (EventSetChannel != null)
                    {
                        EventSetChannel(this, new EventArgsSetChannel(new MediaRetrieverNoRetrieve(items)));
                    }
                }
                else
                {
                    iMutex.ReleaseMutex();
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private void EventItemDrag(object sender, ItemDragEventArgs e)
        {
            List<MrItem> presets = new List<MrItem>();
            List<upnpObject> items = new List<upnpObject>();

            iMutex.WaitOne();
            for (int i = 0; i < iListView.SelectedIndices.Count; ++i)
            {
                MrItem preset = iListView.Tracks[iListView.SelectedIndices[i]];
                items.Add(preset.DidlLite[0]);
                presets.Add(preset);
            }
            MediaProviderDraggable draggable = new MediaProviderDraggable(new MediaRetrieverNoRetrieve(items), iListView);
            iMutex.ReleaseMutex();

            DataObject dataObject = new DataObject();
            dataObject.SetData(draggable);
            if (items.Count == 1)
            {
                if (items[0].Res.Count > 0)
                {
                    dataObject.SetData(items[0].Res[0].Uri);
                }
            }

            iPlaylistSupport.SetDragging(true);
            iListView.DoDragDrop(dataObject, DragDropEffects.Copy | DragDropEffects.Scroll);
            iPlaylistSupport.SetDragging(false);

            iListView.InsertionMark.Index = -1;
        }

        private Mutex iMutex;
        private bool iOpen;

        private Channel iChannel;
        private int iPresetIndex;

        private IPlaylistSupport iPlaylistSupport;
        private IViewSaveSupport iSaveSupport;

        private PanelBusy iPanelPlaylist;
        private PlaylistWidget<MrItem> iListView;
    }
    class ViewWidgetPlaylistAux : IViewWidgetPlaylistAux
    {
        public ViewWidgetPlaylistAux(FormKinskyDesktop aForm, IViewSupport aSupport)
        {
            iMutex = new Mutex(false);

            iPanelPlaylist = aForm.PanelPlaylist;

            iPanel = new PanelBusy();
            iPanel.Name = "ViewWidgetPlaylistAux";
            iPanel.BackColor = aSupport.BackColour;
            iPanel.Paint += Panel_Paint;
        }

        public void Open(string aType)
        {
            iMutex.WaitOne();

            iType = aType;

            iPanelPlaylist.BeginInvoke((MethodInvoker)delegate()
            {
                iPanel.Size = iPanelPlaylist.Size;
                iPanel.Dock = DockStyle.Fill;

                iPanelPlaylist.Controls.Add(iPanel);

                Trace.WriteLine(Trace.kKinsky, "ViewWidgetPlaylistAux Opened");
            });

            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            iPanelPlaylist.BeginInvoke((MethodInvoker)delegate()
            {
                iPanelPlaylist.Controls.Remove(iPanel);

                Trace.WriteLine(Trace.kKinsky, "ViewWidgetPlaylistAux Closed");
            });

            iMutex.ReleaseMutex();
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            iMutex.WaitOne();
            string type = iType;
            iMutex.ReleaseMutex();

            Image image = kIconAuxSource;
            if (type == "Analog")
            {
                image = kIconAuxSource;
            }
            else if (type == "Spdif")
            {
                image = kIconSpdifSource;
            }
            else if (type == "Toslink")
            {
                image = kIconTosSource;
            }
            e.Graphics.DrawImage(image, (iPanel.Width - kImageSize) * 0.5f, (iPanel.Height - kImageSize) * 0.5f, kImageSize, kImageSize);
        }

        private const int kImageSize = 128;

        private readonly Image kIconAuxSource = KinskyDesktop.Properties.Resources.IconAuxSource;
        private readonly Image kIconTosSource = KinskyDesktop.Properties.Resources.IconTosLinkSource;
        private readonly Image kIconSpdifSource = KinskyDesktop.Properties.Resources.IconSpdifSource;

        private Mutex iMutex;

        private string iType;
        private PanelBusy iPanel;
        private Panel iPanelPlaylist;
    }

    class ViewWidgetPlaylistDiscPlayer : IViewWidgetPlaylistDiscPlayer
    {
        public ViewWidgetPlaylistDiscPlayer(FormKinskyDesktop aForm, IViewSupport aSupport)
        {
        }

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void Initialised()
        {
        }

        public void Eject()
        {
        }
    }

    class ViewWidgetBreadcrumb : IViewWidgetLocation
    {
        public ViewWidgetBreadcrumb(FormKinskyDesktop aForm, IViewSupport aSupport, IControllerLocation aControllerLocation, IViewWidgetButton aViewWidgetButton)
        {
            iButtonList = new List<Button>();
            iOpen = false;

            iViewSupport = aSupport;
            iViewSupport.EventSupportChanged += EventSupportChanged;

            iPanelBreadcrumb = aForm.PanelBreadcrumb;
            iControllerLocation = aControllerLocation;
            iViewWidgetButton = aViewWidgetButton;
            iViewWidgetButton.EventClick += ClickEvent;
        }

        public void Open()
        {
            Assert.Check(!iOpen);

            iViewWidgetButton.Open();
            iPanelBreadcrumb.SizeChanged += EventPanelBreadcrumb_SizeChanged;

            iOpen = true;
        }

        public void Close()
        {
            if (iOpen)
            {
                iViewWidgetButton.Close();
                iPanelBreadcrumb.SizeChanged -= EventPanelBreadcrumb_SizeChanged;
            }

            iOpen = false;
        }

        public void SetLocation(IList<string> aLocation)
        {
            if (aLocation.Count > 1)
            {
                iViewWidgetButton.Close();
                iViewWidgetButton.Open();
            }
            else
            {
                iViewWidgetButton.Close();
            }

            iPanelBreadcrumb.BeginInvoke((MethodInvoker)delegate()
            {
                int x = 0;
                for (int i = 0; i < aLocation.Count; ++i)
                {
                    string[] split = aLocation[i].Split('\0');
                    string location = split[0];
                    if (i < aLocation.Count - 1)
                    {
                        location += " >";
                    }
                    if (i < iButtonList.Count)
                    {
                        iButtonList[i].Name = "button" + location;
                        iButtonList[i].Location = new Point(x, iButtonList[i].Location.Y);
                        iButtonList[i].Text = location;
                        // keep track of where the next button should be placed, for when we need to add a new button to the control
                        Control[] button = iPanelBreadcrumb.Controls.Find("button" + location, false);
                        if (button.Length == 1)
                        {
                            x += button[0].Width;
                        }
                    }
                    else
                    {
                        Button button = CreateButton(location);
                        button.Location = new Point(x, (int)((iPanelBreadcrumb.Height - button.Height) * 0.5f));
                        button.Tag = i.ToString();
                        button.Click += EventButtonBreadcrumb_Click;
                        button.MouseEnter += EventButtonBreadcrumb_MouseEnter;
                        button.MouseLeave += EventButtonBreadcrumb_MouseLeave;

                        iButtonList.Add(button);
                        iPanelBreadcrumb.Controls.Add(button);

                        x += button.Width;
                    }
                }

                iPanelBreadcrumb.SuspendLayout();

                int count = iButtonList.Count - aLocation.Count;
                for (int i = 0; i < count; ++i)
                {
                    Button button = iButtonList[aLocation.Count];
                    button.Click -= EventButtonBreadcrumb_Click;
                    button.MouseEnter -= EventButtonBreadcrumb_MouseEnter;
                    button.MouseLeave -= EventButtonBreadcrumb_MouseLeave;

                    iButtonList.Remove(button);
                    if (iPanelBreadcrumb.Controls.Contains(button))
                    {
                        iPanelBreadcrumb.Controls.Remove(button);
                    }
                }

                iPanelBreadcrumb.ResumeLayout(true);

                FitBreadcrumbToPanelSize();
            });
        }

        private void ClickEvent(object sender, EventArgs e)
        {
            iControllerLocation.Up(1);
        }

        private Button CreateButton(string aText)
        {
            Button button = new Button();
            button.Name = "button" + aText;
            button.Text = aText;
            button.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            button.AutoSize = true;
            button.ForeColor = iViewSupport.ForeColourMuted;
            button.Font = iViewSupport.FontSmall;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.Transparent;
            button.FlatAppearance.MouseDownBackColor = Color.Transparent;
            button.TextAlign = ContentAlignment.MiddleCenter;
            button.AllowDrop = true;

            return button;
        }

        private void EventPanelBreadcrumb_SizeChanged(object sender, EventArgs e)
        {
            FitBreadcrumbToPanelSize();
        }

        private void FitBreadcrumbToPanelSize()
        {
            int width = 0;
            foreach (Button b in iButtonList)
            {
                width += b.Width;
            }

            int startIndex = 0;
            if (width > iPanelBreadcrumb.Width)
            {
                int over = width - iPanelBreadcrumb.Width;
                while (over > 0)
                {
                    over -= iButtonList[startIndex].Width;
                    startIndex++;
                }
            }

            if (iPanelBreadcrumb.Controls.Count > 0 && startIndex < iButtonList.Count)
            {
                if (iPanelBreadcrumb.Controls[0] == iButtonList[startIndex])
                {
                    return;
                }
            }

            iPanelBreadcrumb.SuspendLayout();

            iPanelBreadcrumb.Controls.Clear();

            int x = 0;
            for (int i = startIndex; i < iButtonList.Count; ++i)
            {
                Button button = iButtonList[i];
                button.Location = new Point(x, button.Location.Y);
                iPanelBreadcrumb.Controls.Add(button);
                x += button.Width;
            }

            iPanelBreadcrumb.ResumeLayout(true);
        }

        private void EventButtonBreadcrumb_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            int index = int.Parse(button.Tag as string);

            int upCount = iButtonList.Count - index - 1;

            if (upCount > 0)
            {
                iControllerLocation.Up((uint)upCount);
            }

            //iListViewBrowser.Focus();
        }

        private void EventButtonBreadcrumb_MouseEnter(object sender, EventArgs e)
        {
            Button button = sender as Button;
            button.ForeColor = iViewSupport.ForeColourBright;
        }

        private void EventButtonBreadcrumb_MouseLeave(object sender, EventArgs e)
        {
            Button button = sender as Button;
            button.ForeColor = iViewSupport.ForeColourMuted;
        }

        private void EventSupportChanged(object sender, EventArgs e)
        {
            int x = 0;
            foreach (Button b in iButtonList)
            {
                b.Font = iViewSupport.FontSmall;
                b.ForeColor = iViewSupport.ForeColourMuted;
                b.Location = new Point(x, b.Location.Y);
                x += b.Width;
            }

            FitBreadcrumbToPanelSize();
        }

        private bool iOpen;
        private List<Button> iButtonList;

        private IViewSupport iViewSupport;
        private Panel iPanelBreadcrumb;
        private IViewWidgetButton iViewWidgetButton;
        private IControllerLocation iControllerLocation;
    }

    class ViewWidgetButtonSize : ViewWidgetButton
    {
        public ViewWidgetButtonSize(FormKinskyDesktop aForm)
            : base(aForm.ButtonSize, aForm.ToolTip, "Cycle through browser sizes")
        {
        }
    }

    class ViewWidgetButtonView : ViewWidgetButton
    {
        public ViewWidgetButtonView(FormKinskyDesktop aForm)
            : base(aForm.ButtonView, aForm.ToolTip, "Cycle through browser views")
        {
        }
    }

    class ViewWidgetButtonUp : ViewWidgetButton
    {
        public ViewWidgetButtonUp(FormKinskyDesktop aForm)
            : base(aForm.ButtonUp, aForm.ToolTip, "Move up a directory")
        {
        }
    }

    class ViewWidgetTabPage : IViewWidgetLocation
    {
        public ViewWidgetTabPage(FormKinskyDesktop aForm, uint aTabPageIndex)
        {
            iTabControl = aForm.TabControl;
            iTabPageIndex = aTabPageIndex;
        }

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void SetLocation(IList<string> aLocation)
        {
            iTabControl.SetTabLabel((int)iTabPageIndex, aLocation[aLocation.Count - 1]);
        }

        private uint iTabPageIndex;
        private KinskyDesktop.Widgets.TabControl iTabControl;
    }

    class MediatorTab : IViewWidgetLocation
    {
        public MediatorTab(FormKinskyDesktop aForm, IControllerLocation aControllerLocation)
        {
            iControllerLocation = aControllerLocation;

            iTabControl = aForm.TabControl;
            iTabControl.EventSelectedIndexChanged += SelectedIndexChanged;

            iMutex = new Mutex(false);
            iOpen = false;

            iViewWidgetLocationList = new List<IViewWidgetLocation>();
            iViewWidgetContentList = new List<IViewWidgetContent>();
            iBrowserList = new List<IBrowser>();
        }

        public void Open()
        {
            iMutex.WaitOne();

            int index = iTabControl.SelectedTabPageIndex;
            for (int i = 0; i < iBrowserList.Count; ++i)
            {
                iViewWidgetLocationList[i].Open();
                iViewWidgetContentList[i].Close();
            }

            if (index > -1 && index < iBrowserList.Count)
            {
                iViewWidgetContentList[index].Open();
            }

            iOpen = true;

            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            for (int i = 0; i < iBrowserList.Count; ++i)
            {
                iViewWidgetLocationList[i].Close();
                iViewWidgetContentList[i].Close();
            }

            iOpen = false;

            iMutex.ReleaseMutex();
        }

        public void Add(IViewWidgetLocation aViewWidgetLocation, IViewWidgetContent aViewWidgetContent, IBrowser aBrowser)
        {
            iMutex.WaitOne();

            iViewWidgetLocationList.Add(aViewWidgetLocation);
            iViewWidgetContentList.Add(aViewWidgetContent);
            iBrowserList.Add(aBrowser);
            iTabControl.InsertTabPage((uint)(iBrowserList.Count - 1), aBrowser.Location.Current.Metadata.Title);

            aViewWidgetLocation.Open();

            aViewWidgetContent.Close();

            int index = iTabControl.SelectedTabPageIndex;
            if (index == (iBrowserList.Count - 1) && iOpen)
            {
                aViewWidgetContent.Open();
            }

            if (iBrowserList.Count == 1)
            {
                if (iTabControl.SelectedTabPageIndex == -1)
                {
                    iTabControl.SelectedTabPageIndex = 0;
                }
            }

            iMutex.ReleaseMutex();
        }

        public void Remove(uint aIndex)
        {
            iMutex.WaitOne();

            iViewWidgetLocationList.RemoveAt((int)aIndex);
            iViewWidgetContentList.RemoveAt((int)aIndex);
            iBrowserList.RemoveAt((int)aIndex);

            iMutex.ReleaseMutex();
        }

        public void SetLocation(IList<string> aLocation)
        {
            iMutex.WaitOne();

            int index = iTabControl.SelectedTabPageIndex;
            iViewWidgetLocationList[index].SetLocation(aLocation);

            iMutex.ReleaseMutex();
        }

        private void SelectedIndexChanged(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            int index = iTabControl.SelectedTabPageIndex;
            iControllerLocation.SetBrowser(iBrowserList[index]);
            for (int i = 0; i < iViewWidgetContentList.Count; ++i)
            {
                iViewWidgetContentList[i].Close();
            }
            if (index > -1 && index < iBrowserList.Count && iOpen)
            {
                iViewWidgetContentList[index].Open();
            }

            iMutex.ReleaseMutex();
        }

        private Mutex iMutex;
        private bool iOpen;

        private List<IViewWidgetLocation> iViewWidgetLocationList;
        private List<IViewWidgetContent> iViewWidgetContentList;
        private List<IBrowser> iBrowserList;

        private IControllerLocation iControllerLocation;

        private KinskyDesktop.Widgets.TabControl iTabControl;
    }

    abstract class ViewSysTrayContextMenu : IViewWidgetTransportControl, IViewWidgetVolumeControl, IViewWidgetPlayMode
    {
        public ViewSysTrayContextMenu(FormKinskyDesktop aForm)
        {
            iMutex = new Mutex(false);
            iOpen = false;

            iContextMenu = aForm.ContextMenuStripTray;

            iMenuItemShowHide = aForm.ShowToolStripMenuItem;

            iMenuItemPlayPauseStop = aForm.PlayToolStripMenuItem;
            iMenuItemNext = aForm.NextToolStripMenuItem;
            iMenuItemPrevious = aForm.PreviousToolStripMenuItem;

            iMenuItemShuffle = aForm.ShuffleToolStripMenuItem;
            iMenuItemRepeat = aForm.RepeatToolStripMenuItem;

            iMenuItemMute = aForm.MuteToolStripMenuItem;

            iForm = aForm;
            iForm.SizeChanged += SizeChanged;
        }

        public void Open() { }

        void IViewWidgetTransportControl.Open()
        {
            iMutex.WaitOne();

            iMenuItemShowHide.Click += ShowHideClick;

            iMutex.ReleaseMutex();
        }

        void IViewWidgetTransportControl.Close()
        {
            iMutex.WaitOne();

            iMenuItemShowHide.Click -= ShowHideClick;

            iMenuItemPlayPauseStop.Click -= PlayClick;
            iMenuItemNext.Click -= NextClick;
            iMenuItemPrevious.Click -= PreviousClick;

            OnCloseTransportControl();

            iMutex.ReleaseMutex();
        }

        void IViewWidgetVolumeControl.Close()
        {
            iMutex.WaitOne();

            iMenuItemMute.Click -= MuteClick;

            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iMenuItemMute.Enabled = false;
            });

            iMutex.ReleaseMutex();
        }

        void IViewWidgetPlayMode.Close()
        {
            iMutex.WaitOne();

            iMenuItemRepeat.Click -= RepeatClick;
            iMenuItemShuffle.Click -= ShuffleClick;

            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iMenuItemRepeat.Enabled = false;
                iMenuItemShuffle.Enabled = false;
            });

            iMutex.ReleaseMutex();
        }

        void IViewWidgetTransportControl.Initialised()
        {
            iMutex.WaitOne();

            iMenuItemPlayPauseStop.Click += PlayClick;
            iMenuItemNext.Click += NextClick;
            iMenuItemPrevious.Click += PreviousClick;

            OnInitialisedTransportControl();

            iMutex.ReleaseMutex();
        }

        void IViewWidgetVolumeControl.Initialised()
        {
            iMutex.WaitOne();

            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iMenuItemMute.Enabled = true;
            });
            iMenuItemMute.Click += MuteClick;

            iMutex.ReleaseMutex();
        }

        void IViewWidgetPlayMode.Initialised()
        {
            iMutex.WaitOne();

            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iMenuItemRepeat.Enabled = true;
                iMenuItemShuffle.Enabled = true;
            });

            iMenuItemRepeat.Click += RepeatClick;
            iMenuItemShuffle.Click += ShuffleClick;

            iMutex.ReleaseMutex();
        }

        public void SetPlayNowEnabled(bool aEnabled) { }
        public void SetPlayNextEnabled(bool aEnabled) { }
        public void SetPlayLaterEnabled(bool aEnabled) { }

        public void SetDragging(bool aDragging) { }

        public void SetAllowSkipping(bool aAllowSkipping) { }
        public void SetAllowPausing(bool aAllowPausing) { }

        public void SetTransportState(ETransportState aTransportState)
        {
            iMutex.WaitOne();

            iTransportState = aTransportState;

            OnSetTransportState(aTransportState);

            iMutex.ReleaseMutex();
        }

        public void SetDuration(uint aDuration)
        {
        }

        public void SetVolume(uint aVolume) { }

        public void SetMute(bool aMute)
        {
            iMutex.WaitOne();

            iMute = aMute;

            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iMenuItemMute.CheckState = aMute ? CheckState.Checked : CheckState.Unchecked;
            });

            iMutex.ReleaseMutex();
        }

        public void SetVolumeLimit(uint aVolumeLimit) { }

        public void SetShuffle(bool aShuffle)
        {
            iMutex.WaitOne();

            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iMenuItemShuffle.CheckState = aShuffle ? CheckState.Checked : CheckState.Unchecked;
            });

            iMutex.ReleaseMutex();
        }

        public void SetRepeat(bool aRepeat)
        {
            iMutex.WaitOne();

            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iMenuItemRepeat.CheckState = aRepeat ? CheckState.Checked : CheckState.Unchecked;
            });

            iMutex.ReleaseMutex();
        }

        public abstract event EventHandler<EventArgs> EventPause;
        public abstract event EventHandler<EventArgs> EventPlay;
        public abstract event EventHandler<EventArgs> EventStop;

        public event EventHandler<EventArgs> EventPrevious;
        public event EventHandler<EventArgs> EventNext;

        public event EventHandler<EventArgsPlay> EventPlayNow { add { } remove { } }
        public event EventHandler<EventArgsPlay> EventPlayNext { add { } remove { } }
        public event EventHandler<EventArgsPlay> EventPlayLater { add { } remove { } }

        public event EventHandler<EventArgs> EventVolumeIncrement { add { } remove { } }
        public event EventHandler<EventArgs> EventVolumeDecrement { add { } remove { } }
        public event EventHandler<EventArgsVolume> EventVolumeChanged { add { } remove { } }
        public event EventHandler<EventArgsMute> EventMuteChanged;

        public event EventHandler<EventArgs> EventToggleShuffle;
        public event EventHandler<EventArgs> EventToggleRepeat;

        protected abstract void OnCloseTransportControl();
        protected abstract void OnInitialisedTransportControl();
        protected abstract void OnSetTransportState(ETransportState aTransportState);
        protected abstract void OnPlayPauseStopClick();

        protected void PlayClick(object sender, EventArgs e)
        {
            OnPlayPauseStopClick();
        }

        protected void NextClick(object sender, EventArgs e)
        {
            if (EventNext != null)
            {
                EventNext(this, EventArgs.Empty);
            }
        }

        protected void PreviousClick(object sender, EventArgs e)
        {
            if (EventPrevious != null)
            {
                EventPrevious(this, EventArgs.Empty);
            }
        }

        private void MuteClick(object sender, EventArgs e)
        {
            if (EventMuteChanged != null)
            {
                EventMuteChanged(this, new EventArgsMute(!iMute));
            }
        }

        private void ShuffleClick(object sender, EventArgs e)
        {
            if (EventToggleShuffle != null)
            {
                EventToggleShuffle(this, EventArgs.Empty);
            }
        }

        private void RepeatClick(object sender, EventArgs e)
        {
            if (EventToggleRepeat != null)
            {
                EventToggleRepeat(this, EventArgs.Empty);
            }
        }

        private void ShowHideClick(object sender, EventArgs e)
        {
            if (iForm.WindowState == FormWindowState.Minimized)
            {
                iForm.WindowState = FormWindowState.Normal;
            }
            else
            {
                iForm.WindowState = FormWindowState.Minimized;
            }
        }

        private void SizeChanged(object sender, EventArgs e)
        {
            iMutex.WaitOne();

            iMenuItemShowHide.Text = (iForm.WindowState == FormWindowState.Minimized) ? "Show Kinsky" : "Hide Kinsky";

            iMutex.ReleaseMutex();
        }

        protected readonly Image kImageSysTrayPlay = KinskyDesktop.Properties.Resources.SysTrayPlay;
        protected readonly Image kImageSysTrayPause = KinskyDesktop.Properties.Resources.SysTrayPause;
        protected readonly Image kImageSysTrayStop = KinskyDesktop.Properties.Resources.SysTrayStop;

        protected Mutex iMutex;
        protected bool iOpen;

        protected Form iForm;

        protected bool iMute;
        protected ETransportState iTransportState;

        protected ContextMenuStrip iContextMenu;

        protected ToolStripMenuItem iMenuItemPlayPauseStop;
        protected ToolStripMenuItem iMenuItemPrevious;
        protected ToolStripMenuItem iMenuItemNext;

        protected ToolStripMenuItem iMenuItemRepeat;
        protected ToolStripMenuItem iMenuItemShuffle;

        protected ToolStripMenuItem iMenuItemMute;

        private ToolStripMenuItem iMenuItemShowHide;
    }

    class ViewSysTrayContextMenuPause : ViewSysTrayContextMenu
    {
        public ViewSysTrayContextMenuPause(FormKinskyDesktop aForm)
            : base(aForm)
        {
        }

        public override event EventHandler<EventArgs> EventPause;
        public override event EventHandler<EventArgs> EventPlay;
        public override event EventHandler<EventArgs> EventStop { add { } remove { } }

        protected override void OnCloseTransportControl()
        {
            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iMenuItemPlayPauseStop.Enabled = false;
                iMenuItemNext.Enabled = false;
                iMenuItemPrevious.Enabled = false;
            });
        }

        protected override void OnInitialisedTransportControl()
        {
            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iMenuItemPlayPauseStop.Enabled = true;
                iMenuItemNext.Enabled = true;
                iMenuItemPrevious.Enabled = true;
            });
        }

        protected override void OnSetTransportState(ETransportState aTransportState)
        {
            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                switch (aTransportState)
                {
                    case ETransportState.ePlaying:
                    case ETransportState.eBuffering:
                        iMenuItemPlayPauseStop.Text = "Pause";
                        iMenuItemPlayPauseStop.Image = kImageSysTrayPause;
                        break;

                    default:
                        iMenuItemPlayPauseStop.Text = "Play";
                        iMenuItemPlayPauseStop.Image = kImageSysTrayPlay;
                        break;
                }
            });
        }

        protected override void OnPlayPauseStopClick()
        {
            iMutex.WaitOne();

            switch (iTransportState)
            {
                case ETransportState.eBuffering:
                case ETransportState.ePlaying:
                    iMutex.ReleaseMutex();

                    if (EventPause != null)
                    {
                        EventPause(this, EventArgs.Empty);
                    }
                    break;

                default:
                    iMutex.ReleaseMutex();

                    if (EventPlay != null)
                    {
                        EventPlay(this, EventArgs.Empty);
                    }
                    break;
            }
        }
    }

    class ViewSysTrayContextMenuStop : ViewSysTrayContextMenu
    {
        public ViewSysTrayContextMenuStop(FormKinskyDesktop aForm)
            : base(aForm)
        {
        }

        public override event EventHandler<EventArgs> EventPause { add { } remove { } }
        public override event EventHandler<EventArgs> EventPlay;
        public override event EventHandler<EventArgs> EventStop;

        protected override void OnCloseTransportControl()
        {
            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iMenuItemPlayPauseStop.Enabled = false;
                iMenuItemNext.Enabled = false;
                iMenuItemPrevious.Enabled = false;
            });
        }

        protected override void OnInitialisedTransportControl()
        {
            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iMenuItemPlayPauseStop.Enabled = true;
                iMenuItemNext.Enabled = true;
                iMenuItemPrevious.Enabled = true;
            });
        }

        protected override void OnSetTransportState(ETransportState aTransportState)
        {
            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                switch (aTransportState)
                {
                    case ETransportState.eBuffering:
                    case ETransportState.ePlaying:
                        iMenuItemPlayPauseStop.Text = "Stop";
                        iMenuItemPlayPauseStop.Image = kImageSysTrayStop;
                        break;

                    default:
                        iMenuItemPlayPauseStop.Text = "Play";
                        iMenuItemPlayPauseStop.Image = kImageSysTrayPlay;
                        break;
                }
            });
        }

        protected override void OnPlayPauseStopClick()
        {
            iMutex.WaitOne();

            switch (iTransportState)
            {
                case ETransportState.eBuffering:
                case ETransportState.ePlaying:
                    iMutex.ReleaseMutex();

                    if (EventStop != null)
                    {
                        EventStop(this, EventArgs.Empty);
                    }
                    break;

                default:
                    iMutex.ReleaseMutex();

                    if (EventPlay != null)
                    {
                        EventPlay(this, EventArgs.Empty);
                    }
                    break;
            }
        }
    }

    class ViewSysTrayContextMenuStopNoSeek : ViewSysTrayContextMenuStop
    {
        public ViewSysTrayContextMenuStopNoSeek(FormKinskyDesktop aForm)
            : base(aForm)
        {
        }

        protected override void OnCloseTransportControl()
        {
            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iMenuItemPlayPauseStop.Enabled = false;
            });
        }

        protected override void OnInitialisedTransportControl()
        {
            iForm.BeginInvoke((MethodInvoker)delegate()
            {
                iMenuItemPlayPauseStop.Enabled = true;
                iMenuItemNext.Enabled = false;
                iMenuItemPrevious.Enabled = false;
            });
        }
    }
}
