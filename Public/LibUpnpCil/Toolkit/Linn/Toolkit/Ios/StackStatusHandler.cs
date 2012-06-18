using System;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Linn;

namespace Linn.Toolkit.Ios
{
    public class StackStatusHandler : Linn.StackStatusHandler
    {
        private class Delegate : UIAlertViewDelegate
        {
            public override void Dismissed(UIAlertView aAlertView, int aButtonIndex)
            {
                if(aButtonIndex == 0)
                {
                    Console.WriteLine("asked to configure");
                }
                else if(aButtonIndex == 1)
                {
                    Console.WriteLine("asked to close");
                }
            }
        }

        public StackStatusHandler(string aTitle)
        {
            iTitle = aTitle;
        }

        public override void StackStatusChanged(object sender, EventArgsStackStatus e)
        {
        }
        
        public override void StackStatusStartupChanged(object sender, EventArgsStackStatus e)
        {
            switch (e.Status.State)
            {
                case EStackState.eStopped:
                case EStackState.eOk:
                    break;
    
                case EStackState.eNoInterface:
                {
                    string msg = iTitle + " requires a network adapter to be configured.";
                    msg += "\n\nWould you like to select a network adapter now?";

                    UIAlertView alert = new UIAlertView(iTitle, msg, new Delegate(), "Yes", new string[] { "No" });
                    alert.Show();

                    break;
                }

                case EStackState.eBadInterface:
                case EStackState.eNonexistentInterface:
                {
                    string msg = "A problem occurred with the configured network adapter (" + e.Status.Interface.Name + ")";
                    msg += "\n\nWould you like to select a different network adapter now?";

                    UIAlertView alert = new UIAlertView(iTitle, msg, new Delegate(), "Yes", new string[] { "No" });
                    alert.Show();

                    break;
                }
            }
        }
        
        public override void StackStatusOptionsChanged(object sender, EventArgsStackStatus e)
        {
        }

        private string iTitle;
    }
}