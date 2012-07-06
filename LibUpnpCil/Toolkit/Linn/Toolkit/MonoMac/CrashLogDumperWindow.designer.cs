// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Linn.Toolkit.Mac
{
 [Register ("CrashLogDumperWindowController")]
 partial class CrashLogDumperWindowController
 {
     [Outlet]
     MonoMac.AppKit.NSImageView Image { get; set; }

     [Outlet]
     MonoMac.AppKit.NSTextField HeaderText { get; set; }

     [Outlet]
     MonoMac.AppKit.NSTextField FooterText { get; set; }

     [Outlet]
     MonoMac.AppKit.NSButton ButtonDetails { get; set; }

     [Outlet]
     MonoMac.AppKit.NSButton ButtonCancel { get; set; }

     [Outlet]
     MonoMac.AppKit.NSButton ButtonSend { get; set; }

     [Outlet]
     MonoMac.AppKit.NSTextView Details { get; set; }

     [Outlet]
     MonoMac.AppKit.NSScrollView ScrollView { get; set; }
     
     void ReleaseDesignerOutlets ()
     {
         if (Image != null) {
             Image.Dispose ();
             Image = null;
         }

         if (HeaderText != null) {
             HeaderText.Dispose ();
             HeaderText = null;
         }

         if (FooterText != null) {
             FooterText.Dispose ();
             FooterText = null;
         }

         if (ButtonDetails != null) {
             ButtonDetails.Dispose ();
             ButtonDetails = null;
         }

         if (ButtonCancel != null) {
             ButtonCancel.Dispose ();
             ButtonCancel = null;
         }

         if (ButtonSend != null) {
             ButtonSend.Dispose ();
             ButtonSend = null;
         }

         if (Details != null) {
             Details.Dispose ();
             Details = null;
         }

         if (ScrollView != null) {
             ScrollView.Dispose ();
             ScrollView = null;
         }
     }
 }

 [Register ("CrashLogDumperWindow")]
 partial class CrashLogDumperWindow
 {
     
     void ReleaseDesignerOutlets ()
     {
     }
 }
}
