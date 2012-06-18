using System.Windows.Forms;

namespace KinskyDesktop.Widgets
{
    [System.ComponentModel.DesignerCategory("")]

    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            DoubleBuffered = true;
        }
    }
}
