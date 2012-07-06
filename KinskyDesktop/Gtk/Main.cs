using System;

using Gtk;

using Linn;

namespace KinskyDesktopGtk
{
    class MainClass
    {
        public static void Main (string[] args)
        {
            Application.Init ();

            Controller controller = new Controller(args);
            controller.Start();

            Application.Run ();

            controller.Stop();
            controller.Dispose();
        }
    }
}
