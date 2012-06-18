using System;
using System.Reflection;
using System.Diagnostics;
using System.IO;

namespace Linn {
namespace TestFramework {


// Path - static class for some paths - now moved into TestFramework
// so that only test programs can depend on the VOLKANO_INSTALL_ROOT
// environment variable
public class Path
{
    private static string Install() {
        return Environment.GetEnvironmentVariable("VOLKANO_INSTALL_ROOT");
    }
    public static string Share() {
        DirectoryInfo di = new DirectoryInfo(Install() + "/share");
        return di.FullName;
    }
    public static string XmlTags() {
        return Install() + "/include/Linn/Tags.xml";
    }
    public static string AbsPathFromInstall(string aPath) {
        return Install() + aPath;
    }
}

public class Suite
{
    public Suite(string aDescription) {
        iDesc = aDescription;
    }

    public virtual void Setup() {
    }

    public virtual void TearDown() {
    }

    public virtual void Test() {
    }
    
    public string Description {
        get {
            return iDesc;
        }
    }

    public static void TEST( bool aExpression ) {
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(1);
        if(aExpression) {
            Succeed(sf.GetFileName(), sf.GetFileLineNumber());
        }
        else {
            Fail(sf.GetFileName(), sf.GetFileLineNumber(), "");
        }
    }

    // Test a throw occurs when the member function aFunc is called on object aObj
    public static void TEST_THROWS(Type aExcType, object aObj, string aFunc, params object[] aArgs) {
        // Build an array of the argument types
        Type[] argTypes = new Type[aArgs.Length];
        for (int i=0 ; i<aArgs.Length ; i++) {
            argTypes[i] = aArgs[i].GetType();
        }

        // Get the method
        MethodInfo mi = aObj.GetType().GetMethod(aFunc, argTypes);
        if(mi == null) {
            // Get the property
            PropertyInfo pi = aObj.GetType().GetProperty(aFunc, new Type[0]);
            mi = pi.GetSetMethod();
        }

        // Invoke
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(1);
        try {
            if(mi != null) {
                mi.Invoke(aObj, aArgs);
            } else {
                throw new Exception("Unable to invoke " + aFunc + " - unsupported type?");
            }
            Fail(sf.GetFileName(), sf.GetFileLineNumber(), "");
        }
        catch (TargetInvocationException e) {
            if (e.InnerException.GetType() == aExcType) {
                Succeed(sf.GetFileName(), sf.GetFileLineNumber());
                return;
            }
            else {
                // an unexpected exception was thrown
                throw e;
            }
        }
    }

    // Test a throw occurs when the static member function aFunc is called on class aType
    public static void TEST_THROWS(Type aExcType, Type aType, string aFunc, params object[] aArgs) {
        // Build an array of the argument types
        Type[] argTypes = new Type[aArgs.Length];
        for (int i=0 ; i<aArgs.Length ; i++) {
            argTypes[i] = aArgs[i].GetType();
        }

        // Get the static method
        MethodInfo mi = aType.GetMethod(aFunc, argTypes);
        if(mi == null) {
            // Get the property
            PropertyInfo pi = aType.GetProperty(aFunc, new Type[0]);
            mi = pi.GetSetMethod();
        }

        // Invoke
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(1);
        try {
            if(mi != null) {
                mi.Invoke(null, aArgs);
            } else {
                throw new Exception("Unable to invoke " + aFunc + " - unsupported type?");
            }
            Fail(sf.GetFileName(), sf.GetFileLineNumber(), "");
        }
        catch (TargetInvocationException e) {
            if (e.InnerException.GetType() == aExcType) {
                Succeed(sf.GetFileName(), sf.GetFileLineNumber());
                return;
            }
            else {
                // an unexpected exception was thrown
                throw e;
            }
        }
    }

    public static void TEST_THROWS_NEW(Type aExcType, Type aType, params object[] aArgs) {
        StackTrace st = new StackTrace(true);
        StackFrame sf = st.GetFrame(1);
        try {
            Activator.CreateInstance(aType, aArgs);
            Fail(sf.GetFileName(), sf.GetFileLineNumber(), "");
        }
        catch (TargetInvocationException e) {
            if (e.InnerException.GetType() == aExcType) {
                Succeed(sf.GetFileName(), sf.GetFileLineNumber());
                return;
            }
            else {
                // an unexpected exception was thrown
                throw e;
            }
        }
    }

    private static void Succeed(string aFile, int aLine) {
        Runner.iPass++;
        Runner.iLastSuccessfulFile = aFile;
        Runner.iLastSuccessfulLine = aLine;
        Console.Write(".");
    }

    private static void Fail(string aFile, int aLine, string aMsg) {
        Runner.iFail++;
        Console.WriteLine("\nFAILURE: " + aFile + ":" + aLine);
        if(aMsg != "") {
            Console.Write(" -- " + aMsg);
        }
        Console.Write("\n");
    }

    private string iDesc = "";
}

public class Runner
{
    public Runner(string aDescription) {
        iDesc = aDescription;
    }

    public virtual void Add(Suite aSuite) {
        iSuiteList.Add(aSuite);
    }
    
    public void Run() {
        Setup();

        int i=1;
        foreach(Suite suite in iSuiteList) {
            Console.WriteLine("Suite " + i + ": " + suite.Description);
            suite.Setup();
            try {
                suite.Test();
            } catch(System.Exception e) {
                Console.WriteLine("\nFAILURE: Suite: " + i + " caused an unhandled exception:");
                Console.WriteLine(e);
                //Console.WriteLine("Last successful test: %s:%d\n", gLastSuccessfulFile, gLastSuccessfulLine);
                iFail++;
            }
            suite.TearDown();
            Console.Write("\n");
            i++;
        }
        TearDown();
    }

    protected virtual void Setup() {
        Console.WriteLine("Starting Test Runner: " + iDesc);
        iPass = 0;
        iFail = 0;
    }

    protected virtual void TearDown() {
        Console.WriteLine("Finished Test Runner: " + iDesc);
        Console.WriteLine(iPass + " of " + (iPass + iFail) + " tests passed.");
        Console.WriteLine(iFail + " of " + (iPass + iFail) + " tests failed.");
    }

    private System.Collections.ArrayList iSuiteList = new System.Collections.ArrayList();
    private string iDesc = "";
    public static int iPass = 0;
    public static int iFail = 0;
    public static string iLastSuccessfulFile = "";
    public static int iLastSuccessfulLine = 0;
}

} // Testframework
} // Linn

