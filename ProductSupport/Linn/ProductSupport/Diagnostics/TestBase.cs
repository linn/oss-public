// Base class for all diagnostics tests
//
// Derived tests MUST implement the ExecuteTest() method where they
// should implement the actual test execution

using System;


namespace Linn.ProductSupport.Diagnostics
{
    internal abstract class TestBase
    {
        public TestBase(string aInterface, Logger aLog, ETest aTest)
        {
            iInterface = aInterface;
            iLog = aLog;
            iTest = aTest;
            iKill = false;
        }

        public virtual void Shutdown()
        {
        }

        public virtual void Run()
        {
            iLog.StartTest(iTest);
            ExecuteTest();
            iLog.EndTest();
        }

        public virtual void Kill()
        {
            if (EventKill != null)
            {
                EventKill(this, EventArgs.Empty);
            }
            iKill = true;
        }

        public event EventHandler<EventArgs> EventKill;
        public abstract void ExecuteTest();
        protected Logger iLog;
        protected string iInterface;
        protected ETest iTest;
        protected bool iKill;
    }
}
