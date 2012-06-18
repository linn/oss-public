using System;
using Linn.Gui.Scenegraph;
using Linn.Gui;
using Linn.Gui.Resources;
using System.ComponentModel;
using System.Collections.Generic;
using Linn.Topology;

namespace Linn.Kinsky
{

    public class ListableUpnpObject : IListable
    {
        public ListableUpnpObject(UpnpObject aObject)
        {
            iObject = aObject;
        }

        public void Dispose() { }
        public void Highlight() { }
        public void UnHighlight() { }
        public void Select() { }
        public void UnSelect() { }

        public UpnpObject Object
        {
            get
            {
                return iObject;
            }
        }

        private UpnpObject iObject = null;
    }


    public class MessageInsertObject
    {
        public MessageInsertObject(uint aStartIndex, bool aPlayAfterInsert)
        {
            iStartIndex = aStartIndex;
            iPlayAfterInsert = aPlayAfterInsert;
        }

        public uint StartIndex
        {
            get
            {
                return iStartIndex;
            }
        }

        public bool PlayAfterInsert
        {
            get
            {
                return iPlayAfterInsert;
            }
        }

        private uint iStartIndex = 0;
        private bool iPlayAfterInsert = false;
    }

    public class ControllerSourceMediaRendererLinn : ControllerSourceMediaRenderer
    {
        public ControllerSourceMediaRendererLinn(Node aRoot, ModelSourceMediaRendererDs aModel, Library aLibrary)
            : base(aRoot, aModel)
        {
            iView = new ViewSourceMediaRendererLinn(aRoot, this, aModel);
            iControllerLibrary = new ControllerLibrary(aRoot, aLibrary.House);
            iControllerPlaylist = new ControllerPlaylistMediaRenderer(aRoot, aModel);
            Messenger.Instance.PresentationMessage(new MsgSetFocus("CurrentPlaylist.Playlist", true));
        }

        public override void Dispose()
        {
            base.Dispose();
            Trace.WriteLine(Trace.kLinnGuiMediaRenderer, ">ControllerSourceMediaRendererLinn.Dispose");
            if (iView != null)
            {
                iView.Dispose();
                //iView = null;
            }
            if (iControllerLibrary != null)
            {
                iControllerLibrary.Dispose();
                //iControllerLibrary = null;
            }
            if (iControllerPlaylist != null)
            {
                iControllerPlaylist.Dispose();
                //iControllerPlaylist = null;
            }
        }

        public override void Receive(Message aMessage)
        {
            base.Receive(aMessage);
            /*
            if (aMessage.Fullname == "ControlPanel.Play.Monostable")
            {
                MsgStateChanged stateMsg = aMessage as MsgStateChanged;
                if (stateMsg != null)
                {
                    if (stateMsg.NewState == true)
                    {
                        //iModel.SelectSource();
                        if (iHasFocus || iControllerLibrary.ControllerMediaServer == null)
                        {
                            if (iControllerPlaylist.TrackHighlighted != iModelPlaylist.CurrentTrackIndex)
                            {
                                if (iControllerPlaylist.TrackHighlighted != -1 && iModel.TransportState != "Paused")
                                {
                                    iModel.SeekTracks(iControllerPlaylist.TrackHighlighted, "Absolute");
                                }
                                else
                                {
                                    iModel.Play();
                                }
                            }
                            else
                            {
                                iModel.Play();
                            }
                        }
                        else
                        {
                            if (iInsertAtEnd)
                            {
                                Insert(iModelPlaylist.TrackCount, true);
                            }
                            else
                            {
                                Insert((uint)(iModelPlaylist.CurrentTrackIndex + 1), true);
                            }
                        }
                    }
                }
            }
            if (aMessage.Fullname == "Library.Insert.Monostable")
            {
                MsgStateChanged stateMsg = aMessage as MsgStateChanged;
                if (stateMsg != null)
                {
                    if (stateMsg.NewState == false)
                    {
                        if (iInsertAtEnd)
                        {
                            Insert(iModelPlaylist.TrackCount, false);
                        }
                        else
                        {
                            Insert((uint)(iControllerPlaylist.CurrentTrackIndex + 1), false);
                        }
                    }
                }
            }
            */
        }


        protected override void Select(int aIndex, ListableUpnpObject aObject)
        {
            Assert.Check(iControllerLibrary.ControllerMediaServer != null);
            if (iControllerLibrary.ControllerMediaServer.InsertAllowed())
            {
                if (aObject.Object as Item != null)
                {
                    if (iInsertAtEnd)
                    {
                        //Insert(iModelPlaylist.TrackCount, true);
                    }
                    else
                    {
                        //Insert((uint)(iModelPlaylist.CurrentTrackIndex + 1), true);
                    }
                }
            }
        }

        protected override void DoInsertWork(object sender, DoWorkEventArgs args)
        {
            /*
            iView.InsertStarted();
            iControllerLibrary.ControllerMediaServer.SetInserting(true);
            MessageInsertObject msg = args.Argument as MessageInsertObject;
            Assert.Check(msg != null);
            List<Item> items = iControllerLibrary.ControllerMediaServer.RetrieveItems(iModelPlaylist.MaxTracksAllowed, iModel.ProtocolInfo);
            Trace.WriteLine(Trace.kLinnGui, "ControllerSourceMediaRendererLinn.DoInsertWork: items.Count=" + items.Count);
            if (items.Count > 0)
            {
                try
                {
                    iModelPlaylist.Insert(msg.StartIndex, items);
                    if (msg.PlayAfterInsert)
                    {
                        iModel.SeekTracks((int)msg.StartIndex, "Absolute");
                    }
                }
                catch (ControlRequest.InvokeError e)
                {
                    System.Console.WriteLine("ControllerSourceMediaRendererLinn.DoInsertWork:\n" + e);
                }
                catch (ControlRequest.InvokeFailed e)
                {
                    System.Console.WriteLine("ControllerSourceMediaRendererLinn.DoInsertWork:\n" + e);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine("ControllerSourceMediaRendererLinn.DoInsertWork:\n" + e);
                    throw e;
                }
            }
            */
        }

        protected override void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Trace.WriteLine(Trace.kLinnGui, ">ControllerSourceMediaRendererLinn.RunWorkerCompleted");
            if (e.Error != null)
            {
                System.Console.WriteLine("ControllerSourceMediaRendererLinn.RunWorkerCompleted:\n" + e.Error.ToString());
                throw new System.Exception(e.Error.ToString());
            }
            // NOTE: issue here, if we move to a new media server whilst inserting the insert check will not be correct as its stored
            //          per server at the moment....
            ControllerMediaServer server = iControllerLibrary.ControllerMediaServer;
            if (server != null)
            {
                server.SetInserting(false);
            }
            iInsertWorker = null;
            iView.InsertFinished();
        }

        private ControllerLibrary iControllerLibrary;
        private ControllerPlaylistMediaRenderer iControllerPlaylist;
    }

} // Linn.Kinsky
