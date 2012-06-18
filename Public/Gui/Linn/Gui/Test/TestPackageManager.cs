using Linn.TestFramework;
using Linn.Gui.Resources;

namespace Linn {
namespace Gui {
    
internal class SuiteInstanceTests : Suite
{
    public SuiteInstanceTests() : base("Instance tests") {
    }
    
    public override void Test() {
        PackageManager man;
        man = PackageManager.Instance;
        TEST(man.PathList.Count == 1);
        TEST(man.PathList[0] == "");
        TEST(man == PackageManager.Instance);
    }
}

internal class SuitePathSearchingTests : Suite
{
    public SuitePathSearchingTests() : base("Path searching tests") {
    }
    
    public override void Test() {
    }
}

public class TestProgram
{
    public static void Main() {
        Runner runner = new Runner("PackageManager tests");
        runner.Add(new SuiteInstanceTests());
        runner.Add(new SuitePathSearchingTests());
        runner.Run();
    }
}

} // Gui
} // Linn
