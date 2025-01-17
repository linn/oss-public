using System;

using Monobjc.Cocoa;
    
namespace Linn.Toolkit.Cocoa
{  
    public class CrashLogDumperMonobjc : ICrashLogDumper 
    { 
        public CrashLogDumperMonobjc(string aTitle, string aProduct, string aVersion) 
        { 
            iTitle = aTitle; 
            iProduct = aProduct; 
            iVersion = aVersion; 
        } 

        public void Dump(CrashLog aCrashLog) 
        { 
            int result = AppKitFramework.NSRunCriticalAlertPanel("The application " + iTitle + " quit unexpectedly", 
                                                                 "Mac OS X and other applications are not affected.\n\n" + 
                                                                 "Click Report to send a report to Linn", 
                                                                 "Report", 
                                                                 "Ignore", 
                                                                 null); 
            // show crash form 
            if (result == 1) 
            { 
                // post data to Linn 
                DebugReport report = new DebugReport("Crash log generated by " + iProduct + " ver " + iVersion); 
                report.Post(iTitle, aCrashLog.ToString()); 
            } 
        } 
  
        private string iTitle; 
        private string iProduct; 
        private string iVersion; 
    } 
}
