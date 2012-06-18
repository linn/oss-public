
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using Linn;


namespace Linn.Kinsky
{
    public class FileItc2Exception : Exception
    {
    }

    public class FileItc2
    {
        public class Section
        {
            public Section(byte[] aHeader)
            {
                if (HeaderBytes > aHeader.Length)
                {
                    throw new FileItc2Exception();
                }

                // bytes 0-3 : section length
                // bytes 4-7 : section type
                Bytes = Linn.BigEndianConverter.BigEndianToInt32(aHeader, 0);
                Type = ASCIIEncoding.ASCII.GetString(aHeader, 4, 4);
            }

            public const int HeaderBytes = 8;
            public readonly int Bytes;
            public readonly string Type;
        }

        public class Item
        {
            public Item(Section aSection, Stream aStream)
            {
                // read the item header
                byte[] header = new byte[Item.HeaderBytes];
                int bytesRead = aStream.Read(header, 0, Item.HeaderBytes);

                if (bytesRead != Item.HeaderBytes)
                {
                    throw new FileItc2Exception();
                }

                // bytes 0-3     : header length - including the 8 bytes of section header i.e. 208
                // bytes 4-19    : unknown
                // bytes 20-27   : library ID
                // bytes 28-35   : track ID
                // bytes 36-39   : item location e.g. "locl"
                // bytes 40-43   : item format e.g. "PNGf"
                // bytes 44-47   : unknown
                // bytes 48-51   : image width
                // bytes 52-55   : image height
                // bytes 56-67   : unknown
                // bytes 68-71   : display width
                // bytes 72-75   : display height
                // bytes 76-195  : unknown
                // bytes 196-199 : "data" - next byte is start of data
                if (Linn.BigEndianConverter.BigEndianToInt32(header, 0) != Item.HeaderBytes + Section.HeaderBytes ||
                    ASCIIEncoding.ASCII.GetString(header, 196, 4) != "data")
                {
                    throw new FileItc2Exception();
                }

                // read header information
                LibraryId = ASCIIEncoding.ASCII.GetString(header, 20, 8);
                TrackId = ASCIIEncoding.ASCII.GetString(header, 28, 8);
                Location = ASCIIEncoding.ASCII.GetString(header, 36, 4);
                Format = ASCIIEncoding.ASCII.GetString(header, 40, 4);
                ImageWidth = Linn.BigEndianConverter.BigEndianToInt32(header, 48);
                ImageHeight = Linn.BigEndianConverter.BigEndianToInt32(header, 52);
                DisplayWidth = Linn.BigEndianConverter.BigEndianToInt32(header, 68);
                DisplayHeight = Linn.BigEndianConverter.BigEndianToInt32(header, 72);

                // set the stream information
                iStream = aStream;
                iStreamOffset = iStream.Position;
                StreamBytes = aSection.Bytes - Section.HeaderBytes - Item.HeaderBytes;

                // move the stream position to the end of the header
                iStream.Position += StreamBytes;
            }

            public readonly string LibraryId;
            public readonly string TrackId;
            public readonly string Location;
            public readonly string Format;
            public readonly int ImageWidth;
            public readonly int ImageHeight;
            public readonly int DisplayWidth;
            public readonly int DisplayHeight;

            public Stream GetStream()
            {
                iStream.Position = iStreamOffset;
                return iStream;
            }

            public readonly int StreamBytes;

            private const int HeaderBytes = 200;
            private Stream iStream;
            private long iStreamOffset;
        }


        public FileItc2(Stream aStream)
        {
            iStream = aStream;
            iItems = new List<Item>();

            // read first section
            Section section = ReadSectionHeader(iStream);
            if (section == null || section.Type != "itch")
            {
                throw new FileItc2Exception();
            }

            // read the "itch" section
            // bytes 0-15  : unknown
            // bytes 16-19 : content type
            // bytes 20+   : unknown
            byte[] itchData = new byte[20];
            int bytesRead = iStream.Read(itchData, 0, itchData.Length);

            if (bytesRead != itchData.Length ||
                ASCIIEncoding.ASCII.GetString(itchData, 16, 4) != "artw")
            {
                throw new FileItc2Exception();
            }

            // position stream at start of next section
            iStream.Position += section.Bytes - Section.HeaderBytes - 20;

            // read remaining sections
            while (true)
            {
                section = ReadSectionHeader(iStream);

                // check for EOS
                if (section == null)
                {
                    break;
                }

                if (section.Type == "item")
                {
                    // create the item - moves stream position to end of section
                    Item item = new Item(section, iStream);
                    iItems.Add(item);
                }
                else
                {
                    // unrecognised section - skip
                    iStream.Position += section.Bytes - Section.HeaderBytes;
                }
            }
        }

        public IList<Item> Items
        {
            get { return iItems.AsReadOnly(); }
        }

        private Section ReadSectionHeader(Stream aStream)
        {
            byte[] data = new byte[Section.HeaderBytes];

            int bytesRead = aStream.Read(data, 0, Section.HeaderBytes);
            if (bytesRead == Section.HeaderBytes)
            {
                // enough header data
                return new Section(data);
            }
            else if (bytesRead == 0)
            {
                // EOS
                return null;
            }

            // incomplete header
            throw new FileItc2Exception();
        }

        private Stream iStream;
        private List<Item> iItems;
    }
}



