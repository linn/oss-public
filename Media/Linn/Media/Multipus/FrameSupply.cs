using System;
using System.Net;
using System.Diagnostics;

namespace Linn.Media.Multipus
{
    public class FrameSupplyUdp : IFrameReader
    {
        public FrameSupplyUdp(IReader aReader)
        {
            iReader = aReader;
            iStopwatch = new Stopwatch();
            iStopwatch.Start();
            kVersion = new Version(1, 0);
        }

        public Frame Read()
        {
            try
            {
                uint mpus = Parse4();
                long ticks = iStopwatch.ElapsedTicks;

                if (mpus != kMpus)
                {
                    throw new InvalidFrameNotMpus(ticks);
                }

                uint major = Parse1();
                uint minor = Parse1();
                Version version = new Version(major, minor);
                if (!kVersion.IsCompatible(version))
                {
                    throw new InvalidFrameUnsupportedVersion(ticks, version);
                }

                uint headerBytes = Parse2();
                uint uid = Parse4();
                uint frameNumber = Parse4();
                uint sampleRate = Parse4();
                uint bitDepth = Parse1();
                uint bitStorage = Parse1();
                uint channels = Parse1();
                uint reserved = Parse1();
                uint samples = Parse4();

                if (headerBytes > kHeaderBytes)
                {
                    //Strip remainder of unknown use header bytes from new version.
                    iReader.Read((int)(headerBytes - kHeaderBytes));
                }

                byte[] data = iReader.Read((int)(bitStorage / 8 * channels * samples));
                Frame frame = new Frame(ticks, true, version, uid, frameNumber, sampleRate, bitDepth, bitStorage, channels, samples, data);
                iReader.ReadFlush();
                return frame;
            }
            catch (InvalidFrameNotMpus aNotMpus)
            {
                iReader.ReadFlush();
                return new Frame(aNotMpus.Ns, false, new Version(0, 0), 0, 0, 0, 0, 0, 0, 0, null);
            }
            catch (InvalidFrameUnsupportedVersion aBadVersion)
            {
                iReader.ReadFlush();
                return new Frame(aBadVersion.Ns, true, aBadVersion.Version, 0, 0, 0, 0, 0, 0, 0, null);
            }
        }

        private uint Parse1()
        {
            byte[] data = iReader.Read(1);
            return data[0];
        }

        private uint Parse2()
        {
            byte[] data = iReader.Read(2);
            short value = BitConverter.ToInt16(data, 0);
            return (uint)IPAddress.NetworkToHostOrder(value);
        }

        private uint Parse4()
        {
            byte[] data = iReader.Read(4);
            int value = BitConverter.ToInt32(data, 0);
            return (uint)IPAddress.NetworkToHostOrder(value);
        }

        private IReader iReader;
        private Stopwatch iStopwatch;

        private const uint kMpus = 0x4d707573; //Binary for "Mpus"
        private readonly Version kVersion;
        private const uint kHeaderBytes = 28;
    }
}