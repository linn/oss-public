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
    public class AppletFactorySysLog : AppletFactory
    {
        public AppletFactorySysLog() : base("SysLog") {
        }

        public override Applet Create(Target aTarget) {
            return new AppletSysLog(aTarget);
        }
    }

    public class AppletSysLog : Applet
    {
        public AppletSysLog(Target aTarget) : base(aTarget) {
        }

        public override Control Ui {
            get {
                return iUi;
            }
        }

        public override void Activate() {
            if (iUi == null) {
                iUi = new SysLog(base.Target);

                //when an applet is active it should be notified when the Target is changed
                base.Target.EventBoxChanged += BoxChangedHandler;
            }
        }

        public override void Deactivate() {
        }

        protected override void BoxChangedHandler(object obj, EventArgsBox e) {
            if (e.BoxArg.State == Box.EState.eOn) {
                iUi.Enable();
            }
            else {
                iUi.Disable();
            }
        }

        protected SysLog iUi = null;
    }
}
