using System.Windows.Forms;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Linn {
namespace Gui {
       
public class CanvasGdi : Canvas
{   
    protected override void OnPaint(PaintEventArgs e) {
        Ticker ticker = new Ticker();
        base.OnPaint(e);
        if(iLayout != null) {
            Trace.WriteLine(Trace.kRendering, "Canvas.OnPaint: ClipRectangle={X=" + e.ClipRectangle.X + ",Y=" + e.ClipRectangle.Y + ",Width=" + e.ClipRectangle.Width + ",Height=" + e.ClipRectangle.Height + "}");
            RendererGdi renderer = (RendererGdi)Renderer.Instance;
            renderer.Lock();
            renderer.RenderTarget = e.Graphics;
            ProcessPaint(e);
            renderer.Unlock();
        }
        Trace.WriteLine(Trace.kRendering, "CanvasGdi.OnPaint: ms=" + ticker.MilliSeconds);
    }
}

} // Gui
} // Linn
