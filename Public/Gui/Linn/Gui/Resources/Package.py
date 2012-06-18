import os
import Linn.Gui.Serialize
import Linn.Gui.XmlStream
import Linn.Gui.Factory
import Linn.Gui.Resources.Plugin
import Linn.Gui.SceneGraph.Visitor
import PackageManager
import Configuration

class InvalidVersion(Exception):
    def __init__(self, aVersion):
        self.iVersion = aVersion
    def __str__(self):
        return 'Invalid file version: Found %d expected %d' % (self.iVersion, Package.iVersion)

class DuplicatePluginName(Exception):
    def __init__(self, aName):
        self.iName = aName
    def __str__(self):
        return 'Error: Duplicate plugin name %s found' % self.iName
    

class Package(Linn.Gui.Serialize.ISerialize):
    iVersion = 6
    def __init__(self, aFilename=None, aType=None, aNamespace=None):
        if aFilename:
            self.iFilename = aFilename
        else:
            self.iFilename = None
        self.iNamespace = aNamespace
        self.iType = aType
        self.iPluginList = []  # holds references to all plugins currently in package
        self.iPackageList = [] # holds names of all packages this package references
        self.iRoot = None
        
        if self.iFilename:
            xmlFile = Linn.Gui.XmlStream.XmlStream()
            try:
                xmlFile.Open(Configuration.ProjectPath() + self.iFilename, Linn.Gui.XmlStream.kRead)
            except:
                print 'Trying default path...'
                xmlFile.Open(Configuration.DefaultPath() + self.iFilename, Linn.Gui.XmlStream.kRead)
            self.Load(xmlFile)
            xmlFile.Close()
        
    def Filename(self):
        return self.iFilename
    
    def Namespace(self):
        return self.iNamespace
    
    def SetNamespace(self, aNamespace):
        self.iNamespace = aNamespace
        for plugin in self.iPluginList:
            plugin.SetNamespace(aNamespace)
    
    def Type(self):
        return self.iType
    
    def AddPlugin(self, aPlugin, aInitialPlugin=None):
        aPlugin.SetNamespace(self.iNamespace)
        self.iPluginList.append(aPlugin)
        plugin = aPlugin.Plugin()
        if plugin and aInitialPlugin != plugin:
            self.AddPlugin(plugin, aInitialPlugin)
            
    def DeletePlugin(self, aPlugin, aInitialPlugin=None):
        pluginList = [aPlugin]
        if aPlugin.Namespace() == self.iNamespace:
            self.iPluginList.remove(aPlugin)
        plugin = aPlugin.Plugin()
        if plugin and aInitialPlugin != plugin:
            pluginList.extend(self.DeletePlugin(plugin, aInitialPlugin))
        return pluginList
            
    def AddNode(self, aNode):
        self.AddPlugin(aNode, aNode)
        if self.iRoot == None:
            #print 'Setting', aNode.Name(), 'as root'
            self.iRoot = aNode
        else:
            # ensure that the newly added polygon is on top of all current polygons
            zSearch = Linn.Gui.SceneGraph.Visitor.FindGreatestZ()
            z = zSearch.Search(self.iRoot)
            v = aNode.LocalSrt().Translation()
            v.iZ = z + 1
            aNode.SetLocalTranslation(v)
            self.iRoot.AddChild(aNode)
            
    def RemoveNode(self, aNode):
        pluginList = []
        aNode.Parent().RemoveChild(aNode)
        pluginList.extend(self.DeletePlugin(aNode, aNode))
        numChildren = aNode.NumChildren()
        for i in range(numChildren):
            pluginList.extend(self.RemoveNode(aNode.Child(numChildren - 1 - i)))
        return pluginList
            
    def Root(self):
        return self.iRoot
            
    def SetRoot(self, aPlugin):
        self.iRoot = aPlugin
    
    def PluginList(self):
        return self.iPluginList
    
    def PluginByName(self, aPlugin):
        # extract name and namespace from plugin's fullname
        index = aPlugin.find('.')
        if index:
            ns = aPlugin[:index]
            name = aPlugin[index+1:]
            if str(self.iNamespace) == ns:
                for plugin in self.iPluginList:
                    if plugin.Name() == name:
                        return plugin
        raise Linn.Gui.Resources.Plugin.PluginNotFound(aPlugin)

    def PackageList(self):
        return self.iPackageList
    
    def CalculatePackageReferences(self):
        """From the package's plugins obtain a list of external
            packages that this package is dependent on."""
        namespaceList = []
        for plugin in self.iPluginList:
            deps = plugin.PackageDependencies()
            for i in deps:
                try:
                    namespaceList.index(i)
                except Exception, e:
                    print 'Adding', i
                    namespaceList.append(i)
            nextPlugin = plugin.Plugin()
            while nextPlugin and nextPlugin != plugin:
                if nextPlugin.Namespace() != self.Namespace():
                    deps = nextPlugin.PackageDependencies()
                    for i in deps:
                        try:
                            namespaceList.index(i)
                        except Exception, e:
                            print 'Adding', i
                            namespaceList.append(i)
                nextPlugin = nextPlugin.Plugin()
                
        self.iPackageList = []
        for package in PackageManager.GetPackageManager().PackageList():
            try:
                index = namespaceList.index(package.Namespace())
                namespaceList.remove(namespaceList[index])
                if package.Namespace() != self.Namespace():
                    self.iPackageList.append(package.Filename())
            except Exception, e:
                pass
            
        for namespace in namespaceList:
            print 'WARNING: Could not find package for namespace %s' % namespace
    
    def Load(self, aStream):
        # obtain a list of all referenced packages
        package = aStream.LoadNode('Package')
        assert(len(package)==1)
        aStream.PushContextNode(package[0])
        self.iType = package[0].attributes[(None,'type')].value
        
        #print 'Loading package', self.iFilename
        #print '    Type', self.iType
        
        version = aStream.LoadNode('Version')
        if len(version) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Version>', len(version), 1)
        self.iVersion = int(version[0].firstChild.nodeValue)
        if self.iVersion != Package.iVersion:
            raise InvalidVersion(self.iVersion)
        
        namespace = aStream.LoadNode('Namespace')
        if len(namespace) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Namespace>', len(namespace), 1)
        self.iNamespace = namespace[0].firstChild.nodeValue
        
        packages = aStream.LoadNode('Package')
        #print '    %d package references' % len(packages)
        for package in packages:
            self.iPackageList.append(package.firstChild.nodeValue)
            
        # load in all the plugins
        plugins = aStream.LoadNode('Plugin')
        #print '    %d plugins' % len(plugins)
        for plugin in plugins:
            classType = plugin.attributes[(None,'class')].value
            aStream.PushContextNode(plugin)
            newPlugin = Linn.Gui.Factory.CreatePlugin(self.iNamespace, classType, aStream)
            aStream.PopContextNode()
            self.iPluginList.append(newPlugin)
            if classType == 'NodeHit' and newPlugin.Parent() == None:
               self.SetRoot(newPlugin) 
        aStream.PopContextNode()
    
    def Save(self, aStream):
        #print 'Saving package', aStream.Filename()
        self.iFilename = aStream.Filename()
        self.iFilename = self.iFilename.replace(Configuration.ProjectPath(), '')
        #print '    Type', self.iType
        package = aStream.CreateElement('Package')
        type = aStream.CreateAttribute('type', self.iType)
        package.setAttributeNodeNS(type)
        aStream.PushContextNode(package)
        
        version = aStream.CreateElement('Version')
        version.appendChild(aStream.CreateTextNode(Package.iVersion))
        aStream.SaveNode(version)
        
        namespace = aStream.CreateElement('Namespace')
        namespace.appendChild(aStream.CreateTextNode(self.iNamespace))
        aStream.SaveNode(namespace)
        
        self.CalculatePackageReferences()
        
        for packageFilename in self.iPackageList:
            packageRef = aStream.CreateElement('Package')
            packageRef.appendChild(aStream.CreateTextNode(packageFilename))
            aStream.SaveNode(packageRef)
        #print '    %d package references' % len(self.iPackageList)
        
        for plugin in self.iPluginList:
            pluginNode = aStream.CreateElement('Plugin')
            classType = aStream.CreateAttribute('class', None)
            pluginNode.setAttributeNodeNS(classType)
            aStream.PushContextNode(pluginNode)
            plugin.Save(aStream)
            aStream.PopContextNode()
            aStream.SaveNode(pluginNode)
        #print '    %d plugins' % len(self.iPluginList)
            
        aStream.PopContextNode()
        aStream.SaveNode(package)
        
    def Link(self):
        # check for duplicate plugin names
        numPlugins = len(self.iPluginList)
        for i in range(numPlugins):
            plugin = self.iPluginList[i]
            for j in range(i+1, numPlugins):
                testPlugin = self.iPluginList[j]
                if plugin.Fullname() == testPlugin.Fullname():
                    raise DuplicatePluginName(plugin.Fullname())
                
        for plugin in self.iPluginList:
            plugin.Link()
    
    def Refresh(self):
        pass
    
    
    