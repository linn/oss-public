using System;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Data;

namespace Linn.Songcast
{
    /// <summary>
    /// Interaction logic for PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        private const string kOnlineManualUrl = "http://oss.linn.co.uk/trac/wiki/SongcastWindowsDavaarManual";

        private ModelController iModelController;
        private FormSysTray iFormSysTray;

        public PreferencesWindow(IHelper aHelper, ModelController aModelController, FormSysTray aFormSysTray)
        {
            iModelController = aModelController;
            iModelController.EventEnabledChanged += EnabledChanged;

            iFormSysTray = aFormSysTray;

            InitializeComponent();

            DataContext = iModelController;

            textBlockProduct.Text = aHelper.Product;
            textBlockVersion.Text = string.Format("Version {0} {1}", aHelper.Version, aHelper.Family);
            textBlockCopyright.Text = aHelper.Copyright;

            MemoryStream iconStream = new MemoryStream();
            ResourceManager.IconSongcaster.Save(iconStream);
            iconStream.Seek(0, SeekOrigin.Begin);
            Icon = BitmapFrame.Create(iconStream);

            image1.Source = ResourceManager.Icon;
            listBox1.ItemsSource = aModelController.MediaPlayerList;
            comboBoxNetwork.ItemsSource = aModelController.SubnetList;

            SetEnabled(iModelController.Enabled);
            checkBox1.IsChecked = iModelController.ShowInSysTray;
            border2.Visibility = iModelController.FirstRun ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            tabControl1.Visibility = iModelController.FirstRun ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

            comboBoxNetwork.SelectionChanged += comboBoxNetwork_SelectionChanged;
            sliderLatency.ValueChanged += sliderLatency_ValueChanged;
        }

        private void EnabledChanged(object sender, EventArgs e)
        {
            SetEnabled(iModelController.Enabled);
        }

        private void SetEnabled(bool aEnabled)
        {
            if (aEnabled)
            {
                button1.Content = "Turn " + App.kName + " Off";
            }
            else
            {
                button1.Content = "Turn " + App.kName + " On";
            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            iModelController.Enabled = !iModelController.Enabled;
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            Close();

            if (iModelController.ShowBalloonTip)
            {
                iFormSysTray.ShowBalloonTip();
                iModelController.ShowBalloonTip = false;
            }
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
            iModelController.ShowInSysTray = true;
        }

        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            iModelController.ShowInSysTray = false;
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            iModelController.RefreshReceiverList();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            iModelController.ReconnectSelectedReceivers();
        }

        private void comboBoxNetwork_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int index = comboBoxNetwork.SelectedIndex;

            if (index >= 0)
            {
                iModelController.SelectedSubnetIndex = index;
            }
        }

        private void buttonNew_Click(object sender, RoutedEventArgs e)
        {
            iModelController.NewChannel();
        }

        private void buttonHelp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(kOnlineManualUrl);
            }
            catch (Exception)
            {
                MessageBox.Show("Warning", "Failed to contact " + kOnlineManualUrl + "\n\nFailed to open online manual", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void buttonUpdateCheck_Click(object sender, RoutedEventArgs e)
        {
            iModelController.CheckForUpdates(this);
        }

        private void checkBoxAutoUpdate_Clicked(object sender, RoutedEventArgs e)
        {
            iModelController.AutomaticUpdateChecks = checkBoxAutoUpdate.IsChecked.Value;
        }

        private void checkBoxBeta_Clicked(object sender, RoutedEventArgs e)
        {
            iModelController.ParticipateInBeta = checkBoxBeta.IsChecked.Value;
        }

        private void sliderLatency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            iModelController.Latency = (uint)sliderLatency.Value;
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            border2.Visibility = System.Windows.Visibility.Collapsed;
            tabControl1.Visibility = System.Windows.Visibility.Visible;

            iModelController.FirstRun = false;
        }
    }
}
