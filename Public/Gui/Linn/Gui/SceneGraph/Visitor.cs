using Linn.Gui.Scenegraph;
using System.Collections.Generic;
using System;
using Linn.Gui;

namespace Linn {
namespace Gui {
namespace Scenegraph {

public class Visitor {
    public virtual void VisitNode(Node aNode) {
        if(aNode != null) {
            aNode.Visit(this, true);
        }
    }
    
    public virtual void Accept(Node aNode) {
    }
    
    public virtual void AcceptNode(Node aNode) {
        Accept(aNode);
    }
    
    public virtual void AcceptHit(NodeHit aNode) {
        Accept(aNode);
    }
    
    public virtual void AcceptPolygon(NodePolygon aNode) {
        Accept(aNode);
    }
    
    public virtual void AcceptText(NodeText aNode) {
        Accept(aNode);
    }
    
    public virtual void AcceptList(NodeList aNode) {
        Accept(aNode);
    }
}

public class VisitorSelectProxy : Visitor
{
    public VisitorSelectProxy(Vector3d aPosition) {
        iPosition = aPosition;
    }
    
    public List<Node> Hit(Node aNode) {
        VisitNode(aNode);
        return iResults;
    }
    
    public override void AcceptHit(NodeHit aHitNode) {
        if(aHitNode.Active == true) {
            if(aHitNode.Inside(iPosition - aHitNode.WorldSrt.Translation)) {
                iResults.Add(aHitNode);
            }
        }
    }
    
    private Vector3d iPosition;
    private List<Node> iResults = new List<Node>();
}

public sealed class VisitorHitProxy : VisitorSelectProxy
{
    public VisitorHitProxy(Vector3d aPosition) : base(aPosition) {
    }
    
    public override void VisitNode(Node aNode) {
        if(aNode != null) {
            aNode.Visit(this, false);
        }
    }

    public override void AcceptHit(NodeHit aHitNode) {
        if(aHitNode.AllowHits == true) {
            base.AcceptHit(aHitNode);
        }
    }
}

public sealed class VisitorSearch : Visitor
{
    public VisitorSearch(string aName) {
        iName = aName;
    }
    
    public Node Search(Node aNode) {
        Trace.WriteLine(Trace.kGui, "VisitorSearch.Search: " + iName);
        VisitNode(aNode);
        return iResult;
    }
    
    public override void AcceptNode(Node aNode) {
        if(aNode.Fullname == iName) {
            if(iResult != null) {
                throw new DuplicateNode(aNode.Fullname);
            }
            iResult = aNode;
        }
    }
    
    private string iName;
    private Node iResult = null;
}
                
public sealed class VisitorGreatestZ : Visitor
{
    public float Search(Node aNode) {
        VisitNode(aNode);
        return iZ;
    }
    
    public override void AcceptNode(Node aNode) {
        if(iCompare == true) {
            float z = aNode.WorldSrt.Translation.Z;
            if(z > iZ) {
                iZ = z;
            }
        } else {
            iZ = aNode.WorldSrt.Translation.Z;
            iCompare = true;
        }
    }
    
    private bool iCompare = false;
    private float iZ;
}

public sealed class VisitorRender : Visitor
{
    public VisitorRender(IRenderer aRenderer) {
        iRenderer = aRenderer;
    }
    
    public override void VisitNode(Node aNode) {
        if(aNode != null) {
            aNode.Visit(this, false);
        }
    }
    
    public void Render(Node aNode) {
        Ticker render = new Ticker();
        
        Ticker visit = new Ticker(); 
        VisitNode(aNode);
        Trace.WriteLine(Trace.kRendering, "VisitorRender.Render(VisitNode): ms=" + visit.MilliSeconds);
        
        // sort the polygon list
        Ticker sort = new Ticker();
        iRenderList.Sort(new Node.NearToFar());
        Trace.WriteLine(Trace.kRendering, "VisitorRender.Render(Sort): Count=" + iRenderList.Count + ", ms=" + sort.MilliSeconds);
        
        bool success = false;
        while(!success) {
            try {
                float ms;
                iRenderer.BeginFrame();
                Ticker ticker = new Ticker();
                Trace.WriteLine(Trace.kRendering, "TickCount(b loop)=" + ticker.StartTime);
                foreach(Node node in iRenderList) {
                    Ticker drawTicker = new Ticker();
                    Trace.WriteLine(Trace.kRendering, "TickCount(b draw)=" + drawTicker.StartTime);
                    node.BeginDraw();
                    node.Draw(iRenderer);
                    node.EndDraw();
                    ms = drawTicker.MilliSeconds;
                    Trace.WriteLine(Trace.kRendering, "TickCount(e draw)=" + drawTicker.EndTime);
                    Trace.WriteLine(Trace.kRendering, "VisitorRender.Render(Draw): Fullname=" + node.Fullname + ", ms=" + ms);
                }
                ms = ticker.MilliSeconds;
                Trace.WriteLine(Trace.kRendering, "TickCount(e loop)=" + ticker.EndTime);
                Trace.WriteLine(Trace.kRendering, "VisitorRender.Render: loop ms=" + ms);
                iRenderer.EndFrame();
                success = true;
            } catch(System.InvalidOperationException e) {
                System.Console.WriteLine("VisitorRender.Render:\n" + e);
            }
        }
        
        iRenderer.Stats.UpdateTotalMs(render.MilliSeconds);
        Trace.WriteLine(Trace.kRendering, "VisitorRender.Render: ms=" + render.MilliSeconds);
    }
    
    public override void AcceptPolygon(NodePolygon aPolygon) {
        if(aPolygon.Active) {
            iRenderList.Add(aPolygon);
        }
    }
    
    public override void AcceptText(NodeText aText) {
        if(aText.Active) {
            iRenderList.Add(aText);
        }
    }
    
    public override void AcceptList(NodeList aList) {
        if(aList.Active) {
            //aList.Update();
            iRenderList.Add(aList);
        }
    }
    
    private IRenderer iRenderer;
    private List<Node> iRenderList = new List<Node>();
}

public sealed class VisitorPrint : Visitor
{
    public void Print(Node aNode) {
        VisitNode(aNode);
    }
    
    public void NodePrint(Node aNode) {
        Node parent = aNode.Parent;
        while(parent != null) {
            parent = parent.Parent;
            Console.Write("  ");
        }
        Console.WriteLine(aNode.Fullname);
    }
    
    public override void AcceptNode(Node aNode) {
        NodePrint(aNode);
    }
}

} // Scenegraph
} // Gui
} // Linn
