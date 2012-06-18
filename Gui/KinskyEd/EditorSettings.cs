using System;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;
using Linn;
using System.Drawing;

namespace Linn {
namespace Gui {
namespace Editor {
    
public class EditorSettings
{
    public EditorSettings() {
    }
    
    public EditorSettings(string aDirectory) {
        iDirectory = aDirectory;
        iTextureCache = System.IO.Path.GetFullPath(aDirectory);
        iDefaultTextureCache = System.IO.Path.GetFullPath(Path.Combine(aDirectory, "../share/Linn/Gui/Editor/Default"));
        System.Console.WriteLine(iDefaultTextureCache);
        iPackageCache = System.IO.Path.GetFullPath(aDirectory);
        Trace.WriteLine(Trace.kKinskyEd, "Settings directory = " + iDirectory);
    }
    
    public void SaveSettings() {
        StreamWriter writer = null;
        try {
            XmlSerializer serializer = new XmlSerializer(typeof(EditorSettings));
            Trace.WriteLine(Trace.kKinskyEd, "Saving settings: " + Path.Combine(iDirectory, "LinnGuiEd.config"));
            writer = new StreamWriter(Path.Combine(iDirectory, "LinnGuiEd.config"));
            serializer.Serialize(writer, this);
            Trace.WriteLine(Trace.kKinskyEd, "TextureCache: " + iTextureCache);
            Trace.WriteLine(Trace.kKinskyEd, "PackageCache: " + iPackageCache);
            Trace.WriteLine(Trace.kKinskyEd, "Editor location: " + iEditorLocation);
            Trace.WriteLine(Trace.kKinskyEd, "Editor size: " + iEditorSize);
            Trace.WriteLine(Trace.kKinskyEd, "Scenegraph location: " + iScenegraphLocation);
            Trace.WriteLine(Trace.kKinskyEd, "Scenegraph size: " + iScenegraphSize);
            Trace.WriteLine(Trace.kKinskyEd, "Scenegraph visible: " + iScenegraphVisible);
        } catch(Exception ex) {
            MessageBox.Show(ex.Message); 
        }
        finally {
            if(writer != null) {
                writer.Close();
            }
        }
    }
    
    public bool LoadSettings() {
        FileStream fileStream = null;
        bool fileExists = false;
        
        try {
            XmlSerializer serializer = new XmlSerializer(typeof(EditorSettings));
            FileInfo fi = new FileInfo(Path.Combine(iDirectory, "LinnGuiEd.config"));
            if(fi.Exists) {
                fileStream = fi.OpenRead();
                EditorSettings settings = (EditorSettings)serializer.Deserialize(fileStream);
                if(System.IO.Directory.Exists(settings.TextureCache)) {
                    iTextureCache = Path.GetFullPath(settings.TextureCache);
                }
                if(System.IO.Directory.Exists(settings.PackageCache)) {
                    iPackageCache = Path.GetFullPath(settings.PackageCache);
                }
                iEditorLocation = settings.EditorLocation;
                iEditorSize = settings.EditorSize;
                iScenegraphLocation = settings.ScenegraphLocation;
                iScenegraphSize = settings.ScenegraphSize;
                iScenegraphVisible = settings.ScenegraphVisible;
                fileExists = true;
            }
        } catch(Exception ex) {
            MessageBox.Show(ex.Message);
        } finally {
            if(fileStream != null) {
                fileStream.Close();
            }
        }
        return fileExists;
    }
    
    public string TextureCache {
        get {
            return iTextureCache;
        }
        set {
            iTextureCache = value;
        }
    }
    
    public string DefaultTextureCache {
        get {
            return iDefaultTextureCache;
        }
        set {
            iDefaultTextureCache = value;
        }
    }
    
    public string PackageCache {
        get {
            return iPackageCache;
        }
        set {
            iPackageCache = value;
        }
    }
    
    public Point EditorLocation {
        get {
            return iEditorLocation;
        }
        set {
            iEditorLocation = value;
        }
    }
    
    public Size EditorSize {
        get {
            return iEditorSize;
        }
        set {
            iEditorSize = value;
        }
    }
    
    public Point ScenegraphLocation {
        get {
            return iScenegraphLocation;
        }
        set {
            iScenegraphLocation = value;
        }
    }
    
    public Size ScenegraphSize {
        get {
            return iScenegraphSize;
        }
        set {
            iScenegraphSize = value;
        }
    }
    
    public bool ScenegraphVisible {
        get {
            return iScenegraphVisible;
        }
        set {
            iScenegraphVisible = value;
        }
    }
    
    private string iDirectory;
    private string iTextureCache;
    private string iDefaultTextureCache;
    private string iPackageCache;
    private Point iEditorLocation = new Point(-1, -1);
    private Size iEditorSize = new Size(928, 653);
    private Point iScenegraphLocation = new Point(-1, -1);
    private Size iScenegraphSize = new Size(400, 800);
    private bool iScenegraphVisible = false;
}

} // Editor
} // Gui
} // Linn
