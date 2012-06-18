using System.Collections;
using System.Collections.Generic;
using Linn.Gui;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System.Xml;
using System.Xml.XPath;
using System.IO;

namespace Linn {
namespace Gui {
namespace Resources {

public sealed class InvalidVersion : System.Exception {
    public InvalidVersion(uint aExpected, uint aActual) : base("Invalid package version: Expected " + aExpected + " got " + aActual) { }
    public InvalidVersion(uint aExpected, uint aActual, System.Exception aExc) : base("Invalid package version: Expected " + aExpected + " got " + aActual, aExc) {}
}

internal sealed class DuplicatePlugin : System.Exception {}
public sealed class InvalidFilename : System.Exception {}
public sealed class RootAlreadyDefined : System.Exception {}
public sealed class NoRootDefined : System.Exception {}
public sealed class NamespaceNotFound : System.Exception {}
    
public sealed class Package : ISerialiseObject
{
    public Package() {
    }
    
    public Package(string aFilename) {
        Trace.WriteLine(Trace.kGui, "Package.Package: Loading package: " + aFilename);
        bool searching = true;
        int pathIndex = 0;
        while(searching) {
            try {
                string filename = Path.Combine(PackageManager.Instance.PathList[pathIndex], aFilename);
                Trace.WriteLine(Trace.kGui, "Package.Package: Searching " + Path.GetDirectoryName(filename) + "...");
                Load(filename);
                iFilename = filename;
                searching = false;
            } catch(System.IO.IOException e) {
                ++pathIndex;
                if(pathIndex == PackageManager.Instance.PathList.Count) {
                    throw e;
                }
            }
        }
    }
    
    public Package(string aDirectory, string aFilename) {
        Trace.WriteLine(Trace.kGui, "Package.Package: Loading package: " + aDirectory + aFilename);
        string filename = Path.Combine(aDirectory, aFilename);
        Load(filename);
        iFilename = filename;
    }
    
    private void Load(string aPathAndFilename) {
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(aPathAndFilename);
        XmlNodeList list = xmlDoc.DocumentElement.SelectNodes("/Package");
        if(list != null) {
            Assert.Check(list.Count == 1);
            Load(list[0]);
        }
    }
    
    public void Load(XmlNode aXmlNode) {
        foreach(XmlAttribute attrib in aXmlNode.Attributes) {
            if(attrib.Name == "type") {
                iType = attrib.Value;
            }
        }
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("Version");
        if(list != null) {
            Assert.Check(list.Count == 1);
            if(kVersion != uint.Parse(list[0].FirstChild.Value)) {
                throw new InvalidVersion(kVersion, uint.Parse(list[0].FirstChild.Value));
            }
        }
        
        list = aXmlNode.SelectNodes("Namespace");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iNamespace = list[0].FirstChild.Value;
        }
        
        list = aXmlNode.SelectNodes("Package");
        if(list != null) {
            foreach(XmlNode node in list) {
                iPackageList.Add(node.FirstChild.Value);
            }
        }
        
        list = aXmlNode.SelectNodes("Plugin");
        if(list != null) {
            foreach(XmlNode node in list) {
                Plugin plugin = PluginFactory.Instance.Create(node);
                plugin.Namespace = iNamespace;
                iPluginList.Add(plugin);
            }
        }
    }
    
    public void Link() {
        Trace.WriteLine(Trace.kGui, "Package: " + Filename + ": linking...");
        for(int i = 0; i < iPluginList.Count; ++i) {
            Plugin p1 = PluginItem(i);
            for(int j = i + 1; j < iPluginList.Count; ++j) {
                Plugin p2 = PluginItem(j);
                if(p1.Fullname == p2.Fullname) {
                    throw new DuplicatePlugin();
                }
            }
        }
        
        foreach(Plugin plugin in iPluginList) {
            plugin.Link();
            NodeHit node = plugin as NodeHit;
            if(node != null) {
                if(node.IsRoot == true) {
                    Trace.WriteLine(Trace.kGui, "Setting " + node.Fullname + " as Root for " + iFilename);
                    Root = node;
                }
            }
        }
        if(Root == null) {
            throw new NoRootDefined();
        }
        Trace.WriteLine(Trace.kGui, "Package: " + Filename + ": linked. root = " + Root.Fullname);
    }
    
    public void Save(XmlTextWriter aWriter) {
        aWriter.WriteStartDocument();
        
        aWriter.WriteStartElement("Package");
        aWriter.WriteAttributeString("type", "Layout");
        aWriter.WriteStartElement("Version");
        aWriter.WriteString(kVersion.ToString());
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("Namespace");
        aWriter.WriteString(iNamespace);
        aWriter.WriteEndElement();
        foreach(string package in iPackageList) {
            aWriter.WriteStartElement("Package");
            //aWriter.WriteString(package.Filename);
            aWriter.WriteString(package);
            aWriter.WriteEndElement();
        }
        foreach(Plugin plugin in iPluginList) {
            aWriter.WriteStartElement("Plugin");
            string type = plugin.GetType().ToString();
            aWriter.WriteAttributeString("class", type.Substring(type.LastIndexOf('.') + 1));
            plugin.Save(aWriter);
            aWriter.WriteEndElement();
        }
        aWriter.WriteEndElement();
        
        aWriter.WriteEndDocument();
    }
    
    public void Save() {
        if(iFilename == "") {
            throw new InvalidFilename();
        }
        Save(iFilename);
    }
    
    public void Save(string aFilename) {
        UpdatePackageDependecies();
        
        Trace.WriteLine(Trace.kGui, "Save with filename=" + aFilename);
        XmlTextWriter xmlWriter = new XmlTextWriter(aFilename, null);
        xmlWriter.Formatting = Formatting.Indented;
        xmlWriter.Indentation = 2;
        Save(xmlWriter);
        xmlWriter.Close();
        iFilename = aFilename;
        /*if(PackageManager.Instance.RootDirectory != "") {
            iFilename = iFilename.Replace(PackageManager.Instance.RootDirectory, "");
            Trace.WriteLine(Trace.kGui, "package filename=" + iFilename + " (" + PackageManager.Instance.RootDirectory + ")");
        }*/
    }
    
    public string Filename {
        get {
            return iFilename;
        }
        set {
            iFilename = value;
        }
    }
    
    public string Namespace {
        get {
            return iNamespace;
        }
        set {
            iNamespace = value;
            foreach(Plugin plugin in iPluginList) {
                plugin.Namespace = iNamespace;
            }
        }
    }
    
    public string Type {
        get {
            return iType;
        }
        set {
            iType = value;
        }
    }
    
    public NodeHit Root {
        get {
            return iRoot;
        }
        set {
            if(iRoot != null) {
                throw new RootAlreadyDefined();
            }
            iRoot = value;
        }
    }
    
    public List<Plugin> PluginList {
        get {
            return iPluginList;
        }
    }
    
    public Plugin PluginItem(int aIndex) {
        return (Plugin)iPluginList[aIndex];
    }
    
    public List<string> PackageList {
        get {
            return iPackageList;
        }
    }
    
    public void FixUpCrossPackageNodes() {
        foreach(Plugin plugin in iPluginList) {
            Node node = plugin as Node;
            if(node != null) {
                if(node.Parent != null) {
                    if(node.Parent.Namespace != Namespace) {
                        node.Active = node.Parent.Active;
                    }
                }
            }
        }
    }
    
    public Plugin PluginByName(string aName) {
        int i = aName.IndexOf('.');
        if(i < 0) {
            throw new NamespaceNotFound();
        }
        string ns = aName.Substring(0, i);
        string n = aName.Substring(i+1);
        if(ns == iNamespace) {
            foreach(Plugin plugin in iPluginList) {
                if(plugin.Name == n) {
                    return plugin;
                }
            }
        }
        //Trace.WriteLine(Trace.kGui, aName + " not found in " + iFilename);
        return null;
    }
    
    public void AddNode(Node aNode) {
        Trace.WriteLine(Trace.kGui, "Package: adding node " + aNode.Fullname + "...");
        AddPlugin(aNode);
        for(int i = 0; i < aNode.Children.Count; ++i) {
            AddNode(aNode.Child(i));
        }
        Trace.WriteLine(Trace.kGui, "Package: node added.");
    }
    
    public void DeleteNode(Node aNode) {
        Trace.WriteLine(Trace.kGui, "Package: deleting node " + aNode.Fullname + "...");
        if(aNode.Parent != null) {
            aNode.Parent.RemoveChild(aNode);
        }
        DeletePlugin(aNode);
        while(aNode.Children.Count > 0) {
            DeleteNode(aNode.Child(0));
        }
        Trace.WriteLine(Trace.kGui, "Package: node deleted.");
    }
    
    public void AddPlugin(Plugin aPlugin) {
        AddPlugin(aPlugin, aPlugin);
    }
    
    private void AddPlugin(Plugin aPlugin, Plugin aStart) {
        aPlugin.Namespace = iNamespace;
        aPlugin.Name = aPlugin.Name;
        iPluginList.Add(aPlugin);
        Trace.WriteLine(Trace.kGui, "Package: added " + aPlugin.Fullname);
        Plugin plugin = aPlugin.NextPlugin;
        if(plugin != null && aStart != plugin) {
            AddPlugin(plugin, aStart);
        }
    }
    
    public void DeletePlugin(Plugin aPlugin) {
        DeletePlugin(aPlugin, aPlugin);
    }
    
    private void DeletePlugin(Plugin aPlugin, Plugin aStart) {
        iPluginList.Remove(aPlugin);
        Trace.WriteLine(Trace.kGui, "Package: deleted " + aPlugin.Fullname);
        Plugin plugin = aPlugin.NextPlugin;
        if(plugin != null && aStart != plugin) {
            DeletePlugin(plugin, aStart);
        }
    }
    
    private void UpdatePackageDependecies() {
        iPackageList.Clear();
        
        List<string> depsList = new List<string>();
        foreach(Plugin p in iPluginList) {
            List<string> deps = p.PackageDependencies();
            foreach(string s in deps) {
                if(!depsList.Contains(s)) {
                    Trace.WriteLine(Trace.kGui, "Adding " + s);
                    depsList.Add(s);
                }
            }
            Plugin n = p;
            while(n != null && n != p) {
                if(n.Namespace != Namespace) {
                    deps = p.PackageDependencies();
                    foreach(string s in deps) {
                        if(!depsList.Contains(s)) {
                            Trace.WriteLine(Trace.kGui, "Adding " + s);
                            depsList.Add(s);
                        }
                    }
                    n = n.NextPlugin;
                }
            }
        }
        
        foreach(string s in depsList) {
            try {
                Package p = PackageManager.Instance.PackageByNamespace(s);
                if(p.Namespace != Namespace) {
                    Assert.Check(p.Filename != "");
                    iPackageList.Add(Path.GetFileName(p.Filename));
                }
            } catch(PackageNotFound) {
                System.Console.WriteLine("WARNING: Could not find package for namespace {0}", s);
            }
        }
    }
    
    private const uint kVersion = 8;
    private string iFilename = "";
    private string iNamespace = "None";
    private string iType = "Unknown";
    private List<Plugin> iPluginList = new List<Plugin>();
    private List<string> iPackageList = new List<string>();
    private NodeHit iRoot = null;
}

} // Resources
} // Gui
} // Linn
