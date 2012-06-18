
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.IO;

using Linn;
using Linn.Kinsky;

using Upnp;

namespace KinskyPda.Widgets
{
    public class WidgetTrack : Panel, IViewWidgetTrack
    {
        private Mutex iMutex;
        private bool iOpen;

        private Font iFont;
        private Font iFontTitle;

        private Brush iBrushText;

        private Bitmap iNoAlbumArt;
        private Bitmap iBitmapBackground;
        private Bitmap iBackBuffer;

        private Uri iArtworkUri;
        private Image iArtwork;

        private string iTitle;
        private string iArtist;
        private string iAlbum;

        public WidgetTrack()
        {
            iMutex = new Mutex();

            iBrushText = new SolidBrush(Color.White);

            iFont = new Font("Tahoma", 7, FontStyle.Bold);
            iFontTitle = new Font("Tahoma", 8, FontStyle.Bold);

            iBitmapBackground = TextureManager.Instance.Background;
            iNoAlbumArt = TextureManager.Instance.NoAlbumArt;
        }

        public void Open()
        {
            iMutex.WaitOne();
            iOpen = true;
            iMutex.ReleaseMutex();
        }

        public void Close()
        {
            iMutex.WaitOne();

            iOpen = false;

            SetNoArtwork();
            iTitle = string.Empty;
            iArtist = string.Empty;
            iAlbum = string.Empty;

            iMutex.ReleaseMutex();

            Update();
        }

        public void Initialised()
        {
            Update();
        }

        public void SetItem(Upnp.upnpObject aObject)
        {
            iMutex.WaitOne();
            if (aObject != null)
            {
                iTitle = Upnp.DidlLiteAdapter.Title(aObject);
                iArtist = Upnp.DidlLiteAdapter.Artist(aObject);
                iAlbum = Upnp.DidlLiteAdapter.Album(aObject);

                iArtworkUri = Upnp.DidlLiteAdapter.ArtworkUri(aObject);

                if (iArtworkUri == null)
                {
                    SetNoArtwork();
                }
                else
                {
                    DownloadArtwork(iArtworkUri);
                }
            }
            else
            {
                iTitle = string.Empty;
                iArtist = string.Empty;
                iAlbum = string.Empty;

                iArtworkUri = null;
                SetNoArtwork();
            }
            iMutex.ReleaseMutex();

            Update();
        }

        public void SetMetatext(Upnp.upnpObject aObject)
        {
            iMutex.WaitOne();
            
            if (aObject != null)
            {
                iAlbum = DidlLiteAdapter.Title(aObject);
                iArtist = string.Empty;
            }
            
            iMutex.ReleaseMutex();
            
            Update();
        }

        private delegate void UpdateDelegate();
        public new void Update()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new UpdateDelegate(Update));
            }
            else
            {
                Graphics graphics = Graphics.FromImage(iBackBuffer);
                DrawDoubleBuffered(graphics);
                graphics.Dispose();

                Invalidate();
            }
        }

        private void DrawDoubleBuffered(Graphics aGraphics)
        {
            DrawBackground(aGraphics);

            iMutex.WaitOne();

            if (iOpen)
            {
                if (!string.IsNullOrEmpty(iTitle))
                {
                    DrawMetaData(aGraphics);
                }

                Image artwork = (iArtwork != null) ? iArtwork : iNoAlbumArt;
                DrawAlbumArt(aGraphics, artwork);
            }
            
            iMutex.ReleaseMutex();
        }

        public void SetBitrate(uint aBitrate)
        {
        }

        public void SetSampleRate(float aSampleRate)
        {
        }

        public void SetBitDepth(uint aBitDepth)
        {
        }

        public void SetCodec(string aCodec)
        {
        }

        public void SetLossless(bool aLossless)
        {
        }

        private void DownloadArtwork(Uri aAlbumArtUri)
        {
            try
            {
                HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(aAlbumArtUri);
                wreq.Proxy = new WebProxy();
                wreq.KeepAlive = false;
                wreq.Pipelined = false;
                wreq.ContentLength = 0;
                wreq.AllowWriteStreamBuffering = true;
                WebRequestPool.QueueJob(new JobGetResponse(GetResponseCallback, wreq));
            }
            catch (Exception e)
            {
                Console.WriteLine("ViewWidgetTrack.DownloadArtwork: " + e.Message);
            }
        }

        private void GetResponseCallback(object aResult)
        {
            HttpWebRequest wreq = aResult as HttpWebRequest;
            HttpWebResponse wresp = null;
            Stream stream = null;

            try
            {
                wresp = (HttpWebResponse)wreq.GetResponse();
                stream = wresp.GetResponseStream();

                if (stream != null)
                {

                    Bitmap image = new Bitmap(stream);

                    iMutex.WaitOne();

                    if (wreq.RequestUri.AbsoluteUri == iArtworkUri.AbsoluteUri)
                    {
                        iArtwork = image;

                        iMutex.ReleaseMutex();

                        Update();
                    }
                    else
                    {
                        SetNoArtwork();

                        iMutex.ReleaseMutex();

                        image.Dispose();

                        Update();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ViewWidgetTrack.GetResponseCallback: " + e.Message);
            }
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

        private void SetNoArtwork()
        {
            if (iArtwork != null)
            {
                iArtwork.Dispose();
            }
            iArtwork = null;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (iBackBuffer != null)
            {
                iBackBuffer.Dispose();
                iBackBuffer = null;
            }
            iBackBuffer = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
            Update();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            pe.Graphics.DrawImage(iBackBuffer, 0, 0);
        }

        private void DrawAlbumArt(Graphics aGraphics, Image aArtwork)
        {
            Rectangle rect;
            Screen screen = Screen.PrimaryScreen;
            if (screen.Bounds.Width > screen.Bounds.Height)
            {
                rect = DisplayManager.ScaleRectangle(30, 30, 228, 228);
            }
            else
            {
                rect = DisplayManager.ScaleRectangle(90, 30, 300, 300);
            }
            aGraphics.DrawImage(aArtwork, rect, new Rectangle(0, 0, aArtwork.Width, aArtwork.Height), GraphicsUnit.Pixel);
        }

        private void DrawBackground(Graphics aGraphics)
        {
            SizeF scale = DisplayManager.ScaleFactorOriented;
            float ratioW = scale.Width / scale.Height;

            float ratioH = 1.0f;
            if (ratioW > 1.0f)
            {
                ratioH = 1.0f / ratioW;
                ratioW = 1.0f;
            }
            
            aGraphics.DrawImage(iBitmapBackground,
                new Rectangle(0, 0, ClientSize.Width, ClientSize.Height),
                new Rectangle(0, 0, (int)(iBitmapBackground.Width * ratioW), (int)(iBitmapBackground.Height * ratioH)),
                GraphicsUnit.Pixel);
        }

        private void DrawMetaData(Graphics aGraphics)
        {
            Screen screen = Screen.PrimaryScreen;
            if (screen.Bounds.Width > screen.Bounds.Height)
            {
                DrawMetaDataLandscape(aGraphics);
            }
            else
            {
                DrawMetaDataPortrate(aGraphics);
            }
        }

        private void DrawMetaDataLandscape(Graphics aGraphics)
        {
            int width = DisplayManager.ScaleWidth(640);
            int height = DisplayManager.ScaleHeight(288);

            Rectangle rect = new Rectangle(height, 0, width - 30, 0);

            StringFormat stringFormat = new StringFormat();
            stringFormat.FormatFlags = StringFormatFlags.NoWrap;
            stringFormat.Alignment = StringAlignment.Near;

            SizeF fontSize = aGraphics.MeasureString(iTitle, iFontTitle);
            rect.Y = DisplayManager.ScaleHeight(30);
            rect.Height = (int)fontSize.Height;

            aGraphics.DrawString(iTitle, iFontTitle, iBrushText, rect, stringFormat);

            fontSize = aGraphics.MeasureString(iArtist, iFont);
            rect.Y += (int)fontSize.Height + 2;

            aGraphics.DrawString(iArtist, iFont, iBrushText, rect, stringFormat);

            fontSize = aGraphics.MeasureString(iAlbum, iFont);
            rect.Y += (int)fontSize.Height;

            aGraphics.DrawString(iAlbum, iFont, iBrushText, rect, stringFormat);
        }

        private void DrawMetaDataPortrate(Graphics aGraphics)
        {
            int width = DisplayManager.ScaleWidth(480);
            int height = DisplayManager.ScaleHeight(588);

            Rectangle rect = new Rectangle(8, 0, width - 16, 0);

            //centre fomatting
            StringFormat stringFormat = new StringFormat();
            stringFormat.FormatFlags = StringFormatFlags.NoWrap;
            stringFormat.Alignment = StringAlignment.Center;

            SizeF fontSize = aGraphics.MeasureString(iTitle, iFontTitle);
            if (fontSize.Width < width)
            {
                stringFormat.Alignment = StringAlignment.Center;
            }
            else
            {
                stringFormat.Alignment = StringAlignment.Near;
            }
            rect.Y = DisplayManager.ScaleHeight(330) + (int)fontSize.Height;
            rect.Height = (int)fontSize.Height;

            aGraphics.DrawString(iTitle, iFontTitle, iBrushText, rect, stringFormat);

            fontSize = aGraphics.MeasureString(iArtist, iFont);
            if (fontSize.Width < width)
            {
                stringFormat.Alignment = StringAlignment.Center;
            }
            else
            {
                stringFormat.Alignment = StringAlignment.Near;
            }
            rect.Y += (int)fontSize.Height + 2;

            aGraphics.DrawString(iArtist, iFont, iBrushText, rect, stringFormat);

            fontSize = aGraphics.MeasureString(iAlbum, iFont);
            if (fontSize.Width < width)
            {
                stringFormat.Alignment = StringAlignment.Center;
            }
            else
            {
                stringFormat.Alignment = StringAlignment.Near;
            }
            rect.Y += (int)fontSize.Height;

            aGraphics.DrawString(iAlbum, iFont, iBrushText, rect, stringFormat);
        }

        //override for double buffering
        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }
    }
}
