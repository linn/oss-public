using System.Drawing;
using System.Windows.Forms;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System.Drawing.Drawing2D;
using System.Threading;
using System;
using System.Collections.Generic;

namespace Linn {
namespace Gui {

public class RendererGdi : Renderer
{   
    public RendererGdi(CanvasGdi aCanvas) : base() {
        Assert.Check(aCanvas != null);
        Trace.WriteLine(Trace.kGui, ">RendererGdi");
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
        Ticker ticker = new Ticker();
        if(iOffscreenImage == null ||
           iOffscreenImage.Width != iCanvas.ClientSize.Width ||
           iOffscreenImage.Height != iCanvas.ClientSize.Height) { // Bitmap for doublebuffering
               iOffscreenImage = new Bitmap(iCanvas.ClientRectangle.Width, iCanvas.ClientRectangle.Height);
        }
        iOffscreenTarget = Graphics.FromImage(iOffscreenImage);
        //iOffscreenTarget.Clear(Color.Red);
        Trace.WriteLine(Trace.kRendering, "RendererGdi.BeginFrame: ms=" + ticker.MilliSeconds);
    }
    
    public override void EndFrame() {
        Ticker ticker = new Ticker();
        RectangleF r = iRenderTarget.Clip.GetBounds(iRenderTarget);
        Rectangle rect = new Rectangle((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
        iOffscreenTarget.Dispose();
        Trace.WriteLine(Trace.kRendering, "RendererGdi.EndFrame: ms=" + ticker.MilliSeconds);
        Trace.WriteLine(Trace.kRendering, "RendererGdi.EndFrame: iRenderTarget.ClipBounds={X=" + rect.X + ",Y=" + rect.Y + ",Width=" + rect.Width + ",Height=" + rect.Height + "}");
        iRenderTarget.DrawImage(iOffscreenImage, rect, rect, GraphicsUnit.Pixel);
        Trace.WriteLine(Trace.kRendering, "RendererGdi.EndFrame: ms=" + ticker.MilliSeconds);
    }
    
    delegate void RefreshCallback();
    public override void Render() {
        //Trace.WriteLine(Trace.kGui, ">RendererGdi.Render");
        if(iCanvas.InvokeRequired) {    
            RefreshCallback d = new RefreshCallback(Render);
            iCanvas.BeginInvoke(d);
        } else {
            Render(true);
        }
    }
    
    delegate void ForceRefreshCallback(bool aForce);
    public override void Render(bool aForce) {
        //Trace.WriteLine(Trace.kGui, ">RendererGdi.Render");
        if(iCanvas.InvokeRequired) {    
            ForceRefreshCallback d = new ForceRefreshCallback(Render);
            iCanvas.BeginInvoke(d, new object[]{aForce});
        } else {
            iCanvas.Invalidate();
            if(aForce) {
                iCanvas.Update();
            }
        }
    }
    
    delegate void RectRefreshCallback(Rectangle aRect);
    public override void Render(Rectangle aRect) {
        //Trace.WriteLine(Trace.kGui, ">RendererGdi.Render");
        if(iCanvas.InvokeRequired) {    
            RectRefreshCallback d = new RectRefreshCallback(Render);
            iCanvas.BeginInvoke(d, new object[]{aRect});
        } else {
            Render(aRect, true);
        }
    }
    
    delegate void ForceNodeRefreshCallback(Rectangle aRect, bool aForce);
    public override void Render(Rectangle aRect, bool aForce) {
        //Trace.WriteLine(Trace.kGui, ">RendererGdi.Render");
        if(iCanvas.InvokeRequired) {    
            ForceNodeRefreshCallback d = new ForceNodeRefreshCallback(Render);
            iCanvas.BeginInvoke(d, new object[]{aRect, aForce});
        } else {
            iCanvas.Invalidate(aRect);
            if(aForce) {
                iCanvas.Update();
            }
        }
    }
    
    public override void DrawPolygon(NodePolygon aPolygon) {
        Ticker ticker = new Ticker();
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
                Rectangle rect = new Rectangle((int)aPolygon.WorldSrt.Translation.X, (int)aPolygon.WorldSrt.Translation.Y, (int)texture.Width, (int)texture.Height);
                iOffscreenTarget.DrawImage(texture.Surface,
                    rect,
                    new Rectangle(0, 0, (int)texture.Width, (int)texture.Height), GraphicsUnit.Pixel);
            } else {
                if(aPolygon.Width < 1 || aPolygon.Height < 1) {
                    return;
                }
                iOffscreenTarget.DrawImage(texture.Surface,
                    new Rectangle((int)aPolygon.WorldSrt.Translation.X, (int)aPolygon.WorldSrt.Translation.Y, (int)aPolygon.Width, (int)aPolygon.Height),
                    new Rectangle(0, 0, (int)texture.Width, (int)texture.Height), GraphicsUnit.Pixel);
            }
        }
        Trace.WriteLine(Trace.kRendering, "RendererGdi.DrawPolygon: Fullname=" + aPolygon.Fullname + ", ms=" + ticker.MilliSeconds);
    }
    
    public override void DrawText(NodeText aText) {
        Ticker ticker = new Ticker();
        Font font = new Font(aText.CurrFont.Name, aText.CurrFont.Size / 2, aText.CurrFont.Style);
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
        format.FormatFlags = StringFormatFlags.NoWrap;

        Color colour = Color.FromArgb(aText.Colour.R, aText.Colour.G, aText.Colour.B);
        iOffscreenTarget.DrawString(aText.Text, font, new SolidBrush(colour), rect, format);
        Trace.WriteLine(Trace.kRendering, "RendererGdi.DrawText: Fullname=" + aText.Fullname + ", ms=" + ticker.MilliSeconds);
    }
    
    public Graphics RenderTarget {
        get {
            return iRenderTarget;
        }
        set {
            iMutex.WaitOne();
            iRenderTarget = value;
            Trace.WriteLine(Trace.kRendering, "iRenderTarget, iRenderTarget.ClipBounds={X=" + iRenderTarget.ClipBounds.X + ",Y=" + iRenderTarget.ClipBounds.Y + ",Width=" + iRenderTarget.ClipBounds.Width + ",Height=" + iRenderTarget.ClipBounds.Height + "}");
            iMutex.ReleaseMutex();
        }
    }
    
    public CanvasGdi Canvas {
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
    private CanvasGdi iCanvas;
    private Font iDefaultFont = new Font("Arial", 10, FontStyle.Regular);
    private RenderStats iStats = new RenderStats();
    private Bitmap iOffscreenImage = null;
    private Graphics iOffscreenTarget = null;
    protected Graphics iRenderTarget;
}

} // Gui
} // Linn
