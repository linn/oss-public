using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using Upnp;
using Linn.Kinsky;

namespace OssKinskyMppWorldCup2010
{
    interface IView
    {
        Control ViewControl { get; }
        string[] Path { get; set; }
        void Open();
        void Close();
        void OnSizeClick();
        void OnViewClick();
        void Up(uint aLevels);
        event EventHandler<EventArgs> EventLocationChanged;
    }

    [System.ComponentModel.DesignerCategory("")]

    class View : Control, IView
    {
        internal View(IMediaProviderSupportV7 aSupport)
        {
            iPanel = new PanelBusy();

            iSupport = aSupport;
            iSupport.ViewSupport.EventSupportChanged += EventSupportChanged;

            iMutex = new Mutex();

            iLogo = OssKinskyMppWorldCup2010.Properties.Resources.Logo;

            iGroups = new List<Group>();


            Group group;

            group = new Group("A");
            group.Add(new Team(iSupport, iLogo, "France", "http://www.navyband.navy.mil/anthems/ANTHEMS/France.mp3", "http://www.flags.net/images/largeflags/FRAN0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Mexico", "http://www.navyband.navy.mil/anthems/ANTHEMS/Mexico.mp3", "http://www.flags.net/images/largeflags/MEXC0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "South Africa", "http://www.navyband.navy.mil/anthems/ANTHEMS/South Africa.mp3", "http://www.flags.net/images/largeflags/SOAF0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Uruguay", "http://www.navyband.navy.mil/anthems/ANTHEMS/Uruguay (Short).mp3", "http://www.flags.net/images/largeflags/URGY0001.GIF"));
            iGroups.Add(group);

            group = new Group("B");
            group.Add(new Team(iSupport, iLogo, "Argentina", "http://www.navyband.navy.mil/anthems/ANTHEMS/Argentina (Short).mp3", "http://www.flags.net/images/largeflags/ARGE0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Greece", "http://www.navyband.navy.mil/anthems/ANTHEMS/Greece.mp3", "http://www.flags.net/images/largeflags/GREC0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Korea Republic", "http://www.navyband.navy.mil/anthems/ANTHEMS/Korea, South.mp3", "http://www.flags.net/images/largeflags/SKOR0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Nigeria", "http://www.navyband.navy.mil/anthems/ANTHEMS/Nigeria.mp3", "http://www.flags.net/images/largeflags/NGRA0001.GIF"));
            iGroups.Add(group);

            group = new Group("C");
            group.Add(new Team(iSupport, iLogo, "Algeria", "http://www.navyband.navy.mil/anthems/ANTHEMS/Algeria.mp3", "http://www.flags.net/images/largeflags/ALGE0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "England", "http://www.navyband.navy.mil/anthems/ANTHEMS/United Kingdom.mp3", "http://www.flags.net/images/largeflags/UNKG0100.GIF"));
            group.Add(new Team(iSupport, iLogo, "Slovenia", "http://www.navyband.navy.mil/anthems/ANTHEMS/Slovenia.mp3", "http://www.flags.net/images/largeflags/SLVA0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "United States", "http://www.navyband.navy.mil/anthems/ANTHEMS/United States.mp3", "http://www.flags.net/images/largeflags/UNST0001.GIF"));
            iGroups.Add(group);

            group = new Group("D");
            group.Add(new Team(iSupport, iLogo, "Australia", "http://www.navyband.navy.mil/anthems/ANTHEMS/Australia (Advance Australia Fair).mp3", "http://www.flags.net/images/largeflags/ASTL0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Germany", "http://www.navyband.navy.mil/anthems/ANTHEMS/Germany.mp3", "http://www.flags.net/images/largeflags/GERM0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Ghana", "http://www.navyband.navy.mil/anthems/ANTHEMS/Ghana.mp3", "http://www.flags.net/images/largeflags/GHAN0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Serbia", "http://www.navyband.navy.mil/anthems/ANTHEMS/Serbia.mp3", "http://www.flags.net/images/largeflags/SERB0001.GIF"));
            iGroups.Add(group);

            group = new Group("E");
            group.Add(new Team(iSupport, iLogo, "Cameroon", "http://www.navyband.navy.mil/anthems/ANTHEMS/Cameroon.mp3", "http://www.flags.net/images/largeflags/CAME0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Denmark", "http://www.navyband.navy.mil/anthems/ANTHEMS/Denmark (National Anthem).mp3", "http://www.flags.net/images/largeflags/DENM0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Japan", "http://www.navyband.navy.mil/anthems/ANTHEMS/Japan.mp3", "http://www.flags.net/images/largeflags/JAPA0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Netherlands", "http://www.navyband.navy.mil/anthems/ANTHEMS/Netherlands.mp3", "http://www.flags.net/images/largeflags/NETH0001.GIF"));
            iGroups.Add(group);

            group = new Group("F");
            group.Add(new Team(iSupport, iLogo, "Italy", "http://www.navyband.navy.mil/anthems/ANTHEMS/Italy (Short).mp3", "http://www.flags.net/images/largeflags/ITAL0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "New Zealand", "http://www.navyband.navy.mil/anthems/ANTHEMS/New Zealand (God Defend New Zealand).mp3", "http://www.flags.net/images/largeflags/NWZE0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Paraguay", "http://www.navyband.navy.mil/anthems/ANTHEMS/Paraguay.mp3", "http://www.flags.net/images/largeflags/PARA0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Slovakia", "http://www.navyband.navy.mil/anthems/ANTHEMS/Slovack Republic.mp3", "http://www.flags.net/images/largeflags/SVKA0001.GIF"));
            iGroups.Add(group);

            group = new Group("G");
            group.Add(new Team(iSupport, iLogo, "Brazil", "http://www.navyband.navy.mil/anthems/ANTHEMS/Brazil.mp3", "http://www.flags.net/images/largeflags/BRAZ0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Côte d'Ivoire", "http://www.navyband.navy.mil/anthems/ANTHEMS/Cote d'Ivorie.mp3", "http://www.flags.net/images/largeflags/CDIV0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Korea DPR", "http://www.big.or.jp/~jrldr/m/1na1.mp3", "http://www.flags.net/images/largeflags/NKOR0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Portugal", "http://www.navyband.navy.mil/anthems/ANTHEMS/Portugal.mp3", "http://www.flags.net/images/largeflags/PORT0001.GIF"));
            iGroups.Add(group);

            group = new Group("H");
            group.Add(new Team(iSupport, iLogo, "Chile", "http://www.navyband.navy.mil/anthems/ANTHEMS/Chile.mp3", "http://www.flags.net/images/largeflags/CHIL0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Honduras", "http://www.navyband.navy.mil/anthems/ANTHEMS/Honduras.mp3", "http://www.flags.net/images/largeflags/HOND0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Spain", "http://www.navyband.navy.mil/anthems/ANTHEMS/Spain (Short).mp3", "http://www.flags.net/images/largeflags/SPAN0001.GIF"));
            group.Add(new Team(iSupport, iLogo, "Switzerland", "http://www.navyband.navy.mil/anthems/ANTHEMS/Switzerland.mp3", "http://www.flags.net/images/largeflags/SWIT0001.GIF"));
            iGroups.Add(group);

            foreach (Group g in iGroups)
            {
                foreach (Team t in g.Teams)
                {
                    t.EventArtworkUpdated += EventArtworkUpdated;
                }
            }

            iLevel = 0;

            iLocation = new List<string>();

            iViewSize = 1;

            iListView = new ListViewKinsky();
            iListView.Dock = DockStyle.Fill;
            iListView.Visible = true;
            iListView.AllowDrop = false;

            SetViewColours();

            ColumnHeader header;
            header = new ColumnHeader();
            header.Name = "Icon";
            header.Text = "Icon";
            iListView.Columns.Add(header);
            header = new ColumnHeader();
            header.Name = "Title";
            header.Text = "Title";
            iListView.Columns.Add(header);
            header = new ColumnHeader();
            header.Name = "Items";
            header.Text = "Items";
            header.Width = -2;
            iListView.Columns.Add(header);
            iListView.HeaderStyle = ColumnHeaderStyle.None;

            iListView.ItemActivate += OnActivate;

            iListView.ItemDrag += OnItemDrag;

            Populate();

            Controls.Add(iListView);

            Dock = DockStyle.Fill;

            iPanel.Controls.Add(this);
        }

        public void EventArtworkUpdated(object obj, EventArgs e)
        {
            if (iGroup != null)
            {
                Team team = obj as Team;

                foreach (Team t in iGroup.Teams)
                {
                    if (t == team)
                    {
                        iListView.Invoke((MethodInvoker)Populate);
                    }
                }
            }
        }

        public event EventHandler<EventArgs> EventLocationChanged;

        public string[] Path
        {
            get
            {
                iMutex.WaitOne();
                List<string> location = iLocation;
                iMutex.ReleaseMutex();
                return (location.ToArray());
            }
            set
            {
                iMutex.WaitOne();

                iLevel = 0;

                if (value.Length > 0)
                {
                    foreach (Group g in iGroups)
                    {
                        if (g.Title == value[0])
                        {
                            iGroup = g;

                            iLevel++;

                            break;
                        }
                    }
                }

                iMutex.ReleaseMutex();

                UpdateLocation();

                Populate();
            }
        }

        void ResizeControls()
        {
            if (iViewSize == 0)
            {
                iListView.Font = iSupport.ViewSupport.FontSmall;
                iListView.LargeIconSize = new Size(64, 64);
                iListView.SmallIconSize = new Size(20, 20);
                iListView.TileHeight = 70;
            }
            else if (iViewSize == 1)
            {
                iListView.Font = iSupport.ViewSupport.FontMedium;
                
                iListView.LargeIconSize = new Size(128, 128);
                iListView.SmallIconSize = new Size(40, 40);
                iListView.TileHeight = 140;
            }
            else
            {
                iListView.Font = iSupport.ViewSupport.FontLarge;

                iListView.LargeIconSize = new Size(256, 256);
                iListView.SmallIconSize = new Size(80, 80);
                iListView.TileHeight = 281;
            }
        }

        void PopulateGroups()
        {
            iMutex.WaitOne();

            iPanel.StopBusy();
            Visible = true;

            iListView.BeginUpdate();

            iListView.Items.Clear();

            ResizeControls();

            foreach (Group g in iGroups)
            {
                ListViewKinsky.Item item = new ListViewKinsky.Item();

                item.Text = g.Title;
                item.Icon = iLogo;
                item.IconSelected = iLogo;

                ListViewItem.ListViewSubItem subitem;
                
                subitem = new ListViewItem.ListViewSubItem();
                subitem.Name = "Title";
                subitem.Text = g.Title;
                item.SubItems.Add(subitem);

                subitem = new ListViewItem.ListViewSubItem();
                subitem.Name = "Items";
                subitem.Text = "4";
                item.SubItems.Add(subitem);

                if (g == iGroup)
                {
                    item.Selected = true;
                }

                iListView.Items.Add(item);
            }

            iListView.Columns[0].Width = iListView.SmallIconSize.Width + 8;
            iListView.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            iListView.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.ColumnContent);

            iListView.EndUpdate();

            iMutex.ReleaseMutex();
        }

        void PopulateTeams()
        {
            iMutex.WaitOne();

            iListView.BeginUpdate();

            iListView.Items.Clear();

            ResizeControls();

            iPanel.StopBusy();

            Visible = true;

            foreach (Team t in iGroup.Teams)
            {
                ListViewKinsky.Item item = new ListViewKinsky.Item();
                item.Text = t.Name;
                item.Icon = t.Flag;
                item.IconSelected = t.Flag;

                ListViewItem.ListViewSubItem subitem;

                subitem = new ListViewItem.ListViewSubItem();
                subitem.Name = "Title";
                subitem.Text = t.Name;
                item.SubItems.Add(subitem);

                if (t == iTeam)
                {
                    item.Selected = true;
                }

                iListView.Items.Add(item);
            }

            iListView.Columns[0].Width = iListView.SmallIconSize.Width + 8;
            iListView.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            iListView.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.ColumnContent);

            iListView.EndUpdate();

            iMutex.ReleaseMutex();
        }

        public void OnSizeClick()
        {
            iListView.Visible = false;

            if (++iViewSize > 2)
            {
                iViewSize = 0;
            }

            Populate();

            iListView.Visible = true;
        }

        public void OnViewClick()
        {
            iListView.Visible = false;

            if (iListView.View == System.Windows.Forms.View.LargeIcon)
            {
                iListView.View = System.Windows.Forms.View.Tile;
            }
            else if (iListView.View == System.Windows.Forms.View.Tile)
            {
                iListView.View = System.Windows.Forms.View.LargeIcon;
            }

            Populate();

            iListView.Visible = true;
        }


        public void Up(uint aLevels)
        {
            iLevel -= aLevels;

            UpdateLocation();

            Populate();
        }

        void EnsureVisible()
        {
            iListView.Visible = true;

            if (iListView.SelectedIndices.Count > 0)
            {
                iListView.EnsureVisible(iListView.SelectedIndices[0]);
            }
            else if (iListView.Items.Count > 0)
            {
                iListView.EnsureVisible(0);
            }
        }

        void Populate()
        {
            iListView.Visible = false;

            if (iLevel == 0)
            {
                PopulateGroups();
                EnsureVisible();
            }
            else if (iLevel == 1)
            {
                PopulateTeams();
                EnsureVisible();
            }
        }

        private void OnActivate(object obj, EventArgs e)
        {
            if (iLevel == 0) // prevents details view
            {
                iLevel++;

                int selected = iListView.SelectedIndices[0];

                iGroup = iGroups[selected];

                UpdateLocation();

                Populate();
            }
            else if (iLevel == 1)
            {
                int selected = iListView.SelectedIndices[0];

                iTeam = iGroup.Teams[selected];

                List<upnpObject> list = new List<upnpObject>();

                list.Add(CreateUpnpObject(iTeam));


                iSupport.PlaylistSupport.PlayNow(new MediaRetrieverNoRetrieve(list));
            }
        }

        public void OnItemDrag(object sender, ItemDragEventArgs e)
        {
            if (iLevel == 0)
            {
                int selected = iListView.SelectedIndices[0];

                iGroup = iGroups[selected];

                List<upnpObject> list = new List<upnpObject>();

                foreach (Team t in iGroup.Teams)
                {
                    list.Add(CreateUpnpObject(t));
                }

                iSupport.PlaylistSupport.SetDragging(true);

                DragDropEffects effect = iListView.DoDragDrop(new MediaProviderDraggable(new MediaRetrieverNoRetrieve(list)), DragDropEffects.All);

                iSupport.PlaylistSupport.SetDragging(false);
            }
            else if (iLevel == 1)
            {
                int selected = iListView.SelectedIndices[0];

                iTeam = iGroup.Teams[selected];

                List<upnpObject> list = new List<upnpObject>();

                list.Add(CreateUpnpObject(iTeam));

                iSupport.PlaylistSupport.SetDragging(true);
                iListView.DoDragDrop(new MediaProviderDraggable(new MediaRetrieverNoRetrieve(list)), DragDropEffects.All);
                iSupport.PlaylistSupport.SetDragging(false);
            }
        }

        private Uri GetUri(MemoryStream aMemoryStream)
        {
            byte[] buffer = new byte[aMemoryStream.Length];

            aMemoryStream.Read(buffer, 0, buffer.Length);

            try
            {
                Uri uri = new Uri(ASCIIEncoding.UTF8.GetString(buffer).TrimEnd('\0'));
                return uri;
            }
            catch (UriFormatException)
            {
                return (null);
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;

            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy || (e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
            {
                if (e.Data.GetDataPresent("UniformResourceLocator"))
                {
                    MemoryStream stream = e.Data.GetData("UniformResourceLocator") as MemoryStream;

                    Uri uri = GetUri(stream);

                    if (uri != null)
                    {
                        if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                        {
                            e.Effect = DragDropEffects.Copy;
                        }
                        else
                        {
                            e.Effect = DragDropEffects.Link;
                        }
                    }
                }
                else if (e.Data.GetDataPresent(DataFormats.Text) && e.Effect == DragDropEffects.None)
                {
                    string text = e.Data.GetData(DataFormats.Text) as string;

                    try
                    {
                        Uri uri = new Uri(text);

                        if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
                        {
                            e.Effect = DragDropEffects.Copy;
                        }
                        else
                        {
                            e.Effect = DragDropEffects.Link;
                        }
                    }
                    catch (UriFormatException)
                    {
                    }
                }
            }
        }

        private void UpdateLocation()
        {
            List<string> location = new List<string>();

            if (iLevel > 0)
            {
                location.Add(iGroup.Title);
            }

            iMutex.WaitOne();

            iLocation = location;

            iMutex.ReleaseMutex();

            if (EventLocationChanged != null)
            {
                EventLocationChanged(this, EventArgs.Empty);
            }
        }

        public void Open()
        {
        }

        public void Close()
        {
            Up(iLevel);
        }

        public Control ViewControl
        {
            get
            {
                return (iPanel);
            }
        }

        private upnpObject CreateUpnpObject(Team aTeam)
        {
            audioItem item = new audioItem();
            resource resource = new resource();
            resource.Uri = aTeam.Uri;
            resource.Bitrate = 128000 / 8;
            item.Res.Add(resource);
            item.Id = aTeam.Uri;
            item.Title = aTeam.Name;
            item.Genre.Add("Ceremonial");
            item.ArtworkUri.Add(aTeam.FlagUri);
            return (item);
        }

        private void EventSupportChanged(object sender, EventArgs e)
        {
            SetViewColours();
            Populate();
        }

        private void SetViewColours()
        {
            iListView.BackColor = iSupport.ViewSupport.BackColour;
            iListView.ForeColor = iSupport.ViewSupport.ForeColour;
            iListView.ForeColorMuted = iSupport.ViewSupport.ForeColourMuted;
            iListView.ForeColorBright = iSupport.ViewSupport.ForeColourBright;
            iListView.HighlightBackColour = iSupport.ViewSupport.HighlightBackColour;
            iListView.HighlightForeColour = iSupport.ViewSupport.HighlightForeColour;
        }

        IMediaProviderSupportV7 iSupport;
        Mutex iMutex;
        PanelBusy iPanel;
        ListViewKinsky iListView;

        uint iViewSize;
        uint iLevel;
        List<string> iLocation;

        List<Group> iGroups;
        Group iGroup;
        Team iTeam;

        Image iLogo;

    }

    class Group
    {
        public Group(string aName)
        {
            iName = aName;
            iTeams = new List<Team>();
        }

        public string Title
        {
            get
            {
                return ("Group " + iName);
            }
        }

        public List<Team> Teams
        {
            get
            {
                return (iTeams);
            }
        }

        public void Add(Team aTeam)
        {
            iTeams.Add(aTeam);
        }

        private string iName;
        private List<Team> iTeams;
    }

    class Team
    {
        public Team(IMediaProviderSupportV7 aSupport, Image aDefaultFlag, string aName, string aUri, string aFlagUri)
        {
            iSupport = aSupport;
            iName = aName;
            iUri = aUri;
            iFlagUri = aFlagUri;
            iFlag = aDefaultFlag;

            iSupport.ArtworkCache.EventUpdated += EventArtworkDownloaded;
            iArtwork = aSupport.ArtworkCache.Artwork(new Uri(aFlagUri));
        }

        public event EventHandler<EventArgs> EventArtworkUpdated;

        public void EventArtworkDownloaded(object obj, EventArgsArtwork e)
        {
            if (e.Artwork.Uri.AbsoluteUri == iFlagUri)
            {
                if (iArtwork.Image != null)
                {
                    iFlag = iArtwork.Image;

                    if (EventArtworkUpdated != null)
                    {
                        EventArtworkUpdated(this, EventArgs.Empty);
                    }
                }

                iSupport.ArtworkCache.EventUpdated -= EventArtworkDownloaded;
            }
        }

        public string Name
        {
            get
            {
                return (iName);
            }
        }

        public string Uri
        {
            get
            {
                return (iUri);
            }
        }

        public Image Flag
        {
            get
            {
                return (iFlag);
            }
        }

        public string FlagUri
        {
            get
            {
                return (iFlagUri);
            }
        }

        private IMediaProviderSupportV7 iSupport;
        private IArtwork iArtwork;
        private string iName;
        private string iUri;
        private string iFlagUri;
        private Image iFlag;
    }
}
