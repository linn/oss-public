using Linn.TestFramework;
using Linn.Gui.Scenegraph;
using System.Threading;
using System;
using Linn.Gui.Resources;

namespace Linn {
namespace Gui {
    
/*internal class SuiteCentreOnIndexTests : Suite
{
    public SuiteCentreOnIndexTests() : base("CentreOnIndex") {
    }
    
    public override void Test() {
    }
}*/

class Program {
    public static void Main() {
        new RendererNull();
        Runner runner = new Runner("NodeList tests");
        //runner.Add(new SuiteCentreOnIndexTests());
        runner.Run();
    }
}

} // Gui
} // Linn
