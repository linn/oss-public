using System;
using System.Net;

using Gtk;

using Linn;

namespace KinskyDesktopGtk
{
    public partial class MainWindow : Window
    {    
        public MainWindow (Helper aHelper)
            : base (WindowType.Toplevel)
        {
            Build ();
        }

        protected void OnDeleteEvent (object sender, DeleteEventArgs e)
        {
            Application.Quit ();
            e.RetVal = true;
        }

        protected override bool OnExposeEvent (Gdk.EventExpose evnt)
        {
            Gdk.Window window = evnt.Window;

            Gdk.GC gc = new Gdk.GC(window);

            // render header
            Gdk.Pixbuf pb = KinskyDesktopGtk.Properties.ResourceManager.ImageTopLeftEdge;
            pb.RenderToDrawable(window, gc, 0, 0, 0, 0, pb.Width, pb.Height, Gdk.RgbDither.Normal, 0, 0);

            int x = pb.Width;
            int endHeaderY = pb.Height;

            pb = KinskyDesktopGtk.Properties.ResourceManager.ImageTopRightEdge;
            pb.RenderToDrawable(window, gc, 0, 0, evnt.Area.Width - pb.Width, 0, pb.Width, pb.Height, Gdk.RgbDither.Normal, 0, 0);

            int width = evnt.Area.Width - pb.Width - x;
            pb = KinskyDesktopGtk.Properties.ResourceManager.ImageTopFiller;
            pb = pb.ScaleSimple(width, pb.Height, Gdk.InterpType.Bilinear);
            pb.RenderToDrawable(window, gc, 0, 0, x, 0, pb.Width, pb.Height, Gdk.RgbDither.Normal, 0, 0);

            // render footer
            pb = KinskyDesktopGtk.Properties.ResourceManager.ImageBottomLeftEdge;
            int y = evnt.Area.Bottom - pb.Height;
            pb.RenderToDrawable(window, gc, 0, 0, 0, y, pb.Width, pb.Height, Gdk.RgbDither.Normal, 0, 0);

            x = pb.Width;

            pb = KinskyDesktopGtk.Properties.ResourceManager.ImageBottomRightEdge;
            pb.RenderToDrawable(window, gc, 0, 0, evnt.Area.Width - pb.Width, y, pb.Width, pb.Height, Gdk.RgbDither.Normal, 0, 0);

            width = evnt.Area.Width - pb.Width - x;
            pb = KinskyDesktopGtk.Properties.ResourceManager.ImageBottomFiller;
            pb = pb.ScaleSimple(width, pb.Height, Gdk.InterpType.Bilinear);
            pb.RenderToDrawable(window, gc, 0, 0, x, y, pb.Width, pb.Height, Gdk.RgbDither.Normal, 0, 0);

            //render sides
            pb = KinskyDesktopGtk.Properties.ResourceManager.ImageLeftFiller;
            int height = y - endHeaderY;
            pb = pb.ScaleSimple(pb.Width, height, Gdk.InterpType.Bilinear);
            pb.RenderToDrawable(window, gc, 0, 0, 0, endHeaderY, pb.Width, pb.Height, Gdk.RgbDither.Normal, 0, 0);

            pb = KinskyDesktopGtk.Properties.ResourceManager.ImageRightFiller;
            pb = pb.ScaleSimple(pb.Width, height, Gdk.InterpType.Bilinear);
            pb.RenderToDrawable(window, gc, 0, 0, evnt.Area.Width - pb.Width, endHeaderY, pb.Width, pb.Height, Gdk.RgbDither.Normal, 0, 0);

            return true;
        }
    }
}
