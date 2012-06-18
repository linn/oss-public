using Linn.Gui.Resources;
using System;
using System.Drawing;
using System.Globalization;

namespace Linn {
namespace Gui {
namespace Scenegraph {

public class NodeVolume : NodeText
{
    public NodeVolume() : base(UniqueName("NodeVolume")) {
    }
    
    public override Object Clone() {
        NodeVolume v = new NodeVolume();
        Clone(v);
        return v;
    }
    
    protected void Clone(NodeVolume aNodeVolume) {
        Clone((NodeText)aNodeVolume);
        aNodeVolume.iVolume = iVolume;
        aNodeVolume.iRemainder = iRemainder;
    }
    
    public override bool ProcessMessage(Message aMessage) {
        if(aMessage.Fullname == Fullname) {
            if(aMessage as MsgInputRotate != null) {
                float speed = ((MsgInputRotate)aMessage).Speed;
                float inc = iRemainder + speed * 0.05f;
                if(inc < -2.0f) {
                    inc = -2.0f;//0.5f;
                } else if(inc > 2.0f) {
                    inc = 2.0f;
                } else {
                    iRemainder = inc - (int)inc;
                    inc = (int)inc;
                }
                iVolume += inc;
                if(iVolume > 100.0f) {
                    iVolume = 100.0f;
                } else if(iVolume < 0.0f) {
                    iVolume = 0.0f;
                }
                Text = iVolume.ToString();
                return true;
            }
            if(aMessage as MsgSetText != null) {
                NumberFormatInfo nfi = new NumberFormatInfo();
                nfi.NumberDecimalSeparator = ".";
                MsgSetText text = aMessage as MsgSetText;
                iVolume = float.Parse(text.Text, nfi);
            }
        }
        if(base.ProcessMessage(aMessage)) {
            return true;
        }
        return false;
    }
    
    private float iVolume = 0.0f;
    private float iRemainder = 0.0f;
}

} // Scenegraph
} // Gui
} // Linn

