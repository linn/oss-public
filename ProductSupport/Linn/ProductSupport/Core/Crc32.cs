using System;
using System.IO;

namespace Linn.ProductSupport
{
    public class Crc32
    {
        public const UInt32 kDefaultPolynomial = 0xedb88320;
        public const UInt32 kDefaultSeed = 0xffffffff;

        private UInt32 iSeed;
        private UInt32[] iTable;
        private static UInt32[] iDefaultTable;

        public Crc32()
            : this(kDefaultSeed)
        {
        }

        public Crc32(UInt32 aSeed)
        {
            iTable = InitializeTable(kDefaultPolynomial);
            iSeed = aSeed;
        }

        public UInt32 Compute(byte[] buffer)
        {
            UInt32 crc = iSeed;

            for (int i = 0; i < buffer.Length; i++)
            {
                unchecked
                {
                    crc = (crc >> 8) ^ iTable[buffer[i] ^ crc & 0xff];
                }
            }
            return ~crc;
        }

        public uint Compute(FileStream aFile)
        {
            const int kBufferBytes = 4000;

            byte[] buffer = new byte[kBufferBytes];

            uint crc = iSeed;

            while (true)
            {
                int bytes = aFile.Read(buffer, 0, kBufferBytes);

                if (bytes == 0)
                {
                    break;
                }

                for (int i = 0; i < bytes; i++)
                {
                    unchecked
                    {
                        crc = (crc >> 8) ^ iTable[buffer[i] ^ crc & 0xff];
                    }
                }
            }

            return ~crc;
        }

        private static UInt32[] InitializeTable(UInt32 polynomial)
        {
            if (polynomial == kDefaultPolynomial && iDefaultTable != null)
                return iDefaultTable;

            UInt32[] createTable = new UInt32[256];
            for (int i = 0; i < 256; i++)
            {
                UInt32 entry = (UInt32)i;
                for (int j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry = entry >> 1;
                createTable[i] = entry;
            }

            if (polynomial == kDefaultPolynomial)
                iDefaultTable = createTable;

            return createTable;
        }
    }
}
