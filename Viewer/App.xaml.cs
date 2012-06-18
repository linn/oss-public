using System.Windows;
using System;

namespace Viewer
{
    public partial class App : Application
    {
        [STAThread()]
        static void Main()
        {
            App app = new App();
            app.Run();
        }
		
		public App()
        {
            InitializeComponent();
        }
    }
}
