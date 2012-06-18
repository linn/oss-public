using Linn;
using Linn.Gui;
using Linn.Kinsky;
using Linn.Gui.Resources;
using System;
using System.Windows.Forms;

internal class ViewApplicationCanvas : IDisposable
{
    public ViewApplicationCanvas(ApplicationCanvas aForm) {
        iForm = aForm;
        iKeyBindings = new KeyBindings();
        Messenger.Instance.EEventAppMessage += Receive;
    }
    
    public void Dispose() {
        Messenger.Instance.EEventAppMessage -= Receive;
    }
    
    public virtual void Receive(Linn.Gui.Resources.Message aMessage) {
        if(aMessage.Fullname == "Main.ExitButton.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == false && stateMsg.OldState == true) {
                    iForm.Close();
                }
            }
        }
        if(aMessage.Fullname == "Main.MinimiseButton.Monostable") {
            MsgStateChanged stateMsg = aMessage as MsgStateChanged;
            if(stateMsg != null) {
                if(stateMsg.NewState == false && stateMsg.OldState == true) {
                    iForm.WindowState = System.Windows.Forms.FormWindowState.Minimized;
                }
            }
        }
        /*
        if(aMessage.Fullname == "") {
            MsgKeyDown keyDown = aMessage as MsgKeyDown;
            if(keyDown != null) {
                string action = iKeyBindings.Action(keyDown.Key);
                Trace.WriteLine(Trace.kLinnGui, "ViewApplicationCanvas.Receive: keyDown->action=" + action);
                if(action == "Play") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("ControlPanel.Play.Monostable", true));
                    Renderer.Instance.Render();
                } else if(action == "Pause") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("ControlPanel.Pause.Monostable", true));
                    Renderer.Instance.Render();
                } else if(action == "Stop") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("ControlPanel.Stop.Monostable", true));
                    Renderer.Instance.Render();
                } else if(action == "Previous") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("ControlPanel.Skip Backward.Monostable", true));
                    Renderer.Instance.Render();
                } else if(action == "Next") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("ControlPanel.Skip Forward.Monostable", true));
                    Renderer.Instance.Render();
                } else if(action == "Shuffle") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("Main.Shuffle.Monostable", true));
                    Renderer.Instance.Render();
                } else if(action == "Repeat") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("Main.Repeat.Monostable", true));
                    Renderer.Instance.Render();
                } else if(action == "Eject") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("CurrentPlaylist.DeleteAll.Monostable", true));
                    Renderer.Instance.Render();
                } else if(action == "VolumeDown") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("ControlPanel.VolumeDown.Monostable", true));
                    Renderer.Instance.Render();
                } else if(action == "VolumeUp") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("ControlPanel.VolumeUp.Monostable", true));
                    Renderer.Instance.Render();
                } else if(action == "Mute") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("ControlPanel.Mute.Monostable", true));
                    Renderer.Instance.Render();
                } else if(action == "DeleteTrack") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("CurrentPlaylist.RemoveButton.Monostable", true));
                    Renderer.Instance.Render();
                } else if(action == "DeleteAllTracks") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("CurrentPlaylist.DeleteAll.Monostable", true));
                    Renderer.Instance.Render();
                } else if(action == "SwitchView") {
                    Messenger.Instance.PresentationMessage(new MsgToggleState("StatusBar.Room.Bistable"));
                    Renderer.Instance.Render();
                } else if(action == "InsertMode") {
                    Messenger.Instance.PresentationMessage(new MsgToggleState("Library.InsertAtEnd.Bistable"));
                    Renderer.Instance.Render();
                } else if(action == "Insert") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("Library.Insert.Monostable", true));
                    Renderer.Instance.Render();
                } else if(keyDown.Key == Keys.F1) {
                    System.Console.WriteLine("Simulating stack stop");
                    //iForm.Stop();
                } else if(keyDown.Key == Keys.F2) {
                    System.Console.WriteLine("Simulating stack start");
                    //iForm.Start();
                } else if(keyDown.Key == Keys.F3) {
                    System.Console.WriteLine("Simulating stack stop/start");
                    //iForm.Stop();
                    //iForm.Start();
                }
            }
            MsgKeyUp keyUp = aMessage as MsgKeyUp;
            if(keyUp != null) {
                string action = iKeyBindings.Action(keyUp.Key);
                Trace.WriteLine(Trace.kLinnGui, "ViewApplicationCanvas.Receive: keyUp->action=" + action);
                if(action == "Play") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("ControlPanel.Play.Monostable", false));
                    Renderer.Instance.Render();
                } else if(action == "Pause") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("ControlPanel.Pause.Monostable", false));
                    Renderer.Instance.Render();
                } else if(action == "Stop") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("ControlPanel.Stop.Monostable", false));
                    Renderer.Instance.Render();
                } else if(action == "Previous") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("ControlPanel.Skip Backward.Monostable", false));
                    Renderer.Instance.Render();
                } else if(action == "Next") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("ControlPanel.Skip Forward.Monostable", false));
                    Renderer.Instance.Render();
                } else if(action == "Shuffle") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("Main.Shuffle.Monostable", false));
                    Renderer.Instance.Render();
                } else if(action == "Repeat") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("Main.Repeat.Monostable", false));
                    Renderer.Instance.Render();
                } else if(action == "Eject") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("CurrentPlaylist.DeleteAll.Monostable", false));
                    Renderer.Instance.Render();
                } else if(action == "VolumeDown") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("ControlPanel.VolumeDown.Monostable", false));
                    Renderer.Instance.Render();
                } else if(action == "VolumeUp") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("ControlPanel.VolumeUp.Monostable", false));
                    Renderer.Instance.Render();
                } else if(action == "Mute") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("ControlPanel.Mute.Monostable", false));
                    Renderer.Instance.Render();
                } else if(action == "DeleteTrack") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("CurrentPlaylist.RemoveButton.Monostable", false));
                    Renderer.Instance.Render();
                } else if(action == "DeleteAllTracks") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("CurrentPlaylist.DeleteAll.Monostable", false));
                    Renderer.Instance.Render();
                } else if(action == "Insert") {
                    Messenger.Instance.PresentationMessage(new MsgSetState("Library.Insert.Monostable", false));
                    Renderer.Instance.Render();
                }
            }
        }
        */
    }
    
    private KeyBindings iKeyBindings = null;
    private ApplicationCanvas iForm = null;
}
