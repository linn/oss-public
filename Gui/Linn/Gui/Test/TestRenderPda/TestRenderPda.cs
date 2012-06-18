using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Linn.Gui.Resources;
using Linn.Gui.Scenegraph;
using Linn.Gui;
using System.Reflection;
using System.Threading;

namespace Linn {
namespace Gui {
namespace Test {

    public partial class TestRenderPda : Form
{
    public TestRenderPda()
    {
        iCanvas = new CanvasGdi();
        new RendererGdi(iCanvas);

        string dir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
        TextureManager.Instance.AddPath(System.IO.Path.Combine(dir, "share/Linn/Gui/Editor/Default"));
        TextureManager.Instance.AddPath(System.IO.Path.Combine(dir, "share/Linn/Gui/Skins/Sneaky/640x480"));
        PackageManager.Instance.AddPath(System.IO.Path.Combine(dir, "share/Linn/Gui/Skins/Sneaky/640x480"));

        iCanvas.Load("Main.xml");

        InitializeComponent();

        ClientSize = new System.Drawing.Size(iCanvas.ClientSize.Width, iCanvas.ClientSize.Height);
        Controls.Add(iCanvas);
        iCanvas.Paint += TestRenderPda_Paint;
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
            RendererGdi renderer = (RendererGdi)Renderer.Instance;
            for (int i = 0; i < kNumIterations; ++i)
            {
                Ticker total = new Ticker();
                Ticker ticker = new Ticker();
                Graphics g = iCanvas.CreateGraphics();
                renderer.Lock();
                renderer.RenderTarget = g;
                Trace.WriteLine(Trace.kRendering, "CreateGraphics: i=" + i + ", ms=" + ticker.MilliSeconds);
                ticker.Reset();
                Trace.WriteLine(Trace.kRendering, "TickCount(b,v)=" + ticker.StartTime);
                VisitorRender visitor = new VisitorRender(renderer);
                visitor.Render(iCanvas.CurrLayout.Root);
                renderer.Unlock();
                float ms = ticker.MilliSeconds;
                Trace.WriteLine(Trace.kRendering, "TickCount(e,v)=" + ticker.EndTime);
                Trace.WriteLine(Trace.kRendering, "Render: i=" + i + ", ms=" + ms);
                ticker.Reset();
                g.Dispose();
                ms = total.MilliSeconds;
                Trace.WriteLine(Trace.kRendering, "Dispose: i=" + i + ", ms=" + ticker.MilliSeconds);
            }
            Close();
        }
    }

    private const int kNumIterations = 500;
    private CanvasGdi iCanvas = null;
    private Thread iThread = null;

    private void InitializeComponent()
    {
        this.SuspendLayout();
        // 
        // TestRenderPda
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
        this.ClientSize = new System.Drawing.Size(240, 320);
        this.Location = new System.Drawing.Point(0, 0);
        this.Name = "TestRenderPda";
        this.Text = "TestRenderPda";
        this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
        this.ResumeLayout(false);

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
}

public class Program
{
    [MTAThread]
    public static void Main()
    {
        AppWinForm app = new AppWinForm(new App(string[] aArgs));
        app.Start();
        Trace.Level = Trace.kRendering;

        Ticker ticker = new Ticker();
        Application.Run(new TestRenderPda());
        float exeTime = ticker.MilliSeconds;
        RendererGdi renderer = (RendererGdi)Renderer.Instance;
        Trace.WriteLine(Trace.kRendering, "Min = " + renderer.Stats.MinimumMs + "ms");
        Trace.WriteLine(Trace.kRendering, "Avg = " + renderer.Stats.AverageMs + "ms");
        Trace.WriteLine(Trace.kRendering, "Max = " + renderer.Stats.MaximumMs + "ms");
        Trace.WriteLine(Trace.kRendering, "Total = " + renderer.Stats.TotalMs + ", Frames = " + renderer.Stats.Frames);
        Trace.WriteLine(Trace.kRendering, "Execution time = " + exeTime + "ms");

        app.Dispose();
    }
}

} // Test
} // Gui
} // Linn
