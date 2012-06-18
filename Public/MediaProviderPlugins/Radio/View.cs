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

namespace OssKinskyMppRadio
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
        internal View(IMediaProviderSupportV7 aSupport, Lrf aPodcasts)
        {
            iPanel = new PanelBusy();

            iSupport = aSupport;
            iSupport.ViewSupport.EventSupportChanged += EventSupportChanged;

            iLrf = aPodcasts;
            iLrf.EventChanged += EventFeedsChanged;

            iMutex = new Mutex();

            iLevel = 0;

            iLocation = new List<string>();

            iViewSize = 1;

            iFeedList = iLrf.FeedList;

            iListView = new ListViewKinsky();
            iListView.Dock = DockStyle.Fill;
            iListView.Visible = true;
            iListView.AllowDrop = true;

            SetViewColours();

            iListView.View = System.Windows.Forms.View.Tile;

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
            iListView.DragDrop += OnDragDrop;
            iListView.DragOver += OnDragOver;

            iWebBrowser = new WebBrowser();
            iWebBrowser.Dock = DockStyle.Fill;
            iWebBrowser.AllowNavigation = false;
            iWebBrowser.AllowWebBrowserDrop = false;
            iWebBrowser.IsWebBrowserContextMenuEnabled = false;
            iWebBrowser.Visible = false;

            var input = new StringReader(Properties.Resources.Details);
            var reader = new XmlTextReader(input);

            iArguments = new XsltArgumentList();

            iArguments.AddParam("bg", String.Empty, System.Drawing.ColorTranslator.ToHtml(iSupport.ViewSupport.BackColour));
            iArguments.AddParam("fg", String.Empty, System.Drawing.ColorTranslator.ToHtml(iSupport.ViewSupport.ForeColour));
            iArguments.AddParam("hibg", String.Empty, System.Drawing.ColorTranslator.ToHtml(iSupport.ViewSupport.HighlightBackColour));
            iArguments.AddParam("hifg", String.Empty, System.Drawing.ColorTranslator.ToHtml(iSupport.ViewSupport.HighlightForeColour));

            iTransform = new XslCompiledTransform();

            iTransform.Load(reader);

            Populate();

            Controls.Add(iListView);
            Controls.Add(iWebBrowser);

            Dock = DockStyle.Fill;

            iPanel.Controls.Add(this);
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
                    foreach (IFeed f in iFeedList)
                    {
                        if (f.Title == value[0])
                        {
                            iFeed = f;

                            iLevel++;

                            if (value.Length > 1)
                            {
                                if (iFeed.Downloaded)
                                {
                                    foreach (IEntry e in iFeed.EntryList)
                                    {
                                        if (e.Title == value[1])
                                        {
                                            iEntry = e;

                                            iLevel++;
                                        }
                                    }
                                }
                            }
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

        void PopulateFeeds()
        {
            iMutex.WaitOne();

            iPanel.StopBusy();
            Visible = true;

            iListView.BeginUpdate();

            iListView.Items.Clear();

            ResizeControls();

            foreach (IFeed f in iFeedList)
            {
                ListViewKinsky.Item item = new ListViewKinsky.Item();
                item.Text = f.Title;

                if (f.Downloaded)
                {
                    item.Icon = f.Logo;
                }
                else
                {
                    item.Icon = OssKinskyMppRadio.Properties.Resources.Radio;
                }

                item.IconSelected = item.Icon;

                ListViewItem.ListViewSubItem subitem;
                
                subitem = new ListViewItem.ListViewSubItem();
                subitem.Name = "Title";
                subitem.Text = f.Title;
                item.SubItems.Add(subitem);

                subitem = new ListViewItem.ListViewSubItem();
                subitem.Name = "Items";

                if (f.Downloaded)
                {
                    subitem.Text = f.EntryList.Count.ToString();
                }
                else
                {
                    subitem.Text = "Downloading";
                }

                item.SubItems.Add(subitem);

                if (f == iFeed)
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

        void PopulateEntries()
        {
            iMutex.WaitOne();

            iListView.BeginUpdate();

            iListView.Items.Clear();

            ResizeControls();

            if (iFeed.Downloaded)
            {
                iPanel.StopBusy();
                Visible = true;

                foreach (IEntry e in iFeed.EntryList)
                {
                    ListViewKinsky.Item item = new ListViewKinsky.Item();
                    item.Text = e.Title;
                    item.Icon = e.Logo;
                    item.IconSelected = e.Logo;

                    ListViewItem.ListViewSubItem subitem;

                    subitem = new ListViewItem.ListViewSubItem();
                    subitem.Name = "Title";
                    subitem.Text = e.Title;
                    item.SubItems.Add(subitem);

                    subitem = new ListViewItem.ListViewSubItem();
                    subitem.Name = "Country";
                    subitem.Text = e.Location.Country;
                    item.SubItems.Add(subitem);

                    subitem = new ListViewItem.ListViewSubItem();
                    subitem.Name = "Region";
                    subitem.Text = e.Location.Region;
                    item.SubItems.Add(subitem);

                    subitem = new ListViewItem.ListViewSubItem();
                    subitem.Name = "Bitrate";
                    subitem.Text = e.Audio.Bitrate + "kbps";
                    item.SubItems.Add(subitem);

                    if (e == iEntry)
                    {
                        item.Selected = true;
                    }

                    iListView.Items.Add(item);
                }
            }
            else
            {
                iPanel.StartBusy();
                Visible = false;
            }

            iListView.Columns[0].Width = iListView.SmallIconSize.Width + 8;
            iListView.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);
            iListView.AutoResizeColumn(2, ColumnHeaderAutoResizeStyle.ColumnContent);

            iListView.EndUpdate();

            iMutex.ReleaseMutex();
        }

        private void PopulateDetails()
        {
            MemoryStream input = new MemoryStream();

            XmlTextWriter iwriter = new XmlTextWriter(input, Encoding.UTF8);

            iwriter.WriteStartElement("station");

            iwriter.WriteElementString("title", iEntry.Title);
            iwriter.WriteElementString("link", iEntry.Link);
            iwriter.WriteElementString("logo", iEntry.LogoUri);
            iwriter.WriteElementString("description", iEntry.Description);
            iwriter.WriteElementString("bitrate", iEntry.Audio.Bitrate.ToString());
            iwriter.WriteElementString("country", iEntry.Location.Country);
            iwriter.WriteElementString("region", iEntry.Location.Region);

            iwriter.WriteEndElement();

            iwriter.Flush();

            input.Seek(0, SeekOrigin.Begin);

            XmlTextReader ireader = new XmlTextReader(input);

            MemoryStream output = new MemoryStream();

            XmlTextWriter owriter = new XmlTextWriter(output, Encoding.UTF8);

            iTransform.Transform(ireader, iArguments, owriter);

            output.Seek(0, SeekOrigin.Begin);

            iMutex.WaitOne();

            iWebBrowser.DocumentStream = output;

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

            iWebBrowser.Visible = false;

            if (iLevel == 0)
            {
                PopulateFeeds();
                EnsureVisible();
            }
            else if (iLevel == 1)
            {
                PopulateEntries();
                EnsureVisible();
            }
            else if (iLevel == 2)
            {
                PopulateDetails();

                iWebBrowser.Visible = true;
            }
        }

        private void OnActivate(object obj, EventArgs e)
        {
            if (iLevel < 1) // prevents details view
            {
                iLevel++;

                if (iLevel == 1)
                {
                    int selected = iListView.SelectedIndices[0];

                    iFeed = iFeedList[selected];

                }
                else if (iLevel == 2)
                {
                    int selected = iListView.SelectedIndices[0];

                    iEntry = iFeed.EntryList[selected];
                }

                UpdateLocation();

                Populate();
            }
        }

        public void OnItemDrag(object sender, ItemDragEventArgs e)
        {
            if (iLevel == 0)
            {
                int selected = iListView.SelectedIndices[0];

                iFeed = iFeedList[selected];

                if (iFeed.Downloaded)
                {
                    List<upnpObject> list = new List<upnpObject>();

                    foreach (IEntry entry in iFeed.EntryList)
                    {
                        list.Add(CreateUpnpObject(iFeed, entry));
                    }

                    iSupport.PlaylistSupport.SetDragging(true);

                    DragDropEffects effect = iListView.DoDragDrop(new MediaProviderDraggable(new MediaRetrieverNoRetrieve(list)), DragDropEffects.All);

                    iSupport.PlaylistSupport.SetDragging(false);

                    if (effect == DragDropEffects.Move)
                    {
                        iLrf.Remove(iFeed);
                    }
                }
            }
            else if (iLevel == 1)
            {
                int selected = iListView.SelectedIndices[0];

                iEntry = iFeed.EntryList[selected];

                List<upnpObject> list = new List<upnpObject>();

                list.Add(CreateUpnpObject(iFeed, iEntry));

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

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            if ((e.Effect & DragDropEffects.Copy) == DragDropEffects.Copy || (e.AllowedEffect & DragDropEffects.Link) == DragDropEffects.Link)
            {
                if (e.Data.GetDataPresent("UniformResourceLocator"))
                {
                    MemoryStream stream = e.Data.GetData("UniformResourceLocator") as MemoryStream;

                    Uri uri = GetUri(stream);

                    if (uri != null)
                    {
                        DownloadFeed(uri);
                    }
                }
                else if (e.Data.GetDataPresent("Text"))
                {
                    string text = e.Data.GetData("Text") as string;

                    try
                    {
                        Uri uri = new Uri(text);

                        DownloadFeed(uri);
                    }
                    catch (UriFormatException)
                    {
                    }
                }
            }
        }

        private void DownloadFeed(Uri aUri)
        {
            FormDrop form = new FormDrop(this, iSupport, iLrf);

            IFeed feed = form.Download(aUri);

            if (feed != null)
            {
                iLrf.Add(feed);
            }
        }

        private void UpdateLocation()
        {
            bool drop = true;

            List<string> location = new List<string>();

            if (iLevel > 0)
            {
                drop = false;

                location.Add(iFeed.Title);
            }
            if (iLevel > 1)
            {
                location.Add(iEntry.Title);
            }

            iListView.AllowDrop = drop;

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

        private void EventFeedsChanged(object obj, EventArgs e)
        {
            iMutex.WaitOne();

            iFeedList = iLrf.FeedList;

            iMutex.ReleaseMutex();

            if (iFeed != null)
            {
                foreach (IFeed f in iFeedList)
                {
                    if (iFeed.Title == f.Title)
                    {
                        iFeed = f;

                        if (iEntry != null)
                        {
                            foreach (IEntry x in iFeed.EntryList)
                            {
                                if (iEntry.Title == x.Title)
                                {
                                    iEntry = x;

                                    break;
                                }
                            }
                        }

                        break;
                    }
                }
            }

            iListView.Invoke((MethodInvoker)Populate);
        }

        private upnpObject CreateUpnpObject(IFeed aFeed, IEntry aEntry)
        {
            audioItem item = new audioItem();
            resource resource = new resource();
            resource.Uri = aEntry.Audio.Uri;
            resource.Bitrate = (int)aEntry.Audio.Bitrate * 1000;
            item.Res.Add(resource);
            item.Id = aEntry.Audio.Uri;
            item.Title = aEntry.Title;
            item.Genre.Add(aEntry.Category);
            item.ArtworkUri.Add(aEntry.LogoUri);
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
        Lrf iLrf;
        Mutex iMutex;
        IList<IFeed> iFeedList;
        IFeed iFeed;
        IEntry iEntry;
        PanelBusy iPanel;
        ListViewKinsky iListView;
        WebBrowser iWebBrowser;
        XslCompiledTransform iTransform;
        XsltArgumentList iArguments;

        uint iViewSize;
        uint iLevel;
        List<string> iLocation;
    }

    class FormDrop : FormThemed, IFeedDownloadConsole
    {
        public FormDrop(IWin32Window aParent, IMediaProviderSupportV7 aSupport, Lrf aLrf)
        {
            iParent = aParent;
            iSupport = aSupport;
            iLrf = aLrf;
            iMutex = new Mutex();

            Font = new Font(iSupport.ViewSupport.FontSmall, FontStyle.Bold);
            ClientSize = new System.Drawing.Size(400, 230);
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = iSupport.ViewSupport.BackColour;
            ForeColor = iSupport.ViewSupport.ForeColour;

            iPanelMain = new Panel();
            iPanelMain.Size = ClientSize;
            iPanelMain.Location = ClientRectangle.Location;
            iPanelMain.BorderStyle = BorderStyle.None;

            iBoxLogo = new PictureBox();
            iBoxLogo.Size = new Size(100, 100);
            iBoxLogo.Location = new Point(150, 25);
            iBoxLogo.SizeMode = PictureBoxSizeMode.Zoom;

            iLabelStationCount = new Label();
            iLabelStationCount.Size = new Size(398, 25);
            iLabelStationCount.Location = new Point(1, 130);
            iLabelStationCount.Font = new Font(iSupport.ViewSupport.FontMedium, FontStyle.Bold);
            iLabelStationCount.TextAlign = ContentAlignment.MiddleCenter;

            iLabelMessage = new Label();
            iLabelMessage.Size = new Size(398, 25);
            iLabelMessage.Location = new Point(1, 155);
            iLabelMessage.Font = new Font(iSupport.ViewSupport.FontMedium, FontStyle.Bold);
            iLabelMessage.TextAlign = ContentAlignment.MiddleCenter;

            iButtonCancel = new Button();
            iButtonCancel.Location = new Point(120, 190);
            iButtonCancel.Size = new Size(75, 25);
            iButtonCancel.Text = "Cancel";

            iButtonOk = new Button();
            iButtonOk.Location = new Point(205, 190);
            iButtonOk.Size = new Size(75, 25);
            iButtonOk.Text = "Ok";
            iButtonOk.Enabled = false;

            iPanelMain.Controls.Add(iBoxLogo);
            iPanelMain.Controls.Add(iLabelStationCount);
            iPanelMain.Controls.Add(iLabelMessage);
            iPanelMain.Controls.Add(iButtonCancel);
            iPanelMain.Controls.Add(iButtonOk);

            Controls.Add(iPanelMain);
            iPanelMain.Location = ClientRectangle.Location;
            iPanelMain.Size = ClientSize;

            AcceptButton = iButtonOk;
            CancelButton = iButtonCancel;

            iPanelMain.MouseUp += new MouseEventHandler(PassMouseUp);
            iPanelMain.MouseDown += new MouseEventHandler(PassMouseDown);
            iPanelMain.MouseMove += new MouseEventHandler(PassMouseMove);
            iBoxLogo.MouseUp += new MouseEventHandler(PassMouseUp);
            iBoxLogo.MouseDown += new MouseEventHandler(PassMouseDown);
            iBoxLogo.MouseMove += new MouseEventHandler(PassMouseMove);
            iLabelStationCount.MouseUp += new MouseEventHandler(PassMouseUp);
            iLabelStationCount.MouseDown += new MouseEventHandler(PassMouseDown);
            iLabelStationCount.MouseMove += new MouseEventHandler(PassMouseMove);
            iLabelMessage.MouseUp += new MouseEventHandler(PassMouseUp);
            iLabelMessage.MouseDown += new MouseEventHandler(PassMouseDown);
            iLabelMessage.MouseMove += new MouseEventHandler(PassMouseMove);

            iButtonOk.Click += new EventHandler(DoOk);

            iTitle = "Linn Radio Feed";
            iLogo = null;
            iStationCount = 0;

            UpdateControls();
        }

        private void PassMouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        private void PassMouseUp(object sender, MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        private void PassMouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        private void DoOk(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void UpdateControls()
        {
            Lock();
            Text = "Downloading " + iTitle;
            iBoxLogo.Image = iLogo;
            iLabelStationCount.Text = iStationCount + " stations";
            Unlock();
        }

        private void Downloaded()
        {
            Text = "Downloaded " + iTitle;
            iLabelMessage.Text = "Download Complete";
            iButtonOk.Enabled = true;
        }

        private void Failed()
        {
            Text = "ERROR!";
            iLabelMessage.Text = "Download Failed";
            iButtonOk.Enabled = true;
            iButtonCancel.Enabled = false;
        }

        public IFeed Download(Uri aUri)
        {
            iUri = aUri;

            iThread = new Thread(new ThreadStart(Run));

            iThread.Start();

            DialogResult result = ShowDialog(iParent);

            iThread.Abort();
            iThread.Join();

            if (result == DialogResult.Cancel)
            {
                return (null);
            }

            return (iFeed);
        }

        private void Run()
        {
            try
            {
                iFeed = iLrf.Download(iUri, this);

                if (iFeed != null)
                {
                    Invoke((MethodInvoker)Downloaded);
                }
                else
                {
                    Invoke((MethodInvoker)Failed);
                }
            }
            catch (ThreadAbortException)
            {
            }
        }

        public void WriteTitle(string aTitle)
        {
            Lock();
            iTitle = aTitle;
            Unlock();
            Invoke((MethodInvoker)UpdateControls);
        }

        public void WriteLogo(Image aLogo)
        {
            Lock();
            iLogo = aLogo;
            Unlock();
            Invoke((MethodInvoker)UpdateControls);
        }

        public void WriteStationCount(int aStationCount)
        {
            Lock();
            iStationCount = aStationCount;
            Unlock();
            Invoke((MethodInvoker)UpdateControls);
        }

        private void Lock()
        {
            iMutex.WaitOne();
        }

        private void Unlock()
        {
            iMutex.ReleaseMutex();
        }

        private IWin32Window iParent;
        private IMediaProviderSupportV7 iSupport;
        private Lrf iLrf;
        private Mutex iMutex;
        private Panel iPanelMain;
        private PictureBox iBoxLogo;
        private Label iLabelStationCount;
        private Label iLabelMessage;
        private Button iButtonCancel;
        private Button iButtonOk;
        private Uri iUri;
        private Thread iThread;
        private IFeed iFeed;
        private string iTitle;
        private Image iLogo;
        private int iStationCount;
        private Point iLastMouseLocation;
    }
}
