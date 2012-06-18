using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using PcapDotNet.Packets.Transport;

namespace Linn.DsTrace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private ObservableCollection<LivePacketDeviceViewModel> iDevices = new ObservableCollection<LivePacketDeviceViewModel>();
        private Thread iCaptureThread;
        private string iFilename;
        private Dictionary<string, Session> iSessions = new Dictionary<string, Session>();
        private List<Session> iSessionList = new List<Session>();
        private uint iSequence = 1;

        public MainWindow()
        {
            InitializeComponent();

            IList<LivePacketDevice> devices = LivePacketDevice.AllLocalMachine;
            foreach (LivePacketDevice device in devices)
            {
                iDevices.Add(new LivePacketDeviceViewModel(device));
            }
            lstInterfaces.ItemsSource = iDevices;
            if (devices.Count == 0)
            {
                MessageBox.Show("No interfaces found.  Cannot capture packets.");
                tabCapture.IsEnabled = false;
                tabFiltering.IsSelected = true;
            }
            else
            {
                lstInterfaces.SelectedIndex = 0;
            }
            EvaluateButtonState();
        }

        private void ShowFileDialog(TextBox aTextBox, string aFilter, bool aRequestDelete)
        {
            System.Windows.Forms.FileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.CheckFileExists = false;
            dialog.Filter = aFilter;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                bool cancel = false;
                if (File.Exists(dialog.FileName) && aRequestDelete)
                {
                    var clear = MessageBox.Show("Do you wish to delete the existing file contents?", "File exists - delete?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);
                    if (clear == MessageBoxResult.Yes)
                    {
                        File.Delete(dialog.FileName);
                    }
                    else
                    {
                        cancel = clear == MessageBoxResult.Cancel;
                    }
                }
                if (!cancel)
                {
                    aTextBox.Text = dialog.FileName;
                }
            }
        }

        private void StartCapture()
        {
            iFilename = txtCaptureFilename.Text;
            if (lstInterfaces.SelectedItem != null)
            {
                PacketDevice selectedDevice = (lstInterfaces.SelectedItem as LivePacketDeviceViewModel).Item;
                iCaptureThread = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        // Open the device
                        using (PacketCommunicator communicator =
                            selectedDevice.Open(65536,                                  // portion of the packet to capture
                            // 65536 guarantees that the whole packet will be captured on all the link layers
                                                PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                                1000))                                  // read timeout
                        {
                            // Check the link layer. We support only Ethernet for simplicity.
                            if (communicator.DataLink.Kind != DataLinkKind.Ethernet)
                            {
                                this.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    MessageBox.Show("This program works only on Ethernet networks.");
                                }));
                                return;
                            }
                            using (PacketDumpFile dumpFile = communicator.OpenDump(iFilename))
                            {
                                // start the capture
                                communicator.ReceivePackets(0, dumpFile.Dump);
                            }
                        }
                    }
                    catch (ThreadAbortException) { }
                }));
                iCaptureThread.Start();
            }
            btnStartStop.Content = "Stop capture...";
        }

        private void StopCapture()
        {
            if (iCaptureThread != null)
            {
                iCaptureThread.Abort();
            }
            btnStartStop.Content = "Start capture...";
        }

        private void DoFilter()
        {
            try
            {
                // Create the offline device
                if (File.Exists(txtCaptureFilename.Text))
                {
                    if (txtFilter.Text != string.Empty)
                    {
                        OfflinePacketDevice selectedDevice = new OfflinePacketDevice(txtCaptureFilename.Text);
                        iSessions.Clear();
                        iSessionList.Clear();
                        this.Cursor = Cursors.Wait;
                        lblProgress.Visibility = Visibility.Visible;
                        progressBar.Visibility = Visibility.Visible;
                        progressBar.IsIndeterminate = true;
                        progressBar.IsEnabled = true;
                        // Open the capture file
                        using (PacketCommunicator communicator =
                            selectedDevice.Open(65536,                                  // portion of the packet to capture
                            // 65536 guarantees that the whole packet will be captured on all the link layers
                                                PacketDeviceOpenAttributes.Promiscuous, // promiscuous mode
                                                1000))                                  // read timeout
                        {

                            using (BerkeleyPacketFilter filter = communicator.CreateFilter("ip host " + txtFilter.Text))
                            {
                                // Set the filter
                                communicator.SetFilter(filter);
                            }

                            // Read and dispatch packets until EOF is reached
                            communicator.ReceivePackets(0, FilterPacket);
                        }
                        progressBar.IsIndeterminate = false;
                        progressBar.IsEnabled = true;
                        progressBar.Minimum = 0;
                        progressBar.Maximum = iSessionList.Count;
                        progressBar.Value = 0;
                        foreach (Session session in iSessionList)
                        {
                            StringBuilder builder = new StringBuilder();
                            builder.AppendLine("Session: " + session.StartSequence + " - " + session.EndSequence);
                            builder.AppendLine("Route: " + session.Key);
                            foreach (TransportDatagram packet in session.Packets)
                            {
                                if (packet is TcpDatagram)
                                {
                                    TcpDatagram dgram = packet as TcpDatagram;

                                    using (MemoryStream ms = dgram.Payload.ToMemoryStream())
                                    {
                                        ms.Seek(0, SeekOrigin.Begin);
                                        byte[] buffer = new byte[ms.Length];
                                        ms.Read(buffer, 0, (int)ms.Length);
                                        builder.AppendLine(Encoding.UTF8.GetString(buffer));
                                    }
                                }
                                else
                                {
                                    UdpDatagram dgram = packet as UdpDatagram;
                                    using (MemoryStream ms = dgram.Payload.ToMemoryStream())
                                    {
                                        ms.Seek(0, SeekOrigin.Begin);
                                        byte[] buffer = new byte[ms.Length];
                                        ms.Read(buffer, 0, (int)ms.Length);
                                        builder.AppendLine(Encoding.UTF8.GetString(buffer));
                                    }
                                }
                            }
                            builder.AppendLine("--------------------------------------------------------------------------------");
                            File.AppendAllText(txtFilterFilename.Text, builder.ToString());
                            progressBar.Value++;
                        }
                        lblProgress.Visibility = Visibility.Collapsed;
                        progressBar.Visibility = Visibility.Collapsed;
                        this.ClearValue(FrameworkElement.CursorProperty);
                    }
                    else
                    {
                        MessageBox.Show("Please specify an IP Address to filter results by.");
                    }
                }
                else
                {
                    MessageBox.Show("Capture file is empty or doesn't exist.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error caught: " + ex);
            }

        }

        private void FilterPacket(Packet packet)
        {
            TransportDatagram dgram = null;
            if (packet.Ethernet.IpV4.Tcp != null)
            {
                dgram = packet.Ethernet.IpV4.Tcp;
            }
            else if (packet.Ethernet.IpV4.Udp != null)
            {
                dgram = packet.Ethernet.IpV4.Udp;
            }
            if (dgram != null)
            {
                string srcIP = string.Format("{0}:{1}", packet.Ethernet.IpV4.Source.ToString(), dgram.SourcePort);
                string dstIP = string.Format("{0}:{1}", packet.Ethernet.IpV4.Destination.ToString(), dgram.DestinationPort);
                string key = string.Format("{0} -> {1}", srcIP, dstIP);
                if (dgram is UdpDatagram)
                {
                    // make udp 'sessions' unique
                    key += "(UDP) : " + iSequence;
                }
                Session session;
                if (iSessions.ContainsKey(key))
                {
                    session = iSessions[key];
                }
                else
                {
                    session = new Session();
                    session.Key = key;
                    iSessions.Add(key, session);
                    iSessionList.Add(session);
                }
                session.AppendPacket(dgram, iSequence++);
            }
        }

        private void btnCaptureFilename_Click(object sender, RoutedEventArgs e)
        {
            ShowFileDialog(txtCaptureFilename, "Capture files (*.pcap)|*.pcap|All files (*.*)|*.*", false);
        }

        private void btnFilterFilename_Click(object sender, RoutedEventArgs e)
        {
            ShowFileDialog(txtFilterFilename, "Text files (*.txt)|*.txt|All files (*.*)|*.*", true);
        }

        private void btnStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (btnStartStop.IsChecked.HasValue && btnStartStop.IsChecked.Value)
            {
                StartCapture();
            }
            else
            {
                StopCapture();
            }
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            DoFilter();
        }

        private void EvaluateButtonState()
        {

            btnFilter.IsEnabled = File.Exists(txtCaptureFilename.Text) &&
                txtFilterFilename.Text != string.Empty &&
                txtFilter.Text != string.Empty;
            btnStartStop.IsEnabled = iDevices.Count > 0 && txtCaptureFilename.Text != string.Empty;
        }

        private void txtFilterFilename_TextChanged(object sender, TextChangedEventArgs e)
        {
            EvaluateButtonState();
        }

        private void txtCaptureFilename_TextChanged(object sender, TextChangedEventArgs e)
        {
            EvaluateButtonState();
        }


        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            EvaluateButtonState();
        }
    }


    public class LivePacketDeviceViewModel : INotifyPropertyChanged
    {
        public LivePacketDeviceViewModel(LivePacketDevice aDevice)
        {
            Item = aDevice;
        }

        private LivePacketDevice iItem;
        public LivePacketDevice Item
        {
            get
            {
                return iItem;
            }
            set
            {
                iItem = value;
                OnPropertyChanged("Item");
            }
        }

        public override string ToString()
        {
            if (Item != null)
            {
                return Item.Description;
            } return "";
        }

        private void OnPropertyChanged(string aPropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(aPropertyName));
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public class Session
    {
        public string Key { get; set; }

        public uint StartSequence
        {
            get
            {
                return iStart;
            }
        }
        public uint EndSequence
        {
            get
            {
                return iEnd;
            }
        }
        public void AppendPacket(TransportDatagram packet, uint sequence)
        {
            if (iPackets.Count == 0)
            {
                iStart = sequence;
            }
            iEnd = sequence;
            iPackets.Add(packet);
        }
        public IEnumerable<TransportDatagram> Packets
        {
            get
            {
                return new ReadOnlyCollection<TransportDatagram>(iPackets);
            }
        }
        private List<TransportDatagram> iPackets = new List<TransportDatagram>();
        private uint iStart, iEnd;
    }
}
