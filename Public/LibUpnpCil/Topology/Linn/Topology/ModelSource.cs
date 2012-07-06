using System;
using System.Collections.Generic;

using Upnp;

using Linn.ControlPoint.Upnp;

namespace Linn.Topology
{
    public class ModelSourceException : Exception
    {
        public ModelSourceException(int aCode, string aDescription)
            : base("Code=" + aCode.ToString() + " Description=\"" + aDescription + "\"")
        {
            Code = aCode;
            Description = aDescription;
        }

        public int Code;
        public string Description;
    }

    public class MrItem
    {
        public MrItem(uint aId, string aUri, DidlLite aDidlLite)
        {
            iId = aId;
            iUri = aUri;
            iDidlLite = aDidlLite;
        }

        public uint Id
        {
            get
            {
                return iId;
            }
        }

        public string Uri
        {
            get
            {
                return iUri;
            }
        }

        // TODO: change this member to be a upnpObject
        public DidlLite DidlLite
        {
            get
            {
                return iDidlLite;
            }
        }

        public static DidlLite ToDidlLite(IList<MrItem> aList)
        {
            DidlLite didl = new DidlLite();
            foreach (MrItem i in aList)
            {
                didl.AddRange(i.DidlLite);
            }
            return didl;
        }

        private uint iId;
        private string iUri;
        private DidlLite iDidlLite;
    }

    public interface IMediaSupported
    {
        resource BestSupportedResource(upnpObject aObject);
    }
    
    public interface IModelSource
    {
        void Close();
        string Name { get; }
        void Open();
        Linn.Topology.Source Source { get; }
    }

    public abstract class ModelSource : IModelSource
    {
        public ModelSource()
        {
            iMimeTypeAliases = new Dictionary<string, IList<string>>();
            iMimeTypeAliases.Add("audio/x-m4a", new List<string>(new string[] { "audio/mp4" }));
            iMimeTypeAliases.Add("audio/mp4", new List<string>(new string[] { "audio/x-m4a" }));
            iMimeTypeAliases.Add("audio/x-flac", new List<string>(new string[] { "audio/flac" }));
            iMimeTypeAliases.Add("audio/flac", new List<string>(new string[] { "audio/x-flac" }));
        }

        public abstract void Open();
        public abstract void Close();

        public abstract string Name { get; }

        public abstract Source Source { get; }

        protected resource BestSupportedResource(string aProtocolInfo, upnpObject aObject)
        {
            List<resource> supportedList = new List<resource>();

            string supportedFormats = string.Empty;
            for (int i = 0; i < aObject.Res.Count; ++i)
            {
                bool supported = false;
                resource r = aObject.Res[i];
                if (r.ProtocolInfo != null)
                {
                    string[] split = r.ProtocolInfo.Split(':');
                    supportedFormats += (i > 0) ? ";" + r.ProtocolInfo : r.ProtocolInfo;
                    if (split.Length == 4)
                    {
                        // check the protocol info for the resource appears in the list of supported protocol infos
                        string protocol = split[0];
                        string mimetype = split[2];
                        if (aProtocolInfo.Contains(string.Format("{0}:*:*:", protocol)))    // renderer support all types of protocol
                        {
                            supportedList.Add(r);
                            supported = true;
                        }
                        else if (aProtocolInfo.Contains(string.Format("{0}:*:{1}:", protocol, mimetype)))
                        {
                            supportedList.Add(r);
                            supported = true;
                        }
                        else if (aProtocolInfo.Contains(protocol) && mimetype == "*")
                        {
                            supportedList.Add(r);
                            supported = true;
                        }
                        else if (protocol == "*" && mimetype == "*")
                        {
                            supportedList.Add(r);
                            supported = true;
                        }
                        else
                        {
                            IList<string> aliases;
                            if (iMimeTypeAliases.TryGetValue(mimetype, out aliases))
                            {
                                foreach (string a in aliases)
                                {
                                    if (aProtocolInfo.Contains(string.Format("{0}:*:{1}:", protocol, a)))
                                    {
                                        supportedList.Add(r);
                                        supported = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!supported)
                        {
                            Trace.WriteLine(Trace.kTopology, r.ProtocolInfo + " is not supported (" + aObject.Title + ")");
                        }
                    }
                }
                else
                {
                    supportedList.Add(r);
                    supported = true;
                }
            }

            if (supportedList.Count > 0)
            {
                // NOTE: we should do some sorting to pick out the best quality resource, for now assume the original stream format will be the zeroth res element
                return supportedList[0];
            }
            else
            {
                UserLog.WriteLine("No supported formats found for " + aObject.Title);
                UserLog.WriteLine("Supported formats are " + aProtocolInfo + ", item supports " + supportedFormats);
            }

            return null;
        }

        private Dictionary<string, IList<string>> iMimeTypeAliases;
    }
} // Linn.Topology
