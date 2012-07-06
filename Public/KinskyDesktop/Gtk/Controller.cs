using System;

using Linn;

namespace KinskyDesktopGtk
{
    public class Controller : IDisposable
    {
        public Controller(string[] aArgs)
        {
            iHelper = new HelperKinskyDesktop(aArgs);
            iHelper.ProcessOptionsFileAndCommandLine();

            iWindow = new MainWindow(iHelper);
            iModel = new Model(iHelper);
        }

        public void Dispose()
        {
            iHelper.Dispose();
        }

        public void Start()
        {
            iWindow.Show();
            iHelper.Stack.Start();
        }

        public void Stop()
        {
            iWindow.Dispose();
            iHelper.Stack.Stop();
        }

        public void Rescan()
        {
            iHelper.Rescan();
            iModel.Rescan();
        }

        private HelperKinskyDesktop iHelper;
        private Model iModel;
        private MainWindow iWindow;
    }
}

