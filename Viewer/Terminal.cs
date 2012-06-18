using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Linn
{
    /*By overriding OnRender() we can accurately calculate a line count
     * allowing us to display only as many lines as will fit on the screen. */
    public class Terminal : System.Windows.Controls.Control
    {
        static Terminal()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Terminal), new FrameworkPropertyMetadata(typeof(Terminal)));
        }

        protected override void OnRender(DrawingContext aDrawingContext)
        {
            iFormattedText = new FormattedText(iText, CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight, new Typeface("Tahoma"), iFontSize, Brushes.Black);

            aDrawingContext.DrawText(iFormattedText, new Point(1, 1));

            base.OnRender(aDrawingContext);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.SizeChanged += EventResponseSizeChanged;
            iLineSize = CalculateLineSize();
            iMaxVisibleLinesCount = CalculateMaxVisibleLines();
            OnMaxVisibleLinesCountChanged();
            base.OnInitialized(e);
        }

        private void EventResponseSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.HeightChanged)
                return;

            int newMaxLineCount = CalculateMaxVisibleLines();

            if ( newMaxLineCount != iMaxVisibleLinesCount )
            {
                iMaxVisibleLinesCount = newMaxLineCount;
                OnMaxVisibleLinesCountChanged();
            }
        }

        private void OnMaxVisibleLinesCountChanged()
        {
            if (MaxVisibleLinesCountChanged != null)
            {
                MaxVisibleLinesCountChanged(this, new LineCountChangedEventArgs(iMaxVisibleLinesCount));
            }
        }

        private int CalculateMaxVisibleLines()
        {
            return (int)(ActualHeight / iLineSize +1);
        }

        private double CalculateLineSize()
        {
            FormattedText formattedText = new FormattedText("\n", CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight, new Typeface("Tahoma"), iFontSize, Brushes.Black);

            return formattedText.Height;
        }

        public int MaxVisibleLinesCount
        {
            get
            {
                return iMaxVisibleLinesCount;
            }
        }

        public string Text
        {
            get
            {
                return iText;
            }
            set
            {
                iText = value;
                InvalidateVisual();
            }
        }

        private string iText = "";
        private const int iFontSize = 11;
        private FormattedText iFormattedText;
        private double iLineSize;
        private int iMaxVisibleLinesCount;

        public EventHandler<LineCountChangedEventArgs> MaxVisibleLinesCountChanged;
    }

    public class LineCountChangedEventArgs : EventArgs
    {
        public LineCountChangedEventArgs(int aLineCount)
        {
            iLineCount = aLineCount;
        }

        public int LineCount
        {
            get
            {
                return iLineCount;
            }
        }
        private int iLineCount;
    }
}
