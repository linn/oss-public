using Linn.TestFramework;
using Linn.Gui;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System.Collections;

namespace Linn {
namespace Gui {
    
internal class SuiteEventTests : Suite, IMessengerObserver
{
    public SuiteEventTests() : base("Send event tests") {
    }
    
    public override void Test() {
        Package package = new Package();
        package.Namespace = "Test";
        PackageManager.Instance.Packages.Add(package);
        
        NodeHit root = new NodeHit();
        root.Namespace = "Test";
        root.Name = "TestNodeHit";
        Messenger.Instance.EEventAppMessage += Receive;
        Messenger.Instance.Root = root;
        package.AddNode(root);
        
        Node node = new Node();
        node.Namespace = "Test";
        node.Name = "TestNode";
        root.AddChild(node);
        package.AddNode(node);
        
        // test active message
        TEST(node.Active == true);
        MsgSetActive setActive = new MsgSetActive(node, false);
        Messenger.Instance.PresentationMessage(setActive);
        TEST(node.Active == false);
        iMsgQueue.Clear();
        
        setActive = new MsgSetActive(node, true);
        Messenger.Instance.PresentationMessage(setActive);
        TEST(node.Active == true);
        iMsgQueue.Clear();
        
        root.RemoveChild(node);
        
        NodeText textNode = new NodeText();
        textNode.Namespace = "Test";
        textNode.Name = "TestNodeText";
        textNode.Text = "No";
        root.AddChild(textNode);
        package.AddNode(textNode);
        
        // test set text message
        TEST(textNode.Text == "No");
        MsgSetText setText = new MsgSetText(textNode, "New");
        Messenger.Instance.PresentationMessage(setText);
        TEST(textNode.Text == "New");
        
        root.RemoveChild(textNode);

        Bistable bi = new Bistable();
        bi.Namespace = "Test";
        bi.Name = "TestBi";
        
        // test set state message
        node.NextPlugin = bi;
        bi.NextPlugin = node;
        root.AddChild(node);
        package.AddPlugin(bi);
        
        TEST(bi.State == false);
        MsgSetState setState = new MsgSetState(bi, true);
        Messenger.Instance.PresentationMessage(setState);
        TEST(bi.State == true);
        TEST(iMsgQueue.Count == 1);
        TEST(iMsgQueue[0] as MsgStateChanged != null);
        TEST(((MsgStateChanged)iMsgQueue[0]).OldState == false && ((MsgStateChanged)iMsgQueue[0]).NewState == true);
        iMsgQueue.Clear();
        setState = new MsgSetState(bi, false);
        Messenger.Instance.PresentationMessage(setState);
        TEST(bi.State == false);
        TEST(iMsgQueue.Count == 1);
        TEST(iMsgQueue[0] as MsgStateChanged != null);
        TEST(((MsgStateChanged)iMsgQueue[0]).OldState == true && ((MsgStateChanged)iMsgQueue[0]).NewState == false);
        iMsgQueue.Clear();
        
        // test toggle state message
        MsgToggleState toggleState = new MsgToggleState(bi);
        Messenger.Instance.PresentationMessage(toggleState);
        TEST(bi.State == true);
        TEST(iMsgQueue.Count == 1);
        TEST(iMsgQueue[0] as MsgStateChanged != null);
        TEST(((MsgStateChanged)iMsgQueue[0]).OldState == false && ((MsgStateChanged)iMsgQueue[0]).NewState == true);
        iMsgQueue.Clear();
        toggleState = new MsgToggleState(bi);
        Messenger.Instance.PresentationMessage(toggleState);
        TEST(bi.State == false);
        TEST(iMsgQueue.Count == 1);
        TEST(iMsgQueue[0] as MsgStateChanged != null);
        TEST(((MsgStateChanged)iMsgQueue[0]).OldState == true && ((MsgStateChanged)iMsgQueue[0]).NewState == false);
        iMsgQueue.Clear();
        
        node.NextPlugin = null;
        bi.NextPlugin = null;
        
        Monostable mono = new Monostable();
        mono.Namespace = "Test";
        mono.Name = "TestMono";
        
        node.NextPlugin = mono;
        mono.NextPlugin = node;
        package.AddPlugin(mono);
        
        mono.Hit(new Vector3d());
        TEST(iMsgQueue.Count == 1);
        TEST(iMsgQueue[0] as MsgStateChanged != null);
        TEST(((MsgStateChanged)iMsgQueue[0]).OldState == false && ((MsgStateChanged)iMsgQueue[0]).NewState == true);
        iMsgQueue.Remove(iMsgQueue[0]);

        mono.UnHit();
        TEST(iMsgQueue.Count == 1);
        TEST(iMsgQueue[0] as MsgStateChanged != null);
        TEST(((MsgStateChanged)iMsgQueue[0]).OldState == true && ((MsgStateChanged)iMsgQueue[0]).NewState == false);
        iMsgQueue.Clear();
        
        Messenger.Instance.EEventAppMessage -= Receive;
    }
    
    public void Receive(Message aMessage) {
        iMsgQueue.Add(aMessage);
    }
    
    private ArrayList iMsgQueue = new ArrayList();
}



class Program {
    public static void Main() {
        new RendererNull();
        Runner runner = new Runner("Gui Event tests");
        runner.Add(new SuiteEventTests());
        runner.Run();
    }
}

} // Gui
} // Linn
