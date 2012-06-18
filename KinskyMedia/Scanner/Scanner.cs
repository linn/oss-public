using System;
using System.Collections.Generic;

using KinskyMedia.Database;

namespace KinskyMedia.Scanner
{
    public interface IScanner
    {
        void ManageTags(ITagManager aManager);
        IList<IMetadatum> Scan(string aUri);
    }

    public class ScannerManager : IScanner
    {
        public ScannerManager()
        {
            iScannerList = new List<IScanner>();
            iScannerList.Add(new ScannerXiph());
            iScannerList.Add(new ScannerTagLib());
        }
        
        public void ManageTags(ITagManager aManager)
        {
            aManager.Create(TagAv.Ns, TagAv.NameTitle);
            aManager.Create(TagAv.Ns, TagAv.NameVersion);
            aManager.Create(TagAv.Ns, TagAv.NameAlbum);
            aManager.Create(TagAv.Ns, TagAv.NameTrackNumber);
            aManager.Create(TagAv.Ns, TagAv.NameTrackCount);
            aManager.Create(TagAv.Ns, TagAv.NameArtist);
            aManager.Create(TagAv.Ns, TagAv.NamePerformer);
            aManager.Create(TagAv.Ns, TagAv.NameCopyright);
            aManager.Create(TagAv.Ns, TagAv.NameLicense);
            aManager.Create(TagAv.Ns, TagAv.NameOrganisation);
            aManager.Create(TagAv.Ns, TagAv.NameDescription);
            aManager.Create(TagAv.Ns, TagAv.NameGenre);
            aManager.Create(TagAv.Ns, TagAv.NameDate);
            aManager.Create(TagAv.Ns, TagAv.NameLocation);
            aManager.Create(TagAv.Ns, TagAv.NameContact);
            aManager.Create(TagAv.Ns, TagAv.NameIsrc);
            aManager.Create(TagAv.Ns, TagAv.NameBitsPerSample);
            aManager.Create(TagAv.Ns, TagAv.NameChannels);
            aManager.Create(TagAv.Ns, TagAv.NameSamples);
            aManager.Create(TagAv.Ns, TagAv.NameSampleRate);
            aManager.Create(TagAv.Ns, TagAv.NameBeatsPerMinute);
            aManager.Create(TagAv.Ns, TagAv.NameComment);
            aManager.Create(TagAv.Ns, TagAv.NameComposer);
            aManager.Create(TagAv.Ns, TagAv.NameConductor);
            aManager.Create(TagAv.Ns, TagAv.NameDiscNumber);
            aManager.Create(TagAv.Ns, TagAv.NameDiscCount);
            aManager.Create(TagAv.Ns, TagAv.NameGrouping);
            aManager.Create(TagAv.Ns, TagAv.NameLyrics);


            foreach (IScanner scanner in iScannerList)
            {
                scanner.ManageTags(aManager);
            }
        }
        
        public IList<IMetadatum> Scan(string aUri)
        {
            // Use the list of scanners in force when the scan request was made

            foreach (IScanner scanner in iScannerList)
            {
                IList<IMetadatum> metadata = scanner.Scan(aUri);

                if (metadata != null)
                {
                    return (metadata);
                }
            }

            return (null);
        }

        private List<IScanner> iScannerList;
    }
}

