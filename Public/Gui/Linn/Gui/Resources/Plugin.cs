using Linn;
using Linn.Gui;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using System.Collections.Generic;
using System.Xml;
using System;

namespace Linn {

internal class PluginNotFound : System.Exception
{
    public PluginNotFound(string aPlugin) : base("Could not find " + aPlugin) {
    }
}

namespace Gui {
namespace Resources {
    
public class ReferencePlugin<U> : ReferenceObject<U> where U : Linn.Gui.Resources.Plugin
{
    public ReferencePlugin() : base("", null) {
    }
    
    public ReferencePlugin(string aName) : base(aName, null) {
    }
    
    public ReferencePlugin(U aPlugin) {
        if(aPlugin != null) {
            iName = aPlugin.Fullname;
            iObject = aPlugin;
        }
    }
        
    public override void Link() {
        if(iName != "") {
            iObject = (U)PackageManager.Instance.PluginByName(iName);
        }
    }
}

public interface IPluginObserver
{
    void Update(Plugin aPlugin);
}

public class Plugin : ISerialiseObject, ICloneable
{   
    public Plugin() {
    }
    
    protected Plugin(string aName) {
        iName = aName;//UniqueName(aName);
    }
    
    public virtual Object Clone() {
        Plugin p = new Plugin();
        Clone(p);
        return p;
    }
    
    protected void Clone(Plugin aPlugin) {
        aPlugin.iObserverList.AddRange(iObserverList);
        aPlugin.iName = iName;
        aPlugin.iNamespace = iNamespace;
        aPlugin.iPlugin = iPlugin;
    }
    
    public virtual void Load(XmlNode aXmlNode) {
        XmlNodeList list;
        
        list = aXmlNode.SelectNodes("Name");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iName = list[0].FirstChild.Value;
        }
        
        list = aXmlNode.SelectNodes("Plugin");
        if(list != null) {
            Assert.Check(list.Count == 1);
            if(list[0].FirstChild != null) {
                iPlugin = new ReferencePlugin<Plugin>(list[0].FirstChild.Value);
            }
        }
    }
    
    public virtual void Link() {
        Trace.WriteLine(Trace.kGui, Fullname);
        iPlugin.Link();
    }
    
    public virtual void Save(XmlTextWriter aWriter) {
        aWriter.WriteStartElement("Name");
        aWriter.WriteString(iName);
        aWriter.WriteEndElement();
        aWriter.WriteStartElement("Plugin");
        if(NextPlugin != null) {
            aWriter.WriteString(NextPlugin.Fullname);
        }
        aWriter.WriteEndElement();
    }
    
    public void AddObserver(IPluginObserver aObserver) {
        iObserverList.Add(aObserver);
    }
    
    public void RemoveObserver(IPluginObserver aObserver) {
        iObserverList.Remove(aObserver);
    }
    
    public void ObserverUpdate() {
        //Trace.WriteLine(Trace.kGui, "ObserverUpdate for " + Fullname);
        foreach(IPluginObserver observer in iObserverList) {
            observer.Update(this);
        }
    }
    
    public string Name {
        get {
            return iName;
        }
        set {
            // check for name currently in use
            try {
                Trace.WriteLine(Trace.kGui, "Finding " + iNamespace + "." + value);
                PackageManager.Instance.PluginByName(iNamespace + "." + value);
                Trace.WriteLine(Trace.kGui, "Found " + iNamespace + "." + value);
                iName = UniqueName(value);
                ObserverUpdate();
            }
            catch(PluginNotFound) {
                if(value != iName) {
                    iName = value;
                    ObserverUpdate();
                }
            }
        }
    }
    
    public virtual string Namespace {
        get {
            return iNamespace;
        }
        set {
            iNamespace = value;
            ObserverUpdate();
        }
    }
    
    protected static string UniqueName(string aName) {
        iUniqueNum++;
        return aName + iUniqueNum;
    }
    
    public string Fullname {
        get {
            return iNamespace + "." + iName;
        }
    }
    
    public Plugin NextPlugin {
        get {
            return iPlugin.Object;
        }
        set {
            iPlugin = new ReferencePlugin<Plugin>(value);
        }
    }
    
    public virtual List<string> PackageDependencies() {
        return new List<string>();
    }
    
    public virtual void Click(Vector3d aVector) {
        if(iProcessClick) {
            iProcessClick = false;
            if(NextPlugin != null) {
                NextPlugin.Click(aVector);
            }
            iProcessClick = true;
        }
    }
    
    public virtual void DoubleClick(Vector3d aVector) {
        if(iProcessDoubleClick) {
            iProcessDoubleClick = false;
            if(NextPlugin != null) {
                NextPlugin.DoubleClick(aVector);
            }
            iProcessDoubleClick = true;
        }
    }
    
    public virtual void Hit(Vector3d aVector) {
        if(iProcessHit) {
            iProcessHit = false;
            MsgHit msg = new MsgHit(this, aVector);
            SendMessage(msg);
            if(NextPlugin != null) {
                NextPlugin.Hit(aVector);
            }
            iProcessHit = true;
        }
    }
    
    public virtual void Motion(Vector3d aVector) {
        if(iProcessMotion) {
            iProcessMotion = false;
            if(NextPlugin != null) {
                NextPlugin.Motion(aVector);
            }
            iProcessMotion = true;
        }
    }
    
    public virtual void Wheel(float aDirection) {
        if(iProcessWheel) {
            iProcessWheel = false;
            if(NextPlugin != null) {
                NextPlugin.Wheel(aDirection);
            }
            iProcessWheel = true;
        }
    }
    
    public virtual void UnHit() {
        if(iProcessUnHit) {
            iProcessUnHit = false;
            if(NextPlugin != null) {
                NextPlugin.UnHit();
            }
            iProcessUnHit = true;
        }
    }
    
    public virtual void Texture(ReferenceTexture aTexture) {
        if(iProcessTexture) {
            iProcessTexture = false;
            if(NextPlugin != null) {
                NextPlugin.Texture(aTexture);
            }
            iProcessTexture = true;
        }
    }
    
    public virtual void Vector3d(Vector3d aVector) {
        if(iProcessVector3d) {
            iProcessVector3d = false;
            if(NextPlugin != null) {
                NextPlugin.Vector3d(aVector);
            }
            iProcessVector3d = true;
        }
    }
    
//  public virtual void Matrix(Matrix aMatrix) {
//  }

    public virtual bool ProcessMessage(Message aMessage) {
        if(NextPlugin != null) {
            return NextPlugin.ProcessMessage(aMessage);
        }
        return false;
    }
    
    public virtual void SendMessage(Message aMessage) {
        if(NextPlugin != null) {
            NextPlugin.SendMessage(aMessage);
        }
    }
    
    private static int iUniqueNum = 0;
    private List<IPluginObserver> iObserverList = new List<IPluginObserver>();
    private string iName = "Plugin";
    private string iNamespace = "None";
    private ReferencePlugin<Plugin> iPlugin = new ReferencePlugin<Plugin>();
    private bool iProcessClick = true;
    private bool iProcessDoubleClick = true;
    private bool iProcessHit = true;
    private bool iProcessMotion = true;
    private bool iProcessWheel = true;
    private bool iProcessUnHit = true;
    private bool iProcessTexture = true;
    private bool iProcessVector3d = true;
}

} // Resources
} // Gui
} // Linn
