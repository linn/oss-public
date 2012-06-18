using System;
using System.Drawing;
using System.Windows.Forms;

namespace Linn {
namespace Gui {
    
public class Program {
    [STAThread]
    public static void Main() {
        Application.Run(new ApplicationCanvas());
    }
}

public class ApplicationCanvas : Form
{
    public ApplicationCanvas() {
        InitializeComponent();
    }

    private void InitializeComponent() {
        SuspendLayout();
        FormBorderStyle = FormBorderStyle.FixedSingle;
//        ClientSize = new System.Drawing.Size(iCanvas.ClientSize.Width, iCanvas.ClientSize.Height);
        SetStyle(ControlStyles.DoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        UpdateStyles();
//        Controls.Add(iCanvas);
        Name = "TestMouseMove";
        Text = "TestMouseMove";

        try {
            Icon = new System.Drawing.Icon(System.IO.Path.Combine(Application.StartupPath, "../../share/Linn/Gui/Linn.ico"));
        } catch(System.IO.IOException) {} // no icon to set
        ResumeLayout(false);
    }
    
    protected override void OnMouseMove(MouseEventArgs e) {
        long currentTime = DateTime.Now.Ticks;
        base.OnMouseMove(e);
        double ms = TimeSpan.FromTicks(currentTime - iLastMotion).TotalMilliseconds;
        System.Console.WriteLine("tick=" + ms);
        iLastMotion = currentTime;
    }
    
    private long iLastMotion = 0;
}
    
} // Gui
} // Linn
