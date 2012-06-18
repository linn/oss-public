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
        
        Application.Run(new TestWibbler1());
    }
}
    
public sealed class TestWibbler1 : Form, IMessengerObserver
{
    public TestWibbler1() {
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
        iCanvas = new Canvas("TestWibbler1.xml");
        Package layout = iCanvas.CurrLayout;
        Messenger.Instance.Root = layout.Root;
        
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(Linn.TestFramework.Path.Share() + "/Linn/Gui/Test/Wibbler/ExampleList.xml");
        XmlNamespaceManager xmlNsMan = new XmlNamespaceManager(xmlDoc.NameTable);
        xmlNsMan.AddNamespace("ns", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");
        xmlNsMan.AddNamespace("upnp", "urn:schemas-upnp-org:metadata-1-0/upnp/");
        xmlNsMan.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
        
        XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/ns:DIDL-Lite/ns:item/dc:title", xmlNsMan);

        VisitorSearch search1 = new VisitorSearch("TestWibbler1.NodeList1");
        VisitorSearch search2 = new VisitorSearch("TestWibbler1.NodeList2");
        VisitorSearch input = new VisitorSearch("TestWibbler1.NodeInput");
        iList1 = (NodeList)search1.Search(layout.Root);
        iList2 = (NodeList)search2.Search(layout.Root);
        iInput = (NodeInput)input.Search(layout.Root);
        foreach(XmlNode xmlNode in nodeList) {
            iList1.Add(xmlNode.FirstChild.Value);
            iList2.Add(xmlNode.FirstChild.Value);
        }
        System.Console.WriteLine("count = " + nodeList.Count);
        
        InitializeComponent();
    }
    
    public void Receive(Resources.Message aMessage) {
        if(aMessage.Fullname == "TestWibbler1.ExitButton.Monostable") {
            System.Environment.Exit(0);
        }
        
        if(aMessage.Fullname == "TestWibbler1.List1ToggleButton.Bistable") {
            MsgStateChanged stateMsg = aMessage as Resources.MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true && stateMsg.OldState == false) {
                    iInput.MsgReceiver = iList1;
                } else if(stateMsg.NewState == false && stateMsg.OldState == true && iInput.MsgReceiver == iList1) {
                    iInput.MsgReceiver = null;
                }
            }
        }
        
        if(aMessage.Fullname == "TestWibbler1.List2ToggleButton.Bistable") {
            MsgStateChanged stateMsg = aMessage as Resources.MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true && stateMsg.OldState == false) {
                    iInput.MsgReceiver = iList2;
                } else if(stateMsg.NewState == false && stateMsg.OldState == true && iInput.MsgReceiver == iList2) {
                    iInput.MsgReceiver = null;
                }
            }
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
        Name = "TestWibbler1";
        Text = "TestWibbler1";
        Icon = new Icon(System.IO.Path.Combine(Linn.TestFramework.Path.Share(), "Linn/Gui/Linn.ico"));
        BackColor = Color.White;
        ResumeLayout(false);
    }
    
    private Canvas iCanvas;
    private NodeList iList1;
    private NodeList iList2;
    private NodeInput iInput;
}
    
} // Gui
} // Linn
