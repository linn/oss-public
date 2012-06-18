using System.Drawing;
using System.Windows.Forms;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System.Drawing.Drawing2D;
using Linn.Gui;

namespace Linn {
namespace Gui {
namespace Editor {

public sealed class RendererGdiEditor : RendererGdiPlus
{
    public RendererGdiEditor(EditorCanvas aCanvas) : base(aCanvas) {
        Trace.WriteLine(Trace.kKinskyEd, ">RendererGdiEditor");
        new EditorFactory();
        new EditorMessageFactory();
        iBackgroundBrush = new TextureBrush(new Bitmap(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Editor/Resources/Background.bmp")));
        iBackgroundBrush.WrapMode = System.Drawing.Drawing2D.WrapMode.Tile;
    }
    
    public override void BeginFrame() {
        base.BeginFrame();
        iRenderTarget.FillRectangle(iBackgroundBrush, iRenderTarget.ClipBounds.X, iRenderTarget.ClipBounds.Y, iRenderTarget.ClipBounds.Width, iRenderTarget.ClipBounds.Height);
    }
    
    public void DrawSelectedNode(Node aNode) {
        if(aNode.Active) {
            NodeHit node = aNode as NodeHit;
            if(node != null) {
                if((node.Width - 1) < 1 || (node.Height - 1) < 1) {
                    return;
                }
                iRenderTarget.DrawRectangle(Pens.Red,
                    (int)node.WorldSrt.Translation.X, (int)node.WorldSrt.Translation.Y,
                    (int)node.Width - 1, (int)node.Height - 1);
            } else {
                iRenderTarget.FillEllipse(Brushes.Red,
                    (int)(aNode.WorldSrt.Translation.X - 5), (int)(aNode.WorldSrt.Translation.Y - 5), 10, 10);
            }
        }
    }
    
    private TextureBrush iBackgroundBrush;
}

} // Editor
} // Gui
} // Linn
