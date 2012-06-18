using System.Drawing;
using System.Windows.Forms;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System.Drawing.Drawing2D;
using System.Threading;

namespace Linn {
namespace Gui {

public class RendererGdiPlus : Renderer
{
    public RendererGdiPlus(CanvasGdiPlus aCanvas) : base() {
        Assert.Check(aCanvas != null);
        Trace.WriteLine(Trace.kGui, ">RendererGdiPlus");
        new TextureManagerGdi();
        new MessengerGdi(aCanvas);
        iCanvas = aCanvas;
        iMutex = new Mutex(false);
    }
    
    public override RenderStats Stats {
        get {
            return iStats;
        }
    }
    
    public override void Lock() {
        iMutex.WaitOne();
    }
    
    public override void Unlock() {
        iMutex.ReleaseMutex();
    }
    
    public override void BeginFrame() {
        //iRenderTarget.Clear(Color.Red);
    }
    
    public override void EndFrame() {
    }
    
    delegate void RefreshCallback();
    public override void Render() {
        //Trace.WriteLine(Trace.kGui, ">RendererGdiPlus.Render");
        if(iCanvas.InvokeRequired) {    
            RefreshCallback d = new RefreshCallback(Render);
            iCanvas.BeginInvoke(d);
        } else {
            Render(true);
        }
    }
    
    delegate void ForceRefreshCallback(bool aForce);
    public override void Render(bool aForce) {
        //Trace.WriteLine(Trace.kGui, ">RendererGdiPlus.Render");
        if(iCanvas.InvokeRequired) {
            ForceRefreshCallback d = new ForceRefreshCallback(Render);
            iCanvas.BeginInvoke(d, new object[]{aForce});
        } else {
            //System.Console.WriteLine("RendererGdiPlus.Render: aForce=" + aForce);
            iCanvas.Invalidate(true);
            if(aForce) {
                iCanvas.Update();
            }
        }
    }
    
    delegate void RectRefreshCallback(Rectangle aRect);
    public override void Render(Rectangle aRect) {
        //Trace.WriteLine(Trace.kGui, ">RendererGdiPlus.Render");
        if(iCanvas.InvokeRequired) {    
            RectRefreshCallback d = new RectRefreshCallback(Render);
            iCanvas.BeginInvoke(d, new object[]{aRect});
        } else {
            Render(aRect, true);
        }
    }
    
    delegate void ForceNodeRefreshCallback(Rectangle aRect, bool aForce);
    public override void Render(Rectangle aRect, bool aForce) {
        //Trace.WriteLine(Trace.kGui, ">RendererGdiPlus.Render");
        if(iCanvas.InvokeRequired) {
            ForceNodeRefreshCallback d = new ForceNodeRefreshCallback(Render);
            iCanvas.BeginInvoke(d, new object[]{aRect, aForce});
        } else {
            //System.Console.WriteLine("RendererGdiPlus.Render: aForce=" + aForce + ", aRect=" + aRect);
            iCanvas.Invalidate(aRect, true);
            if(aForce) {
                iCanvas.Update();
            }
        }
    }
    
    public override void DrawPolygon(NodePolygon aPolygon) {
        RenderState rs = aPolygon.CurrRenderState;
        if(rs.Texture.Object != null) {
            TextureGdi texture = (TextureGdi)rs.Texture.Object;
            bool clamp = aPolygon.ClampToTextureSize;
            if(texture.Surface == null) {
                texture = (TextureGdi)TextureManager.Instance.NotFoundTexture;
                clamp = false;
            }
            if(clamp == true) {
                if(texture.Width < 1 || texture.Height < 1) {
                    return;
                }
                iRenderTarget.DrawImage(texture.Surface,
                    (int)aPolygon.WorldSrt.Translation.X, (int)aPolygon.WorldSrt.Translation.Y,
                    (int)texture.Width, (int)texture.Height);
            } else {
                if(aPolygon.Width < 1 || aPolygon.Height < 1) {
                    return;
                }
                iRenderTarget.DrawImage(texture.Surface,
                    (int)aPolygon.WorldSrt.Translation.X, (int)aPolygon.WorldSrt.Translation.Y,
                    (int)aPolygon.Width, (int)aPolygon.Height);
            }
        }
    }
    
    public override void DrawText(NodeText aText) {
        Font font = aText.CurrFont;
        if(font == null) {
            font = iDefaultFont;
        }
        if(aText.Width < 1 || aText.Height < 1) {
            return;
        }
        RectangleF rect = new RectangleF(aText.WorldSrt.Translation.X, aText.WorldSrt.Translation.Y, aText.Width, aText.Height);
        StringFormat format = new StringFormat();
        switch(aText.Justification) {
            case(NodeFont.EJustification.EJ_Left):
                format.Alignment = StringAlignment.Near;
                break;
            case(NodeFont.EJustification.EJ_Centre):
                format.Alignment = StringAlignment.Center;
                break;
            case(NodeFont.EJustification.EJ_Right):
                format.Alignment = StringAlignment.Far;
                break;
        }
        switch(aText.Alignment) {
            case(NodeFont.EAlignment.EA_Top):
                format.LineAlignment = StringAlignment.Near;
                break;
            case(NodeFont.EAlignment.EA_Centre):
                format.LineAlignment = StringAlignment.Center;
                break;
            case(NodeFont.EAlignment.EA_Bottom):
                format.LineAlignment = StringAlignment.Far;
                break;
        }
        switch(aText.Trimming) {
            case(NodeFont.ETrimming.ET_None):
                format.Trimming = StringTrimming.None;
                break;
            case(NodeFont.ETrimming.ET_Character):
                format.Trimming = StringTrimming.Character;
                break;
            case(NodeFont.ETrimming.ET_EllipsisCharacter):
                format.Trimming = StringTrimming.EllipsisCharacter;
                break;
            case(NodeFont.ETrimming.ET_Word):
                format.Trimming = StringTrimming.Word;
                break;
            case(NodeFont.ETrimming.ET_EllipsisWord):
                format.Trimming = StringTrimming.EllipsisWord;
                break;
            case(NodeFont.ETrimming.ET_EllipsisPath):
                format.Trimming = StringTrimming.EllipsisPath;
                break;
        }
        format.FormatFlags = StringFormatFlags.NoWrap;

        Color colour = Color.FromArgb(aText.Colour.A, aText.Colour.R, aText.Colour.G, aText.Colour.B);
        iRenderTarget.DrawString(aText.Text, font, new SolidBrush(colour), rect, format);
    }
    
    public Graphics RenderTarget {
        get {
            return iRenderTarget;
        }
        set {
            iRenderTarget = value;
            iRenderTarget.TextContrast = 0;
            iRenderTarget.CompositingMode = CompositingMode.SourceOver;
            iRenderTarget.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            Trace.WriteLine(Trace.kRendering, "RendererGdiPlus.RenderTarget: Clip=" + iRenderTarget.ClipBounds);
        }
    }
    
    public CanvasGdiPlus Canvas {
        get {
            return iCanvas;
        }
    }
    
    public Font DefaultFont {
        get {
            return iDefaultFont;
        }
    }
    
    private Mutex iMutex;
    private CanvasGdiPlus iCanvas;
    private Font iDefaultFont = new Font("Arial", 10, FontStyle.Regular);
    private RenderStats iStats = new RenderStats();
    protected Graphics iRenderTarget;
}

} // Gui
} // Linn
