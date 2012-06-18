using Linn.Gui;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System;
using System.Drawing;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public class ControllerPreamp : IControllerPreamp
{
    public ControllerPreamp(Node aRoot, Room aRoom) {
        iRoom = aRoom;
        iView = new ViewPreamp(aRoot, this, aModel);

        VisitorSearch search = new VisitorSearch("StatusBar.VolumeText");
        iVolume = (NodeText)search.Search(aRoot);
        Assert.Check(iVolume != null);
        
        search = new VisitorSearch("ControlPanel.VolumeUp");
        iVolumeUp = (NodePolygon)search.Search(aRoot);
        Assert.Check(iVolumeUp != null);
        
        search = new VisitorSearch("ControlPanel.VolumeDown");
        iVolumeDown = (NodePolygon)search.Search(aRoot);
        Assert.Check(iVolumeDown != null);

        iVolumeAutoRepeatTimer = new Timer();
        iVolumeAutoRepeatTimer.Interval = kVolumeRampRate;
        iVolumeAutoRepeatTimer.Elapsed += VolumeAutoRepeat;
        iVolumeAutoRepeatTimer.AutoReset = false;
        
        iStartAutoRepeatTimer = new Timer();
        iStartAutoRepeatTimer.Interval = 500;
        iStartAutoRepeatTimer.Elapsed += StartAutoRepeat;
        iStartAutoRepeatTimer.AutoReset = false;
        
        Messenger.Instance.EEventAppMessage += Receive;
        iModel.EEventSubscribed += EventSubscribed;
        iModel.EEventVolumeRampRate += EventVolumeRampRate;
    }
    
    public void Dispose() {
        Messenger.Instance.EEventAppMessage -= Receive;
        iModel.EEventSubscribed -= EventSubscribed;
        iModel.EEventVolumeRampRate -= EventVolumeRampRate;

        if(iVolumeAutoRepeatTimer != null) {
            iVolumeAutoRepeatTimer.Stop();
            //iVolumeAutoRepeatTimer = null;
        }
        if(iStartAutoRepeatTimer != null) {
            iStartAutoRepeatTimer.Stop();
            //iStartAutoRepeatTimer = null;
        }
        if(iView != null) {
            iView.Dispose();
            //iView = null;
        }
    }

    public void Receive(Linn.Gui.Resources.Message aMessage) {
        if(aMessage.Fullname == "StatusBar.VolumeTextInput") {
            if(aMessage as MsgInputBase != null) {
                try {
                    iModel.Volume = uint.Parse(iVolume.Text);
                } catch(System.FormatException e) {
                    System.Console.WriteLine("WARNING: ControllerPreamp.Receive:\n" + e);
                }
            }
        }
        if(aMessage.Fullname == "ControlPanel.VolumeUp.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true && iVolumeInc == 0) {
                    iTargetVolume = iModel.Volume;
                    iStartAutoRepeatTimer.Start();
                    iVolumeInc = 1;
                    SetVolume();
                } else {
                    iStartAutoRepeatTimer.Stop();
                    iVolumeAutoRepeatTimer.Stop();
                    iVolumeInc = 0;
                }
            }
        }
        if(aMessage.Fullname == "ControlPanel.VolumeDown.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true && iVolumeInc == 0) {
                    iTargetVolume = iModel.Volume;
                    iStartAutoRepeatTimer.Start();
                    iVolumeInc = -1;
                    SetVolume();
                } else {
                    iStartAutoRepeatTimer.Stop();
                    iVolumeAutoRepeatTimer.Stop();
                    iVolumeInc = 0;
                }
            }
        }
        if(aMessage.Fullname == "ControlPanel.Mute.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true) {
                    iModel.Mute = !iModel.Mute;
                }
            }
        }
        if(aMessage.Fullname == "StatusBar.VolumeText") {
            if(aMessage as MsgInputRotate != null) {
                try {
                    iModel.Volume = uint.Parse(iVolume.Text);
                } catch(System.FormatException e) {
                    System.Console.WriteLine("WARNING: ControllerPreamp.Receive:\n" + e);
                }
            }
        }
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
            Trace.WriteLine(Trace.kLinnGuiPreamp, "ControllerPreamp.Standby.set: " + value);
            iModel.Standby = value;
            IListEntryProvider p = iModelRoom.SourceList;
            Assert.Check(p != null);
            uint count = p.Count;
            for(uint i = 0; i < count; ++i) {
                ModelRoomSource source = p.Entry(i) as ModelRoomSource;
                if(source != null) { 
                    source.ModelDeviceSource.ModelSource.Standby = value;
                }
            }
        }
    }

    private void EventSubscribed(object aSender) {
        EventVolumeRampRate();
    }

    private void EventVolumeRampRate() {
        float interval = 0.0f;
        if(iModel.VolumeRampRate > 0 ) {
            interval = 1000.0f / iModel.VolumeRampRate;
        }
        if(interval <= 0) {
            interval = kVolumeRampRate;
        }
        Trace.WriteLine(Trace.kLinnGuiPreamp, "ControllerPreamp.EventVolumeRampRate: interval=" + interval);
        iVolumeAutoRepeatTimer.Interval = interval;
    }

    private void StartAutoRepeat(object aSender, EventArgs aArgs) {
        iVolumeAutoRepeatTimer.Start();
    }
    
    private void VolumeAutoRepeat(object aSender, EventArgs aArgs) {
        Trace.WriteLine(Trace.kLinnGuiPreamp, ">ControllerPreamp.VolumeAutoRepeat");
        RendererGdiPlus r = Renderer.Instance as RendererGdiPlus;
        if(r != null) {
            Point p = r.CurrForm.PointToClient(System.Windows.Forms.Form.MousePosition);
            Vector3d mousePos = new Vector3d(p.X, p.Y, 0);
            //System.Console.WriteLine("ControllerPreamp.VolumeAutoRepeat: Focused=" + (System.Windows.Forms.Form.ActiveForm == r.CurrForm) + ", MouseButtons=" + System.Windows.Forms.Form.MouseButtons);
            //System.Console.WriteLine("ControllerPreamp.VolumeAutoRepeat: mousePos=" + mousePos + ", iVolumeUp.WorldTranslation=" + (mousePos - iVolumeUp.WorldTranslation) + ", iVolumeDown.WorldTranslation=" + (mousePos - iVolumeDown.WorldTranslation));
            //System.Console.WriteLine("ControllerPreamp.VolumeAutoRepeat: iVolumeUp.Inside=" + iVolumeUp.Inside(mousePos - iVolumeUp.WorldTranslation) + ", iVolumeDown.Inside=" + iVolumeDown.Inside(mousePos - iVolumeDown.WorldTranslation));
            if(System.Windows.Forms.Form.MouseButtons == System.Windows.Forms.MouseButtons.None ||
                (!iVolumeUp.Inside(mousePos - iVolumeUp.WorldTranslation) && iVolumeInc > 0) ||
                (!iVolumeDown.Inside(mousePos - iVolumeDown.WorldTranslation) && iVolumeInc < 0)) {
                Trace.WriteLine(Trace.kLinnGuiPreamp, "ControllerPreamp.VolumeAutoRepeat: Cancelled auto repeat");
                return;
            }
        }
        SetVolume();
        iVolumeAutoRepeatTimer.Start();
        Trace.WriteLine(Trace.kLinnGuiPreamp, "<ControllerPreamp.VolumeAutoRepeat");
    }
    
    private void SetVolume() {
        if(iVolumeInc > 0) {
            iTargetVolume += (uint)iVolumeInc;
        } else if (iVolumeInc < 0 && iTargetVolume > 0) {
            iTargetVolume -= (uint)-iVolumeInc;
        }
        iModel.Volume = iTargetVolume;
    }
    
    private Room iRoom;
    private ViewPreamp iView;
    private NodeText iVolume;
    private NodePolygon iVolumeUp;
    private NodePolygon iVolumeDown;
    private Timer iStartAutoRepeatTimer;
    private Timer iVolumeAutoRepeatTimer;
    private int iVolumeInc = 0;
    private uint iTargetVolume = 0;

    private static readonly uint kVolumeRampRate = 20;
}
    
} // Kinsky
} // Linn
