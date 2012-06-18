using System;

using Linn;
using Linn.TestFramework;


class TestObject
{
    public override string ToString() {
        return "TestObject";
    }
}

class TestTraceListener : TraceListener
{
    public override void Write(string aMsg) {
        iMsg = aMsg;
    }
    public override void WriteLine(string aMsg) {
        iMsg = aMsg + "\n";
    }
    public string Msg {
        get { return iMsg; }
        set { iMsg = value; }
    }
    private string iMsg = null;
}

class SuiteDebug : Suite
{
    internal SuiteDebug() : base("Tests for Debug functions") {
    }

    public override void Test() {
        TestTraceListener t = new TestTraceListener();
        Trace.Clear();
        Trace.AddListener(t);

        Linn.Trace.Level = Linn.Trace.kAll;
        Linn.Trace.Write(Linn.Trace.kTest, "TestString");
        TEST(t.Msg == "TestString");
        Linn.Trace.Write(Linn.Trace.kTest, new TestObject());
        TEST(t.Msg == "TestObject");
        Linn.Trace.WriteLine(Linn.Trace.kTest, "TestString");
        TEST(t.Msg == "TestString\n");
        Linn.Trace.WriteLine(Linn.Trace.kTest, new TestObject());
        TEST(t.Msg == "TestObject\n");

        // different trace levels
        t.Msg = null;
        Linn.Trace.Level = Linn.Trace.kNone;
        Linn.Trace.Write(Linn.Trace.kTest, "TestString");
        TEST(t.Msg == null);
        Linn.Trace.Write(Linn.Trace.kTest, new TestObject());
        TEST(t.Msg == null);
        Linn.Trace.WriteLine(Linn.Trace.kTest, "TestString");
        TEST(t.Msg == null);
        Linn.Trace.WriteLine(Linn.Trace.kTest, new TestObject());
        TEST(t.Msg == null);

        Linn.Trace.Level = Linn.Trace.kUpnp;
        Linn.Trace.Write(Linn.Trace.kTest, "TestString");
        TEST(t.Msg == null);
        Linn.Trace.Write(Linn.Trace.kTest, new TestObject());
        TEST(t.Msg == null);
        Linn.Trace.WriteLine(Linn.Trace.kTest, "TestString");
        TEST(t.Msg == null);
        Linn.Trace.WriteLine(Linn.Trace.kTest, new TestObject());
        TEST(t.Msg == null);

        Linn.Trace.Write(Linn.Trace.kUpnp, "TestString");
        TEST(t.Msg == "TestString");
        Linn.Trace.Write(Linn.Trace.kUpnp, new TestObject());
        TEST(t.Msg == "TestObject");
        Linn.Trace.WriteLine(Linn.Trace.kUpnp, "TestString");
        TEST(t.Msg == "TestString\n");
        Linn.Trace.WriteLine(Linn.Trace.kUpnp, new TestObject());
        TEST(t.Msg == "TestObject\n");
    }
}

class TestDebug
{
    static void Main(string[] aArgs) {
        Helper helper = new Helper(aArgs);
        helper.ProcessCommandLine();

        Runner runner = new Runner("Debug module tests");
        runner.Add( new SuiteDebug() );
        runner.Run();

        helper.Dispose();
    }
}


