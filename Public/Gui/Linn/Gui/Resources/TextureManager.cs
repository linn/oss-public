using System.Collections.Generic;
using System.Threading;

namespace Linn {
namespace Gui {
namespace Resources {
    
public abstract class TextureManager
{
    public TextureManager() {
        if(iInstance != null) {
            throw new SingletonAlreadyExists();
        }
        iInstance = this;
        iMutex = new Mutex(false);
        iPathList.Add("");      // add current directory as the last path to search
    }

    public static TextureManager Instance {
        get {
            Assert.Check(iInstance != null);
            return iInstance;
        }
    }
    
    public void AddPath(string aPath) {
        iPathList.Insert(0, aPath);
    }

    public void RemovePath(string aPath) {
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
    
    public ITexture TextureByNameOrLoad(string aFilename) {
        iMutex.WaitOne();
        foreach(ITexture texture in iTextureList) {
            if(texture.Filename == aFilename) {
                iTextureList.Remove(texture);
                iTextureList.Add(texture);
                iMutex.ReleaseMutex();
                return texture;
            }
        }
        ITexture newTexture = null;
        if(aFilename.Substring(0, 4) == "http") {
            newTexture = CreateUrlTexture(aFilename);
        } else {
            newTexture = CreateFileTexture(aFilename);
        }
        newTexture.Refresh();
        iTextureList.Add(newTexture);
        iMutex.ReleaseMutex();
        return newTexture;
    }

    public ITexture TextureByName(string aFilename) {
        iMutex.WaitOne();
        foreach(ITexture texture in iTextureList) {
            if(texture.Filename == aFilename) {
                iTextureList.Remove(texture);
                iTextureList.Add(texture);
                iMutex.ReleaseMutex();
                return texture;
            }
        }
        iMutex.ReleaseMutex();
        return null;
    }

    public void AddTexture(ITexture aTexture) {
        iMutex.WaitOne();
        foreach(ITexture texture in iTextureList) {
            if(texture.Filename == aTexture.Filename) {
                iMutex.ReleaseMutex();
                Assert.Check(false);
            }
        }
        iTextureList.Add(aTexture);
        iMutex.ReleaseMutex();
    }

    public void RemoveTexture(ITexture aTexture) {
        iMutex.WaitOne();
        iTextureList.Remove(aTexture);
        iMutex.ReleaseMutex();
    }
    
    protected abstract ITexture CreateFileTexture(string aFilename);
    protected abstract ITexture CreateUrlTexture(string aUrl);
    public abstract ITexture NotFoundTexture { get ; }
    
    public ITexture[] TextureList {
        get {
            iMutex.WaitOne();
            ITexture[] list = iTextureList.ToArray();
            iMutex.ReleaseMutex();
            return list;
        }
    }
    
    public void FlushCache() {
        iMutex.WaitOne();
        iTextureList.Clear();
        iMutex.ReleaseMutex();
    }
    
    public void Refresh() {
        iMutex.WaitOne();
        foreach(ITexture texture in iTextureList) {
            texture.Refresh();
        }
        iMutex.ReleaseMutex();
    }
    
    protected static TextureManager iInstance = null;
    protected List<ITexture> iTextureList = new List<ITexture>();
    protected List<string> iPathList = new List<string>();
    protected Mutex iMutex = null;
}

public sealed class TextureManagerGdi : TextureManager
{
    protected override ITexture CreateFileTexture(string aFilename) {
        return new TextureGdiFile(aFilename);
    }
    
    protected override ITexture CreateUrlTexture(string aUrl) {
        return new TextureGdiWeb(aUrl);
    }
    
    public override ITexture NotFoundTexture {
        get {
            if(iNotFoundTexture == null) {
                iNotFoundTexture = new TextureGdiFile("NotFound.png");
                iNotFoundTexture.Refresh();
                Assert.Check(iNotFoundTexture.Surface != null);
            }
            return iNotFoundTexture;
        }
    }
    
    private TextureGdiFile iNotFoundTexture = null;
}

public sealed class TextureManagerNull : TextureManager
{   
    protected override ITexture CreateFileTexture(string aFilename) {
        return new TextureNull(aFilename);
    }
    
    protected override ITexture CreateUrlTexture(string aUrl) {
        return new TextureNull(aUrl);
    }
    
    public override ITexture NotFoundTexture {
        get {
            return new TextureNull("");
        }
    }
}
    
} // Resources
} // Gui
} // Linn
