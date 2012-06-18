using System;
using Linn;

namespace KinskyJukebox
{
    public class OptionPageOrganisation : OptionPage
    {
        public OptionPageOrganisation()
            : base("Organisation") {

            iSortTypeNew = new OptionBool("sortnew", "Sort by " + MediaCollection.SortTypeToString(MediaCollection.SortType.eNew), "When sorting your scanned collection, include a " + MediaCollection.SortTypeToString(MediaCollection.SortType.eNew) + " category", true);
            Add(iSortTypeNew);

            iNewMusicCutoffDays = new OptionNumber("newcutoff", "New music cutoff (days)", "Music files created/modified before the specified cutoff will included when scanning by 'New'", 14, 1, 999);
            Add(iNewMusicCutoffDays);

            iSortTypeArtistAlbum= new OptionBool("sortartistalbum", "Sort by " + MediaCollection.SortTypeToString(MediaCollection.SortType.eArtistAlbum), "When sorting your scanned collection, include a " + MediaCollection.SortTypeToString(MediaCollection.SortType.eArtistAlbum) + " category", true);
            Add(iSortTypeArtistAlbum);

            iSortByYear = new OptionBoolEnum("albumarrangement", "Album Arrangement", "Albums can either be arranged alphabetically or by year (as Year/Album, oldest to newest like a discography)", false, "Arrange Albums by Year", "Arrange Albums Alphabetically");
            Add(iSortByYear);

            iSortTypeArtist= new OptionBool("sortartist", "Sort by " + MediaCollection.SortTypeToString(MediaCollection.SortType.eArtist), "When sorting your scanned collection, include a " + MediaCollection.SortTypeToString(MediaCollection.SortType.eArtist) + " category", false);
            Add(iSortTypeArtist);

            iIgnoreCompilations = new OptionBoolEnum("groupcompilations", "Group Compilation Artists", "Group compilation artists as 'Various' or retain artist names (only applies when scanning by 'Artist', done automatically when scanning by 'Artist/Album')", false, "Group as 'Various'", "Retain Artist Names");
            Add(iIgnoreCompilations);

            iSortTypeAlbum= new OptionBool("sortalbum", "Sort by " + MediaCollection.SortTypeToString(MediaCollection.SortType.eAlbum), "When sorting your scanned collection, include a " + MediaCollection.SortTypeToString(MediaCollection.SortType.eAlbum) + " category", false);
            Add(iSortTypeAlbum);

            iSortTypeGenre= new OptionBool("sortgenre", "Sort by " + MediaCollection.SortTypeToString(MediaCollection.SortType.eGenre), "When sorting your scanned collection, include a " + MediaCollection.SortTypeToString(MediaCollection.SortType.eGenre) + " category", false);
            Add(iSortTypeGenre);

            iSortTypeComposer= new OptionBool("sortcomposer", "Sort by " + MediaCollection.SortTypeToString(MediaCollection.SortType.eComposer), "When sorting your scanned collection, include a " + MediaCollection.SortTypeToString(MediaCollection.SortType.eComposer) + " category", false);
            Add(iSortTypeComposer);

            iSortTypeConductor= new OptionBool("sortconductor", "Sort by " + MediaCollection.SortTypeToString(MediaCollection.SortType.eConductor), "When sorting your scanned collection, include a " + MediaCollection.SortTypeToString(MediaCollection.SortType.eConductor) + " category", false);
            Add(iSortTypeConductor);

            iSortTypeArtistAz= new OptionBool("sortartistaz", "Sort by " + MediaCollection.SortTypeToString(MediaCollection.SortType.eArtistAz), "When sorting your scanned collection, include a " + MediaCollection.SortTypeToString(MediaCollection.SortType.eArtistAz) + " category", false);
            Add(iSortTypeArtistAz);

            iSortTypeTitleAz= new OptionBool("sorttitleaz", "Sort by " + MediaCollection.SortTypeToString(MediaCollection.SortType.eTitleAz), "When sorting your scanned collection, include a " + MediaCollection.SortTypeToString(MediaCollection.SortType.eTitleAz) + " category", false);
            Add(iSortTypeTitleAz);

            iSortTypeAlbumArtistAlbum= new OptionBool("sortalbumartistalbum", "Sort by " + MediaCollection.SortTypeToString(MediaCollection.SortType.eAlbumArtistAlbum), "When sorting your scanned collection, include a " + MediaCollection.SortTypeToString(MediaCollection.SortType.eAlbumArtistAlbum) + " category", false);
            Add(iSortTypeAlbumArtistAlbum);

            iSortTypeAlbumArtist= new OptionBool("sortalbumartist", "Sort by " + MediaCollection.SortTypeToString(MediaCollection.SortType.eAlbumArtist), "When sorting your scanned collection, include a " + MediaCollection.SortTypeToString(MediaCollection.SortType.eAlbumArtist) + " category", false);
            Add(iSortTypeAlbumArtist);

            iSortTypeAll= new OptionBool("sortall", "Sort by " + MediaCollection.SortTypeToString(MediaCollection.SortType.eAll), "When sorting your scanned collection, include a " + MediaCollection.SortTypeToString(MediaCollection.SortType.eAll) + " category", false);
            Add(iSortTypeAll);

            iIncludeLocalRadio = new OptionBoolEnum("localradio", "Local Radio", "Creates a group of playlists of local radio stations (provided by TuneIn Radio) if selected", false, "Create Local Radio Playlists", "Ignore");
            Add(iIncludeLocalRadio);

            // linked options
            iNewMusicCutoffDays.Enabled = iSortTypeNew.Native;
            EventSortTypeNewChanged += SortTypeNewChanged; 
            iIgnoreCompilations.Enabled = iSortTypeArtist.Native;
            EventSortTypeArtistChanged += SortTypeArtistChanged; 
        }

        public bool SortTypeNew {
            get { return iSortTypeNew.Native; }
        }

        public bool SortTypeArtistAlbum {
            get { return iSortTypeArtistAlbum.Native; }
        }

        public bool SortTypeArtist {
            get { return iSortTypeArtist.Native; }
        }

        public bool SortTypeAlbum {
            get { return iSortTypeAlbum.Native; }
        }

        public bool SortTypeGenre {
            get { return iSortTypeGenre.Native; }
        }

        public bool SortTypeComposer {
            get { return iSortTypeComposer.Native; }
        }

        public bool SortTypeConductor {
            get { return iSortTypeConductor.Native; }
        }

        public bool SortTypeArtistAz {
            get { return iSortTypeArtistAz.Native; }
        }

        public bool SortTypeTitleAz {
            get { return iSortTypeTitleAz.Native; }
        }

        public bool SortTypeAlbumArtistAlbum {
            get { return iSortTypeAlbumArtistAlbum.Native; }
        }

        public bool SortTypeAlbumArtist {
            get { return iSortTypeAlbumArtist.Native; }
        }

        public bool SortTypeAll {
            get { return iSortTypeAll.Native; }
        }

        public bool IgnoreCompilations {
            get { return iIgnoreCompilations.Native; }
        }

        public int NewMusicCutoffDays {
            get { return iNewMusicCutoffDays.Native; }
        }

        public bool SortByYear {
            get { return iSortByYear.Native; }
        }

        public bool IncludeLocalRadio {
            get { return iIncludeLocalRadio.Native; }
        }

        public event EventHandler<EventArgs> EventSortTypeNewChanged {
            add { iSortTypeNew.EventValueChanged += value; }
            remove { iSortTypeNew.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventSortTypeArtistAlbumChanged {
            add { iSortTypeArtistAlbum.EventValueChanged += value; }
            remove { iSortTypeArtistAlbum.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventSortTypeArtistChanged {
            add { iSortTypeArtist.EventValueChanged += value; }
            remove { iSortTypeArtist.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventSortTypeAlbumChanged {
            add { iSortTypeAlbum.EventValueChanged += value; }
            remove { iSortTypeAlbum.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventSortTypeGenreChanged {
            add { iSortTypeGenre.EventValueChanged += value; }
            remove { iSortTypeGenre.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventSortTypeComposerChanged {
            add { iSortTypeComposer.EventValueChanged += value; }
            remove { iSortTypeComposer.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventSortTypeConductorChanged {
            add { iSortTypeConductor.EventValueChanged += value; }
            remove { iSortTypeConductor.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventSortTypeArtistAzChanged {
            add { iSortTypeArtistAz.EventValueChanged += value; }
            remove { iSortTypeArtistAz.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventSortTypeTitleAzChanged {
            add { iSortTypeTitleAz.EventValueChanged += value; }
            remove { iSortTypeTitleAz.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventSortTypeAlbumArtistAlbumChanged {
            add { iSortTypeAlbumArtistAlbum.EventValueChanged += value; }
            remove { iSortTypeAlbumArtistAlbum.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventSortTypeAlbumArtistChanged {
            add { iSortTypeAlbumArtist.EventValueChanged += value; }
            remove { iSortTypeAlbumArtist.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventSortTypeAllChanged {
            add { iSortTypeAll.EventValueChanged += value; }
            remove { iSortTypeAll.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventIgnoreCompilationsChanged {
            add { iIgnoreCompilations.EventValueChanged += value; }
            remove { iIgnoreCompilations.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventNewMusicCutoffDaysChanged {
            add { iNewMusicCutoffDays.EventValueChanged += value; }
            remove { iNewMusicCutoffDays.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventSortByYearChanged {
            add { iSortByYear.EventValueChanged += value; }
            remove { iSortByYear.EventValueChanged -= value; }
        }

        public event EventHandler<EventArgs> EventIncludeLocalRadioChanged {
            add { iIncludeLocalRadio.EventValueChanged += value; }
            remove { iIncludeLocalRadio.EventValueChanged -= value; }
        }

        private void SortTypeNewChanged(object sender, EventArgs e) {
            iNewMusicCutoffDays.Enabled = iSortTypeNew.Native;
        }

        private void SortTypeArtistChanged(object sender, EventArgs e) {
            iIgnoreCompilations.Enabled = iSortTypeArtist.Native;
        }

        private OptionBool iSortTypeNew;
        private OptionBool iSortTypeArtistAlbum;
        private OptionBool iSortTypeArtist;
        private OptionBool iSortTypeAlbum;
        private OptionBool iSortTypeGenre;
        private OptionBool iSortTypeComposer;
        private OptionBool iSortTypeConductor;
        private OptionBool iSortTypeArtistAz;
        private OptionBool iSortTypeTitleAz;
        private OptionBool iSortTypeAlbumArtistAlbum;
        private OptionBool iSortTypeAlbumArtist;
        private OptionBool iSortTypeAll;
        private OptionBoolEnum iIgnoreCompilations;
        private OptionNumber iNewMusicCutoffDays;
        private OptionBoolEnum iSortByYear;
        private OptionBoolEnum iIncludeLocalRadio;
    }
}