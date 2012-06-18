
using System;
using System.ServiceModel;
//using System.ServiceModel.Web;
using KinskyWeb.Comms;
using KinskyWeb.Kinsky;
using Linn.Kinsky;
using Upnp;
using System.Collections.Generic;
namespace KinskyWeb.Services
{

    [ServiceContract]
    interface IServiceViewTransportControl
    {
        [OperationContract/*, WebInvoke*/]
        TransportStateDTO TransportState(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        bool Connected(Guid aWidgetID);
        
        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID, string aRoom, string aSource);
        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);

    }

    [ServiceContract]
    interface IServiceControllerTransportControl
    {
        [OperationContract/*, WebInvoke*/]
        void Pause(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        void Play(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        void Stop(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        void Previous(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        void Next(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        void PlayNow(Guid aWidgetID, Guid aContainerSourceID, UPnpObjectDTO[] aMedia);
        [OperationContract/*, WebInvoke*/]
        void PlayNext(Guid aWidgetID, Guid aContainerSourceID, UPnpObjectDTO[] aMedia);
        [OperationContract/*, WebInvoke*/]
        void PlayLater(Guid aWidgetID, Guid aContainerSourceID, UPnpObjectDTO[] aMedia);

        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID, string aRoom, string aSource);
        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);
    }

    [SelfHostingService(Interface = typeof(IServiceControllerTransportControl), Implementation = typeof(ServiceControllerTransportControl), Endpoint = "ControllerTransportControl")]
    public class ServiceControllerTransportControl : IServiceControllerTransportControl
    {

        #region IServiceControllerTransportControl Members

        public void Pause(Guid aWidgetID)
        {
            IControllerKinskyTransportControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyTransportControl;
            if (widget != null)
            {
                widget.Pause();
            }
        }

        public void Play(Guid aWidgetID)
        {
            IControllerKinskyTransportControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyTransportControl;
            if (widget != null)
            {
                widget.Play();
            }
        }

        public void Stop(Guid aWidgetID)
        {
            IControllerKinskyTransportControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyTransportControl;
            if (widget != null)
            {
                widget.Stop();
            }
        }

        public void Previous(Guid aWidgetID)
        {
            IControllerKinskyTransportControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyTransportControl;
            if (widget != null)
            {
                widget.Previous();
            }
        }

        public void Next(Guid aWidgetID)
        {
            IControllerKinskyTransportControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyTransportControl;
            if (widget != null)
            {
                widget.Next();
            }
        }

        public void PlayNow(Guid aWidgetID, Guid aContainerSourceID, UPnpObjectDTO[] aMedia)
        {
            IControllerKinskyTransportControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyTransportControl;
            IKinskyContainerSource containerSource = WidgetRegistry.GetDefault().Get(aContainerSourceID) as IKinskyContainerSource;
            IContainer sourceContainer = null;
            if (containerSource != null)
            {
                sourceContainer = containerSource.CurrentContainer();
            }
            if (widget != null && sourceContainer != null)
            {
                widget.PlayNow(CreateMediaRetriever(aMedia, sourceContainer));
            }
        }

        public void PlayNext(Guid aWidgetID, Guid aContainerSourceID, UPnpObjectDTO[] aMedia)
        {
            IControllerKinskyTransportControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyTransportControl;
            IKinskyContainerSource containerSource = WidgetRegistry.GetDefault().Get(aContainerSourceID) as IKinskyContainerSource;
            IContainer sourceContainer = null;
            if (containerSource != null)
            {
                sourceContainer = containerSource.CurrentContainer();
            }
            if (widget != null && sourceContainer != null)
            {
                widget.PlayNext(CreateMediaRetriever(aMedia, sourceContainer));
            }
        }

        public void PlayLater(Guid aWidgetID, Guid aContainerSourceID, UPnpObjectDTO[] aMedia)
        {
            IControllerKinskyTransportControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyTransportControl;
            IKinskyContainerSource containerSource = WidgetRegistry.GetDefault().Get(aContainerSourceID) as IKinskyContainerSource;
            IContainer sourceContainer = null;
            if (containerSource != null)
            {
                sourceContainer = containerSource.CurrentContainer();
            }
            if (widget != null && sourceContainer != null)
            {
                widget.PlayLater(CreateMediaRetriever(aMedia, sourceContainer));
            }
        }

        public Guid Open(Guid aContainerID, string aRoom, string aSource)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IControllerKinskyTransportControl widget = WidgetFactory<IControllerKinskyTransportControl>.GetDefault().Create();
                WidgetRegistry.GetDefault().Add(widget, aContainerID);
                widget.Subscribe(aRoom, aSource);
                return widget.ID;
            }
            else
            {
                return Guid.Empty;
            }
        }

        public void Close(Guid aWidgetID)
        {

            IControllerKinskyTransportControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyTransportControl;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }

        private IMediaRetriever CreateMediaRetriever(UPnpObjectDTO[] aMedia, IContainer aContainer)
        {
            List<upnpObject> playlist = new List<upnpObject>();
            foreach (UPnpObjectDTO child in aMedia)
            {
                playlist.Add(new DidlLite(child.DidlLite.ToString())[0]);
            }
            return new MediaRetriever(aContainer, playlist);
        }

        #endregion
    }

    [SelfHostingService(Interface = typeof(IServiceViewTransportControl), Implementation = typeof(ServiceViewTransportControl), Endpoint = "ViewTransportControl")]
    public class ServiceViewTransportControl : IServiceViewTransportControl
    {

        #region IServiceViewTransportControl Members

        public TransportStateDTO TransportState(Guid aWidgetID)
        {
            IViewKinskyTransportControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyTransportControl;
            TransportStateDTO stateDTO = new TransportStateDTO();
            if (widget != null)
            {
                stateDTO.TransportState = widget.TransportState();
                stateDTO.Connected = widget.Connected();
            }
            else
            {
                stateDTO.TransportState = ETransportState.eUnknown;
                stateDTO.Connected = false;
            }
            return stateDTO;
        }

        public bool Connected(Guid aWidgetID)
        {
            IViewKinskyTransportControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyTransportControl;
            if (widget != null)
            {
                return widget.Connected();
            }
            else
            {
                return false;
            }
        }

        public Guid Open(Guid aContainerID, string aRoom, string aSource)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IViewKinskyTransportControl widget = WidgetFactory<IViewKinskyTransportControl>.GetDefault().Create();
                WidgetRegistry.GetDefault().Add(widget, aContainerID);
                widget.Subscribe(aRoom, aSource);
                return widget.ID;
            }
            else
            {
                return Guid.Empty;
            }
        }

        public void Close(Guid aWidgetID)
        {
            IViewKinskyTransportControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyTransportControl;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }
        
        #endregion
    }
}