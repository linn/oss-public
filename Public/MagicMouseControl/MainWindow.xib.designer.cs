// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.1433
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace MagicMouseControl {
	
	
	// Should subclass MonoMac.AppKit.NSView
	[MonoMac.Foundation.Register("MainView")]
	public partial class MainView {
	}
	
	// Should subclass NSObject
	[MonoMac.Foundation.Register("MainWindow")]
	public partial class MainWindow {
		
		private MainView __mt_View;
		
		private global::MonoMac.AppKit.NSWindow __mt_Window;
		
		#pragma warning disable 0169
		[MonoMac.Foundation.Connect("View")]
		private MainView View {
			get {
				this.__mt_View = ((MainView)(this.GetNativeField("View")));
				return this.__mt_View;
			}
			set {
				this.__mt_View = value;
				this.SetNativeField("View", value);
			}
		}
		
		[MonoMac.Foundation.Connect("Window")]
		private global::MonoMac.AppKit.NSWindow Window {
			get {
				this.__mt_Window = ((global::MonoMac.AppKit.NSWindow)(this.GetNativeField("Window")));
				return this.__mt_Window;
			}
			set {
				this.__mt_Window = value;
				this.SetNativeField("Window", value);
			}
		}
	}
}
