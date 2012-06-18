using System;
using System.Windows.Forms;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System.ComponentModel;
using Linn.Gui.Editor.UndoRedo;
using System.Collections.Generic;

namespace Linn {
namespace Gui {
namespace Editor {
    
internal class NodeHitConverter : StringConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
        return true;
    }
    
    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
        List<string> list = new List<string>();
        foreach(Package pkg in PackageManager.Instance.Packages) {
            foreach(Plugin p in pkg.PluginList) {
                if(p as NodeHit != null) {
                    list.Add(p.Fullname);
                }
            }
        }
        return new StandardValuesCollection(list.ToArray());
    }
}
    
internal class EditorNodeSlider : EditorNodePolygon
{
    public EditorNodeSlider(Plugin aOwner, Plugin aEditPlugin, IPluginObserver aObserver) : base(aOwner, aEditPlugin, aObserver) {
    }
        
    [CategoryAttribute("NodeSlider properties"),
     DescriptionAttribute("The slider texture.")]
    public string SliderTexture {
        get {
            return ((NodeSlider)iPlugin).SliderTexture.Name;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandSliderTextureChange((NodeSlider)iPlugin, value));
            Renderer.Instance.Render();
        }
    }
    
    [CategoryAttribute("NodeSlider properties"),
     DescriptionAttribute("The slider position.")]
    public float Position {
        get {
            return ((NodeSlider)iPlugin).Position;
        }
        set {
            float position = value;
            if(value < 0) {
                position = 0;
            } else if(value > 1) {
                position = 1;
            }
            UndoRedoManager.Instance.Commit(new CommandSliderPositionChange((NodeSlider)iPlugin, position));
        }
    }
    
    [CategoryAttribute("NodeSlider properties"),
     DescriptionAttribute("The slider orientation.")]
    public NodeSlider.EOrientation Orientation {
        get {
            return ((NodeSlider)iPlugin).Orientation;
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandSliderOrientationChange((NodeSlider)iPlugin, value));
        }
    }
    
    [CategoryAttribute("NodeSlider properties"),
     DescriptionAttribute("The slider indicator."),
     TypeConverter(typeof(NodeHitConverter))]
    public string IndicatorNode {
        get {
            if(((NodeSlider)iPlugin).IndicatorNode != null) {
                return ((NodeSlider)iPlugin).IndicatorNode.Fullname;
            } else {
                return "";
            }
        }
        set {
            UndoRedoManager.Instance.Commit(new CommandSliderIndicatorNodeChange((NodeSlider)iPlugin, value));
        }
    }
}

} // Linn
} // Gui
} // Linn
