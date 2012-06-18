using System;
using System.Windows;

using System.Threading;

namespace Linn.SongcastPreferences
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            bool createdNew;
            iWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, "LinnSongcastOpenPreferences", out createdNew);

            if (createdNew)
            {
                MessageBox.Show("Linn Songcast is not running", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                iWaitHandle.Set();
            }

            Close();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            iWaitHandle.Set();
        }

        private EventWaitHandle iWaitHandle;
    }
}
