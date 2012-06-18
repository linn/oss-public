import sys
import Linn.Gui.Object
import Linn.Gui.Serialize

class PluginNotFound(Exception):
    def __init__(self, aPlugin):
        self.iPlugin = aPlugin
    def __str__(self):
        return 'Plugin %s not found' % self.iPlugin
    
    
class IPluginObserver:
    def Update(self):
        sys.stderr.write("Unimplemented pure virtual IPluginObserver.Update")
        sys.exit(-1)
    
           
class Plugin(Linn.Gui.Serialize.ISerialize, Linn.Gui.Object.IUpdateObject):
    iUniqueNum = 0
    
    def __init__(self, aName, aPlugin=None):
        self.iObserverList = []
        self.iName = aName
        self.iNamespace = None
        self.iPlugin = aPlugin
        self.iProcessTick = True 
        self.iProcessHit = True
        self.iProcessMotion = True
        self.iProcessUnHit = True 
        self.iProcessMatrix = True 
        self.iProcessVector3d = True
        
    def AddObserver(self, aObserver):
        """Add an editor object observer to the list."""
        self.iObserverList.append(aObserver)
        
    def RemoveObserver(self, aObserver):
        """Remove an editor object observer from the list."""
        self.iObserverList.remove(aObserver)
        
    def ObserverUpdate(self):
        for observer in self.iObserverList:
            observer.Update()
        
    def Name(self):
        return self.iName
    
    def SetName(self, aName):
        if aName == self.iName:
            return
        try:
            import Linn.Gui.Resources.PackageManager
            Linn.Gui.Resources.PackageManager.GetPackageManager().PluginByName(str(self.iNamespace)+'.'+aName)
            self.iName = self.UniqueName(aName)
        except PluginNotFound:
            self.iName = aName
        self.ObserverUpdate()
        
    def Namespace(self):
        return self.iNamespace
    
    def SetNamespace(self, aNamespace):
        self.iNamespace = aNamespace
        self.ObserverUpdate()
        
    def Fullname(self):
        return '%s.%s' % (str(self.iNamespace), self.iName)
        
    def UniqueName(self, aName):
        Plugin.iUniqueNum += 1
        return aName + str(Plugin.iUniqueNum)
        
    def SetPlugin(self, aPlugin):
        self.iPlugin = aPlugin
        
    def Plugin(self):
        return self.iPlugin
    
    def PackageDependencies(self):
        return []
    
    def Load(self, aStream):
        name = aStream.LoadNode('Name')
        if len(name) != 1:
            raise InvalidLayoutFile('<Name>', len(name), 1)
        self.iName = name[0].firstChild.nodeValue
        #print self.iName
        
        plugin = aStream.LoadNode('Plugin')
        if len(plugin) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Plugin>', len(plugin), 1)
        if plugin[0].hasChildNodes():
            self.iPlugin = plugin[0].firstChild.nodeValue
    
    def Save(self, aStream):
        name = aStream.CreateElement('Name')
        name.appendChild(aStream.CreateTextNode(self.Name()))
        aStream.SaveNode(name)
        plugin = aStream.CreateElement('Plugin')
        if self.iPlugin:
            plugin.appendChild(aStream.CreateTextNode(self.iPlugin.Fullname()))
        aStream.SaveNode(plugin)
        
    def Link(self):
        #print 'Linking', self.Name(), '[Plugin]'
        self.iPlugin = self.LinkPluginByName(self.iPlugin)
    
    def LinkPluginByName(self, aPlugin):
        if aPlugin == None:
            return None
        import PackageManager
        return PackageManager.GetPackageManager().PluginByName(aPlugin)
    
    def Tick(self, aDeltaTime):
        if not self.iProcessTick:
            return
        self.iProcessTick = False
        
        if self.iPlugin:
            self.iPlugin.Tick(aDeltaTime)
        
        self.iProcessTick = True
            
    def Hit(self, aVector3d):
        if not self.iProcessHit:
            return
        self.iProcessHit = False
        
        if self.iPlugin:
            self.iPlugin.Hit(aVector3d)
            
        self.iProcessHit = True
        
    def Motion(self, aVector3d):
        if not self.iProcessMotion:
            return
        self.iProcessMotion = False
        if self.iPlugin:
            self.iPlugin.Motion(aVector3d)
        self.iProcessMotion = True
    
    def UnHit(self):
        if not self.iProcessUnHit:
            return
        self.iProcessUnHit = False
        
        if self.iPlugin:
            self.iPlugin.UnHit()
            
        self.iProcessUnHit = True
            
    def Texture(self, aTexture):
        if self.iPlugin:
            self.iPlugin.Texture(aTexture)
        
    def Matrix(self, aMatrix):
        if not self.iProcessMatrix:
            return
        self.iProcessMatrix = False
        
        if self.iPlugin:
            self.iPlugin.Matrix(aMatrix)
            
        self.iProcessMatrix = True
        
    def Vector3d(self, aVector3d):
        if not self.iProcessVector:
            return
        self.iProcessVector = False
        
        if self.iPlugin:
            self.iPlugin.Vector3d(aVector3d)
            
        self.iProcessVector = True
            
    def ReceiveMessage(self, aMessage):
        if self.iPlugin:
            self.iPlugin.ReceiveMessage(aMessage)
            
    def SendMessage(self, aMessage):
        if self.iPlugin:
            self.iPlugin.SendMessage(aMessage)
    

        
        
        
        