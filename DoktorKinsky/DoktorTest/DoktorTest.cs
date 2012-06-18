using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

using Linn;
using Linn.Doktor;

public class DoktorTest : ISupplyHandler, IReportConsole
{
    [STAThread]

    static void Main(string[] aArgs)
    {
        DoktorTest app = new DoktorTest(aArgs);
        app.Run();
    }
    
    public DoktorTest(string[] aArgs)
    {
        iMutex = new Mutex();
        iHelper = new Helper(aArgs);
        iOptionShow = new OptionParser.OptionBool("-s", "--show", "Show tests");

        OptionParser options = iHelper.OptionParser;

        options.Usage =  "usage: DoktorTest [options] [test]\n";

        options.AddOption(iOptionShow);

        string id = "DoktorTest";
        
        iTests = Test.CreateTests(id);
        iSupplies = Supply.CreateSupplies(id);
    }
    
    private Mutex iMutex;
    private IHelper iHelper;
    private OptionParser.OptionBool iOptionShow;
    
    private IList<ITest> iTests;
    private IList<ISupply> iSupplies;

    private List<INode> iNodes;
    
    private ITest iTest;
    
    public void Run()
    {
        iHelper.ProcessCommandLine();
        
        if (!iHelper.OptionParser.HelpSpecified())
        {
            Start(iHelper.OptionParser.PosArgs);
        }
        
        iHelper.Dispose();
    }
    
    public void Add(INode aNode)
    {
        Console.Write(".");
        
        Lock();
        
        iNodes.Add(aNode);
        
        Unlock();
    }
    
    public void Remove(INode aNode)
    {
    }
    
    private void CollectNodes()
    {
        // Wait until 500 milliseconds goes by without collecting any more nodes
        
        Console.Write("Discovering ...");

        iNodes = new List<INode>();

        foreach (ISupply supply in iSupplies)
        {
            supply.Open();
            supply.Subscribe(this);
        }
        
        int collected = 0;
        
        while (true)
        {
            Thread.Sleep(500);
            
            Lock();
            
            int count = iNodes.Count;
            
            Unlock();
            
            if (count == collected)
            {
                break;
            }
            
            collected = count;
        }
        
        foreach (ISupply supply in iSupplies)
        {
            supply.Unsubscribe(this);
            supply.Close();
        }
        
        Console.WriteLine(".");
    }
    
    private void Lock()
    {
        iMutex.WaitOne();
    }
    
    private void Unlock()
    {
        iMutex.ReleaseMutex();
    }
    
    private ITest FindTest(string aName)
    {
        foreach (ITest test in iTests)
        {
            if (test.Name == aName)
            {
                return (test);
            }
        }
        
        return (null);
    }

    private void ShowTests()
    {
        foreach (ITest t in iTests)
        {
            long kMaxNamePositions = 20;

            Console.Write(t.Name);

            long spaces = kMaxNamePositions - t.Name.Length;
            
            for (long i = 0; i < spaces; i++)
            {
                Console.Write(" ");
            }
            
            Console.Write(" ");

            Console.WriteLine(t.Description);
        }
    }
        
    private void ShowTest(ITest aTest)
    {
        Console.WriteLine("Name         : " + aTest.Name);
        Console.WriteLine("Type         : " + aTest.Type);
        Console.WriteLine("Description  : " + aTest.Description);

        Console.WriteLine("");

        Console.WriteLine("Parameters");
        Console.WriteLine("==========");
        
        if (aTest.Parameters.Count == 0)
        {
            Console.WriteLine("None");
        }
        else
        {
            foreach (IParameter p in aTest.Parameters)
            {
                Console.WriteLine("Name        : "  + p.Name);
                Console.WriteLine("Type        : "  + p.Type);
                Console.WriteLine("Description : "  + p.Description);
                
                foreach (KeyValuePair<string, string> a in p.Attributes)
                {
                    Console.WriteLine(a.Key + " : " + a.Value);
                }

                Console.WriteLine("");
            }
        }
    }
    
    private void Start(IList<string> aArgs)
    {
        if (iOptionShow.Value)  // Show Tests
        {
            if (aArgs.Count > 0)
            {
                ITest test = FindTest(aArgs[0]);
                
                if (test != null)
                { 
                    ShowTest(test);
                }
                else
                {
                    Console.WriteLine("Test not found");
                }
            }
            else
            {
                ShowTests();
            }
            return;
        }
        
        if (aArgs.Count == 0)
        {
            Console.WriteLine("Test not specified");
            return;
        }
        
        iTest = FindTest(aArgs[0]);

        if (iTest == null)
        {
            Console.WriteLine("Specified test not found");
            return;
        }

        // Check arguments

        int args = aArgs.Count - 1;

        if (iTest.Parameters.Count > args)
        {
            int i = 1;

            foreach (IParameter parameter in iTest.Parameters)
            {
                if (args-- == 0)
                {
                    Console.WriteLine(iTest.Name + " argument " + i + " not specified (" + parameter.Name + ")");
                    return;
                }

                i++;
            }
        }

        int pos = 1;

        // Set Parameters
        
        foreach (IParameter parameter in iTest.Parameters)
        {
            if (parameter.Kind == "Nodal")
            {
                if (iNodes == null)
                {
                    CollectNodes();
                }
            }

            parameter.Init(iNodes);

            parameter.Value = aArgs[pos];

            if (!parameter.Valid)
            {
                Console.WriteLine(parameter.Name + " invalid");
                return;
            }

            pos++;
        }

        Report report = new Report();
        ReportConsole console = new ReportConsole(this, report);

        iTest.Execute(report);
    }

    public void Write(string aMessage)
    {
        Console.Write(aMessage);
    }
}

