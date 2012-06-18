using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Linq;
using Linn.Kinsky;
using KinskyWeb.Kinsky;
using Newtonsoft.Json.Linq;

namespace KinskyWeb.Services
{
    [DataContract]
    public class BrowserCurrentLocationResultDTO
    {
        [DataMember]
        public bool ErrorOccured { get; set; }
        [DataMember]
        public LocationDTO[] Location { get; set; }
    }
    
    [DataContract]
    public class BrowserChildCountResultDTO
    {
        [DataMember]
        public bool ErrorOccured { get; set; }
        [DataMember]
        public uint ChildCount { get; set; }
    }

    [DataContract]
    public class BrowserConnectedResultDTO
    {
        [DataMember]
        public bool ErrorOccured { get; set; }
        [DataMember]
        public bool Connected { get; set; }
    }

    [DataContract]
    public class BrowserGetChildrenResultDTO
    {
        [DataMember]
        public bool ErrorOccured { get; set; }
        [DataMember]
        public UPnpObjectDTO[] Children { get; set; }
    }

    [DataContract]
    public class UPnpObjectDTO
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string DidlLite { get; set; }
    }

    [DataContract]
    public class TransportStateDTO
    {
        [DataMember]
        public ETransportState TransportState { get; set; }
        [DataMember]
        public bool Connected{ get; set; }
    }

    [DataContract]
    public class VolumeStateDTO
    {
        [DataMember]
        public uint Volume { get; set; }
        [DataMember]
        public bool Mute { get; set; }
        [DataMember]
        public uint VolumeLimit { get; set; }
        [DataMember]
        public bool Connected { get; set; }
    }

    [DataContract]
    public class MediaTimeStateDTO
    {
        [DataMember]
        public ETransportState TransportState { get; set; }
        [DataMember]
        public uint Duration { get; set; }
        [DataMember]
        public uint Seconds { get; set; }
        [DataMember]
        public bool Connected { get; set; }
    }

    [DataContract]
    public class ContainerStateDTO
    {
        [DataMember]
        public bool TimedOut { get; set; }
        [DataMember]
        public Guid[] UpdatedWidgets { get; set; }
    }
    [DataContract]
    public class SourceDTO
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public ESourceType Type { get; set; }
    }
    [DataContract]
    public class TrackDTO
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Artist { get; set; }
        [DataMember]
        public string Album { get; set; }
        [DataMember]
        public uint Bitrate { get; set; }
        [DataMember]
        public float SampleRate { get; set; }
        [DataMember]
        public uint BitDepth { get; set; }
        [DataMember]
        public string Codec { get; set; }
        [DataMember]
        public bool Lossless { get; set; }
        [DataMember]
        public bool Connected { get; set; }
    }

    [DataContract]
    public class LocationDTO
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string BreadcrumbText { get; set; }
    }

    [DataContract]
    public class PlayModeStateDTO
    {
        [DataMember]
        public bool Shuffle { get; set; }
        [DataMember]
        public bool Repeat { get; set; }
        [DataMember]
        public bool Connected { get; set; }
    }
}