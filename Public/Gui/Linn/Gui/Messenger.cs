using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using System;

namespace Linn {
namespace Gui {

public delegate void DGuiEvent(Message aMessage);
public interface IMessengerObserver
{
    void Receive(Message aMessage);
}

public class Messenger
{
    public static Messenger Instance {
        get {
            return iInstance;
        }
    }
    
    public Messenger() {
        if(iInstance != null) {
            throw new SingletonAlreadyExists();
        }
        iInstance = this;
    }
    
    public event DGuiEvent EEventAppMessage {
        add {
            iEventAppMessage += value;
        }
        remove {
            iEventAppMessage -= value;
        }
    }
    
    public Node Root {
        get {
            return iRoot;
        }
        set {
            iRoot = value;
        }
    }
    
    public virtual void ApplicationMessage(Message aMessage) {
        if(iEventAppMessage != null) {
            iEventAppMessage(aMessage);
        }
    }
    
    public virtual void PresentationMessage(Message aMessage) {
        if(iRoot != null) {
            iRoot.Receive(aMessage);
        }
    }
    
    private static Messenger iInstance;
    private Node iRoot = null;
    private event DGuiEvent iEventAppMessage;
}

public class MessengerGdi : Messenger
{
    public MessengerGdi(Canvas aCanvas) : base() {
        iCanvas = aCanvas;
    }
    
    delegate void MessageCallback(Message aMessage);
    public override void PresentationMessage(Message aMessage) {
        if(iCanvas.InvokeRequired) {    
            MessageCallback d = new MessageCallback(PresentationMessage);
            iCanvas.BeginInvoke(d, new object[] {aMessage});
        } else {
            base.PresentationMessage(aMessage);
        }
    }
    
    public override void ApplicationMessage(Message aMessage) {
        if(iCanvas.InvokeRequired) {    
            MessageCallback d = new MessageCallback(ApplicationMessage);
            iCanvas.BeginInvoke(d, new object[] {aMessage});
        } else {
            base.ApplicationMessage(aMessage);
        }
    }
    
    private Canvas iCanvas  = null;
}
    
} // Gui
} // Linn
