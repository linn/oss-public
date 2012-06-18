using Linn.TestFramework;
using Linn.Gui.Scenegraph;
using System;

namespace Linn {
namespace Gui {
    
internal class SuiteCommandTests : Suite
{
    public SuiteCommandTests() : base("Command tests") {
    }
    
    public override void Test() {
        NodeInput n = new NodeInput();
        
        /*n.Hit(new Vector3d(238,298,0));
        n.Motion(new Vector3d(158,148,0));
        n.Motion(new Vector3d(182,272,0));
        n.UnHit();*/
        
        n.Hit(new Vector3d(0, 0, 0));
        for(int i = 0; i < 360; i += 3) {
            n.Motion(new Vector3d((float)Math.Cos(i * Math.PI / 180.0f) * 10, (float)Math.Sin(i * Math.PI / 180.0f) * 10, 0.0f));
        }
        n.UnHit();
        
        /*n.Hit(new Vector3d(0, 0, 0));
        for(int i = 360; i > 0; i -= 1) {
            n.Motion(new Vector3d((float)Math.Cos(i * Math.PI / 180.0f) * 10, (float)Math.Sin(i * Math.PI / 180.0f) * 10, 0.0f));
        }
        n.UnHit();
        
        /*n.Hit(new Vector3d(0, 0, 0));
        n.Motion(new Vector3d(1, 0, 0));
        n.Motion(new Vector3d(2, 1, 0));
        n.UnHit();
        
        n.Hit(new Vector3d(2, 1, 0));
        n.Motion(new Vector3d(1, 0, 0));
        n.Motion(new Vector3d(0, 0, 0));
        n.UnHit();*/
    }
}

class Program {
    public static void Main() {
        new RendererNull();
        Runner runner = new Runner("NodeInput tests");
        runner.Add(new SuiteCommandTests());
        runner.Run();
    }
}

} // Gui
} // Linn
