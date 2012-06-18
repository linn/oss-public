using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Linn.Topology.Boxes;

namespace LinnSetup
{
    public abstract class Applet
    {
        public abstract Control Ui { get; }
        public abstract void Activate();
        public abstract void Deactivate();
        protected abstract void BoxChangedHandler(object obj, EventArgsBox e);

        protected Target Target {
            get {
                return iTarget;
            }
        }

        protected Applet(Target aTarget) {
            iTarget = aTarget;
        }

        private Target iTarget;
    }
}
