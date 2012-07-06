using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

using Linn.ControlPoint.Upnp;

namespace Linn.ProductSupport.Flash
{
    public class RomDirEntry
    {
        public const int kRomDirEntryBytes = 32;

        public static RomDirEntry Create(byte[] aData, uint aOffset)
        {
            uint key = BigEndian.UintAt(aData, aOffset);
            uint flashId = BigEndian.UintAt(aData, aOffset + 4);
            uint offset = BigEndian.UintAt(aData, aOffset + 8);
            uint bytes = BigEndian.UintAt(aData, aOffset + 12);
            uint type = BigEndian.UintAt(aData, aOffset + 16);
            uint crc = BigEndian.UintAt(aData, aOffset + 20);
            uint originalFlashId = BigEndian.UintAt(aData, aOffset + 24);
            uint originalOffset = BigEndian.UintAt(aData, aOffset + 28);

            return (new RomDirEntry(key, flashId, offset, bytes, type, crc, originalFlashId, originalOffset));
        }

        public byte[] Create()
        {
            byte[] key = BigEndian.Bytes(iKey);
            byte[] flashId = BigEndian.Bytes(iFlashId);
            byte[] offset = BigEndian.Bytes(iOffset);
            byte[] bytes = BigEndian.Bytes(iBytes);
            byte[] type = BigEndian.Bytes(iType);
            byte[] crc = BigEndian.Bytes(iCrc);
            byte[] originalFlashId = BigEndian.Bytes(iOriginalFlashId);
            byte[] originalOffset = BigEndian.Bytes(iOriginalOffset);

            byte[] data = new byte[kRomDirEntryBytes];

            Array.Copy(key, 0, data, 0, 4);
            Array.Copy(flashId, 0, data, 4, 4);
            Array.Copy(offset, 0, data, 8, 4);
            Array.Copy(bytes, 0, data, 12, 4);
            Array.Copy(type, 0, data, 16, 4);
            Array.Copy(crc, 0, data, 20, 4);
            Array.Copy(originalFlashId, 0, data, 24, 4);
            Array.Copy(originalOffset, 0, data, 28, 4);

            return (data);
        }

        public RomDirEntry(uint aKey, uint aFlashId, uint aOffset, uint aBytes, uint aType)
            : this(aKey, aFlashId, aOffset, aBytes, aType, 0)
        {
        }

        public RomDirEntry(uint aKey, uint aFlashId, uint aOffset, uint aBytes, uint aType, uint aCrc)
        {
            iKey = aKey;
            iFlashId = aFlashId;
            iOffset = aOffset;
            iBytes = aBytes;
            iType = aType;
            iCrc = aCrc;
            iOriginalFlashId = iFlashId;
            iOriginalOffset = iOffset;
        }

        private RomDirEntry(uint aKey, uint aFlashId, uint aOffset, uint aBytes, uint aType, uint aCrc, uint aOriginalFlashId, uint aOriginalOffset)
        {
            iKey = aKey;
            iFlashId = aFlashId;
            iOffset = aOffset;
            iBytes = aBytes;
            iType = aType;
            iCrc = aCrc;
            iOriginalFlashId = aOriginalFlashId;
            iOriginalOffset = aOriginalOffset;
        }

        public uint Key
        {
            get
            {
                return iKey;
            }
        }

        public uint FlashId
        {
            get
            {
                return iFlashId;
            }
        }

        public uint Offset
        {
            get
            {
                return iOffset;
            }
        }

        public uint Bytes
        {
            get
            {
                return iBytes;
            }
        }

        public uint Type
        {
            get
            {
                return iType;
            }
        }

        public uint Crc
        {
            get
            {
                return iCrc;
            }
            set
            {
                iCrc = value;
            }
        }

        public uint OriginalFlashId
        {
            get
            {
                return iOriginalFlashId;
            }
        }

        public uint OriginalOffset
        {
            get
            {
                return iOriginalOffset;
            }
        }

        private uint iKey;
        private uint iFlashId;
        private uint iOffset;
        private uint iBytes;
        private uint iType;
        private uint iCrc;
        private uint iOriginalFlashId;
        private uint iOriginalOffset;
    }

    public class RomDir
    {
        public const uint kMaxEntryCount = 1000;

        public static RomDir Create(byte[] aData, uint aOffset)
        {
            RomDir romdir = new RomDir();

            uint entries = BigEndian.UintAt(aData, aOffset);

            if (entries > kMaxEntryCount)
            {
                // Rom directory corrupt
                return (null);
            }

            // Read entries

            uint offset = aOffset + 4;

            for (uint i = 0; i < entries; i++)
            {
                romdir.Add(RomDirEntry.Create(aData, offset));
                offset += RomDirEntry.kRomDirEntryBytes;
            }

            return (romdir);
        }

        public byte[] Create()
        {
            byte[] data = new byte[(iEntryList.Count * RomDirEntry.kRomDirEntryBytes) + 4];

            byte[] count = BigEndian.Bytes(iEntryList.Count);

            Array.Copy(count, 0, data, 0, 4);

            int offset = 4;

            foreach (RomDirEntry e in iEntryList)
            {
                byte[] entry = e.Create();
                Array.Copy(entry, 0, data, offset, entry.Length);
                offset += entry.Length;
            }

            return (data);
        }

        public RomDir()
        {
            iEntryList = new List<RomDirEntry>();
        }

        public void Add(RomDirEntry aRomDirEntry)
        {
            iEntryList.Add(aRomDirEntry);
        }

        public IList<RomDirEntry> EntryList
        {
            get
            {
                return (iEntryList.AsReadOnly());
            }
        }

        public RomDirEntry Find(uint aKey)
        {
            foreach (RomDirEntry e in iEntryList)
            {
                if (e.Key == aKey)
                {
                    return (e);
                }
            }

            return (null);
        }

        private List<RomDirEntry> iEntryList;
    }

    // Store Handling

    public class StoreEntry
    {
        public StoreEntry(uint aKey, byte[] aData)
        {
            iKey = aKey;
            iData = aData;
        }

        public static StoreEntry Create(uint aKey, uint aValue)
        {
            byte[] bytes = BigEndian.Bytes(aValue);
            return (new StoreEntry(aKey, bytes));
        }

        public static StoreEntry Create(uint aKey, int aValue)
        {
            byte[] bytes = BigEndian.Bytes(aValue);
            return (new StoreEntry(aKey, bytes));
        }

        public static StoreEntry Create(uint aKey, string aValue)
        {
            byte[] bytes = ASCIIEncoding.UTF8.GetBytes(aValue);
            return (new StoreEntry(aKey, bytes));
        }

        public static StoreEntry Create(uint aKey, FileStream aFile)
        {
            Assert.Check(aFile.Length <= Int32.MaxValue);
            int length = (int)aFile.Length;
            byte[] bytes = new byte[length];
            aFile.Read(bytes, 0, length);
            return (new StoreEntry(aKey, bytes));
        }

        public uint Key
        {
            get
            {
                return iKey;
            }
        }

        public byte[] Data
        {
            get
            {
                return iData;
            }
            set
            {
                iData = value;
            }
        }

        private uint iKey;
        private byte[] iData;
    }


    public class StoreException : Exception
    {
        public StoreException(string aMessage)
            : base(aMessage)
        {
        }
    }

    public class Store
    {
        public Store()
        {
            iStoreEntryList = new SortedList<uint, StoreEntry>();
        }

        private SortedList<uint, StoreEntry> iStoreEntryList;

        public void Add(StoreEntry aEntry)
        {
            try
            {
                iStoreEntryList.Add(aEntry.Key, aEntry);
            }
            catch (ArgumentException)
            {
                Assert.Check(false);
            }
        }

        public StoreEntry Find(uint aKey)
        {
            StoreEntry entry;

            iStoreEntryList.TryGetValue(aKey, out entry);

            return (entry);
        }

        public void Remove(uint aKey)
        {
            if (!iStoreEntryList.Remove(aKey))
            {
                Assert.Check(false);
            }
        }

        public IList<StoreEntry> StoreEntryList
        {
            get
            {
                return (iStoreEntryList.Values);
            }
        }

        private byte[] CreateSector(uint aSectorBytes)
        {
            byte[] sector = new byte[aSectorBytes];

            for (uint i = 0; i < 4; i++)
            {
                sector[i] = 0xcc;
            }

            for (uint i = 4; i < aSectorBytes; i++)
            {
                sector[i] = 0xff;
            }

            return (sector);
        }

        public byte[] CreateRoStore()
        {
            int bytes = 0;

            foreach (StoreEntry e in iStoreEntryList.Values)
            {
                bytes += (e.Data.Length + 8 + 3) & ~3;
            }

            bytes += 4;

            byte[] data = new byte[bytes];

            for (uint i = 0; i < bytes; i++)
            {
                data[i] = 0xff;
            }

            int offset = 0;

            foreach (StoreEntry e in iStoreEntryList.Values)
            {
                byte[] length = BigEndian.Bytes(e.Data.Length + 8);
                byte[] key = BigEndian.Bytes(e.Key);

                Array.Copy(length, 0, data, offset, 4);
                Array.Copy(key, 0, data, offset + 4, 4);
                Array.Copy(e.Data, 0, data, offset + 8, e.Data.Length);

                int skip = (e.Data.Length + 8 + 3) & ~3;

                offset += skip;
            }

            return (data);
        }

        public byte[] CreateRwStore(uint aSectorBytes)
        {
            List<byte[]> sectors = new List<byte[]>();

            byte[] sector = CreateSector(aSectorBytes);

            int offset = 4;

            foreach (StoreEntry e in iStoreEntryList.Values)
            {
                if (offset + e.Data.Length + 8 > aSectorBytes)
                {
                    sectors.Add(sector);
                    sector = CreateSector(aSectorBytes);
                    offset = 4;
                }

                byte[] length = BigEndian.Bytes(e.Data.Length + 8);
                byte[] key = BigEndian.Bytes(e.Key);

                Array.Copy(length, 0, sector, offset, 4);
                Array.Copy(key, 0, sector, offset + 4, 4);
                Array.Copy(e.Data, 0, sector, offset + 8, e.Data.Length);

                int skip = (e.Data.Length + 8 + 3) & ~3;

                offset += skip;
            }

            if (offset > 4)
            {
                byte[] last = new byte[offset];
                Array.Copy(sector, 0, last, 0, offset);
                sectors.Add(last); // add last sector
            }

            int bytes = 0;

            foreach (byte[] s in sectors)
            {
                bytes += s.Length;
            }

            byte[] result = new byte[bytes];

            offset = 0;

            foreach (byte[] s in sectors)
            {
                Array.Copy(s, 0, result, offset, s.Length);
                offset += s.Length;
            }

            return (result);
        }
        
        public static Store CreateRwStore(byte[] aData, uint aSectorBytes)
        {
            Assert.Check((aSectorBytes & (aSectorBytes - 1)) == 0);   // aSectorBytes is power of 2
            Assert.Check(aData.Length % aSectorBytes == 0);     // data length is multiple of aSectorBytes

            Store store = new Store();

            uint numSectors = (uint)aData.Length / aSectorBytes;

            for (uint i = 0; i < numSectors; i++)
            {
                uint sectorIdx = i * aSectorBytes;

                uint sectorFlags = BigEndian.UintAt(aData, sectorIdx);

                if (sectorFlags != 0xcccccccc)
                {
                    // skip free, unknown or erasing sectors
                    Trace.WriteLine(Trace.kCore, "BinaryRwToStore: Sector " + i + " not used: 0x" + sectorFlags.ToString("X"));
                    continue;
                }

                Trace.WriteLine(Trace.kCore, "BinaryRwToStore: Sector " + i + " starting");

                uint currIdx = sectorIdx + 4;

                while (true)
                {
                    // get the length
                    uint entryBytes = BigEndian.UintAt(aData, currIdx);

                    if (entryBytes == 0xffffffff)
                    {
                        // end of entries in this sector
                        break;
                    }

                    // get the key
                    uint entryKey = BigEndian.UintAt(aData, currIdx + 4);

                    // mask off length to see if entry is incomplete and get real length
                    bool entryIncomplete = (entryBytes & 0x80000000) != 0;
                    entryBytes &= 0x7fffffff;

                    // get total length - the entry length rounded up to multiple of 4 bytes
                    uint totalBytes = (entryBytes + 3) & 0xfffffffc;

                    if (totalBytes < 8)
                    {
                        return (null);
                    }
                    else if (currIdx + totalBytes > sectorIdx + aSectorBytes)
                    {
                        return (null);
                    }
                    else if (entryIncomplete)
                    {
                        // skip incomplete enty
                    }
                    else if (entryKey == 0)
                    {
                        // skip obsolete entry
                    }
                    else
                    {
                        // entry is ok

                        byte[] bytes = new byte[entryBytes - 8];

                        Array.Copy(aData, currIdx + 8, bytes, 0, entryBytes - 8);

                        store.Add(new StoreEntry(entryKey, bytes));
                    }

                    currIdx += totalBytes;
                }
            }

            return store;
        }

        public static Store CreateRoStore(byte[] aData)
        {
            Store store = new Store();

            uint index = 0;

            while (true)
            {
                // get the length
                uint entryBytes = BigEndian.UintAt(aData, index);

                if (entryBytes == 0xffffffff)
                {
                    // end of entries
                    break;
                }

                // get the key
                uint entryKey = BigEndian.UintAt(aData, index + 4);

                // mask off length to see if entry is incomplete and get real length
                bool entryIncomplete = (entryBytes & 0x80000000) != 0;
                entryBytes &= 0x7fffffff;

                // get total length - the entry length rounded up to multiple of 4 bytes
                uint totalBytes = (entryBytes + 3) & 0xfffffffc;

                if (totalBytes < 8)
                {
                    return (null);
                }
                else if (index + totalBytes > aData.Length)
                {
                    return (null);
                }
                else if (entryIncomplete)
                {
                    // skip incomplete enty
                }
                else if (entryKey == 0)
                {
                    // skip obsolete entry
                }
                else
                {
                    // entry is ok

                    byte[] bytes = new byte[entryBytes - 8];

                    Array.Copy(aData, index + 8, bytes, 0, entryBytes - 8);

                    store.Add(new StoreEntry(entryKey, bytes));
                }

                index += totalBytes;
            }

            return store;
        }
    }

    public class BigEndian
    {
        public static byte[] Bytes(uint aValue)
        {
            byte[] bytes = System.BitConverter.GetBytes(aValue);

            if (System.BitConverter.IsLittleEndian)
            {
                byte b0 = bytes[0];
                byte b1 = bytes[1];
                byte b2 = bytes[2];
                byte b3 = bytes[3];

                bytes[0] = b3;
                bytes[1] = b2;
                bytes[2] = b1;
                bytes[3] = b0;
            }
            return (bytes);
        }

        public static byte[] Bytes(int aValue)
        {
            byte[] bytes = System.BitConverter.GetBytes(aValue);

            if (System.BitConverter.IsLittleEndian)
            {
                byte b0 = bytes[0];
                byte b1 = bytes[1];
                byte b2 = bytes[2];
                byte b3 = bytes[3];

                bytes[0] = b3;
                bytes[1] = b2;
                bytes[2] = b1;
                bytes[3] = b0;
            }
            return (bytes);
        }

        public static uint UintAt(byte[] aData, uint aOffset)
        {
            byte[] bytes = new byte[4];

            Array.Copy(aData, aOffset, bytes, 0, 4);

            if (System.BitConverter.IsLittleEndian)
            {
                byte b0 = bytes[0];
                byte b1 = bytes[1];
                byte b2 = bytes[2];
                byte b3 = bytes[3];

                bytes[0] = b3;
                bytes[1] = b2;
                bytes[2] = b1;
                bytes[3] = b0;
            }
            return (System.BitConverter.ToUInt32(bytes, 0));
        }

        public static int IntAt(byte[] aData, uint aOffset)
        {
            byte[] bytes = new byte[4];

            Array.Copy(aData, aOffset, bytes, 0, 4);

            if (System.BitConverter.IsLittleEndian)
            {
                byte b0 = bytes[0];
                byte b1 = bytes[1];
                byte b2 = bytes[2];
                byte b3 = bytes[3];

                bytes[0] = b3;
                bytes[1] = b2;
                bytes[2] = b1;
                bytes[3] = b0;
            }
            return (System.BitConverter.ToInt32(bytes, 0));
        }
    }
}
