using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace SneakyRadio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [MTAThread]
        static void Main(string[] aArgs)
        {
            App app = new App();
            app.Run(new Window1());
        }
    }
}
