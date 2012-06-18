using System.Collections.Generic;
namespace Linn.Topology
{

    public interface IModelFactory
    {
        IModelVolumeControl CreateModelVolumeControl(IPreamp aPreamp);
        IModelSourceReceiver CreateModelSourceReceiver(ISource aSource);
        IModelSourceDiscPlayer CreateModelSourceDiscPlayer(ISource aSource);
        IModelSourceMediaRenderer CreateModelSourceMediaRenderer(ISource aSource);
        IModelSourceRadio CreateModelSourceRadio(ISource aSource);
        IModelInfo CreateModelInfo(ISource aSource);
        IModelTime CreateModelTime(ISource aSource);
    }

    public class ModelFactory : IModelFactory
    {
        public const string kSourceUpnpAv = "UpnpAv";
        private Dictionary<string, ModelSource> iModelSourceCache;
        private Dictionary<string, IModelInfo> iModelInfoCache;
        private Dictionary<string, IModelTime> iModelTimeCache;

        public ModelFactory()
        {
            iModelSourceCache = new Dictionary<string, ModelSource>();
            iModelInfoCache = new Dictionary<string, IModelInfo>();
            iModelTimeCache = new Dictionary<string, IModelTime>();
        }

        private ModelSource CheckModelSourceCache(Source aSource)
        {
            string id = string.Format("{0},{1}", aSource.Device, aSource);
            ModelSource result = null;
            if (iModelSourceCache.TryGetValue(id, out result))
            {
                if (result.Source.Device != aSource.Device)
                {
                    iModelSourceCache.Remove(id);
                    result = null;
                }
            }
            return result;
        }

        private void AddToCache(ModelSource aModelSource, Source aSource)
        {
            string id = string.Format("{0},{1}", aSource.Device, aSource);
            if (iModelSourceCache.ContainsKey(id))
            {
                iModelSourceCache[id] = aModelSource;
            }
            else
            {
                iModelSourceCache.Add(id, aModelSource);
            }
        }
        
        private IModelInfo CheckModelInfoCache(Source aSource)
        {
            string id = string.Format("{0},{1}", aSource.Device, aSource);
            IModelInfo result = null;
            if (iModelInfoCache.TryGetValue(id, out result))
            {
                if (result.Device != aSource.Device)
                {
                    iModelInfoCache.Remove(id);
                    result = null;
                }
            }
            return result;
        }

        private void AddToCache(IModelInfo aModelInfo, Source aSource)
        {
            string id = string.Format("{0},{1}", aSource.Device, aSource);
            if (iModelInfoCache.ContainsKey(id))
            {
                iModelInfoCache[id] = aModelInfo;
            }
            else
            {
                iModelInfoCache.Add(id, aModelInfo);
            }
        }
        
        private IModelTime CheckModelTimeCache(Source aSource)
        {
            string id = string.Format("{0},{1}", aSource.Device, aSource);
            IModelTime result = null;
            if (iModelTimeCache.TryGetValue(id, out result))
            {
                if (result.Device != aSource.Device)
                {
                    iModelTimeCache.Remove(id);
                    result = null;
                }
            }
            return result;
        }

        private void AddToCache(IModelTime aModelTime, Source aSource)
        {
            string id = string.Format("{0},{1}", aSource.Device, aSource);
            if (iModelTimeCache.ContainsKey(id))
            {
                iModelTimeCache[id] = aModelTime;
            }
            else
            {
                iModelTimeCache.Add(id, aModelTime);
            }
        }
        

        public IModelVolumeControl CreateModelVolumeControl(IPreamp aPreamp)
        {
            if (aPreamp != null && aPreamp is Preamp)
            {
                if (aPreamp.Type == "Preamp")
                {
                    return (new ModelVolumeControlPreamp(aPreamp as Preamp));
                }
                if (aPreamp.Type == "UpnpAv")
                {
                    return (new ModelVolumeControlUpnpAv(aPreamp as Preamp));
                }
            }

            return (null);
        }

        public IModelSourceReceiver CreateModelSourceReceiver(ISource aSource)
        {
            if (aSource != null && aSource is Source)
            {
                Source source = aSource as Source;
                ModelSourceReceiver result = CheckModelSourceCache(source) as ModelSourceReceiver;
                if (result == null)
                {
                    result = new ModelSourceReceiver(source);
                    AddToCache(result, source);
                }
                return result;
            }
            return null;
        }

        public IModelSourceDiscPlayer CreateModelSourceDiscPlayer(ISource aSource)
        {
            if (aSource != null && aSource is Source)
            {
                Source source = aSource as Source;
                ModelSourceDiscPlayer result = CheckModelSourceCache(source) as ModelSourceDiscPlayer;
                if (result == null)
                {
                    result = new ModelSourceDiscPlayerSdp(source);
                    AddToCache(result, source);
                }
                return result;
            }
            return null;
        }

        public IModelSourceMediaRenderer CreateModelSourceMediaRenderer(ISource aSource)
        {
            if (aSource != null && aSource is Source)
            {
                Source source = aSource as Source;
                ModelSourceMediaRenderer result = CheckModelSourceCache(source) as ModelSourceMediaRenderer;
                if (result == null)
                {
                    if (aSource.Type == kSourceUpnpAv)
                    {
                        result = new ModelSourceMediaRendererUpnpAv(source);
                    }
                    else
                    {
                        result = new ModelSourceMediaRendererDs(source);
                    }
                    AddToCache(result, source);
                }
                return result;
            }
            return null;
        }
        public IModelSourceRadio CreateModelSourceRadio(ISource aSource)
        {
            if (aSource != null && aSource is Source)
            {
                Source source = aSource as Source;
                ModelSourceRadio result = CheckModelSourceCache(source) as ModelSourceRadio;
                if (result == null)
                {
                    result = new ModelSourceRadio(source);
                    AddToCache(result, source);
                }
                return result;
            }
            return null;
        }

        public IModelInfo CreateModelInfo(ISource aSource)
        {            
            if (aSource != null && aSource is Source)
            {
                Source source = aSource as Source;
                if (aSource.Type == kSourceUpnpAv)
                {
                    ModelSourceMediaRendererUpnpAv result = CheckModelSourceCache(source) as ModelSourceMediaRendererUpnpAv;
                    if (result == null)
                    {
                        result = new ModelSourceMediaRendererUpnpAv(source);
                        AddToCache((result as ModelSource), source);
                    }
                    return result;
                }
                else
                {
                    IModelInfo result = CheckModelInfoCache(source);
                    if (result == null)
                    {
                        result = new ModelInfo(source);
                        AddToCache(result, source);
                    }
                    return result;
                }
            }
            return null;
        }

        public IModelTime CreateModelTime(ISource aSource)
        {
            if (aSource != null && aSource is Source)
            {
                Source source = aSource as Source;
                if (aSource.Type == kSourceUpnpAv)
                {
                    ModelSourceMediaRendererUpnpAv result = CheckModelSourceCache(source) as ModelSourceMediaRendererUpnpAv;
                    if (result == null)
                    {
                        result = new ModelSourceMediaRendererUpnpAv(source);
                        AddToCache((result as ModelSource), source);
                    }
                    return result;
                }
                else
                {
                    IModelTime result = CheckModelTimeCache(source);
                    if (result == null)
                    {
                        result = new ModelTime(source);
                        AddToCache(result, source);
                    }
                    return result;
                }
            }
            return null;
        }
    }

}