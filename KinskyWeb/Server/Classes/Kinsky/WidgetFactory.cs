using KinskyWeb.TestStubs;
namespace KinskyWeb.Kinsky
{

    public static class WidgetFactory<T> where T : class, IKinskyWidget
    {
        public static IWidgetFactory<T> GetDefault()
        {
            //return new StubWidgetFactory<T>();
            return new KinskyWidgetFactory<T>();
        }
    }

    public interface IWidgetFactory<T> where T : class, IKinskyWidget
    {
        T Create();
    }


    public class KinskyWidgetFactory<T> : IWidgetFactory<T> where T : class, IKinskyWidget
    {
        
        public T Create()
        {
            if (typeof(T) == typeof(IKinskyContainer))
            {
                return new KinskyContainer() as T;
            }
            if (typeof(T) == typeof(IViewKinskyRoomList))
            {
                return new ViewKinskyRoomList() as T;
            }
            if (typeof(T) == typeof(IViewKinskySourceList))
            {
                return new ViewKinskySourceList() as T;
            }
            if (typeof(T) == typeof(IViewKinskyRoom))
            {
                return new ViewKinskyRoom() as T;
            }
            if (typeof(T) == typeof(IControllerKinskyRoom))
            {
                return new ControllerKinskyRoom() as T;
            }
            if (typeof(T) == typeof(IViewKinskyVolumeControl))
            {
                return new ViewVolumeControl() as T;
            }
            if (typeof(T) == typeof(IControllerKinskyVolumeControl))
            {
                return new ControllerVolumeControl() as T;
            }
            if (typeof(T) == typeof(IViewKinskyMediaTime))
            {
                return new ViewMediaTime() as T;
            }
            if (typeof(T) == typeof(IControllerKinskyMediaTime))
            {
                return new ControllerMediaTime() as T;
            }
            if (typeof(T) == typeof(IViewKinskyTransportControl))
            {
                return new ViewTransportControl() as T;
            }
            if (typeof(T) == typeof(IControllerKinskyTransportControl))
            {
                return new ControllerTransportControl() as T;
            }
            if (typeof(T) == typeof(IKinskyBrowser))
            {
                return new KinskyBrowser() as T;
            }
            if (typeof(T) == typeof(IViewKinskyTrack))
            {
                return new ViewTrack() as T;
            }
            if (typeof(T) == typeof(IViewKinskyPlaylist))
            {
                return new ViewPlaylist() as T;
            }
            if (typeof(T) == typeof(IControllerKinskyPlaylist))
            {
                return new ControllerPlaylist() as T;
            }
            if (typeof(T) == typeof(IViewKinskyPlayMode))
            {
                return new ViewPlayMode() as T;
            }
            if (typeof(T) == typeof(IControllerKinskyPlayMode))
            {
                return new ControllerPlayMode() as T;
            }
            
            return null;
        }
    }

}