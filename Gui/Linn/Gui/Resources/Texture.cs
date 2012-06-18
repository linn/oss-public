using Linn;
using Linn.Gui;
using System.Drawing;
using System.IO;
using System.Net;
using System.Drawing.Imaging;
using System;
using System.Threading;

namespace Linn {
namespace Gui {
namespace Resources {
    
public interface ITexture : IDisposable
{
    int Width { get ; }
    int Height { get ; }
    string Filename { get ; }
    bool Loaded();
    void Refresh();
}

public class ReferenceTexture : ReferenceObject<ITexture>
{
    public ReferenceTexture() : base("", null) {
    }
    
    public ReferenceTexture(string aName) : base(aName, null) {
    }
    
    public ReferenceTexture(ITexture aTexture) {
        if(aTexture != null) {
            iName = aTexture.Filename;
            iObject = aTexture;
        }
    }
    
    public override void Link() {
        if(iName != "") {
            iObject = (ITexture)TextureManager.Instance.TextureByNameOrLoad(iName);
        } else {
            iObject = null;
        }
    }
}

public abstract class TextureGdi : ITexture
{
    public TextureGdi(string aName) {
        iName = aName;
    }
    
    public virtual void Dispose() {
        if(iImage != null) {
            iImage.Dispose();
        }
    }
       
    public string Filename {
        get {
            return iName;
        }
    }
       
    public int Width {
        get {
            return iWidth;
        }
    }
       
    public int Height {
        get {
            return iHeight;
        }
    }
       
    public Image Surface {
        get {
            return iImage;
        }
    }   
    
    public bool Loaded() {
        return iImage != null;
    }
      
    public abstract void Refresh();
       
    protected string iName;
    protected Image iImage = null;
    protected int iWidth = 0;
    protected int iHeight = 0;
}

public class TextureGdiFile : TextureGdi
{
    public TextureGdiFile(string aFilename) : base(aFilename) {
        //Refresh();
    }
    
    public override void Refresh() {
        Trace.WriteLine(Trace.kGui, "TextureGdiFile.Refresh: Loading Texture: " + iName);
        bool searching = true;
        int pathIndex = 0;
        while(searching) {
            try {
                string filename = System.IO.Path.Combine(TextureManager.Instance.PathList[pathIndex], iName);
                Trace.WriteLine(Trace.kGui, "TextureGdiFile.Refresh: Searching " + System.IO.Path.GetDirectoryName(filename) + "...");
                iImage = new Bitmap(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                iWidth = iImage.Width;
                iHeight = iImage.Height;
                searching = false;
                Trace.WriteLine(Trace.kGui, "TextureGdiFile.Refresh: Loaded: " + iName);
            } catch(System.IO.IOException) {
                ++pathIndex;
                if(pathIndex == TextureManager.Instance.PathList.Count) {
                    Trace.WriteLine(Trace.kGui, "TextureGdiFile.Refresh: Could not find " + iName);
                    searching = false;
                    iImage = null;
                }
            } catch(System.ArgumentException) {
                System.Console.WriteLine("TextureGdiFile.Refresh: Invalid filename: " + iName);
                searching = false;
                iImage = null;
            }
        }
    }
}

public class TextureGdiWeb : TextureGdi
{
    internal class HttpWebRequestState
    {
        public HttpWebRequestState(HttpWebRequest aHttpWebRequest) {
            iHttpWebRequest = aHttpWebRequest;
        }
           
        public HttpWebRequest HttpWebRequest {
            get {
                return iHttpWebRequest;
            }
        }
           
        private HttpWebRequest iHttpWebRequest;
    }
    
    public override void Dispose() {
        iMutex.WaitOne();
        
        iDisposed = true;
        base.Dispose();
        
        iMutex.ReleaseMutex();
    }
    
    public TextureGdiWeb(string aUrl) : base(aUrl) {
        Trace.WriteLine(Trace.kGui, "TextureGdiWeb.TextureGdiWeb: Loading Web Texture: " + iName);
        //Refresh();
    }
    
    public event EventHandler<EventArgs> EventTextureRefresh;
    
    public override void Refresh() {
        DownloadImage();
        if(iImage != null) {
            Trace.WriteLine(Trace.kGui, "TextureGdiWeb.Refresh: Loaded: " + iName);
        } else {
            Trace.WriteLine(Trace.kGui, "TextureGdiWeb.Refresh: Could not find " + iName);
        }
    }
    
    private void DownloadImage() {
        HttpWebRequest wreq = null;
        try {
            wreq = (HttpWebRequest)WebRequest.Create(iName);
            wreq.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
            wreq.AllowWriteStreamBuffering = true;
            WebRequestPool.QueueJob(new JobGetResponse(GetResponseCallback, new HttpWebRequestState(wreq)));
        } catch(System.Net.WebException e) {
            Trace.WriteLine(Trace.kGui, "TextureGdiWeb.DownloadImage: " + iName +"\n" + e);
            iImage = null;
        }
    }
    
    private void GetResponseCallback(object aResult) {
        iMutex.WaitOne();
        
        if(iImage != null) {
            iImage.Dispose();
            iImage = null;
        }
        
        if(!iDisposed) {
	        HttpWebResponse wresp = null;
	        Stream stream = null;
	        
	        try {
	            HttpWebRequestState wreqResult = aResult as HttpWebRequestState;
	            HttpWebRequest wreq = wreqResult.HttpWebRequest;
	            wresp = (HttpWebResponse)wreq.GetResponse();
	               
	            if((stream = wresp.GetResponseStream()) != null) {
	                Bitmap bitmap = new Bitmap(stream);
	                if(iWidth > 0 || iHeight > 0) {
	                    iImage = new Bitmap(iWidth, iHeight);
                        Graphics g = Graphics.FromImage(iImage);
                        g.DrawImage(bitmap, new Rectangle(0, 0, iWidth, iHeight), new Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
                        g.Dispose();
                        bitmap.Dispose();
	                } else {
	                    iImage = bitmap;
		                iWidth = iImage.Width;
		                iHeight = iImage.Height;
	                }
	                Assert.Check(iImage != null);
	            }
	        } catch(System.Net.WebException e) {
	            Trace.WriteLine(Trace.kGui, "TextureGdiWeb.DownloadImage: " + iName + "\n" + e);
	            iImage = null;
	        } catch(System.ArgumentException e) {
	            System.Console.WriteLine("TextureGdiWeb.DownloadImage: " + iName + "\n" + e);
	            iImage = null;
	        } catch (System.ObjectDisposedException e) {
	            Trace.WriteLine(Trace.kInformation, "TextureGdiWeb.DownloadImage: " + iName + "\n" + e);
	            iImage = null;
	        } finally {
	            if(stream != null) {
	                stream.Close();
	            }
	            if(wresp != null) {
	                wresp.Close();
	            }
	        }
	        
	        iMutex.ReleaseMutex();
	        
	        if(EventTextureRefresh != null) {
	            EventTextureRefresh(this, EventArgs.Empty);
	        }
	        
	        iMutex.WaitOne();
	    }
	    
	    iMutex.ReleaseMutex();
    }
    
    private bool iDisposed = false;
    private Mutex iMutex = new Mutex(false);
}


public class TextureNull : ITexture
{
    public TextureNull(string aFilename) {
        iFilename = aFilename;
    }
    
    public void Dispose() {}
    
    public int Width {
        get {
            return 0;
        }
    }
    
    public int Height {
        get {
            return 0;
        }
    }
    
    public string Filename {
        get {
            return iFilename;
        }
    }
    
    public bool Loaded() {
        return true;
    }
    
    public void Refresh() {}
    
    private string iFilename;
}

} // Resources
} // Gui
} // Linn

