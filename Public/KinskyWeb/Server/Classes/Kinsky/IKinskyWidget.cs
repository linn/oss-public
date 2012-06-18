using System;
using System.Collections.Generic;
using Linn.Kinsky;
using Upnp;
using KinskyWeb.Services;
using System.Drawing;
namespace KinskyWeb.Kinsky
{
    public interface IKinskyWidget
    {
        Guid ID { get; set; }
        IKinskyContainer Container { get; set; }
        bool Updated { get; set; }
        void OnContainerTerminated();
    }

    public abstract class KinskyWidgetBase : IKinskyWidget
    {

        #region IKinskyWidget Members
        public virtual Guid ID { get; set; }
        public virtual IKinskyContainer Container { get; set; }
        public virtual bool Updated { get; set; }
        public abstract void OnContainerTerminated();
        #endregion
    }

    public enum ESourceType
    {
        eUnknown = 0,
        ePlaylist = 1,
        eRadio = 2,
        eUpnpAv = 3,
        eAnalog = 4,
        eSpdif = 5,
        eDisc = 6,
        eTuner = 7,
        eAux = 8,
        eToslink = 9
    }


    public interface IKinskyBrowser : IKinskyWidget, IKinskyContainerSource
    {
        void Subscribe(string[] aLocation);
        void Unsubscribe();
        LocationDTO[] CurrentLocation();
        bool Connected();
        uint ChildCount();
        UPnpObjectDTO[] GetChildren(uint aStartIndex, uint aCount);
        void Rescan();
        void Activate(UPnpObjectDTO aChild);
        void Up(uint aNumberLevels);
        bool ErrorOccured();
    }
    public interface IKinskyContainerSource : IKinskyWidget
    {
        IContainer CurrentContainer();
    }
    public interface IViewKinskyVolumeControl : IKinskyWidget
    {
        uint Volume();
        bool Mute();
        bool Connected();
        uint VolumeLimit();
        void Subscribe(string aRoom);
        void Unsubscribe();
    }
    public interface IControllerKinskyVolumeControl : IKinskyWidget
    {
        void SetVolume(uint aVolume);
        void SetMute(bool aMute);
        void Subscribe(string aRoom);
        void Unsubscribe();
    }
    public interface IViewKinskyTransportControl : IKinskyWidget
    {
        ETransportState TransportState();
        bool Connected();
        bool PlayNowEnabled();
        bool PlayNextEnabled();
        bool PlayLaterEnabled();
        void Subscribe(string aRoom, string aSource);
        void Unsubscribe();
    }
    public interface IControllerKinskyTransportControl : IKinskyWidget
    {
        void Pause();
        void Play();
        void Stop();
        void Previous();
        void Next();
        void PlayNow(IMediaRetriever aMediaRetriever);
        void PlayNext(IMediaRetriever aMediaRetriever);
        void PlayLater(IMediaRetriever aMediaRetriever);
        void Subscribe(string aRoom, string aSource);
        void Unsubscribe();
    }
    public interface IViewKinskyRoom : IKinskyWidget
    {
        void Subscribe(string aRoom);
        void Unsubscribe();
        string CurrentSource();
        ESourceType CurrentSourceType();
    }
    public interface IControllerKinskyRoom : IKinskyWidget
    {
        void Subscribe(string aRoom);
        void Unsubscribe();
        void SelectSource(string aSource);
        void Standby();
    }
    public interface IViewKinskySourceList : IKinskyWidget
    {
        void Subscribe(string aRoom);
        void Unsubscribe();
        SourceDTO[] Sources();
    }
    public interface IViewKinskyRoomList : IKinskyWidget
    {
        void Subscribe();
        void Unsubscribe();
        string[] Rooms();
    }
    public interface IViewKinskyMediaTime : IKinskyWidget
    {
        void Subscribe(string aRoom, string aSource);
        void Unsubscribe();
        ETransportState TransportState();
        uint Duration();
        uint Seconds();
        bool Connected();
    }
    public interface IControllerKinskyMediaTime : IKinskyWidget
    {
        void Subscribe(string aRoom, string aSource);
        void Unsubscribe();
        void SetSeconds(uint aSeconds);
    }

    public interface IViewKinskyTrack : IKinskyWidget
    {
        void Subscribe(string aRoom, string aSource);
        void Unsubscribe();
        Image Artwork();
        string Title();
        string Artist();
        string Album();
        uint Bitrate();
        float SampleRate();
        uint BitDepth();
        string Codec();
        bool Lossless();
        bool Connected();
    }


    public interface IViewKinskyPlaylist : IKinskyWidget
    {
        void Subscribe(string aRoom, string aSource);
        void Unsubscribe();
        UPnpObjectDTO CurrentItem();
    }
    public interface IControllerKinskyPlaylist : IKinskyWidget
    {
        void Subscribe(string aRoom, string aSource);
        void Unsubscribe();
        void Move(UPnpObjectDTO[] aChildren, UPnpObjectDTO aInsertAfterItem);
        void MoveToEnd(UPnpObjectDTO[] aChildren);
        void Insert(UPnpObjectDTO[] aChildren, UPnpObjectDTO aInsertAfterItem, IContainer aSourceContainer);
        void Delete(UPnpObjectDTO[] aChildren);
        void DeleteAll();
    }
    public interface IViewKinskyPlayMode : IKinskyWidget
    {
        void Subscribe(string aRoom, string aSource);
        void Unsubscribe();
        bool Shuffle();
        bool Repeat();
        bool Connected();
    }
    public interface IControllerKinskyPlayMode : IKinskyWidget
    {
        void Subscribe(string aRoom, string aSource);
        void Unsubscribe();
        void ToggleShuffle();
        void ToggleRepeat();
    }
    
    public interface IControllerKinskyDisc : IKinskyWidget
    {
        void Subscribe(string aRoom, string aSource);
        void Unsubscribe();
        void Eject();
    }
}
