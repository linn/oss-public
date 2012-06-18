#!/usr/bin/python
import Linn.Path
from Linn.TestFramework.TestFramework import *
import Linn.Gui.Resources.Layout
import Linn.Gui.Resources.Configuration
import Linn.Gui.SceneGraph.Node
import Linn.Gui.SceneGraph.NodeDiscovery
import Linn.Gui.SceneGraph.NodeHit
import Linn.Gui.SceneGraph.NodePolygon
import Linn.Gui.SceneGraph.NodeText
import Linn.Gui.SceneGraph.Srt

class SuiteLayoutLoadTests(Suite):
    def __init__(self):
        Suite.__init__(self, 'Layout load tests')
    
    def Test(self):
        Linn.Gui.Resources.Configuration.SetProjectPath(Linn.Path.Share()+'/Linn/Test/Gui')
        TEST(Linn.Gui.Resources.Configuration.ProjectPath() == Linn.Path.Share()+'/Linn/Test/Gui')
        
        # load a single node
        layout = Linn.Gui.Resources.Layout.Layout('/Test1.xml')
        nodeA = layout.Child(0)
        TEST(layout.NumChildren() == 1)
        TEST(isinstance(nodeA, Linn.Gui.SceneGraph.NodeHit.NodeHit))
        TEST(nodeA.Name() == 'NodeA')
        TEST(nodeA.Namespace() == 'TestNs')
        TEST(nodeA.Parent() == layout)
        TEST(nodeA.Active() == 1)
        TEST(nodeA.NumChildren() == 0)
        TEST(nodeA.LocalSrt().Translation() == Linn.Gui.SceneGraph.Srt.Vector3d(40, 50, 60))
        TEST(nodeA.WorldSrt().Translation() == Linn.Gui.SceneGraph.Srt.Vector3d(40, 50, 60))
        TEST(nodeA.Width() == 480)
        TEST(nodeA.Height() == 300)
        
        # load all types of node
        layout = Linn.Gui.Resources.Layout.Layout('/Test2.xml')
        TEST(layout.NumChildren() == 1)
        node = layout.Child(0)
        TEST(node.Parent() == layout)
        TEST(node.LocalSrt().Translation() == Linn.Gui.SceneGraph.Srt.Vector3d(1, 2, 3))
        TEST(node.WorldSrt().Translation() == Linn.Gui.SceneGraph.Srt.Vector3d(1, 2, 3))
        TEST(node.NumChildren() == 4)
        TEST(isinstance(node.Child(0), Linn.Gui.SceneGraph.Node.Node))
        TEST(node.Child(0).Parent() == node)
        TEST(node.Child(0).LocalSrt().Translation() == Linn.Gui.SceneGraph.Srt.Vector3d(1, 2, 3))
        TEST(node.Child(0).WorldSrt().Translation() == Linn.Gui.SceneGraph.Srt.Vector3d(2, 4, 6))
        TEST(isinstance(node.Child(1), Linn.Gui.SceneGraph.NodeDiscovery.NodeDiscovery))
        TEST(node.Child(1).Parent() == node)
        TEST(node.Child(1).LocalSrt().Translation() == Linn.Gui.SceneGraph.Srt.Vector3d(38, 18, 5))
        TEST(node.Child(1).WorldSrt().Translation() == Linn.Gui.SceneGraph.Srt.Vector3d(39, 20, 8))
        TEST(isinstance(node.Child(2), Linn.Gui.SceneGraph.NodeHit.NodeHit))
        TEST(node.Child(2).Parent() == node)
        TEST(node.Child(2).LocalSrt().Translation() == Linn.Gui.SceneGraph.Srt.Vector3d(4, 5, 6))
        TEST(node.Child(2).WorldSrt().Translation() == Linn.Gui.SceneGraph.Srt.Vector3d(5, 7, 9))
        TEST(isinstance(node.Child(3), Linn.Gui.SceneGraph.NodePolygon.NodePolygon))
        TEST(node.Child(3).Parent() == node)
        TEST(node.Child(3).LocalSrt().Translation() == Linn.Gui.SceneGraph.Srt.Vector3d(7, 8, 9))
        TEST(node.Child(3).WorldSrt().Translation() == Linn.Gui.SceneGraph.Srt.Vector3d(8, 10, 12))
        # cannot test NodeText as it is dependent on wxPython.... nasty!
        #TEST(isinstance(node.Child(4), Linn.Gui.SceneGraph.NodeText.NodeText))
        
class SuiteLayoutSaveTests(Suite):
    def __init__(self):
        Suite.__init__(self, 'Layout save tests')
    
    def Test(self):
        Linn.Gui.Resources.Configuration.SetProjectPath(Linn.Path.Share()+'/Linn/Test/Gui')
        # create a layout
        layout = Linn.Gui.Resources.Layout.Layout('TestSaveNs', 400, 400)
        
        nodeA = Linn.Gui.SceneGraph.Node.Node('NodeA')
        nodeB = Linn.Gui.SceneGraph.Node.Node('NodeB')
        nodeC = Linn.Gui.SceneGraph.Node.Node('NodeC')
        nodeD = Linn.Gui.SceneGraph.Node.Node('NodeD')
        nodeE = Linn.Gui.SceneGraph.Node.Node('NodeE')
        layout.AddNode(nodeA)
        layout.AddNode(nodeB)
        layout.AddNode(nodeC)
        layout.AddNode(nodeD)
        layout.AddNode(nodeE)
        # create an structure like so...
        #                    NodeA
        #        NodeB                    NodeC
        #    NodeD    NodeE
        nodeA.AddChild(nodeB)
        nodeA.AddChild(nodeC)
        nodeB.AddChild(nodeD)
        nodeB.AddChild(nodeE)
        
        # save out the scenegraph
        TEST_THROWS(Linn.Gui.Resources.Layout.NoSaveFilename, layout.Save)
        layout.SaveAs(Linn.Gui.Resources.Configuration.ProjectPath()+'/TestSave.xml')
        
        newLayout = Linn.Gui.Resources.Layout.Layout('/TestSave.xml')
        TEST(newLayout.NumChildren() == 1)
        TEST(newLayout.Child(0).Fullname() == 'TestSaveNs.Root')
        TEST(newLayout.Child(0).NumChildren() == 1)
        TEST(newLayout.Child(0).Child(0).Fullname() == 'TestSaveNs.NodeA')
        TEST(newLayout.Child(0).Child(0).NumChildren() == 2)
        TEST(newLayout.Child(0).Child(0).Child(0).Fullname() == 'TestSaveNs.NodeB')
        TEST(newLayout.Child(0).Child(0).Child(0).NumChildren() == 2)
        TEST(newLayout.Child(0).Child(0).Child(0).Child(0).Fullname() == 'TestSaveNs.NodeD')
        TEST(newLayout.Child(0).Child(0).Child(0).Child(1).Fullname() == 'TestSaveNs.NodeE')
        TEST(newLayout.Child(0).Child(0).Child(1).Fullname() == 'TestSaveNs.NodeC')
        TEST(newLayout.Child(0).Child(0).Child(1).NumChildren() == 0)
        
gRunner = Runner('Gui Layout tests')
gRunner.Add(SuiteLayoutLoadTests())
gRunner.Add(SuiteLayoutSaveTests())
gRunner.Run()
