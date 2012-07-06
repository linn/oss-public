using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Globalization;

using Linn.Toolkit;


namespace Linn.Songcast
{
    /// <summary>
    /// Interaction logic for PreferencesWindow.xaml
    /// </summary>
    public partial class PreferencesWindow : Window
    {
        public PreferencesWindow(IHelper aHelper, PreferenceBindings aBindings, Model aModel, HelperAutoUpdate aHelperAutoUpdate)
        {
            InitializeComponent();

            iModel = aModel;
            iHelperAutoUpdate = aHelperAutoUpdate;

            // set the bindings for the window
            DataContext = aBindings;

            // fill in about page info
            textBlockProduct.Text = aHelper.Product;
            textBlockVersion.Text = string.Format("Version {0} {1}", aHelper.Version, aHelper.Family);
            textBlockCopyright.Text = aHelper.Copyright;

            MemoryStream iconStream = new MemoryStream();
            ResourceManager.IconSongcaster.Save(iconStream);
            iconStream.Seek(0, SeekOrigin.Begin);
            Icon = BitmapFrame.Create(iconStream);
            image1.Source = ResourceManager.Icon;
            image2.Source = ResourceManager.ImageRotaryControl;
            image3.Source = ResourceManager.ImageRockerControl;
        }

        public event EventHandler EventButtonHelpClicked;

        private void ButtonRefreshClick(object sender, RoutedEventArgs e)
        {
            iModel.RefreshReceiverList();
        }

        private void ButtonCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonHelpClick(object sender, RoutedEventArgs e)
        {
            if (EventButtonHelpClicked != null)
            {
                EventButtonHelpClicked(this, EventArgs.Empty);
            }
        }

        private void ButtonNewChannelClick(object sender, RoutedEventArgs e)
        {
            Random r = new Random();
            int byte1 = r.Next(254) + 1;    // in range [1,254]
            int byte2 = r.Next(254) + 1;    // in range [1,254]
            int channel = byte1 << 8 | byte2;

            PreferenceBindings p = DataContext as PreferenceBindings;
            p.Channel = (uint)channel;
        }

        private void ButtonMusicLatencyDefaultClick(object sender, RoutedEventArgs e)
        {
            PreferenceBindings p = DataContext as PreferenceBindings;
            p.MusicLatency = iModel.Preferences.DefaultMusicLatencyMs;
        }

        private void ButtonVideoLatencyDefaultClick(object sender, RoutedEventArgs e)
        {
            PreferenceBindings p = DataContext as PreferenceBindings;
            p.VideoLatency = iModel.Preferences.DefaultVideoLatencyMs;
        }

        private void ButtonUpdateCheckClick(object sender, RoutedEventArgs e)
        {
            iHelperAutoUpdate.CheckForUpdates();
        }

        private void TextBoxPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // regex matches characters that are not numbers
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            // mark as handled if the text matches the regex
            e.Handled = regex.IsMatch(e.Text);
        }

        private Model iModel;
        private HelperAutoUpdate iHelperAutoUpdate;
    }

    public class ValidationRuleLatency : ValidationRule
    {
        public ValidationRuleLatency()
        {
        }

        public uint Min { get; set; }
        public uint Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string val = value as string;
            uint number = 0;

            if (val != null
                && UInt32.TryParse(val, out number)
                && number >= Min
                && number <= Max)
            {
                return new ValidationResult(true, null);
            }

            return new ValidationResult(false, null);
        }
    }
}
