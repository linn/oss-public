using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Linn.Control.Upnp.ControlPoint;
using System.Threading;
using Linn.Topology;
using Linn.Gui.Scenegraph;

namespace MediaServerBrowser
{
    public partial class iForm : Form
    {
        private ControlPoint iMediaServerCp = null;
        private Mutex iMutex = null;
        private ModelMediaServer iModelMediaServer = null;
        private List<ListViewItem> iMediaServerList = new List<ListViewItem>();

        public iForm()
        {
            InitializeComponent();

            iMutex = new Mutex(false);

            iMediaServerCp = new ControlPoint("urn:schemas-upnp-org:device:MediaServer", 1);
            iMediaServerCp.AddObserver(EventMediaServerAlive, EventMediaServerByeBye);
            iMediaServerCp.Start();
            iMediaServerCp.Discover(false);
        }

        delegate void EventMediaServer(Device aDevice);
        private void EventMediaServerAlive(Device aDevice) {
            if (InvokeRequired)
            {
                EventMediaServer d = new EventMediaServer(EventMediaServerAlive);
                BeginInvoke(d, new object[] { aDevice });
            }
            else
            {
                ListViewItem item = new ListViewItem();
                ModelMediaServer server = new ModelMediaServer(iMediaServerCp, aDevice);
                item.Text = server.Name;
                item.Tag = server;
                iMutex.WaitOne();
                iListView.Items.Add(item);
                iMediaServerList.Add(item);
                iMutex.ReleaseMutex();
            }
        }

        private void EventMediaServerByeBye(Device aDevice) {
            if (InvokeRequired)
            {
                EventMediaServer d = new EventMediaServer(EventMediaServerByeBye);
                BeginInvoke(d, new object[] { aDevice });
            }
            else
            {
                iMutex.WaitOne();
                for(int i = 0; i < iListView.Items.Count; ++i) {
                    ModelMediaServer server = iListView.Items[i].Tag as ModelMediaServer;
                    if(server.Device == aDevice) {
                        iListView.Items.RemoveAt(i);
                        iMediaServerList.RemoveAt(i);
                        break;
                    }
                }
                iMutex.ReleaseMutex();
            }
        }

        delegate void EventDirectoryCallback();
        private void EventDirectory()
        {
            if (InvokeRequired)
            {
                EventDirectoryCallback d = new EventDirectoryCallback(EventDirectory);
                BeginInvoke(d, new object[] {});
            }
            else
            {
                iListView.Items.Clear();
                if (iModelMediaServer.DirInfo != null)
                {
                    for (uint i = 0; i < iModelMediaServer.ListEntryProviderL2.Count; ++i)
                    {
                        ListableUpnpObject listable = iModelMediaServer.ListEntryProviderL2.Entries(i, 1)[0] as ListableUpnpObject;
                        if (listable != null)
                        {
                            UpnpObject upnpObject = listable.Object;
                            ListViewItem item = new ListViewItem();
                            item.Text = upnpObject.Title;
                            item.Tag = listable;
                            iListView.Items.Add(item);
                        }
                    }
                }
                else
                {
                    iModelMediaServer.UnSubscribe();
                }
            }
        }

        private void EventSubscribed(object aSender)
        {
            EventDirectory();
        }

        private void EventUnSubscribed(object aSender)
        {
            iModelMediaServer.EEventDirectory -= EventDirectory;
            iModelMediaServer.EEventSubscribed -= EventSubscribed;
            iModelMediaServer.EEventUnSubscribed -= EventUnSubscribed;
            iModelMediaServer = null;

            iMutex.WaitOne();
            foreach (ListViewItem item in iMediaServerList)
            {
                iListView.Items.Add(item);
            }
            iMutex.ReleaseMutex();
        }

        private void iButtonBack_Click(object sender, EventArgs e)
        {
            if (iModelMediaServer != null)
            {
                iModelMediaServer.UpDirectory();
            }
        }

        private void iListView_ItemActivate(object sender, EventArgs e)
        {
            if (iListView.SelectedIndices.Count == 1)
            {
                ModelMediaServer server = iListView.Items[iListView.SelectedIndices[0]].Tag as ModelMediaServer;
                if (server != null)
                {
                    iModelMediaServer = server;
                    server.EEventDirectory += EventDirectory;
                    server.EEventSubscribed += EventSubscribed;
                    server.EEventUnSubscribed += EventUnSubscribed;
                    server.Subscribe();
                }
                ListableUpnpObject listable = iListView.Items[iListView.SelectedIndices[0]].Tag as ListableUpnpObject;
                if (listable != null)
                {
                    iModelMediaServer.DownDirectory(iListView.SelectedIndices[0], listable);
                }
            }
        }
    }
}