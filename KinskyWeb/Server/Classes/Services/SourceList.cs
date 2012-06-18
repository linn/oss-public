
using System;
using System.ServiceModel;
//using System.ServiceModel.Web;
using KinskyWeb.Comms;
using KinskyWeb.Kinsky;
namespace KinskyWeb.Services
{
    [ServiceContract]
    interface IServiceViewSourceList
    {
        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID, string aRoom);
        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        SourceDTO[] Sources(Guid aWidgetID);
    }

    [SelfHostingService(Interface = typeof(IServiceViewSourceList), Implementation = typeof(ServiceViewSourceList), Endpoint = "ViewSourceList")]
    public class ServiceViewSourceList : IServiceViewSourceList
    {

        #region IServiceViewSourceList Members

        public Guid Open(Guid aContainerID, string aRoom)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IViewKinskySourceList widget = WidgetFactory<IViewKinskySourceList>.GetDefault().Create();
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
            IViewKinskySourceList widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskySourceList;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }

        public SourceDTO[] Sources(Guid aWidgetID)
        {
            IViewKinskySourceList widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskySourceList;
            if (widget != null)
            {
                return widget.Sources();
            }
            else
            {
                return new SourceDTO[] { };
            }
        }

        #endregion
    }
}