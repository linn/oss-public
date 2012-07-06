// Test harness for Diagnostics DLL

using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;

using Linn.ProductSupport.Diagnostics;
using Linn.Topology;
using Linn.ControlPoint;


namespace Linn.ProductSupport.TestDiagnostics
{
    class TestDiagnostics
    {
        public const string iDsAddress = "10.2.11.212"; //"10.2.9.47"; //"10.15.1.114";
        public const string iInterface = "10.2.11.71"; //"192.168.1.44"; //"10.15.1.114"; //"10.2.11.71";
        public const string iUglyName = "linn-2171e4";

        static void Main(string[] args)
        {
            // Startup diagnostics
            iDiags = new Diagnostics.Diagnostics();
            iTestsComplete = new EventWaitHandle(false, EventResetMode.ManualReset);

            // Add a local message handler
            iDiags.AddLogMessageCb(Log);

//            // Poll status at regular intervals
//            Thread StatusPollThread = new Thread(new ThreadStart(StatusPoll));
//            iPolling = true;
//            StatusPollThread.Start();
//            Thread.Sleep(3000);

            // Monitor events
//            iDiags.evStart += new TestStartEvent(EvTestStart);
//            iDiags.evEnd += new TestEndEvent(EvTestEnd);
//            iDiags.evPass += new PassEvent(EvPass);
//            iDiags.evWarn += new WarnEvent(EvWarn);
//            iDiags.evFail += new FailEvent(EvFail);
            iDiags.evAllComplete += new AllCompleteEvent(EvAllComplete);

            // Run list of tests
//            List<ETest> tests = new List<ETest>();
//            tests.Add(ETest.eDhcp);
//            tests.Add(ETest.eUpnp);
//            tests.Add(ETest.eInternet);
//            tests.Add(ETest.eUdpEcho);
//            tests.Add(ETest.eTcpEcho);
//            tests.Add(ETest.eMulticastFromDs);
//            tests.Add(ETest.eMulticastToDs);
//            iDiags.Run(tests, iInterface, iDsAddress);

            // Run individual tests
//            iDiags.Run(ETest.eDhcp, iInterface);
//            iDiags.Run(ETest.eDhcp, "10.15.1.114");
            iDiags.Run(ETest.eUpnp, iInterface);
//            iDiags.Run(ETest.eInternet, iInterface);
//            iDiags.Run(ETest.eUdpEcho, iInterface, iDsAddress);
//            iDiags.Run(ETest.eTcpEcho, iInterface, iDsAddress);
//            iDiags.Run(ETest.eMulticastFromDs, iInterface, iDsAddress);
//            iDiags.Run(ETest.eMulticastToDs, iInterface, iDsAddress);
//            iDiags.Run(ETest.eAll);

            // Wait for tests to complete and check the results
            iTestsComplete.WaitOne();

            foreach (TestResult res in iDiags.Results())
            {
                Console.WriteLine("\n-------------------------------------");
                Console.WriteLine("TITLE:  " + res.title);
                Console.WriteLine("START:  " + res.startTime);
                Console.WriteLine("END:    " + res.endTime);
                Console.WriteLine("RESULT: " + res.result + " " + res.description);
                Console.WriteLine("\n" + res.content);
//                Console.WriteLine("\n        --------");
//                foreach (Item item in res.items)
//               {
//                    Console.WriteLine("\n" + item.title);
//                    Console.WriteLine(item.content);
//                }
                Console.WriteLine("-------------------------------------\n");
            }

//            // Shutdown status polling thread
//            iPolling = false;
//            StatusPollThread.Join();

            // Stop event monitoring
//            iDiags.evStart -= new TestStartEvent(EvTestStart);
//            iDiags.evEnd -= new TestEndEvent(EvTestEnd);
//            iDiags.evPass -= new PassEvent(EvPass);
//            iDiags.evWarn -= new WarnEvent(EvWarn);
//            iDiags.evFail -= new FailEvent(EvFail);
            iDiags.evAllComplete -= new AllCompleteEvent(EvAllComplete);

            // Shutdown diagnostics
            iDiags.Shutdown();
        }

        // Local message handler - outputs log messages as they are received
        static void Log(LogMessage aMsg)
        {
            Console.WriteLine("CALLBACK " + aMsg.time + " " + aMsg.type + ": " + aMsg.test + " (" + aMsg.subtest + ") " + aMsg.text);
        }

        // Poll diagnostics status at regular interval, output the rx data
        static void StatusPoll()
        {
            while (iPolling)
            {
                Console.WriteLine("\nPOLLED   Executing: " + iDiags.Executing());
                foreach (TestInfo t in iDiags.TestList())
                {
                    Console.WriteLine("POLLED   List: " + t.test);
                }
                Console.WriteLine("POLLED   Idle: " + iDiags.Idle());
                Console.WriteLine("POLLED   Running: " + iDiags.Running());
                Thread.Sleep(1000);
            }
        }

        // Event handlers - output rx events
        public static void EvTestStart(ETest aTest)
        {
            Console.WriteLine("EVENTED  started " + aTest.ToString());
        }

        public static void EvTestEnd(ETest aTest)
        {
            Console.WriteLine("EVENTED  ended " + aTest.ToString());
        }

        public static void EvAllComplete()
        {
            iTestsComplete.Set();
            Console.WriteLine("EVENTED  ALL complete");
        }

        public static void EvPass(LogMessage aMsg)
        {
            Console.WriteLine("EVENTED  PASS " + aMsg.subtest + " " + aMsg.text);
        }

        public static void EvWarn(LogMessage aMsg)
        {
            Console.WriteLine("EVENTED  WARN " + aMsg.subtest + " " + aMsg.text);
        }

        public static void EvFail(LogMessage aMsg)
        {
            Console.WriteLine("EVENTED  FAIL " + aMsg.subtest + " " + aMsg.text);
        }

        // class data
        static private Diagnostics.Diagnostics iDiags;
        static private EventWaitHandle iTestsComplete;
        static private bool iPolling;
    }
}

