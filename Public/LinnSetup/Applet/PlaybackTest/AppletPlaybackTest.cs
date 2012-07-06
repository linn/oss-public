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
    public class AppletFactoryPlaybackTest : AppletFactory
    {
        public AppletFactoryPlaybackTest() : base("PlaybackTest") {
        }

        public override Applet Create(Target aTarget) {
            return new AppletPlaybackTest(aTarget);
        }
    }

    public class AppletPlaybackTest : Applet
    {
        public AppletPlaybackTest(Target aTarget) : base(aTarget) {
        }

        public override Control Ui {
            get {
                return iUi;
            }
        }

        public override void Activate() {
            if (iUi == null) {
                iUi = new PlaybackTest(base.Target);
                base.Target.EventBoxChanged += BoxChangedHandler;
            }
        }

        public override void Deactivate() {
        }

        protected override void BoxChangedHandler(object obj, EventArgsBox e) {
            if (e.BoxArg.State == Box.EState.eOn && !e.BoxArg.IsProxy) {
                iUi.Enable();
            }
            else {
                iUi.Disable();
            }
        }

        protected PlaybackTest iUi = null;
    }
}
