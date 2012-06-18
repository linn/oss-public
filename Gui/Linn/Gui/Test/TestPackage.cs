using Linn.TestFramework;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;

namespace Linn {
namespace Gui {

internal class SuitePackageLoadTests : Suite
{
    public SuitePackageLoadTests() : base("Package XML load tests") {
    }
    
    public override void Test() {
        string startupPath = System.IO.Path.GetDirectoryName(System.Environment.GetCommandLineArgs()[0]);
        Package package = new Package("TestPackage.xml");
        TEST(package.Filename == System.IO.Path.Combine(startupPath, "../../share/Linn/Test/Gui/TestPackage.xml"));
        TEST(package.Namespace == "TestNs");
        TEST(package.Type == "Layout");
        TEST(package.Root == null);
        TEST(package.PluginList.Count == 0);
        TEST(package.PackageList.Count == 0);
        
        package = PackageManager.Instance.Load("TestPackage2.xml");
        TEST(package.Namespace == "TestNs");
        TEST(package.Type == "Layout");
        TEST(package.Root != null);
        TEST(package.PluginList.Count == 3);
        TEST(package.PackageList.Count == 0);
        TEST(package.PluginItem(0).Name == "TestPlugin1");
        TEST(package.PluginItem(1).Name == "TestPlugin2");
        TEST(package.PluginItem(2).Name == "TestPlugin3");
        TEST(package.PluginItem(0).NextPlugin == null);
        TEST(package.PluginItem(1).NextPlugin.Name == "TestPlugin3");
        TEST(package.PluginItem(2).NextPlugin == null);
    }
}

internal class SuitePackageManipulationTests : Suite
{
    public SuitePackageManipulationTests() : base("Package manipulation tests") {
    }
    
    public override void Test() {
        Package package = new Package();
        Node n1 = new Node();
        Node n2 = new Node();
        Node n3 = new Node();
        Node n4 = new Node();
        n1.AddChild(n2);
        n2.AddChild(n3);
        n2.AddChild(n4);
        package.AddNode(n1);
        TEST(package.PluginList.Count == 4);
        TEST(n4.Parent == n2);
        package.DeleteNode(n4);
        TEST(n4.Parent == null);
        TEST(package.PluginList.Count == 3);
        package.DeleteNode(n1);
        TEST(package.PluginList.Count == 0);
    }
}

class TestProgram
{
    public static void Main(string[] aArgs) {
        Helper helper = new Helper(aArgs);
        OptionParser optParser = helper.OptionParser;
        helper.ProcessCommandLine();
        
        if (optParser.PosArgs.Count != 0) {
            System.Console.WriteLine(optParser.Help());
            return;
        }
        
        new RendererNull();
        string startupPath = System.IO.Path.GetDirectoryName(System.Environment.GetCommandLineArgs()[0]);
        PackageManager.Instance.AddPath(System.IO.Path.Combine(startupPath, "../../share/Linn/Test/Gui"));
        Runner runner = new Runner("Package tests");
        runner.Add(new SuitePackageLoadTests());
        runner.Add(new SuitePackageManipulationTests());
        runner.Run();
        
        helper.Dispose();
    }
}

} // Gui
} // Linn
