using System;
using System.Text;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Linn.Control;
using Linn.Control.Ssdp;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;

namespace Linn.ProductSupport.Flash
{
    public interface IFlash
    {
        uint Id { get; }
        uint SectorCount { get; }
        uint SectorBytes { get; }
    }

    internal class Flash : IFlash
    {
        public Flash(uint aId, uint aSectorCount, uint aSectorBytes)
        {
            iId = aId;
            iSectorCount = aSectorCount;
            iSectorBytes = aSectorBytes;
        }

        public uint Id
        {
            get
            {
                return (iId);
            }
        }

        public uint SectorCount
        {
            get
            {
                return (iSectorCount);
            }
        }

        public uint SectorBytes
        {
            get
            {
                return (iSectorBytes);
            }
        }

        private uint iId;
        private uint iSectorCount;
        private uint iSectorBytes;
    }

    public class RomDirInfo
    {
        private const int kMaxRomDirInfoSeconds = 10;

        public RomDirInfo(IConsole aConsole, ServiceFlash aServiceFlash)
        {
            iConsole = aConsole;
            iServiceFlash = aServiceFlash;

            iActionRomDirInfo = iServiceFlash.CreateAsyncActionRomDirInfo();
            iActionRomDirInfo.EventResponse += RomDirInfoResponse;
            iRomDirInfoComplete = new ManualResetEvent(false);

            iRomDirInfoCollected = false;
        }

        public void ReadRomDirInfo()
        {
            if (!iRomDirInfoCollected)
            {
                iRomDirInfoComplete.Reset();
                iActionRomDirInfo.RomDirInfoBegin();
                WaitFor(iRomDirInfoComplete, kMaxRomDirInfoSeconds);
            }

            ReportSuccess();
        }

        public uint RomDirFallbackFlashId
        {
            get
            {
                Assert.Check(iRomDirInfoCollected);
                return (iRomDirFallbackFlashId);
            }
        }

        public uint RomDirFallbackOffset
        {
            get
            {
                Assert.Check(iRomDirInfoCollected);
                return (iRomDirFallbackOffset);
            }
        }

        public uint RomDirFallbackBytes
        {
            get
            {
                Assert.Check(iRomDirInfoCollected);
                return (iRomDirFallbackBytes);
            }
        }

        public uint RomDirMainFlashId
        {
            get
            {
                Assert.Check(iRomDirInfoCollected);
                return (iRomDirMainFlashId);
            }
        }

        public uint RomDirMainOffset
        {
            get
            {
                Assert.Check(iRomDirInfoCollected);
                return (iRomDirMainOffset);
            }
        }

        public uint RomDirMainBytes
        {
            get
            {
                Assert.Check(iRomDirInfoCollected);
                return (iRomDirMainBytes);
            }
        }

        private void WaitFor(ManualResetEvent aEvent, int aMaxSeconds)
        {
            iConsole.ProgressOpen(aMaxSeconds);

            int secs = 0;

            for (uint t = 0; t < aMaxSeconds; t++)
            {
                if (aEvent.WaitOne(1000, false))
                {
                    iConsole.ProgressClose();
                    return;
                }

                iConsole.ProgressSetValue(++secs);
            }

            iConsole.ProgressClose();

            throw (new FlashException());
        }

        private void RomDirInfoResponse(object obj, ServiceFlash.AsyncActionRomDirInfo.EventArgsResponse aResponse)
        {
            iRomDirFallbackFlashId = aResponse.aFlashIdFallback;
            iRomDirFallbackOffset = aResponse.aOffsetFallback;
            iRomDirFallbackBytes = aResponse.aBytesFallback;
            iRomDirMainFlashId = aResponse.aFlashIdMain;
            iRomDirMainOffset = aResponse.aOffsetMain;
            iRomDirMainBytes = aResponse.aBytesMain;
            iRomDirInfoCollected = true;
            iRomDirInfoComplete.Set();
        }

        private void ReportSuccess()
        {
            iConsole.Write("ok");
            iConsole.Newline();
        }

        private IConsole iConsole;
        private ServiceFlash iServiceFlash;

        private ServiceFlash.AsyncActionRomDirInfo iActionRomDirInfo;
        private ManualResetEvent iRomDirInfoComplete;

        private bool iRomDirInfoCollected;
        private uint iRomDirFallbackFlashId;
        private uint iRomDirFallbackOffset;
        private uint iRomDirFallbackBytes;
        private uint iRomDirMainFlashId;
        private uint iRomDirMainOffset;
        private uint iRomDirMainBytes;
    }

    public interface IFallback
    {
        IFlash Find(uint aFlashId);
        byte[] Read(IFlash aFlash, uint aOffset, uint aBytes);
        void Write(IFlash aFlash, uint aOffset, byte[] aData);
        void Write(IFlash aFlash, uint aOffset, FileStream aFile);
        void Erase(IFlash aFlash, uint aOffset, uint aBytes);
        void Close();
    }

    public class FallbackEmulatorSrecA : IFallback
    {
        private IFlash iFlash;
        private FileStream iOutput;

        public FallbackEmulatorSrecA(IConsole aConsole, string aOutput)
        {
            iConsole = aConsole;
            iFlash = new Flash(4, 128, 256 * 1024);

            try
            {
                iOutput = new FileStream(aOutput, FileMode.Create);
            }
            catch (Exception)
            {
                throw (new FlashException());
            }

            byte[] hdr = UTF8Encoding.UTF8.GetBytes("HDR");

            uint count = (uint)hdr.Length;

            string record = "S0";

            uint checksum = 0;

            record += (count + 3).ToString("X2");

            checksum += count + 3;

            uint address = 0;

            record += address.ToString("X4");

            checksum += (byte)address;
            checksum += (byte)(address >> 8);
            checksum += (byte)(address >> 16);
            checksum += (byte)(address >> 24);

            for (int i = 0; i < count; i++)
            {
                byte value = hdr[i];
                record += value.ToString("X2");
                checksum += value;
            }

            checksum = ~checksum;

            record += ((byte)checksum).ToString("X2");

            record += "\n";

            byte[] data = UTF8Encoding.UTF8.GetBytes(record);

            iOutput.Write(data, 0, data.Length);

            ReportSuccess();
        }

        public IFlash Find(uint aFlashId)
        {
            if (aFlashId == 4)
            {
                ReportSuccess();
                return (iFlash);
            }

            throw (new FlashException());
        }

        public byte[] Read(IFlash aFlash, uint aOffset, uint aBytes)
        {
            throw (new FlashException());
        }

        public void Write(IFlash aFlash, uint aOffset, byte[] aData)
        {
            if (aOffset + aData.Length > aFlash.SectorCount * aFlash.SectorBytes)
            {
                throw (new FlashException("Flash size exceeded, writing " + aData.Length + " bytes to " + aOffset));
            }

            uint offset = 0;
            uint count = (uint)aData.Length;

            while (count > 0)
            {
                uint payload = 64;

                if (count < 64)
                {
                    payload = count;
                }

                string record = "S3";

                uint checksum = 0;

                record += (payload + 5).ToString("X2");

                checksum += payload + 5;

                uint address = aOffset + offset;

                record += address.ToString("X8");

                checksum += (byte)address;
                checksum += (byte)(address >> 8);
                checksum += (byte)(address >> 16);
                checksum += (byte)(address >> 24);

                for (int i = 0; i < payload; i++)
                {
                    byte value = aData[offset++];
                    record += value.ToString("X2");
                    checksum += value;
                }

                checksum = ~checksum;

                record += ((byte)checksum).ToString("X2");

                record += "\n";

                byte[] data = UTF8Encoding.UTF8.GetBytes(record);

                iOutput.Write(data, 0, data.Length);

                count -= payload;
            }

            ReportSuccess();
        }

        public void Write(IFlash aFlash, uint aOffset, FileStream aFile)
        {
            byte[] data = new byte[aFile.Length];

            aFile.Read(data, 0, data.Length);

            Write(aFlash, aOffset, data);
        }

        public void Erase(IFlash aFlash, uint aOffset, uint aBytes)
        {
            ReportSuccess();
        }

        public void Close()
        {
            iOutput.Close();
        }

        private void ReportSuccess()
        {
            iConsole.Write("ok");
            iConsole.Newline();
        }

        private IConsole iConsole;
    }

    public class Fallback : IFallback
    {
        public const int kMaxReadBytes = 4096;
        public const int kMaxWriteBytes = 4096;

        private const int kMaxReadSeconds = 25;
        private const int kMaxReadRetries = 5;
        private const int kMaxWriteSeconds = 25;
        private const int kMaxWriteRetries = 5;
        private const int kMaxEraseSectorSeconds = 15;
        private const int kMaxEraseSectorRetries = 5;
        private const int kMaxSectorsSeconds = 10;
        private const int kMaxSectorBytesSeconds = 10;
        private const int kMaxSectorOpRetries = 5;

        public Fallback(IConsole aConsole, ServiceFlash aServiceFlash)
        {
            iConsole = aConsole;
            iServiceFlash = aServiceFlash;

            iActionSectors = iServiceFlash.CreateAsyncActionSectors();
            iActionSectors.EventResponse += SectorsResponse;
            iSectorsComplete = new ManualResetEvent(false);

            iActionRead = iServiceFlash.CreateAsyncActionRead();
            iActionRead.EventResponse += ReadResponse;
            iReadComplete = new ManualResetEvent(false);

            iActionWrite = iServiceFlash.CreateAsyncActionWrite();
            iActionWrite.EventResponse += WriteResponse;
            iWriteComplete = new ManualResetEvent(false);

            iActionSectorBytes = iServiceFlash.CreateAsyncActionSectorBytes();
            iActionSectorBytes.EventResponse += SectorBytesResponse;
            iSectorBytesComplete = new ManualResetEvent(false);

            iActionEraseSector = iServiceFlash.CreateAsyncActionEraseSector();
            iActionEraseSector.EventResponse += EraseSectorResponse;
            iEraseSectorComplete = new ManualResetEvent(false);
        }

        public IFlash Find(uint aFlashId)
        {
            uint repeatCount = kMaxSectorOpRetries;
            bool cont = true;;
            
            while ( cont )
            {
                try
                {
                    iSectorsComplete.Reset();
                    iActionSectors.SectorsBegin(aFlashId);
                    WaitFor(iSectorsComplete, kMaxSectorsSeconds);
                    cont = false;
                }
                catch ( FlashException )
                {
                    if ( --repeatCount == 0 )
                        throw;
                }
            }
            
            repeatCount = kMaxSectorOpRetries;
            cont = true;
            while ( cont )
            {
                try
                {
                    iSectorBytesComplete.Reset();
                    iActionSectorBytes.SectorBytesBegin(aFlashId);
                    WaitFor(iSectorBytesComplete, kMaxSectorsSeconds);
                    cont = false;
                }
                catch ( FlashException )
                {
                    if ( --repeatCount == 0 )
                        throw;
                }
            }

            ReportSuccess();

            return (new Flash(aFlashId, iSectors, iSectorBytes));
        }

        private void WaitFor(ManualResetEvent aEvent, int aMaxSeconds)
        {
            iConsole.ProgressOpen(aMaxSeconds);

            int secs = 0;

            for (uint t = 0; t < aMaxSeconds; t++)
            {
                if (aEvent.WaitOne(1000, false))
                {
                    iConsole.ProgressClose();
                    return;
                }

                iConsole.ProgressSetValue(++secs);
            }

            iConsole.ProgressClose();

            throw (new FlashException());
        }

        private void SectorsResponse(object obj, ServiceFlash.AsyncActionSectors.EventArgsResponse aResponse)
        {
            iSectors = aResponse.aSectors;
            iSectorsComplete.Set();
        }

        private void SectorBytesResponse(object obj, ServiceFlash.AsyncActionSectorBytes.EventArgsResponse aResponse)
        {
            iSectorBytes = aResponse.aBytes;
            iSectorBytesComplete.Set();
        }

        public byte[] Read(IFlash aFlash, uint aOffset, uint aBytes)
        {
            if (aOffset + aBytes > aFlash.SectorCount * aFlash.SectorBytes)
            {
                throw (new FlashException("Flash size exceeded"));
            }

            iData = new byte[aBytes];

            iOffset = 0;

            uint bytes = aBytes;

            iConsole.ProgressOpen((int)aBytes);

            while (bytes > kMaxReadBytes)
            {
            	uint retry = kMaxReadRetries;
            	
            	while (true)
            	{
	                iReadComplete.Reset();
	
	                iActionRead.ReadBegin(aFlash.Id, aOffset + iOffset, kMaxReadBytes);
	
	                if (iReadComplete.WaitOne(kMaxReadSeconds * 1000, false))
	                {
	                	break;
	                }
	                
	                if (--retry == 0)
	                {
	                    iConsole.ProgressClose();
	                    throw (new FlashException());
	                }
                }

                bytes -= kMaxReadBytes;
                iOffset += kMaxReadBytes;

                iConsole.ProgressSetValue((int)iOffset);
            }

            if (bytes > 0)
            {
                iReadComplete.Reset();

                iActionRead.ReadBegin(aFlash.Id, aOffset + iOffset, bytes);

                if (!iReadComplete.WaitOne(kMaxReadSeconds * 1000, false))
                {
                    iConsole.ProgressClose();
                    throw (new FlashException());
                }
            }

            iConsole.ProgressClose();

            ReportSuccess();

            return (iData);
        }

        private void ReadResponse(object obj, ServiceFlash.AsyncActionRead.EventArgsResponse aResponse)
        {
            Array.Copy(aResponse.aBuffer, 0, iData, iOffset, aResponse.aBuffer.Length);

            iReadComplete.Set();
        }

        public void Write(IFlash aFlash, uint aOffset, byte[] aData)
        {
            if (aOffset + aData.Length > aFlash.SectorCount * aFlash.SectorBytes)
            {
                throw (new FlashException("Flash size exceeded"));
            }

            iOffset = 0;

            int bytes = aData.Length;

            iConsole.ProgressOpen(bytes);

            while (bytes > kMaxWriteBytes)
            {
                byte[] data = new byte[kMaxWriteBytes];

                Array.Copy(aData, iOffset, data, 0, kMaxWriteBytes);

				uint retry = kMaxWriteRetries;
				
				while (true)
				{
	                iWriteComplete.Reset();
	
	                iActionWrite.WriteBegin(aFlash.Id, aOffset + (uint)iOffset, data);
	
	                if (iWriteComplete.WaitOne(kMaxWriteSeconds * 1000, false))
	                {
	                	break;
	                }
	                
	                if (--retry == 0)
	                {
	                    iConsole.ProgressClose();
	                    throw (new FlashException());
	                }
                }

                bytes -= kMaxWriteBytes;
                iOffset += kMaxWriteBytes;

                iConsole.ProgressSetValue((int)iOffset);
            }

            if (bytes > 0)
            {
                byte[] data = new byte[bytes];

                Array.Copy(aData, iOffset, data, 0, bytes);

				uint retry = kMaxWriteRetries;
				
				while (true)
				{
	                iWriteComplete.Reset();
	
	                iActionWrite.WriteBegin(aFlash.Id, aOffset + (uint)iOffset, data);
	
	                if (iWriteComplete.WaitOne(kMaxWriteSeconds * 1000, false))
	                {
	                	break;
	                }
	                
	                if (--retry == 0)
	                {
	                    iConsole.ProgressClose();
	                    throw (new FlashException());
	                }
                }
            }
            iConsole.ProgressClose();
            ReportSuccess();
        }

        public void Write(IFlash aFlash, uint aOffset, FileStream aFile)
        {
            if (aOffset + aFile.Length > aFlash.SectorCount * aFlash.SectorBytes)
            {
                throw (new FlashException("Flash size exceeded"));
            }

            iOffset = 0;

            int bytes = (int)aFile.Length;

            iConsole.ProgressOpen(bytes);

            while (bytes > kMaxWriteBytes)
            {
                byte[] data = new byte[kMaxWriteBytes];

                aFile.Read(data, 0, kMaxWriteBytes);

				uint retry = kMaxWriteRetries;
				
				while (true)
				{
	                iWriteComplete.Reset();
	
	                iActionWrite.WriteBegin(aFlash.Id, aOffset + (uint)iOffset, data);
	
	                if (iWriteComplete.WaitOne(kMaxWriteSeconds * 1000, false))
	                {
	                	break;
	                }
	                
	                if (--retry == 0)
	                {
	                    iConsole.ProgressClose();
	                    throw (new FlashException());
	                }
                }

                bytes -= kMaxWriteBytes;
                iOffset += kMaxWriteBytes;

                iConsole.ProgressSetValue((int)iOffset);
            }

            if (bytes > 0)
            {
                byte[] data = new byte[bytes];

                aFile.Read(data, 0, bytes);

				uint retry = kMaxWriteRetries;
				
				while (true)
				{
	                iWriteComplete.Reset();
	
	                iActionWrite.WriteBegin(aFlash.Id, aOffset + (uint)iOffset, data);
	
	                if (iWriteComplete.WaitOne(kMaxWriteSeconds * 1000, false))
	                {
	                	break;
	                }
	                
	                if (--retry == 0)
	                {
	                    iConsole.ProgressClose();
	                    throw (new FlashException());
	                }
                }
            }

            iConsole.ProgressClose();
            ReportSuccess();
        }

        private void WriteResponse(object obj, ServiceFlash.AsyncActionWrite.EventArgsResponse aResponse)
        {
            iWriteComplete.Set();
        }

        public void Erase(IFlash aFlash, uint aOffset, uint aBytes)
        {
            uint id = aFlash.Id;
            uint sectorbytes = aFlash.SectorBytes;

            /*
            if (aOffset % sectorbytes != 0 || aBytes % sectorbytes != 0)
            {
                throw (new FlashException("Sector alignment error"));
            }
            */

            iSector = aOffset / sectorbytes;
            iCount = (aBytes + sectorbytes - 1) / sectorbytes;

            iConsole.ProgressOpen((int)iCount);

            for (int i = 0; i < iCount; )
            {
				uint retry = kMaxEraseSectorRetries;
				
				while (true)
				{
	                iEraseSectorComplete.Reset();
	
	                iActionEraseSector.EraseSectorBegin(id, iSector);

	                if (iEraseSectorComplete.WaitOne(kMaxEraseSectorSeconds * 1000, false))
	                {
	                	break;
	                }
	                
	                if (--retry == 0)
	                {
	                    iConsole.ProgressClose();
	                    throw (new FlashException());
	                }
                }
                
                iSector++;

                iConsole.ProgressSetValue(++i);
            }

            iConsole.ProgressClose();
            ReportSuccess();
        }

        private void EraseSectorResponse(object obj, ServiceFlash.AsyncActionEraseSector.EventArgsResponse aResponse)
        {
            iEraseSectorComplete.Set();
        }

        public void Close()
        {
        }

        private void ReportSuccess()
        {
            iConsole.Write("ok");
            iConsole.Newline();
        }

        private IConsole iConsole;
        private ServiceFlash iServiceFlash;

        private ServiceFlash.AsyncActionSectors iActionSectors;
        private ManualResetEvent iSectorsComplete;

        private ServiceFlash.AsyncActionRead iActionRead;
        private ManualResetEvent iReadComplete;

        private ServiceFlash.AsyncActionWrite iActionWrite;
        private ManualResetEvent iWriteComplete;

        private ServiceFlash.AsyncActionSectorBytes iActionSectorBytes;
        private ManualResetEvent iSectorBytesComplete;

        private ServiceFlash.AsyncActionEraseSector iActionEraseSector;
        private ManualResetEvent iEraseSectorComplete;

        private uint iSectorBytes;
        private uint iSector;
        private uint iCount;
        private uint iSectors;
        private byte[] iData;
        private uint iOffset;
    }

    public class Finder : ISsdpNotifyHandler
    {
        private const int kMaxFindSeconds = 12;
        private const int kMaxFindRetries = 5;
        private const int kMaxSetBootModeSeconds = 10;
        private const int kMaxRebootAcceptedSeconds = 10;
        private const int kMaxByeByeSeconds = 20;
        private const int kMaxAliveSeconds = 20;
        private const int kMaxRunningFallbackSeconds = 5;
        private const int kMaxEstablishRetryCount = 3;

        private const int kLinnUdnLength = 36;
        private const string kLinnUdnPrefix = "4c494e4e-";

        private IPAddress iInterface;
        private IConsole iConsole;
        private SsdpListenerMulticast iListener;
        private Mutex iMutex;
        private DeviceListUpnp iDeviceList;
        private string iUglyName;
        private Device iDevice;
        private string iDeviceUdnPrefix;
        private ManualResetEvent iIsFound;
        private ManualResetEvent iIsBootModeSet;
        private ManualResetEvent iIsRebootAccepted;
        private ManualResetEvent iIsByeByeReceived;
        private ManualResetEvent iIsAliveReceived;
        private bool iNotifyByeByeReceived;

        public Finder(IPAddress aInterface, IConsole aConsole, string aUglyName)
        {
            iInterface = aInterface;
            iConsole = aConsole;
            iUglyName = aUglyName;
            iMutex = new Mutex();
            iIsFound = new ManualResetEvent(false);
            iIsBootModeSet = new ManualResetEvent(false);
            iIsRebootAccepted = new ManualResetEvent(false);
            iIsByeByeReceived = new ManualResetEvent(false);
            iIsAliveReceived = new ManualResetEvent(false);
            iListener = new SsdpListenerMulticast();
            iListener.Add(this);
            iListener.Start(iInterface);
        }

        private bool WaitFor(ManualResetEvent aEvent, int aMaxSeconds)
        {
            iConsole.ProgressOpen(aMaxSeconds);

            int secs = 0;

            for (uint t = 0; t < aMaxSeconds; t++)
            {
                bool done = aEvent.WaitOne(1000, false);

                if (done)
                {
                    iConsole.ProgressClose();
                    iConsole.Write("ok");
                    iConsole.Newline();
                    return (true);
                }

                iConsole.ProgressSetValue(++secs);
            }

            iConsole.ProgressClose();
            return (false);
        }

        public bool EstablishFallback()
        {
        	int retry = kMaxEstablishRetryCount;
        	
            while (retry-- > 0)
            {
                if (!FindDevice())
                {
                    return (false);
                }

                if (DeviceIsInFallback)
                {
                    return (true);
                }

                if (!RebootDeviceIntoFallback())
                {
		            iConsole.Write("skipped");
		            iConsole.Newline();
                }
            }
            
            return (false);
        }

        public bool EstablishMain()
        {
        	int retry = kMaxEstablishRetryCount;
        	
            while (retry-- > 0)
            {
                if (!FindDevice())
                {
                    return (false);
                }

                if (!DeviceIsInFallback)
                {
                    return (true);
                }

                if (!RebootDeviceIntoMain())
                {
		            iConsole.Write("skipped");
		            iConsole.Newline();
                }
            }
            
            return (false);
        }

        public bool DeviceIsInFallback
        {
            get
            {
                Assert.Check(iDevice != null);
                Assert.Check(iDevice.Udn.Length == kLinnUdnLength);
                Assert.Check(iDevice.Udn.StartsWith(kLinnUdnPrefix));
                return (iDevice.Udn.Substring(32, 2) == "00");
            }
        }

        public bool CheckRunningFallback()
        {
            ManualResetEvent never = new ManualResetEvent(false);

            // Wait for device to recover from the battering it gets
            // when it announces itself to the network before trying
            // to retreive its device xml to establish whether it is
            // running the fallback or not.

            WaitFor(never, kMaxRunningFallbackSeconds);

            try
            {
                uint version = iDevice.HasService(ServiceFlash.ServiceType());
                return (version != 0);
            }
            catch (DeviceException)
            {
                throw (new FlashException());
            }
        }

        public bool FindDevice()
        {
            iIsFound.Reset();

            iDeviceList = new DeviceListUpnp(ServiceVolkano.ServiceType(), iListener);

            iDeviceList.EventDeviceAdded += DeviceAdded;

            iDeviceList.Start(iInterface);

			bool result = false;
			
			uint i = kMaxFindRetries;
			
            while (i-- > 0)
            {
	            iConsole.Write("Finding " + iUglyName + " ................ ");
	
	            if (WaitFor(iIsFound, kMaxFindSeconds))
	            {
	            	result = true;
	            	break;
	            }
	            
	            if (i > 0)
	            {
	    			iConsole.Write("Retry");
					iConsole.Newline();
					iDeviceList.Rescan();
				}    			        
            }

            iDeviceList.EventDeviceAdded -= DeviceAdded;
            iDeviceList.Stop();
            
            return (result);
        }

        public void Close()
        {
            iListener.Stop();
        }

        public bool SetBootMode(string aMode)
        {
            iConsole.Write("Setting boot mode .................. ");

            ServiceVolkano volkano = new ServiceVolkano(iDevice);

            ServiceVolkano.AsyncActionSetBootMode async = volkano.CreateAsyncActionSetBootMode();

            async.EventResponse += SetBootModeResponse;

            iIsBootModeSet.Reset();

            async.SetBootModeBegin(aMode);

            return (WaitFor(iIsBootModeSet, kMaxSetBootModeSeconds));
        }

        public bool RebootDeviceIntoFallback()
        {
            if (SetBootMode("Fallback"))
            {
                if (IssueReboot())
                {
                    return (WaitForOffOn());
                }
            }

            return (false);
        }

        public bool RebootDeviceIntoMain()
        {
            if (SetBootMode("Main"))
            {
                if (IssueReboot())
                {
                    return (WaitForOffOn());
                }
            }

            return (false);
        }

        public bool IssueReboot()
        {
            iConsole.Write("Rebooting .......................... ");

            ServiceVolkano volkano = new ServiceVolkano(iDevice);

            ServiceVolkano.AsyncActionReboot async = volkano.CreateAsyncActionReboot();

            async.EventResponse += RebootResponse;

            iIsRebootAccepted.Reset();
            iIsByeByeReceived.Reset();
            iIsAliveReceived.Reset();

            iMutex.WaitOne();

            iNotifyByeByeReceived = false;

            iMutex.ReleaseMutex();

            iDeviceUdnPrefix = iDevice.Udn.Substring(0, 32);

            async.RebootBegin();

            return (WaitFor(iIsRebootAccepted, kMaxRebootAcceptedSeconds));
        }

        public bool WaitForOffOn()
        {
            iConsole.Write("Waiting for device off ............. ");

            if (WaitFor(iIsByeByeReceived, kMaxByeByeSeconds))
            {
                iConsole.Write("Waiting for device on .............. ");

                if (WaitFor(iIsAliveReceived, kMaxAliveSeconds))
                {
                    return (true);
                }
            }

            return (false);
        }

        public Device Device
        {
            get
            {
                return (iDevice);
            }
        }

        private void DeviceAdded(object obj, DeviceList.EventArgsDevice e)
        {
            ServiceVolkano volkano = new ServiceVolkano(e.Device);

            ServiceVolkano.AsyncActionUglyName async = volkano.CreateAsyncActionUglyName();

            async.EventResponse += UglyNameResponse;

            async.UglyNameBegin();
        }

        private void UglyNameResponse(object obj, ServiceVolkano.AsyncActionUglyName.EventArgsResponse aResponse)
        {
            if (String.Compare(aResponse.aUglyName, iUglyName, true) == 0)
            {
                iDevice = (obj as ServiceVolkano).Device;

                iIsFound.Set();
            }
        }

        private void SetBootModeResponse(object obj, ServiceVolkano.AsyncActionSetBootMode.EventArgsResponse aResponse)
        {
            iIsBootModeSet.Set();
        }

        private void RebootResponse(object obj, ServiceVolkano.AsyncActionReboot.EventArgsResponse aResponse)
        {
            iIsRebootAccepted.Set();
        }

        // ISsdpNotifyHandler

        private void NotifyAlive(byte[] aUuid)
        {
            iMutex.WaitOne();

            bool byebye = iNotifyByeByeReceived;

            iMutex.ReleaseMutex();

            if (byebye)
            {
                string uuid = ASCIIEncoding.UTF8.GetString(aUuid);

                if (uuid.StartsWith(iDeviceUdnPrefix))
                {
                    iIsAliveReceived.Set();
                }
            }
        }

        private void NotifyByeBye(byte[] aUuid)
        {
            if (iDeviceUdnPrefix != null)
            {
                string uuid = ASCIIEncoding.UTF8.GetString(aUuid);

                if (uuid.StartsWith(iDeviceUdnPrefix))
                {
                    iIsByeByeReceived.Set();

                    iMutex.WaitOne();

                    iNotifyByeByeReceived = true;

                    iMutex.ReleaseMutex();
                }
            }
        }

        public void NotifyRootAlive(byte[] aUuid, byte[] aLocation, uint aMaxAge)
        {
            NotifyAlive(aUuid);
        }

        public void NotifyUuidAlive(byte[] aUuid, byte[] aLocation, uint aMaxAge)
        {
            NotifyAlive(aUuid);
        }

        public void NotifyDeviceTypeAlive(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion, byte[] aLocation, uint aMaxAge)
        {
            NotifyAlive(aUuid);
        }

        public void NotifyServiceTypeAlive(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion, byte[] aLocation, uint aMaxAge)
        {
            NotifyAlive(aUuid);
        }

        public void NotifyRootByeBye(byte[] aUuid)
        {
            NotifyAlive(aUuid);
        }

        public void NotifyUuidByeBye(byte[] aUuid)
        {
            NotifyByeBye(aUuid);
        }

        public void NotifyDeviceTypeByeBye(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion)
        {
            NotifyByeBye(aUuid);
        }

        public void NotifyServiceTypeByeBye(byte[] aUuid, byte[] aDomain, byte[] aType, uint aVersion)
        {
            NotifyByeBye(aUuid);
        }
    }
}
