using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Linn.Kinsky {

public class KeyBindings
{
    public KeyBindings()
    {
        iKeyToAction =  new Dictionary<Keys, string>();

        // Source actions
        iKeyToAction.Add(Keys.Z | Keys.Alt, "Play");
        iKeyToAction.Add(Keys.X | Keys.Alt, "Pause");
        iKeyToAction.Add(Keys.MediaPlayPause, "PlayPause");
        iKeyToAction.Add(Keys.C | Keys.Alt, "Stop");
        iKeyToAction.Add(Keys.MediaStop, "Stop");
        iKeyToAction.Add(Keys.V | Keys.Alt, "Previous");
        iKeyToAction.Add(Keys.MediaPreviousTrack, "Previous");
        iKeyToAction.Add(Keys.B | Keys.Alt, "Next");
        iKeyToAction.Add(Keys.MediaNextTrack, "Next");
        iKeyToAction.Add(Keys.S | Keys.Alt, "Shuffle");
        iKeyToAction.Add(Keys.R | Keys.Alt, "Repeat");
        iKeyToAction.Add(Keys.E | Keys.Alt, "Eject");

        // preamp actions
        iKeyToAction.Add(Keys.Subtract, "VolumeDown");
        iKeyToAction.Add(Keys.VolumeDown, "VolumeDown");
        iKeyToAction.Add(Keys.OemMinus, "VolumeDown");
        iKeyToAction.Add(Keys.Add, "VolumeUp");
        iKeyToAction.Add(Keys.VolumeUp, "VolumeUp");
        iKeyToAction.Add(Keys.Oemplus, "VolumeUp");
        iKeyToAction.Add(Keys.NumPad0, "Mute");
        iKeyToAction.Add(Keys.VolumeMute, "Mute");
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

    public string Action(Keys aKey)
    {
        if (iKeyToAction.ContainsKey(aKey))
        {
            return iKeyToAction[aKey];
        }
        else
        {
            return "";
        }
    }
    
    private Dictionary<Keys, string> iKeyToAction;
}

} // Linn.Kinsky
