using System;
using System.Collections.Generic;
using Moq;
using Linn.TestFramework;
using System.Threading;

namespace Linn.Kinsky.Test
{
    class Program
    {
        public static void Main()
        {
            Runner runner = new Runner("House Tests");
            runner.Add(new TestHouse());
            runner.Run();
            Console.ReadLine();
        }
    }

    internal class TestHouse : Suite
    {
        public TestHouse() : base("Test Kinsky House") { }

        #region "Setup Code"
        public override void Setup()
        {
            iRunning = true;
            iQueue = new Queue<Action>();
            iMainThread = Thread.CurrentThread;
            iInvokerThread = new Thread(new ThreadStart(() =>
            {
                while (iRunning)
                {
                    lock (iQueue)
                    {
                        while (iQueue.Count > 0)
                        {
                            iQueue.Dequeue()();
                        }
                    }
                    Thread.Sleep(10);
                }
            }));
            iInvokerThread.Start();
            // mock repository
            iRepository = new MockRepository(MockBehavior.Loose);
            iRepository.DefaultValue = DefaultValue.Mock;
            // house
            SetupHouse();
            // models
            SetupModels();
        }

        public override void TearDown()
        {
            iRunning = false;
            iInvokerThread.Abort();
        }

        private void SetupHouse()
        {
            // mock topology house
            iMockTopologyHouse = iRepository.Create<Topology.IHouse>();
            iModelFactory = iRepository.Create<Topology.IModelFactory>();
            iMockTopologyHouse.Setup(h => h.ModelFactory).Returns(iModelFactory.Object);
            iMockModelSenders = iRepository.Create<Topology.IModelSenders>();
            iInvoker = new TestInvoker(iInvokerThread, iMainThread, iQueue);

            // kinsky house
            iHouse = new House(iMockTopologyHouse.Object, iInvoker, iMockModelSenders.Object);
            iHouse.Start(null);
            // mock main room
            iMockMainRoom = iRepository.Create<Topology.IRoom>();
            iMockMainRoom.Setup(m => m.Name).Returns("Main Room");
            iMockMainRoom.Setup(m => m.Standby).Returns(() => { return iMainRoomStandby; });
            iMockMainRoom.Setup(r => r.Current).Returns(() => { return iMainRoomCurrentSource; });
            iMockMainRoom.Setup(m => m.SetStandby(It.IsAny<bool>())).Callback<bool>((aStandby) =>
            {
                iMainRoomStandby = aStandby;
                iMockMainRoom.Raise(r => r.EventStandbyChanged += null, EventArgs.Empty);
            });
            iMockMainRoomGroup1 = iRepository.Create<Topology.IGroup>();
            iMockMainRoomGroup1.Setup(g => g.Name).Returns("Group1");
            iMockMainRoomGroup1.Setup(g => g.HasInfo).Returns(true);
            iMockMainRoomGroup1.Setup(g => g.HasTime).Returns(true);
            iMockMainRoomGroup2 = iRepository.Create<Topology.IGroup>();
            iMockMainRoomGroup2.Setup(g => g.Name).Returns("Group2");
            iMockMainRoomGroup2.Setup(g => g.HasInfo).Returns(true);
            iMockMainRoomGroup2.Setup(g => g.HasTime).Returns(true);
            iMockMainRoomPreamp1 = iRepository.Create<Topology.IPreamp>();
            iMockMainRoomPreamp2 = iRepository.Create<Topology.IPreamp>();
            iMainRoomPreamp = iMockMainRoomPreamp1.Object;
            iMockMainRoom.Setup(m => m.Preamp).Returns(() => { return iMainRoomPreamp; });

            // mock main room, mock playlist source
            iMockMainRoomPlaylistSource1 = iRepository.Create<Topology.ISource>();
            SetupSource(iMockMainRoom, iMockMainRoomPlaylistSource1, "Playlist", "Playlist1Udn", Source.kSourceDs, iMockMainRoomGroup1.Object);

            // mock main room, mock radio source
            iMockMainRoomRadioSource1 = iRepository.Create<Topology.ISource>();
            SetupSource(iMockMainRoom, iMockMainRoomRadioSource1, "Radio", "Radio1Udn", Source.kSourceRadio, iMockMainRoomGroup1.Object);
            iMainRoomCurrentSource = iMockMainRoomRadioSource1.Object;


            // mock main room, mock playlist source 2
            iMockMainRoomPlaylistSource2 = iRepository.Create<Topology.ISource>();
            SetupSource(iMockMainRoom, iMockMainRoomPlaylistSource2, "Playlist", "Playlist2Udn", Source.kSourceDs, iMockMainRoomGroup2.Object);

            // mock main room, mock radio source 2
            iMockMainRoomRadioSource2 = iRepository.Create<Topology.ISource>();
            SetupSource(iMockMainRoom, iMockMainRoomRadioSource2, "Radio", "Radio2Udn", Source.kSourceRadio, iMockMainRoomGroup2.Object);

            // mock main room, mock aux source 1
            iMockMainRoomAuxSource1 = iRepository.Create<Topology.ISource>();
            SetupSource(iMockMainRoom, iMockMainRoomAuxSource1, "Analog1", "Analog1Udn", Source.kSourceAnalog, iMockMainRoomGroup1.Object);

            // mock main room, mock aux source 2
            iMockMainRoomAuxSource2 = iRepository.Create<Topology.ISource>();
            SetupSource(iMockMainRoom, iMockMainRoomAuxSource2, "Analog2", "Analog2Udn", Source.kSourceAnalog, iMockMainRoomGroup1.Object);

            // mock main room, mock disc source 2
            iMockMainRoomDiscSource1 = iRepository.Create<Topology.ISource>();
            SetupSource(iMockMainRoom, iMockMainRoomDiscSource1, "Disc", "DiscUdn", Source.kSourceDisc, iMockMainRoomGroup1.Object);

            // mock main room, mock upnpav source
            iMockMainRoomUpnpSource1 = iRepository.Create<Topology.ISource>();
            SetupSource(iMockMainRoom, iMockMainRoomUpnpSource1, "UpnpAv", "UpnpAv1Udn", Source.kSourceUpnpAv, iMockMainRoomGroup1.Object);

            // mock main room, mock receiver source
            iMockMainRoomReceiverSource = iRepository.Create<Topology.ISource>();
            SetupSource(iMockMainRoom, iMockMainRoomReceiverSource, "Receiver", "ReceiverUdn", Source.kSourceReceiver, iMockMainRoomGroup1.Object);

            // mock source list
            // add a radio source first
            iMainRoomSources.Add(iMockMainRoomRadioSource1.Object);
            iMockMainRoom.Setup(m => m.Sources).Returns(new System.Collections.ObjectModel.ReadOnlyCollection<Topology.ISource>(iMainRoomSources));

            // secondary room
            iMockSecondaryRoom = iRepository.Create<Topology.IRoom>();
            iMockSecondaryRoom.Setup(m => m.Name).Returns("Secondary Room");
            iMockSecondaryRoom.Setup(m => m.Sources).Returns(new System.Collections.ObjectModel.ReadOnlyCollection<Topology.ISource>(iSecondaryRoomSources));
        }

        private void SetupModels()
        {
            // setup a fake media retriever
            Upnp.DidlLite didl = new Upnp.DidlLite();
            didl.Add(new Upnp.item());
            didl.Add(new Upnp.item());
            iMockMediaRetriever.Setup(r => r.Media).Returns(didl);

            // volume control preamp 1
            iMockModelVolumeControl1 = new Mock<Topology.IModelVolumeControl>();
            iPreampState1 = new PreampState();
            SetupModelVolumeControl(iMockModelVolumeControl1, iPreampState1);
            iModelFactory.Setup(f => f.CreateModelVolumeControl(It.Is<Topology.IPreamp>(p => p == iMockMainRoomPreamp1.Object))).Returns(iMockModelVolumeControl1.Object);

            // volume control preamp 2
            iMockModelVolumeControl2 = new Mock<Topology.IModelVolumeControl>();
            iPreampState2 = new PreampState();
            SetupModelVolumeControl(iMockModelVolumeControl2, iPreampState2);
            iModelFactory.Setup(f => f.CreateModelVolumeControl(It.Is<Topology.IPreamp>(p => p == iMockMainRoomPreamp2.Object))).Returns(iMockModelVolumeControl2.Object);

            // time model
            iMockModelTime = new Mock<Topology.IModelTime>();
            SetupModelTime(iMockModelTime);
            iModelFactory.Setup(f => f.CreateModelTime(It.IsAny<Topology.ISource>())).Returns(iMockModelTime.Object);

            //info model
            iMockModelInfo = new Mock<Topology.IModelInfo>();
            SetupModelInfo(iMockModelInfo);
            iModelFactory.Setup(f => f.CreateModelInfo(It.IsAny<Topology.ISource>())).Returns(iMockModelInfo.Object);

            // source models
            iMockModelSourceMediaRenderer = new Mock<Topology.IModelSourceMediaRenderer>();
            for (int i = 0; i < 10; i++)
            {
                iPlaylist.Add(new Topology.MrItem((uint)i, "", new Upnp.DidlLite()));
            }
            SetupModelSourceMediaRenderer(iMockModelSourceMediaRenderer);
            iModelFactory.Setup(f => f.CreateModelSourceMediaRenderer(It.IsAny<Topology.ISource>()))
                .Callback<Topology.ISource>((s) => { Assert.Check(s.Type == Source.kSourceDs || s.Type == Source.kSourceUpnpAv); })
                .Returns(iMockModelSourceMediaRenderer.Object);

            iMockModelSourceDiscPlayer = new Mock<Topology.IModelSourceDiscPlayer>();
            SetupModelSourceDiscPlayer(iMockModelSourceDiscPlayer);
            iModelFactory.Setup(f => f.CreateModelSourceDiscPlayer(It.IsAny<Topology.ISource>()))
                .Callback<Topology.ISource>((s) => { Assert.Check(s.Type == Source.kSourceDisc); })
                .Returns(iMockModelSourceDiscPlayer.Object);

            iMockModelSourceRadio = new Mock<Topology.IModelSourceRadio>();
            SetupModelSourceRadio(iMockModelSourceRadio);
            iModelFactory.Setup(f => f.CreateModelSourceRadio(It.IsAny<Topology.ISource>()))
                .Callback<Topology.ISource>((s) => { Assert.Check(s.Type == Source.kSourceRadio); })
                .Returns(iMockModelSourceRadio.Object);

            iMockModelSourceReceiver = new Mock<Topology.IModelSourceReceiver>();
            SetupModelSourceReceiver(iMockModelSourceReceiver);
            iModelFactory.Setup(f => f.CreateModelSourceReceiver(It.IsAny<Topology.ISource>()))
                .Callback<Topology.ISource>((s) => { Assert.Check(s.Type == Source.kSourceReceiver); })
                .Returns(iMockModelSourceReceiver.Object);

            
        }

        private void SetupModelSourceRadio(Mock<Topology.IModelSourceRadio> aModel)
        {
            Linn.Topology.MrItem test = new Linn.Topology.MrItem(0, "", new Upnp.DidlLite());
            aModel.Setup(m => m.Preset(It.IsAny<uint>())).Returns(test);
            aModel.Setup(m => m.SetChannel(It.IsAny<Upnp.DidlLite>())).Callback<Upnp.DidlLite>((d) =>
            {
                iChannel = d;
                aModel.Raise(m => m.EventChannelChanged += null, EventArgs.Empty);
            });
            aModel.Setup(m => m.Channel).Returns(() =>
            {
                return new Topology.Channel("", iChannel);
            });
            aModel.Setup(f => f.Open()).Callback(() =>
            {
                aModel.Raise(m => m.EventTransportStateChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventPresetsChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventPresetChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventChannelChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventControlInitialised += null, EventArgs.Empty);
            });
        }

        private void SetupModelSourceReceiver(Mock<Topology.IModelSourceReceiver> aModel)
        {
            aModel.Setup(m => m.SetChannel(It.IsAny<Upnp.DidlLite>())).Callback<Upnp.DidlLite>((d) =>
            {
                iChannel = d;
                aModel.Raise(m => m.EventChannelChanged += null, EventArgs.Empty);
            });
            aModel.Setup(m => m.Channel).Returns(() =>
            {
                return new Topology.Channel("", iChannel);
            });
            aModel.Setup(f => f.Open()).Callback(() =>
            {
                aModel.Raise(m => m.EventTransportStateChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventChannelChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventControlInitialised += null, EventArgs.Empty);
            });
            aModel.Setup(m => m.TransportState).Returns(Topology.ModelSourceReceiver.ETransportState.eUnknown);
        }

        private void SetupSource(Mock<Topology.IRoom> aMockRoom, Mock<Topology.ISource> aMockSource, string aName, string aUdn, string aType, Topology.IGroup aGroup)
        {
            aMockSource.Setup(s => s.Select()).Callback(() =>
            {
                iMainRoomCurrentSource = aMockSource.Object;
                aMockRoom.Raise(r => r.EventCurrentChanged += null, EventArgs.Empty);
            });
            aMockSource.Setup(s => s.Name).Returns(aName);
            aMockSource.Setup(s => s.Udn).Returns(aUdn);
            aMockSource.Setup(s => s.Type).Returns(aType);
            aMockSource.Setup(s => s.Group).Returns(aGroup);
        }

        private void SetupModelSourceDiscPlayer(Mock<Topology.IModelSourceDiscPlayer> aModel)
        {
            aModel.Setup(m => m.ProgramMode).Returns(() => { return iShuffle ? Topology.ModelSourceDiscPlayer.EProgramMode.eShuffle : Topology.ModelSourceDiscPlayer.EProgramMode.eOff; });
            aModel.Setup(m => m.RepeatMode).Returns(() => { return iRepeat ? Topology.ModelSourceDiscPlayer.ERepeatMode.eAll : Topology.ModelSourceDiscPlayer.ERepeatMode.eOff; });
            aModel.Setup(m => m.ToggleRepeat()).Callback(() =>
            {
                iRepeat = !iRepeat;
                aModel.Raise(m => m.EventRepeatModeChanged += null, EventArgs.Empty);
            });
            aModel.Setup(m => m.ToggleShuffle()).Callback(() =>
            {
                iShuffle = !iShuffle;
                aModel.Raise(m => m.EventProgramModeChanged += null, EventArgs.Empty);
            });
            aModel.Setup(f => f.Open()).Callback(() =>
            {
                aModel.Raise(m => m.EventRepeatModeChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventProgramModeChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventInitialised += null, EventArgs.Empty);
            });
            aModel.Setup(m => m.PlayState).Returns(Topology.ModelSourceDiscPlayer.EPlayState.eUnknown);
        }

        private void SetupModelVolumeControl(Mock<Topology.IModelVolumeControl> aModel, PreampState aPreampState)
        {
            aModel.Setup(m => m.Volume).Returns(() => { return aPreampState.Volume; });
            aModel.Setup(m => m.VolumeLimit).Returns(() => { return aPreampState.VolumeLimit; });
            aModel.Setup(m => m.Mute).Returns(() => { return aPreampState.Mute; });
            aModel.Setup(f => f.Open()).Callback(() =>
            {
                aModel.Raise(m => m.EventMuteStateChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventVolumeChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventVolumeLimitChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventInitialised += null, EventArgs.Empty);
            });
            aModel.Setup(f => f.SetVolume(It.IsAny<uint>())).Callback<uint>((aVolume) =>
            {
                aPreampState.Volume = aVolume;
                aModel.Raise(m => m.EventVolumeChanged += null, EventArgs.Empty);
            });
            aModel.Setup(f => f.SetMute(It.IsAny<bool>())).Callback<bool>((aMute) =>
            {
                aPreampState.Mute = aMute;
                aModel.Raise(m => m.EventMuteStateChanged += null, EventArgs.Empty);
            });
        }

        private void SetupModelTime(Mock<Topology.IModelTime> aModel)
        {
            aModel.Setup(m => m.Duration).Returns(() => { return iDuration; });
            aModel.Setup(m => m.Seconds).Returns(() => { return iSeconds; });
            aModel.Setup(f => f.Open()).Callback(() =>
            {
                aModel.Raise(m => m.EventDurationChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventSecondsChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventInitialised += null, EventArgs.Empty);
            });
        }

        private void SetupModelInfo(Mock<Topology.IModelInfo> aModel)
        {
            aModel.Setup(m => m.Track).Returns(() => { return iTrack; });
            aModel.Setup(m => m.Metatext).Returns(() => { return iMetatext; });
            aModel.Setup(f => f.Open()).Callback(() =>
            {
                aModel.Raise(m => m.EventTrackChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventMetaTextChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventDetailsChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventInitialised += null, EventArgs.Empty);
            });
        }

        private void SetupModelSourceMediaRenderer(Mock<Topology.IModelSourceMediaRenderer> aModel)
        {
            aModel.Setup(m => m.Repeat).Returns(() => { return iRepeat; });
            aModel.Setup(m => m.Shuffle).Returns(() => { return iShuffle; });
            aModel.Setup(m => m.TrackPlaylistItem).Returns(() => { return new Linn.Topology.MrItem(0, "", new Upnp.DidlLite()); });
            aModel.Setup(m => m.ToggleRepeat()).Callback(() =>
            {
                iRepeat = !iRepeat;
                aModel.Raise(m => m.EventRepeatChanged += null, EventArgs.Empty);
            });
            aModel.Setup(m => m.ToggleShuffle()).Callback(() =>
            {
                iShuffle = !iShuffle;
                aModel.Raise(m => m.EventShuffleChanged += null, EventArgs.Empty);
            });
            aModel.Setup(m => m.PlaylistItem(It.IsAny<uint>())).Returns((uint aPosition) => { return iPlaylist[(int)aPosition]; });
            aModel.Setup(m => m.PlaylistInsert(It.IsAny<uint>(), It.IsAny<Upnp.DidlLite>()))
                .Callback<uint, Upnp.DidlLite>((aInsertAfterId, aDidlLite) =>
                {
                    int id = iPlaylist.Count;
                    int position = 0;
                    bool found = false;
                    foreach (Topology.MrItem item in iPlaylist)
                    {
                        if (item.Id == aInsertAfterId)
                        {
                            found = true;
                            position = iPlaylist.IndexOf(item) + 1;
                            break;
                        }
                    }
                    Assert.Check(found || aInsertAfterId == 0);

                    foreach (Upnp.upnpObject obj in aDidlLite)
                    {
                        iPlaylist.Insert(position++, new Topology.MrItem((uint)id++, "", new Upnp.DidlLite()));
                    }
                    aModel.Raise(m => m.EventPlaylistChanged += null, EventArgs.Empty);
                });
            aModel.Setup(m => m.PlaylistMove(It.IsAny<uint>(), It.IsAny<IList<Topology.MrItem>>()))
                .Callback<uint, IList<Topology.MrItem>>((aInsertAfterId, aPlaylistItems) =>
                {
                    int position = 0;
                    bool found = false;

                    foreach (Topology.MrItem item in aPlaylistItems)
                    {
                        Assert.Check(iPlaylist.IndexOf(item) != -1);
                        iPlaylist.Remove(item);
                    }

                    foreach (Topology.MrItem item in iPlaylist)
                    {
                        if (item.Id == aInsertAfterId)
                        {
                            found = true;
                            position = iPlaylist.IndexOf(item) + 1;
                            break;
                        }
                    }
                    Assert.Check(found || aInsertAfterId == 0);

                    foreach (Topology.MrItem item in aPlaylistItems)
                    {
                        iPlaylist.Insert(position++, item);
                    }
                    aModel.Raise(m => m.EventPlaylistChanged += null, EventArgs.Empty);
                });
            aModel.Setup(m => m.PlaylistDelete(It.IsAny<IList<Topology.MrItem>>()))
                .Callback<IList<Topology.MrItem>>((aItems) =>
                {
                    List<Topology.MrItem> items = new List<Topology.MrItem>(aItems);
                    foreach (Topology.MrItem obj in aItems)
                    {
                        iPlaylist.Remove(obj);
                    }
                    aModel.Raise(m => m.EventPlaylistChanged += null, EventArgs.Empty);
                });
            aModel.Setup(m => m.PlaylistDeleteAll())
                .Callback(() =>
                {
                    iPlaylist.Clear();
                    aModel.Raise(m => m.EventPlaylistChanged += null, EventArgs.Empty);
                });
            aModel.Setup(m => m.PlaylistTrackCount).Returns(() => { return (uint)iPlaylist.Count; });
            aModel.Setup(f => f.Open()).Callback(() =>
            {
                aModel.Raise(m => m.EventTrackChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventPlaylistChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventRepeatChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventShuffleChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventTransportStateChanged += null, EventArgs.Empty);
                aModel.Raise(m => m.EventControlInitialised += null, EventArgs.Empty);
                aModel.Raise(m => m.EventPlaylistInitialised += null, EventArgs.Empty);
            });
            aModel.Setup(m => m.PlayNow(It.IsAny<Upnp.DidlLite>())).Callback<Upnp.DidlLite>((d) =>
            {
                uint insertId = 0;
                if (iPlaylist.Count > 0)
                {
                    insertId = aModel.Object.TrackIndex != -1 ? iPlaylist[aModel.Object.TrackIndex].Id : iPlaylist[iPlaylist.Count - 1].Id;
                }
                aModel.Object.PlaylistInsert(insertId, d);
            });
            aModel.Setup(m => m.PlayNext(It.IsAny<Upnp.DidlLite>())).Callback<Upnp.DidlLite>((d) =>
            {
                uint insertId = 0;
                if (iPlaylist.Count > 0)
                {
                    insertId = aModel.Object.TrackIndex != -1 ? iPlaylist[aModel.Object.TrackIndex].Id : iPlaylist[iPlaylist.Count - 1].Id;
                }
                aModel.Object.PlaylistInsert(insertId, d);
            });
            aModel.Setup(m => m.PlayLater(It.IsAny<Upnp.DidlLite>())).Callback<Upnp.DidlLite>((d) =>
            {
                uint insertId = 0;
                if (iPlaylist.Count > 0)
                {
                    insertId = iPlaylist[iPlaylist.Count - 1].Id;
                }
                aModel.Object.PlaylistInsert(insertId, d);
            });
            aModel.Setup(m => m.TransportState).Returns(Topology.ModelSourceMediaRenderer.ETransportState.eUnknown);
        }

        #endregion

        #region "Test Code"
        public override void Test()
        {
            // ensure house is empty
            TEST(House_Should_Contain_Room_Count(iHouse, 0));

            // insert a room and check room inserted event is raised
            TEST(Room_Add_Should_Raise_EventRoomInserted(iHouse, iMockTopologyHouse, iMockSecondaryRoom));

            // ensure room added
            TEST(House_Should_Contain_Room_Count(iHouse, 1));

            // insert a room and check room inserted event is raised
            TEST(Room_Add_Should_Raise_EventRoomInserted(iHouse, iMockTopologyHouse, iMockMainRoom));

            // ensure room added
            TEST(House_Should_Contain_Room_Count(iHouse, 2));

            IRoom mainRoom = iHouse.Rooms[0];
            IRoom secondaryRoom = iHouse.Rooms[1];

            mainRoom.Open();
            IRoomTime time = mainRoom.Time;
            IRoomVolume volume = mainRoom.Volume;
            IRoomInfo info = mainRoom.Info;
            secondaryRoom.Open();
            time = secondaryRoom.Time;
            volume = secondaryRoom.Volume;
            info = secondaryRoom.Info;

            // ensure order of rooms is alphabetic
            TEST(Room_Name_Should_Equal(mainRoom, iMockMainRoom.Object.Name));
            TEST(Room_Name_Should_Equal(secondaryRoom, iMockSecondaryRoom.Object.Name));

            // ensure room contains default source
            TEST(Room_Should_Contain_Source_Count(mainRoom, 1));

            // insert more sources, checking the source inserted event is raised
            TEST(Source_Add_Should_Raise_EventSourceInserted(mainRoom, iMockMainRoom, iMockMainRoomRadioSource2));
            TEST(Source_Add_Should_Raise_EventSourceInserted(mainRoom, iMockMainRoom, iMockMainRoomUpnpSource1));
            TEST(Source_Add_Should_Raise_EventSourceInserted(mainRoom, iMockMainRoom, iMockMainRoomPlaylistSource2));
            TEST(Source_Add_Should_Raise_EventSourceInserted(mainRoom, iMockMainRoom, iMockMainRoomPlaylistSource1));
            TEST(Source_Add_Should_Raise_EventSourceInserted(mainRoom, iMockMainRoom, iMockMainRoomAuxSource1));
            TEST(Source_Add_Should_Raise_EventSourceInserted(mainRoom, iMockMainRoom, iMockMainRoomAuxSource2));
            TEST(Source_Add_Should_Raise_EventSourceInserted(mainRoom, iMockMainRoom, iMockMainRoomDiscSource1));
            TEST(Source_Add_Should_Raise_EventSourceInserted(mainRoom, iMockMainRoom, iMockMainRoomReceiverSource));

            // ensure room now has 9 sources
            TEST(Room_Should_Contain_Source_Count(mainRoom, 9));


            ISource playlistSource1 = mainRoom.Sources[0];
            ISource playlistSource2 = mainRoom.Sources[1];
            ISource radioSource1 = mainRoom.Sources[2];
            ISource radioSource2 = mainRoom.Sources[3];
            ISource receiverSource = mainRoom.Sources[4];
            ISource analogSource1 = mainRoom.Sources[5];
            ISource analogSource2 = mainRoom.Sources[6];
            ISource discSource = mainRoom.Sources[7];
            ISource upnpAvSource = mainRoom.Sources[8];

            // ensure the sources are in the correct order with the correct names
            TEST(Source_Name_Is_Equal_To(playlistSource1, "Playlist (Group1)"));
            TEST(Source_Name_Is_Equal_To(playlistSource2, "Playlist (Group2)"));
            TEST(Source_Name_Is_Equal_To(radioSource1, "Radio (Group1)"));
            TEST(Source_Name_Is_Equal_To(radioSource2, "Radio (Group2)"));
            TEST(Source_Name_Is_Equal_To(receiverSource, "Receiver"));
            TEST(Source_Name_Is_Equal_To(analogSource1, "Analog1"));
            TEST(Source_Name_Is_Equal_To(analogSource2, "Analog2"));
            TEST(Source_Name_Is_Equal_To(discSource, "Disc"));
            TEST(Source_Name_Is_Equal_To(upnpAvSource, "UpnpAv"));

            // ensure the sources are of the correct type
            TEST(Source_Is_Of_Type(playlistSource1, typeof(IPlaylistSource)));
            TEST(Source_Is_Of_Type(playlistSource2, typeof(IPlaylistSource)));
            TEST(Source_Is_Of_Type(radioSource1, typeof(IRadioSource)));
            TEST(Source_Is_Of_Type(radioSource2, typeof(IRadioSource)));
            TEST(Source_Is_Of_Type(receiverSource, typeof(IReceiverSource)));            
            TEST(Source_Is_Of_Type(discSource, typeof(IDiscSource)));
            TEST(Source_Is_Of_Type(upnpAvSource, typeof(IPlaylistSource)));

            // test source switching
            TEST(Source_Switch_Should_Raise_EventCurrentChanged(mainRoom, playlistSource1));
            TEST(!Source_Switch_Should_Raise_EventCurrentChanged(mainRoom, playlistSource1));
            TEST(Source_Switch_Should_Raise_EventCurrentChanged(mainRoom, playlistSource2));

            // test preamp switching/volume service
            TEST(Preamp_Switch_Should_Raise_EventVolumeChanged(iMockMainRoom, mainRoom));
            TEST(Volume_Setter_Should_Raise_EventVolumeChanged(iMockMainRoom, mainRoom));
            TEST(Preamp_Switch_Should_Change_Volume(iMockMainRoom, mainRoom, iMockMainRoomPreamp2, iMockModelVolumeControl2));

            // test time service
            TEST(Seconds_Change_Should_Raise_EventSecondsChanged(mainRoom, iMockModelTime));

            // test info service
            TEST(Track_Change_Should_Raise_EventTrackChanged(mainRoom, iMockModelInfo));
            TEST(Null_Track_Should_Not_Cause_NullReferenceException(mainRoom, iMockModelInfo));
            TEST(Null_Metatext_Should_Not_Cause_NullReferenceException(mainRoom, iMockModelInfo));

            // test playlist 
            TEST(EventPlaylistChanged_Should_Raise_EventItemsChanged(playlistSource1, iMockModelSourceMediaRenderer));
            List<Topology.MrItem> originalPlaylist = new List<Topology.MrItem>(iPlaylist);
            TEST(Playlist_Insert_Should_Raise_EventPlaylistChanged(playlistSource1, 1, iMockMediaRetriever.Object));
            TEST(!Playlists_Are_Equal(iPlaylist, originalPlaylist));
            List<Topology.MrItem> itemsToDelete = new List<Topology.MrItem>();
            itemsToDelete.Add(iPlaylist[2]);
            itemsToDelete.Add(iPlaylist[3]);
            TEST(Playlist_Delete_Should_Raise_EventPlaylistChanged(playlistSource1, itemsToDelete));
            TEST(Playlists_Are_Equal(iPlaylist, originalPlaylist));
            List<Topology.MrItem> itemsToMove = new List<Topology.MrItem>();
            itemsToMove.Add(iPlaylist[6]);
            itemsToMove.Add(iPlaylist[7]);
            TEST(Playlist_Move_Should_Raise_EventPlaylistChanged(playlistSource1, itemsToMove, 1));
            TEST(!Playlists_Are_Equal(iPlaylist, originalPlaylist));
            TEST(Playlist_Move_Should_Raise_EventPlaylistChanged(playlistSource1, itemsToMove, 5));
            TEST(Playlists_Are_Equal(iPlaylist, originalPlaylist));
            TEST(Playlist_DeleteAll_Should_Raise_EventPlaylistChanged(playlistSource1));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { playlistSource1.PlayNow(iMockMediaRetriever.Object); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { playlistSource1.PlayNext(iMockMediaRetriever.Object); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { playlistSource1.PlayLater(iMockMediaRetriever.Object); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { playlistSource1.Play(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { playlistSource1.Pause(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { playlistSource1.Stop(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { playlistSource1.Previous(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { playlistSource1.Next(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { playlistSource1.Seek(100); }));
            TEST(iPlaylist.Count == 6);
            TEST(IPlayModeProvider_ToggleRepeat_Should_Raise_EventRepeatChanged(playlistSource1));
            TEST(IPlayModeProvider_ToggleShuffle_Should_Raise_EventShuffleChanged(playlistSource1));
            TEST(playlistSource1.TransportState == ETransportState.eUnknown);

            // test disc 
            TEST(IPlayModeProvider_ToggleRepeat_Should_Raise_EventRepeatChanged(discSource));
            TEST(IPlayModeProvider_ToggleShuffle_Should_Raise_EventShuffleChanged(discSource));
            TEST(Action_Should_Throw_InvalidOperationException(() => { discSource.PlayNow(iMockMediaRetriever.Object); }));
            TEST(Action_Should_Throw_InvalidOperationException(() => { discSource.PlayNext(iMockMediaRetriever.Object); }));
            TEST(Action_Should_Throw_InvalidOperationException(() => { discSource.PlayLater(iMockMediaRetriever.Object); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { discSource.Play(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { discSource.Pause(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { discSource.Stop(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { discSource.Previous(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { discSource.Next(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { discSource.Seek(100); }));
            TEST(discSource.TransportState == ETransportState.eUnknown);

            //test radio 
            TEST(ChannelChange_Should_Raise_EventChannelChanged(radioSource1));
            TEST(Action_Should_Throw_InvalidOperationException(() => { radioSource1.PlayNow(iMockMediaRetriever.Object); }));
            TEST(Action_Should_Throw_InvalidOperationException(() => { radioSource1.PlayNext(iMockMediaRetriever.Object); }));
            TEST(Action_Should_Throw_InvalidOperationException(() => { radioSource1.PlayLater(iMockMediaRetriever.Object); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { radioSource1.Play(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { radioSource1.Pause(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { radioSource1.Stop(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { radioSource1.Previous(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { radioSource1.Next(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { radioSource1.Seek(100); }));
            TEST(radioSource1.TransportState == ETransportState.eUnknown);

            //test receiver 
            TEST(ChannelChange_Should_Raise_EventChannelChanged(receiverSource));
            TEST(Action_Should_Throw_InvalidOperationException(() => { receiverSource.PlayNow(iMockMediaRetriever.Object); }));
            TEST(Action_Should_Throw_InvalidOperationException(() => { receiverSource.PlayNext(iMockMediaRetriever.Object); }));
            TEST(Action_Should_Throw_InvalidOperationException(() => { receiverSource.PlayLater(iMockMediaRetriever.Object); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { receiverSource.Play(); }));
            TEST(Action_Should_Throw_InvalidOperationException(() => { receiverSource.Pause(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { receiverSource.Stop(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { receiverSource.Previous(); }));
            TEST(!Action_Should_Throw_InvalidOperationException(() => { receiverSource.Next(); }));
            TEST(Action_Should_Throw_InvalidOperationException(() => { receiverSource.Seek(100); }));
            TEST(receiverSource.TransportState == ETransportState.eUnknown);

            // test analog 
            TEST(Action_Should_Throw_InvalidOperationException(() => { analogSource1.PlayNow(iMockMediaRetriever.Object); }));
            TEST(Action_Should_Throw_InvalidOperationException(() => { analogSource1.PlayNext(iMockMediaRetriever.Object); }));
            TEST(Action_Should_Throw_InvalidOperationException(() => { analogSource1.PlayLater(iMockMediaRetriever.Object); }));
            TEST(Action_Should_Throw_InvalidOperationException(() => { analogSource1.Play(); }));
            TEST(Action_Should_Throw_InvalidOperationException(() => { analogSource1.Pause(); }));
            TEST(Action_Should_Throw_InvalidOperationException(() => { analogSource1.Stop(); }));
            TEST(Action_Should_Throw_InvalidOperationException(() => { analogSource1.Previous(); }));
            TEST(Action_Should_Throw_InvalidOperationException(() => { analogSource1.Next(); }));
            TEST(Action_Should_Throw_InvalidOperationException(() => { analogSource1.Seek(100); }));
            TEST(analogSource1.TransportState == ETransportState.ePlaying);

            // remove a playlist source and ensure other playlist source name is updated and rename event is raised
            TEST(Source_Removed_Should_Raise_EventNameChanged(iMockMainRoom, iMockMainRoomPlaylistSource2, playlistSource1, "Playlist"));
            TEST(Source_Removed_Should_Raise_EventSourceRemoved(mainRoom, iMockMainRoom, iMockMainRoomPlaylistSource1, playlistSource1));

            // ensure the playlist sources were removed
            TEST(Room_Should_Contain_Source_Count(mainRoom, 7));

            //test standby
            TEST(SetStandby_Should_Raise_EventStandbyChanged(mainRoom));

            // remove a room and ensure room removed event is raised
            TEST(Room_Remove_Should_Raise_EventRoomRemoved(iHouse, iMockTopologyHouse, iMockMainRoom));

            TEST(House_Should_Contain_Room_Count(iHouse, 1));

            // remove a room and ensure room removed event is raised
            TEST(Room_Remove_Should_Raise_EventRoomRemoved(iHouse, iMockTopologyHouse, iMockSecondaryRoom));

            // ensure house is empty
            TEST(House_Should_Contain_Room_Count(iHouse, 0));

            // threading/invoker tests
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            Thread nonInvokerThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    TEST(Action_Should_Throw_InvocationException(() => { var x = iHouse.Rooms; }));
                    TEST(Action_Should_Throw_InvocationException(() => { playlistSource1.PlayNow(iMockMediaRetriever.Object); }));
                    TEST(Room_Add_Should_Raise_EventRoomInserted_On_Invoker_Thread(iHouse, iMockTopologyHouse, iMockMainRoom));
                    ManualResetEvent waitHandle2 = new ManualResetEvent(false);
                    iInvoker.BeginInvoke((Action)(() =>
                    {
                        mainRoom = iHouse.Rooms[0];
                        mainRoom.Open();
                        waitHandle2.Set();
                    }));
                    waitHandle2.WaitOne();

                    TEST(Action_Should_Throw_InvocationException(() => { var x = mainRoom.Current; }));
                    TEST(Action_Should_Throw_InvocationException(() => { playlistSource2.Select(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { var x = mainRoom.Sources.Count; }));
                    TEST(Action_Should_Throw_InvocationException(() => { var x = playlistSource1.Name; }));

                    TEST(Action_Should_Throw_InvocationException(() => { playlistSource1.PlayNow(iMockMediaRetriever.Object); }));
                    TEST(Action_Should_Throw_InvocationException(() => { playlistSource1.PlayNext(iMockMediaRetriever.Object); }));
                    TEST(Action_Should_Throw_InvocationException(() => { playlistSource1.PlayLater(iMockMediaRetriever.Object); }));
                    TEST(Action_Should_Throw_InvocationException(() => { playlistSource1.Play(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { playlistSource1.Pause(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { playlistSource1.Stop(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { playlistSource1.Previous(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { playlistSource1.Next(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { playlistSource1.Seek(100); }));
                    TEST(Action_Should_Throw_InvocationException(() => { var x = playlistSource1.TransportState; }));

                    TEST(Action_Should_Throw_InvocationException(() => { discSource.PlayNow(iMockMediaRetriever.Object); }));
                    TEST(Action_Should_Throw_InvocationException(() => { discSource.PlayNext(iMockMediaRetriever.Object); }));
                    TEST(Action_Should_Throw_InvocationException(() => { discSource.PlayLater(iMockMediaRetriever.Object); }));
                    TEST(Action_Should_Throw_InvocationException(() => { discSource.Play(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { discSource.Pause(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { discSource.Stop(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { discSource.Previous(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { discSource.Next(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { discSource.Seek(100); }));
                    TEST(Action_Should_Throw_InvocationException(() => { var x = discSource.TransportState; }));

                    TEST(Action_Should_Throw_InvocationException(() => { radioSource1.PlayNow(iMockMediaRetriever.Object); }));
                    TEST(Action_Should_Throw_InvocationException(() => { radioSource1.PlayNext(iMockMediaRetriever.Object); }));
                    TEST(Action_Should_Throw_InvocationException(() => { radioSource1.PlayLater(iMockMediaRetriever.Object); }));
                    TEST(Action_Should_Throw_InvocationException(() => { radioSource1.Play(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { radioSource1.Pause(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { radioSource1.Stop(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { radioSource1.Previous(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { radioSource1.Next(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { radioSource1.Seek(100); }));
                    TEST(Action_Should_Throw_InvocationException(() => { var x = radioSource1.TransportState; }));

                    TEST(Action_Should_Throw_InvocationException(() => { receiverSource.PlayNow(iMockMediaRetriever.Object); }));
                    TEST(Action_Should_Throw_InvocationException(() => { receiverSource.PlayNext(iMockMediaRetriever.Object); }));
                    TEST(Action_Should_Throw_InvocationException(() => { receiverSource.PlayLater(iMockMediaRetriever.Object); }));
                    TEST(Action_Should_Throw_InvocationException(() => { receiverSource.Play(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { receiverSource.Pause(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { receiverSource.Stop(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { receiverSource.Previous(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { receiverSource.Next(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { receiverSource.Seek(100); }));
                    TEST(Action_Should_Throw_InvocationException(() => { var x = receiverSource.TransportState; }));

                    TEST(Action_Should_Throw_InvocationException(() => { analogSource1.PlayNow(iMockMediaRetriever.Object); }));
                    TEST(Action_Should_Throw_InvocationException(() => { analogSource1.PlayNext(iMockMediaRetriever.Object); }));
                    TEST(Action_Should_Throw_InvocationException(() => { analogSource1.PlayLater(iMockMediaRetriever.Object); }));
                    TEST(Action_Should_Throw_InvocationException(() => { analogSource1.Play(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { analogSource1.Pause(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { analogSource1.Stop(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { analogSource1.Previous(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { analogSource1.Next(); }));
                    TEST(Action_Should_Throw_InvocationException(() => { analogSource1.Seek(100); }));
                    TEST(Action_Should_Throw_InvocationException(() => { var x = analogSource1.TransportState; }));
                    
                    TEST(Source_Switch_Should_Raise_EventCurrentChanged_On_Invoker_Thread(mainRoom, radioSource1));

                    // test preamp switching/volume service
                    TEST(Preamp_Switch_Should_Raise_EventVolumeChanged_On_Invoker_Thread(iMockMainRoom, mainRoom));
                    TEST(Volume_Setter_Should_Raise_EventVolumeChanged_On_Invoker_Thread(iMockMainRoom, mainRoom, iMockModelVolumeControl1));
                    TEST(Preamp_Switch_Should_Change_Volume_On_Invoker_Thread(iMockMainRoom, mainRoom, iMockMainRoomPreamp2, iMockModelVolumeControl1));

                    // test time service
                    TEST(Seconds_Change_Should_Raise_EventSecondsChanged_On_Invoker_Thread(mainRoom, iMockModelTime));

                    // test info service
                    TEST(Track_Change_Should_Raise_EventTrackChanged_On_Invoker_Thread(mainRoom, iMockModelInfo));


                    TEST(Room_Remove_Should_Raise_EventRoomRemoved_On_Invoker_Thread(iHouse, iMockTopologyHouse, iMockMainRoom));
                }
                finally
                {
                    waitHandle.Set();
                }
            }));
            nonInvokerThread.Start();
            waitHandle.WaitOne();
        }

        private bool Action_Should_Throw_InvocationException(Action aAction)
        {
            try
            {
                aAction();
            }
            catch (InvocationException)
            {
                return true;
            }
            return false;
        }

        private bool Action_Should_Throw_InvalidOperationException(Action aAction)
        {
            try
            {
                aAction();
            }
            catch (InvalidOperationException)
            {
                return true;
            }
            return false;
        }

        private bool ChannelChange_Should_Raise_EventChannelChanged(ISource aSource)
        {
            bool raised = false;
            Assert.Check(aSource is IChannelProvider);
            IChannelProvider channelProvider = aSource as IChannelProvider;
            Upnp.DidlLite didl = new Upnp.DidlLite();
            ManualResetEvent waitHandleOpened = new ManualResetEvent(false);

            EventHandler<EventArgs> handlerOpened = (d, e) => { waitHandleOpened.Set(); };
            EventHandler<EventArgs> handlerChannelChanged = (d, e) => { raised = channelProvider.Channel.DidlLite == didl; };

            aSource.EventOpened += handlerOpened;
            channelProvider.EventChannelChanged += handlerChannelChanged;
            waitHandleOpened.WaitOne();
            channelProvider.SetChannel(didl);
            channelProvider.EventChannelChanged -= handlerChannelChanged;
            aSource.EventOpened -= handlerOpened;
            return raised;
        }

        private bool Playlists_Are_Equal(IList<Topology.MrItem> aPlaylist1, IList<Topology.MrItem> aPlaylist2)
        {
            if (aPlaylist1.Count != aPlaylist2.Count)
            {
                return false;
            }
            for (int i = 0; i < aPlaylist1.Count; i++)
            {
                if (aPlaylist1[i] != aPlaylist2[i])
                {
                    return false;
                }
            }
            return true;
        }

        private bool Playlist_DeleteAll_Should_Raise_EventPlaylistChanged(ISource aSource)
        {
            bool raised = false;
            Assert.Check(aSource is IPlaylistSource);
            IPlaylistSource playlistSource = aSource as IPlaylistSource;
            ManualResetEvent waitHandleOpened = new ManualResetEvent(false);

            EventHandler<EventArgs> handlerOpened = (d, e) => { waitHandleOpened.Set(); };
            EventHandler<EventArgs> handlerItemsChanged = (d, e) => { raised = playlistSource.Items.Count == 0; };
            aSource.EventOpened += handlerOpened;
            playlistSource.EventItemsChanged += handlerItemsChanged;
            playlistSource.DeleteAll();
            waitHandleOpened.WaitOne();
            playlistSource.EventItemsChanged -= handlerItemsChanged;
            aSource.EventOpened -= handlerOpened;
            return raised;
        }

        private bool Playlist_Move_Should_Raise_EventPlaylistChanged(ISource aSource, IList<Topology.MrItem> aPlaylistItems, uint aInsertAfterId)
        {
            bool raised = false;
            Assert.Check(aSource is IPlaylistSource);
            IPlaylistSource playlistSource = aSource as IPlaylistSource;
            int expectedCount = playlistSource.Items.Count;

            ManualResetEvent waitHandleOpened = new ManualResetEvent(false);

            EventHandler<EventArgs> handlerOpened = (d, e) => { waitHandleOpened.Set(); };
            EventHandler<EventArgs> handlerItemsChanged = (d, e) => { raised = playlistSource.Items.Count == expectedCount; };
            aSource.EventOpened += handlerOpened;
            playlistSource.EventItemsChanged += handlerItemsChanged;
            playlistSource.Move(aInsertAfterId, aPlaylistItems);
            waitHandleOpened.WaitOne();
            playlistSource.EventItemsChanged -= handlerItemsChanged;
            aSource.EventOpened -= handlerOpened;
            return raised;
        }

        private bool Playlist_Delete_Should_Raise_EventPlaylistChanged(ISource aSource, IList<Topology.MrItem> aPlaylistItems)
        {
            bool raised = false;
            Assert.Check(aSource is IPlaylistSource);
            IPlaylistSource playlistSource = aSource as IPlaylistSource;
            int expectedCount = playlistSource.Items.Count - aPlaylistItems.Count;

            ManualResetEvent waitHandleOpened = new ManualResetEvent(false);

            EventHandler<EventArgs> handlerOpened = (d, e) => { waitHandleOpened.Set(); };
            EventHandler<EventArgs> handlerItemsChanged = (d, e) => { raised = playlistSource.Items.Count == expectedCount; };
            aSource.EventOpened += handlerOpened;
            playlistSource.EventItemsChanged += handlerItemsChanged;
            playlistSource.Delete(aPlaylistItems);
            waitHandleOpened.WaitOne();
            playlistSource.EventItemsChanged -= handlerItemsChanged;
            aSource.EventOpened -= handlerOpened;
            return raised;
        }

        private bool SetStandby_Should_Raise_EventStandbyChanged(IRoom aRoom)
        {
            bool raised = false;
            EventHandler<EventArgs> handler = (d, e) => { raised = aRoom.Standby == true; };
            aRoom.EventStandbyChanged += handler;
            aRoom.Standby = true;
            aRoom.EventStandbyChanged -= handler;
            return raised;
        }

        private bool IPlayModeProvider_ToggleRepeat_Should_Raise_EventRepeatChanged(ISource aSource)
        {
            bool raised = false;
            Assert.Check(aSource is IPlayModeProvider);
            IPlayModeProvider playModeProvider = aSource as IPlayModeProvider;

            ManualResetEvent waitHandleOpened = new ManualResetEvent(false);

            ManualResetEvent waitHandle = new ManualResetEvent(false);
            EventHandler<EventArgs> handlerOpened = (d, e) => { waitHandleOpened.Set(); };
            EventHandler<EventArgs> handlerRepeat = (d, e) => { raised = true; waitHandle.Set(); };
            aSource.EventOpened += handlerOpened;
            playModeProvider.EventRepeatChanged += handlerRepeat;
            waitHandleOpened.WaitOne();
            playModeProvider.Repeat = true;
            waitHandle.WaitOne();
            playModeProvider.EventRepeatChanged -= handlerRepeat;
            aSource.EventOpened -= handlerOpened;
            return raised;
        }

        private bool IPlayModeProvider_ToggleShuffle_Should_Raise_EventShuffleChanged(ISource aSource)
        {
            bool raised = false;
            Assert.Check(aSource is IPlayModeProvider);
            IPlayModeProvider playModeProvider = aSource as IPlayModeProvider;

            ManualResetEvent waitHandleOpened = new ManualResetEvent(false);

            ManualResetEvent waitHandle = new ManualResetEvent(false);
            EventHandler<EventArgs> handlerOpened = (d, e) => { waitHandleOpened.Set(); };
            EventHandler<EventArgs> handlerShuffle = (d, e) => { raised = true; waitHandle.Set(); };
            aSource.EventOpened += handlerOpened;
            playModeProvider.EventShuffleChanged += handlerShuffle;
            waitHandleOpened.WaitOne();
            playModeProvider.Shuffle = true;
            waitHandle.WaitOne();
            playModeProvider.EventShuffleChanged -= handlerShuffle;
            aSource.EventOpened -= handlerOpened;
            return raised;
        }

        private bool EventPlaylistChanged_Should_Raise_EventItemsChanged(ISource aSource, Mock<Topology.IModelSourceMediaRenderer> aMockMediaRenderer)
        {
            bool raised = false;
            Assert.Check(aSource is IPlaylistSource);
            IPlaylistSource playlistSource = aSource as IPlaylistSource;

            ManualResetEvent waitHandleOpened = new ManualResetEvent(false);

            EventHandler<EventArgs> handlerOpened = (d, e) => { waitHandleOpened.Set(); };
            EventHandler<EventArgs> handlerItemsChanged = (d, e) => { raised = true; };
            aSource.EventOpened += handlerOpened;
            playlistSource.EventItemsChanged += handlerItemsChanged;
            aMockMediaRenderer.Raise(r => r.EventPlaylistChanged += null, EventArgs.Empty);
            waitHandleOpened.WaitOne();
            playlistSource.EventItemsChanged -= handlerItemsChanged;
            aSource.EventOpened -= handlerOpened;
            return raised;
        }

        private bool Playlist_Insert_Should_Raise_EventPlaylistChanged(ISource aSource, uint aInsertAfterId, IMediaRetriever aRetriever)
        {
            bool raised = false;
            Assert.Check(aSource is IPlaylistSource);
            IPlaylistSource playlistSource = aSource as IPlaylistSource;
            int expectedCount = playlistSource.Items.Count + aRetriever.Media.Count;

            ManualResetEvent waitHandleOpened = new ManualResetEvent(false);

            EventHandler<EventArgs> handlerOpened = (d, e) => { waitHandleOpened.Set(); };
            EventHandler<EventArgs> handlerItemsChanged = (d, e) => { raised = playlistSource.Items.Count == expectedCount; };
            aSource.EventOpened += handlerOpened;
            playlistSource.EventItemsChanged += handlerItemsChanged;
            playlistSource.Insert(aInsertAfterId, aRetriever);
            waitHandleOpened.WaitOne();
            playlistSource.EventItemsChanged -= handlerItemsChanged;
            aSource.EventOpened -= handlerOpened;
            return raised;
        }

        private bool Track_Change_Should_Raise_EventTrackChanged(IRoom aRoom, Mock<Topology.IModelInfo> aMockModelInfo)
        {
            bool raised = false;
            var didl = new Upnp.DidlLite();
            var item = new Upnp.item();
            didl.Add(item);

            Topology.Channel newChannel = new Topology.Channel("http://foo", didl);
            EventHandler<EventArgs> handlerInfoOpened = (d, e) => { };
            EventHandler<EventArgs> handlerTrackChanged = (d, e) => { raised = aRoom.Info.Track == item; };

            aRoom.Info.EventOpened += handlerInfoOpened;
            aRoom.Info.EventTrackChanged += handlerTrackChanged;
            iTrack = newChannel;
            aMockModelInfo.Raise(r => r.EventTrackChanged += null, EventArgs.Empty);
            aRoom.Info.EventTrackChanged -= handlerTrackChanged;
            aRoom.Info.EventOpened -= handlerInfoOpened;
            return raised;
        }

        private bool Track_Change_Should_Raise_EventTrackChanged_On_Invoker_Thread(IRoom aRoom, Mock<Topology.IModelInfo> aMockModelInfo)
        {
            Assert.Check(iInvoker.InvokeRequired);
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            bool raised = false;
            EventHandler<EventArgs> handlerInfoOpened = (d, e) => { };
            EventHandler<EventArgs> handlerTrackChanged = (d, e) => { raised = true; waitHandle.Set(); };

            IRoomInfo info = null;
            ManualResetEvent waitHandle2 = new ManualResetEvent(false);
            iInvoker.BeginInvoke((Action)(() =>
            {
                info = aRoom.Info;
                waitHandle2.Set();
            }));
            waitHandle2.WaitOne();

            info.EventOpened += handlerInfoOpened;
            info.EventTrackChanged += handlerTrackChanged;
            aMockModelInfo.Raise(r => r.EventTrackChanged += null, EventArgs.Empty);
            waitHandle.WaitOne();
            info.EventTrackChanged -= handlerTrackChanged;
            info.EventOpened -= handlerInfoOpened;
            return raised;
        }

        private bool Null_Track_Should_Not_Cause_NullReferenceException(IRoom aRoom, Mock<Topology.IModelInfo> aMockModelInfo)
        {
            bool raised = false;

            EventHandler<EventArgs> handlerInfoOpened = (d, e) => { };
            EventHandler<EventArgs> handlerTrackChanged = (d, e) => { raised = aRoom.Info.Track == null; };

            aRoom.Info.EventOpened += handlerInfoOpened;
            aRoom.Info.EventTrackChanged += handlerTrackChanged;
            iTrack = null;
            aMockModelInfo.Raise(r => r.EventTrackChanged += null, EventArgs.Empty);
            aRoom.Info.EventTrackChanged -= handlerTrackChanged;
            aRoom.Info.EventOpened -= handlerInfoOpened;
            return raised;
        }

        private bool Null_Metatext_Should_Not_Cause_NullReferenceException(IRoom aRoom, Mock<Topology.IModelInfo> aMockModelInfo)
        {
            bool raised = false;

            EventHandler<EventArgs> handlerInfoOpened = (d, e) => { };
            EventHandler<EventArgs> handlerMetatextChanged = (d, e) => { raised = aRoom.Info.Metatext == null; };

            aRoom.Info.EventOpened += handlerInfoOpened;
            aRoom.Info.EventMetatextChanged += handlerMetatextChanged;
            iMetatext = null;
            aMockModelInfo.Raise(r => r.EventMetaTextChanged += null, EventArgs.Empty);
            aRoom.Info.EventMetatextChanged -= handlerMetatextChanged;
            aRoom.Info.EventOpened -= handlerInfoOpened;
            return raised;
        }

        private bool Seconds_Change_Should_Raise_EventSecondsChanged(IRoom aRoom, Mock<Topology.IModelTime> aMockModelTime)
        {
            bool raised = false;
            EventHandler<EventArgs> handlerTimeOpened = (d, e) => { };
            EventHandler<EventArgs> handlerSecondsChanged = (d, e) => { raised = aRoom.Time.Seconds == 50; };

            aRoom.Time.EventOpened += handlerTimeOpened;
            aRoom.Time.EventSecondsChanged += handlerSecondsChanged;
            iSeconds = 50;
            aMockModelTime.Raise(r => r.EventSecondsChanged += null, EventArgs.Empty);
            aRoom.Time.EventSecondsChanged -= handlerSecondsChanged;
            aRoom.Time.EventOpened -= handlerTimeOpened;
            return raised;
        }

        private bool Seconds_Change_Should_Raise_EventSecondsChanged_On_Invoker_Thread(IRoom aRoom, Mock<Topology.IModelTime> aMockModelTime)
        {
            Assert.Check(iInvoker.InvokeRequired);
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            bool raised = false;
            EventHandler<EventArgs> handlerTimeOpened = (d, e) => { };
            EventHandler<EventArgs> handlerSecondsChanged = (d, e) => { raised = true; waitHandle.Set(); };

            IRoomTime time = null;
            ManualResetEvent waitHandle2 = new ManualResetEvent(false);
            iInvoker.BeginInvoke((Action)(() =>
            {
                time = aRoom.Time;
                waitHandle2.Set();
            }));
            waitHandle2.WaitOne();

            time.EventOpened += handlerTimeOpened;
            time.EventSecondsChanged += handlerSecondsChanged;
            aMockModelTime.Raise(r => r.EventSecondsChanged += null, EventArgs.Empty);
            waitHandle.WaitOne();
            time.EventSecondsChanged -= handlerSecondsChanged;
            time.EventOpened -= handlerTimeOpened;
            return raised;
        }


        private bool Preamp_Switch_Should_Raise_EventVolumeChanged(Mock<Topology.IRoom> aMockRoom, IRoom aRoom)
        {
            bool raised = false;
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            EventHandler<EventArgs> handlerVolumeOpened = (d, e) => { };
            EventHandler<EventArgs> handlerVolumeChanged = (d, e) => { raised = true; waitHandle.Set(); };

            aRoom.Volume.EventOpened += handlerVolumeOpened;
            aRoom.Volume.EventVolumeChanged += handlerVolumeChanged;
            aMockRoom.Raise(r => r.EventPreampChanged += null, EventArgs.Empty);
            waitHandle.WaitOne();
            aRoom.Volume.EventVolumeChanged -= handlerVolumeChanged;
            aRoom.Volume.EventOpened -= handlerVolumeOpened;
            return raised;
        }

        private bool Preamp_Switch_Should_Raise_EventVolumeChanged_On_Invoker_Thread(Mock<Topology.IRoom> aMockRoom, IRoom aRoom)
        {
            Assert.Check(iInvoker.InvokeRequired);
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            bool raised = false;
            EventHandler<EventArgs> handlerVolumeOpened = (d, e) => { };
            EventHandler<EventArgs> handlerVolumeChanged = (d, e) => { raised = true; waitHandle.Set(); };

            IRoomVolume volume = null;
            ManualResetEvent waitHandle2 = new ManualResetEvent(false);
            iInvoker.BeginInvoke((Action)(() =>
            {
                volume = aRoom.Volume;
                waitHandle2.Set();
            }));
            waitHandle2.WaitOne();

            volume.EventOpened += handlerVolumeOpened;
            volume.EventVolumeChanged += handlerVolumeChanged;
            aMockRoom.Raise(r => r.EventPreampChanged += null, EventArgs.Empty);
            waitHandle.WaitOne();
            volume.EventVolumeChanged -= handlerVolumeChanged;
            volume.EventOpened -= handlerVolumeOpened;
            return raised;
        }


        private bool Volume_Setter_Should_Raise_EventVolumeChanged(Mock<Topology.IRoom> aMockRoom, IRoom aRoom)
        {
            bool raised = false;
            EventHandler<EventArgs> handlerVolumeOpened = (d, e) => { };
            EventHandler<EventArgs> handlerVolumeChanged = (d, e) => { raised = aRoom.Volume.Volume == 30; };

            aRoom.Volume.EventVolumeChanged += handlerVolumeChanged;
            aRoom.Volume.EventOpened += handlerVolumeOpened;
            aRoom.Volume.Volume = 30;
            aRoom.Volume.EventOpened -= handlerVolumeOpened;
            aRoom.Volume.EventVolumeChanged -= handlerVolumeChanged;
            return raised;
        }

        private bool Volume_Setter_Should_Raise_EventVolumeChanged_On_Invoker_Thread(Mock<Topology.IRoom> aMockRoom, IRoom aRoom, Mock<Topology.IModelVolumeControl> aMockModelVolumeControl)
        {
            Assert.Check(iInvoker.InvokeRequired);
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            bool raised = false;
            EventHandler<EventArgs> handlerVolumeOpened = (d, e) => { };
            EventHandler<EventArgs> handlerVolumeChanged = (d, e) => { raised = true; waitHandle.Set(); };
            IRoomVolume volume = null;
            ManualResetEvent waitHandle2 = new ManualResetEvent(false);
            iInvoker.BeginInvoke((Action)(() =>
            {
                volume = aRoom.Volume;
                waitHandle2.Set();
            }));
            waitHandle2.WaitOne();

            volume.EventVolumeChanged += handlerVolumeChanged;
            volume.EventOpened += handlerVolumeOpened;
            aMockModelVolumeControl.Raise(m => m.EventVolumeChanged += null, EventArgs.Empty);
            waitHandle.WaitOne();
            volume.EventOpened -= handlerVolumeOpened;
            volume.EventVolumeChanged -= handlerVolumeChanged;
            return raised;
        }

        private bool Preamp_Switch_Should_Change_Volume(Mock<Topology.IRoom> aMockRoom, IRoom aRoom, Mock<Topology.IPreamp> aMockPreamp, Mock<Topology.IModelVolumeControl> aMockVolumeControl)
        {
            bool raised = false;
            ManualResetEvent waitHandleOpened = new ManualResetEvent(false);
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            EventHandler<EventArgs> handlerVolumeOpened = (d, e) => { waitHandleOpened.Set(); };
            EventHandler<EventArgs> handlerVolumeChanged = (d, e) => {
                raised = aRoom.Volume.Volume == aMockVolumeControl.Object.Volume; waitHandle.Set(); 
            };

            aRoom.Volume.EventOpened += handlerVolumeOpened;
            aRoom.Volume.EventVolumeChanged += handlerVolumeChanged;
            waitHandle.WaitOne();
            waitHandleOpened.WaitOne();
            waitHandle.Reset();
            waitHandleOpened.Reset();
            iMainRoomPreamp = aMockPreamp.Object;
            aMockRoom.Raise(r => r.EventPreampChanged += null, EventArgs.Empty);
            waitHandle.WaitOne();
            waitHandleOpened.WaitOne();
            aRoom.Volume.EventVolumeChanged -= handlerVolumeChanged;
            aRoom.Volume.EventOpened -= handlerVolumeOpened;
            return raised;
        }

        private bool Preamp_Switch_Should_Change_Volume_On_Invoker_Thread(Mock<Topology.IRoom> aMockRoom, IRoom aRoom, Mock<Topology.IPreamp> aMockPreamp, Mock<Topology.IModelVolumeControl> aMockVolumeControl)
        {
            Assert.Check(iInvoker.InvokeRequired);
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            bool raised = false;
            EventHandler<EventArgs> handlerVolumeOpened = (d, e) => { };
            EventHandler<EventArgs> handlerVolumeChanged = (d, e) => { raised = true; waitHandle.Set(); };

            IRoomVolume volume = null;
            ManualResetEvent waitHandle2 = new ManualResetEvent(false);
            iInvoker.BeginInvoke((Action)(() =>
            {
                volume = aRoom.Volume;
                waitHandle2.Set();
            }));
            waitHandle2.WaitOne();
            volume.EventOpened += handlerVolumeOpened;
            volume.EventVolumeChanged += handlerVolumeChanged;
            aMockRoom.Raise(r => r.EventPreampChanged += null, EventArgs.Empty);
            waitHandle.WaitOne();
            volume.EventVolumeChanged -= handlerVolumeChanged;
            volume.EventOpened -= handlerVolumeOpened;
            return raised;
        }

        private bool Source_Switch_Should_Raise_EventCurrentChanged(IRoom aRoom, ISource aSource)
        {
            bool raised = false;
            EventHandler<EventArgs> handler = (d, e) => { raised = aRoom.Current.Udn == aSource.Udn; };
            aRoom.EventCurrentChanged += handler;
            aSource.Select();
            aRoom.EventCurrentChanged -= handler;
            return raised;
        }

        private bool Source_Switch_Should_Raise_EventCurrentChanged_On_Invoker_Thread(IRoom aRoom, ISource aSource)
        {
            Assert.Check(iInvoker.InvokeRequired);
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            bool raised = false;
            EventHandler<EventArgs> handler = (d, e) => { 
                raised = Thread.CurrentThread == iInvokerThread; 
                waitHandle.Set(); };
            aRoom.EventCurrentChanged += handler;
            iInvoker.BeginInvoke((Action)(() =>
            {
                aSource.Select();
            }));
            waitHandle.WaitOne();
            aRoom.EventCurrentChanged -= handler;
            return raised;
        }

        private bool Source_Removed_Should_Raise_EventNameChanged(Mock<Topology.IRoom> aMockRoom, Mock<Topology.ISource> aMockRemovedSource, ISource aRenamedSource, string aNewName)
        {
            bool raised = false;
            EventHandler<EventArgs> handler = (d, e) => { raised = aRenamedSource.Name == aNewName; };
            aRenamedSource.EventNameChanged += handler;
            aMockRoom.Raise(r => r.EventSourceRemoved += null, new Topology.EventArgsSource(aMockRemovedSource.Object));
            aRenamedSource.EventNameChanged -= handler;
            return raised;
        }

        private bool Source_Removed_Should_Raise_EventSourceRemoved(IRoom aRoom, Mock<Topology.IRoom> aMockRoom, Mock<Topology.ISource> aMockRemovedSource, ISource aRemovedSource)
        {
            bool raised = false;
            EventHandler<EventArgsItem<ISource>> handler = (d, e) => { raised = e.Item == aRemovedSource; };
            aRoom.EventSourceRemoved += handler;
            aMockRoom.Raise(r => r.EventSourceRemoved += null, new Topology.EventArgsSource(aMockRemovedSource.Object));
            aRoom.EventSourceRemoved -= handler;
            return raised;
        }

        private bool Source_Name_Is_Equal_To(ISource aSource, string aSourceName)
        {
            return aSource.Name == aSourceName;
        }

        private bool Source_Is_Of_Type(ISource aSource, Type aType)
        {
            return aType.IsAssignableFrom(aSource.GetType());
        }

        private bool Source_Add_Should_Raise_EventSourceInserted(IRoom aRoom, Mock<Topology.IRoom> aMockTopologyRoom, Mock<Topology.ISource> aMockSource)
        {
            bool eventHandled = false;
            EventHandler<EventArgsItemInsert<Kinsky.ISource>> handler = (d, e) => { if (e.Item.Udn == aMockSource.Object.Udn) eventHandled = true; };
            aRoom.EventSourceInserted += handler;
            aMockTopologyRoom.Raise(m => m.EventSourceAdded += null, new Topology.EventArgsSource(aMockSource.Object));
            aRoom.EventSourceInserted -= handler;
            return eventHandled;
        }

        private bool Room_Should_Contain_Source_Count(IRoom aRoom, int aSourceCount)
        {
            return aRoom.Sources.Count == aSourceCount;
        }

        private bool House_Should_Contain_Room_Count(IHouse aHouse, int aRoomCount)
        {
            return aHouse.Rooms.Count == aRoomCount;
        }

        private bool Room_Add_Should_Raise_EventRoomInserted(IHouse aHouse, Mock<Topology.IHouse> aMockHouse, Mock<Topology.IRoom> aMockRoom)
        {
            bool eventHandled = false;
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            EventHandler<EventArgsItemInsert<Kinsky.IRoom>> handler = (d, e) => { if (e.Item.Name == aMockRoom.Object.Name) eventHandled = true; waitHandle.Set(); };
            aHouse.EventRoomInserted += handler;
            aMockHouse.Raise(m => m.EventRoomAdded += null, new Topology.EventArgsRoom(aMockRoom.Object));
            waitHandle.WaitOne();
            aHouse.EventRoomInserted -= handler;
            return eventHandled;
        }

        private bool Room_Add_Should_Raise_EventRoomInserted_On_Invoker_Thread(IHouse aHouse, Mock<Topology.IHouse> aMockHouse, Mock<Topology.IRoom> aMockRoom)
        {
            Assert.Check(iInvoker.InvokeRequired);
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            bool eventHandled = false;
            EventHandler<EventArgsItemInsert<Kinsky.IRoom>> handler = (d, e) => { eventHandled = Thread.CurrentThread == iInvokerThread; waitHandle.Set(); };
            aHouse.EventRoomInserted += handler;
            aMockHouse.Raise(m => m.EventRoomAdded += null, new Topology.EventArgsRoom(aMockRoom.Object));
            waitHandle.WaitOne();
            aHouse.EventRoomInserted -= handler;
            return eventHandled;
        }

        private bool Room_Remove_Should_Raise_EventRoomRemoved(IHouse aHouse, Mock<Topology.IHouse> aMockHouse, Mock<Topology.IRoom> aMockRoom)
        {
            bool eventHandled = false;
            EventHandler<EventArgsItem<Kinsky.IRoom>> handler = (d, e) => { if (e.Item.Name == aMockRoom.Object.Name) eventHandled = true; };
            aHouse.EventRoomRemoved += handler;
            aMockHouse.Raise(m => m.EventRoomRemoved += null, new Topology.EventArgsRoom(aMockRoom.Object));
            aHouse.EventRoomRemoved -= handler;
            return eventHandled;
        }

        private bool Room_Remove_Should_Raise_EventRoomRemoved_On_Invoker_Thread(IHouse aHouse, Mock<Topology.IHouse> aMockHouse, Mock<Topology.IRoom> aMockRoom)
        {
            Assert.Check(iInvoker.InvokeRequired);
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            bool eventHandled = false;
            EventHandler<EventArgsItem<Kinsky.IRoom>> handler = (d, e) => { eventHandled = Thread.CurrentThread == iInvokerThread; waitHandle.Set(); };
            aHouse.EventRoomRemoved += handler;
            aMockHouse.Raise(m => m.EventRoomRemoved += null, new Topology.EventArgsRoom(aMockRoom.Object));
            waitHandle.WaitOne();
            aHouse.EventRoomRemoved -= handler;
            return eventHandled;
        }

        private bool Room_Name_Should_Equal(IRoom aRoom, string aName)
        {
            return aRoom.Name == aName;
        }
        #endregion

        private MockRepository iRepository;

        private Mock<Topology.IModelFactory> iModelFactory;
        private Mock<Linn.Topology.IHouse> iMockTopologyHouse;
        private Mock<Linn.Topology.IModelSenders> iMockModelSenders;
        private TestInvoker iInvoker;

        private House iHouse;

        private Mock<Topology.IRoom> iMockMainRoom;
        private bool iMainRoomStandby;
        private Topology.IPreamp iMainRoomPreamp;
        private Topology.ISource iMainRoomCurrentSource;
        private Mock<Topology.IGroup> iMockMainRoomGroup1;
        private Mock<Topology.IGroup> iMockMainRoomGroup2;

        private Mock<Topology.IPreamp> iMockMainRoomPreamp1;
        private Mock<Topology.IPreamp> iMockMainRoomPreamp2;

        private Mock<Topology.ISource> iMockMainRoomPlaylistSource1;
        private Mock<Topology.ISource> iMockMainRoomRadioSource1;
        private Mock<Topology.ISource> iMockMainRoomPlaylistSource2;
        private Mock<Topology.ISource> iMockMainRoomRadioSource2;
        private Mock<Topology.ISource> iMockMainRoomAuxSource1;
        private Mock<Topology.ISource> iMockMainRoomAuxSource2;
        private Mock<Topology.ISource> iMockMainRoomDiscSource1;
        private Mock<Topology.ISource> iMockMainRoomUpnpSource1;
        private Mock<Topology.ISource> iMockMainRoomReceiverSource;
        private List<Topology.ISource> iMainRoomSources = new List<Topology.ISource>();

        private Mock<Topology.IRoom> iMockSecondaryRoom;
        private List<Topology.ISource> iSecondaryRoomSources = new List<Topology.ISource>();

        private Mock<Topology.IModelVolumeControl> iMockModelVolumeControl1;
        private Mock<Topology.IModelVolumeControl> iMockModelVolumeControl2;

        private PreampState iPreampState1;
        private PreampState iPreampState2;

        private Mock<Topology.IModelTime> iMockModelTime;
        private uint iDuration = 300;
        private uint iSeconds = 0;

        private Mock<Topology.IModelInfo> iMockModelInfo;
        private Topology.Channel iTrack = new Topology.Channel("http://localhost", new Upnp.DidlLite());
        private Upnp.DidlLite iMetatext = new Upnp.DidlLite();

        private Mock<Topology.IModelSourceMediaRenderer> iMockModelSourceMediaRenderer;
        private List<Topology.MrItem> iPlaylist = new List<Topology.MrItem>();
        private bool iRepeat;
        private bool iShuffle;

        private Mock<Topology.IModelSourceDiscPlayer> iMockModelSourceDiscPlayer;

        private Mock<IMediaRetriever> iMockMediaRetriever = new Mock<IMediaRetriever>();

        private Mock<Topology.IModelSourceRadio> iMockModelSourceRadio;
        private Mock<Topology.IModelSourceReceiver> iMockModelSourceReceiver;
        private Upnp.DidlLite iChannel;

        private Thread iMainThread;
        private Thread iInvokerThread;
        private bool iRunning;
        private Queue<Action> iQueue;

        private class PreampState
        {
            public uint Volume = 50;
            public uint VolumeLimit = 100;
            public bool Mute = false;
        }

        private class TestInvoker : IInvoker
        {
            
        private Thread iMainThread;
        private Thread iInvokerThread;
        private Queue<Action> iQueue;

            public TestInvoker(Thread aInvokerThread, Thread aMainThread, Queue<Action> aQueue)
            {
                iInvokerThread = aInvokerThread;
                iMainThread = aMainThread;
                iQueue = aQueue;
            }

            #region IInvoker Members

            public bool InvokeRequired
            {
                get { return Thread.CurrentThread != iMainThread && Thread.CurrentThread != iInvokerThread;}
            }

            public void BeginInvoke(Delegate aDelegate, params object[] aArgs)
            { 
                lock (iQueue)
                    {
                        iQueue.Enqueue(() => {
                            try
                            {
                                aDelegate.DynamicInvoke(aArgs);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }
                        });
                    }
            }

            public bool TryBeginInvoke(Delegate aDelegate, params object[] aArgs)
            {
                if (InvokeRequired){
                    BeginInvoke(aDelegate, aArgs);
                    return true;
                }return false;
            }

            #endregion            
        }
    }
}
