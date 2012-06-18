using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System.Drawing;

namespace Linn {
namespace Gui {
    
public interface IRenderer
{
    RenderStats Stats { get; }
    void Lock();
    void Unlock();
    void BeginFrame();
    void EndFrame();
    void Render();
    void Render(bool aForce);
    void Render(Rectangle aRect);
    void Render(Rectangle aRect, bool aForce);
    void DrawPolygon(NodePolygon aPolygon);
    void DrawText(NodeText aText);
}

public class RenderStats
{
    public float MinimumMs {
        get {
            return iMinMs;
        }
    }
    
    public float AverageMs {
        get {
            if(iNumFrames > 0 && iTotalMs > 0) {
                return iTotalMs / (iNumFrames - 1);
            }
            return 0;
        }
    }
    
    public float MaximumMs {
        get {
            return iMaxMs;
        }
    }
    
    public float TotalMs {
        get {
            return iTotalMs;
        }
    }
    
    public int Frames {
        get {
            return iNumFrames - 1;
        }
    }
    
    public void UpdateTotalMs(float aValue) {
        if(iNumFrames > 0) {
            if(iNumFrames == 1) {
                iMinMs = aValue;
                iMaxMs = aValue;
            } else {
                if(aValue < iMinMs) {
                    iMinMs = aValue;
                }
                if(aValue > iMaxMs) {
                    iMaxMs = aValue;
                }
            }
            iTotalMs += aValue;
        }
        iNumFrames++;
    }
    
    public float iMinMs = 0;
    public float iMaxMs = 0;
    public float iTotalMs = 0;
    public int iNumFrames = 0;
}

public abstract class Renderer : IRenderer
{
    public Renderer() {
        Trace.WriteLine(Trace.kGui, ">Renderer");
        if(iInstance != null) {
            throw new SingletonAlreadyExists();
        }
        new PluginFactory();
        new MessageFactory();
        iInstance = this;
    }
    
    public static IRenderer Instance {
        get {
            Assert.Check(iInstance != null);
            return iInstance;
        }
    }
    
    public abstract RenderStats Stats {
        get ;
    }
    
    public abstract void Lock();
    public abstract void Unlock();
    public abstract void BeginFrame();
    public abstract void EndFrame();
    public abstract void Render();
    public abstract void Render(bool aForce);
    public abstract void Render(Rectangle aRect);
    public abstract void Render(Rectangle aRect, bool aForce);
    public abstract void DrawPolygon(NodePolygon aPolygon);
    public abstract void DrawText(NodeText aText);
    
    protected static IRenderer iInstance = null;
}

} // Gui
} // Linn
