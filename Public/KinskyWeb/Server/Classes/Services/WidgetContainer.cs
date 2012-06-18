
using System;
using System.Collections.Generic;
using System.ServiceModel;
//using System.ServiceModel.Web;
using KinskyWeb.Comms;
using KinskyWeb.Kinsky;
namespace KinskyWeb.Services
{
    [ServiceContract]
    interface IServiceWidgetContainer
    {
        [OperationContract/*, WebInvoke*/]
        Guid Open(uint aRequestedTimeoutMilliseconds);
        [OperationContract/*, WebInvoke*/]
        void Close(Guid aContainerID);
        [OperationContract/*, WebInvoke*/]
        ContainerStateDTO UpdateState(Guid aContainerID);
    }

    [SelfHostingService(Interface=typeof(IServiceWidgetContainer), Implementation=typeof(ServiceWidgetContainer), Endpoint="WidgetContainer")]
    class ServiceWidgetContainer : IServiceWidgetContainer
    {
        #region IControlPointService Members

        public Guid Open(uint aRequestedTimeoutMilliseconds)
        {
            IKinskyContainer proxy = WidgetFactory<IKinskyContainer>.GetDefault().Create();
            proxy.RequestedTimeoutMilliseconds = aRequestedTimeoutMilliseconds;
            Guid id = WidgetRegistry.GetDefault().Add(proxy);
            proxy.Subscribe();
            return id;
        }

        public void Close(Guid aContainerID)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                container.Unsubscribe();
            }
            WidgetRegistry.GetDefault().Remove(aContainerID);
        }

        public ContainerStateDTO UpdateState(Guid aContainerID)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            ContainerStateDTO result = new ContainerStateDTO();
            List<Guid> updated = new List<Guid>();
            result.TimedOut = (container == null);
            if (container != null)
            {
                container.LastActivity = DateTime.Now;
                foreach (IKinskyWidget widget in container.Widgets())
                {
                    if (widget.Updated)
                    {
                        updated.Add(widget.ID);
                        widget.Updated = false;
                    }
                }
            }
            result.UpdatedWidgets = updated.ToArray();
            return result;
        }

        #endregion
    }
}