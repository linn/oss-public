using System;
using System.Drawing;
using System.Windows.Forms;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using System.Threading;

namespace Linn {
namespace Gui {
    
public class Program {
    [STAThread]
    public static void Main(string[] aArgs) {
        Helper helper = new Helper(aArgs);
        helper.ProcessCommandLine();
        
        Ticker ticker = new Ticker();
        Application.Run(new ApplicationCanvas());
        float exeTime = ticker.MilliSeconds;
        RendererGdiPlus renderer = (RendererGdiPlus)Renderer.Instance;
        System.Console.WriteLine("Min = " + renderer.Stats.MinimumMs + "ms");
        System.Console.WriteLine("Avg = " + renderer.Stats.AverageMs + "ms");
        System.Console.WriteLine("Max = " + renderer.Stats.MaximumMs + "ms");
        System.Console.WriteLine("Total = " + renderer.Stats.TotalMs + ", Frames = " + renderer.Stats.Frames);
        System.Console.WriteLine("Execution time = " + exeTime + "secs");
        
        helper.Dispose();
    }
}

public class ApplicationCanvas : Form
{
    public ApplicationCanvas() {
        iCanvas = new CanvasGdiPlus();
        new RendererGdiPlus(iCanvas);

        TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../../share/Linn/Gui/Editor/Default"));
        TextureManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../../share/Linn/Gui/Skins/Klimax/800x480"));
        PackageManager.Instance.AddPath(System.IO.Path.Combine(Application.StartupPath, "../../../share/Linn/Gui/Skins/Klimax/800x480/Laptop"));

        iCanvas.Load("Main.xml");

        InitializeComponent();
        
        ClientSize = new System.Drawing.Size(iCanvas.ClientSize.Width, iCanvas.ClientSize.Height);
        Controls.Add(iCanvas);
        
        try {
            Icon = new System.Drawing.Icon(System.IO.Path.Combine(Application.StartupPath, "../../../../share/Linn/Gui/Linn.ico"));
        } catch(System.IO.IOException) {} // no icon to set
        
        iCanvas.Paint += TestRenderPda_Paint;
    }

    private void InitializeComponent() {
        SuspendLayout();
        FormBorderStyle = FormBorderStyle.FixedSingle;
        Name = "TestRender";
        Text = "TestRender";
        ResumeLayout(false);
    }
    
    delegate void RenderCallback();
    private void Render()
    {
        if (InvokeRequired)
        {
            RenderCallback d = new RenderCallback(Render);
            Invoke(d);
        }
        else
        {
            RendererGdiPlus renderer = (RendererGdiPlus)Renderer.Instance;
            for(int i = 0; i < kNumIterations; ++i) {
                Ticker total = new Ticker();
                Ticker ticker = new Ticker();
                Graphics g = iCanvas.CreateGraphics();
                renderer.Lock();
                renderer.RenderTarget = g;
                Console.WriteLine("CreateGraphics: i=" + i + ", ms=" + ticker.MilliSeconds);
                ticker.Reset();
                Console.WriteLine("TickCount(b,v)=" + ticker.StartTime);
                VisitorRender visitor = new VisitorRender(renderer);
                visitor.Render(iCanvas.CurrLayout.Root);
                renderer.Unlock();
                float ms = ticker.MilliSeconds;
                Console.WriteLine("TickCount(e,v)=" + ticker.EndTime);
                Console.WriteLine("Render: i=" + i + ", ms=" + ms);
                ticker.Reset();
                g.Dispose();
                ms = total.MilliSeconds;
                Console.WriteLine("Dispose: i=" + i + ", ms=" + ticker.MilliSeconds);

            }
            Close();
        }
    }
    
    private void TestRenderPda_Paint(object sender, PaintEventArgs e)
    {
        if (iThread == null)
        {
            iThread = new Thread(Render);
            iThread.IsBackground = true;
            iThread.Start();
        }
    }
    
    private const int kNumIterations = 1000;
    private CanvasGdiPlus iCanvas = null;
    private Thread iThread = null;
}
    
} // Gui
} // Linn
