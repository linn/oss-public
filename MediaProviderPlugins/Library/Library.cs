using System;
using System.Net;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using Upnp;

using Linn;
using Linn.Topology;
using Linn.ControlPoint;
using Linn.Kinsky;

[assembly: ContentDirectoryFactoryType("OssKinskyMppLibrary.MediaProviderLibraryFactory")]

namespace OssKinskyMppLibrary
{
    public class MediaProviderLibraryFactory : IContentDirectoryFactory
    {
        public IContentDirectory Create(IContentDirectorySupportV1 aSupport)
        {
            return new MediaProviderLibrary(aSupport);
        }
    }

    public class MediaProviderLibrary : IContentDirectory, IContainer
    {
        public MediaProviderLibrary(IContentDirectorySupportV1 aSupport)
        {
            iSupport = aSupport;

            iMutex = new Mutex(false);

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);

            iMetadata = new container();
            iMetadata.Id = kLibraryId;
            iMetadata.Title = Name;
            iMetadata.WriteStatus = "PROTECTED";
            iMetadata.Restricted = true;
            iMetadata.Searchable = true;
            iMetadata.AlbumArtUri.Add(iSupport.VirtualFileSystem.Uri(Path.Combine(path, "Library.png")));

            iMediaServers = new Dictionary<MediaServer, ModelMediaServer>();
            iSortedMediaServers = new SortedList<string, ModelMediaServer>();

            iLibrary = new Library(iSupport.SsdpNotifyProvider);
        }

        public void Start()
        {
            Trace.WriteLine(Trace.kMediaServer, "MediaProviderLibrary.Start: Using interface " + iSupport.Interface);

            iLibrary.EventMediaServerAdded += MediaServerAdded;
            iLibrary.EventMediaServerRemoved += MediaServerRemoved;

            iLibrary.Start(iSupport.Interface);
        }

        public void Stop()
        {
            iLibrary.Stop();

            iLibrary.EventMediaServerAdded -= MediaServerAdded;
            iLibrary.EventMediaServerRemoved -= MediaServerRemoved;

            iMediaServers.Clear();
            iSortedMediaServers.Clear();
        }

        public string Name
        {
            get
            {
                return "Library";
            }
        }

        public string Company
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length != 0)
                {
                    return ((AssemblyCompanyAttribute)attributes[0]).Company;
                }
                else
                {
                    return "";
                }
            }
        }

        public string Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public IContainer Root
        {
            get
            {
                return this;
            }
        }

        public IOptionPage OptionPage
        {
            get
            {
                return null;
            }
        }

        public uint Open()
        {
            iMutex.WaitOne();

            uint count = (uint)iMediaServers.Count;

            iMutex.ReleaseMutex();

            return count;
        }

        public void Refresh()
        {
            iLibrary.Rescan();
        }

        public IContainer ChildContainer(container aContainer)
        {
            iMutex.WaitOne();

            foreach (ModelMediaServer ms in iMediaServers.Values)
            {
                if (ms.Metadata.Id == aContainer.Id)
                {
                    iMutex.ReleaseMutex();

                    return new ContainerMediaServer(ms, aContainer);
                }
            }

            iMutex.ReleaseMutex();

            return null;
        }

        public container Metadata
        {
            get
            {
                return iMetadata;
            }
        }
		
		public bool HandleMove(DidlLite aDidlLite)
		{
			return false;
		}

        public bool HandleInsert(DidlLite aDidlLite)
        {
            return false;
        }

        public void Insert(string aAfterId, DidlLite aDidlLite)
        {
            throw new NotSupportedException();
        }
		
		public bool HandleDelete(DidlLite aDidlLite)
		{
			return false;
		}

        public void Delete(string aId)
        {
            throw new NotSupportedException();
        }

        public bool HandleRename(upnpObject aObject)
        {
            return false;
        }

        public void Rename(string aId, string aTitle)
        {
            throw new NotSupportedException();
        }

        public DidlLite Items(uint aStartIndex, uint aCount)
        {
            iMutex.WaitOne();

            DidlLite didl = new DidlLite();
            for (int i = (int)aStartIndex;
                i < aStartIndex + aCount && i < iSortedMediaServers.Count;
                i++)
            {
                didl.Add(iSortedMediaServers.Values[i].Metadata);
            }

            iMutex.ReleaseMutex();

            return didl;
        }

        public DidlLite Search(string aSearchCriterea, uint aStartIndex, uint aCount)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgs> EventContentUpdated;
        public event EventHandler<EventArgs> EventContentAdded;
        public event EventHandler<EventArgsContentRemoved> EventContentRemoved;

        private void MediaServerAdded(object sender, Library.EventArgsMediaServer e)
        {
            try
            {
                ModelMediaServer mediaServer = new ModelMediaServer(e.MediaServer, iSupport.EventUpnpProvider);

                iMutex.WaitOne();
                if (iSortedMediaServers.ContainsKey(e.MediaServer.Name))
                {
                    MediaServer matched = null;
                    foreach (MediaServer m in iMediaServers.Keys)
                    {
                        if (m.Name.Equals(e.MediaServer.Name))
                        {
                            matched = m;
                            break;
                        }
                    }
                    if (matched != null)
                    {
                        iMediaServers.Remove(matched);
                    }
                    iSortedMediaServers.Remove(e.MediaServer.Name);
                }
                iMediaServers.Add(e.MediaServer, mediaServer);
                iSortedMediaServers.Add(e.MediaServer.Name, mediaServer);
                Assert.Check(iMediaServers.Keys.Count == iSortedMediaServers.Count);
                iMutex.ReleaseMutex();

                if (EventContentAdded != null)
                {
                    EventContentAdded(this, EventArgs.Empty);
                }
            }
            catch (ServiceException) { }
        }

        private void MediaServerRemoved(object sender, Library.EventArgsMediaServer e)
        {
            iMutex.WaitOne();

            ModelMediaServer ms;
            if (iMediaServers.TryGetValue(e.MediaServer, out ms))
            {
                iMediaServers.Remove(e.MediaServer);
                iSortedMediaServers.Remove(e.MediaServer.Name);
                Assert.Check(iMediaServers.Keys.Count == iSortedMediaServers.Count);
                iMutex.ReleaseMutex();

                if (EventContentRemoved != null)
                {
                    EventContentRemoved(this, new EventArgsContentRemoved(ms.Metadata.Id));
                }
            }
            else
            {
                iMutex.ReleaseMutex();
            }
        }

        private const string kLibraryId = "Library";
        private const uint kCountPerCall = 100;

        private Mutex iMutex;
        private IContentDirectorySupportV1 iSupport;

        private container iMetadata;

        private Library iLibrary;
        private SortedList<string, ModelMediaServer> iSortedMediaServers;
        private Dictionary<MediaServer, ModelMediaServer> iMediaServers;
    }

    internal class ContainerMediaServer : IContainer
    {
        public ContainerMediaServer(ModelMediaServer aMediaServer, container aContainer)
        {
            iUpdateId = 0;
            iMetadata = aContainer;
            iMediaServer = aMediaServer;

            iMutex = new Mutex(false);
        }

        public uint Open()
        {
            iMutex.WaitOne();

            DidlLite didl;
            uint count;
            uint total;

            iMediaServer.Browse(Id, 0, 1, out didl, out count, out total, out iUpdateId);

            iMutex.ReleaseMutex();

            return total;
        }

        public void Refresh() { }

        public IContainer ChildContainer(container aContainer)
        {
            return new ContainerMediaServer(iMediaServer, aContainer);
        }

        public container Metadata
        {
            get
            {
                return iMetadata;
            }
        }
		
		public bool HandleMove(DidlLite aDidlLite)
		{
			return false;
		}
		
		public bool HandleInsert(DidlLite aDidlLite)
        {
            return false;
        }

        public void Insert(string aAfterId, DidlLite aDidlLite)
        {
            throw new NotSupportedException();
        }
        
		public bool HandleDelete(DidlLite aDidlLite)
		{
			return false;
		}
		
		public void Delete(string aId)
        {
            throw new NotSupportedException();
        }

        public bool HandleRename(upnpObject aObject)
        {
            return false;
        }

        public void Rename(string aId, string aTitle)
        {
            throw new NotSupportedException();
        }

        public DidlLite Items(uint aStartIndex, uint aCount)
        {
            iMutex.WaitOne();

            try
            {
                DidlLite didl;
                uint count;
                uint total;

                bool changed = false;
                uint updateId = iUpdateId;

                iMediaServer.Browse(Id, aStartIndex, aCount, out didl, out count, out total, out iUpdateId);

                if (updateId != iUpdateId)
                {
                    changed = true;
                }

                iMutex.ReleaseMutex();

                if (changed)
                {
                    if (EventContentUpdated != null)
                    {
                        EventContentUpdated(this, EventArgs.Empty);
                    }
                }

                return didl;
            }
            catch (ServiceException e)
            {
                Console.WriteLine("ContainerMediaServer.Items: " + e.Message);

                if (e.Code == 801) // Access denied
                {
                    iMutex.ReleaseMutex();

                    DidlLite result = new DidlLite();
                    item item = new item();
                    item.Title = "Access denied";
                    result.Add(item);

                    return result;
                }
                else
                {
                    throw e;
                }
            }

            iMutex.ReleaseMutex();

            return new DidlLite();
        }

        public DidlLite Search(string aSearchCriterea, uint aStartIndex, uint aCount)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<EventArgs> EventContentUpdated;
        public event EventHandler<EventArgs> EventContentAdded;
        public event EventHandler<EventArgsContentRemoved> EventContentRemoved;

        private string Id
        {
            get
            {
                string id = kRootId;
                if (iMetadata.Id != iMediaServer.Udn)
                {
                    id = iMetadata.Id;
                }
                return id;
            }
        }

        private const int kErrorAccessDenied = 801;
        private const string kRootId = "0";

        private Mutex iMutex;

        private uint iUpdateId;
        private container iMetadata;
        private ModelMediaServer iMediaServer;
    }
} // OssKinskyMppLibrary

