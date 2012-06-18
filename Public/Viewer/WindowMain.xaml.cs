using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Serialization;
using Zapp;

namespace Viewer
{
    public partial class WindowMain : Window
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public WindowMain()
        {
            InitializeComponent();
            bool instantiated;
            iInstanceMutex = new Mutex(false, "ViewerMutex", out instantiated);
            if (!instantiated)
            {
                MessageBox.Show("Another instance of Viewer is already running. Please close all other instances and try again.", "Viewer is already running", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
            }

            iDebugLevels = new SortedList<string, DebugLevelItem>();
            UInt32 debugLevel;
            
            iDebugLevels.Add("Store", new DebugLevelItem("Store", debugLevel = 1));
            iDebugLevels.Add("Flash", new DebugLevelItem("Flash", debugLevel *= 2));
            iDebugLevels.Add("Common", new DebugLevelItem("Common", debugLevel *= 2));
            iDebugLevels.Add("Boot", new DebugLevelItem("Boot", debugLevel *= 2));
            iDebugLevels.Add("Thread", new DebugLevelItem("Thread", debugLevel *= 2));
            iDebugLevels.Add("Bsp", new DebugLevelItem("Bsp", debugLevel *= 2));
            iDebugLevels.Add("Network", new DebugLevelItem("Network", debugLevel *= 2));
            iDebugLevels.Add("Event", new DebugLevelItem("Event", debugLevel *= 2));
            iDebugLevels.Add("SysLib", new DebugLevelItem("SysLib", debugLevel *= 2));
            iDebugLevels.Add("UpnpDevice", new DebugLevelItem("UpnpDevice", debugLevel *= 2));
            iDebugLevels.Add("Sdp", new DebugLevelItem("Sdp", debugLevel *= 2));
            iDebugLevels.Add("Ess", new DebugLevelItem("Ess", debugLevel *= 2));
            iDebugLevels.Add("Power", new DebugLevelItem("Power", debugLevel *= 2));
            iDebugLevels.Add("Http", new DebugLevelItem("Http", debugLevel *= 2));
            iDebugLevels.Add("Upnp", new DebugLevelItem("Upnp", debugLevel *= 2));
            iDebugLevels.Add("Preamp", new DebugLevelItem("Preamp", debugLevel *= 2));
            iDebugLevels.Add("Logical", new DebugLevelItem("Logical", debugLevel *= 2));
            iDebugLevels.Add("Viewer", new DebugLevelItem("Viewer", debugLevel *= 2));
            iDebugLevels.Add("Ui", new DebugLevelItem("Ui", debugLevel *= 2));
            iDebugLevels.Add("Isr", new DebugLevelItem("Isr", debugLevel *= 2));
            iDebugLevels.Add("Core", new DebugLevelItem("Core", debugLevel *= 2));
            iDebugLevels.Add("Media", new DebugLevelItem("Media", debugLevel *= 2));
            iDebugLevels.Add("Dac", new DebugLevelItem("Dac", debugLevel *= 2));
            iDebugLevels.Add("Products", new DebugLevelItem("Products", debugLevel *= 2));
            iDebugLevels.Add("Mechanism", new DebugLevelItem("Mechanism", debugLevel *= 2));
            iDebugLevels.Add("Bonjour", new DebugLevelItem("Bonjour", debugLevel *= 2));
            iDebugLevels.Add("Ssdp", new DebugLevelItem("Ssdp", debugLevel *= 2));
            iDebugLevels.Add("Queue", new DebugLevelItem("Queue", debugLevel *= 2));
            iDebugLevels.Add("Codec", new DebugLevelItem("Codec", debugLevel *= 4)); //skipping deprecated option here.
            iDebugLevels.Add("Control", new DebugLevelItem("Control", debugLevel *= 2));
            iDebugLevels.Add("Verbose", new DebugLevelItem("Verbose", debugLevel *= 2));
            foreach(DebugLevelItem i in iDebugLevels.Values)
            {
                i.PropertyChanged += EventDebugLevelChanged;
            }
            listBoxDebugLevel.ItemsSource = iDebugLevels.Values;
            DisableDebugLevels();
            this.DataContext = this;

            iInitParams = new InitParams();
            iLibrary = new Library();
            iLibrary.Initialise(ref iInitParams);
            iLibrary.StartCp();
            iSubnetList = new SubnetList(iInitParams, Dispatcher, iLibrary);
            iDeviceList = new DeviceListUpnp("linn.co.uk", "Volkano", 1, this.Dispatcher);

            comboBoxSubnets.ItemsSource = iSubnetList;
            comboBoxSubnets.SelectionChanged += EventSubnetSelectionChanged;
            
            iActivityLightTimer.Interval = iActivityLightTimerTimeout;
            iActivityLightTimer.Elapsed += EventActivityLightTimerExpired;
            iActivityLightTimer.AutoReset = false;

            iDelayMutexReleaseTimer.Interval = iDelayMutexReleaseTimerTimeout;
            iDelayMutexReleaseTimer.Elapsed += EventDelayMutexReleaseTimerExpired;
            iDelayMutexReleaseTimer.AutoReset = false;

            iRetryConnectTimer.Interval = iRetryConnectTimerTimeout;
            iRetryConnectTimer.Elapsed += EventRetryConnectTimerExpired;
            iRetryConnectTimer.AutoReset = true;

            MainWindow.Closing += new CancelEventHandler(this.EventWindowMainClosing);

            iViewerManager = new ViewerManager();
            iViewerManager.ViewerOutputAvailable += EventViewerOutputAvailable;
            iViewerManager.ConnectionAccepted += EventConnectionAccepted;
            iViewerManager.ConnectionClosed += EventConnectionClosed;
            iViewerManager.ConnectionRefused += EventConnectionRefused;
            iViewerManager.ServicesAvailable += EventServicesAvailable;
            iViewerManager.ServicesUnavailable += EventServicesUnavailable;

            terminal.MaxVisibleLinesCountChanged += EventMaxVisibleLinesCountChanged;

            if(!Directory.Exists(iAppDataDir))            
            {
                Directory.CreateDirectory(iAppDataDir);
            }

            iMainLogFile = iAppDataDir + "\\Viewer-log.txt";
            iMainLogFileCopy = iAppDataDir + "\\Viewer-log-copy.txt";
            iTempLogFile = iAppDataDir + "\\Viewer-log-Temp.txt";
            iSettingsFile = iAppDataDir + "\\Viewer-Settings.txt";
            
            //Wipe the log file clean.
            if (!File.Exists(iMainLogFile))
            {
                iLogFileMutex.WaitOne();
                File.WriteAllText(iMainLogFile, "", new System.Text.UTF8Encoding());
                iLogFileMutex.ReleaseMutex();
            }

            if (File.Exists(iSettingsFile))
            {
                try
                {
                    iUserSettings = UserSettings.ReadSettingsFromFile(iSettingsFile);
                }
                catch (InvalidOperationException)
                {
                    iUserSettings = new UserSettings();
                    AppendToMainWindow("Settings file is corrupt, deleting...");
                    File.WriteAllText(iSettingsFile, "", new System.Text.UTF8Encoding());
                }
                catch (FileFormatException)
                {
                    iUserSettings = new UserSettings();
                    AppendToMainWindow("Settings file is corrupt, deleting...");
                    File.WriteAllText(iSettingsFile, "", new System.Text.UTF8Encoding());
                }
            }
            else
            {
                iUserSettings = new UserSettings();
            }
            if (iUserSettings.SelectedSubnet >= iSubnetList.length)
                iUserSettings.SelectedSubnet = iSubnetList.length - 1;
            comboBoxSubnets.SelectedIndex = iUserSettings.SelectedSubnet;
            iLibrary.SetCurrentSubnet(((Subnet)comboBoxSubnets.SelectedItem).Handle);
            if (!iUserSettings.UglyName.Equals(""))
                selectDeviceButton.Content = iUserSettings.UglyName;
            Connect(iUserSettings.UglyName);
        }

        private void EventViewLogButtonClicked(object sender, RoutedEventArgs e)
        {
            /* This method copies the mainLogFile so that we have a snapshot which is not modified
                whilst we are parsing it. Then we parse the mainLogFile copy and output each payload
                plus its timestamp to another log file which we pass to notepad.*/

            if (iProcess != null && !iProcess.HasExited)
            {
                SetForegroundWindow(iProcess.MainWindowHandle);
                return;
            }

            File.Delete(iMainLogFileCopy);
            //This will block AppendToMainWindowRecords() from printing text to the terminal,
            //however, this copy() call has always been more than fast enough.
            iLogFileMutex.WaitOne();
            File.Copy(iMainLogFile, iMainLogFileCopy);
            iLogFileMutex.ReleaseMutex();

            iCopyLogProgressBar = new ProgressBarWindow();
            iCopyLogProgressBar.cancelButton.Click += EventCopyLogProgessBarCancelButtonClicked; 
            iCopyLogProgressBar.Show();

            iCopyLog = new BackgroundWorker();
            iCopyLog.DoWork += EventCopyLogDoWork;
            iCopyLog.ProgressChanged += EventCopyLogProgressChanged;
            iCopyLog.RunWorkerCompleted += EventCopyLogRunWorkerCompleted;
            iCopyLog.WorkerReportsProgress = true;
            iCopyLog.WorkerSupportsCancellation = true;

            iCopyLog.RunWorkerAsync(new BinaryReader(File.Open(iMainLogFileCopy, FileMode.OpenOrCreate, FileAccess.Read)));
        }

        private void EventClearLogButtonClicked(object sender, RoutedEventArgs e)
        {
            iLogFileMutex.WaitOne();
            terminal.Text = "";
            File.WriteAllText(iMainLogFile, "", new System.Text.UTF8Encoding());
            this.LogFileRecords = 0;
            iLogFileMutex.ReleaseMutex();
        }

        private void EventSelectDeviceButtonClicked(object sender, RoutedEventArgs e)
        {
            iDeviceSelectionWindow = new DeviceSelectionWindow(iDeviceList, iUserSettings.UglyName);
            Nullable<bool> result = iDeviceSelectionWindow.ShowDialog();
            if (result == false)
                return;

            iUserSettings.UglyName = iDeviceSelectionWindow.UglyName;
            ConnectedLightOff();
            TurnOnActivityLight();
            selectDeviceButton.Content = iUserSettings.UglyName;
            UserSettings.WriteSettingsToFile(iSettingsFile, iUserSettings);
            Connect(iUserSettings.UglyName);
            
        }

        private void EventCopyLogDoWork(object sender, DoWorkEventArgs e)
        {
            File.Delete(iTempLogFile);
            using(BinaryReader binaryReader = (BinaryReader)e.Argument)
            using(StreamWriter streamWriter = new StreamWriter(iTempLogFile, false))
            {
                int count = 0;
                long bytesRead = 0;
                long fileSize = new FileInfo(iMainLogFileCopy).Length;
                int percentComplete = 0, newPercentage;
                ViewerRecord viewerRecord;
                ViewerRecord prevViewerRecord = null;

                while (bytesRead < fileSize)
                {
                    if (((BackgroundWorker)sender).CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    viewerRecord = new ViewerRecord(binaryReader);
                    count++;
                    StringBuilder payload = new StringBuilder(viewerRecord.ToString());
                    if (viewerRecord.PayloadLength != 0 && payload.ToString().Contains("\n"))
                    {
                        //We must convert from DS supplied Unix-style '\n' newlines to
                        //windows '\r\n' newlines which are the only ones notepad can handle.
                        payload.Insert(payload.Length - 1, '\r');
                    }

                    //Some lines are made up of multiple records, in this case we only want to write one timestamp
                    //at the start of the line.
                    if (prevViewerRecord == null || prevViewerRecord.Payload.Contains((byte)'\n'))
                    {
                        //Here we trim excess zeroes from the timespan's output.
                        TimeSpan t = TimeSpan.FromMilliseconds(viewerRecord.Timestamp);
                        string timeStamp = t.ToString();
                        //some timestamps don't have milliseconds, so just skip them.
                        if(!(timeStamp.Length < 11))
                            timeStamp = timeStamp.Substring(0, timeStamp.LastIndexOf('.')+3);
                        payload.Insert(0, timeStamp + " ");
                    }
                    streamWriter.Write(payload);

                    bytesRead += viewerRecord.Size;
                    prevViewerRecord = viewerRecord;

                    newPercentage = (int)((bytesRead * 100) / fileSize);
                    if (newPercentage > percentComplete)
                    {
                        iCopyLog.ReportProgress(newPercentage);
                        percentComplete = newPercentage;
                    }
                }
            }
        }    

        private void EventCopyLogProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            iCopyLogProgressBar.Value = e.ProgressPercentage;
        }

        private void EventCopyLogProgessBarCancelButtonClicked(object sender, RoutedEventArgs e)
        {
            iCopyLog.CancelAsync();
        }

        private void EventCopyLogRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                iCopyLogProgressBar.Close();
                return;
            }

            if(e.Error != null)
                AppendToMainWindow(String.Format("An error occurred: {0}\n", e.Error.Message));
            
            iProcess = new Process();
            iProcess.StartInfo.FileName = "notepad.exe";
            iProcess.StartInfo.Arguments = "\"" + iTempLogFile + "\"";
            iProcess.Start();
            iCopyLogProgressBar.Close();
        }

        private void EventSubnetSelectionChanged(object sender, EventArgs e)
        {
            iLibrary.SetCurrentSubnet(((Subnet)comboBoxSubnets.SelectedItem).Handle);
            iUserSettings.SelectedSubnet = comboBoxSubnets.SelectedIndex;
            UserSettings.WriteSettingsToFile(iSettingsFile, iUserSettings);
        }

        private void EventSelectAllClicked(object sender, RoutedEventArgs e)
        {
            lock (iDebugLevels)
            {
                foreach(DebugLevelItem i in iDebugLevels.Values)
                {
                    //don't turn on verbose as Viewer can't handle the flood of data.
                    if (i.Name == "Verbose")
                        continue;

                    i.PropertyChanged -= EventDebugLevelChanged;
                    i.Value = true;
                    i.PropertyChanged += EventDebugLevelChanged;
                }
            }
            EventDebugLevelChanged(null, null);
        }

        private void EventSelectNoneClicked(object sender, RoutedEventArgs e)
        {
            lock (iDebugLevels)
            {
                foreach (DebugLevelItem i in iDebugLevels.Values)
                {
                    i.PropertyChanged -= EventDebugLevelChanged;
                    i.Value = false;
                    i.PropertyChanged += EventDebugLevelChanged;
                }
            }
            EventDebugLevelChanged(null, null);
        }

        private void EventViewerOutputAvailable(object sender, EventArgs e)
        {
            //Make sure AppendRecordsToMainWindow is only executed by one thread at a time.
            iDisplayingRecordsMutex.WaitOne();

            if (iUpdatingTerminal)
            {
                iDisplayingRecordsMutex.ReleaseMutex();
                return;
            }

            iUpdatingTerminal = true;

            iDisplayingRecordsMutex.ReleaseMutex();

            AppendRecordsToMainWindow();
        }

        private void EventConnectionAccepted(object sender, EventArgs e)
        {
            lock (iConnectionChangedLock)
            {
                iDeviceResponded = true;
                ConnectedLightOn();
                AppendToMainWindow("Connected to " + iUserSettings.UglyName + "\n");
                SetWindowTitle("Viewer - Viewing " + iUserSettings.UglyName);
                iAppendToTerminal = true;
                TurnOffActivityLight();
            }
        }

        private void EventConnectionClosed(object sender, EventArgs e)
        {
            if (iShuttingDown)
                return;

            lock (iConnectionChangedLock)
            {
                ConnectedLightOff();
                AppendToMainWindow("Disconnected from " + iUserSettings.UglyName + "\n");
                RunRetryConnectTimer();
                SetWindowTitle("Viewer");
            }
        }

        private void EventConnectionRefused(object sender, EventArgs e)
        {
            AppendToMainWindow("Device refused our connection attempt, is another host already connected?");
            iDeviceResponded = true;
            TurnOffActivityLight();
        }

        private void EventServicesAvailable(object sender, DeviceEventArgs e)
        {
            lock (iCpProxyLinnCoUkDebug2Lock)
            {
                EnableDebugLevels();
                iDevice = e.Device;
                iCpProxyLinnCoUkDebug2 = new CpProxyLinnCoUkDebug2(iDevice.BaseDevice);
                iCpProxyLinnCoUkDebug2.BeginDebugLevel(EventGetDebugLevel);
            }
        }

        private void EventServicesUnavailable(object sender, DeviceEventArgs e)
        {
            lock (iCpProxyLinnCoUkDebug2Lock)
            {
                DisableDebugLevels();
                iCpProxyLinnCoUkDebug2 = null;
                iDevice = null;
            }
        }

        private void EventGetDebugLevel(uint aAsyncHandle)
        {
            if (iShuttingDown)
                return;

            TurnOffActivityLight();
            uint aaDebugLevel;
            lock (iCpProxyLinnCoUkDebug2Lock)
            {
                if (iCpProxyLinnCoUkDebug2 == null)
                    return;
                iCpProxyLinnCoUkDebug2.EndDebugLevel(aAsyncHandle, out aaDebugLevel);
            }

            lock (iDebugLevels)
            {
                foreach (DebugLevelItem i in iDebugLevels.Values)
                {
                    i.PropertyChanged -= EventDebugLevelChanged;
                    if ((i.DebugLevel & aaDebugLevel) == i.DebugLevel)
                        i.Value = true;
                    else
                        i.Value = false;
                    i.PropertyChanged += EventDebugLevelChanged;
                }
            }
        }

        private void EventSetDebugLevel(uint aAsyncHandle)
        {
            TurnOffActivityLight();
            lock (iCpProxyLinnCoUkDebug2Lock)
            {
                if (iCpProxyLinnCoUkDebug2 == null)
                    return;
                iCpProxyLinnCoUkDebug2.EndSetDebugLevel(aAsyncHandle);
            }
        }

        private void EventDebugLevelChanged(object sender, PropertyChangedEventArgs e)
        {
            lock (iCpProxyLinnCoUkDebug2Lock)
            {
                if (iCpProxyLinnCoUkDebug2 == null)
                    return;

                uint iDebugLevel = 0;

                lock (iDebugLevels)
                {
                    foreach (DebugLevelItem i in iDebugLevels.Values)
                    {
                        if (i.Value == true)
                        {
                            iDebugLevel += i.DebugLevel;
                        }
                    }
                }

                iCpProxyLinnCoUkDebug2.BeginSetDebugLevel(iDebugLevel, EventSetDebugLevel);
                TurnOnActivityLight();
            }
        }

        private void EventActivityLightTimerExpired(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (iSwitchOffActivityLight == true)
            {
                ActivityLightToWhite();
                iSwitchOffActivityLight = false;
            }
            else
            {
                iActivityLightTimerExpired = true;
            }
        }

        private void EventDelayMutexReleaseTimerExpired(object sender, System.Timers.ElapsedEventArgs e)
        {
            iDisplayingRecordsMutex.WaitOne();
            if (iViewerManager.AvailableRecords > 0)
            {
                AppendRecordsToMainWindow();
            }
            else
            {
                iUpdatingTerminal = false;
            }
            iDisplayingRecordsMutex.ReleaseMutex();
        }

        private void EventRetryConnectTimerExpired(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (iDeviceResponded)
            {
                iRetryConnectTimer.Stop();
            }
            else
            {
                AppendToMainWindow("Device did not respond. Retrying...\n");
                iViewerManager.Connect(iUserSettings.UglyName);
            }
        }

        private void EventMaxVisibleLinesCountChanged(object sender, Linn.LineCountChangedEventArgs e)
        {
            iMaxLineCount = e.LineCount;
            //Write any data left in the buffer to the screen. Now that the line count has changed, we
            //might have space to display it.
            AppendToMainWindow("");
        }

        private void EventWindowMainClosing(object sender, CancelEventArgs e)
        {
            iShuttingDown = true;
            iAppendToTerminal = false;
            UserSettings.WriteSettingsToFile(iSettingsFile, iUserSettings);

            if (iCopyLog != null && iCopyLog.IsBusy)
                iCopyLog.CancelAsync();
            if (iRetryConnectTimer.Enabled)
                iRetryConnectTimer.Enabled = false;
            if(File.Exists(iTempLogFile))
                File.Delete(iTempLogFile);
            if(File.Exists(iMainLogFileCopy))
                File.Delete(iMainLogFileCopy);

            iViewerManager.Dispose();
            lock (iCpProxyLinnCoUkDebug2Lock)
            {
                if (iCpProxyLinnCoUkDebug2 != null)
                    iCpProxyLinnCoUkDebug2.Dispose();
            }
            iDeviceList.Dispose();
            iSubnetList.Dispose();
            iLibrary.Close();
        }

        private void DisableDebugLevels()
        {
            Dispatcher.BeginInvoke(new Action(delegate()
            {
                listBoxDebugLevel.IsEnabled = false;
                selectAllButton.IsEnabled = false;
                selectNoneButton.IsEnabled = false;
            }), null);
        }

        private void EnableDebugLevels()
        {
            Dispatcher.BeginInvoke(new Action(delegate()
            {
                listBoxDebugLevel.IsEnabled = true;
                selectAllButton.IsEnabled = true;
                selectNoneButton.IsEnabled = true;
            }), null);
        }

        private void RunRetryConnectTimer()
        {
            //We only want to run one 'retry connection' Timer at a time.
            if (iRetryConnectTimer.Enabled)
            {
                return;
            }
            else
            {
                iRetryConnectTimer.Start();
            }
        }

        private void Connect(string aUglyName)
        {
            if (aUglyName.Equals(""))
                return;

            //lock here to ensure this function is reentrant.
            lock (aUglyName)
            {
                AppendToMainWindow("Connecting to " + aUglyName + "...\n");
                iAppendToTerminal = false;
                EventClearLogButtonClicked(null, null);
                try
                {
                    iViewerManager.Connect(aUglyName);
                }
                catch (SocketException)
                {
                    AppendToMainWindow("Ugly name mDNS lookup failed, retrying, did you type the name in correctly?\n");
                }

                iUserSettings.UglyName = aUglyName;
                RunRetryConnectTimer();
            }
        }

        private void TurnOnActivityLight()
        {
            if (iActivityLightTimer.Enabled)
            {
                iActivityLightTimer.Stop();
                iSwitchOffActivityLight = false;
            }
            iActivityLightTimer.Start();
            ActivityLightToOrange();
        }

        private void TurnOffActivityLight()
        {
            if (iActivityLightTimerExpired == true)
            {
                ActivityLightToWhite();
                iActivityLightTimerExpired = false;
            }
            else
            {
                iSwitchOffActivityLight = true;
            }
        }

        private void ActivityLightToOrange()
        {
            Dispatcher.BeginInvoke(new Action(delegate()
            {
                activityLight.Fill = new SolidColorBrush(Colors.Orange);
            }));
        }

        private void ActivityLightToWhite()
        {
            Dispatcher.BeginInvoke(new Action(delegate()
            {
                activityLight.Fill = new SolidColorBrush(Colors.White);
            }));
        }

        private void ConnectedLightOn()
        {
            Dispatcher.BeginInvoke(new Action(delegate()
            {
            connectedLight.Fill = new SolidColorBrush(Colors.Green);
            }));
        }

        private void ConnectedLightOff()
        {
            Dispatcher.BeginInvoke(new Action(delegate()
            {
                connectedLight.Fill = new SolidColorBrush(Colors.Red);
            }));
        }

        private void SetWindowTitle(string aMessage)
        {
            Dispatcher.BeginInvoke(new Action(delegate()
            {
                MainWindow.Title = aMessage;
            }));
        }

        private void AppendToMainWindow(string aMessage)
        {
            Dispatcher.BeginInvoke(new Action(delegate()
            {
                /*We only pass as many lines as are visible on the terminal to terminal.Text and
                 *discard the rest. This allows us to keep our memory footprint down.*/
                StringBuilder text = new StringBuilder(terminal.Text + aMessage);
                string[] lines = text.ToString().Split('\n');

                if (lines.Length >= iMaxLineCount)
                {
                    text = new StringBuilder();
                    for (int i = lines.Length - iMaxLineCount; i < (lines.Length - 1); i++)
                    {
                        text.Append(lines[i] + "\n");
                    }
                    text.Append(lines[lines.Length - 1]);
                }

                terminal.Text = text.ToString();
            }));
        }

        private void AppendRecordsToMainWindow()
        {
            if (!iAppendToTerminal)
            {
                iUpdatingTerminal = false;
                return;
            }

            Dispatcher.BeginInvoke(new Action(delegate()
            {
                StringBuilder iViewerOutputText = new StringBuilder();
                List<ViewerRecord> records = iViewerManager.ViewerOutput;
                //equivalent to Clear(). However, Clear() requires .NET v4.0+
                iViewerOutputText.Length = 0;
                AppendToLogFile(records);

                foreach (ViewerRecord r in records)
                {
                    iViewerOutputText.Append(r.ToString());
                }

                StringBuilder text = new StringBuilder(terminal.Text + iViewerOutputText.ToString());
                string[] lines = text.ToString().Split('\n');

                if (lines.Length >= iMaxLineCount)
                {
                    text = new StringBuilder();

                    for (int i = lines.Length - iMaxLineCount; i < (lines.Length - 1); i++)
                    {
                        text.Append(lines[i] + "\n");
                    }
                    text.Append(lines[lines.Length - 1]);
                }

                terminal.Text = text.ToString();
                /*Delay the next call so ViewerRecords has time to fill up. This helps increase
                 *performance as it reduces the amount of overhead from switching threads*/
                iDelayMutexReleaseTimer.Start();
            }));
        }

        private void AppendToLogFile(List<ViewerRecord> aViewerRecords)
        {
            iLogFileMutex.WaitOne();
            using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(iMainLogFile, FileMode.OpenOrCreate, FileAccess.Write)))
            {
                //we want to append but FileAccess only has a write option, so seek to the end.
                binaryWriter.Seek(0, SeekOrigin.End);
                foreach (ViewerRecord vR in aViewerRecords)
                {
                    binaryWriter.Write(vR.Id);
                    binaryWriter.Write(vR.Timestamp);
                    binaryWriter.Write(vR.PayloadLength);
                    binaryWriter.Write(vR.Payload);          
                }
                this.LogFileRecords += aViewerRecords.Count;
            }   
            iLogFileMutex.ReleaseMutex();
        }

        public long LogFileRecords
        {
            get { return (long)GetValue(LogFileRecordsProperty); }
            set { SetValue(LogFileRecordsProperty, value); }
        }

        public static readonly DependencyProperty LogFileRecordsProperty =
            DependencyProperty.Register("LogFileRecords", typeof(long), typeof(WindowMain), new UIPropertyMetadata((long)0));

        private Library iLibrary;
        private SortedList<string, DebugLevelItem> iDebugLevels;
        private SubnetList iSubnetList;
        private InitParams iInitParams;
        private DeviceListUpnp iDeviceList;
        private DeviceSelectionWindow iDeviceSelectionWindow;
        private UserSettings iUserSettings;
        private Object iConnectionChangedLock = new Object();
        private bool iDeviceResponded = false;
        private System.Timers.Timer iRetryConnectTimer = new System.Timers.Timer();
        private CpProxyLinnCoUkDebug2 iCpProxyLinnCoUkDebug2;
        private Object iCpProxyLinnCoUkDebug2Lock = new Object();
        private DeviceUpnp iDevice;
        private System.Timers.Timer iActivityLightTimer = new System.Timers.Timer();
        private bool iSwitchOffActivityLight = false;
        private bool iActivityLightTimerExpired = false;
        private ViewerManager iViewerManager;
        private Mutex iDisplayingRecordsMutex = new Mutex();
        private bool iUpdatingTerminal = false;
        //we use iAppendToTerminal to stop buffered output from a previous device being displayed whilst connecting to a new device.
        private bool iAppendToTerminal = true;
        private System.Timers.Timer iDelayMutexReleaseTimer = new System.Timers.Timer();
        private string iAppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Viewer";
        private string iMainLogFile;
        private string iMainLogFileCopy;
        private string iTempLogFile;
        private string iSettingsFile;
        private Mutex iLogFileMutex = new Mutex();
        private BackgroundWorker iCopyLog;
        private ProgressBarWindow iCopyLogProgressBar;
        private Process iProcess;
        private int iMaxLineCount;
        private const int iFindDeviceTimeout = 3000;
        private const long iActivityLightTimerTimeout = 100;
        private const double iDelayMutexReleaseTimerTimeout = 50.0;
        private const long iRetryConnectTimerTimeout = 2000;
        private Mutex iInstanceMutex;
        private bool iShuttingDown = false;
    }

    public class DebugLevelItem : INotifyPropertyChanged
    {
        public DebugLevelItem(string aName, UInt32 aDebugLevel)
        {
            iName = aName;
            iDebugLevel = aDebugLevel;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String aProperty)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(aProperty));
            }
        }

        public string Name
        {
            get
            {
                return (iName);
            }
        }

        public bool Value
        {
            get
            {
                return (iValue);
            }

            set
            {
                if (iValue != value)
                {
                    iValue = value;
                    NotifyPropertyChanged("Value");
                }
            }
        }

        public UInt32 DebugLevel
        {
            get
            {
                return iDebugLevel;
            }
        }

        public override string ToString()
        {
            return iName;
        }

        private string iName;
        private bool iValue;
        private UInt32 iDebugLevel;
    }

    [ValueConversion(typeof(long), typeof(String))]
    internal class LongToStringConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            long t = (long)value;
            return t.ToString()+ " Records";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string longNumber = (string)value;
            return long.Parse(longNumber.Substring(6));
        }
    }

    public class UserSettings
    {
        public UserSettings()
        {
            iUglyName = "";
            iSelectedSubnet = 0;
        }

        public static void WriteSettingsToFile(string aFileName, UserSettings aSettings)
        {
            using (TextWriter writer = new StreamWriter(new FileStream(aFileName, FileMode.Create), new UTF8Encoding()))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(UserSettings));
                serializer.Serialize(writer, aSettings);
            }
        }

        public static UserSettings ReadSettingsFromFile(string aFileName)
        {
            using (TextReader reader = new StreamReader(new FileStream(aFileName, FileMode.Open), new UTF8Encoding()))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(UserSettings));
                Object settings = serializer.Deserialize(reader);
                if (settings.GetType() != typeof(UserSettings))
                    throw new FileFormatException("Unrecognised object type in " + aFileName);
                else
                    return (UserSettings)settings;
            }
        }

        public string UglyName
        {
            get
            {
                return iUglyName;
            }
            set
            {
                iUglyName = value;
            }
        }

        public int SelectedSubnet
        {
            get
            {
                return iSelectedSubnet;
            }
            set
            {
                iSelectedSubnet = value;
            }
        }

        private string iUglyName;
        private int iSelectedSubnet;
    }
}
