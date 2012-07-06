// Message logger for diagnostics

using System;
using System.Collections.Generic;
using System.Threading;


namespace Linn.ProductSupport.Diagnostics
{

    //-----------------------------------------------------------------------------
    // Enum defining types of available log messages
    //-----------------------------------------------------------------------------
    public enum EType
    {
        eEnd,
        eFail,
        eInfo,
        ePass,
        eStart,
        eSubtest,
        eWarn
    }

    //-----------------------------------------------------------------------------
    // Class defining a log message
    //-----------------------------------------------------------------------------
    public class LogMessage
    {
        public LogMessage(EType aType, DateTime aTime, ETest aTest, string aSubtest, string aText)
        {
            iSubtest = aSubtest;
            iText = aText;
            iTime = aTime;
            iTest = aTest;
            iType = aType;
        }

        public string subtest
        {
            get
            {
                return iSubtest;
            }
        }

        public ETest test
        {
            get
            {
                return iTest;
            }
        }

        public string text
        {
            get
            {
                return iText;
            }
        }

        public EType type
        {
            get
            {
                return iType;
            }
        }

        public DateTime time
        {
            get
            {
                return iTime;
            }
        }

        private string iSubtest;
        private ETest iTest;
        private string iText;
        private DateTime iTime;
        private EType iType;
    }

    //-----------------------------------------------------------------------------
    // Classes defining a test result
    //-----------------------------------------------------------------------------
    public class TestResult
    {
        public TestResult(List<LogMessage> aMessages)
        {
            iTitle = "";
            iResult = "PASS";
            iDescription = "";
            iContent = "";
            iStartTime = new DateTime(0);
            iEndTime = new DateTime(0);

            lock (aMessages)
            {
                foreach (LogMessage msg in aMessages)
                {
                    switch (msg.type)
                    {
                        case EType.eStart:
                            iTitle = msg.test.ToString().Substring(1);
                            iStartTime = msg.time;
                            break;

                        case EType.eEnd:
                            iEndTime = msg.time;
                            break;

                        case EType.ePass:
                            if (iContent.Length > 0)
                            {
                                iContent += "\n";
                            }
                            iContent += "PASS " + msg.subtest;
                            if (msg.text.Length > 0)
                            {
                                iContent += ": " + msg.text;
                            }
                            break;

                        case EType.eWarn:
                            if (iResult != "FAIL")
                            {
                                iResult = "WARN";
                                iDescription = msg.text;
                            }
                            if (iContent.Length > 0)
                            {
                                iContent += "\n";
                            }
                            iContent += "WARN " + msg.subtest;
                            if (msg.text.Length > 0)
                            {
                                iContent += ": " + msg.text;
                            }
                            break;

                        case EType.eFail:
                            iResult = "FAIL";
                            iDescription = msg.text;
                            if (iContent.Length > 0)
                            {
                                iContent += "\n";
                            }
                            iContent += "FAIL " + msg.subtest;
                            if (msg.text.Length > 0)
                            {
                                iContent += ": " + msg.text;
                            }
                            break;

                        case EType.eSubtest:
                            if (iContent.Length > 0)
                            {
                                iContent += "\n";
                            }
                            break;

                        case EType.eInfo:
                            if (iContent.Length > 0)
                            {
                                iContent += "\n";
                            }
                            iContent += msg.text;
                            break;
                    }
                }
            }
        }

        public string title
        {
            get
            {
                return iTitle;
            }
        }

        public string result
        {
            get
            {
                return iResult;
            }
        }

        public string description
        {
            get
            {
                return iDescription;
            }
        }

        public DateTime startTime
        {
            get
            {
                return iStartTime;
            }
        }

        public DateTime endTime
        {
            get
            {
                return iEndTime;
            }
        }

        public string content
        {
            get
            {
                return iContent;
            }
        }

        private string iTitle;
        private string iResult;
        private string iDescription;
        private string iContent;
        private DateTime iStartTime;
        private DateTime iEndTime;
    }

    //-----------------------------------------------------------------------------
    // Class defining the message logger
    //-----------------------------------------------------------------------------
    internal class Logger
    {
        // external access to the logs is only available via the Diagnostics class

        // log messages received on the public (internal) interface are timestamped 
        // and immediately put into a Q in order to minimise impact on the actual
        // tests which generate them. Message processing and interrogation is then
        // handled on seperate threads which read the data from the input Q
    
        public Logger(Diagnostics aDiags)
        {
            iDiags = aDiags;    // containing Diagnostics instance required so events can be accessed
            iMessages = new List<List<LogMessage>>();
            iMessageQ = new Queue<LogMessage>();
            iMessageAvail = new Semaphore(0, 100);
            iCbList = new List<LoggerCb>();
            iTest = ETest.eNone;
            iSubtest = "";

            // start the offline message processor
            iHandlerThread = new Thread(new ThreadStart(HandleMessages));
            iHandlerThread.Start();
        }

        public void Shutdown()
        {
            iShutdown = true;
            lock (iMessageQ)
            {
                iMessageQ.Enqueue(new LogMessage(EType.eInfo, DateTime.Now, ETest.eNone, "", "Shutdown"));
                iMessageAvail.Release();
            }
        }

        //---------------------------------------------------------------------------
        // Caller can add own callback(s) which are called after every log message is
        // removed from message Q (done AFTER internal message processing methods)
        //---------------------------------------------------------------------------
        public void AddMessageCb(LoggerCb aCallback)
        {
            lock (iCbList)
            {
                iCbList.Add(aCallback);
            }
        }

        public void RemoveMessageCb(LoggerCb aCallback)
        {
            lock (iCbList)
            {
                iCbList.Remove(aCallback);
            }
        }

        //--------------------------------------------------
        // Message logging input methods - add messages to Q
        //--------------------------------------------------
        public void Pass(string aMsg)
        {
            lock (iMessageQ)
            {
                iMessageQ.Enqueue(new LogMessage(EType.ePass, DateTime.Now, iTest, iSubtest, aMsg));
                iMessageAvail.Release();
            }
        }

        public void Warn(string aMsg)
        {
            lock (iMessageQ)
            {
                iMessageQ.Enqueue(new LogMessage(EType.eWarn, DateTime.Now, iTest, iSubtest, aMsg));
                iMessageAvail.Release();
            }
        }

        public void Fail(string aMsg)
        {
            lock (iMessageQ)
            {
                iMessageQ.Enqueue(new LogMessage(EType.eFail, DateTime.Now, iTest, iSubtest, aMsg));
                iMessageAvail.Release();
            }
        }

        public void Info(string aMsg)
        {
            lock (iMessageQ)
            {
                iMessageQ.Enqueue(new LogMessage(EType.eInfo, DateTime.Now, iTest, iSubtest, aMsg));
                iMessageAvail.Release();
            }
        }

        public void StartTest(ETest aTest)
        {
            iTest = aTest;
            iSubtest = "";
            lock (iMessageQ)
            {
                iMessageQ.Enqueue(new LogMessage(EType.eStart, DateTime.Now, iTest, iSubtest, ""));
                iMessageAvail.Release();
            }
        }

        public void Subtest(string aMsg)
        {
            iSubtest = aMsg;
            lock (iMessageQ)
            {
                iMessageQ.Enqueue(new LogMessage(EType.eSubtest, DateTime.Now, iTest, iSubtest, ""));
                iMessageAvail.Release();
            }
        }

        public void EndTest()
        {
            iSubtest = "";
            lock (iMessageQ)
            {
                iMessageQ.Enqueue(new LogMessage(EType.eEnd, DateTime.Now, iTest, iSubtest, ""));
                iMessageAvail.Release();
            }
            iTest = ETest.eNone;
        }

        //-----------------------------------
        // Results interrogation and clearing
        //-----------------------------------
        public List<TestResult> Results()                     // returns all available results
        {
            List<TestResult> results = new List<TestResult>();
            foreach (List<LogMessage> msg in iMessages)
            {
                results.Add(new TestResult(msg));
            }
            return results;
        }
/*
                 Disabled since ability to execute tests across multiple adapters is now available,
                hence tests can no longer be uniquely identified by ther test type, and logger has
                no knowledge of the network adapter used for individual test (which would be needed
                to uniquely identify a test execution).

                public List<ETest> AvailResults()           // returns list of tests with available results
                {
                    List<ETest> availResults = new List<ETest>();
                    lock (iMessages)
                    {
                        foreach (KeyValuePair<ETest, List<LogMessage>> kvp in iMessages)
                        {
                            availResults.Add(kvp.Key);
                        }
                    }
                    return availResults;
                }

                public List<TestResult> Results()                     // returns all available results
                {
                    List<TestResult> results = new List<TestResult>();
                    foreach (ETest test in AvailResults())
                    {
                        results.Add(Results(test));
                    }
                    return results;
                }

                public List<TestResult> Results(List<ETest> aTests)    // returns results for specified list of tests
                {
                    List<TestResult> results = new List<TestResult>();
                    foreach (ETest test in aTests)
                    {
                        results.Add(Results(test));
                    }
                    return results;
                }

                public TestResult Results(ETest aTest)           // returns results for specified test
                {
                    return new TestResult(iMessages[aTest]);
                }
*/
        public void ClearResults()                  // clear all results
        {
            lock (iMessages)
            {
                iMessages.Clear();
            }
        }


        //---------------------------------------------------------------
        // Message processing - remove from Q, process, and call user CBs
        //---------------------------------------------------------------
        private void HandleMessages()
        {
            LogMessage msg;

            while (!iShutdown)
            {  
                iMessageAvail.WaitOne();
                lock (iMessageQ)
                {
                    msg = iMessageQ.Dequeue();
                }

                switch (msg.type)
                {
                    case EType.eStart:
                        iCurrent = new List<LogMessage>();
                        iDiags.OnStart(msg.test);
                        break;
                    case EType.eEnd:
                        // replace any existing messages for specified test with the latest ones
                        lock (iMessages)
                        {
                            iCurrent.Add(msg);
                            //iMessages.Remove(msg.test);
                            //iMessages.Add(msg.test, iCurrent);
                            iMessages.Add(iCurrent);
                        }
                        iDiags.OnEnd(msg.test);
                        break;
                    case EType.ePass:
                        iDiags.OnPass(msg);
                        break;
                    case EType.eWarn:
                        iDiags.OnWarn(msg);
                        break;
                    case EType.eFail:
                        iDiags.OnFail(msg);
                        break;
                }

                if (!iShutdown)
                {
                    if (iCurrent == null)
                    {
                        Assert.Check(false, "Test MUST be started before logging");
                    }

                    if (msg.type != EType.eEnd)
                    {
                        iCurrent.Add(msg);
                    }
                }

                // execute user-added callbacks
                lock (iCbList)
                {
                    foreach (LoggerCb cb in iCbList)
                    {
                        cb(msg);
                    }
                }
            }
        }

        private Diagnostics iDiags;
        private List<List<LogMessage>> iMessages;
        private List<LogMessage> iCurrent;
        private Queue<LogMessage> iMessageQ;
        private Semaphore iMessageAvail;
        private Thread iHandlerThread;
        private string iSubtest;
        private ETest iTest;
        private bool iShutdown;
        private List<LoggerCb> iCbList;
    }
}
