
using System;
using System.ServiceModel;
//using System.ServiceModel.Web;
using KinskyWeb.Comms;
using System.IO;
using KinskyWeb.Kinsky;
using System.Drawing;
using System.Drawing.Imaging;
using KinskyWeb.Helpers.Extensions;

namespace KinskyWeb.Services
{
    [ServiceContract]
    interface IServiceViewTrack
    {
        [OperationContract/*, WebInvoke*/]
        Guid Open(Guid aContainerID, string aRoom, string aSource);
        [OperationContract/*, WebInvoke*/]
        void Close(Guid aWidgetID);
        /* TODO: Put this back in once mono wcf stack is stable
        [OperationContract, WebInvoke(Method = "GET")]
        Stream Artwork(Guid aWidgetID);
         * */
        [OperationContract/*, WebInvoke*/]
        string Title(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        string Artist(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        string Album(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        uint Bitrate(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        float SampleRate(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        uint BitDepth(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        string Codec(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        bool Lossless(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        bool Connected(Guid aWidgetID);
        [OperationContract/*, WebInvoke*/]
        TrackDTO GetState(Guid aWidgetID);
    }

    [SelfHostingService(Interface = typeof(IServiceViewTrack), Implementation = typeof(ServiceViewTrack), Endpoint = "ViewTrack")]
    class ServiceViewTrack : IServiceViewTrack
    {

        #region IServiceViewTrack Members

        public Guid Open(Guid aContainerID, string aRoom, string aSource)
        {
            IKinskyContainer container = WidgetRegistry.GetDefault().Get(aContainerID) as IKinskyContainer;
            if (container != null)
            {
                IViewKinskyTrack widget = WidgetFactory<IViewKinskyTrack>.GetDefault().Create();
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
            IViewKinskyTrack widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyTrack;
            if (widget != null)
            {
                widget.Unsubscribe();
                WidgetRegistry.GetDefault().Remove(aWidgetID);
            }
        }
        /*
        public Stream Artwork(Guid aWidgetID)
        {
            IViewKinskyTrack widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyTrack;
            Image artwork = null;
            if (widget != null)
            {
                artwork = widget.Artwork();
            }
            if (artwork == null){
                artwork = Linn.Kinsky.Properties.Resources.NoAlbumArt;
            }
            Stream resource = artwork.GetStream(ImageFormat.Png);
            WebOperationContext.Current.OutgoingResponse.ContentType = "image/png";
            WebOperationContext.Current.OutgoingResponse.Headers["Cache-Control"] = "no-cache";
            return resource;
        }
        */
        public bool Connected(Guid aWidgetID)
        {
            IViewKinskyTrack widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyTrack;
            if (widget != null)
            {
                return widget.Connected();
            }
            else
            {
                return false;
            }
        }
        public string Title(Guid aWidgetID)
        {
            IViewKinskyTrack widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyTrack;
            if (widget != null)
            {
                return widget.Title();
            }
            else
            {
                return String.Empty;
            }
        }

        public string Artist(Guid aWidgetID)
        {
            IViewKinskyTrack widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyTrack;
            if (widget != null)
            {
                return widget.Artist();
            }
            else
            {
                return String.Empty;
            }
        }

        public string Album(Guid aWidgetID)
        {
            IViewKinskyTrack widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyTrack;
            if (widget != null)
            {
                return widget.Album();
            }
            else
            {
                return String.Empty;
            }
        }

        public uint Bitrate(Guid aWidgetID)
        {
            IViewKinskyTrack widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyTrack;
            if (widget != null)
            {
                return widget.Bitrate();
            }
            else
            {
                return 0;
            }
        }

        public float SampleRate(Guid aWidgetID)
        {
            IViewKinskyTrack widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyTrack;
            if (widget != null)
            {
                return widget.SampleRate();
            }
            else
            {
                return 0;
            }
        }

        public uint BitDepth(Guid aWidgetID)
        {
            IViewKinskyTrack widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyTrack;
            if (widget != null)
            {
                return widget.BitDepth();
            }
            else
            {
                return 0;
            }
        }

        public string Codec(Guid aWidgetID)
        {

            IViewKinskyTrack widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyTrack;
            if (widget != null)
            {
                return widget.Codec();
            }
            else
            {
                return string.Empty;
            }
        }

        public bool Lossless(Guid aWidgetID)
        {

            IViewKinskyTrack widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyTrack;
            if (widget != null)
            {
                return widget.Lossless();
            }
            else
            {
                return false;
            }
        }

        public TrackDTO GetState(Guid aWidgetID)
        {
            IViewKinskyTrack widget = WidgetRegistry.GetDefault().Get(aWidgetID) as IViewKinskyTrack;
            TrackDTO dto = new TrackDTO();
            if (widget != null)
            {
                dto.Title = widget.Title();
                dto.Artist = widget.Artist();
                dto.Album = widget.Album();
                dto.Bitrate = widget.Bitrate();
                dto.SampleRate = widget.SampleRate();
                dto.BitDepth = widget.BitDepth();
                dto.Codec = widget.Codec();
                dto.Lossless = widget.Lossless();
                dto.Connected = widget.Connected();
            }
            return dto;
        }

        #endregion
    }
}