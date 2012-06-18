using System;
using System.Windows.Forms;
using Linn.Gui.Resources;
using System.ComponentModel;
using Linn.Gui.Editor.UndoRedo;

namespace Linn {
namespace Gui {
namespace Editor {
    
internal class EditorMonostable : EditorPlugin
{
    public EditorMonostable(Plugin aOwner, Plugin aEditPlugin, IPluginObserver aObserver) : base(aOwner, aEditPlugin, aObserver) {
    }
    
    [CategoryAttribute("Monostable properties"),
     DescriptionAttribute("The period in seconds.")]
    public float Period {
        get {
            return ((Monostable)iPlugin).Period;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandPeriodChange((Monostable)iPlugin, value));
        }
    }
}

internal class EditorBistable : EditorPlugin
{
    public EditorBistable(Plugin aOwner, Plugin aEditPlugin, IPluginObserver aObserver) : base(aOwner, aEditPlugin, aObserver) {
    }
    
    [CategoryAttribute("Bistable properties"),
     DescriptionAttribute("The starting state.")]
    public bool State {
        get {
            return ((Bistable)iPlugin).State;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandStateChange((Bistable)iPlugin, value));
        }
    }
}

internal class EditorCounter : EditorPlugin
{
    public EditorCounter(Plugin aOwner, Plugin aEditPlugin, IPluginObserver aObserver) : base(aOwner, aEditPlugin, aObserver) {
    }

    [CategoryAttribute("Counter properties"),
     DescriptionAttribute("The number of counts per second.")]
    public float CountsPerSecond {
        get {
            return ((Counter)iPlugin).CountsPerSecond;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandCountsPerSecondChange((Counter)iPlugin, value));
        }
    }

    [CategoryAttribute("Counter properties"),
     DescriptionAttribute("Whether to loop.")]
    public bool Loop {
        get {
            return ((Counter)iPlugin).Loop;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandLoopChange((Counter)iPlugin, value));
        }
    }

    [CategoryAttribute("Counter properties"),
     DescriptionAttribute("The maximum number to count to.")]
    public int MaxCount {
        get {
            return ((Counter)iPlugin).MaxCount;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandMaxCountChange((Counter)iPlugin, value));
            ((Counter)iPlugin).MaxCount = value;
            Renderer.Instance.Render();
        }
    }
    
    [CategoryAttribute("Counter properties"),
     DescriptionAttribute("The current count.")]
    public int Count {
        get {
            return ((Counter)iPlugin).Count;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandCountChange((Counter)iPlugin, value));
            ((Counter)iPlugin).Count = value;
            Renderer.Instance.Render();
        }
    }
    
    [CategoryAttribute("Counter properties"),
     DescriptionAttribute("Whether counter is currently counting.")]
    public bool Counting {
        get {
            return ((Counter)iPlugin).Counting;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandCountingChange((Counter)iPlugin, value));
        }
    }
}

} // Editor
} // Gui
} // Linn
