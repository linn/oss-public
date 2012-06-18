using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using Upnp;

using Linn;
using Linn.Topology;
using Linn.Kinsky;

namespace KinskyPda.Widgets
{
    public class ViewWidgetButton : IViewWidgetButton
    {
        public void Open()
        {
        }

        public void Close()
        {
        }

        public void SetEnabled(bool aEnabled)
        {
        }

        public event EventHandler<EventArgs> EventClick { add { } remove { } }
    }

    public class ViewWidgetPlaylistDiscPlayer : IViewWidgetPlaylistDiscPlayer
    {

        public void Open()
        {
        }

        public void Close()
        {
        }

        public void Initialised()
        {
        }

        public void Eject()
        {
        }

    }

    public class ViewWidgetReceivers : IViewWidgetReceivers
    {
        public void Open()
        {
        }

        public void Close()
        {
        }

        public void SetSender(ModelSender aSender)
        {
        }
    }
}
