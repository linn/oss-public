using System;
using System.Net;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Linn;
using Linn.Kinsky;
using Linn.Toolkit.Ios;

namespace KinskyTouch
{

    // The name AppDelegateIPhone is referenced in the MainWindowIPhone.xib file.
    public partial class AppDelegateIphone : UIApplicationDelegate, IStack
    {
        // This method is invoked when the application has loaded its UI and its ready to run
        public override bool FinishedLaunching (UIApplication app, NSDictionary options)
        {
            helper.Helper.SetStackExtender(this);
            helper.Helper.Stack.EventStatusChanged += StatusChanged;

            ArtworkCacheInstance.Instance = new ArtworkCache();

            controlRotaryVolume.ViewBar.FontSize = 12.0f;
            controlRotaryVolume.ViewBar.InnerCircleRadius = 25.0f;
            controlRotaryVolume.ViewBar.OuterCircleRadius = 30.0f;
            iViewWidgetVolumeControl = new ViewWidgetVolumeControl(controlRotaryVolume);

            iViewWidgetVolumeRotary = new ViewWidgetVolumeRotary("VolumeRotary", null);

            iViewWidgetVolumeButtons = new ViewWidgetVolumeButtons("VolumeButtons", null);
            iViewWidgetVolumeButtons.RepeatInterval = 0.1f;

            iVolumeController = new VolumeControllerIphone(viewControllerNowPlaying, iViewWidgetVolumeRotary, iViewWidgetVolumeButtons, controlVolume, controlTime, scrollView);
            iVolumeController.SetRockers(helper.OptionEnableRocker.Native);

            controlRotaryTime.ViewBar.FontSize = 12.0f;
            controlRotaryTime.ViewBar.InnerCircleRadius = 25.0f;
            controlRotaryTime.ViewBar.OuterCircleRadius = 30.0f;
            iViewWidgetTime = new ViewWidgetTime(controlRotaryTime, viewHourGlass);

            iViewWidgetTimeRotary = new ViewWidgetTimeRotary("TimeRotary", null, iViewWidgetTime);

            iViewWidgetTimeButtons = new ViewWidgetTimeButtons("TimeButtons", null, iViewWidgetTime);
            iViewWidgetTimeButtons.RepeatInterval = 0.1f;

            iTimeController = new TimeControllerIphone(viewControllerNowPlaying, iViewWidgetTimeRotary, iViewWidgetTimeButtons, controlTime, controlVolume, scrollView);
            iTimeController.SetRockers(helper.OptionEnableRocker.Native);

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
                        //new System.Threading.Timer(StartStack, null, 1000, System.Threading.Timeout.Infinite);
                        StartStack();
                    }
                    iLoaded = true;
                }
            }).BeginInvoke(null, null);
            
            Trace.Level = Trace.kKinsky;

            // If you have defined a view, add it here:
            navigationController.View.Frame = new RectangleF(PointF.Empty, viewBrowser.Frame.Size);
            viewBrowser.InsertSubview(navigationController.View, 0);

            window.AddSubview(viewController.View);
            
            window.MakeKeyAndVisible();
            
            return true;
        }

        public override void WillTerminate(UIApplication application)
        {
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
                        //new System.Threading.Timer(StartStack, null, 1000, System.Threading.Timeout.Infinite);
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
            iMediator.Open();
            iLibrary.Start(aIpAddress);
            iRemotePlaylists.Start(aIpAddress);
            iHttpClient.Start();
            iHttpServer.Start(aIpAddress);
            iLocator.Start();
        }

        public void Stop()
        {
            iLocator.Stop();
            iHttpServer.Stop();
            iHttpClient.Stop();
            iRemotePlaylists.Stop();
            iLibrary.Stop();
            iMediator.Close();
        }
        
        private void AddViews()
        {
            UIButton buttonStandby = new UIButton(new RectangleF(0, 0, 29, 29));
            buttonStandby.ShowsTouchWhenHighlighted = true;
            buttonStandby.SetImage(new UIImage("StandbyOn.png"), UIControlState.Normal);
            buttonStandby.SetImage(new UIImage("StandbyDown.png"), UIControlState.Highlighted);
            buttonStandby.SetImage(new UIImage("Standby.png"), UIControlState.Selected);

            iViewMaster.ViewWidgetSelectorRoom.Add(viewControllerRooms);
            iViewMaster.ViewWidgetSelectorRoom.Add(new ViewWidgetSelectorRoomNavigation(helper.Helper, navigationControllerRoomSource, scrollView, viewControllerSources, buttonRefresh, buttonStandby));

            iViewMaster.ViewWidgetSelectorSource.Add(viewControllerSources);

            viewInfo.TopAlign = true;
            viewInfo.Alignment = UITextAlignment.Left;

            ViewWidgetTrackArtworkRetriever artworkRetriever = new ViewWidgetTrackArtworkRetriever();
            ViewWidgetTrackArtwork artwork = new ViewWidgetTrackArtwork(imageViewArtwork);
            artworkRetriever.AddReceiver(artwork);
            artworkRetriever.AddReceiver(new ImageReceiverButton(buttonArtwork));

            iViewMaster.ViewWidgetTrack.Add(artworkRetriever);
            iViewMaster.ViewWidgetTrack.Add(artwork);
            iViewMaster.ViewWidgetTrack.Add(new ViewWidgetTrackMetadata(viewInfo, helper.OptionExtendedTrackInfo));

            ViewWidgetTransportControl transportControl = new ViewWidgetTransportControl(buttonLeft, buttonCentre, buttonRight);
            iViewMaster.ViewWidgetTransportControlDiscPlayer.Add(transportControl);
            iViewMaster.ViewWidgetTransportControlMediaRenderer.Add(transportControl);
            iViewMaster.ViewWidgetTransportControlRadio.Add(transportControl);

            iViewMaster.ViewWidgetVolumeControl.Add(iViewWidgetVolumeControl);
            iViewMaster.ViewWidgetVolumeControl.Add(iViewWidgetVolumeRotary);
            iViewMaster.ViewWidgetVolumeControl.Add(iViewWidgetVolumeButtons);

            iViewMaster.ViewWidgetMediaTime.Add(iViewWidgetTime);
            iViewMaster.ViewWidgetMediaTime.Add(iViewWidgetTimeRotary);
            iViewMaster.ViewWidgetMediaTime.Add(iViewWidgetTimeButtons);

            iViewMaster.ViewWidgetPlayMode.Add(new ViewWidgetPlayMode(sourceToolbar, buttonShuffle, buttonRepeat));
            sourceToolbar.Initialise(buttonShuffle, buttonRepeat);

            iViewMaster.ViewWidgetPlaylist.Add(new ViewWidgetPlaylistMediaRenderer(tableViewSource, sourceToolbar, new UIButton(), iViewSaveSupport, helper.OptionGroupTracks));
            iViewMaster.ViewWidgetPlaylistRadio.Add(new ViewWidgetPlaylistRadio(tableViewSource, new UIButton(), iViewSaveSupport));

            ViewWidgetPlaylistReceiver playlistReceiver = new ViewWidgetPlaylistReceiver(tableViewSource, new UIButton(), imageViewPlaylistAux, iViewSaveSupport);
            iViewMaster.ViewWidgetPlaylistReceiver.Add(playlistReceiver);
            iViewMaster.ViewWidgetSelectorRoom.Add(playlistReceiver);

            iViewMaster.ViewWidgetPlaylistDiscPlayer.Add(new ViewWidgetPlaylistDiscPlayer(imageViewPlaylistAux));
            iViewMaster.ViewWidgetPlaylistAux.Add(new ViewWidgetPlaylistAux(imageViewPlaylistAux));

            ViewWidgetBrowserRoot viewBrowser = navigationController.TopViewController as ViewWidgetBrowserRoot;
            viewBrowser.Initialise(new Location(iLocator.Root), iPlaySupport, new ConfigControllerIphone(viewController, helper.Helper), helper.OptionInsertMode, helper.Helper.LastLocation);

            iViewMaster.ViewWidgetButtonStandby.Add(new ViewWidgetButtonStandby(buttonStandby));
            iViewMaster.ViewWidgetButtonWasteBin.Add(new ViewWidgetButtonWasteBin(sourceToolbar.BarButtonItemDelete));
            iViewMaster.ViewWidgetButtonSave.Add(new ViewWidgetButtonSave(sourceToolbar.BarButtonItemSave));
        }

        private void StartStack(object aObject)
        {
            StartStack();
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

        private VolumeControllerIphone iVolumeController;
        private TimeControllerIphone iTimeController;

        private ViewMaster iViewMaster;

        private ViewWidgetVolumeControl iViewWidgetVolumeControl;
        private ViewWidgetVolumeRotary iViewWidgetVolumeRotary;
        private ViewWidgetVolumeButtons iViewWidgetVolumeButtons;

        private ViewWidgetTime iViewWidgetTime;
        private ViewWidgetTimeRotary iViewWidgetTimeRotary;
        private ViewWidgetTimeButtons iViewWidgetTimeButtons;

        private SaveViewController.Saver iSaver;
    }
}
