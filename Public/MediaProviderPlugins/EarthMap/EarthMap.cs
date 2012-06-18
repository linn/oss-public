using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Threading;

using Linn;
using Linn.Kinsky;

namespace OssKinskyMppEarthMap
{
    public class MediaProviderEarthMapFactory : IMediaProviderFactoryV7
    {
        public IMediaProviderV7 Create(IMediaProviderSupportV7 aSupport)
        {
            return new MediaProviderEarthMap(aSupport);
        }
    }

    internal class MediaProviderEarthMap : IMediaProviderV7
    {
        public MediaProviderEarthMap(IMediaProviderSupportV7 aSupport)
        {
            iSupport = aSupport;

            iTimer = new Linn.Timer();
            iTimer.Interval = 1000 * 60 * 10;//3600000;
            iTimer.Elapsed += TimerElapsed;

            iTimerCloudMap = new Linn.Timer();
            iTimerCloudMap.Interval = 1000 * 60 * 60 * 3;   // update every 3hrs
            iTimerCloudMap.Elapsed += TimerCloudMapElapsed;

            iMapGenerator = new MapGenerator(OssKinskyMppEarthMap.Properties.Resources.Day, OssKinskyMppEarthMap.Properties.Resources.Night);

            try
            {
                Directory.CreateDirectory(Path.Combine(iSupport.AppSupport.SavePath, Name));
                
                string filename = Path.Combine(iSupport.AppSupport.SavePath, Path.Combine(Name, kCloudMapFilename));
                FileInfo info = new FileInfo(filename);
                if (info.Exists)
                {
                    // re-use saved image if it was written to cache within the last 3 hours
                    if (DateTime.Compare(DateTime.Now, info.LastWriteTime.AddHours(3)) < 0)
                    {
                        iClouds = new Bitmap(filename);
                    }
                }
            }
            catch (Exception) { }

            iPanel = new PanelBusy();
            iPanel.ForeColor = aSupport.ViewSupport.ForeColour;
            iPanel.BackColor = aSupport.ViewSupport.BackColour;
            iPanel.Font = aSupport.ViewSupport.FontMedium;

            iMutex = new Mutex(false);

            iPanel.Paint += Paint;
            iPanel.Resize += Resize;
        }

        public void Start()
        {
            iMutex.WaitOne();

            Assert.Check(iThread == null);

            iThread = new Thread(delegate()
            {
                iTimerCloudMap.Start();
                iTimer.Start();

                if (iClouds == null)
                {
                    TimerCloudMapElapsed(this, EventArgs.Empty);
                }
                TimerElapsed(this, EventArgs.Empty);
            });
            iThread.Priority = ThreadPriority.Lowest;
            iThread.Start();

            iMutex.ReleaseMutex();
        }

        public void Stop()
        {
            iMutex.WaitOne();

            Assert.Check(iThread != null);
            iThread.Abort();
            iThread.Join();

            if (iImage != null)
            {
                iImage.Dispose();
                iImage = null;
            }
            iThread = null;

            iMutex.ReleaseMutex();
        }

        public void Open()
        {
            iMutex.WaitOne();
            if (iImage == null)
            {
                iPanel.StartBusy();
                iPanel.SetMessage("Generating image...");
            }
            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iPanel.StopBusy();
        }

        public void Rescan() { }

        public string Name
        {
            get
            {
                return "EarthMap";
            }
        }

        public string Provider
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length != 0)
                {
                    return ((AssemblyCompanyAttribute)attributes[0]).Company;
                }
                else
                {
                    return "";
                }
            }
        }

        public string Version
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public Image Icon
        {
            get
            {
                return null;
            }
        }

        public Image IconSelected
        {
            get
            {
                return null;
            }
        }

        public System.Windows.Forms.Control Control
        {
            get
            {
                return iPanel;
            }
        }

        public IViewUserOptionsPage OptionsPage
        {
            get
            {
                return null;
            }
        }

        public string[] Location
        {
            get
            {
                return new string[] { };
            }
            set
            {
            }
        }

        public void Up(uint aLevels)
        {
        }

        public void OnSizeClick()
        {
        }

        public void OnViewClick()
        {
        }

        public event EventHandler<EventArgs> EventSizeEnabled;
        public event EventHandler<EventArgs> EventSizeDisabled;
        public event EventHandler<EventArgs> EventViewEnabled;
        public event EventHandler<EventArgs> EventViewDisabled;
        public event EventHandler<EventArgs> EventLocationChanged;

        private void TimerElapsed(object sender, EventArgs e)
        {
            if (iImage == null)
            {
                iPanel.StartBusy();
                iPanel.SetMessage("Generating image...");
            }

            Image image = iMapGenerator.CreateBlendedImage(iClouds);

            iMutex.WaitOne();

            if (iImage != null)
            {
                iImage.Dispose();
                iImage = null;
            }
            iImage = image;
            
            iMutex.ReleaseMutex();

            iPanel.StopBusy();
            iPanel.Invalidate();
        }

        private void TimerCloudMapElapsed(object sender, EventArgs e)
        {
            HttpWebResponse wresp = null;
            Stream stream = null;

            try
            {
                HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(kCloudMapUri);
                wreq.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

                wresp = (HttpWebResponse)wreq.GetResponse();
                stream = wresp.GetResponseStream();
                if (stream != null)
                {
                    iClouds = new Bitmap(stream);
                    iClouds.Save(Path.Combine(iSupport.AppSupport.SavePath, Path.Combine(Name, kCloudMapFilename)));
                    iPanel.Invalidate();
                }
            }
            catch (Exception) { }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }

                if (wresp != null)
                {
                    wresp.Close();
                }
            }
        }

        private void Paint(object sender, PaintEventArgs e)
        {
            if (iImage != null)
            {
                e.Graphics.DrawImage(iImage, 0, 0, iPanel.ClientSize.Width, iPanel.ClientSize.Height);
                /*Point p = Pixel(3.10, 55.55);
                e.Graphics.DrawLine(Pens.Red, new Point(p.X - 1, p.Y - 1), new Point(p.X + 1, p.Y + 1));
                e.Graphics.DrawLine(Pens.Red, new Point(p.X + 1, p.Y - 1), new Point(p.X - 1, p.Y + 1));*/
            }
        }

        private void Resize(object sender, EventArgs e)
        {
            iPanel.Invalidate();
        }

        private Point Pixel(double aLongitude, double aLatitude)
        {
            double x = ((180.0f - aLongitude) * iPanel.ClientSize.Width) / 360.0f - 0.5f;
            double y = ((90.0f - aLatitude) * iPanel.ClientSize.Height) / 180.0f - 0.5f;

            return new Point((int)x, (int)y);
        }

        private const string kCloudMapFilename = "Clouds.bmp";
        //private const string kCloudMapUri = "http://xplanet.dyndns.org/clouds/clouds_2048.jpg";
        private const string kCloudMapUri = "http://userpage.fu-berlin.de/~jml/clouds_2048.jpg";

        private IMediaProviderSupportV7 iSupport;

        private PanelBusy iPanel;

        private MapGenerator iMapGenerator;

        private Mutex iMutex;
        private Thread iThread;
        
        private Linn.Timer iTimer;
        private Image iImage;

        private Linn.Timer iTimerCloudMap;
        private Image iClouds;
        
    }

    internal class MapGenerator
    {
        public MapGenerator(Image aImageDay, Image aImageNight)
        {
            iImageDay = new Bitmap(aImageDay);
            iImageNight = new Bitmap(aImageNight);
        }

        public Bitmap CreateBlendedImage(Image aImageClouds)
        {
            Bitmap imageBlended = new Bitmap(iImageDay.Width, iImageDay.Height);

            Bitmap clouds = null;
            if (aImageClouds != null)
            {
                clouds = new Bitmap(aImageClouds, iImageDay.Size);
            }

            // Get the current GMT
            DateTime GMT = DateTime.UtcNow;

            // Calculate the Greenwich Sidereal Time
            double GST = GreenwichSiderealTime(GMT);

            // Calculate the solar right ascension and declination
            double alpha = SolarRightAscension(GMT);
            double delta = SolarDeclination(GMT);

            // Loop over the pixels of the night/day images
            for (int x = 0; x < iImageDay.Width; ++x)
            {
                // Calculate the longitude
                double longitude = 180.0f - (x + 0.5f) / iImageDay.Width * 360.0f;

                // Calculate the solar hour angle 
                double HA = GST * 360.0f / 24.0f - longitude - alpha;

                for (int y = 0; y < iImageDay.Height; ++y)
                {
                    // Calculate the latitude
                    double latitude = 90.0f - (y + 0.5f) / iImageDay.Height * 180.0f;

                    // Calculate the altitude of the sun
                    double alt = Altitude(HA, delta, latitude, longitude);

                    // Work out the interpolation factor for drawing the pixel
                    double intFactor;

                    if (alt > kDayAltMin)
                    {
                        intFactor = 1.0f;
                    }
                    else if (alt < kNightAltMax)
                    {
                        intFactor = 0.0f;
                    }
                    else
                    {
                        intFactor = (alt - kNightAltMax) / (kDayAltMin - kNightAltMax);
                    }

                    // Get the RGB pixels of the day and night images
                    Color dayColor = iImageDay.GetPixel(x, y);
                    Color nightColor = iImageNight.GetPixel(x, y);

                    // Calculate the interpolated value of the blended pixel
                    Color blendedColor = Color.FromArgb((int)(dayColor.R * intFactor + nightColor.R * (1.0f - intFactor)),
                        (int)(dayColor.G * intFactor + nightColor.G * (1.0f - intFactor)),
                        (int)(dayColor.B * intFactor + nightColor.B * (1.0f - intFactor)));

                    if (clouds != null)
                    {
                        Color cloudsColor = clouds.GetPixel(x, y);
                        blendedColor = Color.FromArgb((int)((blendedColor.R + cloudsColor.R) * 0.5f),
                            (int)((blendedColor.G + cloudsColor.G) * 0.5f),
                            (int)((blendedColor.B + cloudsColor.B) * 0.5f));
                    }

                    imageBlended.SetPixel(x, y, blendedColor);
                }
            }

            return imageBlended;
        }

        private double DaysSinceJ2000(DateTime aDateTime)
        {
            // Calculate the number of days from the epoch J2000.0
            // the specified date
            int year = aDateTime.Year;
            int month = aDateTime.Month + 1;
            int day = aDateTime.Day;

            double D0 = (367 * year) - (7 * (year + (month + 9) / 12) / 4) + (275 * month / 9) + day - 730531.5f;
            double D = D0 + GreenwichMeanTime(aDateTime) / 24.0f;

            return D;
        }

        private double GreenwichMeanTime(DateTime aDateTime)
        {
            // Get the Greenwich Mean Time, in hours
            int hour = aDateTime.Hour;
            int minute = aDateTime.Minute;
            int second = aDateTime.Second;

            double GMT = hour + (minute / 60.0f) + (second / 3600.0f);

            return GMT;
        }

        private double GreenwichSiderealTime(DateTime aDateTime)
        {
            // Get the number of days since J2000.0
            double D = DaysSinceJ2000(aDateTime);

            // Calculate the GST
            double T = D / 36525.0f;
            double GST = (280.46061837f + 360.98564736629f * D + 0.000388f * T * T) * 24.0f / 360.0f;

            // Phase it to within 24 hours
            while (GST < 0.0f) { GST += 24.0f; }
            while (GST >= 24.0f) { GST -= 24.0f; }

            return GST;
        }

        private double SolarRightAscension(DateTime aDateTime)
        {
            // Calculate the number of days from the epoch J2000.0
            double D = DaysSinceJ2000(aDateTime);

            // Convert this into centuries
            double T = D / 36525.0f;

            // Calculate the mean longitude and anomaly
            double L = 279.697f + 36000.769f * T;
            double M = 358.476f + 35999.050f * T;

            // Calculate the true longitude
            double lambda = L + (1.919f - 0.005f * T) * Math.Sin(M * kDtoR) + 0.020f * Math.Sin(2 * M * kDtoR);

            // Calculate the obliquity
            double epsilon = 23.452f - 0.013f * T;

            // Calculate the right ascension, in degrees
            double alpha = Math.Atan2(Math.Sin(lambda * kDtoR) * Math.Cos(epsilon * kDtoR), Math.Cos(lambda * kDtoR)) * kRtoD;

            return alpha;
        }

        private double SolarDeclination(DateTime aDateTime)
        {
            // Calculate the number of days from the epoch J2000.0
            double D = DaysSinceJ2000(aDateTime) + GreenwichMeanTime(aDateTime) / 24.0f;

            // Convert this into centuries
            double T = D / 36525.0f;

            // Calculate the obliquity
            double epsilon = 23.452f - 0.013f * T;

            // Calculate the declination, in degrees
            double delta = Math.Asin(Math.Sin(SolarRightAscension(aDateTime) * kDtoR) * Math.Sin(epsilon * kDtoR)) * kRtoD;

            return delta;
        }

        private double Altitude(double aHA, double aDelta, double aLatitude, double aLongitude)
        {
            // Calculate the altitude, in degrees
            double alt = Math.Asin(Math.Sin(aLatitude * kDtoR) * Math.Sin(aDelta * kDtoR) +
                Math.Cos(aLatitude * kDtoR) * Math.Cos(aDelta * kDtoR) * Math.Cos(aHA * kDtoR)) * kRtoD;

            return alt;
        }

        private const double kDtoR = Math.PI / 180.0f;
        private const double kRtoD = 180.0f / Math.PI;

        private const double kDayAltMin = 3.0f;    // Minimum solar altitude for daytime (0)
        private const double kNightAltMax = -3.0f; // Maximum solar altitude for nighttime (-9)

        private Bitmap iImageDay;
        private Bitmap iImageNight;
    }
}
