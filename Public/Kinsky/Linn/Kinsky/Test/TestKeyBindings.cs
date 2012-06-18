using Linn.TestFramework;
using Linn.Kinsky;
using System.Windows.Forms;

internal class SuiteKeyToAction : Suite
{
    public SuiteKeyToAction() : base("Key to action tests") {
    }
    
    public override void Setup() {
    }
    
    public override void Test() {
        KeyBindings keyBindings = new KeyBindings();
        
        TEST(keyBindings.Action(Keys.Alt | Keys.Z) == "Play");
        TEST(keyBindings.Action(Keys.X | Keys.Alt) == "Pause");
        TEST(keyBindings.Action(Keys.C | Keys.Alt) == "Stop");
        TEST(keyBindings.Action(Keys.V | Keys.Alt) == "Previous");
        TEST(keyBindings.Action(Keys.B | Keys.Alt) == "Next");
        TEST(keyBindings.Action(Keys.S | Keys.Alt) == "Shuffle");
        TEST(keyBindings.Action(Keys.R | Keys.Alt) == "Repeat");
        
        // preamp actions
        TEST(keyBindings.Action(Keys.Subtract) == "VolumeDown");
        TEST(keyBindings.Action(Keys.OemMinus) == "VolumeDown");
        TEST(keyBindings.Action(Keys.Add) == "VolumeUp");
        TEST(keyBindings.Action(Keys.Oemplus) == "VolumeUp");
        TEST(keyBindings.Action(Keys.NumPad0) == "Mute");
        TEST(keyBindings.Action(Keys.D0) == "Mute");
        
        // library actions
        TEST(keyBindings.Action(Keys.Left) == "Back");
        TEST(keyBindings.Action(Keys.Insert) == "InsertMode");
        TEST(keyBindings.Action(Keys.Delete) == "DeleteTrack");
        TEST(keyBindings.Action(Keys.Alt | Keys.Delete) == "DeleteAllTracks");
        TEST(keyBindings.Action(Keys.Space) == "Insert");
        
        // list actions
        TEST(keyBindings.Action(Keys.Up) == "Up");
        TEST(keyBindings.Action(Keys.PageUp) == "PageUp");
        TEST(keyBindings.Action(Keys.Down) == "Down");
        TEST(keyBindings.Action(Keys.PageDown) == "PageDown");
        TEST(keyBindings.Action(Keys.Enter) == "Select");
        TEST(keyBindings.Action(Keys.Right) == "Select");
        
        TEST(keyBindings.Action(Keys.Tab) == "SwapFocus");
        TEST(keyBindings.Action(Keys.Home) == "SwitchView");
    }
}

class Program {
    public static void Main() {        
        Runner runner = new Runner("KeyBindings Tests");
        runner.Add(new SuiteKeyToAction());
        runner.Run();
    }
}

