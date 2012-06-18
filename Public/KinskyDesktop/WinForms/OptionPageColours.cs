using System;
using System.Drawing;

using Linn;


namespace KinskyDesktop
{
    public class OptionPageColours : OptionPage
    {
        public OptionPageColours()
            : base("Colours")
        {
            iBackground = new OptionColor("background", "Background", "The background colour of the application", Color.FromArgb(0, 0, 0).ToArgb());
            Add(iBackground);

            iHighlight = new OptionColor("highlight", "Highlight", "The colour of highlights in the application", Color.FromArgb(96, 96, 96).ToArgb());
            Add(iHighlight);

            iText = new OptionColor("text", "Text", "The colour of standard text", Color.FromArgb(191, 191, 191).ToArgb());
            Add(iText);

            iTextMuted = new OptionColor("textmuted", "Text muted", "The colour of muted text", Color.FromArgb(110, 110, 110).ToArgb());
            Add(iTextMuted);

            iTextBright = new OptionColor("textbright", "Text bright", "The colour of brightened text", Color.FromArgb(255, 255, 255).ToArgb());
            Add(iTextBright);

            iTextHighlight = new OptionColor("texthighlighted", "Text highlighted", "The colour of highlighted text", Color.FromArgb(255, 255, 255).ToArgb());
            Add(iTextHighlight);
        }

        public string Background
        {
            get { return iBackground.Value; }
            set { iBackground.Set(value); }
        }

        public string Highlight
        {
            get { return iHighlight.Value; }
            set { iHighlight.Set(value); }
        }

        public string Text
        {
            get { return iText.Value; }
            set { iText.Set(value); }
        }

        public string TextMuted
        {
            get { return iTextMuted.Value; }
            set { iTextMuted.Set(value); }
        }

        public string TextBright
        {
            get { return iTextBright.Value; }
            set { iTextBright.Set(value); }
        }

        public string TextHighlight
        {
            get { return iTextHighlight.Value; }
            set { iTextHighlight.Set(value); }
        }

        private Option iBackground;
        private Option iHighlight;
        private Option iText;
        private Option iTextMuted;
        private Option iTextBright;
        private Option iTextHighlight;
    }
}


