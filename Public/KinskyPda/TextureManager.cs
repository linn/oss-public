using System;
using System.Collections.Generic;
using System.Drawing;

using Linn.Kinsky;

namespace KinskyPda
{
    public class TextureManager
    {
        private TextureManager() { }

        public static TextureManager Instance
        {
            get
            {
                if (iInstance == null)
                {
                    iInstance = new TextureManager();
                }
                return iInstance;
            }
        }

        public Bitmap AuxSource
        {
            get
            {
                iAuxSource = DrawImage(iAuxSource, KinskyPda.Properties.Resources.AuxSource);
                return iAuxSource;
            }
        }

        public Bitmap Background
        {
            get
            {
                iBackground = DrawImage(iBackground, KinskyPda.Properties.Resources.Background);
                return iBackground;
            }
        }

        public Bitmap BlankControl
        {
            get
            {
                iBlankControl = DrawImage(iBlankControl, KinskyPda.Properties.Resources.BlankControl);
                return iBlankControl;
            }
        }

        public Bitmap IconAlbum
        {
            get
            {
                iIconAlbum = DrawImage(iIconAlbum, KinskyPda.Properties.Resources.IconAlbum);
                return iIconAlbum;
            }
        }

        public Bitmap IconArtist
        {
            get
            {
                iIconArtist = DrawImage(iIconArtist, KinskyPda.Properties.Resources.IconArtist);
                return iIconArtist;
            }
        }

        public Bitmap IconDirectory
        {
            get
            {
                iIconDirectory = DrawImage(iIconDirectory, KinskyPda.Properties.Resources.IconDirectory);
                return iIconDirectory;
            }
        }

        public Bitmap IconError
        {
            get
            {
                iIconError = DrawImage(iIconError, KinskyPda.Properties.Resources.IconError);
                return iIconError;
            }
        }

        public Bitmap IconNoAlbumArt
        {
            get
            {
                iIconNoAlbumArt = DrawImage(iIconNoAlbumArt, KinskyPda.Properties.Resources.IconNoAlbumArt);
                return iIconNoAlbumArt;
            }
        }

        public Bitmap IconPlaying
        {
            get
            {
                iIconPlaying = DrawImage(iIconPlaying, KinskyPda.Properties.Resources.IconPlaying);
                return iIconPlaying;
            }
        }

        public Bitmap IconPlaylist
        {
            get
            {
                iIconPlaylist = DrawImage(iIconPlaylist, KinskyPda.Properties.Resources.IconPlaylist);
                return iIconPlaylist;
            }
        }

        public Bitmap IconRadio
        {
            get
            {
                iIconRadio = DrawImage(iIconRadio, KinskyPda.Properties.Resources.IconRadio);
                return iIconRadio;
            }
        }

        public Bitmap IconSelectedServer
        {
            get
            {
                iIconSelectedServer = DrawImage(iIconSelectedServer, KinskyPda.Properties.Resources.IconSelectedServer);
                return iIconSelectedServer;
            }
        }

        public Bitmap IconServer
        {
            get
            {
                iIconServer = DrawImage(iIconDirectory, KinskyPda.Properties.Resources.IconServer);
                return iIconServer;
            }
        }

        public Bitmap IconTrack
        {
            get
            {
                iIconTrack = DrawImage(iIconTrack, KinskyPda.Properties.Resources.IconTrack);
                return iIconTrack;
            }
        }

        public Bitmap InsertPlay
        {
            get
            {
                iInsertPlay = DrawImage(iInsertPlay, KinskyPda.Properties.Resources.InsertPlay);
                return iInsertPlay;
            }
        }

        public Bitmap InsertPlayTouch
        {
            get
            {
                iInsertPlayTouch = DrawImage(iInsertPlayTouch, KinskyPda.Properties.Resources.InsertPlayTouch);
                return iInsertPlayTouch;
            }
        }

        public Bitmap InsertQueue
        {
            get
            {
                iInsertQueue = DrawImage(iInsertQueue, KinskyPda.Properties.Resources.InsertQueue);
                return iInsertQueue;
            }
        }

        public Bitmap InsertQueueTouch
        {
            get
            {
                iInsertQueueTouch = DrawImage(iInsertQueueTouch, KinskyPda.Properties.Resources.InsertQueueTouch);
                return iInsertQueueTouch;
            }
        }

        public Bitmap Mute
        {
            get
            {
                iMute = DrawImage(iMute, KinskyPda.Properties.Resources.Mute);
                return iMute;
            }
        }


        public Bitmap NoAlbumArt
        {
            get
            {
                iNoAlbumArt = DrawImage(iNoAlbumArt, KinskyPda.Properties.Resources.NoAlbumArt);
                return iNoAlbumArt;
            }
        }

        public Bitmap Pause
        {
            get
            {
                iPause = DrawImage(iPause, KinskyPda.Properties.Resources.Pause);
                return iPause;
            }
        }

        public Bitmap PauseTouch
        {
            get
            {
                iPauseTouch = DrawImage(iPauseTouch, KinskyPda.Properties.Resources.PauseTouch);
                return iPauseTouch;
            }
        }

        public Bitmap Play
        {
            get
            {
                iPlay = DrawImage(iPlay, KinskyPda.Properties.Resources.Play);
                return iPlay;
            }
        }

        public Bitmap PlayTouch
        {
            get
            {
                iPlayTouch = DrawImage(iPlayTouch, KinskyPda.Properties.Resources.PlayTouch);
                return iPlayTouch;
            }
        }

        public Bitmap Refresh
        {
            get
            {
                iRefresh = DrawImage(iRefresh, KinskyPda.Properties.Resources.Refresh);
                return iRefresh;
            }
        }

        public Bitmap RefreshTouch
        {
            get
            {
                iRefreshTouch = DrawImage(iRefreshTouch, KinskyPda.Properties.Resources.RefreshTouch);
                return iRefreshTouch;
            }
        }

        public Bitmap SkipBack
        {
            get
            {
                iSkipBack = DrawImage(iSkipBack, KinskyPda.Properties.Resources.SkipBack);
                return iSkipBack;
            }
        }

        public Bitmap SkipBackTouch
        {
            get
            {
                iSkipBackTouch = DrawImage(iSkipBackTouch, KinskyPda.Properties.Resources.SkipBackTouch);
                return iSkipBackTouch;
            }
        }

        public Bitmap SkipForward
        {
            get
            {
                iSkipForward = DrawImage(iSkipForward, KinskyPda.Properties.Resources.SkipForward);
                return iSkipForward;
            }
        }

        public Bitmap SkipForwardTouch
        {
            get
            {
                iSkipForwardTouch = DrawImage(iSkipForwardTouch, KinskyPda.Properties.Resources.SkipForwardTouch);
                return iSkipForwardTouch;
            }
        }

        public Bitmap Standby
        {
            get
            {
                iStandby = DrawImage(iStandby, KinskyPda.Properties.Resources.Standby);
                return iStandby;
            }
        }

        public Bitmap StandbyTouch
        {
            get
            {
                iStandbyTouch = DrawImage(iStandbyTouch, KinskyPda.Properties.Resources.StandbyTouch);
                return iStandbyTouch;
            }
        }

        public Bitmap Stop
        {
            get
            {
                iStop = DrawImage(iStop, KinskyPda.Properties.Resources.Stop);
                return iStop;
            }
        }

        public Bitmap StopTouch
        {
            get
            {
                iStopTouch = DrawImage(iStopTouch, KinskyPda.Properties.Resources.StopTouch);
                return iStopTouch;
            }
        }

        public Bitmap UpDirectory
        {
            get
            {
                iUpDirectory = DrawImage(iUpDirectory, KinskyPda.Properties.Resources.UpDirectory);
                return iUpDirectory;
            }
        }

        public Bitmap UpDirectoryTouch
        {
            get
            {
                iUpDirectoryTouch = DrawImage(iUpDirectoryTouch, KinskyPda.Properties.Resources.UpDirectoryTouch);
                return iUpDirectoryTouch;
            }
        }

        public Bitmap VolumeDown
        {
            get
            {
                iVolumeDown = DrawImage(iVolumeDown, KinskyPda.Properties.Resources.VolumeDown);
                return iVolumeDown;
            }
        }

        public Bitmap VolumeDownTouch
        {
            get
            {
                iVolumeDownTouch = DrawImage(iVolumeDownTouch, KinskyPda.Properties.Resources.VolumeDownTouch);
                return iVolumeDownTouch;
            }
        }

        public Bitmap VolumeUp
        {
            get
            {
                iVolumeUp = DrawImage(iVolumeUp, KinskyPda.Properties.Resources.VolumeUp);
                return iVolumeUp;
            }
        }

        public Bitmap VolumeUpTouch
        {
            get
            {
                iVolumeUpTouch = DrawImage(iVolumeUpTouch, KinskyPda.Properties.Resources.VolumeUpTouch);
                return iVolumeUpTouch;
            }
        }

        public Bitmap Waiting1
        {
            get
            {
                iWaiting1 = DrawImage(iWaiting1, KinskyPda.Properties.Resources.Waiting1);
                return iWaiting1;
            }
        }

        public Bitmap Waiting2
        {
            get
            {
                iWaiting2 = DrawImage(iWaiting2, KinskyPda.Properties.Resources.Waiting2);
                return iWaiting2;
            }
        }

        public Bitmap Waiting3
        {
            get
            {
                iWaiting3 = DrawImage(iWaiting3, KinskyPda.Properties.Resources.Waiting3);
                return iWaiting3;
            }
        }

        public Bitmap Waiting4
        {
            get
            {
                iWaiting4 = DrawImage(iWaiting4, KinskyPda.Properties.Resources.Waiting4);
                return iWaiting4;
            }
        }

        public Bitmap Waiting5
        {
            get
            {
                iWaiting5 = DrawImage(iWaiting5, KinskyPda.Properties.Resources.Waiting5);
                return iWaiting5;
            }
        }

        public Bitmap Waiting6
        {
            get
            {
                iWaiting6 = DrawImage(iWaiting6, KinskyPda.Properties.Resources.Waiting6);
                return iWaiting6;
            }
        }

        public Bitmap Waiting7
        {
            get
            {
                iWaiting7 = DrawImage(iWaiting7, KinskyPda.Properties.Resources.Waiting7);
                return iWaiting7;
            }
        }

        public Bitmap Waiting8
        {
            get
            {
                iWaiting8 = DrawImage(iWaiting8, KinskyPda.Properties.Resources.Waiting8);
                return iWaiting8;
            }
        }

        private Bitmap DrawImage(Bitmap aImage, Bitmap aResource)
        {
            if (aImage == null)
            {
                Bitmap temp = aResource;
                aImage = new Bitmap(DisplayManager.ScaleWidth(temp.Width), DisplayManager.ScaleHeight(temp.Height));
                Graphics g = Graphics.FromImage(aImage);
                g.DrawImage(temp, new Rectangle(0, 0, aImage.Width, aImage.Height), new Rectangle(0, 0, temp.Width, temp.Height), GraphicsUnit.Pixel);
                g.Dispose();
                temp.Dispose();
            }
            return aImage;
        }

        private static TextureManager iInstance;

        private Bitmap iAuxSource;
        private Bitmap iBackground;
        private Bitmap iBlankControl;
        private Bitmap iIconAlbum;
        private Bitmap iIconArtist;
        private Bitmap iIconDirectory;
        private Bitmap iIconError;
        private Bitmap iIconNoAlbumArt;
        private Bitmap iIconPlaying;
        private Bitmap iIconPlaylist;
        private Bitmap iIconRadio;
        private Bitmap iIconSelectedServer;
        private Bitmap iIconServer;
        private Bitmap iIconTrack;
        private Bitmap iInsertPlay;
        private Bitmap iInsertPlayTouch;
        private Bitmap iInsertQueue;
        private Bitmap iInsertQueueTouch;
        private Bitmap iMute;
        private Bitmap iNoAlbumArt;
        private Bitmap iPause;
        private Bitmap iPauseTouch;
        private Bitmap iPlay;
        private Bitmap iPlayTouch;
        private Bitmap iRefresh;
        private Bitmap iRefreshTouch;
        private Bitmap iSkipBack;
        private Bitmap iSkipBackTouch;
        private Bitmap iSkipForward;
        private Bitmap iSkipForwardTouch;
        private Bitmap iStandby;
        private Bitmap iStandbyTouch;
        private Bitmap iStop;
        private Bitmap iStopTouch;
        private Bitmap iUpDirectory;
        private Bitmap iUpDirectoryTouch;
        private Bitmap iVolumeDown;
        private Bitmap iVolumeDownTouch;
        private Bitmap iVolumeUp;
        private Bitmap iVolumeUpTouch;
        private Bitmap iWaiting1;
        private Bitmap iWaiting2;
        private Bitmap iWaiting3;
        private Bitmap iWaiting4;
        private Bitmap iWaiting5;
        private Bitmap iWaiting6;
        private Bitmap iWaiting7;
        private Bitmap iWaiting8;
    }
}
