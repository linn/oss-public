using Linn;
using System;
using System.Threading;
using System.Drawing;           // Rectangle
using System.Windows.Forms;     // Screen
using System.Reflection;
using Linn.KinskyPda;
using Linn.Core;

public class Progam
{
    [MTAThread]
    public static void Main(string[] aArgs) {
        AppNetwork appControl = new AppNetwork(aArgs);
        AppWinForm app = new AppWinForm(appControl);
        OptionParser optParser = appControl.OptionParser;
        
        string dir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
        OptionString package = new OptionString(null, "--pcache", dir + "/Skins/Sneaky/640x480", "Sets the package cache", "PACKAGECACHE");
        OptionString texture = new OptionString(null, "--tcache", dir + "/Skins/Sneaky/640x480", "Sets the texture cache", "TEXTURECACHE");
        optParser.AddOption(package);
        optParser.AddOption(texture);
        OptionString title = new OptionString(null, "--title", "KinskyPda", "Sets the title of the window", "TITLE");
        optParser.AddOption(title);
        OptionBool developer = new OptionBool(null, "--dev", "Turn on developer mode");
        optParser.AddOption(developer);
        app.Start();

        Trace.Level = Trace.kRendering | Trace.kPerformance;// Trace.kPreamp | Trace.kMediaRenderer | Trace.kLinnGui;
        
        /*ThreadPool.SetMaxThreads(250, 1000);
        int a = 0, b = 0;
        ThreadPool.GetMaxThreads(out a, out b);
        Trace.WriteLine(Trace.kUpnp, a + " " + b);*/

        //Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
        app.Run(new ApplicationCanvas(title.Value, kSoftwareVersion, package.Value, texture.Value, appControl.Interface, developer.Value));

        app.Dispose();
    }
    
    private static readonly string kSoftwareVersion = "1.0.1";
}
