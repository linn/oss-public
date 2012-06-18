using Linn.Gui;
using Linn.Gui.Resources;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Linn.Control.Upnp.ControlPoint;
using Linn;
using System;
using System.Threading;
using System.IO;
using System.ComponentModel;
using System.Net.Sockets;
using Linn.Topology;
using System.Reflection;
using System.Net;

namespace Linn {
namespace KinskyPda {

public partial class ApplicationCanvas : Form
{
    public ApplicationCanvas(string aWindowTitle, string aSoftwareVersion, string aPackageCache, string aTextureCache, IPAddress aAddress, bool aDeveloperMode) {
        iCanvas = new CanvasGdi();
        new RendererGdi(iCanvas);
        string dir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
        Trace.WriteLine(Trace.kLinnGui, "ApplicationCanvas: Exe dir: " + dir);
        TextureManager.Instance.AddPath(dir);
        TextureManager.Instance.AddPath(System.IO.Path.Combine(dir, "../../share/Linn/Gui/Editor"));
        TextureManager.Instance.AddPath(System.IO.Path.Combine(dir, "../../share/Linn/Gui/Editor/Default"));
        TextureManager.Instance.AddPath(System.IO.Path.Combine(dir, "../../share/Linn/Gui/Skins/Sneaky/640x480"));
        PackageManager.Instance.AddPath(System.IO.Path.Combine(dir, "../../share/Linn/Gui/Skins/Sneaky/640x480"));
        Trace.WriteLine(Trace.kLinnGui, "ApplicationCanvas: Default TextureCache dir: " + TextureManager.Instance.RootDirectory);
        TextureManager.Instance.AddPath(System.IO.Path.Combine(dir, aTextureCache));
        Trace.WriteLine(Trace.kLinnGui, "ApplicationCanvas: TextureCache dir: " + TextureManager.Instance.RootDirectory);
        PackageManager.Instance.AddPath(System.IO.Path.Combine(dir, aPackageCache));
        Trace.WriteLine(Trace.kLinnGui, "ApplicationCanvas: PackageCache dir: " + PackageManager.Instance.RootDirectory);

        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();
        
        //
        // TODO: Add any constructor code after InitializeComponent call
        //
        iCanvas.Load("Main.xml");
        Package layout = iCanvas.CurrLayout;
        Messenger.Instance.Root = layout.Root;
        iDeveloperMode = aDeveloperMode;
        
        Rectangle rect = Screen.PrimaryScreen.Bounds;
        if(rect.Width == iCanvas.ClientSize.Width && rect.Height == iCanvas.ClientSize.Height) {
            WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Cursor.Hide();
            // NOTE: This is required due to problem with positioning the window due to
            //       NodeText using the drawing facilities before the window is openend fully
            Location = new System.Drawing.Point(0, 0);
        } else {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            ControlBox = true;
        }
        ClientSize = new System.Drawing.Size(iCanvas.ClientSize.Width, iCanvas.ClientSize.Height);
        Text = aWindowTitle + " v" + aSoftwareVersion + " (" + kCompatabilityFamily + ")";
        Controls.Add(iCanvas);
        
        Location = new System.Drawing.Point(Screen.PrimaryScreen.Bounds.Width - iCanvas.ClientSize.Width, Screen.PrimaryScreen.Bounds.Height);

        if(aDeveloperMode) {
            Messenger.Instance.PresentationMessage(new MsgSetText("Main.VersionText", aSoftwareVersion));
            Messenger.Instance.PresentationMessage(new MsgSetActive("Main.VersionText", true));
            Messenger.Instance.PresentationMessage(new MsgSetActive("Main.WibbleSpeed", true));
        }

        iProductCp = new ControlPoint("urn:linn-co-uk:service:Product", 2);
        iProductCp.Interface = aAddress;
        iMediaServerCp = new ControlPoint("urn:schemas-upnp-org:device:MediaServer", 1);
        iMediaServerCp.Interface = aAddress;
        iMediaRendererCp = new ControlPoint("urn:schemas-upnp-org:device:MediaRenderer", 1);
        iMediaRendererCp.Interface = aAddress;
    }
    
    private void ApplicationCanvas_Load(object sender, System.EventArgs e) {
        Trace.WriteLine(Trace.kLinnGui, ">ApplicationCanvas_Load");
        StartStack();
    }

    private void ApplicationCanvas_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
        Trace.WriteLine(Trace.kLinnGui, ">ApplicationCanvas_Closing");
        Hide();
        StopStack();
        RendererGdi renderer = (RendererGdi)Renderer.Instance;
        Trace.WriteLine(Trace.kRendering, "Min = " + renderer.Stats.MinimumMs + "ms");
        Trace.WriteLine(Trace.kRendering, "Avg = " + renderer.Stats.AverageMs + "ms");
        Trace.WriteLine(Trace.kRendering, "Max = " + renderer.Stats.MaximumMs + "ms");
        Trace.WriteLine(Trace.kRendering, "Total = " + renderer.Stats.TotalMs + ", Frames = " + renderer.Stats.Frames);
    }
    
    private void ApplicationCanvas_Deactivate(object sender, EventArgs e) {
        Trace.WriteLine(Trace.kLinnGui, ">ApplicationCanvas_Deactivate");
        iCanvas.Deactivate();
    }

    private void StartStack() {
        Trace.WriteLine(Trace.kLinnGui, ">ApplicationCanvas.StartStack");
        if(!iStarted) {
            Trace.WriteLine(Trace.kLinnGui, "ApplicationCanvas.StartStack: Starting control points...");
            iModelSystem = new ModelSystem(new ModelLibrary(new ConverterLibraryDidlLite(), iMediaServerCp), iProductCp, iMediaRendererCp, iDeveloperMode);
            iControllerRoomSource = new ControllerRoomSource(iCanvas.CurrLayout.Root, iModelSystem);
            
            iProductCp.AddObserver(iModelSystem.ProductAlive, iModelSystem.ProductByeBye);
            iMediaRendererCp.AddObserver(iModelSystem.MediaRendererAlive, iModelSystem.MediaRendererByeBye);
            iMediaServerCp.AddObserver(iModelSystem.MediaServerAlive, iModelSystem.MediaServerByeBye);
            iMediaServerCp.Start();
            iMediaRendererCp.Start();
            iProductCp.Start();
            
            // send out multiple discover messages to speed up device discovery
            iProductCp.Discover(false);
            iMediaServerCp.Discover(false);
            iMediaRendererCp.Discover(false);
            Trace.WriteLine(Trace.kLinnGui, "ApplicationCanvas.StartStack: Discovery Done");
            iStarted = true;
        }
        Trace.WriteLine(Trace.kLinnGui, "<ApplicationCanvas.StartStack");
    }
    
    private void StopStack() {
        Trace.WriteLine(Trace.kLinnGui, ">ApplicationCanvas.StopStack");
        if(iStarted) {
            Trace.WriteLine(Trace.kLinnGui, "ApplicationCanvas.StopStack: Obtained event");
            iProductCp.RemoveObserver(iModelSystem.ProductAlive, iModelSystem.ProductByeBye);
            iMediaRendererCp.RemoveObserver(iModelSystem.MediaRendererAlive, iModelSystem.MediaRendererByeBye);
            iMediaServerCp.RemoveObserver(iModelSystem.MediaServerAlive, iModelSystem.MediaServerByeBye);
            if(iModelSystem != null) {
                iModelSystem.Dispose();
            }
            if(iControllerRoomSource != null) {
                iControllerRoomSource.Dispose();
            }
            iProductCp.Stop();
            iMediaServerCp.Stop();
            iMediaRendererCp.Stop();
            iStarted = false;
        }
        Trace.WriteLine(Trace.kLinnGui, "<ApplicationCanvas.StopStack");
    }
    
    private const string kCompatabilityFamily = "Bute";
    private bool iDeveloperMode = false;
    private CanvasGdi iCanvas = null;
    private ModelSystem iModelSystem = null;
    private ControlPoint iProductCp = null;
    private ControlPoint iMediaServerCp = null;
    private ControlPoint iMediaRendererCp = null;
    private bool iStarted = false;
    private ControllerRoomSource iControllerRoomSource = null;
}

} // KinskyPda
} // Linn
