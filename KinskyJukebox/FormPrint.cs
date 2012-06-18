using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Linn;
using Linn.Control.Ssdp;
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;

namespace KinskyJukebox
{
    public partial class FormPrint : Form
    {
        public FormPrint(HelperKinskyJukebox aHelper, TreeView aPresets, bool aPreview, uint aPresetCount, DProgressChanged aProgressChanged) {
            iHelper = aHelper;
            iTreeViewPreset = aPresets;
            iDocumentPreview = aPreview;
            iPresetCount = aPresetCount;
            iProgressChanged = aProgressChanged;
            iUserOptionsApplication = aHelper.ApplicationOptions;
            InitializeComponent();
            if (aPreview) {
                this.Icon = Icon.FromHandle(Properties.Resources.PrintPreview.GetHicon());
            }
            else {
                this.Icon = Icon.FromHandle(Properties.Resources.Print.GetHicon());
            }

            int index = 0;
            printSectionCheckedListBox.Items.Add(ApplicationOptions.kToc);
            printSectionCheckedListBox.SetItemChecked(index++, iHelper.ApplicationOptions.IsPrintSectionEnabled(ApplicationOptions.kToc));
            foreach (TreeNode bookmark in aPresets.Nodes) {
                printSectionCheckedListBox.Items.Add(bookmark.Text);
                if (bookmark.Text == MediaCollection.SortTypeToString(MediaCollection.SortType.eAll)) {
                    iAllIndex = index;
                }
                printSectionCheckedListBox.SetItemChecked(index++, iHelper.ApplicationOptions.IsPrintSectionEnabled(bookmark.Text));
            }
            switch (iUserOptionsApplication.PrintPageLayout) {
                case ApplicationOptions.PrintPageLayoutOptions.ePortraitWithTrackDetails: {
                    iPageLayoutTrackDetailsPortrait.Checked = true;
                    break;
                }
                case ApplicationOptions.PrintPageLayoutOptions.ePortraitWithoutTrackDetails: {
                    iPageLayoutAlbumArtOnlyPortrait.Checked = true;
                    break;
                }
                case ApplicationOptions.PrintPageLayoutOptions.eLandscapeWithTrackDetails: {
                    iPageLayoutTrackDetailsLandscape.Checked = true;
                    break;
                }
                case ApplicationOptions.PrintPageLayoutOptions.eLandscapeWithoutTrackDetails: {
                    iPageLayoutAlbumArtOnlyLandscape.Checked = true;
                    break;
                }
            }

            iPagesPerSheet.SelectedIndex = iUserOptionsApplication.PrintPagesPerSheetIndex;
            iPrintOrderBooklet.Checked = iUserOptionsApplication.PrintOrderBooklet;

            if (iUserOptionsApplication.PrintDocumentType == ApplicationOptions.PrintDocumentTypeOptions.eRtf) {
                iDocTypeRtf.Checked = true;
            }
            else {
                iDocTypePdf.Checked = true;
            }
        }

        private void buttonSelectAll_Click(object sender, EventArgs e) {
            for (int i = 0; i < printSectionCheckedListBox.Items.Count; i++) {
                // don't allow 'All' to be printed by accident
                if (i != iAllIndex) {
                    printSectionCheckedListBox.SetItemChecked(i, true);
                }
            }
        }

        private void buttonSelectNone_Click(object sender, EventArgs e) {
            for (int i = 0; i < printSectionCheckedListBox.Items.Count; i++) {
                printSectionCheckedListBox.SetItemChecked(i, false);
            }
        }

        private void buttonPrintOK_Click(object sender, EventArgs e) {
            bool sectionsToPrint = false;
            int pagesPerSheet = 1;
            for (int i = 0; i < printSectionCheckedListBox.Items.Count; i++) {
                if (printSectionCheckedListBox.GetItemChecked(i)) {
                    sectionsToPrint = true;
                    break;
                }
            }
            if (iPagesPerSheet.SelectedItem != null) {
                pagesPerSheet = int.Parse(iPagesPerSheet.SelectedItem.ToString());
            }

            if (sectionsToPrint) {
                saveFileDialog.DefaultExt = "pdf";
                saveFileDialog.Filter = "Pdf File (*.pdf)|*.pdf|Rich Text File (*.rtf)|*.rtf";
                if (iDocTypeRtf.Checked) {
                    saveFileDialog.DefaultExt = "rtf";
                    saveFileDialog.Filter = "Rich Text File (*.rtf)|*.rtf|Pdf File (*.pdf)|*.pdf";
                }
                if (iDocumentPreview) {
                    string fileName = Path.GetTempPath() + iHelper.Title + System.Guid.NewGuid().ToString() + "Presets." + saveFileDialog.DefaultExt;
                    CreateDocument(fileName, pagesPerSheet);
                }
                else {
                    saveFileDialog.FileName = iHelper.Title + "Presets." + saveFileDialog.DefaultExt;
                    if (saveFileDialog.ShowDialog() == DialogResult.OK) {
                        CreateDocument(saveFileDialog.FileName, pagesPerSheet);
                    }
                    else {
                        this.DialogResult = DialogResult.Cancel;
                    }
                }
            }
            else {
                this.DialogResult = DialogResult.Cancel;
            }
            this.Close();
        }

        private void CreateDocument(string aFilename, int aPagesPerSheet) {
            // creat document in new thread
            iDocumentFilename = aFilename;
            iDocumentPagesPerSheet = aPagesPerSheet;
            iCreateDocumentThread = new Thread(CreateDocument);
            iCreateDocumentThread.Name = "CreateDocument";
            iCreateDocumentThread.IsBackground = true;
            iCreateDocumentThread.Start();
            Linn.UserLog.WriteLine("Print started");
        }

        public void StopPrint() {
            Linn.UserLog.WriteLine("Print stopped");
            iCreateDocumentThread.Abort();
        }

        private void CreateDocument() {
            iProgressChanged(0, "Catalog: Initialise", Progress.State.eInProgress);
            string fileName = Path.GetFullPath(iDocumentFilename);
            iTextSharp.text.Document document = null;
            bool pdf = true;
            if (iPageLayoutTrackDetailsLandscape.Checked || iPageLayoutAlbumArtOnlyLandscape.Checked) {
                document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate()); // landscape
            }
            else {
                document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4); // portrait
            }
            try {
                float fontSizePreset = 10F;
                float fontSizePresetNumber = 10F;
                float fontSizeBookmark = 12F;
                float fontSizePresetLarge = 32F;
                float fontSizePresetNumberLarge = 48F;

                iTextSharp.text.Font fontPreset = new iTextSharp.text.Font(iTextSharp.text.Font.HELVETICA, fontSizePreset, iTextSharp.text.Font.NORMAL, iTextSharp.text.Color.BLACK);
                iTextSharp.text.Font fontPresetNumber = new iTextSharp.text.Font(iTextSharp.text.Font.HELVETICA, fontSizePresetNumber, iTextSharp.text.Font.BOLD, iTextSharp.text.Color.BLACK);
                iTextSharp.text.Font fontBookmark = new iTextSharp.text.Font(iTextSharp.text.Font.HELVETICA, fontSizeBookmark, iTextSharp.text.Font.BOLD, iTextSharp.text.Color.BLUE);
                iTextSharp.text.Font fontPresetLarge = new iTextSharp.text.Font(iTextSharp.text.Font.HELVETICA, fontSizePresetLarge, iTextSharp.text.Font.NORMAL, iTextSharp.text.Color.BLACK);
                iTextSharp.text.Font fontPresetNumberLarge = new iTextSharp.text.Font(iTextSharp.text.Font.HELVETICA, fontSizePresetNumberLarge, iTextSharp.text.Font.BOLD, iTextSharp.text.Color.BLACK);

                if (Path.GetExtension(fileName) == ".rtf") {
                    iTextSharp.text.rtf.RtfWriter2.GetInstance(document, new FileStream(fileName, FileMode.Create));
                    pdf = false;
                }
                else {
                    iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create));
                    writer.ViewerPreferences = iTextSharp.text.pdf.PdfWriter.PageLayoutSinglePage;
                }
                document.Open();
                if (pdf) {
                    document.Add(new iTextSharp.text.Paragraph()); // pdf needs alignment on first page, rtf does not
                }
                // TOC
                bool firstPage = true;
                string text = "";
                if (printSectionCheckedListBox.GetItemChecked(0)) {
                    iProgressChanged(0, "Catalog: TOC", Progress.State.eInProgress);
                    if (iTreeViewPreset.Nodes.Count > 0) {
                        firstPage = false;
                        foreach (TreeNode bookmark in iTreeViewPreset.Nodes) {
                            if (bookmark.Index > 0) {
                                document.Add(new iTextSharp.text.Chunk("\n"));
                            }
                            document.Add(new iTextSharp.text.Phrase(fontSizeBookmark + 2, bookmark.Text, fontBookmark));
                            document.Add(new iTextSharp.text.Paragraph());
                            if (bookmark.Nodes.Count > 0) {
                                foreach (TreeNode preset in bookmark.Nodes) {
                                    document.Add(new iTextSharp.text.Chunk("0" + preset.Name, fontPresetNumber));
                                    text = preset.Text.Replace("/", " / ");
                                    document.Add(new iTextSharp.text.Phrase(fontSizePreset + 2, " - " + text + " (" + preset.Nodes.Count + ")", fontPreset));
                                    document.Add(new iTextSharp.text.Paragraph());
                                }
                            }
                        }
                    }
                }
                // One page per preset (with album covers)
                if (iTreeViewPreset.Nodes.Count > 0) {
                    int currItem = 0;
                    int totalItems = 0;
                    foreach (TreeNode bookmark in iTreeViewPreset.Nodes) {
                        if (printSectionCheckedListBox.GetItemChecked(bookmark.Index + 1)) {
                            totalItems += bookmark.Nodes.Count; // only add selected sections to total
                        }
                    }
                    foreach (TreeNode bookmark in iTreeViewPreset.Nodes) {
                        if (bookmark.Nodes.Count > 0 && iPresetCount > 0) {
                            if (!printSectionCheckedListBox.GetItemChecked(bookmark.Index+1)) {
                                // only print selected sections
                                continue;
                            }
                            foreach (TreeNode preset in bookmark.Nodes) {
                                iProgressChanged((++currItem * 100 / totalItems), "Catalog: Page " + currItem + " of " + totalItems, Progress.State.eInProgress);
                                if (!firstPage) {
                                    document.NewPage();
                                }
                                firstPage = false;
                                document.Add(new iTextSharp.text.Chunk("0" + preset.Name, fontPresetNumberLarge));
                                text = preset.Text.Replace("/", " / ");
                                document.Add(new iTextSharp.text.Phrase(fontSizePresetLarge + 2, "   " + text, fontPresetLarge));
                                document.Add(new iTextSharp.text.Paragraph());
                                document.Add(new iTextSharp.text.Chunk("\n"));
                                foreach (TreeNode track in preset.Nodes) {
                                    TrackMetadata data = (TrackMetadata)track.Tag;
                                    if (track.Index == 0) {
                                        try {
                                            iTextSharp.text.Image albumArt = iTextSharp.text.Image.GetInstance(new Uri(data.AlbumArtPath));

                                            if (iPageLayoutTrackDetailsLandscape.Checked || iPageLayoutAlbumArtOnlyLandscape.Checked) {
                                                // landscape
                                                if (iPageLayoutTrackDetailsLandscape.Checked) {
                                                    // include track details
                                                    albumArt.Alignment = iTextSharp.text.Image.LEFT_ALIGN | iTextSharp.text.Image.TEXTWRAP;
                                                    albumArt.IndentationRight = 36;
                                                    albumArt.ScaleToFit(350, 350);
                                                    // album art not lined up - use all available space for track details
                                                }
                                                else {
                                                    // no track details
                                                    if (pdf) {
                                                        albumArt.ScaleToFit(400, 400);
                                                    }
                                                    else {
                                                        albumArt.ScaleToFit(350, 350); // margins handled slightly differently
                                                    }
                                                    albumArt.SetAbsolutePosition(221, 32.5f); // all album art lined up in document
                                                }
                                            }
                                            else {
                                                // portrait
                                                if (iPageLayoutTrackDetailsPortrait.Checked) {
                                                    // include track details
                                                    albumArt.Alignment = iTextSharp.text.Image.LEFT_ALIGN;
                                                    albumArt.ScaleToFit(350, 350);
                                                    // album art not lined up - use all available space for track details
                                                }
                                                else {
                                                    // no track details
                                                    albumArt.ScaleToFit(525, 525);
                                                    albumArt.SetAbsolutePosition(35, 108.5f); // all album art lined up in document
                                                }
                                            }
                                            document.Add(albumArt);
                                            document.Add(new iTextSharp.text.Chunk("\n"));
                                        }
                                        catch { // invalid or missing album art
                                        }
                                    }
                                    if (iPageLayoutTrackDetailsLandscape.Checked || iPageLayoutTrackDetailsPortrait.Checked) { // include track details
                                        string trackInfo = (track.Index + 1).ToString().PadLeft(2,'0');
                                        trackInfo += ".  " + data.Title;
                                        trackInfo += " - " + data.Artist;
                                        trackInfo += " - " + data.Album;
                                        trackInfo += " (" + data.Duration;
                                        trackInfo += ")";
                                        document.Add(new iTextSharp.text.Phrase(fontSizePreset + 2, trackInfo, fontPreset));
                                        document.Add(new iTextSharp.text.Paragraph());
                                    }
                                }
                            }
                        }
                    }
                }
                iProgressChanged(100, "Catalog: Finalise", Progress.State.eInProgress);
            }
            catch (Exception exc) {
                string message = "Unable to create file: " + fileName + Environment.NewLine + exc.Message;
                if (exc.GetType() == typeof(ThreadAbortException)) {
                    message = "Operation was cancelled by the user" + Environment.NewLine;
                }
                Linn.UserLog.WriteLine("Print Failed: " + message);
                iProgressChanged(0, message, Progress.State.eFail);
                iProgressChanged(100, null, Progress.State.eComplete);
                return;
            }
            finally {
                try {
                    document.Close();
                }
                catch (System.IO.IOException) { // document has no pages
                }
            }

            try {
                if (pdf && iDocumentPagesPerSheet >= kMultiplePagesMin && iDocumentPagesPerSheet <= kMultiplePagesMax) {
                    string newName = Path.GetTempPath() + iHelper.Title + System.Guid.NewGuid().ToString() + "MultiplePagesPresets.pdf";
                    MultiplePages(iDocumentFilename, newName, iDocumentPagesPerSheet, (iPageLayoutTrackDetailsLandscape.Checked || iPageLayoutAlbumArtOnlyLandscape.Checked));
                    File.Delete(iDocumentFilename); // delete source file
                    File.Copy(newName, iDocumentFilename);
                }
            }
            catch (Exception e) {
                if (e.GetType() == typeof(ThreadAbortException)) {
                    string msg = "Operation was cancelled by the user" + Environment.NewLine;
                    Linn.UserLog.WriteLine(msg);
                    iProgressChanged(0, msg, Progress.State.eFail);
                    return;
                }
                else {
                    Linn.UserLog.WriteLine("Print (multiple pages) Failed: " + e.Message);
                }
            }
            finally {
                iProgressChanged(100, null, Progress.State.eComplete);
            }

            if (iDocumentPreview) {
                try {
                    System.Diagnostics.Process.Start(iDocumentFilename);
                }
                catch (Exception exc) {
                    string docType = "PDF";
                    if (!pdf) {
                        docType = "RTF";
                    }
                    Linn.UserLog.WriteLine("Unable to preview catalog (" + docType + " docuemnt support required): " + exc.Message);
                    MessageBox.Show("Unable to preview catalog: " + docType + " docuemnt support required", "Preview Failed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
            iProgressChanged(0, null, Progress.State.eSuccess);
            Linn.UserLog.WriteLine("Print complete");
        }

        private void MultiplePages(string aSourceFile, string aDestFile, int aPages, bool aLandscape) {
            iTextSharp.text.Document document = null;
            try {
                if (aPages < kMultiplePagesMin || aPages > kMultiplePagesMax) {
                    throw new Exception("Multiple Pages only supports " + kMultiplePagesMin + " to " + kMultiplePagesMin + " pages per sheet (Invalid value: " + aPages.ToString() + ")");
                }
                iProgressChanged(0, "Generating Multiple Pages Per Sheet: Initialising Document", Progress.State.eInProgress);
                iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(aSourceFile);
                int n = reader.NumberOfPages;

                // step 1: creation of a document-object
                if (aLandscape && aPages != 2 && aPages != 8 && aPages != 32) { // pages = 2,8,32: print landscape pages onto portrait document in landscape orientation
                    document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4.Rotate()); // landscape
                }
                else {
                    document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4); // portrait
                }

                // step 2: we create a writer that listens to the document
                iTextSharp.text.pdf.PdfWriter writer = iTextSharp.text.pdf.PdfWriter.GetInstance(document, new FileStream(aDestFile, FileMode.Create));
                writer.ViewerPreferences = iTextSharp.text.pdf.PdfWriter.PageLayoutSinglePage;

                // step 3: we open the document
                document.Open();
                iTextSharp.text.pdf.PdfContentByte cb = writer.DirectContent;
                iTextSharp.text.pdf.PdfImportedPage page = null;

                float[] xPoints = new float[7] { -1f, -1f, -1f, -1f, -1f, -1f, -1f };
                float[] yPoints = new float[9] { -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f };
                int[] xFactors = new int[37] { 0, 1, 1, 0, 2, 0, 0, 0, 2, 3, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 6 };
                int[] yFactors = new int[37] { 0, 1, 2, 0, 2, 0, 0, 0, 4, 3, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 6 };

                int xFactor = xFactors[aPages];
                int yFactor = yFactors[aPages];
                float xScaler = 1f / xFactor;
                float yScaler = 1f / yFactor;
                float docWidth = document.PageSize.Width;
                float docHeight = document.PageSize.Height;
                float specialScaler = (docWidth/docHeight) / xFactor;
                int pyBase = yFactor;
                if (!aLandscape) {
                    pyBase--;
                }

                for (uint i = 0; i <= xFactor; i++) {
                    xPoints[i] = (docWidth / xFactor) * i;
                }
                for (uint i = 0; i <= yFactor; i++) {
                    yPoints[i] = (docHeight / yFactor) * i;
                }
                int px = 0;
                int py = pyBase;
                int pxSpecial = xFactor - 1;
                int pySpecial = yFactor;
                int j = 0;
                int p = 0;

                // step 4: we add content
                int finalTotal = n / aPages;
                if (n % aPages != 0) {
                    finalTotal++;
                }

                int forward = 1;
                int backwardBase = (finalTotal * aPages) - (aPages/2) + 1;
                int backward = backwardBase; 
                int getPage = 0;
                bool flip = !aLandscape;
                int forward2 = 0;
                int backward2 = 0;
                bool booklet = iPrintOrderBooklet.Enabled && iPrintOrderBooklet.Checked;
                bool upsideDown = ((aPages == 4 || aPages == 16 || aPages == 36) && !aLandscape && booklet);
                bool upsideDownSpecial = ((aPages == 2 || aPages == 8 || aPages == 32) && aLandscape && booklet);
                if (upsideDownSpecial) {
                    upsideDown = true;
                    flip = true;
                }

                while (j < n) {
                    j++;
                    iProgressChanged((((j / aPages) + 1) * 100 / finalTotal), "Generating Multiple Pages Per Sheet: Page " + ((j / aPages) + 1) + " of " + finalTotal, Progress.State.eInProgress);

                    if (booklet) {
                        bool test = false;
                        if ((aPages == 2 || aPages == 8 || aPages == 32) && !aLandscape) {
                            test = (pySpecial <= (yFactor / 2));
                        }
                        else {
                            if (upsideDownSpecial) {
                                test = (py <= (yFactor / 2));
                            }
                            else if (upsideDown) {
                                test = (py < (yFactor / 2));
                            }
                            else {
                                test = (px < (xFactor / 2));
                            }
                        }
                        if (test) {
                            if (!flip) {
                                getPage = backward++;
                                backwardBase--;
                                if (upsideDown) {
                                    getPage = forward2--;
                                }
                            }
                            else {
                                getPage = forward++;
                            }
                        }
                        else {
                            if (!flip) {
                                getPage = forward++;
                                if (upsideDown) {
                                    getPage = backward2--;
                                }
                            }
                            else {
                                getPage = backward++;
                                backwardBase--;
                            }
                        }
                    }
                    else {
                        getPage = j;
                    }
                    if (getPage <= n) {
                        page = writer.GetImportedPage(reader, getPage);
                    }
                    else {
                        j--;
                    }

                    if (p == 0) {
                        // draw layout lines (once per destination document page)
                        cb.SetRGBColorStroke(0xC0, 0xC0, 0xC0);
                        foreach (float xPoint in xPoints) {
                            if (xPoint >= 0) {
                                cb.MoveTo(xPoint, 0);
                                cb.LineTo(xPoint, docHeight);
                                if (docWidth > docHeight && booklet) {
                                    if (xPoint == (docWidth / 2)) {
                                        cb.SetLineDash(2f, 4f, 0);
                                    }
                                }
                                cb.Stroke();
                                cb.SetLineDash(0);
                            }
                        }
                        foreach (float yPoint in yPoints) {
                            if (yPoint >= 0) {
                                cb.MoveTo(0, yPoint);
                                cb.LineTo(docWidth, yPoint);
                                if (docHeight > docWidth && booklet) {
                                    if (yPoint == (docHeight / 2)) {
                                        cb.SetLineDash(2f, 4f, 0);
                                    }
                                }
                                cb.Stroke();
                                cb.SetLineDash(0);
                            }
                        }
                    }

                    if (getPage <= n) {
                        if (aPages == 2 || aPages == 8 || aPages == 32) {
                            if (aLandscape) {
                                if (upsideDown && !flip) {
                                    cb.AddTemplate(page, 0, specialScaler, -specialScaler, 0, xPoints[px+1], yPoints[py-1]);
                                }
                                else {
                                    cb.AddTemplate(page, 0, -specialScaler, specialScaler, 0, xPoints[px], yPoints[py]);
                                }
                            }
                            else {
                                cb.AddTemplate(page, 0, -specialScaler, specialScaler, 0, xPoints[pxSpecial], yPoints[pySpecial]);
                            }
                        }
                        else {
                            if (aLandscape) {
                                cb.AddTemplate(page, 0, -xScaler, yScaler, 0, xPoints[px], yPoints[py]);
                            }
                            else {
                                if (upsideDown && !flip) {
                                    cb.AddTemplate(page, -xScaler, 0, 0, -yScaler, xPoints[px + 1], yPoints[py + 1]);
                                }
                                else {
                                    cb.AddTemplate(page, xScaler, 0, 0, yScaler, xPoints[px], yPoints[py]);
                                }
                            }
                        }
                    }

                    cb.Stroke();
                    p++;
                    if ((p % xFactor) == 0) {
                        px = 0;
                        py--;
                    }
                    else {
                        px++;
                    }
                    if ((p % yFactor) == 0) {
                        pxSpecial--;
                        pySpecial = yFactor;
                    }
                    else {
                        pySpecial--;
                    }
                    if (p == aPages) {
                        p = 0;
                        px = 0;
                        py = pyBase;
                        pxSpecial = xFactor - 1;
                        pySpecial = yFactor;
                        backward = backwardBase;
                        forward2 = (forward - 1) + (aPages / 2);
                        backward2 = (backward - 1) + (aPages / 2);
                        flip = !flip;
                        document.NewPage();
                    }
                }
                iProgressChanged(100, "Generating Multiple Pages Per Sheet: Finalising Document", Progress.State.eInProgress);
            }
            catch (Exception e) {
                throw e;
            }
            finally {
                try {
                    // step 5: we close the document
                    if (document != null) {
                        document.Close();
                    }
                }
                catch (System.IO.IOException) { // document has no pages
                }
            }
        }

        private void iDocType_CheckedChanged(object sender, EventArgs e) {
            if (iDocTypePdf.Checked) {
                iPagesPerSheet.Enabled = true;
                iPagesPerSheetLabel.Enabled = true;
            }
            else {
                iPagesPerSheet.Enabled = false;
                iPagesPerSheetLabel.Enabled = false;
            }
            iPagesPerSheet_SelectedIndexChanged(sender, e);
        }

        private void iPagesPerSheet_SelectedIndexChanged(object sender, EventArgs e) {
            if (iDocTypeRtf.Checked || iPagesPerSheet.SelectedItem == null) {
                iPrintOrderBooklet.Enabled = false;
            }
            else {
                iPrintOrderBooklet.Enabled = ((int.Parse(iPagesPerSheet.SelectedItem.ToString()) % 2) == 0);
            }
        }

        private void FormPrint_FormClosing(object sender, FormClosingEventArgs e) {
            SavePrintOptions();
        }

        private void SavePrintOptions() {
            iHelper.ApplicationOptions.ClearPrintSections();
            for (int i = 0; i < printSectionCheckedListBox.Items.Count; i++ ) {
                string section = printSectionCheckedListBox.Items[i] as string;
                bool enabled = (printSectionCheckedListBox.GetItemCheckState(i) == CheckState.Checked);
                if (enabled) {
                    iHelper.ApplicationOptions.AddPrintSection(section); // list is enabled sections only
                }
            }
            if (iPageLayoutTrackDetailsPortrait.Checked) {
                iUserOptionsApplication.PrintPageLayout = ApplicationOptions.PrintPageLayoutOptions.ePortraitWithTrackDetails;
            }
            else if (iPageLayoutAlbumArtOnlyPortrait.Checked) {
                iUserOptionsApplication.PrintPageLayout = ApplicationOptions.PrintPageLayoutOptions.ePortraitWithoutTrackDetails;
            }
            else if (iPageLayoutTrackDetailsLandscape.Checked) {
                iUserOptionsApplication.PrintPageLayout = ApplicationOptions.PrintPageLayoutOptions.eLandscapeWithTrackDetails;
            }
            else if (iPageLayoutAlbumArtOnlyLandscape.Checked) {
                iUserOptionsApplication.PrintPageLayout = ApplicationOptions.PrintPageLayoutOptions.eLandscapeWithoutTrackDetails;
            }

            iUserOptionsApplication.PrintPagesPerSheetIndex = iPagesPerSheet.SelectedIndex;
            iUserOptionsApplication.PrintOrderBooklet = iPrintOrderBooklet.Checked;

            if (iDocTypePdf.Checked) {
                iUserOptionsApplication.PrintDocumentType = ApplicationOptions.PrintDocumentTypeOptions.ePdf;
            }
            else {
                iUserOptionsApplication.PrintDocumentType = ApplicationOptions.PrintDocumentTypeOptions.eRtf;
            }
        }

        public delegate void DProgressChanged(int aPercent, string aMessage, Progress.State aProgressState);
        private HelperKinskyJukebox iHelper;
        private TreeView iTreeViewPreset;
        private bool iDocumentPreview = false;
        private Thread iCreateDocumentThread = null;
        private string iDocumentFilename = null;
        private int iDocumentPagesPerSheet = 1;
        private DProgressChanged iProgressChanged;
        private uint iPresetCount = 0;
        private int iAllIndex = -1;
        private ApplicationOptions iUserOptionsApplication;

        private readonly int kMultiplePagesMin = 2;
        private readonly int kMultiplePagesMax = 36;
    }
}

