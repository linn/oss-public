using Linn.TestFramework;
using Linn.Gui;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System.Drawing;

namespace Linn {
namespace Gui {
    
internal class SuiteLayoutNodeLoadTests : Suite
{
    public SuiteLayoutNodeLoadTests() : base("Layout node load tests") {
    }
    
    public override void Test() {
        PackageManager.Instance.FlushCache();
        // load a single node
        Package package = PackageManager.Instance.Load("Test1.xml");
        NodeHit nodeA = package.Root;
        TEST(nodeA.Name == "NodeA");
        TEST(nodeA.Namespace == "TestNs");
        TEST(nodeA.Parent == null);
        TEST(nodeA.Active == true);
        TEST(nodeA.Children.Count == 0);
        TEST(nodeA.LocalSrt.Translation == new Vector3d(40, 50, 60));
        TEST(nodeA.WorldSrt.Translation == new Vector3d(40, 50, 60));
        TEST(nodeA.Width == 480);
        TEST(nodeA.Height == 300);
        
        // load all types of plugins (nodes and controllers)
        package = PackageManager.Instance.Load("Test2.xml");
        Node node = package.Root;
        TEST(node.Parent == null);
        TEST(node.LocalSrt.Translation == new Vector3d(1, 2, 3));
        TEST(node.WorldSrt.Translation == new Vector3d(1, 2, 3));
        TEST(node.Children.Count == 5);
        TEST(node.Child(0) as Node != null);
        TEST(node.Child(0).Parent == node);
        TEST(node.Child(0).LocalSrt.Translation == new Vector3d(1, 2, 3));
        TEST(node.Child(0).WorldSrt.Translation == new Vector3d(2, 4, 6));
        TEST(node.Child(1) as NodeList != null);
        TEST(node.Child(1).Parent == node);
        TEST(node.Child(1).LocalSrt.Translation == new Vector3d(38, 18, 5));
        TEST(node.Child(1).WorldSrt.Translation == new Vector3d(39, 20, 8));
        TEST(node.Child(2) as NodeHit != null);
        TEST(node.Child(2).Parent == node);
        TEST(node.Child(2).LocalSrt.Translation == new Vector3d(4, 5, 6));
        TEST(node.Child(2).WorldSrt.Translation == new Vector3d(5, 7, 9));
        TEST(((NodeHit)node.Child(2)).Width == 20);
        TEST(((NodeHit)node.Child(2)).Height == 30);
        TEST(node.Child(3) as NodePolygon != null);
        TEST(node.Child(3).Parent == node);
        TEST(node.Child(3).LocalSrt.Translation == new Vector3d(7, 8, 9));
        TEST(node.Child(3).WorldSrt.Translation == new Vector3d(8, 10, 12));
        TEST(((NodePolygon)node.Child(3)).Width == 50);
        TEST(((NodePolygon)node.Child(3)).Height == 60);
        TEST(node.Child(4) as NodeText != null);
        TEST(node.Child(4).Parent == node);
        TEST(node.Child(4).LocalSrt.Translation == new Vector3d(7, 8, 9));
        TEST(node.Child(4).WorldSrt.Translation == new Vector3d(8, 10, 12));
        TEST(((NodeText)node.Child(4)).Width == 50);
        TEST(((NodeText)node.Child(4)).Height == 40);
        TEST(((NodeText)node.Child(4)).Text == "Test");
        TEST(((NodeText)node.Child(4)).Colour == new Colour(60, 255, 0, 0));
        TEST(((NodeText)node.Child(4)).FaceName == "Arial");
        TEST(((NodeText)node.Child(4)).PointSize == 10);
        TEST(((NodeText)node.Child(4)).Bold == false);
        TEST(((NodeText)node.Child(4)).Italic == false);
        TEST(((NodeText)node.Child(4)).Underline == false);
        TEST(package.PluginItem(6) as Monostable != null);
        TEST(package.PluginItem(6).Fullname == "TestNs.Monostable");
        TEST(((Monostable)package.PluginItem(6)).Period == 4.0);
        TEST(package.PluginItem(7) as Bistable != null);
        TEST(package.PluginItem(7).Fullname == "TestNs.Bistable");
        TEST(((Bistable)package.PluginItem(7)).State == true);
        TEST(package.PluginItem(8) as Counter != null);
        TEST(package.PluginItem(8).Fullname == "TestNs.Counter");
        TEST(((Counter)package.PluginItem(8)).MaxCount == 4);
        TEST(((Counter)package.PluginItem(8)).CountsPerSecond == 2);
        TEST(((Counter)package.PluginItem(8)).Loop == true);
    }
}

internal class SuiteLayoutTranslatorLoadTests : Suite
{
    public SuiteLayoutTranslatorLoadTests() : base("Layout translator load tests") {
    }
    
    public override void Test() {
        PackageManager.Instance.FlushCache();
        // load message translators
        Package package = PackageManager.Instance.Load("Test3.xml");
        
        Node node = package.Root;
        TEST(node.Children.Count == 1);
        
        Node child = node.Child(0);
        TEST(child.TranslatorOut.Translators.Count == 5);
        
        Translator translator = (Translator)child.TranslatorOut.Translators[0];
        TEST(translator.ToMessage as MsgSetActive != null);
        MsgSetActive activeTo = (MsgSetActive)translator.ToMessage;
        TEST(activeTo.Fullname == "TestNs.NodeB");
        TEST(activeTo.Active == false);
        TEST(translator.FromMessage as MsgSetActive != null);
        MsgSetActive activeFrom = (MsgSetActive)translator.FromMessage;
        TEST(activeFrom.Fullname == "TestNs.NodeA");
        TEST(activeFrom.Active == true);

        translator = (Translator)child.TranslatorOut.Translators[1];
        TEST(translator.ToMessage as MsgSetText != null);
        MsgSetText textTo = (MsgSetText)translator.ToMessage;
        TEST(textTo.Fullname == "TestNs.NodeB");
        TEST(textTo.Text == "Test2");
        TEST(translator.FromMessage as MsgSetText != null);
        MsgSetText textFrom = (MsgSetText)translator.FromMessage;
        TEST(textFrom.Fullname == "TestNs.NodeA");
        TEST(textFrom.Text == "Test1");
        
        translator = (Translator)child.TranslatorOut.Translators[2];
        TEST(translator.ToMessage as MsgStateChanged != null);
        MsgStateChanged changedTo = (MsgStateChanged)translator.ToMessage;
        TEST(changedTo.Fullname == "TestNs.NodeB");
        TEST(changedTo.OldState == false);
        TEST(changedTo.NewState == true);
        TEST(translator.FromMessage as MsgStateChanged != null);
        MsgStateChanged changedFrom = (MsgStateChanged)translator.FromMessage;
        TEST(changedFrom.Fullname == "TestNs.NodeA");
        TEST(changedFrom.OldState == true);
        TEST(changedFrom.NewState == false);
        
        translator = (Translator)child.TranslatorOut.Translators[3];
        TEST(translator.ToMessage as MsgSetState != null);
        MsgSetState stateTo = (MsgSetState)translator.ToMessage;
        TEST(stateTo.Fullname == "TestNs.NodeB");
        TEST(stateTo.State == false);
        TEST(translator.FromMessage as MsgSetState != null);
        MsgSetState stateFrom = (MsgSetState)translator.FromMessage;
        TEST(stateFrom.Fullname == "TestNs.NodeA");
        TEST(stateFrom.State == true);
        
        translator = (Translator)child.TranslatorOut.Translators[4];
        TEST(translator.ToMessage as MsgToggleState != null);
        MsgToggleState toggleTo = (MsgToggleState)translator.ToMessage;
        TEST(toggleTo.Fullname == "TestNs.NodeB");
        TEST(translator.FromMessage as MsgToggleState != null);
        MsgToggleState toggleFrom = (MsgToggleState)translator.FromMessage;
        TEST(toggleFrom.Fullname == "TestNs.NodeA");
    }
}

internal class SuiteLayoutNodeSaveTests : Suite
{
    public SuiteLayoutNodeSaveTests() : base("Layout node save tests") {
    }
    
    public override void Test() {
        string startupPath = System.IO.Path.GetDirectoryName(System.Environment.GetCommandLineArgs()[0]);
        PackageManager.Instance.FlushCache();
        // create a layout
        Package package = new Package();
        PackageManager.Instance.Packages.Add(package);
        package.Namespace = "TestSaveNs";
        
        NodeHit nodeA = new NodeHit();
        nodeA.Name = "NodeA";
        nodeA.Width = 400;
        nodeA.Height = 500;
        Node nodeB = new Node();
        nodeB.Name = "NodeB";
        Node nodeC = new Node();
        nodeC.Name = "NodeC";
        Node nodeD = new Node();
        nodeD.Name = "NodeD";
        Node nodeE = new Node();
        nodeE.Name = "NodeE";
        package.AddNode(nodeA);
        package.AddNode(nodeB);
        package.AddNode(nodeC);
        package.AddNode(nodeD);
        package.AddNode(nodeE);
        // create an structure like so...
        //                    NodeA
        //        NodeB                    NodeC
        //    NodeD    NodeE
        package.Root = nodeA;
        nodeA.AddChild(nodeB);
        nodeA.AddChild(nodeC);
        nodeB.AddChild(nodeD);
        nodeB.AddChild(nodeE);
        
        // save out the scenegraph
        //TEST_THROWS(Linn.Gui.Resources.Layout.NoSaveFilename, layout.Save);
        package.Save(System.IO.Path.Combine(startupPath ,"../../share/Linn/Test/Gui/TestSave.xml"));
        TEST(package.PackageList.Count == 0);
        
        Package newLayout = PackageManager.Instance.Load("TestSave.xml");
        NodeHit root = newLayout.Root;
        TEST(root.Width == 400);
        TEST(root.Height == 500);
        TEST(root.Children.Count == 2);
        TEST(root.Fullname == "TestSaveNs.NodeA");
        TEST(root.Child(0).Fullname == "TestSaveNs.NodeB");
        TEST(root.Child(0).Parent == root);
        TEST(root.Child(0).Children.Count == 2);
        TEST(root.Child(0).Child(0).Fullname == "TestSaveNs.NodeD");
        TEST(root.Child(0).Child(0).Parent == root.Child(0));
        TEST(root.Child(0).Child(1).Fullname == "TestSaveNs.NodeE");
        TEST(root.Child(0).Child(1).Parent == root.Child(0));
        TEST(root.Child(0).Child(1).Children.Count == 0);
        TEST(root.Child(1).Fullname == "TestSaveNs.NodeC");
        TEST(root.Child(1).Parent == root);
        
        package = new Package();
        PackageManager.Instance.Packages.Add(package);
        package.Namespace = "TestSavePlugins";
        package.Root = new NodeHit();
        package.AddNode(package.Root);
        package.Root.Name = "HitPanel";
        package.Root.Width = 500;
        package.Root.Height = 600;
        
        Node node = new Node();
        node.Name = "NodeA";
        
        NodeList list = new NodeList();
        list.Name = "List";
        
        NodePolygon polygonA = new NodePolygon();
        polygonA.Name = "SquareA";
        
        NodePolygon polygonB = new NodePolygon();
        polygonB.Name = "SquareB";
        
        NodePolygon polygonC = new NodePolygon();
        polygonC.Name = "SquareC";
        
        NodeText text = new NodeText();
        text.Name = "SomeText";
        text.Text = "SomeText";
        text.Justification = NodeText.EJustification.EJ_Centre;
        text.Colour = new Colour(0, 255, 128, 30);
        
        //NodeTextGdi textGdi = new NodeTextGdi();
        //textGdi.Name = "SomeMoreText";
        //textGdi.Text = "SomeMoreText";
        
        TextureArray array = new TextureArray();
        TextureArrayFixed arrayFixed = new TextureArrayFixed();
        
        Monostable monostable = new Monostable();
        monostable.Name = "Monostable";
        
        Bistable bistable = new Bistable();
        bistable.Name = "Bistable";
        
        Counter counter = new Counter();
        counter.Name = "Counter";
        
        polygonA.NextPlugin = monostable;
        monostable.NextPlugin = arrayFixed;
        
        polygonB.NextPlugin = bistable;
        bistable.NextPlugin = array;
        
        polygonC.NextPlugin = counter;
        
        package.AddNode(node);
        package.AddNode(list);
        package.AddNode(polygonA);
        package.AddNode(polygonB);
        package.AddNode(polygonC);
        package.AddNode(text);
        
        monostable.Period = 2;
        bistable.State = true;
        counter.MaxCount = 2;
        counter.CountsPerSecond = 1;
        counter.Loop = false;
        //package.AddNode(textGdi);
        
        // create a scenegraph like so...
        //                                        Root
        //              Node                      List              Polygon
        //  Polygon     Text    (TextGdi)
        //  Polygon
        package.Root.AddChild(node);
        package.Root.AddChild(list);
        package.Root.AddChild(polygonC);
        node.AddChild(polygonA);
        node.AddChild(text);
        //node.AddChild(textGdi);
        polygonA.AddChild(polygonB);
        
        package.Save(System.IO.Path.Combine(startupPath, "../../share/Linn/Test/Gui/TestSavePlugins.xml"));
        TEST(package.PackageList.Count == 0);
        
        newLayout = PackageManager.Instance.Load("TestSavePlugins.xml");
        root = newLayout.Root;
        TEST(root.Width == 500);
        TEST(root.Height == 600);
        TEST(root.Children.Count == 3);
        TEST(root.Child(0).Fullname == "TestSavePlugins.NodeA");
        TEST(root.Child(0).NextPlugin == null);
        TEST(root.Child(1).Fullname == "TestSavePlugins.List");
        TEST(root.Child(2).Fullname == "TestSavePlugins.SquareC");
        TEST(root.Child(2).NextPlugin as Counter != null);
        TEST(root.Child(2).NextPlugin.Fullname == "TestSavePlugins.Counter");
        TEST(((Counter)root.Child(2).NextPlugin).MaxCount == 2);
        TEST(((Counter)root.Child(2).NextPlugin).CountsPerSecond == 1);
        TEST(((Counter)root.Child(2).NextPlugin).Loop == false);
        TEST(root.Child(0).Children.Count == 2);
        TEST(root.Child(1).Children.Count == 0);
        TEST(root.Child(0).Child(0) as NodePolygon != null);
        TEST(root.Child(0).Child(0).Fullname == "TestSavePlugins.SquareA");
        TEST(root.Child(0).Child(0).NextPlugin as Monostable != null);
        TEST(root.Child(0).Child(0).NextPlugin.Fullname == "TestSavePlugins.Monostable");
        TEST(((Monostable)root.Child(0).Child(0).NextPlugin).Period == 2);
        TEST(root.Child(0).Child(1) as NodeText != null);
        TEST(root.Child(0).Child(1).Fullname == "TestSavePlugins.SomeText");
        TEST(((NodeText)root.Child(0).Child(1)).Text == "SomeText");
        TEST(((NodeText)root.Child(0).Child(1)).Justification == NodeText.EJustification.EJ_Centre);
        TEST(((NodeText)root.Child(0).Child(1)).Colour == new Colour(0, 255, 128, 30));
        //TEST(root.Child(0).Child(2) as NodeText != null);
        //TEST(root.Child(0).Child(2).Fullname == "TestSavePlugins.SomeMoreText");
        //TEST(((NodeText)root.Child(0).Child(2)).Text == "SomeMoreText");
        TEST(root.Child(0).Child(0).Children.Count == 1);
        TEST(root.Child(0).Child(0).Child(0).Fullname == "TestSavePlugins.SquareB");
        TEST(root.Child(0).Child(0).Child(0).NextPlugin as Bistable != null);
        TEST(root.Child(0).Child(0).Child(0).NextPlugin.Fullname == "TestSavePlugins.Bistable");
        TEST(((Bistable)root.Child(0).Child(0).Child(0).NextPlugin).State == true);
        TEST(root.Child(0).Child(0).Child(0).Children.Count == 0);
    }
}

internal class SuiteLayoutTranslatorSaveTests : Suite
{
    public SuiteLayoutTranslatorSaveTests() : base("Layout translator save tests") {
    }
    
    public override void Test() {
        string startupPath = System.IO.Path.GetDirectoryName(System.Environment.GetCommandLineArgs()[0]);
        PackageManager.Instance.FlushCache();
        // create a layout
        Package package = new Package();
        PackageManager.Instance.Packages.Add(package);
        package.Namespace = "TestSaveTranslators";
        NodeHit root = new NodeHit();
        root.Name = "HitPanel";
        root.Width = 500;
        root.Height = 600;
        package.AddNode(root);
        package.Root = root;
        
        Package packageA = new Package();
        PackageManager.Instance.Packages.Add(packageA);
        packageA.Namespace = "NsA";
        NodeHit rootA = new NodeHit();
        rootA.Name = "Root";
        Node nodeA = new Node();
        nodeA.Name = "NodeA";
        Node nodeB = new Node();
        nodeB.Name = "NodeB";
        packageA.AddNode(nodeA);
        packageA.AddNode(nodeB);
        packageA.AddNode(rootA);
        packageA.Root = rootA;
        
        root.TranslatorOut.AddTranslator(new Translator(new MsgSetActive(nodeA, false), new MsgSetText(nodeB, "Test")));
        root.TranslatorOut.AddTranslator(new Translator(new MsgSetState(nodeA, true), new MsgToggleState(nodeB)));
        root.TranslatorOut.AddTranslator(new Translator(new MsgStateChanged(nodeA, true, false), new MsgStateChanged(nodeB, false, true)));
        root.TranslatorOut.AddTranslator(new Translator(new MsgSetText(nodeA, "Test2"), new MsgSetState(nodeB, false)));
        
        TEST(root.TranslatorOut.Translators.Count == 4);
        root.TranslatorOut.RemoveTranslator((Translator)root.TranslatorOut.Translators[3]);
        
        packageA.Save(System.IO.Path.Combine(startupPath, "../../share/Linn/Test/Gui/TestSaveNsA.xml"));
        package.Save(System.IO.Path.Combine(startupPath, "../../share/Linn/Test/Gui/TestSaveTranslators.xml"));
        Package newLayout = PackageManager.Instance.Load("TestSaveTranslators.xml");
        
        TEST(newLayout.Root.TranslatorOut.Translators.Count == 3);
        Translator translator = (Translator)newLayout.Root.TranslatorOut.Translators[0];
        TEST(translator.FromMessage as MsgSetActive != null);
        TEST(((MsgSetActive)translator.FromMessage).Fullname == "NsA.NodeA");
        TEST(((MsgSetActive)translator.FromMessage).Active == false);
        TEST(translator.ToMessage as MsgSetText != null);
        TEST(((MsgSetText)translator.ToMessage).Fullname == "NsA.NodeB");
        TEST(((MsgSetText)translator.ToMessage).Text == "Test");
        
        translator = (Translator)newLayout.Root.TranslatorOut.Translators[1];
        TEST(translator.FromMessage as MsgSetState != null);
        TEST(((MsgSetState)translator.FromMessage).Fullname == "NsA.NodeA");
        TEST(((MsgSetState)translator.FromMessage).State == true);
        TEST(translator.ToMessage as MsgToggleState != null);
        TEST(((MsgToggleState)translator.ToMessage).Fullname == "NsA.NodeB");
        
        translator = (Translator)newLayout.Root.TranslatorOut.Translators[2];
        TEST(translator.FromMessage as MsgStateChanged != null);
        TEST(((MsgStateChanged)translator.FromMessage).Fullname == "NsA.NodeA");
        TEST(((MsgStateChanged)translator.FromMessage).OldState == true);
        TEST(((MsgStateChanged)translator.FromMessage).NewState == false);
        TEST(translator.ToMessage as MsgStateChanged != null);
        TEST(((MsgStateChanged)translator.ToMessage).Fullname == "NsA.NodeB");
        TEST(((MsgStateChanged)translator.ToMessage).OldState == false);
        TEST(((MsgStateChanged)translator.ToMessage).NewState == true);
        TEST(package.PackageList.Count == 1);
        TEST(PackageManager.Instance.PackageByFilename(package.PackageList[0]).Namespace == "NsA");
    }
}

class Program {
    public static void Main() {
        new RendererNull();
        string startupPath = System.IO.Path.GetDirectoryName(System.Environment.GetCommandLineArgs()[0]);
        PackageManager.Instance.AddPath(System.IO.Path.Combine(startupPath, "../../share/Linn/Test/Gui"));
        Runner runner = new Runner("Gui Layout tests");
        runner.Add(new SuiteLayoutNodeLoadTests());
        runner.Add(new SuiteLayoutTranslatorLoadTests());
        runner.Add(new SuiteLayoutNodeSaveTests());
        runner.Add(new SuiteLayoutTranslatorSaveTests());
        runner.Run();
    }
}

} // Gui
} // Linn
