
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Linn.Kinsky;


namespace KinskyDesktop
{
    public class DropConverter : DropConverterRoot
    {
        public MediaProviderDraggable Convert(IDataObject aDataObject)
        {
            DraggableData data = new DraggableData(aDataObject);
            return Convert(data);
        }
    }

    public class DraggableData : IDraggableData
    {
        public DraggableData(IDataObject aDataObject)
        {
            iDataObject = aDataObject;
        }

        public MediaProviderDraggable GetInternal()
        {
            if (iDataObject.GetDataPresent(typeof(MediaProviderDraggable)))
            {
                return iDataObject.GetData(typeof(MediaProviderDraggable)) as MediaProviderDraggable;
            }

            return null;
        }

        public string GetText()
        {
            if (iDataObject.GetDataPresent(DataFormats.Text))
            {
                return iDataObject.GetData(DataFormats.Text) as string;
            }

            return null;
        }

        public string GetUri()
        {
            if (iDataObject.GetDataPresent("UniformResourceLocator"))
            {
                MemoryStream stream = iDataObject.GetData("UniformResourceLocator") as MemoryStream;

                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return ASCIIEncoding.UTF8.GetString(buffer, 0, buffer.Length).TrimEnd('\0');
            }

            return null;
        }

        public string[] GetFileList()
        {
            if (iDataObject.GetDataPresent(DataFormats.FileDrop))
            {
                return iDataObject.GetData(DataFormats.FileDrop) as string[];
            }

            return null;
        }

        IDataObject iDataObject;
    }
}


