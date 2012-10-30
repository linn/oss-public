using System;

using Gtk;
using WebKit;

namespace Linn.Konfig
{
    public partial class MainWindow : Window, IUpdateListener
    {    
        public MainWindow (Helper aHelper)
            : base (WindowType.Toplevel)
        {
            SetSizeRequest(980, 650);
            
            iWebView = new WebView();
            iWebView.SetSizeRequest(980, 650);
            
            Add(iWebView);
        }
        
        public void SetUpdating(bool aUpdating)
        {
        }
        
        public WebView WebView
        {
            get
            {
                return iWebView;
            }
        }

        protected void OnDeleteEvent (object sender, DeleteEventArgs e)
        {
            Application.Quit ();
            e.RetVal = true;
        }

        /*protected override bool OnExposeEvent (Gdk.EventExpose evnt)
        {
            Gdk.Window window = evnt.Window;

            window.Add(iWebView); 
            
            return true;
        }*/
        
        private WebView iWebView;
    }
}
