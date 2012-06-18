using System;
using System.Drawing;
using System.Net;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Linn;
using Linn.Kinsky;
using Linn.Toolkit.Ios;

namespace KinskyTouch
{
    // The name AppDelegateIPad is referenced in the MainWindowIPad.xib file.
    public abstract class AppDelegate : UIApplicationDelegate, IStack
    {
        // This method is invoked when the application has loaded its UI and its ready to run
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            helper.Helper.SetStackExtender(this);
            helper.Helper.Stack.EventStatusChanged += StatusChanged;

            ArtworkCacheInstance.Instance = new ArtworkCache();

            controlRotaryVolume.ViewBar.FontSize = 15.0f;
            controlRotaryVolume.ViewBar.InnerCircleRadius = 30.0f;
            controlRotaryVolume.ViewBar.OuterCircleRadius = 35.0f;
            iViewWidgetVolumeControl = new ViewWidgetVolumeControl(controlRotaryVolume);

            iViewWidgetVolumeButtons = new ViewWidgetVolumeButtons("VolumeButtons", null);
            iViewWidgetVolumeButtons.RepeatInterval = 0.1f;

            iViewWidgetVolumeRotary = new ViewWidgetVolumeRotary("VolumeRotary", null);

            iVolumeController = CreateVolumeController(iViewWidgetVolumeButtons, iViewWidgetVolumeRotary, controlVolume);
            iVolumeController.SetRockers(helper.OptionEnableRocker.Native);
            viewController.SetVolumeController(iVolumeController);

            controlRotaryTime.ViewBar.FontSize = 15.0f;
            controlRotaryTime.ViewBar.InnerCircleRadius = 30.0f;
            controlRotaryTime.ViewBar.OuterCircleRadius = 35.0f;
            iViewWidgetTime = new ViewWidgetTime(controlRotaryTime, viewHourGlass);

            iViewWidgetTimeButtons = new ViewWidgetTimeButtons("TimeButtons", null, iViewWidgetTime);
            iViewWidgetTimeButtons.RepeatInterval = 0.1f;

            iViewWidgetTimeRotary = new ViewWidgetTimeRotary("TimeRotary", null, iViewWidgetTime);

            iTimeController = CreateTimeController(iViewWidgetTimeButtons, iViewWidgetTimeRotary, controlTime);
            iTimeController.SetRockers(helper.OptionEnableRocker.Native);
            viewController.SetTimeController(iTimeController);

            controlVolume.Hidden = false;
            controlTime.Hidden = false;

            helper.OptionEnableRocker.EventValueChanged += delegate(object sender, EventArgs e) {
                iVolumeController.SetRockers(helper.OptionEnableRocker.Native);
                iTimeController.SetRockers(helper.OptionEnableRocker.Native);
            };

            Reachability.LocalWifiConnectionStatus();
            Reachability.ReachabilityChanged += delegate(object sender, EventArgs e) {
                helper.Helper.Interface.NetworkChanged();
            };

            new Action(delegate {
                iViewMaster = new ViewMaster();
    
                iHttpServer = new HttpServer(HttpServer.kPortKinskyTouch);
                iHttpClient = new HttpClient();
    
                iLibrary = new MediaProviderLibrary(helper.Helper);
                iRemotePlaylists = new RemotePlaylists(helper.Helper);
                iLocalPlaylists = new LocalPlaylists(helper.Helper, false);
                iLocalPlaylists.SaveDirectory.ResetToDefault();

                iConfigController = CreateConfigController(helper.Helper);
                iPlaySupport = new PlaySupport();
    
                MediaProviderSupport support = new MediaProviderSupport(iHttpServer);
                PluginManager pluginManager = new PluginManager(helper.Helper, iHttpClient, support);
    
                iLocator = new ContentDirectoryLocator(pluginManager, new AppRestartHandler());
                iLocator.Add(MediaProviderLibrary.kLibraryId, iLibrary);
                OptionBool optionRemotePlaylists = iLocator.Add(RemotePlaylists.kRootId, iRemotePlaylists);
                OptionBool optionLocalPlaylists = iLocator.Add(LocalPlaylists.kRootId, iLocalPlaylists);

                iSaveSupport = new SaveSupport(helper.Helper, iRemotePlaylists, optionRemotePlaylists, iLocalPlaylists, optionLocalPlaylists);
                iViewSaveSupport = new ViewSaveSupport(SavePlaylistHandler, iSaveSupport);
    
                helper.Helper.AddOptionPage(iLocator.OptionPage);
    
                AddViews();

                iModel = new Model(iViewMaster, iPlaySupport);
                iMediator = new Mediator(helper.Helper, iModel);

                lock(this)
                {
                    if(!iStackStarted)
                    {
                        StartStack();
                    }
                    iLoaded = true;
                }
            }).BeginInvoke(null, null);
            
            Trace.Level = Trace.kKinskyTouch;
            
            // If you have defined a view, add it here:
            navigationController.View.Frame = new RectangleF(PointF.Empty, viewBrowser.Frame.Size);
            viewBrowser.InsertSubview(navigationController.View, 0);

            /*ArtworkTileViewFactory f = new ArtworkTileViewFactory(iLibrary);
            NSBundle.MainBundle.LoadNib("ArtworkTileView", f, null);
            viewController.View.AddSubview(f.View);
            f.Initialise();*/

            window.AddSubview(viewController.View);
            
            window.MakeKeyAndVisible();

            Console.WriteLine("<FinishedLaunching");
            return true;
        }
     
        public override void WillTerminate(UIApplication application)
        {
            Console.WriteLine("WillTerminate");

            lock(this)
            {
                if(iStackStarted)
                {
                    helper.Helper.Stack.Stop();
                    helper.Helper.Dispose();
                    iStackStarted = false;
                }
                Trace.WriteLine(Trace.kKinskyTouch, "Terminated");
            }
        }

        public override void OnActivated(UIApplication application)
        {
            Trace.WriteLine(Trace.kKinskyTouch, "OnActivated");
            Console.WriteLine("OnActivated");

            Action a = new Action(delegate {
                lock(this)
                {
                    if(!iStackStarted && iLoaded)
                    {
                        StartStack();
                    }
                }
            });
            a.BeginInvoke(null, null);
        }

        public override void OnResignActivation(UIApplication application)
        {
            Trace.WriteLine(Trace.kKinskyTouch, "OnResignActivation");
            Console.WriteLine("OnResignActivation");

            lock(this)
            {
                helper.Helper.Stack.Stop();
                iStackStarted = false;
            }
        }

        public override void DidEnterBackground(UIApplication application)
        {
            Trace.WriteLine(Trace.kKinskyTouch, "DidEnterBackground");
            Console.WriteLine("DidEnterBackground");

            int task = 0;
            task = application.BeginBackgroundTask(delegate {
                if(task != 0)
                {
                    application.EndBackgroundTask(task);
                    task = 0;
                }
            });

            Action a = new Action(delegate {
                lock(this)
                {
                    if(iStackStarted)
                    {
                        helper.Helper.Stack.Stop();
                        iStackStarted = false;
                    }
                }

                application.BeginInvokeOnMainThread(delegate {
                    if(task != 0)
                    {
                        application.EndBackgroundTask(task);
                        task = 0;
                    }
                });
            });
            a.BeginInvoke(null, null);
        }

        public override void ReceiveMemoryWarning(UIApplication aApplication)
        {
            ArtworkCacheInstance.Instance.Flush();
        }
     
        public void Start(IPAddress aIpAddress)
        {
            Console.WriteLine("AppDelegate.Start");
            iMediator.Open();
            iLibrary.Start(aIpAddress);
            iRemotePlaylists.Start(aIpAddress);
            iHttpClient.Start();
            iHttpServer.Start(aIpAddress);
            iLocator.Start();
        }

        public void Stop()
        {
            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
            Console.WriteLine(t.ToString());
            Console.WriteLine("AppDelegate.Stop");
            iLocator.Stop();
            iHttpServer.Stop();
            iHttpClient.Stop();
            iRemotePlaylists.Stop();
            iLibrary.Stop();
            iMediator.Close();
        }

        protected abstract void CreateVolumeController();
        protected abstract void CreateTimeController();
        protected abstract ConfigController CreateConfigController(Helper aHelper);
     
        protected virtual void AddViews()
        {
            UIButton buttonStandby = new UIButton(new RectangleF(0, 0, 29, 29));
            buttonStandby.ShowsTouchWhenHighlighted = true;
            buttonStandby.SetImage(new UIImage("StandbyOn.png"), UIControlState.Normal);
            buttonStandby.SetImage(new UIImage("StandbyDown.png"), UIControlState.Highlighted);
            buttonStandby.SetImage(new UIImage("Standby.png"), UIControlState.Selected);

            ViewWidgetSelectorRoom viewWidgetSelectorRoom = new ViewWidgetSelectorRoom(buttonStandby);
            ViewWidgetSelectorPopover<Linn.Kinsky.Room> popOverRoom = new ViewWidgetSelectorPopover<Room>(helper.Helper, viewWidgetSelectorRoom, viewWidgetSelectorRoom, navigationItemSource.LeftBarButtonItem, navigationItemSource.RightBarButtonItem);
            iViewMaster.ViewWidgetSelectorRoom.Add(viewWidgetSelectorRoom);
            iViewMaster.ViewWidgetSelectorRoom.Add(popOverRoom);

            ViewWidgetSelectorSource viewWidgetSelectorSource = new ViewWidgetSelectorSource();
            ViewWidgetSelectorPopover<Linn.Kinsky.Source> popOverSource = new ViewWidgetSelectorPopover<Source>(helper.Helper, viewWidgetSelectorSource, viewWidgetSelectorSource, navigationItemSource.RightBarButtonItem, navigationItemSource.LeftBarButtonItem);
            iViewMaster.ViewWidgetSelectorSource.Add(viewWidgetSelectorSource);
            iViewMaster.ViewWidgetSelectorSource.Add(popOverSource);

            viewInfo.Alignment = UITextAlignment.Left;
            viewInfo.TopAlign = true;

            viewOverlayInfo.Alignment = UITextAlignment.Center;
            viewOverlayInfo.TopAlign = false;

            ViewWidgetTrackArtworkRetriever artworkRetriever = new ViewWidgetTrackArtworkRetriever();
            ViewWidgetTrackArtwork artwork = new ViewWidgetTrackArtwork(imageViewArtwork);
            artworkRetriever.AddReceiver(artwork);

            iViewMaster.ViewWidgetTrack.Add(artworkRetriever);
            iViewMaster.ViewWidgetTrack.Add(artwork);
            iViewMaster.ViewWidgetTrack.Add(new ViewWidgetTrackMetadata(viewInfo, helper.OptionExtendedTrackInfo));
            iViewMaster.ViewWidgetTrack.Add(new ViewWidgetTrackMetadata(viewOverlayInfo, helper.OptionExtendedTrackInfo));
         
            ViewWidgetTransportControl transportControl = new ViewWidgetTransportControl(buttonLeft, buttonCentre, buttonRight);
            iViewMaster.ViewWidgetTransportControlDiscPlayer.Add(transportControl);
            iViewMaster.ViewWidgetTransportControlMediaRenderer.Add(transportControl);
            iViewMaster.ViewWidgetTransportControlRadio.Add(transportControl);

            iViewMaster.ViewWidgetVolumeControl.Add(iViewWidgetVolumeControl);
            iViewMaster.ViewWidgetVolumeControl.Add(iViewWidgetVolumeButtons);
            iViewMaster.ViewWidgetVolumeControl.Add(iViewWidgetVolumeRotary);

            iViewMaster.ViewWidgetMediaTime.Add(iViewWidgetTime);
            iViewMaster.ViewWidgetMediaTime.Add(iViewWidgetTimeButtons);
            iViewMaster.ViewWidgetMediaTime.Add(iViewWidgetTimeRotary);

            iViewMaster.ViewWidgetPlayMode.Add(new ViewWidgetPlayMode(sourceToolbar, buttonShuffle, buttonRepeat));
            sourceToolbar.Initialise(buttonShuffle, buttonRepeat);

            iViewMaster.ViewWidgetPlaylist.Add(new ViewWidgetPlaylistMediaRenderer(tableViewSource, sourceToolbar, buttonViewInfo, iViewSaveSupport, helper.OptionGroupTracks));
            iViewMaster.ViewWidgetPlaylistRadio.Add(new ViewWidgetPlaylistRadio(tableViewSource, buttonViewInfo, iViewSaveSupport));

            ViewWidgetPlaylistReceiver playlistReceiver = new ViewWidgetPlaylistReceiver(tableViewSource, buttonViewInfo, imageViewPlaylistAux, iViewSaveSupport);
            iViewMaster.ViewWidgetPlaylistReceiver.Add(playlistReceiver);
            iViewMaster.ViewWidgetSelectorRoom.Add(playlistReceiver);

            iViewMaster.ViewWidgetPlaylistDiscPlayer.Add(new ViewWidgetPlaylistDiscPlayer(imageViewPlaylistAux));
            iViewMaster.ViewWidgetPlaylistAux.Add(new ViewWidgetPlaylistAux(imageViewPlaylistAux));

            ViewWidgetBrowserRoot viewBrowser = navigationController.TopViewController as ViewWidgetBrowserRoot;
            viewBrowser.Initialise(new Location(iLocator.Root), iPlaySupport, CreateConfigController(helper.Helper), helper.OptionInsertMode, helper.Helper.LastLocation);

            iViewMaster.ViewWidgetButtonStandby.Add(new ViewWidgetButtonStandby(buttonStandby));
            iViewMaster.ViewWidgetButtonWasteBin.Add(new ViewWidgetButtonWasteBin(sourceToolbar.BarButtonItemDelete));
            iViewMaster.ViewWidgetButtonSave.Add(new ViewWidgetButtonSave(sourceToolbar.BarButtonItemSave));
        }

        private void StartStack()
        {
            lock(this)
            {
                helper.Helper.Stack.Start();
                iStackStarted = true;
    
                // setup timer for rescan 2s later
                new System.Threading.Timer(Rescan, null, 2000, System.Threading.Timeout.Infinite);
            }
        }

        private void Rescan(object aObject)
        {
            lock(this)
            {
                if(iStackStarted)
                {
                    helper.Helper.Rescan();
                }
            }
        }

        private void SavePlaylistHandler(ISaveSupport aSaveSupport)
        {
            iSaver = new SaveViewController.Saver(aSaveSupport);
            //UIViewController controller = new SaveViewController(iSaver, "SaveDialog", NSBundle.MainBundle);
            //UINavigationController controller = new SaveNavigationController(new SaveViewController(iSaver, "SaveDialog", NSBundle.MainBundle));
            UINavigationController controller = new UINavigationController(new SaveViewController(iSaver, aSaveSupport, "SaveDialog", NSBundle.MainBundle));
            controller.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;

            viewController.PresentModalViewController(controller, true);
        }

        private void StatusChanged(object sender, EventArgsStackStatus e)
        {
            switch(e.Status.State)
            {
            case EStackState.eBadInterface:
                Console.WriteLine("eBadInterface");
                helper.Helper.Interface.NetworkChanged();
                break;

            case EStackState.eNoInterface:
                Console.WriteLine("eNoInterface");
                break;

            case EStackState.eNonexistentInterface:
                Console.WriteLine("eNonexistentInterface");
                if(helper.Helper.Interface.Allowed.Count == 2)
                {
                    helper.Helper.Interface.Set(helper.Helper.Interface.Allowed[1]);
                }
                break;

            default:
                break;
            }
        }

        /*private void DO_NOT_CALL()
        {
            new OssKinskyMppBbc.ContentDirectoryFactoryBbc();
            new OssKinskyMppMovieTrailers.MediaProviderMovieTrailersFactory();
            new OssKinskyMppWfmu.ContentDirectoryFactoryWfmu();
            throw new NotSupportedException();
        }*/

        private bool iStackStarted;
        private bool iLoaded;

        private HttpClient iHttpClient;
        private HttpServer iHttpServer;

        private ContentDirectoryLocator iLocator;
        private MediaProviderLibrary iLibrary;
        private LocalPlaylists iLocalPlaylists;
        private RemotePlaylists iRemotePlaylists;
     
        private Mediator iMediator;
        private Model iModel;

        private IViewSaveSupport iViewSaveSupport;
        private PlaySupport iPlaySupport;
        private SaveSupport iSaveSupport;

        private ConfigController iConfigController;

        private ViewMaster iViewMaster;

        private ViewWidgetVolumeControl iViewWidgetVolumeControl;
        private ViewWidgetVolumeButtons iViewWidgetVolumeButtons;
        private ViewWidgetVolumeRotary iViewWidgetVolumeRotary;

        private ViewWidgetTime iViewWidgetTime;
        private ViewWidgetTimeButtons iViewWidgetTimeButtons;
        private ViewWidgetTimeRotary iViewWidgetTimeRotary;

        private SaveViewController.Saver iSaver;
    }
}
