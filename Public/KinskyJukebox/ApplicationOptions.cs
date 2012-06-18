using System;
using System.Drawing;
using System.Collections.Generic;
using Linn;

namespace KinskyJukebox
{
    public class ApplicationOptions
    {
        public static string kToc = "Table of Contents";

        public enum PrintPageLayoutOptions
        {
            ePortraitWithTrackDetails,
            eLandscapeWithTrackDetails,
            ePortraitWithoutTrackDetails,
            eLandscapeWithoutTrackDetails,
        }
        public enum PrintDocumentTypeOptions
        {
            ePdf,
            eRtf
        }

        public ApplicationOptions(IHelper aHelper) {
            iLeftSplitterLocation = new OptionInt("leftsplitter", "Left Splitter Location", "Graphical location in pixels of left splitter", 300);
            aHelper.AddOption(iLeftSplitterLocation);

            iRightSplitterLocation = new OptionInt("rightsplitter", "Right Splitter Location", "Graphical location in pixels of right splitter", 300);
            aHelper.AddOption(iRightSplitterLocation);

            iWindowWidth = new OptionInt("width", "Window Width", "Width of application window in pixels", 900);
            aHelper.AddOption(iWindowWidth);

            iWindowHeight = new OptionInt("height", "Window Height", "Height of application window in pixels", 600);
            aHelper.AddOption(iWindowHeight);

            iWindowMaximised = new OptionBool("maximised", "Window Maximised", "Flag to determine if the application window is maximised", false);
            aHelper.AddOption(iWindowMaximised);

            iWindowMinimised = new OptionBool("minimised", "Window Minimised", "Flag to determine if the application window is minimised", false);
            aHelper.AddOption(iWindowMinimised);

            iPrintPagesPerSheetIndex = new OptionInt("printpagespersheet", "Print Pages Per Sheet Index", "Index value of pages per sheet selection for printing a catalog (last selected by user)", 0);
            aHelper.AddOption(iPrintPagesPerSheetIndex);

            iPrintPageLayout = new OptionEnum("printpagelayout", "Print Page Layout", "Page layout selection for printing a catalog (last selected by user)");
            iPrintPageLayout.AddDefault(kPotraitTrackDetails);
            iPrintPageLayout.Add(kLandscapeTrackDetails);
            iPrintPageLayout.Add(kPotrait);
            iPrintPageLayout.Add(kLandscape);
            aHelper.AddOption(iPrintPageLayout);

            iPrintOrderBooklet = new OptionBool("printorderbooklet", "Print Order Booklet", "Booklet order selection for printing a catalog (last selected by user)", false);
            aHelper.AddOption(iPrintOrderBooklet);

            iPrintDocumentType = new OptionEnum("printdoctype", "Print Document Type", "Document type selection for printing a catalog (last selected by user)");
            iPrintDocumentType.AddDefault(kPdf);
            iPrintDocumentType.Add(kRtf);
            aHelper.AddOption(iPrintDocumentType);

            iPrintSections = new OptionListString("printsections", "Print Sections", "User selected sections to print when creating a catalog", new List<string>() { kToc });
            aHelper.AddOption(iPrintSections);
        }

        public int LeftSplitterLocation {
            get {
                return iLeftSplitterLocation.Native;
            }
            set {
                iLeftSplitterLocation.Native = value;
            }
        }

        public int RightSplitterLocation {
            get {
                return iRightSplitterLocation.Native;
            }
            set {
                iRightSplitterLocation.Native = value;
            }
        }

        public Size WindowSize {
            get {
                return new Size(iWindowWidth.Native, iWindowHeight.Native);
            }
            set {
                iWindowWidth.Native = value.Width;
                iWindowHeight.Native = value.Height;
            }
        }

        public bool WindowMaximised {
            get {
                return iWindowMaximised.Native;
            }
            set {
                iWindowMaximised.Native = value;
            }
        }

        public bool WindowMinimised {
            get {
                return iWindowMinimised.Native;
            }
            set {
                iWindowMinimised.Native = value;
            }
        }

        public int PrintPagesPerSheetIndex {
            get {
                return iPrintPagesPerSheetIndex.Native;
            }
            set {
                iPrintPagesPerSheetIndex.Native = value;
            }
        }

        public PrintPageLayoutOptions PrintPageLayout {
            get {
                if (iPrintPageLayout.Value == kPotrait) {
                    return PrintPageLayoutOptions.ePortraitWithoutTrackDetails;
                }
                else if (iPrintPageLayout.Value == kLandscape) {
                    return PrintPageLayoutOptions.eLandscapeWithoutTrackDetails;
                }
                else if (iPrintPageLayout.Value == kLandscapeTrackDetails) {
                    return PrintPageLayoutOptions.eLandscapeWithTrackDetails;
                }
                else {
                    return PrintPageLayoutOptions.ePortraitWithTrackDetails;
                }
            }
            set {
                if (value == PrintPageLayoutOptions.ePortraitWithoutTrackDetails) {
                    iPrintPageLayout.Set(kPotrait);
                }
                else if (value == PrintPageLayoutOptions.eLandscapeWithoutTrackDetails) {
                    iPrintPageLayout.Set(kLandscape);
                }
                else if (value == PrintPageLayoutOptions.eLandscapeWithTrackDetails) {
                    iPrintPageLayout.Set(kLandscapeTrackDetails);
                }
                else {
                    iPrintPageLayout.Set(kPotraitTrackDetails);
                }
            }
        }

        public bool PrintOrderBooklet {
            get {
                return iPrintOrderBooklet.Native;
            }
            set {
                iPrintOrderBooklet.Native = value;
            }
        }

        public PrintDocumentTypeOptions PrintDocumentType {
            get {
                if (iPrintDocumentType.Value == kRtf) {
                    return PrintDocumentTypeOptions.eRtf;
                }
                else {
                    return PrintDocumentTypeOptions.ePdf;
                }
            }
            set {
                if (value == PrintDocumentTypeOptions.eRtf) {
                    iPrintDocumentType.Set(kRtf);
                }
                else {
                    iPrintDocumentType.Set(kPdf);
                }
            }
        }

        public void ClearPrintSections() {
            iPrintSections.Clear();
        }

        public void AddPrintSection(string aSection) {
            iPrintSections.Add(aSection);
        }

        public bool IsPrintSectionEnabled(string aSection) {
            if (iPrintSections.Native.Contains(aSection)) {
                return true;
            }
            return false;
        }

        public void ResetToDefaults() {
            iLeftSplitterLocation.ResetToDefault();
            iRightSplitterLocation.ResetToDefault();
            iWindowWidth.ResetToDefault();
            iWindowHeight.ResetToDefault();
            iWindowMaximised.ResetToDefault();
            iWindowMinimised.ResetToDefault();
            iPrintPagesPerSheetIndex.ResetToDefault();
            iPrintPageLayout.ResetToDefault();
            iPrintOrderBooklet.ResetToDefault();
            iPrintDocumentType.ResetToDefault();
            iPrintSections.ResetToDefault();
        }

        private static string kPdf = "Pdf";
        private static string kRtf = "Rtf";
        private static string kPotrait = "Portait";
        private static string kLandscape = "Landscape";
        private static string kPotraitTrackDetails = "Portait Track Details";
        private static string kLandscapeTrackDetails = "Landscape Track Details";

        private OptionInt iLeftSplitterLocation;
        private OptionInt iRightSplitterLocation;
        private OptionInt iWindowWidth;
        private OptionInt iWindowHeight;
        private OptionBool iWindowMaximised;
        private OptionBool iWindowMinimised;
        private OptionInt iPrintPagesPerSheetIndex;
        private OptionEnum iPrintPageLayout;
        private OptionBool iPrintOrderBooklet;
        private OptionEnum iPrintDocumentType;
        private OptionListString iPrintSections;
    }
}