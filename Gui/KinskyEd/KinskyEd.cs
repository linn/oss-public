using System;
using System.Windows.Forms;

namespace Linn {
namespace Gui {
namespace Editor {
    
public class Progam
{
    [STAThread]
    public static void Main(string[] aArgs) {
        Helper helper = new Helper(aArgs);

        helper.ProcessCommandLine();
        Application.Run(new EditorForm());
        helper.Dispose();
    }
}
    
} // Editor
} // Gui
} // Linn
