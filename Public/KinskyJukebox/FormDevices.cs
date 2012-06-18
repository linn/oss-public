using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Linn;
using Linn.Control.Ssdp;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;

namespace KinskyJukebox
{
    public partial class FormDevices : Form
    {
        public FormDevices(HelperKinskyJukebox aHelper) {
            iHelper = aHelper;
            InitializeComponent();
            this.Icon = Icon.FromHandle(Properties.Resources.Export.GetHicon());
            try {
                iPresetUri = Presets.UriPath(aHelper);
                iPresetDir = Presets.DirectoryPath(aHelper.OptionPageSetup);
                locationTextBox.Text = Parameter.kNameJukeboxPresetPrefix + ":  " + iPresetUri;
            }
            catch (Exception e) {
                Linn.UserLog.WriteLine("Sync with Linn DS Failed: " + e.Message);
                MessageBox.Show(e.Message, "Sync with Linn DS Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Close();
            }
        }

        private void FormDevices_Load(object sender, EventArgs e) {
            try {
                // create discovery system
                iListenerNotify = new SsdpListenerMulticast();

                // create device lists
                iDeviceListJukebox = new DeviceListUpnp(ServiceJukebox.ServiceType(1), iListenerNotify);

                // hook in to discovery events
                iDeviceListJukebox.EventDeviceAdded += DeviceAlive;
                iDeviceListJukebox.EventDeviceRemoved += DeviceByeBye;

                NetworkInfoModel iface = iHelper.Interface.Interface.Info;
                if (iface != null) {
                    // start discovery process
                    iListenerNotify.Start(iface.IPAddress);
                    iDeviceListJukebox.Start(iface.IPAddress);

                    // improve discovery process
                    iDeviceListJukebox.Rescan();
                }
                else {
                    DiscoveryFailed("Device discovery failed: no valid network interface card selected");
                }
            }
            catch (Linn.Network.NetworkError ne) {
                DiscoveryFailed("NetworkError on device discovery: " + ne.Message);
            }
            catch (Exception exc) {
                DiscoveryFailed("Error on device discovery: " + exc.Message);
            }
        }

        private void DiscoveryFailed(string aLogMessage) {
            Linn.UserLog.WriteLine(aLogMessage);
            MessageBox.Show("Please insure you have selected a valid network interface (Tools/Options/Network)", "Device Discovery Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            Close();
        }

        private void FormDevices_FormClosed(object sender, FormClosedEventArgs e) {
            try {
                // insure result of form is correct
                if (buttonDone.Text == "Done") {
                    this.DialogResult = DialogResult.OK;
                }
                else {
                    this.DialogResult = DialogResult.Cancel;
                }

                // unlink from the control points
                iDeviceListJukebox.EventDeviceAdded -= DeviceAlive;
                iDeviceListJukebox.EventDeviceRemoved -= DeviceByeBye;

                // shut down the control points
                iDeviceListJukebox.Stop();
                iListenerNotify.Stop();

                // abort background threads if running
                if (iApplyThread != null && iApplyThread.IsAlive) {
                    iApplyThread.Abort();
                }
                if (iTestThread != null && iTestThread.IsAlive) {
                    iTestThread.Abort();
                }
            }
            catch (Exception) {
            }
        }

        private void buttonSelectAll_Click(object sender, EventArgs e) {
            for (int i = 0; i < deviceCheckedListBox.Items.Count; i++) {
                if (deviceCheckedListBox.GetItemCheckState(i) != CheckState.Indeterminate) {
                    deviceCheckedListBox.SetItemChecked(i, true);
                }
            }
        }

        private void buttonSelectNone_Click(object sender, EventArgs e) {
            for (int i = 0; i < deviceCheckedListBox.Items.Count; i++) {
                if (deviceCheckedListBox.GetItemCheckState(i) != CheckState.Indeterminate) {
                    deviceCheckedListBox.SetItemChecked(i, false);
                }
            }
        }

        private void buttonApply_Click(object sender, EventArgs e) {
            buttonDone.Text = "Cancel";
            buttonApply.Enabled = false;
            buttonSelectAll.Enabled = false;
            buttonSelectNone.Enabled = false;
            deviceCheckedListBox.Enabled = false;
            buttonTest.Enabled = false;
            List<JukeboxDevice> jbDevices = new List<JukeboxDevice>();

            for (int i = 0; i < deviceCheckedListBox.Items.Count; i++) {
                if (deviceCheckedListBox.GetItemChecked(i) && deviceCheckedListBox.GetItemCheckState(i) != CheckState.Indeterminate) {
                    JukeboxDevice jbDevice = (JukeboxDevice)deviceCheckedListBox.Items[i];
                    jbDevice.Index = i;
                    jbDevices.Add(jbDevice);
                }
            }

            iApplyThread = new Thread(Apply);
            iApplyThread.Name = "ApplyPresetPrefix";
            iApplyThread.IsBackground = true;
            iApplyThread.Start(jbDevices);
        }

        private void Apply(object aListJukeboxDevices) {
            try {
                string deviceName = "Unknown";
                foreach (JukeboxDevice jb in (List<JukeboxDevice>)aListJukeboxDevices) {
                    try {
                        deviceName = jb.UpnpDevice.Name;
                        jb.ChangeUri(iPresetUri);
                        ApplyProgress(jb, false);
                        Linn.UserLog.WriteLine("Applied Uri to " + deviceName + ": " + iPresetUri);
                    }
                    catch (Exception exc) {
                        if (exc.GetType() == typeof(ThreadAbortException)) {
                            throw exc;
                        }
                        MessageBox.Show("Unable to apply preset location to: " + deviceName + Environment.NewLine + exc.Message, "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        Linn.UserLog.WriteLine("Unable to apply preset location to: " + deviceName + Environment.NewLine + exc.Message);
                    }
                }
                ApplyProgress(null, true);
            }
            catch (ThreadAbortException) {
                return;
            }
        }

        private void ApplyProgress(JukeboxDevice aJukeboxDevice, bool aComplete) {
            if (this.InvokeRequired) {
                // no need to block
                this.BeginInvoke(new DEventProgress(ApplyProgress), new object[] { aJukeboxDevice, aComplete });
                return;
            }
            if (aComplete) {
                buttonDone.Text = "Done";
                buttonApply.Enabled = true;
                buttonSelectAll.Enabled = true;
                buttonSelectNone.Enabled = true;
                deviceCheckedListBox.Enabled = true;
            }
            else if (aJukeboxDevice != null) {
                deviceCheckedListBox.SetItemCheckState(aJukeboxDevice.Index, CheckState.Indeterminate);
                if (aJukeboxDevice.Index == deviceCheckedListBox.SelectedIndex) {
                    buttonTest.Enabled = true;
                }
            }
        }

        private void buttonTest_Click(object sender, EventArgs e) {
            if (deviceCheckedListBox.SelectedIndex >= 0) {
                buttonDone.Text = "Cancel";
                buttonApply.Enabled = false;
                buttonSelectAll.Enabled = false;
                buttonSelectNone.Enabled = false;
                deviceCheckedListBox.Enabled = false;
                buttonTest.Enabled = false;

                iTestThread = new Thread(Test);
                iTestThread.Name = "TestPresetPrefix";
                iTestThread.IsBackground = true;
                iTestThread.Start(((JukeboxDevice)deviceCheckedListBox.Items[deviceCheckedListBox.SelectedIndex]));
            }
        }

        private void Test(object aJukeboxDevice) {
            try {
                bool testPass = false;
                string testFailMessage = "Could not loacte any dpl files here: " + iPresetDir;
                string deviceName = "Unknown";
                uint dplToUse = 0;
                string[] dplFiles = null;
                try {
                    dplFiles = Directory.GetFiles(iPresetDir, "*.dpl", SearchOption.TopDirectoryOnly);
                    foreach (string dpl in dplFiles) {
                        testFailMessage = "Could not loacte a dpl file with track data here: " + iPresetDir;
                        FileInfo fi = new FileInfo(dpl);
                        if (fi.Length > 500) { // at least one track in dpl file
                            try {
                                uint dplNumber = uint.Parse(Path.GetFileNameWithoutExtension(dpl));
                                if (dplNumber != iLastDplUsed) {
                                    dplToUse = dplNumber;
                                    break;
                                }
                            }
                            catch (Exception exc) {
                                if (exc.GetType() == typeof(ThreadAbortException)) {
                                    throw exc;
                                }
                            }
                        }
                    }
                }
                catch (Exception exc) {
                    if (exc.GetType() == typeof(ThreadAbortException)) {
                        throw exc;
                    }
                }
                if (dplToUse != 0) {
                    testFailMessage = "Device selected for test does not have the correct preset prefix";
                    if (((JukeboxDevice)aJukeboxDevice).PresetUriMatch) {
                        try {
                            deviceName = ((JukeboxDevice)aJukeboxDevice).UpnpDevice.Name;
                            DialogResult result = MessageBox.Show("This test will send preset " + dplToUse + " to " + deviceName + Environment.NewLine + "This will clear any playlist currently stored on this device", "Proceed with Test", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                            if (result == DialogResult.OK) {
                                uint aToken = 0;
                                uint aNewToken = 0;
                                byte[] aArray = null;
                                byte[] aNewArray = null;
                                ((JukeboxDevice)aJukeboxDevice).PlaylistService.StopSync();
                                ((JukeboxDevice)aJukeboxDevice).PlaylistService.DeleteAllSync();
                                ((JukeboxDevice)aJukeboxDevice).PlaylistService.IdArraySync(out aToken, out aArray);
                                ((JukeboxDevice)aJukeboxDevice).JukeboxService.SetCurrentPresetSync(dplToUse);
                                iLastDplUsed = dplToUse;

                                uint wait = 0;
                                bool playing = false;
                                while (wait++ < 40) {
                                    Thread.Sleep(1000); // 40 seconds to start playing
                                    if (((JukeboxDevice)aJukeboxDevice).PlaylistService.TransportStateSync() == "Playing") {
                                        playing = true;
                                        break;
                                    }
                                }

                                ((JukeboxDevice)aJukeboxDevice).PlaylistService.IdArraySync(out aNewToken, out aNewArray);
                                if (aArray.Length == 0 && aNewArray.Length > 0) {
                                    if (playing) {
                                        testPass = true;
                                    }
                                    else {
                                        testFailMessage = "Playlist loaded but could not be played on device: " + deviceName;
                                    }
                                }
                                else {
                                    testFailMessage = "Playlist has not been updated on device: " + deviceName;
                                }
                            }
                            else {
                                // test cancelled - exit here (no pass or fail message)
                                TestProgress(null, true);
                                return;
                            }
                        }
                        catch (Linn.ControlPoint.ServiceException sexc) {
                            if (sexc.Description == "UPnPError") {
                                testFailMessage = "Could not load the preset to device: " + deviceName + Environment.NewLine + "Please check the URL: " + iPresetUri;
                            }
                            else {
                                testFailMessage = "Could not communicate with device: " + deviceName + Environment.NewLine + "Error Message: " + sexc.Message;
                            }
                            
                        }
                        catch (Exception exc) {
                            if (exc.GetType() == typeof(ThreadAbortException)) {
                                throw exc;
                            }
                            testFailMessage = "Could not communicate with device: " + deviceName + Environment.NewLine + "Error Message: " + exc.Message;
                        }
                    }
                }
                if (testPass) {
                    MessageBox.Show("Preset " + dplToUse + " now playing on " + deviceName, "Test Passed", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                else {
                    MessageBox.Show(testFailMessage, "Test Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
                TestProgress(null, true);
            }
            catch (ThreadAbortException) {
                return;
            }
        }

        private void TestProgress(JukeboxDevice aJukeboxDevice, bool aComplete) {
            if (this.InvokeRequired) {
                // no need to block
                this.BeginInvoke(new DEventProgress(TestProgress), new object[] { aJukeboxDevice, aComplete });
                return;
            }
            if (aComplete) {
                buttonDone.Text = "Done";
                buttonTest.Enabled = true;
                buttonApply.Enabled = true;
                buttonSelectAll.Enabled = true;
                buttonSelectNone.Enabled = true;
                deviceCheckedListBox.Enabled = true;
            }
        }

        private void deviceCheckedListBox_SelectedIndexChanged(object sender, EventArgs e) {
            if (deviceCheckedListBox.SelectedIndex < 0) {
                buttonTest.Enabled = false;
            }
            else if (deviceCheckedListBox.SelectedIndex != iPreviousSelectedIndex) {
                if (((JukeboxDevice)deviceCheckedListBox.Items[deviceCheckedListBox.SelectedIndex]).PresetUriMatch) {
                    buttonTest.Enabled = true;
                }
                else {
                    buttonTest.Enabled = false;
                    deviceCheckedListBox.SetItemChecked(deviceCheckedListBox.SelectedIndex, !deviceCheckedListBox.GetItemChecked(deviceCheckedListBox.SelectedIndex));
                }
            }
            iPreviousSelectedIndex = deviceCheckedListBox.SelectedIndex;
        }

        private void DeviceAlive(object obj, DeviceList.EventArgsDevice e) {
            try {
                if (this.InvokeRequired) {
                    this.BeginInvoke((MethodInvoker)delegate() { DeviceAlive(obj, e); });
                }
                else {
                    if (e.Device.Name != null && e.Device.Name != "") {
                        JukeboxDevice jd = new JukeboxDevice(e.Device, iPresetUri);
                        int index = deviceCheckedListBox.Items.Add(jd);
                        if (jd.PresetUriMatch) {
                            deviceCheckedListBox.SetItemCheckState(index, CheckState.Indeterminate);
                        }
                    }
                }
            }
            catch (Exception) {
            }
        }

        private void DeviceByeBye(object obj, DeviceList.EventArgsDevice e) {
            try {
                if (this.InvokeRequired) {
                    this.BeginInvoke((MethodInvoker)delegate() { DeviceByeBye(obj, e); });
                }
                else {
                    foreach (JukeboxDevice jd in deviceCheckedListBox.Items) {
                        if (jd.UpnpDevice == e.Device) {
                            deviceCheckedListBox.Items.Remove(jd);
                            break;
                        }
                    }
                }
            }
            catch (Exception) {
            }
        }

        private DeviceListUpnp iDeviceListJukebox;
        private SsdpListenerMulticast iListenerNotify;
        private HelperKinskyJukebox iHelper;
        private string iPresetUri = "";
        private string iPresetDir = "";
        private int iPreviousSelectedIndex = -1;
        private uint iLastDplUsed = 0;
        private Thread iApplyThread = null;
        private Thread iTestThread = null;
        private delegate void DEventProgress(JukeboxDevice aJukeboxDevice, bool aComplete);
    }

    public class JukeboxDevice
    {
        public JukeboxDevice(Device aDevice, string aNewUri) {
            iDevice = aDevice;
            try {
                iNewPresetUri = Uri.UnescapeDataString(aNewUri);
            }
            catch {
                iNewPresetUri = "";
            }
            iJukeboxService = new ServiceJukebox(aDevice);
            iConfigurationService = new ServiceConfiguration(aDevice);
            iPlaylistService = new ServicePlaylist(aDevice);
            try {
                string currUri = ProductSupport.GetParameterValue(iConfigurationService.ParameterXmlSync(), Parameter.kTargetJukebox, Parameter.kNameJukeboxPresetPrefix);
                iCurrentPresetUri = Uri.UnescapeDataString(currUri);
            }
            catch (Exception e) {
                iCurrentPresetUri = "Unknown: " + e.Message;
            }
        }

        public override string ToString() {
            if (PresetUriMatch) {
                return iDevice.Name + " - Already set to correct location";
            }
            else {
                return iDevice.Name + " - " + iCurrentPresetUri;
            }
        }

        public ServiceJukebox JukeboxService {
            get { return iJukeboxService; }
        }

        public ServiceConfiguration ConfigurationService {
            get { return iConfigurationService; }
        }

        public ServicePlaylist PlaylistService {
            get { return iPlaylistService; }
        }

        public Device UpnpDevice {
            get { return iDevice; }
        }

        public int Index {
            get { return iIndex; }
            set { iIndex = value; }
        }

        public bool PresetUriMatch {
            get { return (iCurrentPresetUri == iNewPresetUri); }
        }

        public void ChangeUri(string aUri) {
        	iConfigurationService.SetParameterSync(Parameter.kTargetJukebox, Parameter.kNameJukeboxPresetPrefix, aUri);
        	iCurrentPresetUri = Uri.UnescapeDataString(aUri);
        }

        private Device iDevice = null;
        private ServiceJukebox iJukeboxService = null;
        private ServiceConfiguration iConfigurationService = null;
        private ServicePlaylist iPlaylistService = null;
        private string iCurrentPresetUri = "";
        private string iNewPresetUri = "";
        private int iIndex = -1;
    }
}
