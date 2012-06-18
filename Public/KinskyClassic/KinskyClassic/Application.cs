using System;
using System.Threading;
using System.Drawing;           // Rectangle
using System.Windows.Forms;     // Screen
using System.Net;

using Linn;
using Linn.Kinsky;
using Linn.Toolkit.WinForms;

namespace KinskyClassic
{
    public class Progam
    {
        [STAThread]
        public static void Main(string[] aArgs)
        {
            HelperKinsky helper = new HelperKinsky(aArgs, new Invoker());

            /*OptionParser optParser = appControl.OptionParser;

            OptionString package = new OptionString(null, "--pcache", "Skins/Klimax/800x480/Laptop", "Sets the package cache", "PACKAGECACHE");
            OptionString texture = new OptionString(null, "--tcache", "Skins/Klimax/800x480", "Sets the texture cache", "TEXTURECACHE");
            Rectangle rect = Screen.PrimaryScreen.Bounds;
            if (rect.Width == 800 && rect.Height == 480)
            {
                package = new OptionString(null, "--pcache", "Skins/Klimax/800x480/Touchscreen", "Sets the package cache", "PACKAGECACHE");
                texture = new OptionString(null, "--tcache", "Skins/Klimax/800x480", "Sets the texture cache", "TEXTURECACHE");
            }
            else if (rect.Width == 1024 && rect.Height == 600)
            {
                package = new OptionString(null, "--pcache", "Skins/Klimax/1024x600/Touchscreen", "Sets the package cache", "PACKAGECACHE");
                texture = new OptionString(null, "--tcache", "Skins/Klimax/1024x600", "Sets the texture cache", "TEXTURECACHE");
            }
            optParser.AddOption(package);
            optParser.AddOption(texture);
            OptionString title = new OptionString(null, "--title", "KinskyClassic", "Sets the title of the window", "TITLE");
            optParser.AddOption(title);
            OptionBool developer = new OptionBool(null, "--dev", "Enable developer mode");
            optParser.AddOption(developer);
            OptionBool mouse = new OptionBool(null, "--mouse", "Forces mouse cursor to be visible");
            optParser.AddOption(mouse);*/

            helper.ProcessOptionsFileAndCommandLine();
            Application.Run(new ApplicationCanvas(helper/*, title.Value, app.Version, package.Value, texture.Value, developer.Value, mouse.Value*/));
            helper.Dispose();
        }
    }
} // KinskyClassic
