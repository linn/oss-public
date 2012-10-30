using System;

using Gtk;

using Linn;

namespace Linn.Konfig
{
    class MainClass
    {
        public static void Main (string[] args)
        {
            Application.Init ();

            Controller controller = new Controller(args);

            Application.Run ();

            controller.Dispose();
        }
    }
}
