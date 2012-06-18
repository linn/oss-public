using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;

namespace Linn.Doktor
{
    public interface IReport
    {
        void Reference(string aReference);
        void Context(string aKey, string aValue);
        void High(string aDescription);
        void Medium(string aDescription);
        void Low(string aDescription);
    }

    public class Specification
    {
        public Specification(string aOrganisation, string aTitle)
        {
            iOrganisation = aOrganisation;
            iTitle = aTitle;
        }

        public string Reference(uint aPage)
        {
            return (iOrganisation + ", " + iTitle + ", p. " + aPage + ".");
        }

        public string Reference(uint aStartPage, uint aEndPage)
        {
            return (iOrganisation + ", " + iTitle + ", pp. " + aStartPage + "-" + aEndPage);
        }

        private string iOrganisation;
        private string iTitle;
    }

    public class Error
    {
        public const string kSeverityLow = "Low";
        public const string kSeverityMedium = "Medium";
        public const string kSeverityHigh = "High";

        public Error(string aSeverity, string aDescription, List<string> aReferences, Dictionary<string, string> aContext)
        {
            iSeverity = aSeverity;
            iDescription = aDescription;
            iReferences = aReferences;
            iContext = aContext;

            StringBuilder builder;
            
            builder = new StringBuilder();

            if (iReferences.Count > 0)
            {
                foreach (string reference in iReferences)
                {
                    builder.Append(reference);
                    builder.Append("\n");
                }
            }
            else
            {
                builder.Append("None");
            }

            iReferenceText = builder.ToString();

            builder = new StringBuilder();

            foreach (KeyValuePair<string, string> entry in iContext)
            {
                builder.Append(entry.Key);
                builder.Append(" = ");
                builder.Append(entry.Value);
                builder.Append("\n");
            }

            iContextText = builder.ToString();
        }

        public string Severity
        {
            get
            {
                return (iSeverity);
            }
        }

        public string Description
        {
            get
            {
                return (iDescription);
            }
        }

        public IList<string> References
        {
            get
            {
                return (iReferences);
            }
        }

        public IDictionary<string, string> Context
        {
            get
            {
                return (iContext);
            }
        }

        public string ReferenceText
        {
            get
            {
                return (iReferenceText);
            }
        }

        public string ContextText
        {
            get
            {
                return (iContextText);
            }
        }

        private string iSeverity;
        private string iDescription;
        private List<string> iReferences;
        private Dictionary<string, string> iContext;
        private string iReferenceText;
        private string iContextText;
    }

    public class Report : IReport
    {
        public Report()
        {
            iMutex = new Mutex();
            iReferences = new List<string>();
            iContext = new Dictionary<string, string>();
            iErrors = new List<Error>();
            iCounterHigh = 0;
            iCounterMedium = 0;
            iCounterLow = 0;
        }

        public void Reference(string aReference)
        {
            iReferences.Add(aReference);
        }

        public void Context(string aKey, string aValue)
        {
            Lock();

            iContext.Add(aKey, aValue);

            Unlock();
        }

        public void High(string aDescription)
        {
            Lock();

            iErrors.Add(new Error(Error.kSeverityHigh, aDescription, iReferences, iContext));

            iCounterHigh++;

            iReferences = new List<string>();
            iContext = new Dictionary<string,string>();

            Unlock();

            ReportChanged();
        }

        public void Medium(string aDescription)
        {
            Lock();

            iErrors.Add(new Error(Error.kSeverityMedium, aDescription, iReferences, iContext));

            iCounterMedium++;

            iReferences = new List<string>();
            iContext = new Dictionary<string, string>();

            Unlock();

            ReportChanged();
        }

        public void Low(string aDescription)
        {
            Lock();

            iErrors.Add(new Error(Error.kSeverityLow, aDescription, iReferences, iContext));

            iCounterLow++;

            iReferences = new List<string>();
            iContext = new Dictionary<string, string>();

            Unlock();

            ReportChanged();
        }

        public void Counters(out uint aHigh, out uint aMedium, out uint aLow)
        {
            Lock();

            aHigh = iCounterHigh;
            aMedium = iCounterMedium;
            aLow = iCounterLow;

            Unlock();
        }

        public IList<Error> Errors
        {
            get
            {
                return (DuplicateErrors());
            }
        }

        public EventHandler<EventArgs> EventChanged;

        private List<Error> DuplicateErrors()
        {
            Lock();

            List<Error> errors = new List<Error>(iErrors);

            Unlock();

            return (errors);
        }

        private void ReportChanged()
        {
            if (EventChanged != null)
            {
                EventChanged(this, EventArgs.Empty);
            }
        }

        private void Lock()
        {
            iMutex.WaitOne();
        }

        private void Unlock()
        {
            iMutex.ReleaseMutex();
        }

        private Mutex iMutex;
        private List<string> iReferences;
        private Dictionary<string, string> iContext;
        private List<Error> iErrors;
        private uint iCounterHigh;
        private uint iCounterMedium;
        private uint iCounterLow;
    }

    public interface IReportConsole
    {
        void Write(string aMessage);
    }

    public class ReportConsole
    {
        private const int kTypeColumnWidth = 10;

        public ReportConsole(IReportConsole aConsole, Report aReport)
        {
            iConsole = aConsole;
            iReport = aReport;
            iReport.EventChanged += ReportChanged;

            iSpaces = new string(' ', kTypeColumnWidth);
        }

        public void ReportChanged(object obj, EventArgs e)
        {
            iConsole.Write(".");
        }

        IReportConsole iConsole;
        Report iReport;
        string iSpaces;
    }

    /*
        public interface IProgress
        {
            void Set(int aValue);
            void Close();
        }
    
        IProgress CreateProgress(string aDescription);
        IProgress CreateProgress(string aDescription, int aMax);
        IProgress CreateProgress(string aDescription, int aMin, int aMax);

        // Valid types:
        // Test
        // Parameter
        // Success
        // Error

        public interface IReport
        {
            void Begin(ITest aTest);
            void Case(string aDescription);
            void Info(string aMessage);
            void Fail(string aExplanation);
            void End(ITest aTest);
        }

        public interface IReportConsole
        {
            void Write(string aMessage);
        }

        public class ReportConsole : IReportStream
        {
            private const int kTypeColumnWidth = 10;

            public ReportConsole(IReportConsole aConsole)
            {
                iConsole = aConsole;
                iSpaces = new string(' ', kTypeColumnWidth);
            }

            // IReportStream

            public void Write(ReportStreamItem aItem)
            {
                iConsole.Write(aItem.Type + iSpaces.Substring(0, kTypeColumnWidth - aItem.Type.Length) + aItem.Message + "\n");
            }

            IReportConsole iConsole;
            string iSpaces;
        }

        public class ReportStreamItem
        {
            public const string kTypeTest = "Test";
            public const string kTypeCase = "Case";
            public const string kTypeInfo = "Info";
            public const string kTypeFail = "Fail";
            public const string kTypeEnd = "End";

            public ReportStreamItem(string aType, string aMessage)
            {
                iType = aType;
                iMessage = aMessage;
            }

            public string Type
            {
                get
                {
                    return (iType);
                }
            }

            public string Message
            {
                get
                {
                    return (iMessage);
                }
            }

            string iType;
            string iMessage;
        }

        public interface IReportStream
        {
            void Write(ReportStreamItem aItem);
        }

        public class ReportStream : IReport
        {
            public ReportStream(IReportStream aStream)
            {
                iStream = aStream;
            }

            // IReport

            public void Begin(ITest aTest)
            {
                iStream.Write(new ReportStreamItem(ReportStreamItem.kTypeTest, aTest.Name));
            }

            public void Case(string aDescription)
            {
                iStream.Write(new ReportStreamItem(ReportStreamItem.kTypeCase, aDescription));
            }

            public void Info(string aMessage)
            {
                iStream.Write(new ReportStreamItem(ReportStreamItem.kTypeInfo, aMessage));
            }

            public void Fail(string aExplanation)
            {
                iStream.Write(new ReportStreamItem(ReportStreamItem.kTypeFail, aExplanation));
            }

            public void End(ITest aTest)
            {
                iStream.Write(new ReportStreamItem(ReportStreamItem.kTypeEnd, aTest.Score + "/" + aTest.MaxScore));
            }

            private IReportStream iStream;
        }

        public class ReportCapture : IReport
        {
            private enum EType
            {
                eBegin,
                eCase,
                eInfo,
                eSucceed,
                eFail,
                eEnd,
                eTime
            }

            private class Entry
            {
                public Entry(EType aType, object aObject)
                {
                    iType = aType;
                    iObject = aObject;
                }

                public EType Type
                {
                    get
                    {
                        return (iType);
                    }
                }

                public Object Object
                {
                    get
                    {
                        return (iObject);
                    }
                }

                EType iType;
                object iObject;
            }

            public ReportCapture(ITest aTest, IReport aReport)
            {
                iTest = aTest;
                iReport = aReport;
                iTime = DateTime.Now;
                iList = new List<Entry>();
            }

            public void Begin(ITest aTest)
            {
                iList.Add(new Entry(EType.eBegin, aTest));
                iList.Add(new Entry(EType.eTime, DateTime.Now));
                iReport.Begin(aTest);
            }

            public void Case(string aDescription)
            {
                iList.Add(new Entry(EType.eCase, aDescription));
                iReport.Case(aDescription);
            }

            public void Info(string aMessage)
            {
                iList.Add(new Entry(EType.eInfo, aMessage));
                iReport.Info(aMessage);
            }

            public void Fail(string aExplanation)
            {
                iList.Add(new Entry(EType.eFail, aExplanation));
                iReport.Fail(aExplanation);
            }

            public void End(ITest aTest)
            {
                iList.Add(new Entry(EType.eEnd, aTest));
                iReport.End(aTest);
            }

            public void Replay(IReport aReport)
            {
                foreach (Entry entry in iList)
                {
                    switch (entry.Type)
                    {
                        case EType.eBegin:
                            aReport.Begin(entry.Object as ITest);
                            break;
                        case EType.eCase:
                            aReport.Case(entry.Object as string);
                            break;
                        case EType.eFail:
                            aReport.Fail(entry.Object as string);
                            break;
                        case EType.eEnd:
                            aReport.End(entry.Object as ITest);
                            break;
                    }
                }
            }

            private ITest iTest;
            private IReport iReport;
            private DateTime iTime;
            private List<Entry> iList;
        }

        */
    /*
    public class ReportXml : IReport
    {
        public const string kElementTest = "Test";
        public const string kElementName = "Name";
        public const string kElementType = "Type";
        public const string kElementDescription = "Description";
        public const string kElementParameter = "Parameter";
        public const string kElementValue = "Value";
        public const string kElementReport = "Report";
        public const string kElementTime = "Time";

        public ReportXml()
        {
            iMutex = new Mutex();

            // Create the appendix

            iReport = new StringBuilder();
            iWriter = XmlWriter.Create(iReport);
            iWriter.WriteStartElement(kElementTest);
            iWriter.WriteStartElement(kElementReport);
            iWriter.Flush();
            int preamble = iReport.ToString().Length;
            iWriter.WriteEndElement();
            iWriter.WriteEndElement();
            iWriter.Flush();
            iAppendix = iReport.ToString().Substring(preamble);

            iReport = new StringBuilder();
            iWriter = XmlWriter.Create(iReport);

            iWriter.WriteStartElement(kElementTest);

            iWriter.WriteElementString(kElementName, iTest.Name);
            iWriter.WriteElementString(kElementType, iTest.Type);
            iWriter.WriteElementString(kElementDescription, iTest.Description);
            iWriter.WriteElementString(kElementDescription, iTest);

            foreach (IParameter parameter in iTest.Parameters)
            {
                iWriter.WriteStartElement(kElementParameter);

                iWriter.WriteElementString(kElementName, parameter.Name);
                iWriter.WriteElementString(kElementType, parameter.Type);
                iWriter.WriteElementString(kElementDescription, parameter.Description);
                iWriter.WriteElementString(kElementDescription, parameter.Value);

                iWriter.WriteEndElement();
            }

            iWriter.WriteStartElement(kElementReport);
        }

        public void Write(EMessageType aType, string aMessage)
        {
            Lock();

            Unlock();
        }

        public string Report
        {
            get
            {
                Lock();

                iWriter.Flush();

                string report = iReport.ToString();

                Unlock();

                return (report + iAppendix);
            }
        }

        private void Lock()
        {
            iMutex.WaitOne();
        }

        private void Unlock()
        {
            iMutex.ReleaseMutex();
        }

        private ITest iTest;
        private Mutex iMutex;
        private StringBuilder iReport;
        private string iAppendix;
        private XmlWriter iWriter;
    }

    public class Reporter
    {
        public Reporter(IReport aReport)
        {
            iReport = aReport;
        }
        
        public void Begin(EMessageType aType)
        {
            Assert.Check(iMessage == null);
            iType = aType;
            iMessage = new StringBuilder();
        }
        
        public void Write<T>(T aValue)
        {
            Assert.Check(iMessage != null);
            iMessage.Append(aValue.ToString());
        }
        
        public void Newline()
        {
            Assert.Check(iMessage != null);
            iMessage.Append("\n");
        }
        
        public void End()
        {
            Assert.Check(iMessage != null);
            iReport.Write(iType, iMessage.ToString());
            iMessage = null;
        }
        
        private IReport iReport;
        private EMessageType iType;
        private StringBuilder iMessage;
    }
    */
}
