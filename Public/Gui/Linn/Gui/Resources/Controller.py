import sys
import wx
import Linn.Gui.Renderer
import Linn.Gui.Resources.Plugin
import Linn.Gui.Resources.Message
import Linn.Gui.Serialize
import Linn.Gui.SceneGraph.Srt

class Timer(wx.Timer):
    def __init__(self, aParent=None):
        wx.Timer.__init__(self)
        self.iParent = aParent
        
    def __deepcopy__(self, memo={}):
        new = self.__class__()
        new.iParent = memo[id(self.iParent)]
        return new
        
    def Notify(self):
        if self.iParent:
            self.iParent.Notify()
            Linn.Gui.Renderer.GetRenderer().Render()
    
        
class Monostable(Linn.Gui.Resources.Plugin.Plugin):
    def __init__(self, aName=None, aPeriod=0.0):
        if aName == None:
            aName = self.UniqueName('Monostable')
        Linn.Gui.Resources.Plugin.Plugin.__init__(self, aName)
        self.iTimer = Timer(self)
        self.iState = False
        self.iPeriod = aPeriod
    
    def Notify(self):
        self.SetState(False)
        
    def Period(self):
        return self.iPeriod
    
    def SetPeriod(self, aPeriod):
        self.iPeriod = aPeriod
        self.ObserverUpdate()
        
    def SetState(self, aState):
        if aState != self.iState:
            self.iState = aState
            if self.iPlugin:
                self.iPlugin.Vector3d(Linn.Gui.SceneGraph.Srt.Vector3d(int(self.iState), 0, 0))
            msg = Linn.Gui.Resources.Message.MsgStateChanged(self.Fullname(), not self.iState, self.iState)
            self.SendMessage(msg)
        
    def Hit(self, aVector3d):
        self.SetState(True)

    def UnHit(self):
        if self.iPeriod:
            self.iTimer.Start(self.iPeriod*1000, wx.TIMER_ONE_SHOT)
        else:
            self.iTimer.Start(1, wx.TIMER_ONE_SHOT)
                
    def Load(self, aStream):
        Linn.Gui.Resources.Plugin.Plugin.Load(self, aStream)
        
        period = aStream.LoadNode('Period')
        if len(period) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Period>', len(period), 1)
        self.iPeriod = float(period[0].firstChild.nodeValue)
        #print 'period =', self.iPeriod
    
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'Monostable')
            
        Linn.Gui.Resources.Plugin.Plugin.Save(self, aStream)
        
        period = aStream.CreateElement('Period')
        period.appendChild(aStream.CreateTextNode(str(self.iPeriod)))
        aStream.SaveNode(period)
                    

class Bistable(Linn.Gui.Resources.Plugin.Plugin):
    def __init__(self, aName=None, aStartState=0):
        if aName == None:
            aName = self.UniqueName('Bistable')
        Linn.Gui.Resources.Plugin.Plugin.__init__(self, aName)
        self.iStartState = False
        self.iState = False
        
    def StartState(self):
        return self.iStartState
    
    def SetStartState(self, aStartState):
        self.iStartState = aStartState
        self.ObserverUpdate()
        
    def SetState(self, aState):
        if aState != self.iState:
            self.iState = aState
            if self.iPlugin:
                self.iPlugin.Vector3d(Linn.Gui.SceneGraph.Srt.Vector3d(int(self.iState), 0, 0))
            msg = Linn.Gui.Resources.Message.MsgStateChanged(self.Fullname(), not self.iState, self.iState)
            self.SendMessage(msg)
        
    def Hit(self, aVector3d):
        self.SetState(not self.iState)
    
    def ReceiveMessage(self, aMessage):
        if aMessage.Fullname() == self.Fullname():
            if isinstance(aMessage, Linn.Gui.Resources.Message.MsgSetState):
                self.SetState(aMessage.Value())
            if isinstance(aMessage, Linn.Gui.Resources.Message.MsgToggleState):
                self.SetState(not self.iState)
            return
        Linn.Gui.Resources.Plugin.Plugin.ReceiveMessage(self, aMessage)
    
    def Load(self, aStream):
        Linn.Gui.Resources.Plugin.Plugin.Load(self, aStream)
        
        state = aStream.LoadNode('StartState')
        if len(state) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<StartState>', len(state), 1)
        value = state[0].firstChild.nodeValue.lower()
        if(value == '1' or value == 'true'):
            self.iStartState = True
        else:
            self.iStartState = False
        self.iState = self.iStartState
        #print 'startState =', self.iStartState
    
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'Bistable')
            
        Linn.Gui.Resources.Plugin.Plugin.Save(self, aStream)
        
        state = aStream.CreateElement('StartState')
        state.appendChild(aStream.CreateTextNode(str(self.iStartState).lower()))
        aStream.SaveNode(state)
        
        
class Counter(Linn.Gui.Resources.Plugin.Plugin):
    def __init__(self, aName=None, aCountMax=0, aCountsPerSecond=0, aLoop=False):
        if aName == None:
            aName = self.UniqueName('Counter')
        Linn.Gui.Resources.Plugin.Plugin.__init__(self, aName)
        self.iTimer = Timer(self)
        self.iCount = 0
        self.iCountMax = aCountMax
        if aCountsPerSecond:
            self.iTimeStep = 1.0 / aCountsPerSecond
        else:
            self.iTimeStep = 0.0
        self.iCounting = False
        self.iReverseCount = 0
        self.iLoop = aLoop
        
    def MaxCount(self):
        return self.iCountMax
    
    def SetMaxCount(self, aMaxCount):
        self.iCountMax = aMaxCount
        if self.iCount >= self.iCountMax:
            self.SetCount(self.iCountMax - 1)
        self.ObserverUpdate()
        
    def Count(self):
        return self.iCount
    
    def SetCount(self, aCount):
        if aCount < 0:
            self.iCount = 0
        elif aCount >= self.iCountMax:
            self.iCount = self.iCountMax - 1
        else:
            self.iCount = aCount
        if self.iPlugin:
            self.iPlugin.Vector3d(Linn.Gui.SceneGraph.Srt.Vector3d(self.iCount, 0.0, 0.0))
        self.ObserverUpdate()
        
    def Fps(self):
        if self.iTimeStep:
            return 1.0 / self.iTimeStep
        else:
            return self.iTimeStep
        
    def SetFps(self, aFps):
        if aFps > 0:
            self.iTimeStep = 1.0 / aFps
        else:
            self.iTimeStep = 0
        self.ObserverUpdate()
        
    def Loop(self):
        return self.iLoop
    
    def SetLoop(self, aLoop):
        self.iLoop = aLoop
        if self.iLoop:
            self.Start()
        self.ObserverUpdate()
        
    def Counting(self):
        return self.iCounting
        
    def Start(self):
        self.iCounting = True
        if self.iTimeStep:
            self.iTimer.Start(self.iTimeStep*1000, wx.TIMER_ONE_SHOT)
        self.ObserverUpdate()
        
    def Stop(self):
        self.iCounting = False
        self.iTimer.Stop()
        self.ObserverUpdate()
            
    def Notify(self):
        counts = 1    # we have no count length so just update as fast as possible
        # subtract frames from index if counting in reverse
        count = self.iCount - (~counts + ((2 * self.iReverseCount * counts) + 1))
        newCount = count % self.iCountMax
        if newCount == count or self.iLoop:
            # inc/dec or wrap the counter
            self.iCount = newCount
            if self.iTimeStep:
                self.iTimer.Start(self.iTimeStep*1000, wx.TIMER_ONE_SHOT)
        else:
            # clamp counter to bottom/top depending on count direction
            self.iCount = (not self.iReverseCount) * (self.iCountMax - 1)
            self.iCounting = False
            msg = Linn.Gui.Resources.Message.MsgCountEnd(self.Fullname())
            self.SendMessage(msg)
        if self.iPlugin:
            self.iPlugin.Vector3d(Linn.Gui.SceneGraph.Srt.Vector3d(self.iCount, 0.0, 0.0))
        self.ObserverUpdate()
        
    def Vector3d(self, aVector3d):
        self.iReverseCount = not aVector3d.iX
        self.Start()
            
    def ReceiveMessage(self, aMessage):
        if aMessage.Fullname() == self.Fullname():
            if isinstance(aMessage, Linn.Gui.Resources.Message.MsgCountStart):
                self.Start()
            if isinstance(aMessage, Linn.Gui.Resources.Message.MsgCountStop):
                self.Stop()
            return
        Linn.Gui.Resources.Plugin.Plugin.ReceiveMessage(self, aMessage)
            
    def Load(self, aStream):
        Linn.Gui.Resources.Plugin.Plugin.Load(self, aStream)
        
        count = aStream.LoadNode('MaxCount')
        if len(count) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<MaxCount>', len(count), 1)
        self.iCountMax = int(count[0].firstChild.nodeValue)
        #print 'countMax =', self.iCountMax
        
        cps = aStream.LoadNode('CountsPerSecond')
        if len(cps) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<CountsPerSecond>', len(cps), 1)
        countsPerSecond = float(cps[0].firstChild.nodeValue)
        self.SetFps(countsPerSecond)
        #print 'cps =', self.iTimeStep
        
        loop = aStream.LoadNode('Loop')
        if len(cps) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Loop>', len(loop), 1)
        value = loop[0].firstChild.nodeValue.lower()
        if(value == '1' or value == 'true'):
            self.iLoop = True
        else:
            self.iLoop = False
        if self.iLoop:
            self.iCounting = True
            if self.iTimeStep:
                self.iTimer.Start(self.iTimeStep*1000, wx.TIMER_ONE_SHOT)
        #print 'loop =', self.iLoop
    
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'Counter')
            
        Linn.Gui.Resources.Plugin.Plugin.Save(self, aStream)
        
        count = aStream.CreateElement('MaxCount')
        count.appendChild(aStream.CreateTextNode(str(int(self.iCountMax))))
        aStream.SaveNode(count)
        
        cps = aStream.CreateElement('CountsPerSecond')
        if self.iTimeStep:
            cps.appendChild(aStream.CreateTextNode(str(1.0 / self.iTimeStep)))
        else:
            cps.appendChild(aStream.CreateTextNode(str(0)))
        aStream.SaveNode(cps)
        
        loop = aStream.CreateElement('Loop')
        loop.appendChild(aStream.CreateTextNode(str(self.iLoop).lower()))
        aStream.SaveNode(loop)
    
    
    
            