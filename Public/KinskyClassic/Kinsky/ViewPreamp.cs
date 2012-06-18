using Linn;
using Linn.Gui;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using System;
using System.Drawing;
using System.Windows.Forms;
using Linn.Topology;

namespace Linn {
namespace Kinsky {
    
public class ViewPreamp : IDisposable
{   
    public ViewPreamp(Node aRoot, IControllerPreamp aController, Room aModel) {
        //iRoot = aRoot;
        iModel = aModel;
        //iController = aController;

        VisitorSearch search = new VisitorSearch("StatusBar.VolumeText");
        iVolume = (NodeText)search.Search(aRoot);
        Assert.Check(iVolume != null);
        
        iFadeTimer = new Timer();
        iFadeTimer.Interval = 10;
        iFadeTimer.Elapsed += FadeText;
        iFadeTimer.AutoReset = false;
        
        iModel.EEventSubscribed += EventSubscribed;
        iModel.EEventUnSubscribed += EventUnSubscribed;
        iModel.EEventStatusOk += EventStatusOk;
    }
    
    public void Dispose() {
        iModel.EEventSubscribed -= EventSubscribed;
        iModel.EEventUnSubscribed -= EventUnSubscribed;
        iModel.EEventStatusOk -= EventStatusOk;
        iModel.EEventVolume -= EventVolume;
        iModel.EEventMute -= EventMute;
        
        iVolume.Text = "";
        iAlpha = 255;
        iVolume.Colour = new Colour((int)iAlpha, iVolume.Colour.R, iVolume.Colour.G, iVolume.Colour.B);
        
        if(iFadeTimer != null) {
            iFadeTimer.Stop();
            iFadeTimer = null;
        }
        DoUnSubscribe();
    }
    
    private void EventSubscribed(object aSender) {
        Trace.WriteLine(Trace.kLinnGuiPreamp, ">ViewPreamp.EventSubscribed");
        iModel.EEventVolume += EventVolume;
        iModel.EEventMute += EventMute;
        EventVolume();
        EventMute();
        Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.VolumeInfo", true));
        Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.VolumeControl", true));
        Renderer.Instance.Render();
    }
    
    private void EventUnSubscribed(object aSender) {
        DoUnSubscribe();
    }
    
    private void DoUnSubscribe() {
        Messenger.Instance.PresentationMessage(new MsgSetActive("StatusBar.VolumeInfo", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.VolumeControl", false));
        Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.MuteBlue", false));
        Renderer.Instance.Render();
    }
    
    private void EventVolume() {
        Messenger.Instance.PresentationMessage(new MsgSetText("StatusBar.VolumeText", Convert.ToString(iModel.Volume)));
    }
    
    private void EventMute() {
        Messenger.Instance.PresentationMessage(new MsgSetActive("ControlPanel.MuteBlue", Convert.ToBoolean(iModel.Mute)));
        Renderer.Instance.Render();
    }
    
    private void EventStatusOk() {
        iPrevTime = DateTime.Now.Ticks;
        iFadeOut = !iModel.StatusOk;
        iFadeTimer.Start();
    }
    
    private void FadeText(object aSender, EventArgs aArgs) {
        double t = TimeSpan.FromTicks(DateTime.Now.Ticks - iPrevTime).TotalSeconds;
        double delta = 255.0f * t * kFadeTime;
        //Trace.WriteLine(Trace.kLinnGuiPreamp, "ViewPreamp.FadeText: t=" + t + ", delta=" + delta);

        if(!iFadeOut) {
            if(iAlpha + delta > 255) {
                iAlpha = 255;
            } else {
                iAlpha += delta;
                iFadeTimer.Start();
            }
        } else {
            if(iAlpha - delta < 0) {
                iAlpha = 0;

            } else {
                iAlpha -= delta;
                iFadeTimer.Start();
            }
        }
        
        iVolume.Colour = new Colour((int)iAlpha, iVolume.Colour.R, iVolume.Colour.G, iVolume.Colour.B);
        
        iPrevTime = DateTime.Now.Ticks;
        Renderer.Instance.Render();
    }
    
    //private Node iRoot = null;
    //private IControllerPreamp iController = null;
    private Room iModel;
    private NodeText iVolume;
    private const float kFadeTime = 2.0f;
    private Timer iFadeTimer;
    private double iAlpha = 255;
    private bool iFadeOut = false;
    private long iPrevTime = 0;
}
    
} // Kinsky
} // Linn
