using System;
using System.Net;
using System.Windows.Forms;

using Linn;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.Topology;
using Upnp;

namespace TestMediaServerSearch
{
    public partial class Form1 : Form
    {
        private IHelper iHelper;

        private EventServerUpnp iEventServer;
        private SsdpListenerMulticast iListenerNotify;

        private ModelLibrary iModelLibrary;

        internal class ComboItem
        {
            public ComboItem(upnpObject aObject)
            {
                Object = aObject;
            }

            public override string ToString()
            {
                return Object.Title;
            }

            public upnpObject Object;
        }

        public Form1(IHelper aHelper)
        {
            InitializeComponent();

            Closed += Form1_FormClosed;

            iHelper = aHelper;

            iEventServer = new EventServerUpnp();
            iListenerNotify = new SsdpListenerMulticast();
            iModelLibrary = new ModelLibrary(iListenerNotify, iEventServer);

            iModelLibrary.EventContainerUpdated += EventContainerUpdated;
        }

        private void EventContainerUpdated(object sender, ModelMediaServer.EventArgsContainerUpdate e)
        {
            string id = iModelLibrary.HomeContainer.Id;

            if (e.ContainerId == id)
            {
                uint initialUpdateId, updateId;
                uint count = iModelLibrary.Count("", id, out initialUpdateId);
                DidlLite didl = iModelLibrary.Items("", id, 0, count, out updateId);

                UpdateComboBox(didl);
            }
        }

        private delegate void UpdateComboBoxCallback(DidlLite aDidlLite);
        private void UpdateComboBox(DidlLite aDidlLite)
        {
            if (comboBox1.InvokeRequired)
            {
                comboBox1.BeginInvoke(new UpdateComboBoxCallback(UpdateComboBox), new object[] { aDidlLite });
            }
            else
            {
                string selection = string.Empty;
                if (comboBox1.SelectedIndex > -1)
                {
                    selection = comboBox1.Items[comboBox1.SelectedIndex].ToString();
                }

                comboBox1.BeginUpdate();

                comboBox1.Items.Clear();

                for (int i = 0; i < aDidlLite.Count; i++)
                {
                    upnpObject o = aDidlLite[i];

                    comboBox1.Items.Add(new ComboItem(o));

                    if (o.Title == selection)
                    {
                        comboBox1.SelectedIndex = i;
                    }
                }

                comboBox1.EndUpdate();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            IPAddress addr = iHelper.Interface.Interface.Info.IPAddress;
            iEventServer.Start(addr);
            iListenerNotify.Start(addr);
            iModelLibrary.Start(addr);
        }

        private void Form1_FormClosed(object sender, EventArgs e)
        {
            iEventServer.Stop();
            iListenerNotify.Stop();
            iModelLibrary.Stop();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                if (comboBox1.SelectedIndex > -1)
                {
                    textBox2.Text = "Searching...";

                    ComboItem item = comboBox1.Items[comboBox1.SelectedIndex] as ComboItem;

                    try
                    {
                        uint updateId;
                        string search = string.Empty;
                        search += "dc:title contains \"" + textBox1.Text + "\"";
                        search += " or";
                        search += " dc:creator contains \"" + textBox1.Text + "\"";
                        search += " or";
                        search += " upnp:artist contains \"" + textBox1.Text + "\"";
                        search += " or";
                        /*search += " upnp:author contains \"" + textBox1.Text + "\"";
                        search += " or";
                        search += " upnp:actor contains \"" + textBox1.Text + "\"";
                        search += " or";*/
                        search += " upnp:album contains \"" + textBox1.Text + "\"";
                        /*search += " or";
                        search += " upnp:genre contains \"" + textBox1.Text + "\"";*/
                        DidlLite didl = iModelLibrary.Search(search, item.Object.Id, "0", 0, 0, out updateId);

                        textBox2.Text = didl.Xml;
                    }
                    catch (Exception ex)
                    {
                        textBox2.Text = ex.Message;
                    }
                }
            }
        }
    }
}
