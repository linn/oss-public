using System.Collections.Generic;
using System.Windows.Forms;
using System;

namespace Linn {
namespace Kinsky {

public class KeyBindings : IDisposable
{
    public KeyBindings() {
        CreateKeyToAction();
    }
    
    public void Dispose() {
        iKeyToAction.Clear();
    }
    
    public string Action(Keys aKey) {
        try {
            return iKeyToAction[aKey];
        } catch(KeyNotFoundException) {
            return "";
        }
    }
    
    private void CreateKeyToAction() {
        // Source actions
        iKeyToAction.Add(Keys.Z | Keys.Alt, "Play");
        iKeyToAction.Add(Keys.X | Keys.Alt, "Pause");
        iKeyToAction.Add(Keys.C | Keys.Alt, "Stop");
        iKeyToAction.Add(Keys.V | Keys.Alt, "Previous");
        iKeyToAction.Add(Keys.B | Keys.Alt, "Next");
        iKeyToAction.Add(Keys.S | Keys.Alt, "Shuffle");
        iKeyToAction.Add(Keys.R | Keys.Alt, "Repeat");
        iKeyToAction.Add(Keys.E | Keys.Alt, "Eject");
        
        // preamp actions
        iKeyToAction.Add(Keys.Subtract, "VolumeDown");
        iKeyToAction.Add(Keys.Add, "VolumeUp");
        iKeyToAction.Add(Keys.NumPad0, "Mute");
        iKeyToAction.Add(Keys.D0, "Mute");
        
        // library actions
        iKeyToAction.Add(Keys.Left, "Back");
        iKeyToAction.Add(Keys.Insert, "InsertMode");
        iKeyToAction.Add(Keys.Delete, "DeleteTrack");
        iKeyToAction.Add(Keys.Delete | Keys.Alt, "DeleteAllTracks");
        iKeyToAction.Add(Keys.Space, "Insert");
        
        // list actions
        iKeyToAction.Add(Keys.Up, "Up");
        iKeyToAction.Add(Keys.PageUp, "PageUp");
        iKeyToAction.Add(Keys.Down, "Down");
        iKeyToAction.Add(Keys.PageDown, "PageDown");
        iKeyToAction.Add(Keys.Enter, "Select");
        iKeyToAction.Add(Keys.Right, "Select");
        
        iKeyToAction.Add(Keys.Tab, "SwapFocus");
        iKeyToAction.Add(Keys.Home, "SwitchView");
    }
    
    private Dictionary<Keys, string> iKeyToAction = new Dictionary<Keys, string>();
}

} // Kinsky
} // Linn
