
using System.Collections.Generic;
using System;
using System.Threading;
using System.IO;

using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.Topology;
using Upnp;

namespace Linn.Kinsky
{
    public class PlaySupport : IPlaylistSupport
    {
        private class PlayParams
        {
            public enum EPlayType
            {
                ePlayNow,
                ePlayNext,
                ePlayLater,
                ePlayInsert
            }

            public PlayParams(EPlayType aPlayType, IMediaRetriever aRetriever)
            {
                PlayType = aPlayType;
                Retriever = aRetriever;
                InsertAfterId = 0;
            }

            public PlayParams(EPlayType aPlayType, IMediaRetriever aRetriever, uint aInsertAfterId)
            {
                PlayType = aPlayType;
                Retriever = aRetriever;
                InsertAfterId = aInsertAfterId;
            }

            public EPlayType PlayType;
            public uint InsertAfterId;
            public IMediaRetriever Retriever;
        }

        public PlaySupport()
        {
            iLock = new object();
            iEventInsert = new ManualResetEvent(false);
        }

        public void Open()
        {
            Assert.Check(!iOpen);

            iPlayParams = null;
            iAborting = false;

            iEventInsert.Reset();

            iThreadInsert = new Thread(ProcessInsert);
            iThreadInsert.Name = "PlaySupport";
            iThreadInsert.IsBackground = true;

            iThreadInsert.Start();

            iOpen = true;

            if (EventIsOpenChanged != null)
            {
                EventIsOpenChanged(this, EventArgs.Empty);
            }
        }

        public void Close()
        {
            iAborting = true;
            
            iThreadInsert.Abort();
            iThreadInsert.Join();

            iThreadInsert = null;

            iOpen = false;

            if (EventIsOpenChanged != null)
            {
                EventIsOpenChanged(this, EventArgs.Empty);
            }
        }

        public bool IsOpen()
        {
            return iOpen;
        }

        public bool IsInserting()
        {
            lock (iLock)
            {
                return (iPlayParams != null);
            }
        }

        public bool IsDragging()
        {
            return iDragging;
        }

        public bool IsInsertAllowed()
        {
            return iInsertAllowed;
        }

        protected void FireEventPlayNowRequest()
        {
            if (EventPlayNowRequest != null)
            {
                EventPlayNowRequest(this, EventArgs.Empty);
            }
        }

        protected void FireEventPlayNextRequest()
        {
            if (EventPlayNextRequest != null)
            {
                EventPlayNextRequest(this, EventArgs.Empty);
            }
        }

        protected void FireEventPlayLaterRequest()
        {
            if (EventPlayLaterRequest != null)
            {
                EventPlayLaterRequest(this, EventArgs.Empty);
            }
        }

        public void SetDragging(bool aDragging)
        {
            if (aDragging != iDragging)
            {
                iDragging = aDragging;

                if (EventIsDraggingChanged != null)
                {
                    EventIsDraggingChanged(this, EventArgs.Empty);
                }
            }
        }

        public void SetInsertAllowed(bool aInsertAllowed)
        {
            if (aInsertAllowed != iInsertAllowed)
            {
                iInsertAllowed = aInsertAllowed;

                if (EventIsInsertAllowedChanged != null)
                {
                    EventIsInsertAllowedChanged(this, EventArgs.Empty);
                }
            }
        }

        public void PlayNow(IMediaRetriever aMediaRetriever)
        {
            Play(aMediaRetriever, PlayParams.EPlayType.ePlayNow);
        }

        public void PlayNext(IMediaRetriever aMediaRetriever)
        {
            Play(aMediaRetriever, PlayParams.EPlayType.ePlayNext);
        }

        public void PlayLater(IMediaRetriever aMediaRetriever)
        {
            Play(aMediaRetriever, PlayParams.EPlayType.ePlayLater);
        }

        private void Play(IMediaRetriever aMediaRetriever, PlayParams.EPlayType aPlayType)
        {
            EventHandler<EventArgs> ev = null;

            lock (iLock)
            {
                if (iPlayParams == null)
                {
                    iPlayParams = new PlayParams(aPlayType, aMediaRetriever);
                    iEventInsert.Set();
                    ev = EventIsInsertingChanged;
                }
            }

            if (ev != null)
            {
                ev(this, EventArgs.Empty);
            }
        }

        public void PlayInsert(uint aInsertAfterId, IMediaRetriever aMediaRetriever)
        {
            EventHandler<EventArgs> ev = null;

            lock (iLock)
            {
                if (iPlayParams == null)
                {
                    iPlayParams = new PlayParams(PlayParams.EPlayType.ePlayInsert, aMediaRetriever, aInsertAfterId);
                    iEventInsert.Set();
                    ev = EventIsInsertingChanged;
                }
            }

            if (ev != null)
            {
                ev(this, EventArgs.Empty);
            }
        }

        private void ProcessInsert()
        {
            try
            {
                while (true)
                {
                    iEventInsert.WaitOne();

                    if(iAborting)
                    {
                        break;
                    }

                    Assert.Check(iPlayParams != null);

                    try
                    {
                        Trace.WriteLine(Trace.kKinsky, "Insert into playlist started...");
                        UserLog.WriteLine("Insert into playlist started...");
    
                        switch (iPlayParams.PlayType)
                        {
                            case PlayParams.EPlayType.ePlayNow:
                                if (EventPlayNow != null)
                                {
                                    EventPlayNow(this, new EventArgsPlay(iPlayParams.Retriever));
                                }
                                break;
                            case PlayParams.EPlayType.ePlayNext:
                                if (EventPlayNext != null)
                                {
                                    EventPlayNext(this, new EventArgsPlay(iPlayParams.Retriever));
                                }
                                break;
                            case PlayParams.EPlayType.ePlayLater:
                                if (EventPlayLater != null)
                                {
                                    EventPlayLater(this, new EventArgsPlay(iPlayParams.Retriever));
                                }
                                break;
                            case PlayParams.EPlayType.ePlayInsert:
                                if (EventPlayInsert != null)
                                {
                                    EventPlayInsert(this, new EventArgsInsert(iPlayParams.InsertAfterId, iPlayParams.Retriever));
                                }
                                break;
                            default:
                                Assert.Check(false);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(Trace.kKinsky, "Insert into playlist failed (" + e + ")");
                        UserLog.WriteLine("Insert into playlist failed (" + e + ")");
                    }

                    lock (iLock)
                    {
                        iEventInsert.Reset();
                        iPlayParams = null;
                    }

                    if (EventIsInsertingChanged != null)
                    {
                        EventIsInsertingChanged(this, EventArgs.Empty);
                    }
                }
            }
            catch(ThreadAbortException) { }
        }

        public event EventHandler<EventArgs> EventIsOpenChanged;
        public event EventHandler<EventArgs> EventIsInsertingChanged;
        public event EventHandler<EventArgs> EventIsDraggingChanged;
        public event EventHandler<EventArgs> EventIsInsertAllowedChanged;

        public event EventHandler<EventArgsPlay> EventPlayNow;
        public event EventHandler<EventArgsPlay> EventPlayNext;
        public event EventHandler<EventArgsPlay> EventPlayLater;
        public event EventHandler<EventArgsInsert> EventPlayInsert;

        // These events are used when the main application requires a plugin to process a play now/next/later action
        // KinskyDesktop uses the drag 'n' drop interface for communication, KinskyPda uses these events for communication
        public event EventHandler<EventArgs> EventPlayNowRequest;
        public event EventHandler<EventArgs> EventPlayNextRequest;
        public event EventHandler<EventArgs> EventPlayLaterRequest;

        private bool iOpen;
        private bool iDragging;
        private bool iInsertAllowed;

        private bool iAborting;
        private Thread iThreadInsert;
        private ManualResetEvent iEventInsert;
        private object iLock;
        private PlayParams iPlayParams;
    }

} // Linn.Kinsky