using System;

using OpenHome.Xapp;

namespace RotaryControlXappPrototype
{
    public class MainPage : Page
    {
        public MainPage (string aId, string aViewId)
            : base(aId, aViewId)
        {
        }

        protected override void OnActivated (Session aSession)
        {
            Send("Initialise", "");
        }
    }
}

