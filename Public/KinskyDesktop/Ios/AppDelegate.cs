using System;
using System.Net;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using Linn;
using Linn.Kinsky;

namespace KinskyTouch
{
    public abstract class AppDelegate : UIApplicationDelegate, IStack
    {
		
		public AppDelegate() : base()
		{
			iScheduler = new Scheduler("StackScheduler", 1);
		}
		
        public override void WillTerminate(UIApplication application)
        {
            Trace.WriteLine(Trace.kKinskyTouch, "AppDelegate.WillTerminate");
			iScheduler.Schedule(()=>
			{
                if(iStackStarted)
                {
                    Helper.Helper.Stack.Stop();
                    Helper.Helper.Dispose();
                    iStackStarted = false;
                }
                Trace.WriteLine(Trace.kKinskyTouch, "AppDelegate.Terminated");
            });
        }

        public override void OnActivated(UIApplication application)
        {
            Trace.WriteLine(Trace.kKinskyTouch, "AppDelegate.OnActivated");

            iScheduler.Schedule(()=>
			{
                if(!iStackStarted && iLoaded)
                {
                    StartStack();
                }
            });
        }

        public override void OnResignActivation(UIApplication application)
        {
            Trace.WriteLine(Trace.kKinskyTouch, "AppDelegate.OnResignActivation");

            iScheduler.Schedule(()=>
			{
				if (iStackStarted)
				{
                    Helper.Helper.Stack.Stop();
                    iStackStarted = false;
				}
            });
        }

        public override void DidEnterBackground(UIApplication application)
        {
            Trace.WriteLine(Trace.kKinskyTouch, "AppDelegate.DidEnterBackground");

            int task = 0;
            task = application.BeginBackgroundTask(delegate {
                if(task != 0)
                {
                    application.EndBackgroundTask(task);
                    task = 0;
                }
            });

				
            Trace.WriteLine(Trace.kKinskyTouch, "AppDelegate.DidEnterBackground 1");
				
			iScheduler.Schedule(()=>
			    {
                    Trace.WriteLine(Trace.kKinskyTouch, "AppDelegate.DidEnterBackground 2");
				    if (iStackStarted)
				    {
                        Helper.Helper.Stack.Stop();
                        iStackStarted = false;
				    }
					Trace.WriteLine(Trace.kKinskyTouch, "AppDelegate.DidEnterBackground 3");

                    application.BeginInvokeOnMainThread(delegate {
                        Trace.WriteLine(Trace.kKinskyTouch, "AppDelegate.DidEnterBackground 4");
                        if(task != 0)
                        {
                            application.EndBackgroundTask(task);
                            task = 0;
                        }
                    });
                });                

            Trace.WriteLine(Trace.kKinskyTouch, "AppDelegate.DidEnterBackground 5");
        }

        public override void ReceiveMemoryWarning(UIApplication aApplication)
        {
            ArtworkCacheInstance.Instance.Flush();
        }

        public void Start(IPAddress aIpAddress)
        {
            Trace.WriteLine(Trace.kKinskyTouch, "AppDelegate.Start");
            iMediator.Open();
            iLibrary.Start(aIpAddress);
            iSharedPlaylists.Start(aIpAddress);
            iHttpClient.Start();
            iHttpServer.Start(aIpAddress);
            iLocator.Start();
        }

        public void Stop()
        {
            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
            Trace.WriteLine(Trace.kKinskyTouch, t.ToString());
            Trace.WriteLine(Trace.kKinskyTouch, "AppDelegate.Stop");
            iLocator.Stop();
            iHttpServer.Stop();
            iHttpClient.Stop();
            iSharedPlaylists.Stop();
            iLibrary.Stop();
            iMediator.Close();
        }

        protected abstract HelperKinskyTouch Helper
        {
            get;
        }

        protected abstract UIViewController ViewController
        {
            get;
        }

        protected void OnFinishedLaunching()
        {
			iScheduler.Schedule(()=>
			{
                if(!iStackStarted)
                {
                    StartStack();
                }
				iLoaded = true;
            });
        }

        protected void SavePlaylistHandler(ISaveSupport aSaveSupport)
        {
            iSaver = new SaveViewController.Saver(aSaveSupport);
            UINavigationController controller = new UINavigationController(new SaveViewController(iSaver, aSaveSupport, "SaveDialog", NSBundle.MainBundle));
            controller.ModalPresentationStyle = UIModalPresentationStyle.FormSheet;

            ViewController.PresentViewController(controller, true, () => {});
        }

        protected void StatusChanged(object sender, EventArgsStackStatus e)
        {
            switch(e.Status.State)
            {
            case EStackState.eBadInterface:
                Trace.WriteLine(Trace.kKinskyTouch, "AppDelegate.StatusChanged: eBadInterface");
                Helper.Helper.Interface.NetworkChanged();
                break;

            case EStackState.eNoInterface:
                Trace.WriteLine(Trace.kKinskyTouch, "AppDelegate.StatusChanged: eNoInterface");
                break;

            case EStackState.eNonexistentInterface:
                Trace.WriteLine(Trace.kKinskyTouch, "AppDelegate.StatusChanged: eNonexistentInterface");
                if(Helper.Helper.Interface.Allowed.Count == 2)
                {
                    Helper.Helper.Interface.Set(Helper.Helper.Interface.Allowed[1]);
                }
                break;

            default:
                break;
            }
        }

        private void StartStack()
        {
            Helper.Helper.Stack.Start();
            iStackStarted = true;
    
            // setup timer for rescan 2s later
            new System.Threading.Timer(Rescan, null, 2000, System.Threading.Timeout.Infinite);
        }

        private void Rescan(object aObject)
        {
            iScheduler.Schedule(()=>
			{
                if(iStackStarted)
                {
                    Helper.Helper.Rescan();
					iSharedPlaylists.Refresh();
					iLibrary.Refresh();
                }
            });
        }

        protected HttpClient iHttpClient;
        protected HttpServer iHttpServer;

        protected ContentDirectoryLocator iLocator;
        protected MediaProviderLibrary iLibrary;
        protected SharedPlaylists iSharedPlaylists;
     
        protected Mediator iMediator;

        private bool iStackStarted;
        private bool iLoaded;

        private SaveViewController.Saver iSaver;
		private Scheduler iScheduler;
    }
}