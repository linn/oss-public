using System;
using System.IO;
using System.Xml.Serialization;

using Linn;

namespace OssKinskyMppLastFm
{
    internal interface IOptionsPageLastFm
    {
        void SetUsername(string aUsername);
        string Username { get; }

        void SetPassword(string aPassword);
        string Password { get; }

        event EventHandler<EventArgs> EventUsernameChanged;
        event EventHandler<EventArgs> EventPasswordChanged;
    }

    [XmlRoot("LastFm")]
    public class OptionsLastFm
    {
        protected OptionsLastFm() { }

        public OptionsLastFm(string aBasePath)
        {
            iOptionsFile = aBasePath + Path.DirectorySeparatorChar + "LastFm.xml";

            iOptionsPageLastFm = new OptionsPageLastFm();

            iOptionsPageLastFm.EventUsernameChanged += UsernameChanged;
            iOptionsPageLastFm.EventPasswordChanged += PasswordChanged;

            SetDefaults();
            Load();
        }

        public IViewUserOptionsPage OptionsPageLastFm
        {
            get
            {
                return iOptionsPageLastFm;
            }
        }

        [XmlElement("Version")]
        public uint Version
        {
            get
            {
                return iVersion;
            }
            set
            {
                iVersion = value;
            }
        }

        [XmlElement("Username")]
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

        [XmlElement("Password")]
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

        public event EventHandler<EventArgs> EventUsernamePasswordChanged;

        private void Load()
        {
            TextReader reader = null;
            try
            {
                XmlSerializer serialiser = new XmlSerializer(typeof(OptionsLastFm));
                reader = new StreamReader(iOptionsFile);
                OptionsLastFm options = serialiser.Deserialize(reader) as OptionsLastFm;
                reader.Close();

                if (options.Version == kVersion)
                {
                    iUsername = options.Username;
                    iPassword = options.Password;

                    iOptionsPageLastFm.SetUsername(options.Username);
                    iOptionsPageLastFm.SetPassword(options.Password);
                }
                else
                {
                    UserLog.WriteLine(DateTime.Now + ": Last.fm settings file version incorrect, found " + options.Version + " expected " + kVersion + ", using default settings");
                    SetDefaults();
                    Save();
                }
            }
            catch (FileNotFoundException)
            {
                UserLog.WriteLine(DateTime.Now + ": Last.fm settings file not found, using default settings");
                SetDefaults();
                Save();
            }
            catch (InvalidOperationException e)
            {
                if (reader != null)
                {
                    reader.Close();
                }
                UserLog.WriteLine(DateTime.Now + ": Last.fm settings file corrupt, using default settings (" + e.Message + ")");
                SetDefaults();
                Save();
            }
        }

        private void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(OptionsLastFm));
            TextWriter writer = new StreamWriter(iOptionsFile);
            serializer.Serialize(writer, this);
            writer.Close();
        }

        private void SetDefaults()
        {
            iVersion = kVersion;
            iUsername = "";
            iPassword = "";
        }

        private void UsernameChanged(object sender, EventArgs e)
        {
            if (iUsername != iOptionsPageLastFm.Username)
            {
                iUsername = iOptionsPageLastFm.Username;

                Save();

                if (EventUsernamePasswordChanged != null)
                {
                    EventUsernamePasswordChanged(this, EventArgs.Empty);
                }
            }
        }

        private void PasswordChanged(object sender, EventArgs e)
        {
            if (iPassword != iOptionsPageLastFm.Password)
            {
                iPassword = iOptionsPageLastFm.Password;

                Save();

                if (EventUsernamePasswordChanged != null)
                {
                    EventUsernamePasswordChanged(this, EventArgs.Empty);
                }
            }
        }

        private const uint kVersion = 2;

        private uint iVersion;
        private string iOptionsFile;
        private OptionsPageLastFm iOptionsPageLastFm;

        private string iUsername;
        private string iPassword;
    }
}