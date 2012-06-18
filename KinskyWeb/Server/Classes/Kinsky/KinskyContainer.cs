using Linn.Kinsky;
using System;
using Linn;

namespace KinskyWeb.Kinsky
{

    public enum EContentBrowserType
    {
        eMediaServer = 0,
        eMediaRenderer = 1
    }

    public class KinskyContainer : KinskyContainerBase, IKinskyContainer, IView
    {

        private Mediator iMediator;
        private IBrowser iBrowser;
        private IViewWidgetSelector iViewWidgetSelectorRoom;
        private IViewWidgetSelector iViewWidgetSelectorSource;
        private IViewWidgetVolumeControl iViewWidgetVolumeControl;
        private IViewWidgetMediaTime iViewWidgetMediaTime;
        private IViewWidgetTransportControl iViewWidgetTransportControl;
        private ViewWidgetBrowserAdaptor iViewWidgetBrowser;
        private ViewWidgetPlaylistAdaptor iViewWidgetPlaylist;
        private ViewWidgetPlaylistAdaptor iViewWidgetPlaylistRadio;
        private ViewWidgetPlaylistAdaptor iViewWidgetPlaylistAux;
        private ViewWidgetPlaylistAdaptor iViewWidgetPlaylistDiscPlayer;
        private IViewWidgetTrack iViewWidgetTrack;
        private IViewWidgetPlayMode iViewWidgetPlayMode;
        private IViewWidgetButton iViewWidgetButtonStandby;

        private IView iViewStub;

        public KinskyContainer()
            : base()
        {
            iBrowser = new Browser(new Location(KinskyStack.GetDefault().Locator.Root));

            iViewWidgetSelectorRoom = new ViewWidgetSelectorRoomAdaptor();
            iViewWidgetSelectorSource = new ViewWidgetSelectorSourceAdaptor();
            iViewWidgetVolumeControl = new ViewWidgetVolumeControlAdaptor();
            iViewWidgetMediaTime = new ViewWidgetMediaTimeAdaptor();
            iViewWidgetTransportControl = new ViewWidgetTransportControlAdaptor();
            iViewWidgetBrowser = new ViewWidgetBrowserAdaptor(iBrowser);
            iViewWidgetPlaylist = new ViewWidgetPlaylistAdaptor(ESourceType.ePlaylist);
            iViewWidgetPlaylistRadio = new ViewWidgetPlaylistAdaptor(ESourceType.eRadio);
            iViewWidgetPlaylistAux = new ViewWidgetPlaylistAdaptor(ESourceType.eUnknown);
            iViewWidgetPlaylistDiscPlayer = new ViewWidgetPlaylistAdaptor(ESourceType.eDisc);
            iViewWidgetTrack = new ViewWidgetTrackAdaptor();
            iViewWidgetPlayMode = new ViewWidgetPlayModeAdaptor();
            iViewWidgetButtonStandby = new ViewWidgetStandbyAdaptor();

            iViewStub = new ViewStub();
            PlaySupport support = new PlaySupport();

            Linn.Kinsky.Model model = new Linn.Kinsky.Model(this, support);
            iMediator = new Mediator(KinskyStack.GetDefault().Helper, model);
        }

        public override void Subscribe()
        {
            base.Subscribe();
            Trace.WriteLine(Trace.kKinskyWeb, "Container subscribe");
            iMediator.Open();
            Trace.WriteLine(Trace.kKinskyWeb, "Mediator opened");
            iBrowser.Refresh();
            Trace.WriteLine(Trace.kKinskyWeb, "Browser refreshed");
        }

        public override void Unsubscribe()
        {
            base.Unsubscribe();
            Trace.WriteLine(Trace.kKinskyWeb, "Container unsubscribe");
            iMediator.Close();
            Trace.WriteLine(Trace.kKinskyWeb, "Mediator closed");
            iViewWidgetBrowser.Close();
            Trace.WriteLine(Trace.kKinskyWeb, "Browser closed");
        }

        public IViewWidgetBrowseableAdaptor GetContentBrowser(EContentBrowserType aBrowserType, ESourceType aSourceType)
        {
            if (aBrowserType == EContentBrowserType.eMediaServer)
            {
                return iViewWidgetBrowser;
            }
            else
            {
                switch (aSourceType)
                {
                    case ESourceType.ePlaylist:
                    case ESourceType.eUpnpAv:
                        return iViewWidgetPlaylist;
                    case ESourceType.eRadio:
                        return iViewWidgetPlaylistRadio;
                    case ESourceType.eDisc:
                        return iViewWidgetPlaylistDiscPlayer;
                    default:
                        return iViewWidgetPlaylistAux;
                }
            }
        }

        public IViewWidgetPlaylistAdaptor GetPlaylist(ESourceType aSourceType)
        {
            switch (aSourceType)
            {
                case ESourceType.ePlaylist:
                    return iViewWidgetPlaylist;
                case ESourceType.eRadio:
                    return iViewWidgetPlaylistRadio;
                case ESourceType.eDisc:
                    return iViewWidgetPlaylistDiscPlayer;
                default:
                    return iViewWidgetPlaylistAux;
            }
        }

        #region IView Members

        public IViewWidgetSelector ViewWidgetSelectorRoom
        {
            get { return iViewWidgetSelectorRoom; }
        }

        public IViewWidgetButton ViewWidgetButtonStandby
        {
            get { return iViewWidgetButtonStandby; }
        }

        public IViewWidgetSelector ViewWidgetSelectorSource
        {
            get { return iViewWidgetSelectorSource; }
        }

        public IViewWidgetVolumeControl ViewWidgetVolumeControl
        {
            get { return iViewWidgetVolumeControl; }
        }

        public IViewWidgetMediaTime ViewWidgetMediaTime
        {
            get { return iViewWidgetMediaTime; }
        }

        public IViewWidgetTransportControl ViewWidgetTransportControlMediaRenderer
        {
            get { return iViewWidgetTransportControl; }
        }

        public IViewWidgetTransportControl ViewWidgetTransportControlDiscPlayer
        {
            get { return iViewWidgetTransportControl; }
        }

        public IViewWidgetTransportControl ViewWidgetTransportControlRadio
        {
            get { return iViewWidgetTransportControl; }
        }

        public IViewWidgetTrack ViewWidgetTrack
        {
            get { return iViewWidgetTrack; }
        }

        public IViewWidgetPlayMode ViewWidgetPlayMode
        {
            get { return iViewWidgetPlayMode; }
        }

        public IViewWidgetPlaylist ViewWidgetPlaylist
        {
            get { return iViewWidgetPlaylist; }
        }

        public IViewWidgetPlaylistRadio ViewWidgetPlaylistRadio
        {
            get { return iViewWidgetPlaylistRadio; }
        }

        public IViewWidgetPlaylistAux ViewWidgetPlaylistAux
        {
            get { return iViewWidgetPlaylistAux; }
        }

        public IViewWidgetPlaylistDiscPlayer ViewWidgetPlaylistDiscPlayer
        {
            get { return iViewWidgetPlaylistDiscPlayer; }
        }

        public IViewWidgetContent ViewWidgetBrowser
        {
            get { return iViewWidgetBrowser; }
        }

        public IViewWidgetButton ViewWidgetButtonSave
        {
            get { return iViewStub.ViewWidgetButtonSave; }
        }

        public IViewWidgetButton ViewWidgetButtonWasteBin
        {
            get { return iViewStub.ViewWidgetButtonWasteBin; }
        }

        public override void OnContainerTerminated()
        {
            Unsubscribe();
        }

        #endregion
    }

}