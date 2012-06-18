using System;
using Linn.Gui;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using Linn.Topology;
using System.Threading;
using System.Collections.Generic;

namespace Linn {
namespace KinskyPda {

public class ControllerPreamp : IMessengerObserver, IDisposable
{
    public ControllerPreamp(Node aRoot, IModelPreamp aModelPreamp, ModelRoom aModelRoom) {
        iModelRoom = aModelRoom;
        iModelPreamp = aModelPreamp;
        iViewPreamp = new ViewPreamp(aRoot, aModelPreamp);
        
        iDisposeMutex = new Mutex(false);
        
        iVolumeAutoRepeatTimer = new Timer();
        iVolumeAutoRepeatTimer.Interval = 1000;
        iVolumeAutoRepeatTimer.Elapsed += VolumeAutoRepeat;
        iVolumeAutoRepeatTimer.AutoReset = false;
        
        iStartAutoRepeatTimer = new Timer();
        iStartAutoRepeatTimer.Interval = 500;
        iStartAutoRepeatTimer.Elapsed += StartAutoRepeat;
        iStartAutoRepeatTimer.AutoReset = false;
        
        Messenger.Instance.EEventAppMessage += Receive;
        iModelPreamp.EEventVolumeRampRate += EventVolumeRampRate;
    }
    
    public void Dispose() {
        iDisposeMutex.WaitOne();
        Messenger.Instance.EEventAppMessage -= Receive;
        iModelPreamp.EEventVolumeRampRate -= EventVolumeRampRate;
        if(iViewPreamp != null) {
            iViewPreamp.Dispose();
        }
        if(iStartAutoRepeatTimer != null) {
            iStartAutoRepeatTimer.Stop();
            iStartAutoRepeatTimer.Dispose();
        }
        if(iVolumeAutoRepeatTimer != null) {
            iVolumeAutoRepeatTimer.Stop();
            iVolumeAutoRepeatTimer.Dispose();
        }
        iDisposed = true;
        iDisposeMutex.ReleaseMutex();
    }
    
    public void Receive(Message aMessage) {
        iDisposeMutex.WaitOne();
        if (iDisposed)
        {
            iDisposeMutex.ReleaseMutex();
            return;
        }
        iDisposeMutex.ReleaseMutex();
        if(aMessage.Fullname == "VolumeControl.VolumeUp.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true && iVolumeInc == 0) {
                    iTargetVolume = iModelPreamp.Volume;
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
        if(aMessage.Fullname == "VolumeControl.VolumeDown.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true && iVolumeInc == 0) {
                    iTargetVolume = iModelPreamp.Volume;
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
        if(aMessage.Fullname == "VolumeControl.VolumeMute.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == true) {
                    iModelPreamp.Mute = !iModelPreamp.Mute;
                }
            }
        }
        /*if(aMessage.Fullname == "RoomSelection.StandByButton.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == false) {
                    Standby = true;
                }
            }
        }*/
    }
    
    public bool Standby {
        set {
            iDisposeMutex.WaitOne();
            if (iDisposed)
            {
                iDisposeMutex.ReleaseMutex();
                return;
            }
            iDisposeMutex.ReleaseMutex();
            Trace.WriteLine(Trace.kPreamp, "ControllerPreamp.Standby.set: " + value);
            iModelPreamp.Standby = value;
            List<IListable> listables = iModelRoom.SourceList.Entries(0, iModelRoom.SourceList.Count);
            foreach(IListable listable in listables) {
                ModelRoomSource source = listable as ModelRoomSource;
                if(source != null) { 
                    source.ModelDeviceSource.ModelSource.Standby = value;
                }
            }
        }
    }
    
    private void EventVolumeRampRate() {
        float interval = 0.0f;
        if(iModelPreamp.VolumeRampRate > 0 ) {
            interval = 1000.0f / iModelPreamp.VolumeRampRate;
        }
        if(interval <= 0) {
            interval = 1000.0f;
        }
        Trace.WriteLine(Trace.kPreamp, "ControllerPreamp.EventVolumeRampRate: interval=" + interval);
        iVolumeAutoRepeatTimer.Interval = interval;
    }
    
    private void StartAutoRepeat(object aSender, EventArgs aArgs) {
        iVolumeAutoRepeatTimer.Start();
    }
    
    private void VolumeAutoRepeat(object aSender, EventArgs aArgs) {
        Trace.WriteLine(Trace.kPreamp, ">ControllerPreamp.VolumeAutoRepeat");
        SetVolume();
        if (iVolumeInc != 0)
        {
            iVolumeAutoRepeatTimer.Start();
        }
        Trace.WriteLine(Trace.kPreamp, "<ControllerPreamp.VolumeAutoRepeat");
    }
    
    private void SetVolume() {
        if(iVolumeInc > 0) {
            iTargetVolume += (uint)iVolumeInc;
        } else if (iVolumeInc < 0 && iTargetVolume > 0) {
            iTargetVolume -= (uint)-iVolumeInc;
        }
        System.Console.WriteLine("ControllerPreamp.SetVolume: iTargetVolume=" + iTargetVolume);
        iModelPreamp.Volume = iTargetVolume;
    }
    
    private Mutex iDisposeMutex = null;
    private bool iDisposed = false;
    private ModelRoom iModelRoom = null;
    private IModelPreamp iModelPreamp = null;
    private ViewPreamp iViewPreamp = null;
    private Timer iStartAutoRepeatTimer = null;
    private Timer iVolumeAutoRepeatTimer = null;
    private int iVolumeInc = 0;
    private uint iTargetVolume = 0;
}

} // KinskyPda
} // Linn
