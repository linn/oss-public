using System;
using System.Collections.Generic;
using System.Threading;

using SneakyMedia.Scanner;
using SneakyMedia.Database;

namespace SneakyMedia.Scanner
{
    public class TagLegacyTagLib : TagLegacy
    {
        public const string kNs = "taglib";

        public TagLegacyTagLib(string aName, Hypertag aHypertag)
            : base(kNs, aName)
        {
            iHypertag = aHypertag;
        }

        public Hypertag Hypertag
        {
            get
            {
                return (iHypertag);
            }
        }

        private Hypertag iHypertag;
    }

    public class ModuleScannerTagLib : ModuleScanner
    {

        public ModuleScannerTagLib()
            : base("TagLib#", new Version(2, 0, 0, 3))
        {
        }

        protected override void OnCreate()
        {
            CreateHypertags();

            CreateTags();

        }

        protected override void OnLoad()
        {
            CreateHypertags();

            CreateTags();
        }

        private void CreateHypertags()
        {
            iHypertagAlbum = new HypertagAv(HypertagAv.kNameAlbum);
            iHypertagArtist = new HypertagAv(HypertagAv.kNameArtist);
            iHypertagBeatsPerMinute = new HypertagAv(HypertagAv.kNameBeatsPerMinute);
            iHypertagComment = new HypertagAv(HypertagAv.kNameComment);
            iHypertagComposer = new HypertagAv(HypertagAv.kNameComposer);
            iHypertagConductor = new HypertagAv(HypertagAv.kNameConductor);
            iHypertagCopyright = new HypertagAv(HypertagAv.kNameCopyright);
            iHypertagDate = new HypertagAv(HypertagAv.kNameDate);
            iHypertagDiscNumber = new HypertagAv(HypertagAv.kNameDiscNumber);
            iHypertagDiscCount = new HypertagAv(HypertagAv.kNameDiscCount);
            iHypertagGenre = new HypertagAv(HypertagAv.kNameGenre);
            iHypertagGrouping = new HypertagAv(HypertagAv.kNameGrouping);
            iHypertagLyrics = new HypertagAv(HypertagAv.kNameLyrics);
            iHypertagPerformer = new HypertagAv(HypertagAv.kNamePerformer);
            iHypertagTitle = new HypertagAv(HypertagAv.kNameTitle);
            iHypertagTrackNumber = new HypertagAv(HypertagAv.kNameTrackNumber);
            iHypertagTrackCount = new HypertagAv(HypertagAv.kNameTrackCount);
        }

        private void CreateTags()
        {
            iTagAlbum = new TagLegacyTagLib("Album", iHypertagAlbum);
            iTagAlbumArtist = new TagLegacyTagLib("AlbumArtist", iHypertagArtist);
            iTagBeatsPerMinute = new TagLegacyTagLib("BeatsPerMinute", iHypertagBeatsPerMinute);
            iTagComment = new TagLegacyTagLib("Comment", iHypertagComment);
            iTagComposer = new TagLegacyTagLib("Composer", iHypertagComposer);
            iTagConductor = new TagLegacyTagLib("Conductor", iHypertagConductor);
            iTagCopyright = new TagLegacyTagLib("Copyright", iHypertagCopyright);
            iTagDisc = new TagLegacyTagLib("Disc", iHypertagDiscNumber);
            iTagDiscCount = new TagLegacyTagLib("DiscCount", iHypertagDiscCount);
            iTagGenre = new TagLegacyTagLib("Genre", iHypertagGenre);
            iTagGrouping = new TagLegacyTagLib("Grouping", iHypertagGrouping);
            iTagLyrics = new TagLegacyTagLib("Lyrics", iHypertagLyrics);
            iTagPerformer = new TagLegacyTagLib("Performer", iHypertagPerformer);
            iTagTitle = new TagLegacyTagLib("Title", iHypertagTitle);
            iTagTrack = new TagLegacyTagLib("Track", iHypertagTrackNumber);
            iTagTrackCount = new TagLegacyTagLib("TrackCount", iHypertagTrackCount);
            iTagYear = new TagLegacyTagLib("Year", iHypertagDate);
        }

        public override IList<IMetadatum> Scan(string aUri)
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
                List<IMetadatum> tags = new List<IMetadatum>();

                if (!String.IsNullOrEmpty(tag.Album))
                {
                    tags.Add(new Metadatum(iHypertagAlbum, tag.Album));
                    tags.Add(new Metadatum(iTagAlbum, tag.Album));
                }

                if (tag.AlbumArtists != null)
                {
                    if (tag.AlbumArtists.Length > 0)
                    {
                        foreach (string albumartist in tag.AlbumArtists)
                        {
                            if (albumartist.Length > 0)
                            {
                                tags.Add(new Metadatum(iTagComposer, albumartist));
                                tags.Add(new Metadatum(iHypertagComposer, albumartist));
                            }
                        }
                    }
                }

                if (tag.BeatsPerMinute != 0)
                {
                    string beatsperminute = tag.BeatsPerMinute.ToString();

                    tags.Add(new Metadatum(iTagBeatsPerMinute, beatsperminute));
                    tags.Add(new Metadatum(iHypertagBeatsPerMinute, beatsperminute));
                }

                if (!String.IsNullOrEmpty(tag.Comment))
                {
                    tags.Add(new Metadatum(iTagComment, tag.Comment));
                    tags.Add(new Metadatum(iHypertagComment, tag.Comment));
                }

                if (tag.Composers != null)
                {
                    if (tag.Composers.Length > 0)
                    {
                        foreach (string composer in tag.Composers)
                        {
                            if (composer.Length > 0)
                            {
                                tags.Add(new Metadatum(iTagComposer, composer));
                                tags.Add(new Metadatum(iHypertagComposer, composer));
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(tag.Conductor))
                {
                    tags.Add(new Metadatum(iTagConductor, tag.Conductor));
                    tags.Add(new Metadatum(iHypertagConductor, tag.Conductor));
                }

                if (!String.IsNullOrEmpty(tag.Copyright))
                {
                    tags.Add(new Metadatum(iTagCopyright, tag.Copyright));
                    tags.Add(new Metadatum(iHypertagCopyright, tag.Copyright));
                }

                if (tag.Disc != 0)
                {
                    string disc = tag.Disc.ToString();

                    tags.Add(new Metadatum(iTagDisc, disc));
                    tags.Add(new Metadatum(iHypertagDiscNumber, disc));
                }

                if (tag.DiscCount != 0)
                {
                    string disccount = tag.DiscCount.ToString();

                    tags.Add(new Metadatum(iTagDiscCount, disccount));
                    tags.Add(new Metadatum(iHypertagDiscCount, disccount));
                }

                if (tag.Genres != null)
                {
                    if (tag.Genres.Length > 0)
                    {
                        foreach (string genre in tag.Genres)
                        {
                            if (genre.Length > 0)
                            {
                                tags.Add(new Metadatum(iTagGenre, genre));
                                tags.Add(new Metadatum(iHypertagGenre, genre));
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(tag.Grouping))
                {
                    tags.Add(new Metadatum(iTagGrouping, tag.Grouping));
                    tags.Add(new Metadatum(iHypertagGrouping, tag.Grouping));
                }

                if (!String.IsNullOrEmpty(tag.Lyrics))
                {
                    tags.Add(new Metadatum(iTagLyrics, tag.Lyrics));
                    tags.Add(new Metadatum(iHypertagLyrics, tag.Lyrics));
                }

                if (tag.Performers != null)
                {
                    if (tag.Performers.Length > 0)
                    {
                        foreach (string performer in tag.Performers)
                        {
                            if (performer.Length > 0)
                            {
                                tags.Add(new Metadatum(iTagPerformer, performer));
                                tags.Add(new Metadatum(iHypertagPerformer, performer));
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(tag.Title))
                {
                    tags.Add(new Metadatum(iTagTitle, tag.Title));
                    tags.Add(new Metadatum(iHypertagTitle, tag.Title));
                }

                if (tag.Track != 0)
                {
                    string track = tag.Track.ToString();

                    tags.Add(new Metadatum(iTagTrack, track));
                    tags.Add(new Metadatum(iHypertagTrackNumber, track));
                }

                if (tag.TrackCount != 0)
                {
                    string trackcount = tag.TrackCount.ToString();

                    tags.Add(new Metadatum(iTagTrackCount, trackcount));
                    tags.Add(new Metadatum(iHypertagTrackCount, trackcount));
                }

                if (tag.Year != 0)
                {
                    string year = tag.Year.ToString();

                    tags.Add(new Metadatum(iTagYear, year));
                    tags.Add(new Metadatum(iHypertagDate, year));
                }

                return (tags);
            }

            return (null);
        }

        Hypertag iHypertagAlbum;
        Hypertag iHypertagArtist;
        Hypertag iHypertagBeatsPerMinute;
        Hypertag iHypertagComment;
        Hypertag iHypertagComposer;
        Hypertag iHypertagConductor;
        Hypertag iHypertagCopyright;
        Hypertag iHypertagDate;
        Hypertag iHypertagDiscNumber;
        Hypertag iHypertagDiscCount;
        Hypertag iHypertagGenre;
        Hypertag iHypertagGrouping;
        Hypertag iHypertagLyrics;
        Hypertag iHypertagPerformer;
        Hypertag iHypertagTitle;
        Hypertag iHypertagTrackNumber;
        Hypertag iHypertagTrackCount;

        Tag iTagAlbum;
        Tag iTagAlbumArtist;
        Tag iTagBeatsPerMinute;
        Tag iTagComment;
        Tag iTagComposer;
        Tag iTagConductor;
        Tag iTagCopyright;
        Tag iTagDisc;
        Tag iTagDiscCount;
        Tag iTagGenre;
        Tag iTagGrouping;
        Tag iTagLyrics;
        Tag iTagPerformer;
        Tag iTagTitle;
        Tag iTagTrack;
        Tag iTagTrackCount;
        Tag iTagYear;
    }
}
