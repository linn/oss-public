using System.Windows.Forms;
using System.Drawing;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using Linn.TestFramework;

namespace Linn {
namespace Gui {
    
public sealed class TestWidgetsCanvas : Form, IMessengerObserver
{
    public TestWidgetsCanvas() {
        iCanvas = new CanvasGdiPlus();
        new RendererGdiPlus(iCanvas);
        Messenger.Instance.EEventAppMessage += Receive;
        
        TextureManager.Instance.AddPath(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Gui/Editor"));
        PackageManager.Instance.AddPath(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Test/Gui"));
        
        iCanvas.Load("TestWidgets.xml");
        Messenger.Instance.Root = iCanvas.CurrLayout.Root;
        InitializeComponent();
    }
    
    public void Receive(Resources.Message aMessage) {
        System.Console.WriteLine("Recevied: " + aMessage.GetType() + " - " + aMessage.Fullname);
    }
    
    private void InitializeComponent() {
        SuspendLayout(); 
        ClientSize = new System.Drawing.Size(iCanvas.ClientSize.Width, iCanvas.ClientSize.Height);
        MaximumSize = new System.Drawing.Size(Width, Height);
        MinimumSize = new System.Drawing.Size(Width, Height);
        Controls.Add(iCanvas);
        Name = "TestWidgets";
        Text = "TestWidgets";
        Icon = new Icon(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Gui/Linn.ico"));
        BackColor = Color.White;
        ResumeLayout(false);
    }
    
    private CanvasGdiPlus iCanvas;
}

public sealed class Program {
    public static void Main() {
        Application.Run(new TestWidgetsCanvas());
    }
}
    
} // Gui
} // Linn
