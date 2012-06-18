using Linn.Gui;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System;
using System.Drawing;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public class ControllerPreampPhantom : IControllerPreamp
{
    public ControllerPreampPhantom(Node aRoot, Room aRoom) {
        iModel = aRoom;
        iView = new ViewPreampPhantom();
        Messenger.Instance.EEventAppMessage += Receive;
    }
    
    public void Dispose() {
        Messenger.Instance.EEventAppMessage -= Receive;
        if(iView != null) {
            iView.Dispose();
            //iView = null;
        }
    }
    
    public void Receive(Linn.Gui.Resources.Message aMessage) {
        if(aMessage.Fullname == "RoomSelection.StandByButton.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == false) {
                    Standby = true;
                }
            }
        }
    }

    public bool Standby {
        set {
            Trace.WriteLine(Trace.kLinnGuiPreamp, "ControllerPreampPhantom.Standby.set: " + value);
            iModel.Standby = value;
            //IListEntryProvider p = iModel.ComponentList;
            Assert.Check(p != null);
            uint count = p.Count;
            for(uint i = 0; i < count; ++i) {
                Component source = p.Entry(i) as Component;
                if(source != null) {
                    source.ModelDeviceSource.ModelSource.Standby = value;
                }
            }
        }
    }
    
    private Room iModel;
    private ViewPreampPhantom iView;
}
    
} // Kinsky
} // Linn
