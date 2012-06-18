import os
import Linn.Gui.XmlStream
import Linn.Gui.Serialize
import Linn.Gui.SceneGraph.Node
import Linn.Gui.SceneGraph.NodeHit
import Linn.Gui.Resources.Plugin
import PackageManager
import Package
import TextureManager
import Configuration

class NoSaveFilename(Exception):
    pass

class IncorrectNumArgs(Exception):
    def __str__(self):
        return 'Layout requires either 0,1, or 3 constructor arguments'
    
gLayout = None

class Layout(Linn.Gui.SceneGraph.Node.Node, Linn.Gui.Resources.Plugin.IPluginObserver):
    def __init__(self, *args, **kw):
        if len(args) == 0:
            self.ConstructZeroArg()
        elif len(args) == 1:
            self.ConstructOneArg(args[0])
        elif len(args) == 3:
            self.ConstructThreeArg(args[0], args[1], args[2])
        else:
            raise IncorrectNumArgs()
        
    def ConstructZeroArg(self):
        Linn.Gui.SceneGraph.Node.Node.__init__(self, 'SceneGraphRoot')
        self.iPackage = None
        self.iWidth = 0
        self.iHeight = 0
        
    def ConstructOneArg(self, aFilename):
        self.ConstructZeroArg()
        self.Load(aFilename)
        
    def ConstructThreeArg(self, aNamespace, aWidth, aHeight):
        self.ConstructZeroArg()
        self.iPackage = Package.Package(None, 'Layout', aNamespace)
        self.iPackage.AddNode(Linn.Gui.SceneGraph.NodeHit.NodeHit('Root', aWidth, aHeight))
        self.AddChild(self.iPackage.Root())
        PackageManager.GetPackageManager().AddPackage(self.iPackage)
        self.SetNamespace(aNamespace)
        self.iWidth = aWidth
        self.iHeight = aHeight
        
    def Update(self):
        self.iWidth = self.Root().Width()
        self.iHeight = self.Root().Height()
        
    def Import(self, aFilename):
        # remove the root path, it gets added by the package, when loading
        aFilename = aFilename.replace('\\', '/')
        aFilename = aFilename.replace(Configuration.ProjectPath(), '')
        #print 'Importing...'
        newPackage = PackageManager.GetPackageManager().Load(aFilename)
        if newPackage == None:
            return
        if newPackage.Type() == 'Layout':
            root = newPackage.Root()
            root.SetActive(False)
            self.AddChild(root)
            #self.Child(0).AddChild(root)
            for filename in newPackage.PackageList():
                package = PackageManager.GetPackageManager().PackageByName(filename)
                if package.Type() == 'Layout' and package != self.iPackage:
                    root = package.Root()
                    root.SetActive(False)
                    #self.AddChild(root)
        else:
            raise PackageManager.CorruptFile(aFilename, 'package is not a Layout package')
        
        #print '%d textures in cache' % TextureManager.GetTextureManager().NumTextures()
        
    def Load(self, aFilename):
        PackageManager.GetPackageManager().FlushCache()
        TextureManager.GetTextureManager().FlushCache()
        # remove the root path, it gets added by the package, when loading
        aFilename = aFilename.replace('\\', '/')
        aFilename = aFilename.replace(Configuration.ProjectPath(), '')
        #print 'Loading...'
        newPackage = PackageManager.GetPackageManager().Load(aFilename)
        if newPackage == None:
            return
        if newPackage.Type() == 'Layout':
            self.AddChild(newPackage.Root())
            for filename in newPackage.PackageList():
                package = PackageManager.GetPackageManager().PackageByName(filename)
                if package.Type() == 'Layout':
                    root = package.Root()
                    root.SetActive(False)
                    #self.AddChild(root)
        else:
            raise PackageManager.CorruptFile(aFilename, 'package is not a Layout package')
        self.iPackage = newPackage
        
        self.SetNamespace(self.iPackage.Namespace())
        self.iWidth = self.Root().Width()
        self.iHeight = self.Root().Height()
        #print '%d textures in cache' % TextureManager.GetTextureManager().NumTextures()
        
        self.Root().AddObserver(self)
    
    def SaveAs(self, aFilename):
        return self.Save(aFilename)
        
    def Save(self, aFilename=None):
        if self.iPackage:
            if not self.iPackage.Filename() and not aFilename:
                raise NoSaveFilename
            elif self.iPackage.Filename() and not aFilename:
                aFilename = Configuration.ProjectPath() + self.iPackage.Filename()
            
            #print 'Saving...'
            aFilename = aFilename.replace('\\', '/')
            xmlFile = Linn.Gui.XmlStream.XmlStream()
            xmlFile.Open(aFilename, Linn.Gui.XmlStream.kWrite)
            
            if self.iPackage:
                if self.iPackage.Type() == 'Layout':
                    self.iPackage.Save(xmlFile)
                else:
                    raise Linn.Gui.Serialize.InvalidLayoutFile("<Package type=Layout>'", 0, 1)
                
            # write xml to file
            xmlFile.Close()
            return True
        return False
    
    def SetNamespace(self, aNamespace):
        Linn.Gui.SceneGraph.Node.Node.SetNamespace(self, aNamespace)
        if self.iPackage:
            self.iPackage.SetNamespace(aNamespace)
        
    def Filename(self):
        if self.iPackage and self.iPackage.Filename():
            return Configuration.ProjectPath() + self.iPackage.Filename()
        return None
    
    def Root(self):
        return self.iPackage.Root()
    
    def Width(self):
        return self.iWidth
    
    def Height(self):
        return self.iHeight
    
    def AddNode(self, aNode):
        #print 'Adding node to package...'
        self.iPackage.AddNode(aNode)
        
    def RemoveNode(self, aNode):
        #print 'Deleting node from package...'
        return self.iPackage.RemoveNode(aNode)
        
        
    