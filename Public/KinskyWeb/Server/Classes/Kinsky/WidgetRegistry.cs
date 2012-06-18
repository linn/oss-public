using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Linn;
using Timer = System.Timers.Timer;

namespace KinskyWeb.Kinsky
{
    public class WidgetRegistry
    {
        private Dictionary<Guid, IKinskyWidget> iWidgets;
        private Timer iHousekeeper;
        private object iHousekeeperLock = new object();
        private uint kHousekeeperIntervalMilliseconds = 10000;


        private static WidgetRegistry iWidgetRegistry = new WidgetRegistry();
        public static WidgetRegistry GetDefault()
        {
            return iWidgetRegistry;
        }

        private WidgetRegistry()
        {
            iWidgets = new Dictionary<Guid, IKinskyWidget>();
            iHousekeeper = new Timer();
            iHousekeeper.Elapsed += new ElapsedEventHandler((sender, e) =>
            {
                lock (iHousekeeperLock)
                {
                    List<IKinskyContainer> expiredcontainers;
                    lock (iWidgets)
                    {
                        expiredcontainers = (from p in iWidgets.Values.OfType<IKinskyContainer>()
                                             where p.Expired() == true
                                             select p).ToList();
                    }
                    foreach (IKinskyContainer expired in expiredcontainers)
                    {
                        Trace.WriteLine(Trace.kKinskyWeb, String.Format("Removing expired container: {0}", expired.ID));
                        expired.Unsubscribe();
                        Remove(expired.ID);
                    }
                }
            });
            iHousekeeper.Interval = kHousekeeperIntervalMilliseconds;
            iHousekeeper.Start();
        }

        public Guid Add(IKinskyWidget aWidget)
        {
            return Add(aWidget, Guid.Empty);
        }

        public Guid Add(IKinskyWidget aWidget, Guid aContainerID)
        {
            Guid g = DoAdd(aWidget);
            Trace.WriteLine(Trace.kKinskyWeb, String.Format("Adding widget of type {0} with ID {1} to container {2}", 
                aWidget.GetType().ToString(),
                aWidget.ID.ToString(),
                aContainerID.ToString()));
            if (!(aWidget is IKinskyContainer))
            {
                IKinskyContainer container = Get(aContainerID) as IKinskyContainer;
                if (container != null)
                {
                    container.Add(aWidget);
                }
            }
            return g;
        }

        public void Remove(Guid aWidgetID)
        {
            IKinskyWidget widget = Get(aWidgetID);
            if (widget != null)
            {
                String containerID = "NULL";
                if (widget.Container != null)
                {
                    containerID = widget.Container.ID.ToString();
                }
                Trace.WriteLine(Trace.kKinskyWeb, String.Format("Removing widget of type {0} with ID {1} from container {2}",
                widget.GetType().ToString(),
                widget.ID.ToString(),
                containerID));
                DoRemove(widget.ID);
                if (widget.Container != null)
                {
                    widget.Container.Remove(widget);
                }
            }
        }

        public bool Contains(Guid aWidgetID)
        {
            lock (iWidgets)
            {
                return iWidgets.ContainsKey(aWidgetID);
            }
        }

        public IKinskyWidget Get(Guid aID)
        {
            lock (iWidgets)
            {
                if (iWidgets.ContainsKey(aID))
                {
                    return iWidgets[aID];
                }
                else
                {
                    return null;
                }
            }
        }

        private Guid DoAdd(IKinskyWidget aWidget)
        {
            Guid id = Guid.NewGuid();
            lock (iWidgets)
            {
                while (iWidgets.ContainsKey(id))
                {
                    id = Guid.NewGuid();
                }
                aWidget.ID = id;
                iWidgets.Add(id, aWidget);
                if (aWidget is KinskyContainer)
                {
                    Trace.WriteLine(Trace.kKinskyWeb,"Added container.  Container count: " + iWidgets.Values.OfType<IKinskyContainer>().Count());
                }
            }
            return id;
        }

        private void DoRemove(Guid aWidgetID)
        {
            lock (iWidgets)
            {
                if (iWidgets.ContainsKey(aWidgetID))
                {
                    iWidgets.Remove(aWidgetID);
                }
            }
        }

        public void Close()
        {
            lock (iWidgets)
            {
                List<IKinskyContainer> containers = (from p in iWidgets.Values.OfType<IKinskyContainer>()
                                                            select p).ToList();
                foreach (IKinskyContainer c in containers)
                {
                    c.Unsubscribe();
                    iWidgets.Remove(c.ID);
                }
            }
        }

    }
}
