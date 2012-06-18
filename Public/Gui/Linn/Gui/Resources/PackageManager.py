import os
import sys
import Linn.Gui.SceneGraph.Node
import Package
import Plugin

gPackageManager = None

def GetPackageManager():
    global gPackageManager
    if not gPackageManager:
        gPackageManager = PackageManager()
    return gPackageManager


class CorruptFile(Exception):
    def __init__(self, aFilename, aMsg):
        self.iFilename = aFilename
        self.iMsg = aMsg
    def __str__(self):
        return 'Error loading %s: %s' % (self.iFilename, self.iMsg)
    
class NamespaceInUse(Exception):
    def __init__(self, aNamespace):
        self.iNamespace = aNamespace
    def __str__(self):
        return 'Namespace %s already present (through alternate package)' % self.iNamespace
    
class PackageNotFound(Exception):
    def __init__(self, aPackage):
        self.iPackage = aPackage
    def __str__(self):
        return 'Package %s not found' % self.iPackage
    

class IPackageObserver:
    """An interface for objects to be notified of package loading and unloading."""
    def Loaded(self, aPackage):
        sys.stderr.write("Unimplemented pure virtual IPackageObserver.Loaded")
        sys.exit(-1)
    
    def UnLoaded(self, aPackage):
        sys.stderr.write("Unimplemented pure virtual IPackageObserver.UnLoaded")
        sys.exit(-1)


class PackageManager:
    def __init__(self):
        self.iObserverList = []
        self.iPackageList = []
        
    def AddObserver(self, aObserver):
        """Add a package observer to the list."""
        self.iObserverList.append(aObserver)

    def RemoveObserver(self, aObserver):
        """Remove a package observer from the list."""
        self.iObserverList.remove(aObserver)
    
    def IsPackageLoaded(self, aFilename):
        for package in self.iPackageList:
            if package.Filename() == aFilename:
                return True
        return False
    
    def IsNamespaceInUse(self, aPackage):
        for package in self.iPackageList:
            if package.Namespace() == aPackage.Namespace() and \
               package.Filename() != aPackage.Filename():
                return True
        return False
    
    def Load(self, aFilename):
        oldPackageList = self.iPackageList
        try:
            loadedPackage = len(self.iPackageList)
            newPackage = self.LoadPackage(aFilename)
            if len(self.iPackageList) > loadedPackage:
                self.LinkPackages(self.iPackageList[loadedPackage:])
                for package in self.iPackageList[loadedPackage:]:
                    if package.Type() == 'Layout':
                        #for plugin in package.PluginList():
                        #    if isinstance(plugin, Linn.Gui.SceneGraph.NodeHit.NodeHit) and plugin.Parent() == None:
                        #        plugin.Update()
                        #        package.SetRoot(plugin)
                        package.Root().Update()
                for package in self.iPackageList[loadedPackage:]:
                    for observer in self.iObserverList:
                        observer.Loaded(package)
            return newPackage
        except Exception, e:
            self.iPackageList = oldPackageList
            raise
    
    def LoadPackage(self, aFilename):
        #try:
        newPackage = None
        try:
            newPackage = self.PackageByName(aFilename)
        except PackageNotFound:
            newPackage = Package.Package(aFilename)
            if self.IsNamespaceInUse(newPackage):
                raise NamespaceInUse(newPackage.Namespace())
            self.iPackageList.append(newPackage)
            #self.iPackageList.append(newPackage)
            packageList = newPackage.PackageList()
            # load all referenced packages
            for package in packageList:
                if self.IsPackageLoaded(package) == False:
                    self.LoadPackage(package)
                #else:
                    #print 'Skipped package', package
        return newPackage
        #except Exception, e:
        #    raise CorruptFile(aFilename, e)
        
    def PluginByName(self, aPlugin):
        for package in self.iPackageList:
            try:
                return package.PluginByName(aPlugin)
            except Plugin.PluginNotFound, e:
                pass
        raise Plugin.PluginNotFound(aPlugin)
        
    def LinkPackages(self, aPackages):
        #print 'Linking...'
        for package in aPackages:
            package.Link()
                
    def PackageByName(self, aPackage):
        for package in self.iPackageList:
            if package.Filename() == aPackage:
                return package
        raise PackageNotFound(aPackage)
                
    def AddPackage(self, aPackage):
        self.iPackageList.append(aPackage)
        for observer in self.iObserverList:
            observer.Loaded(aPackage)
        
    #def RemovePackage(self, aPackage):
    #    if aPackage:
    #        removeList = [aPackage]
    #        baseIndex = self.iPackageList.index(aPackage)
    #        for filename in aPackage.PackageList():
    #            package = self.PackageByName(filename)
    #            index = self.iPackageList.index(package)
    #            # only remove referenced packages that were loaded by package
    #            if index > baseIndex:
    #                removeList.append(package)
    #        for package in removeList:
    #            self.iPackageList.remove(package)
                
    def PackageList(self):
        return self.iPackageList
    
    def FlushCache(self):
        oldPackageList = self.iPackageList
        self.iPackageList = []
        for package in oldPackageList:
            for observer in self.iObserverList:
                observer.UnLoaded(package)
        #print 'Package cache flushed...'
        
    def Refresh(self):
        #print 'Refreshing package cache...'
        for package in self.iPackageList[1:]:
            package.Refresh()

        
        