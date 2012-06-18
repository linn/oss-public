using System.Windows.Forms;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Linn {
namespace Gui {
       
public class CanvasGdiPlus : Canvas
{
    public CanvasGdiPlus() : base() {
        SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        UpdateStyles();
    }
    
    protected override void OnPaint(PaintEventArgs e) {
        Ticker ticker = new Ticker();
        base.OnPaint(e);
        if(iLayout != null) {
            Trace.WriteLine(Trace.kGui, "Canvas.OnPaint: ClipRectangle=" + e.ClipRectangle);
            RendererGdiPlus renderer = (RendererGdiPlus)Renderer.Instance;
            renderer.Lock();
            renderer.RenderTarget = e.Graphics;
            ProcessPaint(e);
            renderer.Unlock();
        }
        Trace.WriteLine(Trace.kRendering, "CanvasGdiPlus.OnPaint: ms=" + ticker.MilliSeconds);
    }
    
    protected override void OnMouseWheel(MouseEventArgs e) {
        base.OnMouseWheel(e);
        iMouseDownPos = new Vector3d(e.X, e.Y, 0);
        iCurrNode = FindNode(iMouseDownPos);
        ProcessMouseWheel(e);
        iCurrNode = null;
    }
    
    protected override bool IsInputKey(Keys keyData) {
        // Make sure we get arrow keys
        switch( keyData ) {
            case Keys.Up:
            case Keys.Left:
            case Keys.Down:
            case Keys.Right:
            case Keys.Tab:
            case Keys.VolumeDown:
            case Keys.VolumeUp:
            case Keys.VolumeMute:
            case Keys.MediaNextTrack:
            case Keys.MediaPlayPause:
            case Keys.MediaPreviousTrack:
            case Keys.MediaStop:
            return true;
        }

        // The rest can be determined by the base class
        return base.IsInputKey(keyData);
    }
    
    protected virtual void ProcessMouseWheel(MouseEventArgs e) {
        if(iCurrNode != null) {
            iCurrNode.Wheel(e.Delta);
        }
        //System.Console.WriteLine(e.Delta + " " + e.Button + " " + e.Clicks + " " + e.Location + " " + e.X + " " + e.Y );
    }
}

} // Gui
} // Linn
