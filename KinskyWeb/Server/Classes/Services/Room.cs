
using System;
using System.ServiceModel;
//using System.ServiceModel.Web;
using KinskyWeb.Comms;
using KinskyWeb.Kinsky;
namespace KinskyWeb.Services
{
    [ServiceContract]
    interface IServiceViewRoom
    {
        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID, string aRoom);
        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        SourceDTO CurrentSource(Guid aWidgetID);
    }

    [ServiceContract]
    interface IServiceControllerRoom
    {
        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID, string aRoom);
        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        void SelectSource(Guid aWidgetID, string aSource);
        [OperationContract/*, WebInvoke*/]
        void Standby(Guid aWidgetID);
    }

    [SelfHostingService(Interface = typeof(IServiceViewRoom), Implementation = typeof(ServiceViewRoom), Endpoint = "ViewRoom")]
    class ServiceViewRoom : IServiceViewRoom
    {

        #region IServiceViewRoom Members

        public Guid Open(Guid aContainerID, string aRoom)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IViewKinskyRoom widget = WidgetFactory<IViewKinskyRoom>.GetDefault().Create();
                WidgetRegistry.GetDefault().Add(widget, aContainerID);
                widget.Subscribe(aRoom);
                return widget.ID;
            }
            else
            {
                return Guid.Empty;
            }
        }

        public void Close(Guid aWidgetID)
        {
            IViewKinskyRoom widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyRoom;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }

        public SourceDTO CurrentSource(Guid aWidgetID)
        {
            IViewKinskyRoom widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyRoom;
            if (widget != null)
            {
                SourceDTO result = new SourceDTO();
                result.Name = widget.CurrentSource();
                result.Type = widget.CurrentSourceType();
                return result;
            }
            else
            {
                return new SourceDTO();
            }
        }

        #endregion
    }


    [SelfHostingService(Interface = typeof(IServiceControllerRoom), Implementation = typeof(ServiceControllerRoom), Endpoint = "ControllerRoom")]
    class ServiceControllerRoom : IServiceControllerRoom
    {

        #region IServiceControllerRoom Members

        public Guid Open(Guid aContainerID, string aRoom)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IControllerKinskyRoom widget = WidgetFactory<IControllerKinskyRoom>.GetDefault().Create();
                WidgetRegistry.GetDefault().Add(widget, aContainerID);
                widget.Subscribe(aRoom);
                return widget.ID;
            }
            else
            {
                return Guid.Empty;
            }
        }

        public void Close(Guid aWidgetID)
        {
            IControllerKinskyRoom widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyRoom;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }

        public void SelectSource(Guid aWidgetID, string aSource)
        {
            IControllerKinskyRoom widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyRoom;
            if (widget != null)
            {
                widget.SelectSource(aSource);
            }
        }

        public void Standby(Guid aWidgetID)
        {
            IControllerKinskyRoom widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyRoom;
            if (widget != null)
            {
                widget.Standby();
            }
        }
        #endregion
    }
}