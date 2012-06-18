#!/usr/bin/python
from Linn.TestFramework.TestFramework import *

class SuiteEventTests(Suite):
    def __init__(self):
        Suite.__init__(self, 'Event tests')
    
    def Test(self):
        pass
            
gRunner = Runner('Gui SceneGraph Event tests')
gRunner.Add(SuiteEventTests())
gRunner.Run()
