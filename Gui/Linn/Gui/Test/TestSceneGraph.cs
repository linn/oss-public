using Linn.TestFramework;
using Linn.Gui.Scenegraph;

namespace Linn {
namespace Gui {

internal class SuiteBasicNodeTests : Suite
{
    public SuiteBasicNodeTests() : base("Basic Node tests") {
    }
    
    public override void Test() {
        Node node1 = new Node();
        TEST(node1.Name == "Node1");
        TEST(node1.Namespace == "None");
        TEST(node1.Active == true);
        
        node1.Name = "TestNode";
        TEST(node1.Name == "TestNode");
        TEST(node1.Fullname == "None.TestNode");
        
        node1.Namespace = "TestNs";
        TEST(node1.Namespace == "TestNs");
        TEST(node1.Fullname == "TestNs.TestNode");
        
        Node node2 = new Node();
        TEST(node2.Name == "Node2");
        node2.Name = "TestNode2";
        node2.Namespace = "TestNs";
        TEST(node2.Name == "TestNode2");
        TEST(node2.Fullname == "TestNs.TestNode2");
        TEST(node2.Active == true);
    }
}

internal class SuiteTransformationTests : Suite
{
    public SuiteTransformationTests() : base("Transformation tests") {
    }
    
    public override void Test() {
        Node node1 = new Node();
        node1.Name = "Node1";
        Node node2 = new Node();
        node2.Name = "Node2";
        // set scenegraph structure
        // node1
        //    node2
        // check adding a node
        node1.AddChild(node2);
        TEST(node1.Parent == null);
        TEST(node2.Parent == node1);
        TEST(node1.Children.Count == 1);
        TEST(node2.Children.Count == 0);
        TEST(node1.Child(0) == node2);
        // check we cant add the node twice
        //TEST_THROWS(Linn.Gui.SceneGraph.Node.DuplicateElement, node1.AddChild, node2)
        
        // check setting active state
        node2.Active = false;
        TEST(node1.Active == true);
        TEST(node2.Active == false);
        
        node1.Active = false;
        TEST(node1.Active == false);
        TEST(node2.Active == false);
        
        node1.Active = true;
        TEST(node1.Active == true);
        TEST(node2.Active == true);
        
        // check removing a node
        node1.RemoveChild(node2);
        TEST(node2.Parent == null);
        TEST(node1.Children.Count == 0);
        // check we cant remove the node twice
        //TEST_THROWS(Linn.Gui.SceneGraph.Node.NodeNotParent, node1.RemoveChild, node2)
        node2.Parent = node1;
        //TEST_THROWS(Linn.Gui.SceneGraph.Node.ElementNotFound, node1.RemoveChild, node2)
        node2.Parent = null;
        
        // check initial translations
        Vector3d trans = node1.LocalSrt.Translation;
        TEST(trans.X == 0 && trans.Y == 0 && trans.Z == 0);
        trans = node1.WorldSrt.Translation;
        TEST(trans.X == 0 && trans.Y == 0 && trans.Z == 0);
        trans = node2.WorldSrt.Translation;
        TEST(trans.X == 0 && trans.Y == 0 && trans.Z == 0);
        
        // check setting new local translation
        Vector3d newTrans = new Vector3d(100, 50, 10);
        node1.LocalTranslation = newTrans;
        trans = node1.LocalSrt.Translation;
        TEST(trans.X == 100 && trans.Y == 50 && trans.Z == 10);
        trans = node1.WorldSrt.Translation;
        TEST(trans.X == 100 && trans.Y == 50 && trans.Z == 10);
        trans = node2.LocalSrt.Translation;
        TEST(trans.X == 0 && trans.Y == 0 && trans.Z == 0);
        trans = node2.WorldSrt.Translation;
        TEST(trans.X == 0 && trans.Y == 0 && trans.Z == 0);
        
        // check setting new world translation
        newTrans = new Vector3d(200, 10, 20);
        node2.WorldTranslation = newTrans;
        trans = node2.LocalSrt.Translation;
        TEST(trans.X == 200 && trans.Y == 10 && trans.Z == 20);
        trans = node2.WorldSrt.Translation;
        TEST(trans.X == 200 && trans.Y == 10 && trans.Z == 20);
        
        // add a node and check it obtains the correct local and world translations from its parent
        node1.AddChild(node2);
        trans = node1.LocalSrt.Translation;
        TEST(trans.X == 100 && trans.Y == 50 && trans.Z == 10);
        trans = node1.WorldSrt.Translation;
        TEST(trans.X == 100 && trans.Y == 50 && trans.Z == 10);
        trans = node2.LocalSrt.Translation;
        TEST(trans.X == 200 && trans.Y == 10 && trans.Z == 20);
        trans = node2.WorldSrt.Translation;
        TEST(trans.X == 300 && trans.Y == 60 && trans.Z == 30);
        
        // check child node can set its world translation and implicitly its local translation
        newTrans = new Vector3d(150, 100, 20);
        node2.WorldTranslation = newTrans;
        trans = node2.LocalSrt.Translation;
        TEST(trans.X == 50 && trans.Y == 50 && trans.Z == 10);
        trans = node2.WorldSrt.Translation;
        TEST(trans.X == 150 && trans.Y == 100 && trans.Z == 20);
        
        // scenegraph structure
        // node 1
        //    node 2
        //        node 3
        // check propagation of translations and addition of local translation
        Node node3 = new Node();
        node3.LocalTranslation = new Vector3d(1, 1, 1);
        node2.AddChild(node3);
        trans = node3.WorldSrt.Translation;
        TEST(trans.X == 151 && trans.Y == 101 && trans.Z == 21);
        TEST(node3.Parent == node2);
        node1.AddChild(node3);
        TEST(node3.Parent == node1);
        trans = node3.WorldSrt.Translation;
        TEST(trans.X == 101 && trans.Y == 51 && trans.Z == 11);
    }
}

public class TestProgram
{
    public static void Main() {
        new RendererNull();
        Runner runner = new Runner("SceneGraph tests");
        runner.Add(new SuiteBasicNodeTests());
        runner.Add(new SuiteTransformationTests());
        runner.Run();
    }
}

} // Gui
} // Linn
