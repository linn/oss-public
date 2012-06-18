
using System;
using System.Threading;
using System.Timers;
using Linn;
using Linn.TestFramework;


class SuiteTimer : Suite
{
    public SuiteTimer() : base("Tests for Linn C# timer") {
    }

    public override void Test() {
        Linn.Timer timer = null;

        // Default construction
        timer = new Linn.Timer();
        TEST(timer.AutoReset == true);
        TEST(timer.Enabled == false);
        TEST(timer.Interval == 100);

        // Non-default construction
        timer = new Linn.Timer(1000);
        TEST(timer.AutoReset == true);
        TEST(timer.Enabled == false);
        TEST(timer.Interval == 1000);

        // Setting/Getting of properties
        timer.Interval = 1;
        TEST(timer.Interval == 1);
        timer.AutoReset = false;
        TEST(timer.AutoReset == false);

        // AutoReset == false
        iEventHandler1 = 0;
        iEventHandler2 = 0;
        timer = new Linn.Timer(1000);
        TestNoAutoReset(timer, StartUsingEnabled, StopUsingEnabled);
        iEventHandler1 = 0;
        iEventHandler2 = 0;
        timer = new Linn.Timer(1000);
        TestNoAutoReset(timer, StartUsingStart, StopUsingStop);

        // AutoReset == true
        iEventHandler1 = 0;
        iEventHandler2 = 0;
        timer = new Linn.Timer(1000);
        TestAutoReset(timer, StartUsingEnabled, StopUsingEnabled);
        iEventHandler1 = 0;
        iEventHandler2 = 0;
        timer = new Linn.Timer(1000);
        TestAutoReset(timer, StartUsingStart, StopUsingStop);
    }

    private void StartUsingStart(Linn.Timer aTimer) {
        aTimer.Start();
    }
    private void StartUsingEnabled(Linn.Timer aTimer) {
        aTimer.Enabled = true;
    }
    private void StopUsingStop(Linn.Timer aTimer) {
        aTimer.Stop();
    }
    private void StopUsingEnabled(Linn.Timer aTimer) {
        aTimer.Enabled = false;
    }

    private delegate void DStartStopFunc(Linn.Timer aTimer);

    private void TestNoAutoReset(Linn.Timer aTimer, DStartStopFunc aStartFunc, DStartStopFunc aStopFunc) {
        aTimer.AutoReset = false;
        aTimer.Interval = 100;
        TEST(iEventHandler1 == 0);
        TEST(iEventHandler2 == 0);

        // 0 handlers
        aStartFunc(aTimer);
        Thread.Sleep(200);
        TEST(iEventHandler1 == 0);
        TEST(iEventHandler2 == 0);
        TEST(aTimer.Enabled == false);

        // 1 handler
        aTimer.Elapsed += new EventHandler(EventHandler1);
        aStartFunc(aTimer);
        Thread.Sleep(200);
        TEST(iEventHandler1 == 1);
        TEST(iEventHandler2 == 0);
        TEST(iEventHandler1Sender == aTimer);
        TEST(aTimer.Enabled == false);

        // 2 handlers
        aTimer.Elapsed += new EventHandler(EventHandler2);
        aStartFunc(aTimer);
        Thread.Sleep(200);
        TEST(iEventHandler1 == 2);
        TEST(iEventHandler2 == 1);
        TEST(iEventHandler1Sender == aTimer);
        TEST(iEventHandler2Sender == aTimer);
        TEST(aTimer.Enabled == false);

        // Start & Stop - should get no events
        aStartFunc(aTimer);
        aStopFunc(aTimer);
        TEST(iEventHandler1 == 2);
        TEST(iEventHandler2 == 1);
        TEST(aTimer.Enabled == false);
    }

    private void TestAutoReset(Linn.Timer aTimer, DStartStopFunc aStartFunc, DStartStopFunc aStopFunc) {
        aTimer.AutoReset = true;
        aTimer.Interval = 100;
        TEST(iEventHandler1 == 0);
        TEST(iEventHandler2 == 0);

        // 0 handlers
        aStartFunc(aTimer);
        Thread.Sleep(1050); // 10 events should have happened
        TEST(aTimer.Enabled == true);
        aStopFunc(aTimer);
        TEST(iEventHandler1 == 0);
        TEST(iEventHandler2 == 0);
        TEST(aTimer.Enabled == false);

        // 1 handler
        aTimer.Elapsed += new EventHandler(EventHandler1);
        aStartFunc(aTimer);
        Thread.Sleep(1050); // 10 events should have happened
        TEST(aTimer.Enabled == true);
        aStopFunc(aTimer);
        TEST(iEventHandler1 == 10);
        TEST(iEventHandler2 == 0);
        TEST(iEventHandler1Sender == aTimer);
        TEST(aTimer.Enabled == false);

        // 2 handlers
        aTimer.Elapsed += new EventHandler(EventHandler2);
        aStartFunc(aTimer);
        Thread.Sleep(1050); // 10 events should have happened
        TEST(aTimer.Enabled == true);
        aStopFunc(aTimer);
        TEST(iEventHandler1 == 20);
        TEST(iEventHandler2 == 10);
        TEST(iEventHandler1Sender == aTimer);
        TEST(iEventHandler2Sender == aTimer);
        TEST(aTimer.Enabled == false);
    }

    private void EventHandler1(object aSender, EventArgs aArgs) {
        iEventHandler1++;
        iEventHandler1Sender = aSender;
    }
    private void EventHandler2(object aSender, EventArgs aArgs) {
        iEventHandler2++;
        iEventHandler2Sender = aSender;
    }
    private int iEventHandler1 = 0;
    private int iEventHandler2 = 0;
    private object iEventHandler1Sender = null;
    private object iEventHandler2Sender = null;
}

class TestTimer
{
    static void Main(string[] aArgs) {
        Helper helper = new Helper(aArgs);
        helper.ProcessCommandLine();

        Runner runner = new Runner("Some Linn C# timer tests");
        runner.Add( new SuiteTimer() );
        runner.Run();

        helper.Dispose();
    }
}


