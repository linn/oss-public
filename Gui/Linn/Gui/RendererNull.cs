using Linn.Gui.Resources;
using System.Drawing;
using Linn.Gui.Scenegraph;

namespace Linn {
namespace Gui {

public sealed class RendererNull : Renderer
{
    public RendererNull() {
        new TextureManagerNull();
        new Messenger();
    }
    
    public override RenderStats Stats {
        get {
            return new RenderStats();
        }
    }
    
    public override void Lock() {}
    public override void Unlock() {}
    public override void BeginFrame() {}
    public override void EndFrame() {}
    public override void Render() {}
    public override void Render(bool aForce) {}
    public override void Render(Rectangle aRect) {}
    public override void Render(Rectangle aRect, bool aForce) {}
    public override void DrawPolygon(NodePolygon aPolygon) {}
    public override void DrawText(NodeText aText) {}
}

} // Gui
} // Linn
