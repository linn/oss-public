
using System;
using System.Net;
using System.Collections.Generic;
using System.Windows.Forms;

using Linn;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.Topology;
using Linn.Toolkit.WinForms;

namespace LinnTopology
{
    internal class App : IStack
    {
        public App(Helper aHelper, Form1 aForm)
        {
            iForm = aForm;
            iForm.Load += this.OnLoad;
            iForm.Closed += this.OnClosed;
            iForm.ListViewRoom.SelectedIndexChanged += this.EventRoomSelectedIndexChanged;
            iForm.ListViewSource.SelectedIndexChanged += this.EventSourceSelectedIndexChanged;
            iForm.MenuItemAbout.Click += this.MenuItemAboutClick;
            iForm.MenuItemDebug.Click += this.MenuItemDebugClick;
            iForm.MenuItemOptions.Click += this.MenuItemOptionsClick;

            iHelper = aHelper;
            iHelper.Stack.SetStack(this);
            iHelper.Stack.EventStatusChanged += StackStatusChanged;

            iEventServer = new EventServerUpnp();
            iListenerNotify = new SsdpListenerMulticast();

            iHouse = new House(iListenerNotify, iEventServer, new ModelFactory());
            iHouse.EventRoomAdded += RoomAdded;
            iHouse.EventRoomRemoved += RoomRemoved;

            iLibrary = new Library(iListenerNotify);
            iLibrary.EventMediaServerAdded += LibraryAdded;
            iLibrary.EventMediaServerRemoved += LibraryRemoved;
        }

        void IStack.Start(IPAddress aIpAddress)
        {
            iEventServer.Start(aIpAddress);
            iListenerNotify.Start(aIpAddress);
            iHouse.Start(aIpAddress);
            iLibrary.Start(aIpAddress);
        }

        void IStack.Stop()
        {
            iLibrary.Stop();
            iHouse.Stop();
            iListenerNotify.Stop();
            iEventServer.Stop();
        }

        private void StackStatusChanged(object sender, EventArgsStackStatus e)
        {
        }

        private void OnLoad(object sender, EventArgs e)
        {
            iHelper.Stack.Start();

            StackStatus status = iHelper.Stack.Status;
            if (status.State != EStackState.eOk)
            {
                ShowOptionsDialog();
            }
        }

        private void OnClosed(object sender, EventArgs e)
        {
            iHelper.Stack.Stop();
        }

        public void RoomAdded(object obj, EventArgsRoom e)
        {
            ReplaceListViewRooms(iHouse.RoomList);
        }

        public void RoomRemoved(object obj, EventArgsRoom e)
        {
            ReplaceListViewRooms(iHouse.RoomList);
        }

        public void LibraryAdded(object sender, Library.EventArgsMediaServer e)
        {
            AddListViewLibraries(e.MediaServer);
        }

        public void LibraryRemoved(object sender, Library.EventArgsMediaServer e)
        {
            RemoveListViewLibraries(e.MediaServer);
        }

        private delegate void AddListViewLibrariesDelegate(MediaServer aServer);

        public void AddListViewLibraries(MediaServer aServer)
        {
            if (iForm.InvokeRequired)
            {
                iForm.BeginInvoke(new AddListViewLibrariesDelegate(AddListViewLibraries), new object[] { aServer });
            }
            else
            {
                //iForm.ListViewLibrary.BeginUpdate();

                bool added = false;
                string name = aServer.Name;
                if (name != null)
                {
                    ListViewItem item = new ListViewItem(name);
                    for (int i = 0; i < iForm.ListViewLibrary.Items.Count; ++i)
                    {
                        if (item.Text.CompareTo(iForm.ListViewLibrary.Items[i].Text) < 0)
                        {
                            iForm.ListViewLibrary.Items.Insert(i, item);
                            added = true;
                            break;
                        }
                    }
                    if (!added)
                    {
                        iForm.ListViewLibrary.Items.Add(item);
                    }
                }
                else
                {
                    Console.WriteLine("No name for server " + aServer);
                }

                //iForm.ListViewLibrary.EndUpdate();
            }
        }

        private delegate void RemoveListViewLibrariesDelegate(MediaServer aServer);

        public void RemoveListViewLibraries(MediaServer aServer)
        {
            if (iForm.InvokeRequired)
            {
                iForm.BeginInvoke(new RemoveListViewLibrariesDelegate(RemoveListViewLibraries), new object[] { aServer });
            }
            else
            {
                //iForm.ListViewLibrary.BeginUpdate();

                foreach (ListViewItem item in iForm.ListViewLibrary.Items)
                {
                    if (item.Text == aServer.Name)
                    {
                        iForm.ListViewLibrary.Items.Remove(item);
                        break;
                    }
                }

                //iForm.ListViewLibrary.EndUpdate();
            }
        }

        private delegate void ReplaceListViewRoomsDelegate(IList<Room> aRoomList);

        public void ReplaceListViewRooms(IList<Room> aRoomList)
        {
            if (iForm.InvokeRequired)
            {
                iForm.BeginInvoke(new ReplaceListViewRoomsDelegate(ReplaceListViewRooms), new object[] { aRoomList });
            }
            else
            {
                iForm.ListViewRoom.BeginUpdate();

                iForm.ListViewRoom.Items.Clear();

                foreach (Room room in aRoomList)
                {
                    ListViewItem listViewItem = new ListViewItem(room.Name);
                    iForm.ListViewRoom.Items.Add(listViewItem);
                }

                iForm.ListViewRoom.EndUpdate();
            }
        }

        private void EventRoomSelectedIndexChanged(object sender, EventArgs e)
        {
            if (iRoom != null)
            {
                iRoom.EventSourceAdded -= EventSourcesChanged;
                iRoom.EventSourceRemoved -= EventSourcesChanged;
            }

            iRoom = null;

            int count = iForm.ListViewRoom.SelectedIndices.Count;

            if (count > 0)
            {
                try
                {
                    iRoom = iHouse.RoomList[iForm.ListViewRoom.SelectedIndices[0]];
                    iRoom.EventSourceAdded += EventSourcesChanged;
                    iRoom.EventSourceRemoved += EventSourcesChanged;
                }
                catch (IndexOutOfRangeException)
                {
                }
            }

            UpdateSourceList();
        }

        private void EventSourceSelectedIndexChanged(object sender, EventArgs e)
        {
            if (iRoom != null)
            {
                int count = iForm.ListViewSource.SelectedIndices.Count;

                if (count > 0)
                {
                    try
                    {
                        Source source = iRoom.SourceList[iForm.ListViewSource.SelectedIndices[0]];
                        source.Select();
                    }
                    catch (IndexOutOfRangeException)
                    {
                    }
                }
            }
        }

        private void EventSourcesChanged(object obj, EventArgs e)
        {
            UpdateSourceList();
        }

        private delegate void UpdateSourceListDelegate();

        private void UpdateSourceList()
        {
            if (iForm.InvokeRequired)
            {
                iForm.BeginInvoke(new UpdateSourceListDelegate(UpdateSourceList), new object[] { });
            }
            else
            {
                iForm.ListViewSource.BeginUpdate();

                iForm.ListViewSource.Clear();

                if (iRoom != null)
                {
                    IList<Source> sources = iRoom.SourceList;

                    foreach (Source source in sources)
                    {
                        ListViewItem item = new ListViewItem(source.FullName + "   [" + source.Type + "]");
                        iForm.ListViewSource.Items.Add(item);
                    }
                }

                iForm.ListViewSource.EndUpdate();
            }
        }

        private void ShowOptionsDialog()
        {
            FormUserOptions options = new FormUserOptions(iHelper.OptionPages);
            options.Icon = iForm.Icon;
            options.ShowDialog();
            options.Dispose();
        }

        private void MenuItemOptionsClick(object sender, EventArgs e)
        {
            ShowOptionsDialog();
        }

        private void MenuItemDebugClick(object sender, EventArgs e)
        {
        }

        private void MenuItemAboutClick(object sender, EventArgs e)
        {
            FormAboutBox aboutBox = new FormAboutBox(iHelper);
            aboutBox.ShowDialog();
            aboutBox.Dispose();
        }

        private Helper iHelper;
        private EventServerUpnp iEventServer;
        private SsdpListenerMulticast iListenerNotify;
        private House iHouse;
        private Library iLibrary;
        private Room iRoom;
        private Form1 iForm;
    }
}


