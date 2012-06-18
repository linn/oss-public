
using System;
using System.ServiceModel;
//using System.ServiceModel.Web;
using KinskyWeb.Comms;
using KinskyWeb.Kinsky;
namespace KinskyWeb.Services
{
    [ServiceContract]
    interface IServiceViewRoomList
    {
        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID);
        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        string[] Rooms(Guid aWidgetID);
    }

    [SelfHostingService(Interface = typeof(IServiceViewRoomList), Implementation = typeof(ServiceViewRoomList), Endpoint = "ViewRoomList")]
    public class ServiceViewRoomList : IServiceViewRoomList
    {

        #region IServiceViewRoomList Members

        public Guid Open(Guid aContainerID)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IViewKinskyRoomList widget = WidgetFactory<IViewKinskyRoomList>.GetDefault().Create();
                WidgetRegistry.GetDefault().Add(widget, aContainerID);
                widget.Subscribe();
                return widget.ID;
            }
            else
            {
                return Guid.Empty;
            }
        }

        public void Close(Guid aWidgetID)
        {
            IViewKinskyRoomList widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyRoomList;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }

        public string[] Rooms(Guid aWidgetID)
        {
            IViewKinskyRoomList widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyRoomList;
            if (widget != null)
            {
                return widget.Rooms();
            }
            else
            {
                return new string[] { };
            }
        }

        #endregion
    }
}