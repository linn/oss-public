using System;
using System.ServiceModel;
//using System.ServiceModel.Web;
using KinskyWeb.Comms;
using KinskyWeb.Kinsky;

namespace KinskyWeb.Services
{
    [ServiceContract]
    interface IServiceViewVolumeControl
    {
        [OperationContract/*, WebInvoke*/]
        uint Volume(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        bool Mute(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        uint VolumeLimit(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        bool Connected(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID, string aRoom);
        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        VolumeStateDTO GetState(Guid aWidgetID);
    }

    [ServiceContract]
    interface IServiceControllerVolumeControl
    {
        [OperationContract/*, WebInvoke*/]
        void SetVolume(Guid aWidgetID, uint aVolume);
        [OperationContract/*, WebInvoke*/]
        void SetMute(Guid aWidgetID, bool aMute);
        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID, string aRoom);
        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);
    }

    [SelfHostingService(Interface = typeof(IServiceViewVolumeControl), Implementation = typeof(ServiceViewVolumeControl), Endpoint = "ViewVolumeControl")]
    public class ServiceViewVolumeControl : IServiceViewVolumeControl
    {

        #region IServiceViewVolumeControl Members

        public uint Volume(Guid aWidgetID)
        {
            IViewKinskyVolumeControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyVolumeControl;
            if (widget != null)
            {
                return widget.Volume();
            }
            else
            {
                return 0;
            }
        }

        public bool Mute(Guid aWidgetID)
        {
            IViewKinskyVolumeControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyVolumeControl;
            if (widget != null)
            {
                return widget.Mute();
            }
            else
            {
                return true;
            }
        }

        public uint VolumeLimit(Guid aWidgetID)
        {
            IViewKinskyVolumeControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyVolumeControl;
            if (widget != null)
            {
                return widget.VolumeLimit();
            }
            else
            {
                return 0;
            }
        }

        public bool Connected(Guid aWidgetID)
        {
            IViewKinskyVolumeControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyVolumeControl;
            if (widget != null)
            {
                return widget.Connected();
            }
            else
            {
                return false;
            }
        }

        public Guid Open(Guid aContainerID, string aRoom)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IViewKinskyVolumeControl widget = WidgetFactory<IViewKinskyVolumeControl>.GetDefault().Create();
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
            IViewKinskyVolumeControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyVolumeControl;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }

        public VolumeStateDTO GetState(Guid aWidgetID)
        {
            IViewKinskyVolumeControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyVolumeControl;
            VolumeStateDTO result = new VolumeStateDTO();
            if (widget != null)
            {
                result.Volume = widget.Volume();
                result.Mute = widget.Mute();
                result.VolumeLimit = widget.VolumeLimit();
                result.Connected = widget.Connected();
            }
            return result;
        }

        #endregion
    }

    [SelfHostingService(Interface = typeof(IServiceControllerVolumeControl), Implementation = typeof(ServiceControllerVolumeControl), Endpoint = "ControllerVolumeControl")]
    public class ServiceControllerVolumeControl : IServiceControllerVolumeControl
    {
        #region IServiceControllerVolumeControl Members

        public void SetVolume(Guid aWidgetID, uint aVolume)
        {
            IControllerKinskyVolumeControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyVolumeControl;
            if (widget != null)
            {
                widget.SetVolume(aVolume);
            }
        }

        public void SetMute(Guid aWidgetID, bool aMute)
        {
            IControllerKinskyVolumeControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyVolumeControl;
            if (widget != null)
            {
                widget.SetMute(aMute);
            }
        }

        public Guid Open(Guid aContainerID, string aRoom)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IControllerKinskyVolumeControl widget = WidgetFactory<IControllerKinskyVolumeControl>.GetDefault().Create();
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
            IControllerKinskyVolumeControl widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IControllerKinskyVolumeControl;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }

        #endregion
    }
}
