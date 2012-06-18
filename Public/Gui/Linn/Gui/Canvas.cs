using System.Windows.Forms;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System.Collections;
using System;
using System.Collections.Generic;

namespace Linn {
namespace Gui {
       
public class Canvas : System.Windows.Forms.Control
{
    public Canvas() {
        Name = "CanvasPanel";
    }
    
    public void Load(string aFilename) {
        CurrLayout = PackageManager.Instance.Load(aFilename);
        Messenger.Instance.Root = CurrLayout.Root;
    }
    
    public Package CurrLayout {
        get {        
            return iLayout;
        }
        set {
            iLayout = value;
            if(iLayout != null) {
                iWidth = iLayout.Root.Width;
                iHeight = iLayout.Root.Height;
                Size = new System.Drawing.Size((int)iWidth, (int)iHeight);
            } else {
                iWidth = 0;
                iHeight = 0;
                Size = new System.Drawing.Size(0, 0);
            }
        }
    }
    
    public void Deactivate() {
        Trace.WriteLine(Trace.kGui, ">Canvas.Deactivate");
        if(iCurrNode != null) {
            iCurrNode.UnHit();
            iCurrNode = null;
        }
        Renderer.Instance.Render();
        Trace.WriteLine(Trace.kGui, "<Canvas.Deactivate");
    }
    
    protected override void OnPaintBackground(PaintEventArgs e) {
    }
    
    protected override void OnClick(EventArgs e) {
        base.OnClick(e);
        ProcessClick(e);
        Renderer.Instance.Render();
    }
    
    protected override void OnDoubleClick(EventArgs e) {
        base.OnDoubleClick(e);
        ProcessDoubleClick(e);
        Renderer.Instance.Render();
    }
    
    protected override void OnMouseDown(MouseEventArgs e) {
        base.OnMouseDown(e);
        iMouseDownPos = new Vector3d(e.X, e.Y, 0);
        iCurrNode = FindNode(iMouseDownPos);
        ProcessMouseDown(e);
        Renderer.Instance.Render();
    }
    
    protected override void OnMouseUp(MouseEventArgs e) {
        base.OnMouseUp(e);
        ProcessMouseUp(e);
        Renderer.Instance.Render();
    }
    
    protected override void OnMouseMove(MouseEventArgs e) {
        base.OnMouseMove(e);
        ProcessMouseMotion(e);
    }
    
    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);
        ProcessKeyDown(e);
    }
    
    protected override void OnKeyPress(KeyPressEventArgs e) {
        base.OnKeyPress(e);
        ProcessKeyPress(e);
    }
    
    protected override void OnKeyUp(KeyEventArgs e) {
        base.OnKeyUp(e);
        ProcessKeyUp(e);
    }
    
    protected virtual Node FindNode(Vector3d aPosition) {
        List<Node> result = new VisitorHitProxy(aPosition).Hit(iLayout.Root);
        if(result.Count > 0) {
            result.Sort(new Node.FarToNear());
            return((Node)result[0]);
        }
        return null;
    }
    
    protected virtual void ProcessPaint(PaintEventArgs e) {
        Ticker ticker = new Ticker();
        VisitorRender visitor = new VisitorRender(Renderer.Instance);
        visitor.Render(iLayout.Root);
        Trace.WriteLine(Trace.kRendering, "Canvas.ProcessPaint: ms=" + ticker.MilliSeconds);
    }
    
    protected virtual void ProcessClick(EventArgs e) {
        if(iCurrNode != null) {
            iCurrNode.Click(iMouseDownPos);
        }
    }
    
    protected virtual void ProcessDoubleClick(EventArgs e) {
        if(iCurrNode != null) {
            iCurrNode.DoubleClick(iMouseDownPos);
        }
    }
    
    protected virtual void ProcessMouseDown(MouseEventArgs e) {
        if(iCurrNode != null) {
            iCurrNode.Hit(iMouseDownPos);
        }
        // for wibble
        iPrevTime = Environment.TickCount;
        iAccAngle = 0;
        iLastMessage = new MsgInputRotate(iCurrNode, 0);
    }
    
    protected virtual void ProcessMouseUp(MouseEventArgs e) {
        if(iCurrNode != null) {
            iCurrNode.UnHit();
            iCurrNode = null;
        }
    }
    
    protected virtual void ProcessMouseMotion(MouseEventArgs e) {
        if(iCurrNode != null) {
            Vector3d v = new Vector3d(e.X, e.Y, 0);
            iCurrNode.Motion(v);
            iMousePosList.Add(v);
            GenerateWibble();
        }
    }
    
    protected virtual void ProcessKeyDown(KeyEventArgs e) {
        //System.Console.WriteLine("Canvas.ProcessKeyDown: " + e.KeyData + ", alt=" + (e.KeyData & Keys.Alt) + ", ctrl=" + (e.KeyData & Keys.Control)); 
        Messenger.Instance.ApplicationMessage(new MsgKeyDown(e.KeyData));
    }
    
    protected virtual void ProcessKeyPress(KeyPressEventArgs e) { 
        Messenger.Instance.ApplicationMessage(new MsgKeyPress(e.KeyChar));
    }
    
    protected virtual void ProcessKeyUp(KeyEventArgs e) {
        Messenger.Instance.ApplicationMessage(new MsgKeyUp(e.KeyData));
    }
    
    
    private void GenerateWibble() {
        if(iMousePosList.Count < 3 * kSampleRate) {     // we need at least 3 sample points
            return;
        }
        
        float angleAcc = 0;
        int lastIndex = 0;
        for(int i = 0; ((i + 2) * kSampleRate) < iMousePosList.Count; ++i) {
            Vector3d p1 = iMousePosList[i * kSampleRate];
            Vector3d p2 = iMousePosList[(i+1) * kSampleRate];
            Vector3d p3 = iMousePosList[(i+2) * kSampleRate];
            lastIndex = (i+2) * kSampleRate;
            //Console.WriteLine("NodeInput.SendCommand: p1=" + p1 + ", p2=" + p2 + ", p3=" + p3);
            Vector3d edge1 = p1 - p2;
            //Console.WriteLine("NodeInput.SendCommand: edge1=" + edge1);
            edge1.Normalise();
            //Console.WriteLine("NodeInput.SendCommand: ~edge1=" + edge1);
            Vector3d edge2 = p3 - p2;
            //Console.WriteLine("NodeInput.SendCommand: edge2=" + edge2);
            edge2.Normalise();
            //Console.WriteLine("NodeInput.SendCommand: ~edge2=" + edge2);
            
            //Console.WriteLine(edge1 * edge2);
            float dot = edge1 * edge2;
            if(dot < -1) {
                dot = -1;
            } else if(dot > 1) {
                dot = 1;
            }
            float angle = (float)Math.Acos(dot);
            Trace.WriteLine(Trace.kGui, "Canvas.GenerateWibble: " + p1 + " : " + p2 + " : " + p3);
            Trace.WriteLine(Trace.kGui, "Canvas.GenerateWibble: " + edge1 + " : " + edge2 + " = " + angle);
        
            Trace.WriteLine(Trace.kGui, "Canvas.GenerateWibble: edge1 ^ edge2=" + (edge1 ^ edge2));
            if((edge1 ^ edge2).Z >= 0) {
                Trace.WriteLine(Trace.kGui, "Canvas.GenerateWibble: Clockwise");
                iAccAngle += (float)(Math.PI - angle);
                angleAcc += (float)(Math.PI - angle);
            } else {
                Trace.WriteLine(Trace.kGui, "Canvas.GenerateWibble: Anti-Clockwise");
                iAccAngle += (float)(angle - Math.PI);
                angleAcc += (float)(angle - Math.PI);
            }
            Trace.WriteLine(Trace.kGui, "Canvas.GenerateWibble: angle=" + angle + "(" + (angle * 180 / Math.PI) + "), iAccAngle=" + iAccAngle + ", angleAcc=" + angleAcc);
        }
        float accDist = 0;
        for(int i = 0; i + 1 < lastIndex; ++i) {
            accDist = accDist + (iMousePosList[i+1] - iMousePosList[i]).Size();
        }
        float elapsedTime = (float)TimeSpan.FromTicks(Environment.TickCount - iPrevTime).TotalMilliseconds;
        float mouseSpeed = accDist / elapsedTime;
        System.Console.WriteLine("accDist=" + accDist + ", elapsedTime=" + elapsedTime + ", mouseSpeed=" + mouseSpeed);
        if(mouseSpeed > 30000) {
            mouseSpeed = 30000;
        }
        mouseSpeed = mouseSpeed / 300;
        Trace.WriteLine(Trace.kGui, "Canvas.GenerateWibble: mouseVelocity=" + iAccDist + ", mouseSpeed=" + mouseSpeed + ", elapsedTime=" + elapsedTime);
        //Messenger.Instance.PresentationMessage(new MsgSetText("Main.WibbleSpeed", iAccDist.ToString() + ", " + elapsedTime.ToString() + ", " + mouseSpeed.ToString()));

        //Console.WriteLine("Canvas.GenerateWibble: angle=" + angle + "(" + (angle * 180 / Math.PI) + "), iAccAngle=" + iAccAngle);
        Trace.WriteLine(Trace.kGui, "Canvas.GenerateWibble: iAccAngle=" + iAccAngle);
        if(iAccAngle < 0) {
            Trace.WriteLine(Trace.kGui, "Canvas.GenerateWibble: cmd=CLOCKWISE");
            MsgInputRotate m = new MsgInputRotate(iCurrNode, mouseSpeed);
            if(iLastMessage.Speed > 0) {
                Messenger.Instance.PresentationMessage(m);
                Messenger.Instance.ApplicationMessage(m);
            }
            iLastMessage = m;
            iAccAngle = 0;
            iPrevTime = Environment.TickCount;
        } else if(iAccAngle > 0) {
            Trace.WriteLine(Trace.kGui, "Canvas.GenerateWibble: cmd=ANTI_CLOCKWISE");
            MsgInputRotate m = new MsgInputRotate(iCurrNode, -mouseSpeed);
            if(iLastMessage.Speed < 0) {
                Messenger.Instance.PresentationMessage(m);
                Messenger.Instance.ApplicationMessage(m);
            }
            iLastMessage = m;
            iAccAngle = 0;
            iPrevTime = Environment.TickCount;
        }
        iMousePosList.RemoveRange(0, lastIndex);
    }
    
    
    
    
    protected Package iLayout = null;
    protected string iAppName = "";
    protected bool iFullscreen = false;
    protected float iWidth = 0;
    protected float iHeight = 0;
    protected Node iCurrNode = null;
    protected Vector3d iMouseDownPos = null;
    
    
    private int iPrevTime;
    private List<Vector3d> iMousePosList = new List<Vector3d>();
    private float iAccAngle = 0;
    private float iAccDist = 0;
    private MsgInputRotate iLastMessage = null; // for smoothing glitches
    
    // constants that modify the way the wibbler operates
    private const int kSampleRate = 5;
    private const int kSpeedSamples = 2;
}

} // Gui
} // Linn
