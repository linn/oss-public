using System;
using System.IO;

using Gdk;

namespace KinskyDesktopGtk.Properties
{
    internal class ResourceManager
    {
        internal ResourceManager() {
        }

        internal static Pixbuf GetObject(string aName) {
            string filename = aName + ".png";
            return new Pixbuf(filename);
        }

        private static Pixbuf iImageIcon;
        internal static Pixbuf Icon
        {
            get
            {
                if(iImageIcon == null)
                {
                    iImageIcon = GetObject("KinskyLogoAbout");
                }
                return iImageIcon;
            }
        }

        private static Pixbuf iImageButton;
        internal static Pixbuf Button
        {
            get
            {
                if(iImageButton == null)
                {
                    iImageButton = GetObject("Button");
                }
                return iImageButton;
            }
        }

        private static Pixbuf iImageAlbum;
        internal static Pixbuf Album
        {
            get
            {
                if(iImageAlbum == null)
                {
                    iImageAlbum = GetObject("Album");
                }
                return iImageAlbum;
            }
        }

        private static Pixbuf iImageAlbumError;
        internal static Pixbuf AlbumError
        {
            get
            {
                if(iImageAlbumError == null)
                {
                    iImageAlbumError = GetObject("AlbumArtError");
                }
                return iImageAlbumError;
            }
        }

        private static Pixbuf iImageArtist;
        internal static Pixbuf Artist
        {
            get
            {
                if(iImageArtist == null)
                {
                    iImageArtist = GetObject("Artist");
                }
                return iImageArtist;
            }
        }

        private static Pixbuf iImageDirectory;
        internal static Pixbuf Directory
        {
            get
            {
                if(iImageDirectory == null)
                {
                    iImageDirectory = GetObject("Directory");
                }
                return iImageDirectory;
            }
        }

        private static Pixbuf iImageLibrary;
        internal static Pixbuf Library
        {
            get
            {
                if(iImageLibrary == null)
                {
                    iImageLibrary = GetObject("Library");
                }
                return iImageLibrary;
            }
        }

        private static Pixbuf iImageList;
        internal static Pixbuf List
        {
            get
            {
                if(iImageList == null)
                {
                    iImageList = GetObject("List");
                }
                return iImageList;
            }
        }

        private static Pixbuf iImagePlaylist;
        internal static Pixbuf Playlist
        {
            get
            {
                if(iImagePlaylist == null)
                {
                    iImagePlaylist = GetObject("Playlist");
                }
                return iImagePlaylist;
            }
        }

        private static Pixbuf iImagePlaylistItem;
        internal static Pixbuf PlaylistItem
        {
            get
            {
                if(iImagePlaylistItem == null)
                {
                    iImagePlaylistItem = GetObject("PlaylistItem");
                }
                return iImagePlaylistItem;
            }
        }

        private static Pixbuf iImageTrack;
        internal static Pixbuf Track
        {
            get
            {
                if(iImageTrack == null)
                {
                    iImageTrack = GetObject("Track");
                }
                return iImageTrack;
            }
        }

        internal static Pixbuf Radio
        {
            get
            {
                return SourceRadio;
            }
        }

        private static Pixbuf iImageVideo;
        internal static Pixbuf Video
        {
            get
            {
                if(iImageVideo == null)
                {
                    iImageVideo = GetObject("Video");
                }
                return iImageVideo;
            }
        }

        private static Pixbuf iImageDisclosure;
        internal static Pixbuf Disclosure
        {
            get
            {
                if(iImageDisclosure == null)
                {
                    iImageDisclosure = GetObject("DisclosureIndicator");
                }
                return iImageDisclosure;
            }
        }

        private static Pixbuf iImageLoading;
        internal static Pixbuf Loading
        {
            get
            {
                if(iImageLoading == null)
                {
                    iImageLoading = GetObject("Loading");
                }
                return iImageLoading;
            }
        }

        private static Pixbuf iImageRoom;
        internal static Pixbuf Room
        {
            get
            {
                if(iImageRoom == null)
                {
                    iImageRoom = GetObject("Room");
                }
                return iImageRoom;
            }
        }

        private static Pixbuf iImageSourceExternal;
        internal static Pixbuf SourceExternal
        {
            get
            {
                if(iImageSourceExternal == null)
                {
                    iImageSourceExternal = GetObject("Source");
                }
                return iImageSourceExternal;
            }
        }

        private static Pixbuf iImageSourceDisc;
        internal static Pixbuf SourceDisc
        {
            get
            {
                if(iImageSourceDisc == null)
                {
                    iImageSourceDisc = GetObject("CD");
                }
                return iImageSourceDisc;
            }
        }

        private static Pixbuf iImageSourcePlaylist;
        internal static Pixbuf SourcePlaylist
        {
            get
            {
                if(iImageSourcePlaylist == null)
                {
                    iImageSourcePlaylist = GetObject("PlaylistSource");
                }
                return iImageSourcePlaylist;
            }
        }

        private static Pixbuf iImageSourceRadio;
        internal static Pixbuf SourceRadio
        {
            get
            {
                if(iImageSourceRadio == null)
                {
                    iImageSourceRadio = GetObject("Radio");
                }
                return iImageSourceRadio;
            }
        }

        private static Pixbuf iImageSourceSongcast;
        internal static Pixbuf SourceSongcast
        {
            get
            {
                if(iImageSourceSongcast == null)
                {
                    iImageSourceSongcast = GetObject("Sender");
                }
                return iImageSourceSongcast;
            }
        }

        private static Pixbuf iImageSourceSongcastNotSending;
        internal static Pixbuf SourceSongcastNotSending
        {
            get
            {
                if(iImageSourceSongcastNotSending == null)
                {
                    iImageSourceSongcastNotSending = GetObject("SenderNoReceive");
                }
                return iImageSourceSongcastNotSending;
            }
        }

        private static Pixbuf iImageSourceUpnpAv;
        internal static Pixbuf SourceUpnpAv
        {
            get
            {
                if(iImageSourceUpnpAv == null)
                {
                    iImageSourceUpnpAv = GetObject("UPNP");
                }
                return iImageSourceUpnpAv;
            }
        }

        private static Pixbuf iImageWheel;
        internal static Pixbuf Wheel
        {
            get
            {
                if(iImageWheel == null)
                {
                    iImageWheel = GetObject("Wheel");
                }
                return iImageWheel;
            }
        }

        private static Pixbuf iImageWheelLarge;
        internal static Pixbuf WheelLarge
        {
            get
            {
                if(iImageWheelLarge == null)
                {
                    iImageWheelLarge = GetObject("WheelLarge");
                }
                return iImageWheelLarge;
            }
        }

        private static Pixbuf iImageWheelLargeOver;
        internal static Pixbuf WheelLargeOver
        {
            get
            {
                if(iImageWheelLargeOver == null)
                {
                    iImageWheelLargeOver = GetObject("WheelLargeOver");
                }
                return iImageWheelLargeOver;
            }
        }

        private static Pixbuf iImageWheelGripLarge;
        internal static Pixbuf WheelGripLarge
        {
            get
            {
                if(iImageWheelGripLarge == null)
                {
                    iImageWheelGripLarge = GetObject("ScrewsLarge");
                }
                return iImageWheelGripLarge;
            }
        }

        private static Pixbuf iImageWheelMute;
        internal static Pixbuf WheelMute
        {
            get
            {
                if(iImageWheelMute == null)
                {
                    iImageWheelMute = GetObject("WheelMute");
                }
                return iImageWheelMute;
            }
        }

        private static Pixbuf iImageMute;
        internal static Pixbuf Mute
        {
            get
            {
                if(iImageMute == null)
                {
                    iImageMute = GetObject("Mute");
                }
                return iImageMute;
            }
        }

        private static Pixbuf iImageMuteActive;
        internal static Pixbuf MuteActive
        {
            get
            {
                if(iImageMuteActive == null)
                {
                    iImageMuteActive = GetObject("MuteActive");
                }
                return iImageMuteActive;
            }
        }

        private static Pixbuf iImageClockElapsed;
        internal static Pixbuf ClockElapsed
        {
            get
            {
                if(iImageClockElapsed == null)
                {
                    iImageClockElapsed = GetObject("ClockIconElapsed");
                }
                return iImageClockElapsed;
            }
        }

        private static Pixbuf iImageClockRemaining;
        internal static Pixbuf ClockRemaining
        {
            get
            {
                if(iImageClockRemaining == null)
                {
                    iImageClockRemaining = GetObject("ClockIconRemaining");
                }
                return iImageClockRemaining;
            }
        }

        private static Pixbuf iImageTopLeftEdge;
        internal static Pixbuf ImageTopLeftEdge
        {
            get
            {
                if(iImageTopLeftEdge == null)
                {
                    iImageTopLeftEdge = GetObject("TopLeftEdge");
                }
                return iImageTopLeftEdge;
            }
        }

        private static Pixbuf iImageTopFiller;
        internal static Pixbuf ImageTopFiller
        {
            get
            {
                if(iImageTopFiller == null)
                {
                    iImageTopFiller = GetObject("TopFiller");
                }
                return iImageTopFiller;
            }
        }

        private static Pixbuf iImageTopRightEdge;
        internal static Pixbuf ImageTopRightEdge
        {
            get
            {
                if(iImageTopRightEdge == null)
                {
                    iImageTopRightEdge = GetObject("TopRightEdge");
                }
                return iImageTopRightEdge;
            }
        }

        private static Pixbuf iImageBottomLeftEdge;
        internal static Pixbuf ImageBottomLeftEdge
        {
            get
            {
                if(iImageBottomLeftEdge == null)
                {
                    iImageBottomLeftEdge = GetObject("BottomLeftEdge");
                }
                return iImageBottomLeftEdge;
            }
        }

        private static Pixbuf iImageBottomFiller;
        internal static Pixbuf ImageBottomFiller
        {
            get
            {
                if(iImageBottomFiller == null)
                {
                    iImageBottomFiller = GetObject("BottomFiller");
                }
                return iImageBottomFiller;
            }
        }

        private static Pixbuf iImageBottomRightEdge;
        internal static Pixbuf ImageBottomRightEdge
        {
            get
            {
                if(iImageBottomRightEdge == null)
                {
                    iImageBottomRightEdge = GetObject("BottomRightEdge");
                }
                return iImageBottomRightEdge;
            }
        }

        private static Pixbuf iImageLeftFiller;
        internal static Pixbuf ImageLeftFiller
        {
            get
            {
                if(iImageLeftFiller == null)
                {
                    iImageLeftFiller = GetObject("LeftFiller");
                }
                return iImageLeftFiller;
            }
        }

        private static Pixbuf iImageRightFiller;
        internal static Pixbuf ImageRightFiller
        {
            get
            {
                if(iImageRightFiller == null)
                {
                    iImageRightFiller = GetObject("RightFiller");
                }
                return iImageRightFiller;
            }
        }
    }
}

