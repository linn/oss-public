using System;
using Linn.Topology.Boxes;

namespace LinnSetup
{
    //once a box has been created for an applet the box should always exist for the applet. when room changes the box is
    //removed from the topology tree and a new one is added. target hides this adding and removing of the box from the applets
    public class Target
    {
        public EventHandler<EventArgsBox> EventBoxChanged;

        public Target(Box aBox) {
            iBox = aBox;
            iBox.EventBoxChanged += BoxChangedHandler;
        }

        public Box Box {
            get {
                return iBox;
            }
        }

        public void AddBoxChangeEvent(Box box) {
            iBox = box;
            iBox.EventBoxChanged += BoxChangedHandler;
        }

        private void BoxChangedHandler(object sender, EventArgsBox e) {
            if (EventBoxChanged != null) {
                EventBoxChanged(this, new EventArgsBox(e.BoxArg));
            }
        }

        private Box iBox;
    }
}
