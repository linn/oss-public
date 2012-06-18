using System;
using System.Drawing;
using System.Collections.Generic;

using MonoMac.AppKit;
using MonoMac.Foundation;

namespace MagicMouseControl
{
    public class InputTrackerDoubleTouch : InputTracker
    {
        public InputTrackerDoubleTouch()
        {
            iThreshold = 1.0f;
        }

        public override void TouchesBeganWithEvent(NSEvent aEvent)
        {
            if(!Enabled)
            {
                return;
            }

            NSSet touches = aEvent.TouchesMatchingPhaseinView(NSTouchPhase.Touching, View);

            if(touches.Count == 2)
            {
                iInitialPoint = View.ConvertPointFromBase(aEvent.LocationInWindow);
                foreach(NSTouch t in touches.ToArray<NSTouch>())
                {
                    iInitialTouches.Add(t);
                    iCurrentTouches.Add(t);
                }
            }
            else if(touches.Count > 2)
            {
                if(iIsTracking)
                {
                    CancelTracking();
                }
            }
        }

        public override void TouchesMovedWithEvent(NSEvent aEvent)
        {
            if(!Enabled)
            {
                return;
            }

            iModifiers = aEvent.ModifierFlags;
            NSSet touches = aEvent.TouchesMatchingPhaseinView(NSTouchPhase.Touching, View);

            if(touches.Count == 2 && iInitialTouches.Count > 0)
            {
                foreach(NSTouch t in touches.ToArray<NSTouch>())
                {
                    if(t == iInitialTouches[0])
                    {
                        iCurrentTouches[0] = t;
                    }
                    else
                    {
                        iCurrentTouches[1] = t;
                    }
                }

                if(!iIsTracking)
                {
                    PointF deltaOrigin = DeltaOrigin();
                    SizeF deltaSize = DeltaSize();

                    if(Math.Abs(deltaOrigin.X) > iThreshold || Math.Abs(deltaOrigin.Y) > iThreshold || Math.Abs(deltaSize.Width) > iThreshold || Math.Abs(deltaSize.Height) > iThreshold)
                    {
                        iIsTracking = true;
                        if(EventBegin != null)
                        {
                            EventBegin(this, EventArgs.Empty);
                        }
                    }
                }
                else
                {
                    if(EventUpdate != null)
                    {
                        EventUpdate(this, EventArgs.Empty);
                    }
                }
            }
        }

        public override void TouchesEndedWithEvent(NSEvent aEvent)
        {
            if(!Enabled)
            {
                return;
            }

            iModifiers = aEvent.ModifierFlags;
            CancelTracking();
        }

        public override void TouchesCancelledWithEvent(NSEvent aEvent)
        {
            CancelTracking();
        }

        public EventHandler<EventArgs> EventBegin;
        public EventHandler<EventArgs> EventUpdate;
        public EventHandler<EventArgs> EventEnd;

        public override void CancelTracking()
        {
            if(iIsTracking)
            {
                if(EventEnd != null)
                {
                    EventEnd(this, EventArgs.Empty);
                }
            }

            iIsTracking = false;
            iInitialTouches.Clear();
            iCurrentTouches.Clear();
        }

        private PointF DeltaOrigin()
        {
            if(iInitialTouches.Count == 0 || iCurrentTouches.Count == 0)
            {
                return PointF.Empty;
            }

            float x1 = Math.Min(iInitialTouches[0].NormalizedPosition.X, iInitialTouches[1].NormalizedPosition.X);
            float x2 = Math.Min(iCurrentTouches[0].NormalizedPosition.X, iCurrentTouches[1].NormalizedPosition.X);
            float y1 = Math.Min(iInitialTouches[0].NormalizedPosition.Y, iInitialTouches[1].NormalizedPosition.Y);
            float y2 = Math.Min(iCurrentTouches[0].NormalizedPosition.Y, iCurrentTouches[1].NormalizedPosition.Y);

            SizeF deviceSize = iInitialTouches[0].DeviceSize;
            PointF delta = new PointF((x2 - x1) * deviceSize.Width, (y2 - y1) * deviceSize.Height);

            return delta;
        }

        private SizeF DeltaSize()
        {
            if(iInitialTouches.Count == 0 || iCurrentTouches.Count == 0)
            {
                return SizeF.Empty;
            }

            float x1, x2, y1, y2, width1, width2, height1, height2;

            x1 = Math.Min(iInitialTouches[0].NormalizedPosition.X, iInitialTouches[1].NormalizedPosition.X);
            x2 = Math.Max(iInitialTouches[0].NormalizedPosition.X, iInitialTouches[1].NormalizedPosition.X);
            width1 = x2 - x1;

            y1 = Math.Min(iInitialTouches[0].NormalizedPosition.Y, iInitialTouches[1].NormalizedPosition.Y);
            y2 = Math.Max(iInitialTouches[0].NormalizedPosition.Y, iInitialTouches[1].NormalizedPosition.Y);
            height1 = y2 - y1;

            x1 = Math.Min(iCurrentTouches[0].NormalizedPosition.X, iCurrentTouches[1].NormalizedPosition.X);
            x2 = Math.Max(iCurrentTouches[0].NormalizedPosition.X, iCurrentTouches[1].NormalizedPosition.X);
            width2 = x2 - x1;

            y1 = Math.Min(iCurrentTouches[0].NormalizedPosition.Y, iCurrentTouches[1].NormalizedPosition.Y);
            y2 = Math.Max(iCurrentTouches[0].NormalizedPosition.Y, iCurrentTouches[1].NormalizedPosition.Y);
            height2 = y2 - y1;

            SizeF deviceSize = iInitialTouches[0].DeviceSize;
            SizeF delta = new SizeF((width2 - width1) * deviceSize.Width, (height2 - height1) * deviceSize.Height);

            return delta;
        }

        private float iThreshold;

        private bool iIsTracking;

        private PointF iInitialPoint;
        private NSEventModifierMask iModifiers;

        private List<NSTouch> iInitialTouches;
        private List<NSTouch> iCurrentTouches;
    }
}

