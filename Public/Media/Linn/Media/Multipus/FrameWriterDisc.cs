using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Linn.Media.Multipus
{

    public class FrameWriterDisk : IFrameReader
    {
	    public FrameWriterDisk(IFrameReader aReader)
	    {
            iReader = aReader;
            //iFileStream = new FileStream("c:\\multipus.dat", FileMode.Create, FileAccess.Write);
            //iBinFormatter = new BinaryFormatter();
	    }

        public Frame Read()
        {
            Frame frame = iReader.Read();
            //iBinFormatter.Serialize(iFileStream, frame);
            return frame;
        }

        private IFrameReader iReader;
        private FileStream iFileStream;
        private BinaryFormatter iBinFormatter;
    }
}