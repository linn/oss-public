using System;
using System.Windows.Forms;
using System.Globalization;

namespace Linn {
namespace Gui {
namespace Editor {

public partial class NewLayoutDialog : Form
{
    public NewLayoutDialog() {
        InitializeComponent();
    }
    
    private void okButton_Click(object sender, EventArgs e) {
    }
    
    public string LayoutNamespace {
        get {
            return nsTextBox.Text;
        }
    }
    
    public float LayoutWidth {
        get {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            return float.Parse(widthTextBox.Text, nfi);
        }
    }
    
    public float LayoutHeight {
        get {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            return float.Parse(heightTextBox.Text, nfi);
        }
    }
}

} // Editor
} // Gui
} // Linn
