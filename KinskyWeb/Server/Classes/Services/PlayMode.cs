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
    interface IServiceViewPlayMode
    {
        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID, string aRoom, string aSource);

        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);

        [OperationContract/*, WebInvoke*/]
        bool Shuffle(Guid aWidgetID);

        [OperationContract/*, WebInvoke*/]
        bool Repeat(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        bool Connected(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        PlayModeStateDTO GetState(Guid aWidgetID);

    }
    [ServiceContract]
    interface IServiceControllerPlayMode
    {
        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID, string aRoom, string aSource);

        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);

        [OperationContract/*, WebInvoke*/]
        void ToggleShuffle(Guid aWidgetID);

        [OperationContract/*, WebInvoke*/]
        void ToggleRepeat(Guid aWidgetID);
    }

    [SelfHostingService(Interface = typeof(IServiceViewPlayMode), Implementation = typeof(ServiceViewPlayMode), Endpoint = "ViewPlayMode")]
    public class ServiceViewPlayMode : IServiceViewPlayMode
    {

        public Guid Open(Guid aContainerID, string aRoom, string aSource)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IViewKinskyPlayMode widget = WidgetFactory<IViewKinskyPlayMode>.GetDefault().Create();
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
            IViewKinskyPlayMode widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyPlayMode;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }

        public bool Shuffle(Guid aWidgetID)
        {
            IViewKinskyPlayMode widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyPlayMode;
            if (widget != null)
            {
                return widget.Shuffle();
            }
            else
            {
                return false;
            }
        }

        public bool Repeat(Guid aWidgetID)
        {
            IViewKinskyPlayMode widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyPlayMode;
            if (widget != null)
            {
                return widget.Repeat();
            }
            else
            {
                return false;
            }
        }
        public bool Connected(Guid aWidgetID)
        {
            IViewKinskyPlayMode widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyPlayMode;
            if (widget != null)
            {
                return widget.Connected();
            }
            else
            {
                return false;
            }
        }

        public PlayModeStateDTO GetState(Guid aWidgetID)
        {
            PlayModeStateDTO dto = new PlayModeStateDTO();
            IViewKinskyPlayMode widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyPlayMode;
            if (widget != null)
            {
                dto.Repeat = widget.Repeat();
                dto.Shuffle = widget.Shuffle();
                dto.Connected = widget.Connected();
            }
            return dto;
        }

    }


    [SelfHostingService(Interface = typeof(IServiceControllerPlayMode), Implementation = typeof(ServiceControllerPlayMode), Endpoint = "ControllerPlayMode")]
    public class ServiceControllerPlayMode : IServiceControllerPlayMode
    {

        public Guid Open(Guid aContainerID, string aRoom, string aSource)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IControllerKinskyPlayMode widget = WidgetFactory<IControllerKinskyPlayMode>.GetDefault().Create();
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
            IControllerKinskyPlayMode widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyPlayMode;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }


        public void ToggleShuffle(Guid aWidgetID)
        {
            IControllerKinskyPlayMode widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyPlayMode;
            if (widget != null)
            {
                widget.ToggleShuffle();
            }
        }

        public void ToggleRepeat(Guid aWidgetID)
        {
            IControllerKinskyPlayMode widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyPlayMode;
            if (widget != null)
            {
                widget.ToggleRepeat();
            }
        }
    }
}
