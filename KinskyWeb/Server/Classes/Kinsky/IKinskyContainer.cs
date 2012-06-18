using System;
using System.Collections.Generic;
using System.Linq;
namespace KinskyWeb.Kinsky
{
    public interface IKinskyContainer : IKinskyWidget
    {
        void Add(IKinskyWidget aWidget);
        IKinskyWidget Get(Guid aID);
        void Remove(IKinskyWidget aWidget);
        IEnumerable<IKinskyWidget> Widgets();
        DateTime LastActivity { get; set; }
        uint RequestedTimeoutMilliseconds { get; set; }
        void Subscribe();
        void Unsubscribe();
        bool Expired();
    }

    public abstract class KinskyContainerBase : KinskyWidgetBase, IKinskyContainer
    {
        private const uint kDefaultExpiryTimeoutMilliseconds = 10000;
        private const uint kMaxExpiryTimeoutMilliseconds = 86400000;
        private uint iExpiryTimeoutMilliseconds;

        public KinskyContainerBase()
        {
            this.iWidgets = new Dictionary<Guid, IKinskyWidget>();
            iExpiryTimeoutMilliseconds = kDefaultExpiryTimeoutMilliseconds;
        }

        private Dictionary<Guid, IKinskyWidget> iWidgets { get; set; }

        #region IKinskyContainer Members

        public virtual void Add(IKinskyWidget aWidget)
        {
            lock (this.iWidgets)
            {
                if (!this.iWidgets.ContainsKey(aWidget.ID))
                {
                    this.iWidgets.Add(aWidget.ID, aWidget);
                }
            }
            aWidget.Container = this;
        }

        public virtual IKinskyWidget Get(Guid aID)
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

        public virtual void Remove(IKinskyWidget aWidget)
        {
            lock (this.iWidgets)
            {
                if (iWidgets.ContainsKey(aWidget.ID))
                {
                    iWidgets.Remove(aWidget.ID);
                }
            }
            aWidget.OnContainerTerminated();
            aWidget.Container = null;
        }


        public virtual void Unsubscribe()
        {
            lock (iWidgets)
            {
                foreach (IKinskyWidget widget in iWidgets.Values.ToArray())
                {
                    WidgetRegistry.GetDefault().Remove(widget.ID);
                }
            }
        }

        public virtual void Subscribe()
        {
            this.LastActivity = DateTime.Now;
        }

        public virtual uint RequestedTimeoutMilliseconds
        {
            get
            {
                return iExpiryTimeoutMilliseconds;
            }
            set
            {
                if (value > kMaxExpiryTimeoutMilliseconds)
                {
                    iExpiryTimeoutMilliseconds = kMaxExpiryTimeoutMilliseconds;
                }
                else if (value == 0)
                {
                    iExpiryTimeoutMilliseconds = kDefaultExpiryTimeoutMilliseconds;
                }
                else
                {
                    iExpiryTimeoutMilliseconds = value;
                }
            }
        }
        public virtual DateTime LastActivity { get; set; }
        public virtual bool Expired()
        {
            DateTime expiryTime = DateTime.Now.AddMilliseconds(Math.Abs(RequestedTimeoutMilliseconds) * -1);
            return this.LastActivity.CompareTo(expiryTime) < 0;
        }


        public virtual IEnumerable<IKinskyWidget> Widgets()
        {
            lock (iWidgets)
            {
                return iWidgets.Values.ToList().AsEnumerable();
            }
        }

        #endregion
    }
}
