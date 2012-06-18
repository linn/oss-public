#!/usr/bin/python
from Linn.TestFramework.TestFramework import *
import Linn.Gui.SceneGraph.Srt

class SuiteVector3dTests(Suite):
    def __init__(self):
        Suite.__init__(self, 'Vector3d tests')
    
    def Test(self):
        v1 = Linn.Gui.SceneGraph.Srt.Vector3d()
        TEST(v1.iX == 0 and v1.iY == 0 and v1.iZ == 0)
        
        v2 = Linn.Gui.SceneGraph.Srt.Vector3d(5, 7, 8)
        TEST(v2.iX == 5 and v2.iY == 7 and v2.iZ == 8)
        
        v3 = Linn.Gui.SceneGraph.Srt.Vector3d(10, 15, 20)
        v4 = v3 + v2
        TEST(v2.iX == 5 and v2.iY == 7 and v2.iZ == 8)
        TEST(v3.iX == 10 and v3.iY == 15 and v3.iZ == 20)
        TEST(v4.iX == 15 and v4.iY == 22 and v4.iZ == 28)
        
        v5 = v3 - v2
        TEST(v2.iX == 5 and v2.iY == 7 and v2.iZ == 8)
        TEST(v3.iX == 10 and v3.iY == 15 and v3.iZ == 20)        
        TEST(v5.iX == 5 and v5.iY == 8 and v5.iZ == 12)

gRunner = Runner('Scale/Rotation/Translation tests')
gRunner.Add(SuiteVector3dTests())
gRunner.Run()
