// Diagnostics library. Set of utilities to exercise diagnostics on network
// and network-attached devices, including Linn players

using System;
using System.Collections.Generic;
using System.Threading;
using Linn.ControlPoint;

namespace Linn.ProductSupport.Diagnostics
{
    //-----------------------------------------------------------------------------
    // Enum defining available tests
    //-----------------------------------------------------------------------------

    public enum ETest
    {
        eNone,
        eAll,               // not implemented
        eDhcp,              // no device
        eInternet,          // no device
        eMulticastFromDs,
        eMulticastToDs,
        eUpnp,              // no device
        eUdpEcho,
        eTcpEcho
    }


    //-----------------------------------------------------------------------------
    // Delegate which is used in user-defined log message callbacks
    //-----------------------------------------------------------------------------

    public delegate void LoggerCb(LogMessage aMsg);


    //-----------------------------------------------------------------------------
    // Delegates which are used by notification events
    //-----------------------------------------------------------------------------

    public delegate void TestStartEvent(ETest aTest);
    public delegate void TestEndEvent(ETest aTest);
    public delegate void AllCompleteEvent();
    public delegate void FailEvent(LogMessage aMsg);
    public delegate void WarnEvent(LogMessage aMsg);
    public delegate void PassEvent(LogMessage aMsg);


    //-----------------------------------------------------------------------------
    // Test information class
    //-----------------------------------------------------------------------------

    public class TestInfo
    {
        public TestInfo(ETest aTest, string aInterface, string aDeviceIp = null)
        {
            iTest = aTest;
            iInterface = aInterface;
            iDeviceIp = aDeviceIp;
        }

        public ETest test
        {
            get
            {
                return iTest;
            }
        }

        public string deviceIp
        {
            get
            {
                return iDeviceIp;
            }
        }

        public string iface
        {
            get
            {
                return iInterface;
            }
        }

        public ETest iTest;
        public string iDeviceIp;
        public string iInterface;
    }


    //-----------------------------------------------------------------------------
    // Diagnostics class definition
    //-----------------------------------------------------------------------------

    public class Diagnostics
    {
        public Diagnostics()
        {
            iTestList = new List<TestInfo>();
            iTestAvail = new Semaphore(0, 100);
            iRunning = false;
            iAllComplete = true;
            iShutdown = false;
            iLog = new Logger(this);    // pass instance to contained Logger to permit it to publish Diagnostics events
            iExecutionerThread = new Thread(new ThreadStart(Execute));
            iExecutionerThread.Start();
        }

        public void Shutdown()
        {
            iShutdown = true;
            if (iTest != null)
            {
                iTest.Kill();
            }
            iTestAvail.Release();       // triggers shutdown of test executioner thread
            iExecutionerThread.Join();
            iLog.Shutdown();
        }

        //--------------------------------------------------------------
        // Execute tests - specified individually, as a list or as 'all'
        //--------------------------------------------------------------

        public void Run(ETest aTest, string aInterface, string aDeviceIp = null)
        {
            lock (iTestList)
            {
                iAllComplete = false;
                iTestList.Add(new TestInfo(aTest, aInterface, aDeviceIp));
                iTestAvail.Release();
            }
        }

        public void Run(List<ETest> tests, string aInterface, string aDeviceIp = null)
        {
            foreach (ETest test in tests)
            {
                Run(test, aInterface, aDeviceIp);
            }
        }

        public void Execute()   // runs on its own thread, executing tests sequenetially
        {
            TestInfo t;

            while (!iShutdown)
            {
                iTest = null;
                iTestAvail.WaitOne();
                if (iTestList.Count > 0)
                {
                    lock (iTestList)
                    {
                        t = iTestList[0];
                        iTestList.RemoveAt(0);
                    }
                    iRunningTest = t.test;
                    iRunning = true;

                    switch (t.test)
                    {
                        case ETest.eAll:
                            break;
                        case ETest.eDhcp:
                            iTest = new TestDhcp(t.iface, iLog, t.test);
                            break;
                        case ETest.eInternet:
                            iTest = new TestInternet(t.iface, iLog, t.test);
                            break;
                        case ETest.eMulticastFromDs:
                            iTest = new TestTransport(EProtocol.eUdp, EProtocol.eUdpMulti, t.iface, t.deviceIp, iLog, t.test);
                            break;
                        case ETest.eMulticastToDs:
                            iTest = new TestTransport(EProtocol.eUdpMulti, EProtocol.eUdp, t.iface, t.deviceIp, iLog, t.test);
                            break;
                        case ETest.eUpnp:
                            iTest = new TestUpnp(t.iface, iLog, t.test);
                            break;
                        case ETest.eUdpEcho:
                            iTest = new TestTransport(EProtocol.eUdp, EProtocol.eUdp, t.iface, t.deviceIp, iLog, t.test);
                            break;
                        case ETest.eTcpEcho:
                            iTest = new TestTransport(EProtocol.eTcp, EProtocol.eTcp, t.iface, t.deviceIp, iLog, t.test);
                            break;
                        default:
                            Assert.Check(false, "Unhandled ETest enum");
                            break;
                    }
                    iTest.Run();
                    iTest.Shutdown();
                }
                iRunningTest = ETest.eNone;
                iRunning = false;
                if (iTestList.Count == 0)
                {
                    OnAllComplete();
                    iAllComplete = true;
                }
            }
        }

        //-----------------
        // Published Events
        //-----------------

        public event TestStartEvent evStart;
        public event TestEndEvent evEnd;
        public event AllCompleteEvent evAllComplete;
        public event FailEvent evFail;
        public event WarnEvent evWarn;
        public event PassEvent evPass;

        //--------------
        // Polled status
        //--------------

        public bool Running()               // true if test running
        {
            return iRunning;
        }

        public bool Idle()                  // opposite of 'Running()'
        {
            return !Running();
        }

        public bool AllComplete()
        {
            return iAllComplete;
        }

        public ETest Executing()            // current running test
        {
            return iRunningTest;
        }

        public List<TestInfo> TestList()    // list of tests being executed
        {
            return iTestList;
        }

        //-----------------------------------
        // Results interrogation and clearing
        //-----------------------------------

        public List<TestResult> Results()                   // ALL results
        {
            return iLog.Results();
        }

        public void ClearResults()                          // clear all results
        {
            iLog.ClearResults();
        }

/*
        Disabled since ability to execute tests across multiple adapters is now available,
        hence tests can no longer be uniquely identified by ther test type, and logger has
        no knowledge of the network adapter used for individual test (which would be needed
        to uniquely identify a test execution).

        public List<ETest> AvailResults()                   // list of tests for which results available
        {
            return iLog.AvailResults();
        }

        public TestResult Results(ETest test)               // results for specified test
        {
            return iLog.Results(test);
        }

        public List<TestResult> Results(List<ETest> tests)  // results for specified tests
        {
            return iLog.Results(tests);
        }
*/
        //--------------------------------------------------------------------------
        // Register/Deregister callback(s) to be executed on receipt of log messages
        //--------------------------------------------------------------------------

        public void AddLogMessageCb(LoggerCb callback)
        {
            iLog.AddMessageCb(callback);
        }

        public void RemoveLogMessageCb(LoggerCb callback)
        {
            iLog.RemoveMessageCb(callback);
        }

        //-------------------------------
        // Internal event handler methods
        //-------------------------------

        // C# events can only be published by the declaring class (and its subclasses). There
        // is no way for a contained instance (ie. the Logger) to publish these events itself.
        // Hence, to keep the Diagnostics class as the only public interface (rather than letting
        // the Logger become public), these methods are declared as internal, and the Logger
        // receives a reference to its containing Diagnostics instance at startup. This  enables
        // it to call these events directly using its reference to the Diagnostics instance.

        internal void OnStart(ETest aTest)
        {
            if (evStart != null)
            {
                evStart(aTest);
            }
        }

        internal void OnEnd(ETest aTest)
        {
            if (evEnd != null)
            {
                evEnd(aTest);
            }
        }

        internal void OnAllComplete()
        {
            if (evAllComplete != null)
            {
                evAllComplete();
            }
        }

        internal void OnFail(LogMessage aMsg)
        {
            if (evFail != null)
            {
                evFail(aMsg);
            }
        }

        internal void OnWarn(LogMessage aMsg)
        {
            if (evWarn != null)
            {
                evWarn(aMsg);
            }
        }

        internal void OnPass(LogMessage aMsg)
        {
            if (evPass != null)
            {
                evPass(aMsg);
            }
        }

        //-------------------
        // Class data members
        //-------------------

        private TestBase iTest;
        private bool iRunning;
        private bool iAllComplete;
        private bool iShutdown;
        private ETest iRunningTest;
        private List<TestInfo> iTestList;
        private Logger iLog;
        private Thread iExecutionerThread;
        private Semaphore iTestAvail;
    }


    //--------------------------------------------------------------------------
    // Diagnostic information class
    //--------------------------------------------------------------------------

    public class DiagnosticInfo
    {
        public DiagnosticInfo() {
            iBox = null;
        }

        public DiagnosticInfo(Box aBox) {
            iBox = aBox;
        }

        public string ProductId {
            get {
                if (iBox == null) {
                    return "";
                }
                return iBox.ProductId;
            }
        }

        public string MacAddress {
            get {
                if (iBox == null) {
                    return "";
                }
                return iBox.MacAddress;
            }
        }

        public string SoftwareVersion {
            get {
                if (iBox == null) {
                    return "";
                }
                return iBox.SoftwareVersion;
            }
        }

        public string DeviceInfo {
            get {
                if (iBox == null) {
                    return "";
                }
                string deviceInfo = "";

                deviceInfo += "Box: " + iBox.ToString() + Environment.NewLine;
                deviceInfo += "Model: " + iBox.Model + Environment.NewLine;
                deviceInfo += "Mac Address: " + iBox.MacAddress + Environment.NewLine;
                deviceInfo += "Software Version: " + iBox.SoftwareVersion + Environment.NewLine;
                deviceInfo += "Product Id: " + iBox.ProductId + Environment.NewLine;
                deviceInfo += "Ip Address: " + iBox.IpAddress + Environment.NewLine;
                deviceInfo += "Udn: " + iBox.Udn + Environment.NewLine;
                deviceInfo += "Board Info: " + Environment.NewLine + iBox.BoardInfoString + Environment.NewLine;

                return deviceInfo;
            }
        }

        public string SystemInfo {
            get {
                return Linn.DebugInformation.SystemDetails();
            }
        }

        public string UserLog {
            get {
                return Linn.UserLog.Text;
            }
        }

        public string TimeZoneId {
            get
            {
                return System.TimeZoneInfo.Local.Id;
            }
        }
        
        public string OperatingSystem {
            get {
                return Linn.SystemInfo.VersionString;
            }
        }


        private Box iBox;
    }
}
