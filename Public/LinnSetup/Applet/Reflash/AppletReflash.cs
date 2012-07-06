using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Linn;
using Linn.ControlPoint.Upnp;
using Linn.ProductSupport;

namespace LinnSetup
{
    public class AppletFactoryReflash : AppletFactory
    {
        public AppletFactoryReflash() : base("Reflash") {
        }

        public override Applet Create(Target aTarget) {
            return new AppletReflash(aTarget);
        }
    }

    public class AppletReflash : Applet
    {
        public AppletReflash(Target aTarget) : base(aTarget) {
        }

        public override Control Ui {
            get {
                return iUi;
            }
        }

        public override void Activate() {
            if (iUi == null) {
                iUi = new Reflash(base.Target);
                base.Target.EventBoxChanged += BoxChangedHandler;
            }
        }

        public override void Deactivate() {
        }

        protected override void BoxChangedHandler(object obj, EventArgsBox e) {
            if (e.BoxArg.State != Box.EState.eOff && !e.BoxArg.IsProxy) {
                iUi.Enable();
            }
            else {
                iUi.Disable();
            }
        }

        protected Reflash iUi = null;
    }
}
