using Linn.Gui.Resources;
using System.Xml;
using Linn.Gui.Scenegraph;
using System;
using Linn;
using System.Globalization;

namespace Linn {
namespace Gui {
namespace Resources {
    
public sealed class Monostable : Plugin
{
    public Monostable() : base("Monostable") {
        iTimer = new Timer();
        iTimer.Elapsed += Notify;
        iTimer.AutoReset = false;
    }
    
    public Monostable(string aName) : base(aName) {
        iTimer = new Timer();
        iTimer.Elapsed += Notify;
        iTimer.AutoReset = false;
    }
    
    public override void Load(XmlNode aXmlNode) {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("Period");
        if(list != null) {
            Assert.Check(list.Count == 1);
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            iPeriod = float.Parse(list[0].FirstChild.Value, nfi);
        }
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        aWriter.WriteStartElement("Period");
        NumberFormatInfo nfi = new NumberFormatInfo();
        nfi.NumberDecimalSeparator = ".";
        aWriter.WriteString(iPeriod.ToString(nfi));
        aWriter.WriteEndElement();
    }
    
    public float Period {
        get {
            return iPeriod;
        }
        set {
            iPeriod = value;
            if(iPeriod > 0) {
                iTimer.Interval = iPeriod * 1000;
            }
            ObserverUpdate();
        }
    }
    
    private void Notify(object aSender, EventArgs aArgs) {
        iTimer.Stop();
        State = false;
        Renderer.Instance.Render();
    }
    
    public override void Hit(Vector3d aVector) {
        iTimer.Stop();
        State = true;
    }
    
    public override void UnHit() {
        if(iPeriod > 0.0f) {
            iTimer.Start();
        } else {
            State = false;
        }
    }
    
    public override bool ProcessMessage(Message aMessage) {
        if(aMessage.Fullname == Fullname) {
            MsgSetState setState = aMessage as MsgSetState;
            if(setState != null) { 
                State = setState.State;
            }
            return true;
        }
        if(base.ProcessMessage(aMessage)) {
            return true;
        }
        return false;
    }
    
    public bool State {
        get {
            return iState;
        }
        set{
            if(iState != value) {
                iState = value;
                if(NextPlugin != null) {
                    if(iState == true) {
                        NextPlugin.Vector3d(new Vector3d(1, 0, 0));
                    } else {
                        NextPlugin.Vector3d(new Vector3d(0, 0, 0));
                    }
                }
                MsgStateChanged msg = new MsgStateChanged(this, !iState, iState);
                SendMessage(msg);
            }
        }
    }
    
    private bool iState = false;
    private float iPeriod = 0.0f;
    private Timer iTimer = null;
}

public sealed class Bistable : Plugin
{
    public Bistable() : base("Bistable") {
    }
    
    public Bistable(string aName) : base(aName) {
    }
    
    public override void Load(XmlNode aXmlNode) {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("StartState");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iState = bool.Parse(list[0].FirstChild.Value);
        }
    }
    
    public override void Link() {
        base.Link();
        if(NextPlugin != null) {
            if(iState == true) {
                NextPlugin.Vector3d(new Vector3d(1, 0, 0));
            } else {
                NextPlugin.Vector3d(new Vector3d(0, 0, 0));
            }
        }
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        aWriter.WriteStartElement("StartState");
        aWriter.WriteString(iState.ToString());
        aWriter.WriteEndElement();
    }
    
    public override void Hit(Vector3d aVector) {
        State = !iState;
    }
    
    public override bool ProcessMessage(Message aMessage) {
        if(aMessage.Fullname == Fullname) {
            MsgSetState setState = aMessage as MsgSetState;
            if(setState != null) {
                State = setState.State;
                return true;
            }
            MsgToggleState toggleState = aMessage as MsgToggleState;
            if(toggleState != null) {
                State = !State;
                return true;
            }
        }
        if(base.ProcessMessage(aMessage)) {
            return true;
        }
        return false;
    }
    
    public bool State {
        get {
            return iState;
        }
        set {
            if(iState != value) {
                iState = value;
                if(NextPlugin != null) {
                    if(iState == true) {
                        NextPlugin.Vector3d(new Vector3d(1, 0, 0));
                    } else {
                        NextPlugin.Vector3d(new Vector3d(0, 0, 0));
                    }
                }
                MsgStateChanged msg = new MsgStateChanged(this, !iState, iState);
                SendMessage(msg);
                ObserverUpdate();
            }
        }
    }
    
    private bool iState = false;
}

public sealed class Counter : Plugin
{
    public Counter() : base("Counter") {
        iTimer = new Timer();
        iTimer.Interval = 40;
        iTimer.Elapsed += Notify;
        iTimer.AutoReset = false;
    }
    
    public Counter(string aName) : base(aName) {
        iTimer = new Timer();
        iTimer.Interval = 40;
        iTimer.Elapsed += Notify;
        iTimer.AutoReset = false;
    }
    
    public override void Load(XmlNode aXmlNode) {
        base.Load(aXmlNode);
        
        XmlNodeList list;
        list = aXmlNode.SelectNodes("MaxCount");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iMaxCount = int.Parse(list[0].FirstChild.Value);
        }
        
        list = aXmlNode.SelectNodes("Count");
        if(list != null) {
            if(list.Count > 0) {
                Assert.Check(list.Count == 1);
                iCount = int.Parse(list[0].InnerText);
            }
        }
        
        list = aXmlNode.SelectNodes("CountsPerSecond");
        if(list != null) {
            Assert.Check(list.Count == 1);
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            CountsPerSecond = float.Parse(list[0].FirstChild.Value, nfi);
        }
        
        list = aXmlNode.SelectNodes("Loop");
        if(list != null) {
            Assert.Check(list.Count == 1);
            Loop = bool.Parse(list[0].FirstChild.Value);
        }

        list = aXmlNode.SelectNodes("Counting");
        if(list != null) {
            Assert.Check(list.Count == 1);
            iCounting = bool.Parse(list[0].FirstChild.Value);
            if(iCounting == true) {
                Start();
            } else {
                Stop();
            }
        }
    }
    
    public override void Link() {
        base.Link();
        if(NextPlugin != null) {
            NextPlugin.Vector3d(new Vector3d(iCount, 0, 0));
        }
    }
    
    public override void Save(XmlTextWriter aWriter) {
        base.Save(aWriter);
        
        aWriter.WriteStartElement("MaxCount");
        aWriter.WriteString(iMaxCount.ToString());
        aWriter.WriteEndElement();
        
        aWriter.WriteStartElement("CountsPerSecond");
        aWriter.WriteString(iCountsPerSecond.ToString());
        aWriter.WriteEndElement();
        
        aWriter.WriteStartElement("Loop");
        aWriter.WriteString(iLoop.ToString());
        aWriter.WriteEndElement();
        
        aWriter.WriteStartElement("Counting");
        aWriter.WriteString(iCounting.ToString());
        aWriter.WriteEndElement();
    }
    
    public void Notify(object aSender, EventArgs aArgs) {
        long curTime = DateTime.Now.Ticks;
        float elapsedTime = (float)TimeSpan.FromTicks(curTime - iLastTime).TotalSeconds;
        int count = (int)Math.Floor((elapsedTime + iRemainder) / iTimeStep);
        iRemainder = (elapsedTime + iRemainder) - (iTimeStep * count);
        System.Console.WriteLine("elapsedTime=" + elapsedTime + ", iTimeStep=" + iTimeStep + ", count=" + count + ", iRemainder=" + iRemainder);
        // subtract frames from index if counting in reverse
        if(iReverseCount) {
            count = Count - count;
        } else {
            count = Count + count;
        }
        int newCount = count % iMaxCount;
        Trace.WriteLine(Trace.kGui, "newCount = " + newCount + ", count = " + count);
        if((newCount == count && count >= 0) || Loop) {
            // inc/dec or wrap the counter
            Count = newCount;
            iLastTime = curTime;
            iTimer.Start();
        } else {
            // clamp counter to bottom/top depending on count direction
            if(iReverseCount) {
                Count = 0;
            } else {
                Count = iMaxCount - 1;
            }
            Stop();
            MsgCountEnd msg = new MsgCountEnd(this);
            SendMessage(msg);
        }
        Renderer.Instance.Render();
    }
    
    public float CountsPerSecond {
        get {
            return iCountsPerSecond;
        }
        set {
            if(value != iCountsPerSecond) {
                iCountsPerSecond = value;
                if(iCountsPerSecond > 0) {
                    iTimeStep = 1.0f / iCountsPerSecond;
                } else {
                    Stop();
                }
                ObserverUpdate();
            }
        }
    }
    
    public bool Loop {
        get {
            return iLoop;
        }
        set {
            if(value != iLoop) {
                iLoop = value;
                /*if(iLoop) {
                    if(iTimeStep > 0 && !iCounting) {
                        Start();
                    }
                } else {
                    Stop();
                }*/
                ObserverUpdate();
            }
        }
    }
    
    public int MaxCount {
        get {
            return iMaxCount;
        }
        set {
            if(value != iMaxCount) {
                iMaxCount = value;
                if(iCount >= iMaxCount) {
                    Count = iMaxCount - 1;
                }
                ObserverUpdate();
            }
        }
    }
    
    public int Count {
        get {
            return iCount;
        }
        set {
            if(value != iCount) {
                Trace.WriteLine(Trace.kGui, "iCount = " + iCount);
                if(value < 0) {
                    iCount = 0;
                } else if(value >= iMaxCount) {
                    iCount = iMaxCount - 1;
                } else {
                    iCount = value;
                }
                if(NextPlugin != null) {
                    NextPlugin.Vector3d(new Vector3d(iCount, 0, 0));
                }
                ObserverUpdate();
            }
        }
    }
    
    public bool Counting {
        get {
            return iCounting;
        }
    }
    
    public void Start() {
        Trace.WriteLine(Trace.kGui, "Counter's timer started.");
        iCounting = true;
        iTimer.Start();
        iLastTime = DateTime.Now.Ticks;
    }
    
    public void Stop() {
        Trace.WriteLine(Trace.kGui, "Counter's timer stopped.");
        iCounting = false;
        iReverseCount = false;
        iTimer.Stop();
    }
    
    public override void Vector3d(Vector3d aVector) {
        if(aVector.X == 0) {
            iReverseCount = true;
        } else {
            iReverseCount = false;
        }
        if(iTimeStep > 0 && !iCounting) {
            Start();
        }
    }
    
    public override bool ProcessMessage(Message aMessage) {
        if(aMessage.Fullname == Fullname) {
            MsgCountStart start = aMessage as MsgCountStart;
            if(start != null) {
                Start();
                return true;
            }
            MsgCountStop stop = aMessage as MsgCountStop;
            if(stop != null) {
                Stop();
                return true;
            }
        }
        if(base.ProcessMessage(aMessage)) {
            return true;
        }
        return false;
    }
        
    private float iCountsPerSecond = 0.0f;
    private bool iLoop = false;
    private int iMaxCount = 0;
    private int iCount = 0;
    private float iTimeStep = 0;
    private bool iCounting = false;
    private bool iReverseCount = false;
    private Timer iTimer = null;
    private long iLastTime;
    private float iRemainder = 0.0f;
}

} // Resources
} // Gui
} // Linn
