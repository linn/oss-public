using System;
using System.Threading;

using Gtk;

using OpenHome.Xapp;

namespace Linn.Konfig
{
    public class Controller : IDisposable
    {
        private class Invoker : IInvoker
        {
            public Invoker()
            {
                iMainThread = Thread.CurrentThread;
            }

            public bool InvokeRequired
            {
                get
                {
                    return Thread.CurrentThread != iMainThread;
                }
            }

            public void BeginInvoke(Delegate aDelegate, params object[] aArgs)
            {
                Application.Invoke(delegate {
                    aDelegate.DynamicInvoke(aArgs);
                });
            }

            public bool TryBeginInvoke(Delegate aDelegate, params object[] aArgs)
            {
                if(InvokeRequired)
                {
                    BeginInvoke(aDelegate, aArgs);
                    return true;
                }

                return false;
            }

            private Thread iMainThread;
        }
        
        public Controller(string[] aArgs)
        {
            iHelper = new Helper(aArgs);
            iHelper.ProcessOptionsFileAndCommandLine();

            iMainWindow = new MainWindow(iHelper);
            iMainWindow.ShowAll();
            
            Preferences preferences = new Preferences(iHelper);
            Model.Instance = new Model(preferences);

            // create the xapp controller and view
            Invoker invoker = new Invoker();
            PageBase page = new SettingsPageBasic(invoker, preferences, "settings", "settings");
            iXappController = new XappController(invoker, iHelper, Model.Instance, preferences, page, iMainWindow);
            iViewer = new ViewerBrowser(iMainWindow.WebView, iXappController.MainPageUri);
        }

        public void Dispose()
        {
            iHelper.Dispose();
            iMainWindow.Dispose();
        }

        private Helper iHelper;
        private MainWindow iMainWindow;
        private XappController iXappController;
        private ViewerBrowser iViewer;
    }
}

