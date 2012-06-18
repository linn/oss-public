using Linn.Gui;
using System.Windows.Forms;
using System.Drawing;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn;
using System;
using System.Xml;
using System.Xml.XPath;

namespace Linn {
namespace Gui {
    
public class Progam
{
    [STAThread]
    public static void Main(string[] aArgs) {
        App app = new App(aArgs);
        app.Start();
        
        Application.Run(new TestWibbler2());
    }
}
    
public sealed class TestWibbler2 : Form, IMessengerObserver
{
    public TestWibbler2() {
        new RendererGdi(this);
        Messenger.Instance.AddObserver(this);
        
        TextureManager.Instance.AddPath(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Gui/Editor"));
        TextureManager.Instance.AddPath(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Gui/Editor/Default"));
        TextureManager.Instance.AddPath(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Gui/Test/Wibbler"));
        PackageManager.Instance.AddPath(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Gui/Test/Wibbler"));
        TextureManager.Instance.AddPath("../../share/Linn/Gui/Editor");
        TextureManager.Instance.AddPath("../../share/Linn/Gui/Editor/Default");
        TextureManager.Instance.AddPath("../../share/Linn/Gui/Test/Wibbler");
        PackageManager.Instance.AddPath("../../share/Linn/Gui/Test/Wibbler");
        iCanvas = new Canvas("TestWibbler2.xml");
        Package layout = iCanvas.CurrLayout;
        Messenger.Instance.Root = layout.Root;
        
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(Linn.TestFramework.Path.Share() + "/Linn/Gui/Test/Wibbler/ExampleList.xml");
        XmlNamespaceManager xmlNsMan = new XmlNamespaceManager(xmlDoc.NameTable);
        xmlNsMan.AddNamespace("ns", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");
        xmlNsMan.AddNamespace("upnp", "urn:schemas-upnp-org:metadata-1-0/upnp/");
        xmlNsMan.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
        
        XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/ns:DIDL-Lite/ns:item/dc:title", xmlNsMan);

        VisitorSearch search1 = new VisitorSearch("TestWibbler2.NodeList1");
        VisitorSearch search2 = new VisitorSearch("TestWibbler2.NodeList2");
        NodeList node1 = (NodeList)search1.Search(layout.Root);
        NodeList node2 = (NodeList)search2.Search(layout.Root);
        foreach(XmlNode xmlNode in nodeList) {
            node1.Add(xmlNode.FirstChild.Value);
            node2.Add(xmlNode.FirstChild.Value);
        }
        System.Console.WriteLine("count = " + nodeList.Count);
        
        InitializeComponent();
    }
    
    public void Receive(Resources.Message aMessage) {
        if(aMessage.Fullname == "TestWibbler2.ExitButton.Monostable") {
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
        Name = "TestWibbler2";
        Text = "TestWibbler2";
        Icon = new Icon(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Gui/Linn.ico"));
        BackColor = Color.White;
        ResumeLayout(false);
    }
    
    private Canvas iCanvas;
}
    
} // Gui
} // Linn
