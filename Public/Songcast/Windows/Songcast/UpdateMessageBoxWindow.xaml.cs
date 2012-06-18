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
    public partial class UpdateMessageBoxWindow : Window
    {
        public UpdateMessageBoxWindow()
        {
            InitializeComponent();

            MemoryStream iconStream = new MemoryStream();
            ResourceManager.IconSongcaster.Save(iconStream);
            iconStream.Seek(0, SeekOrigin.Begin);
            Icon = BitmapFrame.Create(iconStream);

            image1.Source = ResourceManager.Icon;
        }

        public EventHandler<EventArgs> EventUpdate;
        public EventHandler<EventArgs> EventPreferences;

        private void buttonPreferences_Click(object sender, RoutedEventArgs e)
        {
            if (EventPreferences != null)
            {
                EventPreferences(this, EventArgs.Empty);
            }
        }

        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (EventUpdate != null)
            {
                EventUpdate(this, EventArgs.Empty);
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
