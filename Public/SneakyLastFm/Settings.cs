using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Forms;

namespace SneakyLastFm
{
    public class Settings
    {
        public void Load()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            FileInfo fi = new FileInfo(Application.StartupPath + "/SneakyLastFm.config");
            if (fi.Exists)
            {
                FileStream fileStream = fi.OpenRead();
                Settings settings = (Settings)serializer.Deserialize(fileStream);
                iUsername = settings.Username;
                iPassword = settings.Password;
                fileStream.Close();
            }
        }

        public void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            StreamWriter writer = new StreamWriter(Application.StartupPath + "/SneakyLastFm.config");
            serializer.Serialize(writer, this);
            writer.Close();
        }

        public string Username
        {
            get
            {
                return iUsername;
            }
            set
            {
                iUsername = value;
            }
        }

        public string Password
        {
            get
            {
                return iPassword;
            }
            set
            {
                iPassword = value;
            }
        }

        private string iUsername = "";
        private string iPassword = "";
    }
}
