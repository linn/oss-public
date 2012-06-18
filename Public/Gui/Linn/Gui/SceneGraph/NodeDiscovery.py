import Linn.Gui.SceneGraph.Node
import Linn.Gui.SceneGraph.NodeHit
import Linn.Gui.SceneGraph.NodePolygon
import Linn.Gui.SceneGraph.NodeText
import Linn.Gui.Resources.Message
import Linn.Gui.Resources.TextureManager
import Linn.Gui.Resources.TextureArray
import Linn.Gui.Resources.Controller
import Linn.Control.Upnp.ControlPoint.Device

class NodeDevice(Linn.Gui.SceneGraph.Node.Node):
    def __init__(self, aTextureOn, aTextureOff, aDevice=None, aX=0, aIndex=None):
        name = self.UniqueName('NodeDevice')
        if aDevice:
            name = aDevice.Uuid()
        Linn.Gui.SceneGraph.Node.Node.__init__(self, name)
        
        self.iDevice = aDevice
        
        textureArray = Linn.Gui.Resources.TextureArray.TextureArrayFixed(name+'.TextureArrayFixed', [aTextureOff, aTextureOn])
        bi = Linn.Gui.Resources.Controller.Bistable('Discovery.Bistable'+str(aIndex), 0.0)
        
        self.iIcon = Linn.Gui.SceneGraph.NodePolygon.NodePolygon(name+'Icon')
        self.iIcon.SetPlugin(bi)
        bi.SetPlugin(textureArray)
        textureArray.SetPlugin(self.iIcon)
        self.iIcon.Texture(aTextureOff)
        trans = self.iIcon.LocalSrt().Translation()
        self.iIcon.SetLocalTranslation(trans)
        self.AddChild(self.iIcon)
        
        self.iText = Linn.Gui.SceneGraph.NodeText.NodeText(name+'Text', aDevice.FriendlyName())
        self.iText.SetJustification(Linn.Gui.SceneGraph.NodeText.NodeText.iCentre)
        trans = self.iText.LocalSrt().Translation()
        trans.iX = self.iIcon.Width() * 0.5
        trans.iY = self.iIcon.LocalSrt().Translation().iY + self.iIcon.Height()
        self.iText.SetLocalTranslation(trans)
        self.AddChild(self.iText)
        
        trans = self.LocalSrt().Translation()
        trans.iX = aX
        trans.iY = 1
        trans.iZ = 10
        self.SetLocalTranslation(trans)
        
    def SetNamespace(self, aNamespace):
        Linn.Gui.SceneGraph.Node.Node.SetNamespace(self, aNamespace)
        self.iIcon.SetNamespace(aNamespace)
        self.iIcon.Plugin().SetNamespace(aNamespace)
        self.iIcon.Plugin().Plugin().SetNamespace(aNamespace)
        self.iText.SetNamespace(aNamespace)
        
    def SetIndex(self, aIndex):
        self.iIcon.Plugin().SetName('Discovery.Bistable'+str(aIndex))
        
    def Device(self):
        return self.iDevice
        

class NodeDiscovery(Linn.Gui.SceneGraph.NodePolygon.NodePolygon):
    def __init__(self, aName=None):
        if aName == None:
            aName = self.UniqueName('NodeDiscovery')
        Linn.Gui.SceneGraph.NodePolygon.NodePolygon.__init__(self, aName)
        self.iDeviceList = []
        self.iNdPosList = []
        self.iDeviceIconOn = None
        self.iDeviceIconOff = None 
        
    def Load(self, aStream):
        Linn.Gui.SceneGraph.NodePolygon.NodePolygon.Load(self, aStream)
        
        di = aStream.LoadNode('DeviceIcon')
        if len(di) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<DeviceIcon>', len(di), 1)
        if di[0].hasChildNodes():
            aStream.PushContextNode(di[0])
            textures = aStream.LoadNode('Texture')
            if len(textures) != 3 and len(textures) != 2:
                raise Linn.Gui.Serialize.InvalidLayoutFile('<Texture>', len(textures), 3)
            self.iDeviceIconOn = textures[0].firstChild.nodeValue
            #print self.iDeviceIconOn
            self.iDeviceIconOff = textures[1].firstChild.nodeValue
            #print self.iDeviceIconOff
            aStream.PopContextNode()
    
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'NodeDiscovery')
            
        Linn.Gui.SceneGraph.NodePolygon.NodePolygon.Save(self, aStream)
        
        di = aStream.CreateElement('DeviceIcon')
        if self.iDeviceIconOn:
            texture = aStream.CreateElement('Texture')
            texture.appendChild(aStream.CreateTextNode(self.iDeviceIconOn.Filename()))
            di.appendChild(texture)
        if self.iDeviceIconOff:
            texture = aStream.CreateElement('Texture')
            texture.appendChild(aStream.CreateTextNode(self.iDeviceIconOff.Filename()))
            di.appendChild(texture)
        aStream.SaveNode(di)
        
    def Link(self):
        #print 'Linking', self.Fullname(), '[TextureArray]'
        Linn.Gui.SceneGraph.NodePolygon.NodePolygon.Link(self)
        
        if self.iDeviceIconOn:
            self.iDeviceIconOn = Linn.Gui.Resources.TextureManager.GetTextureManager().TextureByNameOrLoad(self.iDeviceIconOn)
            #print self.iDeviceIconOn
        if self.iDeviceIconOff:
            self.iDeviceIconOff = Linn.Gui.Resources.TextureManager.GetTextureManager().TextureByNameOrLoad(self.iDeviceIconOff)
            #print self.iDeviceIconOff
        self.iLinked = True
        
    def ReceiveMessage(self, aMessage):
        if self.ProcessMessage(aMessage):
            Linn.Gui.SceneGraph.NodePolygon.NodePolygon.ReceiveMessage(self, aMessage)
            
            if isinstance(aMessage, Linn.Gui.Resources.Message.MsgDeviceDiscovered):
                if aMessage.Fullname() == self.Fullname():
                    self.AddDevice(aMessage.Device())
            if isinstance(aMessage, Linn.Gui.Resources.Message.MsgDeviceRemoved):
                if aMessage.Fullname() == self.Fullname():
                    self.RemoveDevice(aMessage.Device())
            
    def AddDevice(self, aDevice):
        x = 8
        if len(self.iDeviceList):
            x = 80 * len(self.iDeviceList) + 8
        device = NodeDevice(self.iDeviceIconOn, self.iDeviceIconOff, aDevice, x, len(self.iDeviceList))
        device.SetNamespace(self.Namespace())
        # ensure device node is active/non-active in line with DiscoveryNode
        device.SetActive(self.Active())
        self.iDeviceList.append(device)
        self.AddChild(device)
        Linn.Gui.Renderer.GetRenderer().Render()
        
    def RemoveDevice(self, aDevice):
        for device in self.iDeviceList:
            if device.Device() == aDevice:
                self.iDeviceList.remove(device)
                self.RemoveChild(device)
                break
        x = 8
        index = 0
        # update remaining device's positions
        for device in self.iDeviceList:
            trans = device.LocalSrt().Translation()
            trans.iX = x
            device.SetLocalTranslation(trans)
            device.SetIndex(index)
            x += 80
            index += 1
        Linn.Gui.Renderer.GetRenderer().Render()
        
        





            
