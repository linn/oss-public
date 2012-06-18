using Linn;
using Linn.Gui;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;

namespace Linn {
namespace Gui {
namespace Editor {
    
public sealed class EditorFactory
{
    public EditorFactory() {
        if(iInstance != null) {
            throw new SingletonAlreadyExists();
        }
        iInstance = this;
    }
    
    public static EditorFactory Instance {
        get {
            if(iInstance == null) {
                throw new SingletonDoesntExist();
            }
            return iInstance;
        }
    }
    
    public EditorPlugin Create(Plugin aPlugin, IPluginObserver aObserver) {
        return Create(aPlugin, aPlugin, aObserver);
    }
    
    public EditorPlugin Create(Plugin aOwner, Plugin aPlugin, IPluginObserver aObserver) {
        EditorPlugin newEditor = null;
        if(aPlugin as NodeText != null) {
            newEditor = new EditorNodeText(aOwner, aPlugin, aObserver);
        } else if(aPlugin as NodeList != null) {
            newEditor = new EditorNodeList(aOwner, aPlugin, aObserver);
        } else if(aPlugin as NodeInput != null) {
            newEditor = new EditorNodeInput(aOwner, aPlugin, aObserver);
        } else if(aPlugin as NodeSlider != null) {
            newEditor = new EditorNodeSlider(aOwner, aPlugin, aObserver);
        } else if(aPlugin as NodePolygon != null) {
            newEditor = new EditorNodePolygon(aOwner, aPlugin, aObserver);
        } else if(aPlugin as NodeHit != null) {
            newEditor = new EditorNodeHit(aOwner, aPlugin, aObserver);
        } else if(aPlugin as Node != null) {
            newEditor = new EditorNode(aOwner, aPlugin, aObserver);
        } else if(aPlugin as Monostable!= null) {
            newEditor = new EditorMonostable(aOwner, aPlugin, aObserver);
        } else if(aPlugin as Bistable != null) {
            newEditor = new EditorBistable(aOwner, aPlugin, aObserver);
        } else if(aPlugin as Counter != null) {
            newEditor = new EditorCounter(aOwner, aPlugin, aObserver);
        } else if(aPlugin as TextureArray != null) {
            newEditor = new EditorTextureArray(aOwner, aPlugin, aObserver);
        }else if(aPlugin as TextureArrayFixed != null) {
            newEditor = new EditorTextureArrayFixed(aOwner, aPlugin, aObserver);
        }
        if(newEditor != null) {
            return newEditor;
        }
        throw new UnknownPlugin();
    }
    
    private static EditorFactory iInstance = null;
}


public sealed class EditorMessageFactory
{
    public EditorMessageFactory() {
        if(iInstance != null) {
            throw new SingletonAlreadyExists();
        }
        iInstance = this;
    }
    
    public static EditorMessageFactory Instance {
        get {
            if(iInstance == null) {
                throw new SingletonDoesntExist();
            }
            return iInstance;
        }
    }
    
    public EditorMessage Create(Message aMessage) {
        EditorMessage newEditorMsg = null;
        if(aMessage as MsgSetActive != null) {
            newEditorMsg = new EditorMsgSetActive(aMessage);
        } else if(aMessage as MsgHit != null) {
            newEditorMsg = new EditorMsgHit(aMessage);
        }/* else if(aMessage as MsgSetTexture != null) {
            newEditorMsg = new EditorMsgSetTexture(aMessage);
        } */else if(aMessage as MsgSetText != null) {
            newEditorMsg = new EditorMsgSetText(aMessage);
        } else if(aMessage as MsgStateChanged != null) {
            newEditorMsg = new EditorMsgStateChanged(aMessage);
        } else if(aMessage as MsgSetState!= null) {
            newEditorMsg = new EditorMsgSetState(aMessage);
        } else if(aMessage as MsgToggleState != null) {
            newEditorMsg = new EditorMsgToggleState(aMessage);
        } else if(aMessage as MsgCountStart != null) {
            newEditorMsg = new EditorMsgCountStart(aMessage);
        } else if(aMessage as MsgCountStop != null) {
            newEditorMsg = new EditorMsgCountStop(aMessage);
        } else if(aMessage as MsgCountEnd != null) {
            newEditorMsg = new EditorMsgCountEnd(aMessage);
        } else if(aMessage as MsgNext != null) {
            newEditorMsg = new EditorMsgNext(aMessage);
        } else if(aMessage as MsgPrevious != null) {
            newEditorMsg = new EditorMsgPrevious(aMessage);
        } else {
            newEditorMsg = new EditorMessage(aMessage);
        }
        if(newEditorMsg != null) {
            return newEditorMsg;
        }
        throw new UnknownPlugin();
    }
    
    public EditorMessage Create(string aMessage) {
        EditorMessage newEditorMsg = null;
        if(aMessage == "MsgSetActive") {
            newEditorMsg = new EditorMsgSetActive(new MsgSetActive());
        } else if(aMessage == "MsgHit") {
            newEditorMsg = new EditorMsgHit(new MsgHit());
        }/* else if(aMessage == "MsgSetTexture") {
            newEditorMsg = new EditorMsgSetTexture(new MsgSetTexture());
        } */else if(aMessage == "MsgSetText") {
            newEditorMsg = new EditorMsgSetText(new MsgSetText());
        } else if(aMessage == "MsgStateChanged") {
            newEditorMsg = new EditorMsgStateChanged(new MsgStateChanged());
        } else if(aMessage == "MsgSetState") {
            newEditorMsg = new EditorMsgSetState(new MsgSetState());
        } else if(aMessage == "MsgToggleState") {
            newEditorMsg = new EditorMsgToggleState(new MsgToggleState());
        } else if(aMessage == "MsgCountStart") {
            newEditorMsg = new EditorMsgCountStart(new MsgCountStart());
        } else if(aMessage == "MsgCountStop") {
            newEditorMsg = new EditorMsgCountStop(new MsgCountStop());
        } else if(aMessage == "MsgCountEnd") {
            newEditorMsg = new EditorMsgCountEnd(new MsgCountEnd());
        } else if(aMessage == "MsgNext") {
            newEditorMsg = new EditorMsgNext(new MsgNext());
        } else if(aMessage == "MsgPrevious") {
            newEditorMsg = new EditorMsgPrevious(new MsgPrevious());
        }
        if(newEditorMsg != null) {
            return newEditorMsg;
        }
        throw new UnknownPlugin();
    }
    
    private static EditorMessageFactory iInstance = null;
}
    
} // Editor
} // Gui
} // Linn
