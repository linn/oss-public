using Linn.Gui;
using Linn.Gui.Resources;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Linn;
using System;
using System.Threading;
using System.IO;
using System.ComponentModel;
using System.Net.Sockets;
using System.Net;
using Linn.Topology;
using Linn.Gui.Scenegraph;


namespace Linn {
namespace Kinsky {

public partial class ApplicationCanvas : Form
{
    public ApplicationCanvas(string aWindowTitle, string aSoftwareVersion, string aPackageCache, string aTextureCache, IPAddress aAddress, bool aDeveloperMode, bool aMouse, IPAddress aInterface) {
        iCanvas = new CanvasGdiPlus();
        new RendererGdiPlus(iCanvas);
        Trace.WriteLine(Trace.kKinskyTouch, "ApplicationCanvas: Exe dir: " + Application.StartupPath);
        TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Default"));
        TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Skins/Klimax/800x480"));
        PackageManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Skins/Klimax/800x480/Laptop"));
        Trace.WriteLine(Trace.kKinskyTouch, "ApplicationCanvas: Default TextureCache dir: " + TextureManager.Instance.RootDirectory);
        TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, aTextureCache));
        Trace.WriteLine(Trace.kKinskyTouch, "ApplicationCanvas: TextureCache dir: " + TextureManager.Instance.RootDirectory);
        PackageManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, aPackageCache));
        Trace.WriteLine(Trace.kKinskyTouch, "ApplicationCanvas: PackageCache dir: " + PackageManager.Instance.RootDirectory);

        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();
        
        //
        // TODO: Add any constructor code after InitializeComponent call
        //
        iView = new ViewApplicationCanvas(this);
        iCanvas.Load("Main.xml");
        Package layout = iCanvas.CurrLayout;
        Messenger.Instance.Root = layout.Root;
        iDeveloperMode = aDeveloperMode;

        Node minimiseButton = layout.Root.Search("Main.MinimiseButton");
        Node exitButton = layout.Root.Search("Main.ExitButton");
        Node versionText = layout.Root.Search("Main.VersionText");
        Node wibbleSpeed = layout.Root.Search("Main.WibbleSpeed");

        Rectangle rect = Screen.PrimaryScreen.Bounds;
        if(rect.Width == iCanvas.ClientSize.Width && rect.Height == iCanvas.ClientSize.Height) {
            WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.None;
            if(!aMouse) {
                Cursor.Hide();
            }
            // NOTE: This is required due to problem with positioning the window due to
            //       NodeText using the drawing facilities before the window is openend fully
            Location = new System.Drawing.Point(0, 0);
        } else {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            ControlBox = true;
            Messenger.Instance.PresentationMessage(new MsgSetActive(minimiseButton, false));
            Messenger.Instance.PresentationMessage(new MsgSetActive(exitButton, false));
        }
        ClientSize = new System.Drawing.Size(iCanvas.ClientSize.Width, iCanvas.ClientSize.Height);
        Text = aWindowTitle + " v" + aSoftwareVersion + " (" + kCompatabilityFamily + ")";
        Controls.Add(iCanvas);
        SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        UpdateStyles();

        if(aDeveloperMode) {
            Messenger.Instance.PresentationMessage(new MsgSetText(versionText, aSoftwareVersion));
            Messenger.Instance.PresentationMessage(new MsgSetActive(versionText, true));
            Messenger.Instance.PresentationMessage(new MsgSetActive(wibbleSpeed, true));
        }

        iControllerSystem = new ControllerSystem(iCanvas.CurrLayout.Root, aInterface);
        
        /*
        iMutex = new Mutex(false);
        iMutexStack = new Mutex(false);
        iEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        iWorkerCancelled = new EventWaitHandle(false, EventResetMode.ManualReset);
        iMsgQueue = new Queue<bool>();
        iNetworkWorker = new BackgroundWorker();
        iNetworkWorker.WorkerSupportsCancellation = true;
        iNetworkWorker.DoWork += DoNetworkWork;
        iNetworkWorker.RunWorkerCompleted += RunWorkerCompleted;
        iNetworkWorker.RunWorkerAsync();
        */

        PowerManagement.PowerModeChanged += new PowerManagement.PowerModeChangedEventHandler(PowerModeChanged);

        System.Net.NetworkInformation.NetworkChange.NetworkAvailabilityChanged +=
            new System.Net.NetworkInformation.NetworkAvailabilityChangedEventHandler(NetworkAvailabilityChanged);
    }
    
    private void ApplicationCanvas_Load(object sender, System.EventArgs e) {
        Trace.WriteLine(Trace.kKinskyTouch, ">ApplicationCanvas_Load");
        //Start();
        iControllerSystem.Start();
        iFadeInTimer.Start();
    }

    private void ApplicationCanvas_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
        if(Opacity > 0) {
            iFadeOutTimer.Start();
            e.Cancel = true;
            //Hide();
            //Stop();
            /*
            if(iNetworkWorker != null) {
                iNetworkWorker.CancelAsync();
                iNetworkWorker = null;
            }
            iWorkerCancelled.WaitOne();       // wait till message has been processed
            if(iView != null) {
                iView.Dispose();
                iView = null;
            }
            */
        }
        Trace.WriteLine(Trace.kKinskyTouch, ">ApplicationCanvas_Closing");
    }
    
    private void ApplicationCanvas_Deactivate(object sender, EventArgs e) {
        Trace.WriteLine(Trace.kKinskyTouch, ">ApplicationCanvas_Deactivate");
        iCanvas.Deactivate();
    }
    
    private void PowerModeChanged(System.Object sender, PowerManagement.PowerModeChangedEventArgs e) {
        Trace.WriteLine(Trace.kKinskyTouch, ">ApplicationCanvas.PowerModeChanged: e.Mode=" + e.Mode.ToString());
        if (e.Mode == PowerManagement.PowerModes.Suspend) {
            iControllerSystem.Stop();
        }
    }
    
    private void NetworkAvailabilityChanged(object sender, System.Net.NetworkInformation.NetworkAvailabilityEventArgs e) {
        Trace.WriteLine(Trace.kKinskyTouch, "ApplicationCanvas.NetworkAvailabilityChanged: IsAvailable=" + e.IsAvailable);
        if(e.IsAvailable) {
            iControllerSystem.Start();
        } else {
            iControllerSystem.Stop();
        }
    }
    
    private const string kCompatabilityFamily = "Bute";

    private bool iDeveloperMode = false;

    private CanvasGdiPlus iCanvas;
    private ControllerSystem iControllerSystem;
    private ViewApplicationCanvas iView;

    private void iFadeInTimer_Tick(object sender, EventArgs e) {
        if(Opacity + 0.1 >= 1) {
            Opacity = 1.0;
            iFadeInTimer.Stop();
        } else {
            Opacity += 0.2;
        }
    }

    private void iFadeOutTimer_Tick(object sender, EventArgs e) {
        Opacity -= 0.2;
        if(Opacity <= 0) {
            iFadeOutTimer.Stop();
            Close();
        }
    }
}

} // Kinsky
} // Linn

