using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using Linn.Gui;
using System;
using System.Drawing;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public abstract class ViewStatus : IDisposable
{
    public ViewStatus(Node aRoot, Component aModel) {
        iRoot = aRoot;
        iModelSource = aModel;
        
        VisitorSearch search = new VisitorSearch("StatusBar.TimeSlider");
        iSlider = (NodeSlider)search.Search(aRoot);
        Assert.Check(iSlider != null);
        
        search = new VisitorSearch("StatusBar.Album");
        iAlbum = (NodeText)search.Search(aRoot);
        Assert.Check(iAlbum != null);

        search = new VisitorSearch("StatusBar.Track");
        iTrack = (NodeText)search.Search(aRoot);
        Assert.Check(iTrack != null);

        search = new VisitorSearch("StatusBar.TimeText");
        iTime = (NodeText)search.Search(aRoot);
        Assert.Check(iTime != null);
        
        search = new VisitorSearch("StatusBar.CoverArt");
        iDefaultCoverArt = (NodePolygon)search.Search(aRoot);
        Assert.Check(iDefaultCoverArt != null);
        
        iFadeTimer = new Timer();
        iFadeTimer.Interval = 10;
        iFadeTimer.Elapsed += FadeText;
        iFadeTimer.AutoReset = false;
        
        Assert.Check(iAlbum.Colour.A == 255);
        Assert.Check(iTrack.Colour.A == 255);
        Assert.Check(iTime.Colour.A == 255);
        
        //iModelSource.EEventStatusOk += EventStatusOk;
    }
    
    public virtual void Dispose() {
        //iModelSource.EEventStatusOk -= EventStatusOk;
        if(iFadeTimer != null) {
            iFadeTimer.Stop();
            iFadeTimer = null;
        }
        iAlbum.Colour = new Colour(255, iAlbum.Colour.R, iAlbum.Colour.G, iAlbum.Colour.B);
        iTrack.Colour = new Colour(255, iTrack.Colour.R, iTrack.Colour.G, iTrack.Colour.B);
        iTime.Colour = new Colour(255, iTime.Colour.R, iTime.Colour.G, iTime.Colour.B);
    }
    
    public Component Model {
        get {
            return iModelSource;
        }
    }
    
    protected void EventStatusOk() {
        iPrevTime = DateTime.Now.Ticks;
        //iFadeOut = !iModelSource.StatusOk;
        iFadeTimer.Start();
    }
    
    private void FadeText(object aSender, EventArgs aArgs) {
        double t = TimeSpan.FromTicks(DateTime.Now.Ticks - iPrevTime).TotalSeconds;
        double delta = 255.0f * t * kFadeTime;
        //Trace.WriteLine(Trace.kLinnGui, "ViewMediaRenderer.FadeText: t=" + t + ", delta=" + delta);

        if(!iFadeOut) {
            if(iAlpha + delta > 255) {
                iAlpha = 255;
                iSlider.Active = true;
                iDefaultCoverArt.Active = true;
            } else {
                iAlpha += delta;
                iFadeTimer.Start();
            }
        } else {
            if(iAlpha - delta < 0) {
                iAlpha = 0;
                iSlider.Active = false;
                iDefaultCoverArt.Active = false;
            } else {
                iAlpha -= delta;
                iFadeTimer.Start();
            }
        }
        
        iAlbum.Colour = new Colour((int)iAlpha, iAlbum.Colour.R, iAlbum.Colour.G, iAlbum.Colour.B);
        iTrack.Colour = new Colour((int)iAlpha, iTrack.Colour.R, iTrack.Colour.G, iTrack.Colour.B);
        iTime.Colour = new Colour((int)iAlpha, iTime.Colour.R, iTime.Colour.G, iTime.Colour.B);
        
        iPrevTime = DateTime.Now.Ticks;
        Renderer.Instance.Render();
    }
    
    protected Node iRoot = null;
    protected NodeSlider iSlider = null;
    protected NodeText iAlbum = null;
    protected NodeText iTrack = null;
    protected NodeText iTime = null;
    protected NodePolygon iDefaultCoverArt = null;
    
    private Component iModelSource = null;
    private const float kFadeTime = 2.0f;
    private Timer iFadeTimer = null;
    private double iAlpha = 255;
    private bool iFadeOut = false;
    private long iPrevTime = 0;
}

} // Kinsky
} // Linn
