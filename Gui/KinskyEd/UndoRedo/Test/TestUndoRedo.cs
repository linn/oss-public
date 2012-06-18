using Linn.TestFramework;
using Linn.Gui.UndoRedo;

namespace Linn {
namespace Gui {

internal class SuiteBasicTests : Suite
{
    public SuiteBasicTests() : base("Basic tests") {
    }
    
    public override void Setup() {
        UndoRedoManager.Instance.MaxHistorySize = 20;
    }
    
    public override void TearDown() {
        UndoRedoManager.Instance.FlushHistory();
    }

    public override void Test() {
        UndoRedo<int> i = new UndoRedo<int>(0);
        
        UndoRedoManager.Instance.Start("");
        i.Value = 1;
        UndoRedoManager.Instance.Commit();
        TEST(i.Value == 1);
        TEST(UndoRedoManager.Instance.CanUndo == true);
        TEST(UndoRedoManager.Instance.CanRedo == false);
        
        UndoRedoManager.Instance.Undo();
        TEST(i.Value == 0);
        TEST(UndoRedoManager.Instance.CanUndo == false);
        TEST(UndoRedoManager.Instance.CanRedo == true);
        
        UndoRedoManager.Instance.Redo();
        TEST(i.Value == 1);
    }
}

internal class SuiteFlushTests : Suite
{
    public SuiteFlushTests() : base("Flush history tests") {
    }
    
    public override void Setup() {
        UndoRedoManager.Instance.MaxHistorySize = 20;
    }
    
    public override void TearDown() {
        UndoRedoManager.Instance.FlushHistory();
    }

    public override void Test() {
        UndoRedo<int> i = new UndoRedo<int>(0);
        
        // Start and commit before flushing
        UndoRedoManager.Instance.Start("");
        i.Value = 1;
        UndoRedoManager.Instance.Commit();
        TEST(i.Value == 1);
        
        UndoRedoManager.Instance.FlushHistory();
        TEST(UndoRedoManager.Instance.CanUndo == false);
        TEST(UndoRedoManager.Instance.CanRedo == false);
        TEST(i.Value == 1);
        
        // Start then flush
        UndoRedoManager.Instance.Start("");
        i.Value = 2;
        UndoRedoManager.Instance.FlushHistory();
        TEST(i.Value == 2);
        TEST(UndoRedoManager.Instance.CanUndo == false);
        TEST(UndoRedoManager.Instance.CanRedo == false);
    }
}

internal class SuiteCancelTests : Suite
{
    public SuiteCancelTests() : base("Cancel tests") {
    }
    
    public override void Setup() {
        UndoRedoManager.Instance.MaxHistorySize = 20;
    }
    
    public override void TearDown() {
        UndoRedoManager.Instance.FlushHistory();
        UndoRedoManager.Instance.MaxHistorySize = 0;
    }

    public override void Test() {
        UndoRedo<int> i = new UndoRedo<int>(0);
        UndoRedoList<int> list = new UndoRedoList<int>(new int[] {1, 2, 3});
        //UndoRedoDictionary<int, string> dict = new UndoRedoDictionary<int, string>();

        UndoRedoManager.Instance.Start("");
        i.Value = 1;
        list.Add(4);
        //dict[1] = "One";
        UndoRedoManager.Instance.Cancel();

        TEST(i.Value == 0);
        TEST(list.Count == 3);
        //TEST(dict.ContainsKey(1) == false);

        // ensure future commits still work
        UndoRedoManager.Instance.Start("");
        i.Value = 1;
        UndoRedoManager.Instance.Commit();
        TEST(i.Value == 1);
    }
}

class TestProgram
{
    public static void Main() {
        Runner runner = new Runner("Undo/Redo framework tests");
        runner.Add(new SuiteBasicTests());
        runner.Add(new SuiteFlushTests());
        runner.Add(new SuiteCancelTests());
        runner.Run();
    }
}

} // Gui
} // Linn
