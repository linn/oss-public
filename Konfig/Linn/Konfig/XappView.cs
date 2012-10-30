using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Text;

using OpenHome.Xapp;

namespace Linn.Konfig
{
    public class XappView : IView
    {
        public XappView(string aId, string aResourcePath)
        {
            iId = aId;
            iResourcePath = aResourcePath;
        }

        public string Id
        {
            get 
            {
                return iId;
            }
        }

        public bool WriteResource(string aUri, int aIndex, List<string> aLanguageList, IResourceWriter aResourceWriter)
        {
            if (aUri == "/index.xapp")
            {
                string file = Path.Combine(iResourcePath, iId, "index.xapp");
                StreamReader reader = new StreamReader(file);
                string document = reader.ReadToEnd();
                reader.Close();

                int index = 0;
                while (index != -1)
                {
                    index = document.IndexOf("<page-frag frag=");
                    if (index != -1)
                    {
                        int end = document.IndexOf(">", index);

                        file = Path.Combine(iResourcePath, document.Substring(index + 17, (end - 3) - (index + 17)));
                        reader = new StreamReader(file);
                        string xml = reader.ReadToEnd();
                        reader.Close();

                        document = document.Insert(end + 2, xml);
                        document = document.Remove(index, (end + 1) - index);
                    }
                }

                byte[] buffer = UTF8Encoding.UTF8.GetBytes(document);

                aResourceWriter.WriteResourceBegin(buffer.Length, string.Empty);
                aResourceWriter.WriteResource(buffer, buffer.Length);
                aResourceWriter.WriteResourceEnd();

                return true;
            }

            return false;
        }

        private string iId;
        private string iResourcePath;
    }
}
