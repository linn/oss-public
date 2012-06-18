using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

using Luminescence.Xiph;

using SneakyMedia.Database;

namespace SneakyMedia.Scanner
{
    public class TagLegacyVorbis : TagLegacy
    {
        public const string kNs = "vorbis";

        public TagLegacyVorbis(string aName)
            : base(kNs, aName)
        {
        }
    }

    public class TagLegacyFlac : TagLegacy
    {
        public const string kNs = "flac";

        public TagLegacyFlac(string aName)
            : base(kNs, aName)
        {
        }
    }

    public class TagLegacyOgg : TagLegacy
    {
        public const string kNs = "ogg";

        public TagLegacyOgg(string aName)
            : base(kNs, aName)
        {
        }
    }

    public class TagVorbisComment
    {
        public TagVorbisComment(string aName)
        {
            iHypertag = new HypertagAv(aName);
            iTag = new TagLegacyVorbis(aName);
        }

        public Tag Tag
        {
            get
            {
                return (iTag);
            }
        }

        public Hypertag Hypertag
        {
            get
            {
                return (iHypertag);
            }
        }

        Tag iTag;
        Hypertag iHypertag;
    }

    public class ModuleScannerXiph : ModuleScanner
    {
        public ModuleScannerXiph()
            : base("Luminescence.Xiph", new Version(2, 0, 0, 0))
        {
        }

        protected override void OnCreate()
        {
            CreateHypertags();

            CreateVorbisComments();

            CreateTags();
        }

        protected override void OnLoad()
        {
            CreateHypertags();

            CreateVorbisComments();

            CreateTags();
        }

        private void CreateHypertags()
        {
            iHypertagChannels = new HypertagAv(HypertagAv.kNameChannels);
            iHypertagSamples = new HypertagAv(HypertagAv.kNameSamples);
            iHypertagSampleRate = new HypertagAv(HypertagAv.kNameSampleRate);
            iHypertagBitsPerSample = new HypertagAv(HypertagAv.kNameBitsPerSample);
        }

        private void CreateVorbisComments()
        {
            iTagVorbisComments = new Dictionary<string, TagVorbisComment>();

            AddVorbisComment(HypertagAv.kNameTitle);
            AddVorbisComment(HypertagAv.kNameVersion);
            AddVorbisComment(HypertagAv.kNameAlbum);
            AddVorbisComment(HypertagAv.kNameTrackNumber);
            AddVorbisComment(HypertagAv.kNameArtist);
            AddVorbisComment(HypertagAv.kNamePerformer);
            AddVorbisComment(HypertagAv.kNameCopyright);
            AddVorbisComment(HypertagAv.kNameLicense);
            AddVorbisComment(HypertagAv.kNameOrganisation);
            AddVorbisComment(HypertagAv.kNameDescription);
            AddVorbisComment(HypertagAv.kNameGenre);
            AddVorbisComment(HypertagAv.kNameDate);
            AddVorbisComment(HypertagAv.kNameLocation);
            AddVorbisComment(HypertagAv.kNameContact);
            AddVorbisComment(HypertagAv.kNameIsrc);
        }

        private void AddVorbisComment(string aName)
        {
            iTagVorbisComments.Add(aName.ToUpperInvariant(), new TagVorbisComment(aName));
        }

        private void CreateTags()
        {
            iTagOggChannels = new TagLegacyOgg("Channels");
            iTagOggSamples = new TagLegacyOgg("Samples");
            iTagOggSampleRate = new TagLegacyOgg("SampleRate");
            iTagFlacBitsPerSample = new TagLegacyFlac("BitsPerSample");
        }

        public override IList<IMetadatum> Scan(string aUri)
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
            List<IMetadatum> tags = new List<IMetadatum>();

            AddVorbisComments(aFile, tags);

            // Add Flac Technical Tags

            string bitspersample = aFile.BitsPerSample.ToString();

            tags.Add(new Metadatum(iHypertagBitsPerSample, bitspersample));
            tags.Add(new Metadatum(iTagFlacBitsPerSample, bitspersample));

            // Add Ogg Technical Tags

            string channels = aFile.Channels.ToString();
            string samples = aFile.Samples.ToString();
            string samplerate = aFile.SampleRate.ToString();

            tags.Add(new Metadatum(iHypertagChannels, channels));
            tags.Add(new Metadatum(iHypertagSamples, samples));
            tags.Add(new Metadatum(iHypertagSampleRate, samplerate));

            tags.Add(new Metadatum(iTagOggChannels, channels));
            tags.Add(new Metadatum(iTagOggSamples, samples));
            tags.Add(new Metadatum(iTagOggSampleRate, samplerate));

            return (tags);
        }

        IList<IMetadatum> ProcessOggFlac(OggTagger aFile)
        {
            List<IMetadatum> tags = new List<IMetadatum>();

            AddVorbisComments(aFile, tags);

            // Add Flac Technical Tags

            string bitspersample = aFile.FlacBitsPerSample.ToString();

            tags.Add(new Metadatum(iHypertagBitsPerSample, bitspersample));
            tags.Add(new Metadatum(iTagFlacBitsPerSample, bitspersample));

            // Add Ogg Technical Tags

            string channels = aFile.Channels.ToString();
            string samples = aFile.Samples.ToString();
            string samplerate = aFile.SampleRate.ToString();

            tags.Add(new Metadatum(iHypertagChannels, channels));
            tags.Add(new Metadatum(iHypertagSamples, samples));
            tags.Add(new Metadatum(iHypertagSampleRate, samplerate));

            tags.Add(new Metadatum(iTagOggChannels, channels));
            tags.Add(new Metadatum(iTagOggSamples, samples));
            tags.Add(new Metadatum(iTagOggSampleRate, samplerate));

            return (tags);
        }

        IList<IMetadatum> ProcessOggVorbis(OggTagger aFile)
        {
            List<IMetadatum> tags = new List<IMetadatum>();

            AddVorbisComments(aFile, tags);

            // Add Ogg Technical Tags

            string channels = aFile.Channels.ToString();
            string samples = aFile.Samples.ToString();
            string samplerate = aFile.SampleRate.ToString();

            tags.Add(new Metadatum(iHypertagChannels, channels));
            tags.Add(new Metadatum(iHypertagSamples, samples));
            tags.Add(new Metadatum(iHypertagSampleRate, samplerate));

            tags.Add(new Metadatum(iTagOggChannels, channels));
            tags.Add(new Metadatum(iTagOggSamples, samples));
            tags.Add(new Metadatum(iTagOggSampleRate, samplerate));

            return (tags);
        }

        IList<IMetadatum> ProcessOggSpeex(OggTagger aFile)
        {
            List<IMetadatum> tags = new List<IMetadatum>();

            AddVorbisComments(aFile, tags);

            // Add Ogg Technical Tags

            string channels = aFile.Channels.ToString();
            string samples = aFile.Samples.ToString();
            string samplerate = aFile.SampleRate.ToString();

            tags.Add(new Metadatum(iHypertagChannels, channels));
            tags.Add(new Metadatum(iHypertagSamples, samples));
            tags.Add(new Metadatum(iHypertagSampleRate, samplerate));

            tags.Add(new Metadatum(iTagOggChannels, channels));
            tags.Add(new Metadatum(iTagOggSamples, samples));
            tags.Add(new Metadatum(iTagOggSampleRate, samplerate));

            return (tags);
        }

        void AddVorbisComments(VorbisComment aFile, List<IMetadatum> aTags)
        {
            SortedList<string, List<string>> scanned = aFile.GetAllTags();

            foreach (KeyValuePair<string, List<string>> entry in scanned)
            {
                TagVorbisComment comment;

                iTagVorbisComments.TryGetValue(entry.Key.ToUpperInvariant(), out comment);

                if (comment != null)
                {
                    foreach (string value in entry.Value)
                    {
                        if (value.Length > 0)
                        {
                            aTags.Add(new Metadatum(comment.Hypertag, value));
                            aTags.Add(new Metadatum(comment.Tag, value));
                        }
                    }
                }
            }
        }

        // Hypertags

        private Hypertag iHypertagChannels;
        private Hypertag iHypertagSamples;
        private Hypertag iHypertagSampleRate;
        private Hypertag iHypertagBitsPerSample;

        private Dictionary<string, TagVorbisComment> iTagVorbisComments;

        // Ogg Technical Tags

        private Tag iTagOggChannels;
        private Tag iTagOggSamples;
        private Tag iTagOggSampleRate;

        // Flac Technical Tags

        private Tag iTagFlacBitsPerSample;
    }
}
