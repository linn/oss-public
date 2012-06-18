using System;
using System.Collections;
using System.Windows;
using Zapp;

namespace Viewer
{
    public partial class DeviceSelectionWindow : Window
    {
        public DeviceSelectionWindow(IEnumerable aCollection, string aUglyName)
        {
            InitializeComponent();
            uglyNameTextBox.Text = aUglyName;
            comboBoxDevices.ItemsSource = aCollection;
        }

        private void EventOkButtonClicked(object sender, RoutedEventArgs e)
        {
            if (uglyNameTextBox.Text == "")
            {
                MessageBox.Show("You must enter an ugly name or select one from the text box", "No Device Selected",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            if (!ViewerManager.IsValidUglyName(uglyNameTextBox.Text))
                ShowUglyNameParseError();

            DialogResult = true;
            iUglyName = uglyNameTextBox.Text;
            this.Close();
        }

        private void EventCancelButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void EventDeviceSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (comboBoxDevices.SelectedIndex == -1)
                return;

            DeviceUpnp device = (DeviceUpnp)comboBoxDevices.SelectedItem;
            uglyNameTextBox.Text = ViewerManager.GetUglyName(device.Udn);
        }

        private void ShowUglyNameParseError()
        {
            MessageBox.Show("Invalid ugly name entered, please check you have entered it correctly", "Ugly Name Parse Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public string UglyName
        {
            get
            {
                return iUglyName;
            }
        }

        private string iUglyName;
    }
}
