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
    public static void Main() {
        App app = new App();
        app.Start();
        
        Application.Run(new WibblerPrototype());
    }
}
    
public sealed class WibblerPrototype : Form, IMessengerObserver
{
    public WibblerPrototype() {
        new RendererGdi(this);
        Messenger.Instance.AddObserver(this);
        
        TextureManager.Instance.AddPath(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Gui/Prototype/Wibbler"));
        PackageManager.Instance.AddPath(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Gui/Prototype/Wibbler"));
        iCanvas = new Canvas("WibblerPrototype.xml");
        Package layout = iCanvas.CurrLayout;
        Messenger.Instance.Root = layout.Root;
        
        VisitorSearch search = new VisitorSearch("WibblerPrototype.NodeList");
        NodeList node = (NodeList)search.Search(layout.Root);
        for(int i = 0; i < 1000; ++i) {
            node.Add("Item " + i.ToString());
        }
        
        InitializeComponent();
    }
    
    public void Receive(Resources.Message aMessage) {
        if(aMessage.Fullname == "WibblerPrototype.ExitButton.Monostable") {
            System.Environment.Exit(0);
        }
    }
    
    private void InitializeComponent() {
        SuspendLayout();
        FormBorderStyle = FormBorderStyle.None;
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
        Name = "WibblerPrototype";
        Text = "WibblerPrototype";
        Icon = new Icon(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Gui/Linn.ico"));
        BackColor = Color.White;
        ResumeLayout(false);
    }
    
    private Canvas iCanvas;
}
    
} // Gui
} // Linn
