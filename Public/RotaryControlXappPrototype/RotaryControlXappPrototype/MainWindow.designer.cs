// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace RotaryControlXappPrototype
{
 [Register ("MainWindowController")]
 partial class MainWindowController
 {
     [Outlet]
     MonoMac.WebKit.WebView iWebView { get; set; }
     
     void ReleaseDesignerOutlets ()
     {
         if (iWebView != null) {
             iWebView.Dispose ();
             iWebView = null;
         }
     }
 }

 [Register ("MainWindow")]
 partial class MainWindow
 {
     
     void ReleaseDesignerOutlets ()
     {
     }
 }
}
