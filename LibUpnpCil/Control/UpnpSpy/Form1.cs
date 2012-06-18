using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Net;
using System.Windows.Forms;
using System.Threading;

using Linn;
using Linn.Control;
using Linn.Control.Ssdp;
using Linn.Toolkit.WinForms;

namespace UpnpSpy
{
    public partial class Form1 : Form, IStack
    {
        public Form1(Helper aHelper)
        {
            InitializeComponent();

            iDevices = new Dictionary<string,Device>();
            iMutex = new Mutex();

            iListenerMulticast = new SsdpListenerMulticast();
            iListenerMulticast.Add(new MySsdpNotifyHandler(this, true));

            iListenerUnicast = new SsdpListenerUnicast(new MySsdpNotifyHandler(this, false));

            iHelper = aHelper;
            iHelper.Stack.SetStack(this);
        }

        void IStack.Start(IPAddress aIpAddress)
        {
            iListenerMulticast.Start(aIpAddress);
            iListenerUnicast.Start(aIpAddress);
            iListenerUnicast.SsdpMsearchAll(5);
        }

        void IStack.Stop()
        {
            iListenerMulticast.Stop();
            iListenerUnicast.Stop();
        }

        public void AddSsdpItem(string aUuid, ListViewItem aItem)
        {
            Device device;

            iDevices.TryGetValue(aUuid, out device);

            if (device == null)
            {
                device = new Device(aUuid);
                device.Add(aItem);
                iMutex.WaitOne();
                iDevices.Add(aUuid, device);
                iMutex.ReleaseMutex();
                AddListBoxDevicesItem(aUuid);
            }
            else
            {
                iMutex.WaitOne();
                device.Add(aItem);
                iMutex.ReleaseMutex();
//              AddListViewSsdpItem(aUuid, aItem);
                AddListViewSsdpItem(aUuid);
            }
        }

        private delegate void AddListBoxDevicesItemDelegate(string aUuid);

        public void AddListBoxDevicesItem(string aUuid)
        {
            if (listBoxDevices.InvokeRequired)
            {
                // This is a non UI thread so delegate the task.
                listBoxDevices.Invoke(new AddListBoxDevicesItemDelegate(AddListBoxDevicesItem), aUuid);
            }
            else
            {
                // This is the UI thread so perform the task.
                iMutex.WaitOne();
                listBoxDevices.Items.Add(aUuid);
                iMutex.ReleaseMutex();
            }
        }

        private delegate void AddListViewSsdpItemDelegate(string aUuid);

        public void AddListViewSsdpItem(string aUuid)
        {
            if (listViewSsdp.InvokeRequired)
            {
                // This is a non UI thread so delegate the task.
                listViewSsdp.Invoke(new AddListViewSsdpItemDelegate(AddListViewSsdpItem), aUuid);
            }
            else
            {
                // This is the UI thread so perform the task.
                iMutex.WaitOne();
                if (listBoxDevices.SelectedItem != null)
                {
                    if (listBoxDevices.SelectedItem.ToString() == aUuid)
                    {
                        Device device = iDevices[aUuid];

                        if (device.ListViewItems().Count != listViewSsdp.Items.Count)
                        {
                            listViewSsdp.Items.Clear();
                            foreach (ListViewItem item in device.ListViewItems())
                            {
                                listViewSsdp.Items.Add(item);
                            }
                        }
                    }
                }
                iMutex.ReleaseMutex();
            }
        }

        private void listViewSsdp_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Set the ListViewItemSorter property to a new ListViewItemComparer 
            // object. Setting this property immediately sorts the 
            // ListView using the ListViewItemComparer object.
            listViewSsdp.ListViewItemSorter = new ListViewItemComparer(e.Column);
        }

        private void listBoxDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            iMutex.WaitOne();
            Device device = iDevices[listBoxDevices.SelectedItem.ToString()];
            listViewSsdp.Items.Clear();
            foreach (ListViewItem item in device.ListViewItems())
            {
                listViewSsdp.Items.Add(item);
            }
            iMutex.ReleaseMutex();
        }

        private Dictionary<string, Device> iDevices;
        private Mutex iMutex;

        private void listViewSsdp_MouseDoubleClick(object sender, MouseEventArgs e)
        {

            foreach (ListViewItem item in listViewSsdp.SelectedItems)
            {
                foreach (ListViewItem.ListViewSubItem subitem in item.SubItems)
                {
                    if ((string)subitem.Tag == "Location")
                    {
                        if (!String.IsNullOrEmpty(subitem.Text))
                        {
                            System.Diagnostics.Process.Start(subitem.Text);
                        }
                    }
                }
            }
        }

        private void EventFormLoad(object sender, EventArgs e)
        {
            iHelper.Stack.Start();
        }

        private void EventFormClosed(object sender, FormClosedEventArgs e)
        {
            iHelper.Stack.Stop();
        }

        private void MenuItemOptionsClick(object sender, EventArgs e)
        {
            FormUserOptions form = new FormUserOptions(iHelper.OptionPages);
            form.ShowDialog();
            form.Dispose();
        }

        SsdpListenerMulticast iListenerMulticast;
        SsdpListenerUnicast iListenerUnicast;
        Helper iHelper;
    }

    class MySsdpNotifyHandler : ISsdpNotifyHandler
    {
        const string kDateTimeFormat = "d MMM yyyy  HH:mm:ss";

        static readonly Color kBackColorAlive = Color.LightGreen;
        static readonly Color kBackColorByeBye = Color.LavenderBlush;
        static readonly Color kBackColorMsearch = Color.LightGoldenrodYellow;

        static readonly Color kForeColorAlive = Color.Black;
        static readonly Color kForeColorByeBye = Color.Black;
        static readonly Color kForeColorMsearch = Color.Black;

        public MySsdpNotifyHandler(Form1 aForm, bool aNotifyOrMsearch)
        {
            iForm = aForm;
            iNotifyOrMsearch = aNotifyOrMsearch;
        }

        public void NotifyRootAlive(byte[] aUuid, byte[] aLocation, uint aMaxAge)
        {
            ListViewItem item = new ListViewItem();
            ListViewItem.ListViewSubItem kind = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem location = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem domain = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem type = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem version = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem age = new ListViewItem.ListViewSubItem();

            kind.Tag = "Kind";
            location.Tag = "Location";
            domain.Tag = "Domain";
            type.Tag = "Type";
            version.Tag = "Version";
            age.Tag = "Age";

            item.Text = DateTime.Now.ToString(kDateTimeFormat);

            if (iNotifyOrMsearch)
            {
                item.BackColor = kBackColorAlive;
                item.ForeColor = kForeColorAlive;
            }
            else
            {
                item.BackColor = kBackColorMsearch;
                item.ForeColor = kForeColorMsearch;
            }

            kind.Text = "Root";
            location.Text = ASCIIEncoding.UTF8.GetString(aLocation);
            domain.Text = "-";
            type.Text = "-";
            version.Text = "-";
            age.Text = aMaxAge.ToString();

            item.SubItems.Add(kind);
            item.SubItems.Add(location);
            item.SubItems.Add(domain);
            item.SubItems.Add(type);
            item.SubItems.Add(version);
            item.SubItems.Add(age);

            iForm.AddSsdpItem(ASCIIEncoding.UTF8.GetString(aUuid), item);
        }

        public void NotifyUuidAlive(byte[] aUuid, byte[] aLocation, uint aMaxAge)
        {
            ListViewItem item = new ListViewItem();
            ListViewItem.ListViewSubItem kind = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem location = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem domain = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem type = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem version = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem age = new ListViewItem.ListViewSubItem();

            kind.Tag = "Kind";
            location.Tag = "Location";
            domain.Tag = "Domain";
            type.Tag = "Type";
            version.Tag = "Version";
            age.Tag = "Age";

            item.Text = DateTime.Now.ToString(kDateTimeFormat);

            if (iNotifyOrMsearch)
            {
                item.BackColor = kBackColorAlive;
                item.ForeColor = kForeColorAlive;
            }
            else
            {
                item.BackColor = kBackColorMsearch;
                item.ForeColor = kForeColorMsearch;
            }

            kind.Text = "Uuid";
            location.Text = ASCIIEncoding.UTF8.GetString(aLocation);
            domain.Text = "-";
            type.Text = "-";
            version.Text = "-";
            age.Text = aMaxAge.ToString();

            item.SubItems.Add(kind);
            item.SubItems.Add(location);
            item.SubItems.Add(domain);
            item.SubItems.Add(type);
            item.SubItems.Add(version);
            item.SubItems.Add(age);

            iForm.AddSsdpItem(ASCIIEncoding.UTF8.GetString(aUuid), item);
        }

        public void NotifyDeviceTypeAlive(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion, byte[] aLocation, uint aMaxAge)
        {
            ListViewItem item = new ListViewItem();
            ListViewItem.ListViewSubItem kind = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem location = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem domain = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem type = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem version = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem age = new ListViewItem.ListViewSubItem();

            kind.Tag = "Kind";
            location.Tag = "Location";
            domain.Tag = "Domain";
            type.Tag = "Type";
            version.Tag = "Version";
            age.Tag = "Age";

            item.Text = DateTime.Now.ToString(kDateTimeFormat);

            if (iNotifyOrMsearch)
            {
                item.BackColor = kBackColorAlive;
                item.ForeColor = kForeColorAlive;
            }
            else
            {
                item.BackColor = kBackColorMsearch;
                item.ForeColor = kForeColorMsearch;
            }

            kind.Text = "DeviceType";
            location.Text = ASCIIEncoding.UTF8.GetString(aLocation);
            domain.Text = ASCIIEncoding.UTF8.GetString(aDomain);
            type.Text = ASCIIEncoding.UTF8.GetString(aType);
            version.Text = aVersion.ToString();
            age.Text = aMaxAge.ToString();

            item.SubItems.Add(kind);
            item.SubItems.Add(location);
            item.SubItems.Add(domain);
            item.SubItems.Add(type);
            item.SubItems.Add(version);
            item.SubItems.Add(age);

            iForm.AddSsdpItem(ASCIIEncoding.UTF8.GetString(aUuid), item);
        }

        public void NotifyServiceTypeAlive(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion, byte[] aLocation, uint aMaxAge)
        {
            ListViewItem item = new ListViewItem();
            ListViewItem.ListViewSubItem kind = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem location = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem domain = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem type = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem version = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem age = new ListViewItem.ListViewSubItem();

            kind.Tag = "Kind";
            location.Tag = "Location";
            domain.Tag = "Domain";
            type.Tag = "Type";
            version.Tag = "Version";
            age.Tag = "Age";

            item.Text = DateTime.Now.ToString(kDateTimeFormat);

            if (iNotifyOrMsearch)
            {
                item.BackColor = kBackColorAlive;
                item.ForeColor = kForeColorAlive;
            }
            else
            {
                item.BackColor = kBackColorMsearch;
                item.ForeColor = kForeColorMsearch;
            }

            kind.Text = "ServiceType";
            location.Text = ASCIIEncoding.UTF8.GetString(aLocation);
            domain.Text = ASCIIEncoding.UTF8.GetString(aDomain);
            type.Text = ASCIIEncoding.UTF8.GetString(aType);
            version.Text = aVersion.ToString();
            age.Text = aMaxAge.ToString();

            item.SubItems.Add(kind);
            item.SubItems.Add(location);
            item.SubItems.Add(domain);
            item.SubItems.Add(type);
            item.SubItems.Add(version);
            item.SubItems.Add(age);

            iForm.AddSsdpItem(ASCIIEncoding.UTF8.GetString(aUuid), item);
        }

        public void NotifyRootByeBye(byte[] aUuid)
        {
            ListViewItem item = new ListViewItem();
            ListViewItem.ListViewSubItem kind = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem location = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem domain = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem type = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem version = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem age = new ListViewItem.ListViewSubItem();

            kind.Tag = "Kind";
            location.Tag = "Location";
            domain.Tag = "Domain";
            type.Tag = "Type";
            version.Tag = "Version";
            age.Tag = "Age";

            item.Text = DateTime.Now.ToString(kDateTimeFormat);

            if (iNotifyOrMsearch)
            {
                item.BackColor = kBackColorByeBye;
                item.ForeColor = kForeColorByeBye;
            }
            else
            {
                Assert.Check(false);
            }

            kind.Text = "Root";
            domain.Text = "-";
            type.Text = "-";
            version.Text = "-";
            age.Text = "-";

            item.SubItems.Add(kind);
            item.SubItems.Add(location);
            item.SubItems.Add(domain);
            item.SubItems.Add(type);
            item.SubItems.Add(version);
            item.SubItems.Add(age);

            iForm.AddSsdpItem(ASCIIEncoding.UTF8.GetString(aUuid), item);
        }

        public void NotifyUuidByeBye(byte[] aUuid)
        {
            ListViewItem item = new ListViewItem();
            ListViewItem.ListViewSubItem kind = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem location = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem domain = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem type = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem version = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem age = new ListViewItem.ListViewSubItem();

            kind.Tag = "Kind";
            location.Tag = "Location";
            domain.Tag = "Domain";
            type.Tag = "Type";
            version.Tag = "Version";
            age.Tag = "Age";

            item.Text = DateTime.Now.ToString(kDateTimeFormat);

            if (iNotifyOrMsearch)
            {
                item.BackColor = kBackColorByeBye;
                item.ForeColor = kForeColorByeBye;
            }
            else
            {
                Assert.Check(false);
            }

            kind.Text = "Uuid";
            domain.Text = "-";
            type.Text = "-";
            version.Text = "-";
            age.Text = "-";

            item.SubItems.Add(kind);
            item.SubItems.Add(location);
            item.SubItems.Add(domain);
            item.SubItems.Add(type);
            item.SubItems.Add(version);
            item.SubItems.Add(age);

            iForm.AddSsdpItem(ASCIIEncoding.UTF8.GetString(aUuid), item);
        }

        public void NotifyDeviceTypeByeBye(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion)
        {
            ListViewItem item = new ListViewItem();
            ListViewItem.ListViewSubItem kind = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem location = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem domain = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem type = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem version = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem age = new ListViewItem.ListViewSubItem();

            kind.Tag = "Kind";
            location.Tag = "Location";
            domain.Tag = "Domain";
            type.Tag = "Type";
            version.Tag = "Version";
            age.Tag = "Age";

            item.Text = DateTime.Now.ToString(kDateTimeFormat);

            if (iNotifyOrMsearch)
            {
                item.BackColor = kBackColorByeBye;
                item.ForeColor = kForeColorByeBye;
            }
            else
            {
                Assert.Check(false);
            }

            kind.Text = "DeviceType";
            domain.Text = ASCIIEncoding.UTF8.GetString(aDomain);
            type.Text = ASCIIEncoding.UTF8.GetString(aType);
            version.Text = aVersion.ToString();
            age.Text = "-";

            item.SubItems.Add(kind);
            item.SubItems.Add(location);
            item.SubItems.Add(domain);
            item.SubItems.Add(type);
            item.SubItems.Add(version);
            item.SubItems.Add(age);

            iForm.AddSsdpItem(ASCIIEncoding.UTF8.GetString(aUuid), item);
        }

        public void NotifyServiceTypeByeBye(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion)
        {
            ListViewItem item = new ListViewItem();
            ListViewItem.ListViewSubItem kind = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem location = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem domain = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem type = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem version = new ListViewItem.ListViewSubItem();
            ListViewItem.ListViewSubItem age = new ListViewItem.ListViewSubItem();

            kind.Tag = "Kind";
            location.Tag = "Location";
            domain.Tag = "Domain";
            type.Tag = "Type";
            version.Tag = "Version";
            age.Tag = "Age";

            item.Text = DateTime.Now.ToString(kDateTimeFormat);

            if (iNotifyOrMsearch)
            {
                item.BackColor = kBackColorByeBye;
                item.ForeColor = kForeColorByeBye;
            }
            else
            {
                Assert.Check(false);
            }

            kind.Text = "ServiceType";
            domain.Text = ASCIIEncoding.UTF8.GetString(aDomain);
            type.Text = ASCIIEncoding.UTF8.GetString(aType);
            version.Text = aVersion.ToString();
            age.Text = "-";

            item.SubItems.Add(kind);
            item.SubItems.Add(location);
            item.SubItems.Add(domain);
            item.SubItems.Add(type);
            item.SubItems.Add(version);
            item.SubItems.Add(age);

            iForm.AddSsdpItem(ASCIIEncoding.UTF8.GetString(aUuid), item);
        }

        Form1 iForm;
        bool iNotifyOrMsearch;
    }

    // Implements the manual sorting of items by columns.
    class ListViewItemComparer : System.Collections.IComparer
    {
        private int col;

        public ListViewItemComparer()
        {
            col = 0;
        }
        public ListViewItemComparer(int column)
        {
            col = column;
        }
        public int Compare(object x, object y)
        {
            return String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
        }
    }

    public class Device
    {
        public Device(string aUuid)
        {
            iUuid = aUuid;
            iListViewItems = new List<ListViewItem>();
        }

        public string Uuid()
        {
            return (iUuid);
        }

        public void Add(ListViewItem aItem)
        {
            iListViewItems.Add(aItem);
        }

        public List<ListViewItem> ListViewItems()
        {
            return (iListViewItems);
        }

        private string iUuid;

        private List<ListViewItem> iListViewItems;
    }
}
