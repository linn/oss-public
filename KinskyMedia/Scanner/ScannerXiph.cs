using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

using Luminescence.Xiph;

using KinskyMedia.Database;

namespace KinskyMedia.Scanner
{
    public class TagVorbisComment
    {
        public TagVorbisComment(ITag aAvTag, ITag aVorbisTag)
        {
            iAvTag = aAvTag;
            iVorbisTag = aVorbisTag;
        }

        public ITag AvTag
        {
            get
            {
                return (iAvTag);
            }
        }

        public ITag VorbisTag
        {
            get
            {
                return (iVorbisTag);
            }
        }

        ITag iAvTag;
        ITag iVorbisTag;
    }

    public class ScannerXiph : IScanner
    {
        public const string kNsVorbis = "vorbis";
        public const string kNsFlac = "flac";
        public const string kNsOgg = "ogg";

        public ScannerXiph()
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

            iTagAvVersion = aManager.Find(TagAv.Ns, TagAv.NameVersion);
            iTagAvLicense = aManager.Find(TagAv.Ns, TagAv.NameLicense);
            iTagAvOrganisation = aManager.Find(TagAv.Ns, TagAv.NameOrganisation);
            iTagAvDescription = aManager.Find(TagAv.Ns, TagAv.NameDescription);
            iTagAvLocation = aManager.Find(TagAv.Ns, TagAv.NameLocation);
            iTagAvContact = aManager.Find(TagAv.Ns, TagAv.NameContact);
            iTagAvIsrc = aManager.Find(TagAv.Ns, TagAv.NameIsrc);

            iTagAvChannels = aManager.Find(TagAv.Ns, TagAv.NameChannels);
            iTagAvSamples = aManager.Find(TagAv.Ns, TagAv.NameSamples);
            iTagAvSampleRate = aManager.Find(TagAv.Ns, TagAv.NameSampleRate);
            iTagAvBitsPerSample = aManager.Find(TagAv.Ns, TagAv.NameBitsPerSample);

            // Vorbis comments

            iTagVorbisTitle = aManager.Create(kNsVorbis, TagAv.NameTitle);
            iTagVorbisVersion = aManager.Create(kNsVorbis, TagAv.NameVersion);
            iTagVorbisAlbum = aManager.Create(kNsVorbis, TagAv.NameAlbum);
            iTagVorbisTrackNumber = aManager.Create(kNsVorbis, TagAv.NameTrackNumber);
            iTagVorbisArtist = aManager.Create(kNsVorbis, TagAv.NameArtist);
            iTagVorbisPerformer = aManager.Create(kNsVorbis, TagAv.NamePerformer);
            iTagVorbisCopyright = aManager.Create(kNsVorbis, TagAv.NameCopyright);
            iTagVorbisLicense = aManager.Create(kNsVorbis, TagAv.NameLicense);
            iTagVorbisOrganisation = aManager.Create(kNsVorbis, TagAv.NameOrganisation);
            iTagVorbisDescription = aManager.Create(kNsVorbis, TagAv.NameDescription);
            iTagVorbisGenre = aManager.Create(kNsVorbis, TagAv.NameGenre);
            iTagVorbisDate = aManager.Create(kNsVorbis, TagAv.NameDate);
            iTagVorbisLocation = aManager.Create(kNsVorbis, TagAv.NameLocation);
            iTagVorbisContact = aManager.Create(kNsVorbis, TagAv.NameContact);
            iTagVorbisIsrc = aManager.Create(kNsVorbis, TagAv.NameIsrc);

            // Ogg technical

            iTagOggChannels = aManager.Create(kNsOgg, "Channels");
            iTagOggSamples = aManager.Create(kNsOgg, "Samples");
            iTagOggSampleRate = aManager.Create(kNsOgg, "SampleRate");
            
            // Flac technical

            iTagFlacBitsPerSample = aManager.Create(kNsFlac, "BitsPerSample");

            // Searchable table of vorbis comments

            iTagVorbisComments = new Dictionary<string, TagVorbisComment>();

            AddVorbisComment(iTagAvTitle, iTagVorbisTitle);
            AddVorbisComment(iTagAvVersion, iTagVorbisVersion);
            AddVorbisComment(iTagAvAlbum, iTagVorbisAlbum);
            AddVorbisComment(iTagAvTrackNumber, iTagVorbisTrackNumber);
            AddVorbisComment(iTagAvArtist, iTagVorbisArtist);
            AddVorbisComment(iTagAvPerformer, iTagVorbisPerformer);
            AddVorbisComment(iTagAvCopyright, iTagVorbisCopyright);
            AddVorbisComment(iTagAvLicense, iTagVorbisLicense);
            AddVorbisComment(iTagAvOrganisation, iTagVorbisOrganisation);
            AddVorbisComment(iTagAvDescription, iTagVorbisDescription);
            AddVorbisComment(iTagAvGenre, iTagVorbisGenre);
            AddVorbisComment(iTagAvDate, iTagVorbisDate);
            AddVorbisComment(iTagAvLocation, iTagVorbisLocation);
            AddVorbisComment(iTagAvContact, iTagVorbisContact);
            AddVorbisComment(iTagAvIsrc, iTagVorbisIsrc);
        }
        
        private void AddVorbisComment(ITag aAvTag, ITag aVorbisTag)
        {
            iTagVorbisComments.Add(aAvTag.Name.ToUpperInvariant(), new TagVorbisComment(aAvTag, aVorbisTag));
        }

        public IList<IMetadatum> Scan(string aUri)
        {
            FlacTagger flac;

            try
            {
                flac = new FlacTagger(aUri);

                return ProcessFlac(flac);
            }
            catch (FileFormatException)
            {
                OggTagger ogg;

                try
                {
                    ogg = new OggTagger(aUri);
                }
                catch (FileFormatException)
                {
                    return (null);
                }

                switch (ogg.Codec)
                {
                    case OggCodec.FLAC:
                        return ProcessOggFlac(ogg);
                    case OggCodec.Vorbis:
                        return ProcessOggVorbis(ogg);
                    case OggCodec.Speex:
                        return ProcessOggSpeex(ogg);
                    default:
                        return (null);
                }
            }
        }

        IList<IMetadatum> ProcessFlac(FlacTagger aFile)
        {
            List<IMetadatum> metadata = new List<IMetadatum>();

            AddVorbisComments(aFile, metadata);

            // Add Flac Technical Tags

            string bitspersample = aFile.BitsPerSample.ToString();

            metadata.Add(new Metadatum(iTagAvBitsPerSample, bitspersample));
            metadata.Add(new Metadatum(iTagFlacBitsPerSample, bitspersample));

            // Add Ogg Technical Tags

            string channels = aFile.Channels.ToString();
            string samples = aFile.Samples.ToString();
            string samplerate = aFile.SampleRate.ToString();

            metadata.Add(new Metadatum(iTagAvChannels, channels));
            metadata.Add(new Metadatum(iTagAvSamples, samples));
            metadata.Add(new Metadatum(iTagAvSampleRate, samplerate));

            metadata.Add(new Metadatum(iTagOggChannels, channels));
            metadata.Add(new Metadatum(iTagOggSamples, samples));
            metadata.Add(new Metadatum(iTagOggSampleRate, samplerate));

            return (metadata);
        }

        IList<IMetadatum> ProcessOggFlac(OggTagger aFile)
        {
            List<IMetadatum> metadata = new List<IMetadatum>();

            AddVorbisComments(aFile, metadata);

            // Add Flac Technical Tags

            string bitspersample = aFile.FlacBitsPerSample.ToString();

            metadata.Add(new Metadatum(iTagAvBitsPerSample, bitspersample));
            metadata.Add(new Metadatum(iTagFlacBitsPerSample, bitspersample));

            // Add Ogg Technical Tags

            string channels = aFile.Channels.ToString();
            string samples = aFile.Samples.ToString();
            string samplerate = aFile.SampleRate.ToString();

            metadata.Add(new Metadatum(iTagAvChannels, channels));
            metadata.Add(new Metadatum(iTagAvSamples, samples));
            metadata.Add(new Metadatum(iTagAvSampleRate, samplerate));

            metadata.Add(new Metadatum(iTagOggChannels, channels));
            metadata.Add(new Metadatum(iTagOggSamples, samples));
            metadata.Add(new Metadatum(iTagOggSampleRate, samplerate));

            return (metadata);
        }

        IList<IMetadatum> ProcessOggVorbis(OggTagger aFile)
{
            List<IMetadatum> metadata = new List<IMetadatum>();

            AddVorbisComments(aFile, metadata);

            // Add Ogg Technical Tags

            string channels = aFile.Channels.ToString();
            string samples = aFile.Samples.ToString();
            string samplerate = aFile.SampleRate.ToString();

            metadata.Add(new Metadatum(iTagAvChannels, channels));
            metadata.Add(new Metadatum(iTagAvSamples, samples));
            metadata.Add(new Metadatum(iTagAvSampleRate, samplerate));

            metadata.Add(new Metadatum(iTagOggChannels, channels));
            metadata.Add(new Metadatum(iTagOggSamples, samples));
            metadata.Add(new Metadatum(iTagOggSampleRate, samplerate));

            return (metadata);
        }

        IList<IMetadatum> ProcessOggSpeex(OggTagger aFile)
        {
            List<IMetadatum> metadata = new List<IMetadatum>();

            AddVorbisComments(aFile, metadata);

            // Add Ogg Technical Tags

            string channels = aFile.Channels.ToString();
            string samples = aFile.Samples.ToString();
            string samplerate = aFile.SampleRate.ToString();

            metadata.Add(new Metadatum(iTagAvChannels, channels));
            metadata.Add(new Metadatum(iTagAvSamples, samples));
            metadata.Add(new Metadatum(iTagAvSampleRate, samplerate));

            metadata.Add(new Metadatum(iTagOggChannels, channels));
            metadata.Add(new Metadatum(iTagOggSamples, samples));
            metadata.Add(new Metadatum(iTagOggSampleRate, samplerate));

            return (metadata);
        }

        void AddVorbisComments(VorbisComment aFile, List<IMetadatum> aTags)
        {
            SortedList<string, List<string>> scanned = aFile.GetAllTags();

            foreach (KeyValuePair<string, List<string>> entry in scanned)
            {
                TagVorbisComment tag;

                iTagVorbisComments.TryGetValue(entry.Key.ToUpperInvariant(), out tag);

                if (tag != null)
                {
                    foreach (string value in entry.Value)
                    {
                        if (value.Length > 0)
                        {
                            aTags.Add(new Metadatum(tag.AvTag, value));
                            aTags.Add(new Metadatum(tag.VorbisTag, value));
                        }
                    }
                }
            }
        }

        private Dictionary<string, TagVorbisComment> iTagVorbisComments;

        private ITag iTagAvAlbum;
        private ITag iTagAvArtist;
        private ITag iTagAvBeatsPerMinute;
        private ITag iTagAvComment;
        private ITag iTagAvComposer;
        private ITag iTagAvConductor;
        private ITag iTagAvCopyright;
        private ITag iTagAvDate;
        private ITag iTagAvDiscNumber;
        private ITag iTagAvDiscCount;
        private ITag iTagAvGenre;
        private ITag iTagAvGrouping;
        private ITag iTagAvLyrics;
        private ITag iTagAvPerformer;
        private ITag iTagAvTitle;
        private ITag iTagAvTrackNumber;
        private ITag iTagAvTrackCount;

        private ITag iTagAvChannels;
        private ITag iTagAvSamples;
        private ITag iTagAvSampleRate;
        private ITag iTagAvBitsPerSample;
        
        private ITag iTagAvVersion;
        private ITag iTagAvLicense;
        private ITag iTagAvOrganisation;
        private ITag iTagAvDescription;
        private ITag iTagAvLocation;
        private ITag iTagAvContact;
        private ITag iTagAvIsrc;

        // Vorbis comments

        private ITag iTagVorbisTitle;
        private ITag iTagVorbisVersion;
        private ITag iTagVorbisAlbum;
        private ITag iTagVorbisTrackNumber;
        private ITag iTagVorbisArtist;
        private ITag iTagVorbisPerformer;
        private ITag iTagVorbisCopyright;
        private ITag iTagVorbisLicense;
        private ITag iTagVorbisOrganisation;
        private ITag iTagVorbisDescription;
        private ITag iTagVorbisGenre;
        private ITag iTagVorbisDate;
        private ITag iTagVorbisLocation;
        private ITag iTagVorbisContact;
        private ITag iTagVorbisIsrc;

        // Ogg Technical Tags

        private ITag iTagOggChannels;
        private ITag iTagOggSamples;
        private ITag iTagOggSampleRate;

        // Flac Technical Tags

        private ITag iTagFlacBitsPerSample;
    }
}
