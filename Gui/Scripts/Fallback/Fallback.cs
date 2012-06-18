using System.Diagnostics;
using System.Windows.Forms;
using System;
using Linn;

namespace Linn {
namespace Gui {

public class Progam
{
    [STAThread]
    public static void Main(string[] aArgs) {
        Helper helper = new Helper(aArgs);
        helper.ProcessCommandLine();      
          
        Application.Run(new Fallback());
        
        helper.Dispose();
    }
}

public class Fallback : Form
{
    public Fallback() {
        InitializeComponent();
    }
    
    protected override void Dispose(bool aDisposing)
    {
        if(aDisposing) {
            if(iComponents != null) {
                iComponents.Dispose();
            }
        }
        base.Dispose(aDisposing);
    }

    private void InitializeComponent() {
        iComponents = new System.ComponentModel.Container();
        iNotifyIcon = new System.Windows.Forms.NotifyIcon(iComponents);
        iContextMenu = new System.Windows.Forms.ContextMenu();
        iMenuItemStart = new System.Windows.Forms.MenuItem();
        iMenuItemStop = new System.Windows.Forms.MenuItem();
        iMenuItemExit = new System.Windows.Forms.MenuItem();

        SuspendLayout();
        iNotifyIcon.ContextMenu = iContextMenu;
        iNotifyIcon.Icon = new System.Drawing.Icon("../share/Linn/Gui/Linn.ico");
        iNotifyIcon.Text = "Fallback";
        iNotifyIcon.Visible = true;

        iContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] { iMenuItemStart,
                                                                              iMenuItemStop,
                                                                              iMenuItemExit });
        iMenuItemStart.Index = 0;
        iMenuItemStart.Text = "Start";
        iMenuItemStart.Click += new System.EventHandler(menuItemStart_Click);
        iMenuItemStop.Index = 1;
        iMenuItemStop.Text = "Stop";
        iMenuItemStop.Click += new System.EventHandler(menuItemStop_Click);
        iMenuItemExit.Index = 2;
        iMenuItemExit.Text = "Exit";
        iMenuItemExit.Click += new System.EventHandler(menuItemExit_Click);

        Name = "Fallback";
        Text = "Fallback";
        Icon = new System.Drawing.Icon("../share/Linn/Gui/Linn.ico");
        WindowState = System.Windows.Forms.FormWindowState.Minimized;
        ShowInTaskbar = false;
        //Closing += new System.ComponentModel.CancelEventHandler(Fallback_Closing);
        ResumeLayout(false);
    }
    
    /*private void Fallback_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
        e.Cancel();
    }*/
    
    private string Filename() {
        string filename = "LinnGui_xxx_rx.exe";
#if TRACE
        filename = "LinnGui_xxx_tx.exe";
#endif
#if DEBUG
        filename = "LinnGui_xxx_dx.exe";
#endif
        return filename;
    }
    
    private void menuItemStart_Click(object sender, System.EventArgs e) {
        if(iProcess == null || iProcess.HasExited) {
            try {
                iProcess = Process.Start(Filename());
            } catch(System.ComponentModel.Win32Exception) {
            }
        }
    }
    
    private void menuItemStop_Click(object sender, System.EventArgs e) {
        CloseProcess();
    }
    
    private void menuItemExit_Click(object sender, System.EventArgs e) {
        iNotifyIcon.Visible = false;
        CloseProcess();
        Application.Exit();
    }
    
    private void CloseProcess() {
        if(iProcess != null) {
            if(!iProcess.HasExited) {
                iProcess.CloseMainWindow();
                iProcess.Close();
            }
            iProcess = null;
        }
    }
    
    private System.Diagnostics.Process iProcess = null;
    private System.ComponentModel.IContainer iComponents = null;
    private System.Windows.Forms.NotifyIcon iNotifyIcon = null;
    private System.Windows.Forms.ContextMenu iContextMenu = null;
    private System.Windows.Forms.MenuItem iMenuItemStart = null;
    private System.Windows.Forms.MenuItem iMenuItemStop = null;
    private System.Windows.Forms.MenuItem iMenuItemExit = null;
}
    
} // Gui
} // Linn
