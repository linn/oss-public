
using System;
using System.Collections.Generic;

using Upnp;
using Linn;
using Linn.Kinsky;


namespace KinskyPda
{
    internal class BrowserItems
    {
        public BrowserItems(Browser aBrowser)
        {
            iBrowser = aBrowser;
            iState = new State();
            iCollector = null;
            iSelectedId = string.Empty;
        }

        public class State
        {
            public State()
            {
                Container = null;
                Type = EType.eUninitialised;
                Items = null;
                ErrorMessage = string.Empty;
                SelectedId = string.Empty;
            }

            public State(IContainer aContainer)
            {
                Container = aContainer;
                Type = EType.eScanning;
                Items = null;
                ErrorMessage = string.Empty;
                SelectedId = string.Empty;
            }

            public State(IContainer aContainer, IList<upnpObject> aItems, string aSelectedId)
            {
                Container = aContainer;
                Type = EType.eContentOk;
                Items = aItems;
                ErrorMessage = string.Empty;
                SelectedId = aSelectedId;
            }

            public State(IContainer aContainer, string aErrorMessage)
            {
                Container = aContainer;
                Type = EType.eContentError;
                Items = null;
                ErrorMessage = aErrorMessage;
                SelectedId = string.Empty;
            }

            public enum EType
            {
                eUninitialised,
                eScanning,
                eContentOk,
                eContentError
            }

            public readonly IContainer Container;
            public readonly EType Type;
            public readonly IList<upnpObject> Items;
            public readonly string ErrorMessage;
            public readonly string SelectedId;
        }

        public void Start()
        {
            lock (this)
            {
                Assert.Check(iState.Type == State.EType.eUninitialised);

                iBrowser.EventLocationChanged += BrowserLocationChanged;

                // start a collector for the current location
                IContainer newContainer = iBrowser.Location.Current;
                iState = new State(newContainer);
                iCollector = new ContentCollector(newContainer, ItemCollected, CollectorFinished, CollectorError);
            }

            // notify that state has changed
            if (EventStateChanged != null)
                EventStateChanged(this);
        }

        public void Stop()
        {
            ContentCollector toDispose = null;

            lock (this)
            {
                iBrowser.EventLocationChanged -= BrowserLocationChanged;

                // reset state to uninitialised
                iState = new State();
                toDispose = iCollector;
            }

            // notify that state has changed
            if (EventStateChanged != null)
                EventStateChanged(this);

            // clean up the collector
            if (toDispose != null)
                toDispose.Dispose();
        }

        public State CurrentState
        {
            get { return iState; }
        }

        public delegate void DEventHandler(BrowserItems aBrowserItems);
        public event DEventHandler EventStateChanged;
        public delegate void DEventProgress(BrowserItems aBrowserItems, uint aCount, uint aTotal);
        public event DEventProgress EventProgress;

        public void Down(container aContainer)
        {
            Assert.Check(iState.Type == State.EType.eContentOk);

            if (iState.Items.Contains(aContainer))
            {
                iBrowser.Down(aContainer);
            }
        }

        private void BrowserLocationChanged(object sender, EventArgs e)
        {
            IContainer newContainer = iBrowser.Location.Current;
            ContentCollector toDispose = null;
            bool stateChanged = false;

            iSelectedId = iBrowser.SelectedId;

            lock (this)
            {
                // even though this delegate is not hooked up to the browser in the eUninitialised
                // state, there is no guarantee that this will not be called 
                if (iState.Type == State.EType.eUninitialised)
                    return;

                if (newContainer != iState.Container)
                {
                    // current browse container has changed so change
                    // the state
                    iState = new State(newContainer);
                    stateChanged = true;
                }

                // start a new collector - the current collector will need
                // to be disposed outside of the lock
                toDispose = iCollector;
                iCollector = new ContentCollector(newContainer, ItemCollected, CollectorFinished, CollectorError);
            }

            // notify that state has changed
            if (stateChanged && EventStateChanged != null)
                EventStateChanged(this);

            // finally, clean up the previous collector
            if (toDispose != null)
                toDispose.Dispose();
        }

        private void ItemCollected(ContentCollector aCollector, uint aCount, uint aTotal)
        {
            lock (this)
            {
                // ignore any events from collectors other than the current
                if (aCollector != iCollector)
                    return;
            }

            if (EventProgress != null)
                EventProgress(this, aCount, aTotal);
        }

        private void CollectorFinished(ContentCollector aCollector, IContainer aContainer, IList<upnpObject> aItems)
        {
            lock (this)
            {
                // ignore any events from collectors other than the current
                if (aCollector != iCollector)
                    return;

                // change the state
                Assert.Check(iState.Container == aContainer);
                iState = new State(aContainer, aItems, iSelectedId);
            }
            if (EventStateChanged != null)
                EventStateChanged(this);
        }

        private void CollectorError(ContentCollector aCollector, IContainer aContainer, string aMessage)
        {
            lock (this)
            {
                // ignore any events from collectors other than the current
                if (aCollector != iCollector)
                    return;

                // change state
                Assert.Check(iState.Container == aContainer);
                iState = new State(aContainer, aMessage);
            }
            if (EventStateChanged != null)
                EventStateChanged(this);
        }

        // nested private class to get the contents of a container in
        // a background thread
        private class ContentCollector : IContentHandler, IDisposable
        {
            public delegate void DFinished(ContentCollector aCollector, IContainer aContainer, IList<upnpObject> aItems);
            public delegate void DItemCollected(ContentCollector aCollector, uint aCount, uint aTotal);
            public delegate void DError(ContentCollector aCollector, IContainer aContainer, string aMessage);

            public ContentCollector(IContainer aContainer, DItemCollected aItemCollected, DFinished aFinished, DError aError)
            {
                iContainer = aContainer;
                iItemCollected = aItemCollected;
                iFinished = aFinished;
                iError = aError;
                iCount = 0;
                iItems = new List<upnpObject>();

                iLock = new object();

                // need to lock this - the Create method starts the content collector
                // thread which can run until the Open method, below, is called and,
                // given the iCollector has not been set yet, a NullReferenceException
                // is thrown
                lock (iLock)
                {
                    iCollector = ContentCollectorMaster.Create(iContainer, this);
                }
            }

            public void Open(IContentCollector aCollector, uint aCount)
            {
                iCount = aCount;
                iItems.Capacity = (int)iCount;
                if (iCount > 0)
                {
                    // lock in case the content collector has got to this point before
                    // the ContentCollectorMaster.Create() method that is called in the
                    // constructor has returned
                    lock (iLock)
                    {
                        iCollector.Range(0, iCount);
                    }
                }
                else
                {
                    // copy delegate for thread safety
                    DFinished finished = iFinished;
                    if (finished != null)
                        finished(this, iContainer, iItems.AsReadOnly());
                }
            }

            public void Item(IContentCollector aCollector, uint aIndex, upnpObject aObject)
            {
                iItems.Add(aObject);
                DItemCollected itemCollected = iItemCollected;
                if (itemCollected != null)
                    itemCollected(this, (uint)iItems.Count, iCount);

                if (iItems.Count == iCount)
                {
                    // copy delegate for thread safety
                    DFinished finished = iFinished;
                    if (finished != null)
                        finished(this, iContainer, iItems.AsReadOnly());
                }
            }

            public void Items(IContentCollector aCollector, uint aStartIndex, IList<upnpObject> aObjects)
            {
            }

            public void ContentError(IContentCollector aCollector, string aMessage)
            {
                // copy delegate for thread safety
                DError error = iError;
                if (error != null)
                    error(this, iContainer, aMessage);
            }

            public void Dispose()
            {
                iItemCollected = null;
                iFinished = null;
                iError = null;

                // no need to lock the iCollector here - this method is called on the
                // instance of this object so the constructor must have finished - sounds
                // odd but read the comments regarding the iCollector in the constructor
                // and the Open method
                iCollector.Dispose();
            }

            private IContainer iContainer;
            private DItemCollected iItemCollected;
            private DFinished iFinished;
            private DError iError;
            private uint iCount;
            private List<upnpObject> iItems;
            private IContentCollector iCollector;
            private object iLock;
        }

        private Browser iBrowser;
        private State iState;
        private ContentCollector iCollector;
        private string iSelectedId;
    }
}


