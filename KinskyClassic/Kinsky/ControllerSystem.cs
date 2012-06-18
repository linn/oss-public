using System;
using System.Net;
using Linn.Gui;
using Linn.Gui.Scenegraph;
using Linn.Gui.Resources;
using System.Diagnostics;
using System.Threading;
using Linn.Topology;

namespace Linn {
namespace Kinsky {

    public class ControllerSystem : IMessengerObserver
    {
        public ControllerSystem(Node aRoot, IPAddress aInterface)
        {
            iRoot = aRoot;
            iHouse = new House(aInterface);

            iViewRooms = new ViewRooms(iRoot, iHouse);
            iViewSources = new ViewSources(iRoot, iHouse);

            Messenger.Instance.EEventAppMessage += Receive;
        }

        public void Start()
        {
            iHouse.Start();
        }

        public void Stop()
        {
            iHouse.Stop();
        }

        public void Receive(Message aMessage)
        {
            // handle status clicked

            if (aMessage.Fullname == "StatusBar.Status")
            {
                if (aMessage is MsgHit)
                {
                    iHouse.Rescan();
                }
            }
        }

        private Node iRoot;
        private House iHouse;
        private ViewRooms iViewRooms;
        private ViewSources iViewSources;
    }

} // Kinsky
} // Linn
