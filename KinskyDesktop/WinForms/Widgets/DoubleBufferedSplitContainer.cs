using System.Windows.Forms;

namespace KinskyDesktop.Widgets
{
    [System.ComponentModel.DesignerCategory("")]

    public class DoubleBufferSplitContainer : SplitContainer
    {
        public DoubleBufferSplitContainer()
        {
            DoubleBuffered = true;
        }
    }
}
