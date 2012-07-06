using System;

namespace Linn.ProductSupport
{
    // Once a box has been created for an app the box should always exist for the app. When the room changes the box is
    // removed from the tree and a new one is added. Target hides this adding and removing of the box from the apps
    public class Target
    {
        public event EventHandler<EventArgsBox> EventBoxChanged;

        
        public Target(Box aBox, string aNetworkAdaptor) {
            iBox = aBox;
            iBox.EventBoxChanged += BoxChangedHandler;
            iNetworkAdaptor = aNetworkAdaptor;
        }

        public Target(Box aBox)  : this (aBox, "d") {

        }

        public Box Box
        {
            get
            {
                return iBox;
            }
        }

        public string NetworkAdaptor
        {
            get
            {
                return iNetworkAdaptor;
            }
        }

        public void AddBoxChangeEvent(Box box)
        {
            iBox = box;
            iBox.EventBoxChanged += BoxChangedHandler;
        }

        private void BoxChangedHandler(object sender, EventArgsBox e) {
            if (EventBoxChanged != null) {
                EventBoxChanged(this, new EventArgsBox(e.BoxArg));
            }
        }

        private Box iBox;
        private string iNetworkAdaptor = "";
    }
}
