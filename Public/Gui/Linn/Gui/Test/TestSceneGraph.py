#!/usr/bin/python
from Linn.TestFramework.TestFramework import *
import Linn.Gui.Resources.Layout
import Linn.Gui.SceneGraph.Node
import Linn.Gui.SceneGraph.Srt

class SuiteBasicNodeTests(Suite):
    def __init__(self):
        Suite.__init__(self, 'Basic Node tests')
    
    def Test(self):
        node1 = Linn.Gui.SceneGraph.Node.Node()
        TEST(node1.Name() == 'Node1')
        TEST(node1.Namespace() == None)
        TEST(node1.Active() == True)
        
        node1.SetName('TestNode')
        TEST(node1.Name() == 'TestNode')
        TEST(node1.Fullname() == 'None.TestNode')
        
        node1.SetNamespace('TestNs')
        TEST(node1.Namespace() == 'TestNs')
        TEST(node1.Fullname() == 'TestNs.TestNode')
        
        node2 = Linn.Gui.SceneGraph.Node.Node('TestNode2')
        node2.SetNamespace('TestNs')
        TEST(node2.Name() == 'TestNode2')
        TEST(node2.Fullname() == 'TestNs.TestNode2')
        TEST(node2.Active() == True)
        
class SuiteTransformationTests(Suite):
    def __init__(self):
        Suite.__init__(self, 'Transformation tests')
    
    def Test(self):
        node1 = Linn.Gui.SceneGraph.Node.Node('Node1')
        node2 = Linn.Gui.SceneGraph.Node.Node('Node2')
        # set scenegraph structure
        # node1
        #    node2
        # check adding a node
        node1.AddChild(node2)
        TEST(node1.Parent() == None)
        TEST(node2.Parent() == node1)
        TEST(node1.NumChildren() == 1)
        TEST(node2.NumChildren() == 0)
        TEST(node1.Child(0) == node2)
        # check we cant add the node twice
        TEST_THROWS(Linn.Gui.SceneGraph.Node.DuplicateElement, node1.AddChild, node2)
        
        # check setting active state
        node2.SetActive(False)
        TEST(node1.Active() == True)
        TEST(node2.Active() == False)
        
        node1.SetActive(False)
        TEST(node1.Active() == False)
        TEST(node2.Active() == False)
        
        node1.SetActive(True)
        TEST(node1.Active() == True)
        TEST(node2.Active() == True)
        
        # check removing a node
        node1.RemoveChild(node2)
        TEST(node2.Parent() == None)
        TEST(node1.NumChildren() == 0)
        # check we cant remove the node twice
        TEST_THROWS(Linn.Gui.SceneGraph.Node.NodeNotParent, node1.RemoveChild, node2)
        node2.iParent = node1
        TEST_THROWS(Linn.Gui.SceneGraph.Node.ElementNotFound, node1.RemoveChild, node2)
        node2.iParent = None
        
        # check initial translations
        trans = node1.LocalSrt().Translation()
        TEST(trans.iX == 0 and trans.iY == 0 and trans.iZ == 0)
        trans = node1.WorldSrt().Translation()
        TEST(trans.iX == 0 and trans.iY == 0 and trans.iZ == 0)
        trans = node2.WorldSrt().Translation()
        TEST(trans.iX == 0 and trans.iY == 0 and trans.iZ == 0)
        
        # check setting new local translation
        newTrans = Linn.Gui.SceneGraph.Srt.Vector3d(100, 50, 10)
        node1.SetLocalTranslation(newTrans)
        trans = node1.LocalSrt().Translation()
        TEST(trans.iX == 100 and trans.iY == 50 and trans.iZ == 10)
        trans = node1.WorldSrt().Translation()
        TEST(trans.iX == 100 and trans.iY == 50 and trans.iZ == 10)
        trans = node2.LocalSrt().Translation()
        TEST(trans.iX == 0 and trans.iY == 0 and trans.iZ == 0)
        trans = node2.WorldSrt().Translation()
        TEST(trans.iX == 0 and trans.iY == 0 and trans.iZ == 0)
        
        # check setting new world translation
        newTrans = Linn.Gui.SceneGraph.Srt.Vector3d(200, 10, 20)
        node2.SetWorldTranslation(newTrans)
        trans = node2.LocalSrt().Translation()
        TEST(trans.iX == 200 and trans.iY == 10 and trans.iZ == 20)
        trans = node2.WorldSrt().Translation()
        TEST(trans.iX == 200 and trans.iY == 10 and trans.iZ == 20)
        
        # add a node and check it obtains the correct local and world translations from its parent
        node1.AddChild(node2)
        trans = node1.LocalSrt().Translation()
        TEST(trans.iX == 100 and trans.iY == 50 and trans.iZ == 10)
        trans = node1.WorldSrt().Translation()
        TEST(trans.iX == 100 and trans.iY == 50 and trans.iZ == 10)
        trans = node2.LocalSrt().Translation()
        TEST(trans.iX == 200 and trans.iY == 10 and trans.iZ == 20)
        trans = node2.WorldSrt().Translation()
        TEST(trans.iX == 300 and trans.iY == 60 and trans.iZ == 30)
        
        # check child node can set its world translation and implicitly its local translation
        newTrans = Linn.Gui.SceneGraph.Srt.Vector3d(150, 100, 20)
        node2.SetWorldTranslation(newTrans)
        trans = node2.LocalSrt().Translation()
        TEST(trans.iX == 50 and trans.iY == 50 and trans.iZ == 10)
        trans = node2.WorldSrt().Translation()
        TEST(trans.iX == 150 and trans.iY == 100 and trans.iZ == 20)
        
        # scenegraph structure
        # node 1
        #    node 2
        #        node 3
        # check propagation of translations and addition of local translation
        node3 = Linn.Gui.SceneGraph.Node.Node('Node3')
        node3.SetLocalTranslation(Linn.Gui.SceneGraph.Srt.Vector3d(1, 1, 1))
        node2.AddChild(node3)
        trans = node3.WorldSrt().Translation()
        TEST(trans.iX == 151 and trans.iY == 101 and trans.iZ == 21)
        TEST(node3.Parent() == node2)
        node1.AddChild(node3)
        TEST(node3.Parent() == node1)
        trans = node3.WorldSrt().Translation()
        TEST(trans.iX == 101 and trans.iY == 51 and trans.iZ == 11)
        


gRunner = Runner('SceneGraph tests')
gRunner.Add(SuiteBasicNodeTests())
gRunner.Add(SuiteTransformationTests())
gRunner.Run()
