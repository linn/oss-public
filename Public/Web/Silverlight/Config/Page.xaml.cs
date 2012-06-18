using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SilverlightConfig
{
    public partial class Page : UserControl
    {
        public Page()
        {
            InitializeComponent();

            string host = App.Current.Host.Source.Host;
            int port = App.Current.Host.Source.Port;
            string root = "http://" + host + ":" + port + "/";
            iServiceDsPlay = new ServiceDs.PlayPortTypeClient("PlayPort", root + "Ds/Ds/control");
            iServiceDsPause = new ServiceDs.PausePortTypeClient("PausePort", root + "Ds/Ds/control");
            iServiceDsStop = new ServiceDs.StopPortTypeClient("StopPort", root + "Ds/Ds/control");
        }

        private void ButtonPlayClick(object sender, RoutedEventArgs e)
        {
            iServiceDsPlay.PlayAsync();
        }

        private void ButtonPauseClick(object sender, RoutedEventArgs e)
        {
            iServiceDsPause.PauseAsync();
        }

        private void ButtonStopClick(object sender, RoutedEventArgs e)
        {
            iServiceDsStop.StopAsync();
        }

        ServiceDs.PlayPortTypeClient iServiceDsPlay;
        ServiceDs.PausePortTypeClient iServiceDsPause;
        ServiceDs.StopPortTypeClient iServiceDsStop;
    }
}
