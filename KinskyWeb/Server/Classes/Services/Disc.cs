using System;
using System.Collections.Generic;
using System.ServiceModel;
//using System.ServiceModel.Web;
using System.Xml.Linq;
using KinskyWeb.Comms;
using KinskyWeb.Helpers.Extensions;
using KinskyWeb.Kinsky;
using Upnp;

namespace KinskyWeb.Services
{
    [ServiceContract]
    interface IServiceControllerDisc
    {
        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID, string aRoom, string aSource);

        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);

        [OperationContract/*, WebInvoke*/]
        void Eject(Guid aWidgetID);

    }

    [SelfHostingService(Interface = typeof(IServiceControllerDisc), Implementation = typeof(ServiceControllerDisc), Endpoint = "Playlist")]
    public class ServiceControllerDisc : IServiceControllerDisc
    {

        #region IServiceControllerDisc Members


        public Guid Open(Guid aContainerID, string aRoom, string aSource)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IControllerKinskyDisc widget = WidgetFactory<IControllerKinskyDisc>.GetDefault().Create();
                WidgetRegistry.GetDefault().Add(widget, aContainerID);
                //widget.Subscribe(aLocation);
                return widget.ID;
            }
            else
            {
                return Guid.Empty;
            }
        }

        public void Close(Guid aWidgetID)
        {
            IControllerKinskyDisc widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyDisc;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }


        public void Eject(Guid aWidgetID)
        {
            IControllerKinskyDisc widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyDisc;
            if (widget != null)
            {
                widget.Eject();
            }
        }

        #endregion
    }
}
