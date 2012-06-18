using System;
using System.Collections.Generic;
using System.Drawing;

using Linn.ControlPoint.Upnp;
using Linn.Topology.Boxes;

namespace LinnSetup
{
    public class TargetMediator
    {
        public TargetMediator(Target aTarget, AppletManager aManager, EventServerUpnp aEventServer) {
            iTarget = aTarget;
            iApplets = aManager.CreateApplets(iTarget, aEventServer);
            iCurrentApplet = iApplets[0];
        }
        
        public Target Target {
            get {
                return iTarget;
            }
        }

        public Applet CurrentApplet {
            get {
                return iCurrentApplet;
            }
        }

        public int AppletSelectedIndex {
            get {
                return iApplets.IndexOf(iCurrentApplet);
            }
        }

        public Color StatusColor {
            get {
                if (iTarget.Box.State == Box.EState.eFallback) {
                    return Color.Yellow;
                }
                else if (iTarget.Box.State == Box.EState.eOn) {
                    return Color.Green;
                }
                else {
                    return Color.Red;
                }
            }
        }

        public void Activate(int index) {
            iCurrentApplet = iApplets[index];
            iCurrentApplet.Activate();

            Deactivate();
        }

        public override string ToString() {
            return iTarget.Box.ToString();
        }

        //deactivate all non active applets
        private void Deactivate() {
            foreach (Applet applet in iApplets) {
                if (applet == iCurrentApplet) {
                    continue;
                }

                applet.Deactivate();
            }
        }

        private Target iTarget;
        private List<Applet> iApplets;
        private Applet iCurrentApplet;
    }
}
