using System;
using System.Drawing;
using System.Net;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

using Linn;
using Linn.Control.Ssdp;
using Linn.ControlPoint.Upnp;
using Linn.Topology;
using Linn.Kinsky;
using Upnp;

namespace KinskyDesktop
{
    internal class MediaProviderSupport : IContentDirectorySupportV2
    {
        public MediaProviderSupport(IVirtualFileSystem aVirtualFileSystem)
        {

            iVirtualFileSystem = aVirtualFileSystem;
        }

        public IVirtualFileSystem VirtualFileSystem
        {
            get
            {
                return iVirtualFileSystem;
            }
        }

        private IVirtualFileSystem iVirtualFileSystem;
    }

    public interface IViewSupport
    {
        Color BackColour { get; }

        Color ForeColourMuted { get; }
        Color ForeColour { get; }
        Color ForeColourBright { get; }

        Color HighlightBackColour { get; }
        Color HighlightForeColour { get; }

        Font FontLarge { get; }
        Font FontMedium { get; }
        Font FontSmall { get; }

        event EventHandler<EventArgs> EventSupportChanged;
    }

    internal class ViewSupport : IViewSupport
    {
        public ViewSupport(OptionPageColours aOptionPageColours, OptionPageFonts aOptionPageFonts)
        {
            iOptionPageColours = aOptionPageColours;
            iOptionPageFonts = aOptionPageFonts;

            iFontSmall = new Font("Arial", iOptionPageFonts.SmallSize, FontStyle.Bold);
            iFontMedium = new Font("Arial", iOptionPageFonts.MediumSize, FontStyle.Bold);
            iFontLarge = new Font("Arial", iOptionPageFonts.LargeSize, FontStyle.Bold);

            iOptionPageColours.EventChanged += OptionPageColoursChanged;
            iOptionPageFonts.EventChanged += OptionPageFontsChanged;
        }

        public Color BackColour
        {
            get { return ToColor(iOptionPageColours.Background); }
        }

        public Color ForeColourMuted
        {
            get { return ToColor(iOptionPageColours.TextMuted); }
        }

        public Color ForeColour
        {
            get { return ToColor(iOptionPageColours.Text); }
        }

        public Color HighlightBackColour
        {
            get { return ToColor(iOptionPageColours.Highlight); }
        }

        public Color HighlightForeColour
        {
            get { return ToColor(iOptionPageColours.TextHighlight); }
        }

        public Color ForeColourBright
        {
            get { return ToColor(iOptionPageColours.TextBright); }
        }

        public Font FontLarge
        {
            get { return iFontLarge; }
        }
        
        public Font FontMedium
        {
            get { return iFontMedium; }
        }

        public Font FontSmall
        {
            get { return iFontSmall; }
        }

        public event EventHandler<EventArgs> EventSupportChanged;

        private Color ToColor(string argb)
        {
            return Color.FromArgb(int.Parse(argb));
        }

        private void OptionPageColoursChanged(object sender, EventArgs e)
        {
            if (EventSupportChanged != null)
            {
                EventSupportChanged(sender, e);
            }
        }

        private void OptionPageFontsChanged(object sender, EventArgs e)
        {
            iFontSmall = new Font("Arial", iOptionPageFonts.SmallSize, FontStyle.Bold);
            iFontMedium = new Font("Arial", iOptionPageFonts.MediumSize, FontStyle.Bold);
            iFontLarge = new Font("Arial", iOptionPageFonts.LargeSize, FontStyle.Bold);

            if (EventSupportChanged != null)
            {
                EventSupportChanged(sender, e);
            }
        }

        private OptionPageColours iOptionPageColours;
        private OptionPageFonts iOptionPageFonts;
        private Font iFontLarge;
        private Font iFontMedium;
        private Font iFontSmall;
    }

} // KinskyDesktop
