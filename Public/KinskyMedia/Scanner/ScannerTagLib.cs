using System;
using System.Collections.Generic;
using System.Threading;

using KinskyMedia.Database;

namespace KinskyMedia.Scanner
{
    public class ScannerTagLib : IScanner
    {
        public const string kNs = "taglib";

        public ScannerTagLib()
        {
        }

        public void ManageTags(ITagManager aManager)
        {
            iTagAvAlbum = aManager.Find(TagAv.Ns, TagAv.NameAlbum);
            iTagAvArtist = aManager.Find(TagAv.Ns, TagAv.NameArtist);
            iTagAvBeatsPerMinute = aManager.Find(TagAv.Ns, TagAv.NameBeatsPerMinute);
            iTagAvComment = aManager.Find(TagAv.Ns, TagAv.NameComment);
            iTagAvComposer = aManager.Find(TagAv.Ns, TagAv.NameComposer);
            iTagAvConductor = aManager.Find(TagAv.Ns, TagAv.NameConductor);
            iTagAvCopyright = aManager.Find(TagAv.Ns, TagAv.NameCopyright);
            iTagAvDate = aManager.Find(TagAv.Ns, TagAv.NameDate);
            iTagAvDiscNumber = aManager.Find(TagAv.Ns, TagAv.NameDiscNumber);
            iTagAvDiscCount = aManager.Find(TagAv.Ns, TagAv.NameDiscCount);
            iTagAvGenre = aManager.Find(TagAv.Ns, TagAv.NameGenre);
            iTagAvGrouping = aManager.Find(TagAv.Ns, TagAv.NameGrouping);
            iTagAvLyrics = aManager.Find(TagAv.Ns, TagAv.NameLyrics);
            iTagAvPerformer = aManager.Find(TagAv.Ns, TagAv.NamePerformer);
            iTagAvTitle = aManager.Find(TagAv.Ns, TagAv.NameTitle);
            iTagAvTrackNumber = aManager.Find(TagAv.Ns, TagAv.NameTrackNumber);
            iTagAvTrackCount = aManager.Find(TagAv.Ns, TagAv.NameTrackCount);

            iTagAlbum = aManager.Create(kNs, "Album");
            iTagAlbumArtist = aManager.Create(kNs, "AlbumArtist");
            iTagBeatsPerMinute = aManager.Create(kNs, "BeatsPerMinute");
            iTagComment = aManager.Create(kNs, "Comment");
            iTagComposer = aManager.Create(kNs, "Composer");
            iTagConductor = aManager.Create(kNs, "Conductor");
            iTagCopyright = aManager.Create(kNs, "Copyright");
            iTagDisc = aManager.Create(kNs, "Disc");
            iTagDiscCount = aManager.Create(kNs, "DiscCount");
            iTagGenre = aManager.Create(kNs, "Genre");
            iTagGrouping = aManager.Create(kNs, "Grouping");
            iTagLyrics = aManager.Create(kNs, "Lyrics");
            iTagPerformer = aManager.Create(kNs, "Performer");
            iTagTitle = aManager.Create(kNs, "Title");
            iTagTrack = aManager.Create(kNs, "Track");
            iTagTrackCount = aManager.Create(kNs, "TrackCount");
            iTagYear = aManager.Create(kNs, "Year");
        }
        
        public IList<IMetadatum> Scan(string aUri)
        {
            TagLib.File file;

            try
            {
                file = TagLib.File.Create(aUri);
            }
            catch (TagLib.CorruptFileException)
            {
                return (null);
            }
            catch (TagLib.UnsupportedFormatException)
            {
                return (null);
            }

            TagLib.Tag tag = file.Tag;

            if (tag != null)
            {
                List<IMetadatum> metadata = new List<IMetadatum>();

                if (!String.IsNullOrEmpty(tag.Album))
                {
                    metadata.Add(new Metadatum(iTagAvAlbum, tag.Album));
                    metadata.Add(new Metadatum(iTagAlbum, tag.Album));
                }

                if (tag.AlbumArtists != null)
                {
                    if (tag.AlbumArtists.Length > 0)
                    {
                        foreach (string albumartist in tag.AlbumArtists)
                        {
                            if (albumartist.Length > 0)
                            {
                                metadata.Add(new Metadatum(iTagAlbumArtist, albumartist));
                                metadata.Add(new Metadatum(iTagAvArtist, albumartist));
                            }
                        }
                    }
                }

                if (tag.BeatsPerMinute != 0)
                {
                    string beatsperminute = tag.BeatsPerMinute.ToString();

                    metadata.Add(new Metadatum(iTagBeatsPerMinute, beatsperminute));
                    metadata.Add(new Metadatum(iTagAvBeatsPerMinute, beatsperminute));
                }

                if (!String.IsNullOrEmpty(tag.Comment))
                {
                    metadata.Add(new Metadatum(iTagComment, tag.Comment));
                    metadata.Add(new Metadatum(iTagAvComment, tag.Comment));
                }

                if (tag.Composers != null)
                {
                    if (tag.Composers.Length > 0)
                    {
                        foreach (string composer in tag.Composers)
                        {
                            if (composer.Length > 0)
                            {
                                metadata.Add(new Metadatum(iTagComposer, composer));
                                metadata.Add(new Metadatum(iTagAvComposer, composer));
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(tag.Conductor))
                {
                    metadata.Add(new Metadatum(iTagConductor, tag.Conductor));
                    metadata.Add(new Metadatum(iTagAvConductor, tag.Conductor));
                }

                if (!String.IsNullOrEmpty(tag.Copyright))
                {
                    metadata.Add(new Metadatum(iTagCopyright, tag.Copyright));
                    metadata.Add(new Metadatum(iTagAvCopyright, tag.Copyright));
                }

                if (tag.Disc != 0)
                {
                    string disc = tag.Disc.ToString();

                    metadata.Add(new Metadatum(iTagDisc, disc));
                    metadata.Add(new Metadatum(iTagAvDiscNumber, disc));
                }

                if (tag.DiscCount != 0)
                {
                    string disccount = tag.DiscCount.ToString();

                    metadata.Add(new Metadatum(iTagDiscCount, disccount));
                    metadata.Add(new Metadatum(iTagAvDiscCount, disccount));
                }

                if (tag.Genres != null)
                {
                    if (tag.Genres.Length > 0)
                    {
                        foreach (string genre in tag.Genres)
                        {
                            if (genre.Length > 0)
                            {
                                metadata.Add(new Metadatum(iTagGenre, genre));
                                metadata.Add(new Metadatum(iTagAvGenre, genre));
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(tag.Grouping))
                {
                    metadata.Add(new Metadatum(iTagGrouping, tag.Grouping));
                    metadata.Add(new Metadatum(iTagAvGrouping, tag.Grouping));
                }

                if (!String.IsNullOrEmpty(tag.Lyrics))
                {
                    metadata.Add(new Metadatum(iTagLyrics, tag.Lyrics));
                    metadata.Add(new Metadatum(iTagAvLyrics, tag.Lyrics));
                }

                if (tag.Performers != null)
                {
                    if (tag.Performers.Length > 0)
                    {
                        foreach (string performer in tag.Performers)
                        {
                            if (performer.Length > 0)
                            {
                                metadata.Add(new Metadatum(iTagPerformer, performer));
                                metadata.Add(new Metadatum(iTagAvPerformer, performer));
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(tag.Title))
                {
                    metadata.Add(new Metadatum(iTagTitle, tag.Title));
                    metadata.Add(new Metadatum(iTagAvTitle, tag.Title));
                }

                if (tag.Track != 0)
                {
                    string track = tag.Track.ToString();

                    metadata.Add(new Metadatum(iTagTrack, track));
                    metadata.Add(new Metadatum(iTagAvTrackNumber, track));
                }

                if (tag.TrackCount != 0)
                {
                    string trackcount = tag.TrackCount.ToString();

                    metadata.Add(new Metadatum(iTagTrackCount, trackcount));
                    metadata.Add(new Metadatum(iTagAvTrackCount, trackcount));
                }

                if (tag.Year != 0)
                {
                    string year = tag.Year.ToString();

                    metadata.Add(new Metadatum(iTagYear, year));
                    metadata.Add(new Metadatum(iTagAvDate, year));
                }

                return (metadata);
            }

            return (null);
        }

        ITag iTagAvAlbum;
        ITag iTagAvArtist;
        ITag iTagAvBeatsPerMinute;
        ITag iTagAvComment;
        ITag iTagAvComposer;
        ITag iTagAvConductor;
        ITag iTagAvCopyright;
        ITag iTagAvDate;
        ITag iTagAvDiscNumber;
        ITag iTagAvDiscCount;
        ITag iTagAvGenre;
        ITag iTagAvGrouping;
        ITag iTagAvLyrics;
        ITag iTagAvPerformer;
        ITag iTagAvTitle;
        ITag iTagAvTrackNumber;
        ITag iTagAvTrackCount;

        ITag iTagAlbum;
        ITag iTagAlbumArtist;
        ITag iTagBeatsPerMinute;
        ITag iTagComment;
        ITag iTagComposer;
        ITag iTagConductor;
        ITag iTagCopyright;
        ITag iTagDisc;
        ITag iTagDiscCount;
        ITag iTagGenre;
        ITag iTagGrouping;
        ITag iTagLyrics;
        ITag iTagPerformer;
        ITag iTagTitle;
        ITag iTagTrack;
        ITag iTagTrackCount;
        ITag iTagYear;
    }
}
