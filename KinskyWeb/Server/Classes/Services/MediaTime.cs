
using System;
using System.ServiceModel;
//using System.ServiceModel.Web;
using KinskyWeb.Comms;
using KinskyWeb.Kinsky;
using Linn.Kinsky;
namespace KinskyWeb.Services
{
    [ServiceContract]
    interface IServiceViewMediaTime
    {
        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID, string aRoom, string aSource);
        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        TransportStateDTO TransportState(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        uint Duration(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        uint Seconds(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        MediaTimeStateDTO GetState(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        bool Connected(Guid aWidgetID);
    }

    [ServiceContract]
    interface IServiceControllerMediaTime
    {
        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID, string aRoom, string aSource);
        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        void SetSeconds(Guid aWidgetID, uint aSeconds);
    }

    [SelfHostingService(Interface = typeof(IServiceViewMediaTime), Implementation = typeof(ServiceViewMediaTime), Endpoint = "ViewMediaTime")]
    public class ServiceViewMediaTime : IServiceViewMediaTime
    {
        #region IServiceViewMediaTime Members

        public Guid Open(Guid aContainerID, string aRoom, string aSource)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IViewKinskyMediaTime widget = WidgetFactory<IViewKinskyMediaTime>.GetDefault().Create();
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
            IViewKinskyMediaTime widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyMediaTime;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }

        public TransportStateDTO TransportState(Guid aWidgetID)
        {
            IViewKinskyMediaTime widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyMediaTime;
            TransportStateDTO transportStateDTO = new TransportStateDTO();
            if (widget != null)
            {
                ETransportState state = widget.TransportState();
                transportStateDTO.TransportState = state;
            }
            else
            {
                transportStateDTO.TransportState = ETransportState.eUnknown;
            }
            return transportStateDTO;
        }

        public uint Duration(Guid aWidgetID)
        {
            IViewKinskyMediaTime widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyMediaTime;
            if (widget != null)
            {
                return widget.Duration();
            }
            else
            {
                return 0;
            }
        }

        public uint Seconds(Guid aWidgetID)
        {
            IViewKinskyMediaTime widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyMediaTime;
            if (widget != null)
            {
                return widget.Seconds();
            }
            else
            {
                return 0;
            }
        }
        public bool Connected(Guid aWidgetID)
        {
            IViewKinskyMediaTime widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyMediaTime;
            if (widget != null)
            {
                return widget.Connected();
            }
            else
            {
                return false;
            }
        }
        public MediaTimeStateDTO GetState(Guid aWidgetID)
        {
            IViewKinskyMediaTime widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyMediaTime;
            MediaTimeStateDTO result = new MediaTimeStateDTO();
            if (widget != null)
            {
                result.Seconds = widget.Seconds();
                result.Duration = widget.Duration();
                result.TransportState = widget.TransportState();
                result.Connected = widget.Connected();
            }
            return result;
        }

        #endregion
    }

    [SelfHostingService(Interface = typeof(IServiceControllerMediaTime), Implementation = typeof(ServiceControllerMediaTime), Endpoint = "ControllerMediaTime")]
    public class ServiceControllerMediaTime : IServiceControllerMediaTime
    {
        #region IServiceControllerMediaTime Members

        public Guid Open(Guid aContainerID, string aRoom, string aSource)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IControllerKinskyMediaTime widget = WidgetFactory<IControllerKinskyMediaTime>.GetDefault().Create();
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
            IControllerKinskyMediaTime widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyMediaTime;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }

        public void SetSeconds(Guid aWidgetID, uint aSeconds)
        {
            IControllerKinskyMediaTime widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyMediaTime;
            if (widget != null)
            {
                widget.SetSeconds(aSeconds);
            }
        }

        #endregion
    }
}