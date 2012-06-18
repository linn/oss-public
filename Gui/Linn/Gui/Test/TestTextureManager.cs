using Linn.TestFramework;
using Linn.Gui.Resources;

namespace Linn {
namespace Gui {
    
internal class SuiteInstanceTests : Suite
{
    public SuiteInstanceTests() : base("Instance tests") {
    }
    
    public override void Test() {
        TextureManager man;
        bool success = false;
        try {
            man = TextureManager.Instance;
        } catch(Linn.AssertionError) {
            success = true;
        } catch(System.Exception) {
        }
        TEST(success);
        man = new TextureManagerNull();
        TEST(man.PathList.Count == 1);
        TEST(man.PathList[0] == "");
        TEST(man == TextureManager.Instance);
        success = false;
        try {
            man = new TextureManagerNull();
        } catch(SingletonAlreadyExists) {
            success = true;
        } catch(System.Exception) {
        }
        TEST(success);
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
        Runner runner = new Runner("TextureManager tests");
        runner.Add(new SuiteInstanceTests());
        runner.Add(new SuitePathSearchingTests());
        runner.Run();
    }
}

} // Gui
} // Linn
