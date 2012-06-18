using Linn;
using Linn.TestFramework;
using Linn.Gui.Resources;
using System.Collections;

class TestCollection : CollectionBase
{
    public int this[int index] {
        get {
            return (int)List[index];
        }
        set {
            List[index] = value;
        }
    }
    
    public int Add(int value) {
        return List.Add(value);
    }
    
    public void Remove(int value) {
        List.Remove(value);
    }
   
    protected override void OnInsert(int index, object value) {
        Trace.WriteLine(Trace.kGui, "OnInsert(" + index + ")");
        iInsert++;
    }
    
    protected override void OnRemove(int index, object value) {
        Trace.WriteLine(Trace.kGui, "OnRemove(" + index + ")");
        iInsert--;
    }
    
    public int Insert {
        get {
            return iInsert;
        }
    }
    
    int iInsert = 0;
}

class TestObject
{
    public TestCollection Collection {
        get {
            return iCollection;
        }
        set {
            iCollection = value;
        }
    }
    
    private TestCollection iCollection = new TestCollection();
}

public sealed class SuiteEditorTranslatorList : Suite
{
    public SuiteEditorTranslatorList() : base("EditorTranslatorList class tests") {
    }

    public override void Test() {
        TestObject obj = new TestObject();
        TEST(obj.Collection.Insert == 0);
        obj.Collection.Add(4);
        TEST(obj.Collection.Insert == 1);
        obj.Collection.Add(5);
        obj.Collection.Add(6);
        obj.Collection.Add(7);
        TEST(obj.Collection.Insert == 4);
        obj.Collection.Remove(7);
        TEST(obj.Collection.Insert == 3);
        obj.Collection.Remove(6);
        obj.Collection.Remove(5);
        obj.Collection.Remove(4);
        TEST(obj.Collection.Insert == 0);
        
    }
}

public sealed class TestEditorTranslatorList
{
    public static void Main(string[] aArgs) {
        Helper helper = new Helper(aArgs);
        helper.ProcessCommandLine();

        Runner runner = new Runner("CollectorBase tests");
        runner.Add(new SuiteEditorTranslatorList());
        runner.Run();
        
        helper.Dispose();
    }
}
