using System.Collections.Generic;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using System.IO;

namespace Linn {
namespace Gui {
namespace Resources {
    
sealed class CorruptFile : System.Exception
{
}
    
sealed class NamespaceInUse : System.Exception
{
    public NamespaceInUse(string aNamespace) : base("Namespace " + aNamespace + " is in use") {
    }
}

sealed class PackageNotFound : System.Exception
{
    public PackageNotFound(string aPackage) : base("Could not find " + aPackage) {
    }
}

public interface IPackageObserver
{
    void Loaded(Package aPackage);
    void UnLoaded(Package aPackage);
}
    
public class PackageManager
{
    public static PackageManager Instance {
        get {
            if(iInstance == null) {
                iInstance = new PackageManager();
            }
            return iInstance;
        }
    }
    
    private PackageManager() {
        iPathList.Add("");      // add current directory as the last path to search
    }
    
    public void AddObserver(IPackageObserver aObserver) {
        iObserverList.Add(aObserver);
    }
    
    public void RemoveObserver(IPackageObserver aObserver) {
        iObserverList.Remove(aObserver);
    }
    
    public bool PackageLoaded(string aFilename) {
        foreach(Package package in iPackageList) {
            if(package.Filename == aFilename) {
                return true;
            }
        }
        return false;
    }
    
    public Plugin PluginByName(string aName) {
        Plugin result;
        foreach(Package package in iPackageList) {
            //System.Console.WriteLine("Searching: " + package.Filename + " for " + aName);
            result = package.PluginByName(aName);
            if(result != null) {
                return result;
            }
        }
        Trace.WriteLine(Trace.kGui, "PackageManager.PluginByName: Could not find plugin " + aName);
        throw new PluginNotFound(aName);
    }
    
    public Package Load(string aFilename) {
        return Import(aFilename, true);
    }
    
    public Package Import(string aFilename) {
        return Import(aFilename, false);
    }
    
    private Package Import(string aFilename, bool aFlushCache) {
        List<Package> oldPackageList = iPackageList;
        if(aFlushCache) {
            FlushCache();
        }
        try {
            int numLoaded = iPackageList.Count;
            Package newPackage = LoadPackage(aFilename);
            for(int i = numLoaded; i < iPackageList.Count; ++i) {
                Package package = (Package)iPackageList[i];
                package.Link();
            }
            bool active = newPackage.Root.Active;
            for(int i = numLoaded; i < iPackageList.Count; ++i) {
                Package package = (Package)iPackageList[i];
                package.FixUpCrossPackageNodes();
                if(package.Type == "Layout") {
                    package.Root.Update(true, true);
                }
                foreach(IPackageObserver observer in iObserverList) {
                    observer.Loaded(package);
                }
            }
            // NOTE: this is required from loading other packages that could have
            //       referenced this node as a child node.
            newPackage.Root.Parent = null;
            newPackage.Root.Active = active;
            newPackage.Root.Update(true, true);
            return newPackage;
        } catch(System.Exception e) {
            iPackageList = oldPackageList;
            foreach(Package package in iPackageList) {
                foreach(IPackageObserver observer in iObserverList) {
                    observer.Loaded(package);
                }
            }
            throw new System.Exception(e.Message, e);
        }
    }
    
    private Package LoadPackage(string aFilename) {
        Package newPackage = null;
        try {
            newPackage = PackageByFilename(aFilename);
        }
        catch(PackageNotFound) {
            newPackage = new Package(aFilename);
            //NamespaceInUse();
            iPackageList.Add(newPackage);
            foreach(string package in newPackage.PackageList) {
                try {
                    PackageByFilename(package);
                }
                catch(PackageNotFound) {
                    LoadPackage(package);
                }
            }
        }
        return newPackage;
    }
    
    public Package PackageByFilename(string aFilename) {
        foreach(Package package in iPackageList) {
            if(Path.GetFileName(package.Filename) == Path.GetFileName(aFilename)) {
                return package;
            }
        }
        throw new PackageNotFound(aFilename);
    }
    
    public Package PackageByNamespace(string aNamespace) {
        foreach(Package package in iPackageList) {
            if(package.Namespace == aNamespace) {
                return package;
            }
        }
        throw new PackageNotFound(aNamespace);
    }
    
    public void AddPath(string aPath) {
        iPathList.Insert(0, aPath);
    }

    public void RemovePath(string aPath){
        iPathList.Remove(aPath);
    }
    
    public List<string> PathList {
        get {
            return iPathList;
        }
    }
    
    public string RootDirectory {
        get {
            return iPathList[0];
        }
    }
    
    public List<Package> Packages {
        get {
            return iPackageList;
        }
    }
    
    public void FlushCache() {
        foreach(Package package in iPackageList) {
            foreach(IPackageObserver observer in iObserverList) {
                observer.UnLoaded(package);
            }
        }
        iPackageList.Clear();
    }
    
    static private PackageManager iInstance = null;
    private List<IPackageObserver> iObserverList = new List<IPackageObserver>();
    private List<Package> iPackageList = new List<Package>();
    private List<string> iPathList = new List<string>();
}

} // Resources
} // Gui
} // Linn
