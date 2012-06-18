using System;
using System.Collections.Generic;
using System.ServiceModel;
//using System.ServiceModel.Web;
using System.Xml.Linq;
using KinskyWeb.Comms;
using KinskyWeb.Helpers.Extensions;
using KinskyWeb.Kinsky;
using Upnp;
using Linn.Kinsky;

namespace KinskyWeb.Services
{
    [ServiceContract]
    interface IServiceViewPlaylist
    {
        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID, string aRoom, string aSource);

        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);        

        [OperationContract/*, WebInvoke*/]
        UPnpObjectDTO CurrentItem(Guid aWidgetID);

    }
    [ServiceContract]
    interface IServiceControllerPlaylist
    {
        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID, string aRoom, string aSource);

        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);        

        [OperationContract/*, WebInvoke*/]
        void Move(Guid aWidgetID, UPnpObjectDTO[] aChildren, UPnpObjectDTO aInsertAfterItem);

        [OperationContract/*, WebInvoke*/]
        void MoveToEnd(Guid aWidgetID, UPnpObjectDTO[] aChildren);

        [OperationContract/*, WebInvoke*/]
        void Insert(Guid aWidgetID, Guid aContainerSourceID, UPnpObjectDTO[] aChildren, UPnpObjectDTO aInsertAfterItem);

        [OperationContract/*, WebInvoke*/]
        void Delete(Guid aWidgetID, UPnpObjectDTO[] aChildren);

        [OperationContract/*, WebInvoke*/]
        void DeleteAll(Guid aWidgetID);

    }

    [SelfHostingService(Interface = typeof(IServiceViewPlaylist), Implementation = typeof(ServiceViewPlaylist), Endpoint = "ViewPlaylist")]
    public class ServiceViewPlaylist : IServiceViewPlaylist
    {

        public Guid Open(Guid aContainerID, string aRoom, string aSource)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IViewKinskyPlaylist widget = WidgetFactory<IViewKinskyPlaylist>.GetDefault().Create();
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
            IViewKinskyPlaylist widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyPlaylist;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }

        public UPnpObjectDTO CurrentItem(Guid aWidgetID)
        {
            IViewKinskyPlaylist widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyPlaylist;
            if (widget != null)
            {
                return widget.CurrentItem();
            }
            else
            {
                return new UPnpObjectDTO();
            }
        }

    }


    [SelfHostingService(Interface = typeof(IServiceControllerPlaylist), Implementation = typeof(ServiceControllerPlaylist), Endpoint = "ControllerPlaylist")]
    public class ServiceControllerPlaylist : IServiceControllerPlaylist
    {

        public Guid Open(Guid aContainerID, string aRoom, string aSource)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IControllerKinskyPlaylist widget = WidgetFactory<IControllerKinskyPlaylist>.GetDefault().Create();
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
            IControllerKinskyPlaylist widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyPlaylist;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }


        public void Move(Guid aWidgetID, UPnpObjectDTO[] aChildren, UPnpObjectDTO aInsertAfterItem)
        {
            IControllerKinskyPlaylist widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyPlaylist;
            if (widget != null)
            {
                widget.Move(aChildren, aInsertAfterItem);
            }
        }
        
        public void MoveToEnd(Guid aWidgetID, UPnpObjectDTO[] aChildren)
        {
            IControllerKinskyPlaylist widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyPlaylist;
            if (widget != null)
            {
                widget.MoveToEnd(aChildren);
            }
        }

        public void Insert(Guid aWidgetID, Guid aContainerSourceID, UPnpObjectDTO[] aChildren, UPnpObjectDTO aInsertAfterItem)
        {
            IControllerKinskyPlaylist widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyPlaylist;
            IKinskyContainerSource containerSource = WidgetRegistry.GetDefault().Get(aContainerSourceID) as IKinskyContainerSource;
            IContainer sourceContainer = null;
            if (containerSource != null)
            {
                sourceContainer = containerSource.CurrentContainer();
            }

            if (widget != null)
            {
                widget.Insert(aChildren, aInsertAfterItem, sourceContainer);
            }
        }

        public void Delete(Guid aWidgetID, UPnpObjectDTO[] aChildren)
        {
            IControllerKinskyPlaylist widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyPlaylist;
            if (widget != null)
            {
                widget.Delete(aChildren);
            }
        }

        public void DeleteAll(Guid aWidgetID)
        {
            IControllerKinskyPlaylist widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyPlaylist;
            if (widget != null)
            {
                widget.DeleteAll();
            }
        }
}
}
