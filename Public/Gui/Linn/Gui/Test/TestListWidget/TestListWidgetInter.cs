using Linn.Gui;
using System.Windows.Forms;
using System.Drawing;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn;
using System;

namespace Linn {
namespace Gui {
    
public class Progam
{
    [STAThread]
    public static void Main(string[] aArgs) {
        App app = new App(aArgs);
        app.Start();
        
        Application.Run(new TestListWidget());
    }
}
    
public sealed class TestListWidget : Form, IMessengerObserver
{
    public TestListWidget() {
        new RendererGdi(this);
        Messenger.Instance.AddObserver(this);
        
        TextureManager.Instance.AddPath(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Gui/Editor"));
        TextureManager.Instance.AddPath(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Gui/Editor/Default"));
        TextureManager.Instance.AddPath(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Gui/TestListWidget"));
        PackageManager.Instance.AddPath(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Gui/TestListWidget"));
        iCanvas = new Canvas("TestListWidget.xml");
        Package layout = iCanvas.CurrLayout;
        Messenger.Instance.Root = layout.Root;
        
        InitializeComponent();
    }
    
    public void Receive(Resources.Message aMessage) {
        if(aMessage.Fullname == "TestListWidget.ExitButton.Monostable") {
            System.Environment.Exit(0);
        }
        MsgStateChanged msg = aMessage as MsgStateChanged;
        if(msg != null) {
            if(msg.NewState == false) {
                if(aMessage.Fullname == "TestListWidget.AddItemButton.Monostable") {
                    VisitorSearch search = new VisitorSearch("TestListWidget.List");
                    NodeList node = (NodeList)search.Search(iCanvas.CurrLayout.Root);
                    node.Add("Item " + iCount.ToString());
                    iCount++;
                }
                if(aMessage.Fullname == "TestListWidget.RemoveItemButton.Monostable") {
                    if(iCount > 0) {
                        VisitorSearch search = new VisitorSearch("TestListWidget.List");
                        NodeList node = (NodeList)search.Search(iCanvas.CurrLayout.Root);
                        iCount--;
                        node.Remove("Item " + iCount.ToString());
                    }
                }
            }
        }
    }
    
    private void InitializeComponent() {
        SuspendLayout();
        //FormBorderStyle = FormBorderStyle.None;
        //Cursor.Hide();
        // NOTE: This is required due to problem with positioning the window due to
        //       NodeText using the drawing facilities before the window is openend fully
        Location = new System.Drawing.Point(0, 0);
        MaximizeBox = false;
        ClientSize = new System.Drawing.Size(iCanvas.ClientSize.Width, iCanvas.ClientSize.Height);
        MaximumSize = new System.Drawing.Size(Width, Height);
        MinimumSize = new System.Drawing.Size(Width, Height);
        SetStyle(ControlStyles.DoubleBuffer, true);
        SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        SetStyle(ControlStyles.UserPaint, true);
        Controls.Add(iCanvas);
        Name = "TestListWidget";
        Text = "TestListWidget";
        Icon = new System.Drawing.Icon(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Gui/Linn.ico"));
        BackColor = Color.White;
        ResumeLayout(false);
    }
    
    private Canvas iCanvas;
    private int iCount = 0;
}
    
} // Gui
} // Linn
