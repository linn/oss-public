using System;
using System.Net;
using System.Diagnostics;

namespace Linn.Media.Multipus
{
    public class InvalidFrame : Exception
    {
    }

    public class InvalidFrameNotMpus : InvalidFrame
    {
        public InvalidFrameNotMpus(long aNs)
        {
            iNs = aNs;
        }

        public long Ns
        {
            get {return iNs;}
        }

        private long iNs;
    }

    public class InvalidFrameUnsupportedVersion : InvalidFrame
    {
        public InvalidFrameUnsupportedVersion(long aNs, Version aVersion)
        {
            iNs = aNs;
            iVersion = aVersion;
        }

        public long Ns
        {
            get { return iNs; }
        }

        public Version Version
        {
            get { return iVersion; }
        }

        private long iNs;
        private Version iVersion;
    }

    [Serializable]
    public class Version
    {
        public Version(uint aMajor, uint aMinor)
        {
            iMajor = aMajor;
            iMinor = aMinor;
        }

        public bool IsCompatible(Version aVersion)
        {
            if (iMajor == aVersion.iMajor)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        private uint iMajor;
        private uint iMinor;
    }

    [Serializable]
    public class Frame
    {
        public Frame(long aTimestampNs, bool aIsMpus, Version aVersion, uint aUid, uint aFrameNumber, uint aSampleRate, uint aBitDepth, uint aBitStorage, uint aChannels, uint aSamples, byte[] aData)
        {
            iTimestampNs = aTimestampNs;
            iIsMpus = aIsMpus;
            iVersion = aVersion;
            iUid = aUid;
            iFrameNumber = aFrameNumber;
            iSampleRate = aSampleRate;
            iBitDepth = aBitDepth;
            iBitStorage = aBitStorage;
            iChannels = aChannels;
            iSamples = aSamples;
            iData = aData;
        }

        public long TimeStampNs
        {
            get { return iTimestampNs; }
        }

        public bool IsMpus
        {
            get { return iIsMpus; }
        }

        public Version Version
        {
            get { return iVersion; }
        }

        public uint Uid
        {
            get { return iUid; }
        }

        public uint FrameNumber
        {
            get { return iFrameNumber; }
        }

        public uint SampleRate
        {
            get { return iSampleRate; }
        }

        public uint BitDepth
        {
            get { return iBitDepth; }
        }

        public uint BitStorage
        {
            get { return iBitStorage; }
        }

        public uint Channels
        {
            get { return iChannels; }
        }

        public uint Samples
        {
            get { return iSamples; }
        }

        public byte[] Data
        {
            get { return iData; }
        }

        private long iTimestampNs;
        private bool iIsMpus;
        private Version iVersion;
        private uint iUid;
        private uint iFrameNumber;
        private uint iSampleRate;
        private uint iBitDepth;
        private uint iBitStorage;
        private uint iChannels;
        private uint iSamples;
        private byte[] iData;
    }

    public interface IFrameReader
    {
        Frame Read();
    }


}