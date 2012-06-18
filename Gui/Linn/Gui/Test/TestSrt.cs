using Linn.TestFramework;
using Linn.Gui.Scenegraph;

namespace Linn {
namespace Gui {

internal class SuiteVector3dTests : Suite
{
    public SuiteVector3dTests() : base("Vector3d tests") {
    }

    public override void Test() {
        // set constructors
        Vector3d v1 = new Vector3d();
        TEST(v1.X == 0 && v1.Y == 0 && v1.Z == 0);
        Vector3d v2 = new Vector3d(5, 7, 8);
        TEST(v2.X == 5 && v2.Y == 7 && v2.Z == 8);
        
        // test + operator
        Vector3d v3 = new Vector3d(10, 15, 20);
        Vector3d v4 = v3 + v2;
        TEST(v2.X == 5 && v2.Y == 7 && v2.Z == 8);
        TEST(v3.X == 10 && v3.Y == 15 && v3.Z == 20);
        TEST(v4.X == 15 && v4.Y == 22 && v4.Z == 28);
        
        // test - operator
        Vector3d v5 = v3 - v2;
        TEST(v2.X == 5 && v2.Y == 7 && v2.Z == 8);
        TEST(v3.X == 10 && v3.Y == 15 && v3.Z == 20);
        TEST(v5.X == 5 && v5.Y == 8 && v5.Z == 12);
        
        // test float * operator
        Vector3d v6 = v3 * 2.0f;
        TEST(v6.X == 20 && v6.Y == 30 && v6.Z == 40);
        TEST(v3.X == 10 && v3.Y == 15 && v3.Z == 20);
        
        // test dot product
        TEST((v3 * v2) == 315.0f);
        TEST(v2.X == 5 && v2.Y == 7 && v2.Z == 8);
        TEST(v3.X == 10 && v3.Y == 15 && v3.Z == 20);
        
        // test float / operator
        Vector3d v7 = v2 / 10.0f;
        TEST(v7.X == 0.5f && v7.Y == 0.7f && v7.Z == 0.8f);
        TEST(v2.X == 5 && v2.Y == 7 && v2.Z == 8);
        
        // test Size()
        TEST((v7.Size() - 1.1747f) < 0.0001f);
        TEST((v6.Size() - 53.8516f) < 0.001f);
        
        TEST(v2.Normalise() == true);
        TEST(v2 == new Vector3d(0.42563f, 0.59588f, 0.68101f));
        TEST(v1.Normalise() == false);
        
        // test cross product
        TEST((v5 ^ v6) == new Vector3d(-40, 40, -10));
        Vector3d v8 = new Vector3d(1, 0, 0);
        Vector3d v9 = new Vector3d(0, 1, 0);
        TEST((v8 ^ v9) == new Vector3d(0, 0, 1));
        TEST((v9 ^ v8) == new Vector3d(0, 0, -1));
    }
}

class TestProgram
{
    public static void Main() {
        Runner runner = new Runner("Scale/Rotation/Translation tests");
        runner.Add(new SuiteVector3dTests());
        runner.Run();
    }
}

} // Gui
} // Linn
