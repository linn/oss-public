using System;
using System.Windows;
using System.Threading;
using System.IO;
using System.Windows.Media.Imaging;

namespace Linn.Songcast
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        private enum EState
        {
            eChecking,
            eAvailable,
            eUnavailable,
            eDownloading,
            eUpdating,
            eFailed
        }

        private AutoUpdate iAutoUpdate;
        private AutoUpdate.AutoUpdateInfo iUpdateInfo;

        private EState iState;
        private bool iUpdateStarted;
        private Thread iThread;

        public UpdateWindow(AutoUpdate aAutoUpdate)
        {
            iAutoUpdate = aAutoUpdate;

            InitializeComponent();

            MemoryStream iconStream = new MemoryStream();
            ResourceManager.IconSongcaster.Save(iconStream);
            iconStream.Seek(0, SeekOrigin.Begin);
            Icon = BitmapFrame.Create(iconStream);

            iUpdateInfo = null;
            iState = EState.eChecking;
            iUpdateStarted = false;
            UpdateControls();
        }

        public UpdateWindow(Window aParent, AutoUpdate aAutoUpdate)
            : this(aAutoUpdate)
        {
            Owner = aParent;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        public bool UpdateStarted
        {
            get
            {
                return iUpdateStarted;
            }
        }

        private void UpdateControls()
        {
            switch (iState)
            {
                case EState.eChecking:
                    progressBar.IsIndeterminate = true;
                    textBlock.Text = "Checking for updates...";
                    buttonDetails.IsEnabled = false;
                    buttonUpdate.IsEnabled = false;
                    buttonClose.IsEnabled = true;
                    break;

                case EState.eUnavailable:
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = 0;
                    textBlock.Text = "There are no updates available.";
                    buttonClose.Content = "Close";
                    buttonDetails.IsEnabled = false;
                    buttonUpdate.IsEnabled = false;
                    buttonClose.IsEnabled = true;
                    break;

                case EState.eAvailable:
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = 0;
                    textBlock.Text = string.Format("There is a new version of {0} ({1}) available.", iUpdateInfo.Name, iUpdateInfo.Version);
                    buttonClose.Content = "Not Now";
                    buttonDetails.IsEnabled = true;
                    buttonUpdate.IsEnabled = true;
                    buttonClose.IsEnabled = true;
                    break;

                case EState.eDownloading:
                    textBlock.Text = string.Format("Downloading {0} ({1}).", iUpdateInfo.Name, iUpdateInfo.Version);
                    buttonDetails.IsEnabled = true;
                    buttonUpdate.IsEnabled = false;
                    buttonClose.IsEnabled = true;
                    break;

                case EState.eUpdating:
                    progressBar.IsIndeterminate = true;
                    textBlock.Text = string.Format("Updating {0} to {1}.", iUpdateInfo.Name, iUpdateInfo.Version);
                    buttonDetails.IsEnabled = false;
                    buttonUpdate.IsEnabled = false;
                    buttonClose.IsEnabled = false;
                    break;

                case EState.eFailed:
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = 0;
                    textBlock.Text = string.Format("The update failed.");
                    buttonClose.Content = "Close";
                    buttonDetails.IsEnabled = false;
                    buttonUpdate.IsEnabled = false;
                    buttonClose.IsEnabled = true;
                    break;
            }
        }

        private void AutoUpdateProgress(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
            {
                progressBar.Value = iAutoUpdate.UpdateProgress;
            });
        }

        private void AutoUpdateFailed(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
            {
                iState = EState.eFailed;
                UpdateControls();
            });
        }

        private void ThreadFuncCheck()
        {
            iUpdateInfo = iAutoUpdate.CheckForUpdate();

            Dispatcher.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
            {
                iState = (iUpdateInfo != null) ? EState.eAvailable : EState.eUnavailable;
                iThread = null;

                UpdateControls();
            });
        }

        private void ThreadFuncUpdate()
        {
            // download
            iAutoUpdate.DownloadUpdate(iUpdateInfo);

            // update UI
            Dispatcher.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
            {
                iState = EState.eUpdating;
                UpdateControls();
            });

            // start the update
            if (iAutoUpdate.ApplyUpdate(iUpdateInfo))
            {
                // update started successfully - close the dialog
                Dispatcher.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
                {
                    iUpdateStarted = true;
                    iThread = null;
                    Close();
                });
            }
            else
            {
                // failed to update
                Dispatcher.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
                {
                    iThread = null;
                });
            }
        }

        private void buttonDetails_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(iUpdateInfo.History.AbsoluteUri);
            }
            catch (Exception)
            {
                MessageBox.Show("Warning", "Failed to contact " + iUpdateInfo.History + "\n\nFailed to retrieve update details", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (iUpdateInfo.IsCompatibilityFamilyUpgrade)
            {
                MessageBoxResult r = MessageBox.Show("Warning", "This is a compatibility family upgrade. Do you wish to continue with the upgrade?" + "\n\nUpdating " + iUpdateInfo.Name + " to a new compatibility family will also require updating Linn DS firmware.", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (r == MessageBoxResult.No)
                {
                    return;
                }
            }

            iState = EState.eDownloading;
            UpdateControls();

            iAutoUpdate.EventUpdateProgress += AutoUpdateProgress;
            iAutoUpdate.EventUpdateFailed += AutoUpdateFailed;

            iThread = new Thread(ThreadFuncUpdate);
            iThread.IsBackground = true;
            iThread.Name = "Update";
            iThread.Start();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (iThread != null)
            {
                iThread.Abort();
                iThread.Join();
                iThread = null;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            iThread = new Thread(ThreadFuncCheck);
            iThread.IsBackground = true;
            iThread.Name = "UpdateCheck";
            iThread.Start();
        }
    }
}
