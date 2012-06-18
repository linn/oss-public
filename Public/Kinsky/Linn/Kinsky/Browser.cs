using System.Collections.Generic;
using System;
using System.Threading;
using System.Net;
using System.IO;
using System.Text;

using Linn;
using Linn.Topology;

using Upnp;

namespace Linn.Kinsky
{
    public interface IBrowser
    {
        void Lock();
        void Unlock();

        void Refresh();

        void Up(uint aLevels);
        void Down(container aContainer);
        void Browse(Location aLocation);

        string SelectedId { get; }
        Location Location { get; }

        event EventHandler<EventArgs> EventLocationChanged;
    }

    public class Browser : IBrowser
    {
        public Browser(Location aLocation)
        {
            iMutex = new Mutex(false);

            iSelectedId = string.Empty;
            iLocation = aLocation;

            for (int i=0 ; i<iLocation.Containers.Count ; i++)
            {
                IContainer container = iLocation.Containers[i];
                container.EventContentAdded += ContentAdded;
                container.EventContentRemoved += ContentRemoved;
                container.EventContentUpdated += ContentUpdated;
            }
        }

        public void Refresh()
        {
            if (EventLocationChanged != null)
            {
                EventLocationChanged(this, EventArgs.Empty);
            }
        }

        public void Up(uint aLevels)
        {
            Up(aLevels, true);
        }

        public void Up(uint aLevels, bool aRaiseLocationChangedEvent)
        {
            Lock();

            for (uint i = 0; i < aLevels && iLocation.Containers.Count > 1; ++i)
            {
                IContainer container = iLocation.Current;
                container.EventContentAdded -= ContentAdded;
                container.EventContentRemoved -= ContentRemoved;
                container.EventContentUpdated -= ContentUpdated;

                iSelectedId = container.Metadata.Id;
                iLocation = iLocation.PreviousLocation();
            }

            Unlock();

            if (EventLocationChanged != null && aRaiseLocationChangedEvent)
            {
                EventLocationChanged(this, EventArgs.Empty);
            }
        }

        public void Down(container aContainer)
        {
            Down(aContainer, true);
        }

        public void Down(container aContainer, bool aRaiseLocationChangedEvent)
        {
            Lock();

            IContainer container = iLocation.Current.ChildContainer(aContainer);

            if (container != null)
            {
                iSelectedId = string.Empty;
                iLocation = new Location(iLocation, container);

                container.EventContentAdded += ContentAdded;
                container.EventContentRemoved += ContentRemoved;
                container.EventContentUpdated += ContentUpdated;
            }

            Unlock();

            if (container != null && EventLocationChanged != null && aRaiseLocationChangedEvent)
            {
                EventLocationChanged(this, EventArgs.Empty);
            }
        }

        public void Browse(Location aLocation)
        {
            Lock();

            Location commonAncestor = iLocation.CommonAncestor(aLocation);
            int levelsUp = iLocation.Containers.Count - commonAncestor.Containers.Count;
            Up((uint)levelsUp, false);

            for (int i = commonAncestor.Containers.Count; i < aLocation.Containers.Count; i++)
            {
                container next = aLocation.Containers[i].Metadata;
                Down(next, false);
            }

            Unlock();

            if (EventLocationChanged != null)
            {
                EventLocationChanged(this, EventArgs.Empty);
            }
        }

        public void Lock()
        {
            iMutex.WaitOne();
        }

        public void Unlock()
        {
            iMutex.ReleaseMutex();
        }

        public string SelectedId
        {
            get
            {
                return iSelectedId;
            }
        }

        public Location Location
        {
            get
            {
                return iLocation;
            }
        }

        public event EventHandler<EventArgs> EventLocationChanged;

        private void ContentAdded(object sender, EventArgs e)
        {
            Lock();

            if (sender == iLocation.Current)
            {
                Unlock();

                if (EventLocationChanged != null)
                {
                    EventLocationChanged(this, EventArgs.Empty);
                }
            }
            else
            {
                Unlock();
            }
        }

        private void ContentRemoved(object sender, EventArgsContentRemoved e)
        {
            if (EventLocationChanged != null)
            {
                EventLocationChanged(this, EventArgs.Empty);
            }
        }

        private void ContentUpdated(object sender, EventArgs e)
        {
            if (EventLocationChanged != null)
            {
                EventLocationChanged(this, EventArgs.Empty);
            }
        }

        private Mutex iMutex;

        private Location iLocation;
        private string iSelectedId;
    }
} // Linn.Kinsky

