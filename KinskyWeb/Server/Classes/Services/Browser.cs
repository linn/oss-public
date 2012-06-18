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
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace KinskyWeb.Services
{
    [ServiceContract]
    interface IServiceBrowser
    {
        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID, string[] aLocation);

        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);

        [OperationContract/*, WebInvoke*/]
        BrowserConnectedResultDTO Connected(Guid aWidgetID);

        [OperationContract/*, WebInvoke*/]
        BrowserChildCountResultDTO ChildCount(Guid aWidgetID);

        [OperationContract/*, WebInvoke*/]
        BrowserGetChildrenResultDTO GetChildren(Guid aWidgetID, uint aStartIndex, uint aCount);

        [OperationContract/*, WebInvoke*/]
        BrowserCurrentLocationResultDTO CurrentLocation(Guid aWidgetID);

        [OperationContract/*, WebInvoke*/]
        void Rescan(Guid aWidgetID);

        [OperationContract/*, WebInvoke*/]
        void Activate(Guid aWidgetID, UPnpObjectDTO aChild);

        [OperationContract/*, WebInvoke*/]
        void Up(Guid aWidgetID, uint aNumberLevels);

    }



    [SelfHostingService(Interface = typeof(IServiceBrowser), Implementation = typeof(ServiceBrowser), Endpoint = "Browser")]
    public class ServiceBrowser : IServiceBrowser
    {

        #region IServiceBrowser Members


        public Guid Open(Guid aContainerID, string[] aLocation)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IKinskyBrowser widget = WidgetFactory<IKinskyBrowser>.GetDefault().Create();
                WidgetRegistry.GetDefault().Add(widget, aContainerID);
                widget.Subscribe(aLocation);
                return widget.ID;
            }
            else
            {
                return Guid.Empty;
            }
        }

        public void Close(Guid aWidgetID)
        {
            IKinskyBrowser widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IKinskyBrowser;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }

        public BrowserCurrentLocationResultDTO CurrentLocation(Guid aWidgetID)
        {
            IKinskyBrowser widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IKinskyBrowser;
            if (widget != null)
            {
                return new BrowserCurrentLocationResultDTO() { ErrorOccured = false, Location = widget.CurrentLocation() };
            }
            else
            {
                return new BrowserCurrentLocationResultDTO() { ErrorOccured = true };
            }
        }

        public BrowserConnectedResultDTO Connected(Guid aWidgetID)
        {
            IKinskyBrowser widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IKinskyBrowser;
            if (widget != null)
            {
                return new BrowserConnectedResultDTO() { ErrorOccured = false, Connected = widget.Connected() };
            }
            else
            {
                return new BrowserConnectedResultDTO() {ErrorOccured=true};
            }
        }

        public BrowserChildCountResultDTO ChildCount(Guid aWidgetID)
        {
            IKinskyBrowser widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IKinskyBrowser;
            if (widget != null)
            {
                return new BrowserChildCountResultDTO() {ErrorOccured=false, ChildCount=widget.ChildCount()};
            }
            else
            {
                return new BrowserChildCountResultDTO (){ErrorOccured =true};
            }
        }

        public BrowserGetChildrenResultDTO GetChildren(Guid aWidgetID, uint aStartIndex, uint aCount)
        {
            IKinskyBrowser widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IKinskyBrowser;
            if (widget != null && !widget.ErrorOccured())
            {
                return new BrowserGetChildrenResultDTO() { ErrorOccured = false, Children = widget.GetChildren(aStartIndex, aCount) };
            }
            else
            {
                return new BrowserGetChildrenResultDTO() { ErrorOccured = true };
            }
        }


        public void Rescan(Guid aWidgetID)
        {
            IKinskyBrowser widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IKinskyBrowser;
            if (widget != null)
            {
                widget.Rescan();
            }
        }

        public void Activate(Guid aWidgetID, UPnpObjectDTO aChild)
        {
            IKinskyBrowser widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IKinskyBrowser;
            if (widget != null)
            {
                widget.Activate(aChild);
            }
        }

        public void Up(Guid aWidgetID, uint aNumberLevels)
        {
            IKinskyBrowser widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IKinskyBrowser;
            if (widget != null)
            {
                widget.Up(aNumberLevels);
            }
        }
        #endregion

    }

}
