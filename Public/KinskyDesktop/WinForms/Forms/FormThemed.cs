using System;
using System.Windows.Forms;
using System.Drawing;

namespace KinskyDesktop
{
    public partial class FormThemed : Form
    {
        private enum ESizeType
        {
            eLeft,
            eRight,
            eTop,
            eBottom,
            eTopLeft,
            eTopRight,
            eBottomLeft,
            eBottomRight,
            eNone
        }

        private const uint kSizeBorder = 5;
        private const uint kCornerSizeSquare = 15;

        private bool iUseTheme;
        private Rectangle iClientRectangle;
        private Size iClientSize;

        private bool iAllowResize;
        private bool iDragging;
        private Point iLastMouseLocation;
        private ESizeType iSizeType;

        private Image iImageUpperLeft;
        private Image iImageUpper;
        private Image iImageUpperRight;
        private Image iImageLeft;
        private Image iImageBackground;
        private Image iImageRight;
        private Image iImageLowerLeft;
        private Image iImageLower;
        private Image iImageLowerRight;

        public FormThemed()
        {
            InitializeComponent();

            iUseTheme = true;
            iSizeType = ESizeType.eNone;
            
            iClientRectangle = base.ClientRectangle;
            iClientSize = base.ClientSize;

            iImageUpperLeft = Linn.Kinsky.Properties.Resources.DialogBoxWing1;
            iImageUpper = Linn.Kinsky.Properties.Resources.DialogBoxTopFiller;
            iImageUpperRight = Linn.Kinsky.Properties.Resources.DialogBoxWing2;
            iImageLeft = Linn.Kinsky.Properties.Resources.DialogBoxLeftFiller;
            iImageBackground = Linn.Kinsky.Properties.Resources.DialogBoxBackground;
            iImageRight = Linn.Kinsky.Properties.Resources.DialogBoxRightFiller;
            iImageLowerLeft = Linn.Kinsky.Properties.Resources.DialogBoxWing4;
            iImageLower = Linn.Kinsky.Properties.Resources.DialogBoxBottomFiller;
            iImageLowerRight = Linn.Kinsky.Properties.Resources.DialogBoxWing3;
        }

        public bool UseTheme
        {
            get
            {
                return iUseTheme;
            }
            set
            {
                iUseTheme = value;
                if (iUseTheme)
                {
                    BackColor = Color.FromArgb(255, 128, 128);
                    TransparencyKey = BackColor;
                    FormBorderStyle = FormBorderStyle.None;
                    ButtonClose.Visible = true;
                    ButtonMaximize.Visible = WindowState != FormWindowState.Maximized && MaximizeBox;
                    ButtonMinimize.Visible = MinimizeBox;
                    ButtonRestore.Visible = !ButtonMaximize.Visible && MaximizeBox;
                }
                else
                {
                    TransparencyKey = Color.White;
                    FormBorderStyle = FormBorderStyle.Sizable;
                    ButtonClose.Visible = false;
                    ButtonMaximize.Visible = false;
                    ButtonMinimize.Visible = false;
                    ButtonRestore.Visible = false;
                }
            }
        }

        public bool AllowThemeResize
        {
            get
            {
                return iAllowResize;
            }
            set
            {
                iAllowResize = value;
            }
        }

        public new Rectangle ClientRectangle
        {
            get
            {
                if (iUseTheme)
                {
                    return iClientRectangle;
                }
                else
                {
                    return base.ClientRectangle;
                }
            }
        }

        public new Size ClientSize
        {
            get
            {
                if (iUseTheme)
                {
                    return iClientSize;
                }
                else
                {
                    return base.ClientSize;
                }
            }
            set
            {
                if (iUseTheme)
                {
                    Size = new Size(value.Width + iImageLeft.Width + iImageRight.Width, value.Height + iImageUpper.Height + iImageLower.Height);
                }
                else
                {
                    base.ClientSize = value;
                }
            }
        }

        public new Size Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                base.Size = value;
                SetClientRectangle();
            }
        }

        public new bool MaximizeBox
        {
            get
            {
                return base.MaximizeBox;
            }
            set
            {
                if (iUseTheme)
                {
                    ButtonMaximize.Visible = value;
                    ButtonRestore.Visible = false;
                }
                if (value)
                {
                    ButtonMinimize.Location = new Point(ButtonMaximize.Location.X - ButtonMinimize.Width, ButtonMinimize.Location.Y);
                }
                else
                {
                    ButtonMinimize.Location = new Point(ButtonMaximize.Location.X, ButtonMinimize.Location.Y);
                }
                base.MaximizeBox = value;
            }
        }

        public new bool MinimizeBox
        {
            get
            {
                return base.MinimizeBox;
            }
            set
            {
                if (iUseTheme)
                {
                    ButtonMinimize.Visible = value;
                }
                base.MinimizeBox = value;
            }
        }

        public new string Text
        {
            get
            {
                return base.Text;
            }
            set
            {
                base.Text = value;
                Invalidate();
            }
        }

        public Image ImageUpperLeft
        {
            get
            {
                return iImageUpperLeft;
            }
            set
            {
                iImageUpperLeft = value;
                SetClientRectangle();
            }
        }

        public Image ImageUpper
        {
            get
            {
                return iImageUpper;
            }
            set
            {
                iImageUpper = value;
                SetClientRectangle();
            }
        }

        public Image ImageUpperRight
        {
            get
            {
                return iImageUpperRight;
            }
            set
            {
                iImageUpperRight = value;
                SetClientRectangle();
            }
        }

        public Image ImageLeft
        {
            get
            {
                return iImageLeft;
            }
            set
            {
                iImageLeft = value;
                SetClientRectangle();
            }
        }

        public Image ImageBackground
        {
            get
            {
                return iImageBackground;
            }
            set
            {
                iImageBackground = value;
                SetClientRectangle();
            }
        }

        public Image ImageRight
        {
            get
            {
                return iImageRight;
            }
            set
            {
                iImageRight = value;
                SetClientRectangle();
            }
        }

        public Image ImageLowerLeft
        {
            get
            {
                return iImageLowerLeft;
            }
            set
            {
                iImageLowerLeft = value;
                SetClientRectangle();
            }
        }

        public Image ImageLower
        {
            get
            {
                return iImageLower;
            }
            set
            {
                iImageLower = value;
                SetClientRectangle();
            }
        }

        public Image ImageLowerRight
        {
            get
            {
                return iImageLowerRight;
            }
            set
            {
                iImageLowerRight = value;
                SetClientRectangle();
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (iUseTheme)
            {
                if (MaximizeBox)
                {
                    if (iImageUpper != null)
                    {
                        int width = ButtonClose.Location.X;
                        if (MaximizeBox)
                        {
                            width = ButtonMaximize.Location.X;
                        }
                        if (MinimizeBox)
                        {
                            width = ButtonMinimize.Location.X;
                        }
                        Rectangle r = new Rectangle(0, 0, width, iImageUpper.Height);
                        if (r.Contains(e.Location))
                        {
                            if (WindowState == FormWindowState.Normal)
                            {
                                ButtonMaximize_EventClick(this, EventArgs.Empty);
                            }
                            else if (WindowState == FormWindowState.Maximized)
                            {
                                ButtonRestore_EventClick(this, EventArgs.Empty);
                            }
                        }
                    }
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                iDragging = true;
                iLastMouseLocation = PointToScreen(new Point(e.X, e.Y));
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (iDragging)
            {
                Point current = PointToScreen(new Point(e.X, e.Y));
                if (iSizeType == ESizeType.eNone)
                {
                    MoveForm(current);
                }
                else
                {
                    ResizeForm(current);
                }
            }
            else
            {
                if (iAllowResize)
                {
                    if (e.X < kSizeBorder && e.Y > kCornerSizeSquare && e.Y < Height - kCornerSizeSquare)
                    {
                        SetSizeType(ESizeType.eLeft);
                    }
                    else if (e.X > Width - kSizeBorder && e.Y > kCornerSizeSquare && e.Y < Height - kCornerSizeSquare)
                    {
                        SetSizeType(ESizeType.eRight);
                    }
                    else if (e.Y < kSizeBorder && e.X > kCornerSizeSquare && e.X < Width - kCornerSizeSquare)
                    {
                        SetSizeType(ESizeType.eTop);
                    }
                    else if (e.Y > Height - kSizeBorder && e.X > kCornerSizeSquare && e.X < Width - kCornerSizeSquare)
                    {
                        SetSizeType(ESizeType.eBottom);
                    }
                    else if (e.X < kSizeBorder && e.Y < kCornerSizeSquare || e.Y < kSizeBorder && e.X < kCornerSizeSquare)
                    {
                        SetSizeType(ESizeType.eTopLeft);
                    }
                    else if (e.Y < kSizeBorder && e.X > Width - kCornerSizeSquare || e.X < kSizeBorder && e.Y < kCornerSizeSquare)
                    {
                        SetSizeType(ESizeType.eTopRight);
                    }
                    else if (e.X < kSizeBorder && e.Y > Height - kCornerSizeSquare || e.Y > Height - kSizeBorder && e.X < kCornerSizeSquare)
                    {
                        SetSizeType(ESizeType.eBottomLeft);
                    }
                    else if (e.Y > Height - kSizeBorder && e.X > Width - kCornerSizeSquare || e.X > Width - kSizeBorder && e.Y > Height - kCornerSizeSquare)
                    {
                        SetSizeType(ESizeType.eBottomRight);
                    }
                    else
                    {
                        SetSizeType(ESizeType.eNone);
                    }
                }
                else
                {
                    SetSizeType(ESizeType.eNone);
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            iDragging = false;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            iDragging = false;
            SetSizeType(ESizeType.eNone);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            ButtonClose.Location = new Point(Width - ButtonClose.Width - 5, ButtonClose.Location.Y);
            ButtonMaximize.Location = new Point(ButtonClose.Location.X - ButtonMaximize.Width, ButtonMaximize.Location.Y);
            ButtonRestore.Location = ButtonMaximize.Location;
            if (MaximizeBox)
            {
                ButtonMinimize.Location = new Point(ButtonMaximize.Location.X - ButtonMinimize.Width, ButtonMinimize.Location.Y);
            }
            else
            {
                ButtonMinimize.Location = new Point(ButtonClose.Location.X - ButtonMinimize.Width, ButtonMinimize.Location.Y);
            }
            SetClientRectangle();

            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            if (iUseTheme)
            {
                // draw the header
                e.Graphics.DrawImage(iImageUpperLeft, 0, 0);
                e.Graphics.DrawImage(iImageUpperRight, base.ClientRectangle.Width - iImageUpperRight.Width, 0);

                // draw the footer
                e.Graphics.DrawImage(iImageLowerLeft, 0, base.ClientRectangle.Height - iImageLowerLeft.Height);
                e.Graphics.DrawImage(iImageLowerRight, base.ClientRectangle.Width - iImageLowerRight.Width, base.ClientRectangle.Height - iImageLowerRight.Height);

                int width = base.ClientRectangle.Width - iImageUpperLeft.Width - iImageUpperRight.Width;
                if (width > 0)
                {
                    using (TextureBrush brushHeader = new TextureBrush(iImageUpper))
                    {
                        brushHeader.TranslateTransform(0, 0);
                        e.Graphics.FillRectangle(brushHeader, iImageUpperLeft.Width, 0, width, iImageUpper.Height);
                    }
                }

                using (SolidBrush b = new SolidBrush(ForeColor))
                {
                    StringFormat f = new StringFormat();
                    f.Alignment = StringAlignment.Center;
                    f.LineAlignment = StringAlignment.Center;
                    f.FormatFlags = StringFormatFlags.NoWrap;
                    f.Trimming = StringTrimming.EllipsisCharacter;
                    int right = ButtonClose.Location.X;
                    if (MinimizeBox)
                    {
                        right = ButtonMinimize.Location.X;
                    }
                    else if (MaximizeBox)
                    {
                        right = ButtonMaximize.Location.X;
                    }
                    int left = Width - right;
                    int w = right - left;
                    e.Graphics.DrawString(Text, Font, b, new Rectangle(left, 0, w, iImageUpper.Height), f);
                }

                width = base.ClientRectangle.Width - iImageLowerLeft.Width - iImageLowerRight.Width;
                if (width > 0)
                {
                    using (TextureBrush brushFooter = new TextureBrush(iImageLower))
                    {
                        brushFooter.TranslateTransform(iImageLowerLeft.Width, base.ClientRectangle.Height - iImageLower.Height);
                        e.Graphics.FillRectangle(brushFooter, iImageLowerLeft.Width, base.ClientRectangle.Height - iImageLower.Height, width, iImageLower.Height);
                    }
                }

                // draw the side bars
                int height = base.ClientRectangle.Height - iImageUpper.Height - iImageLower.Height;
                if (height > 0)
                {
                    using (TextureBrush brushWindow = new TextureBrush(iImageLeft))
                    {
                        brushWindow.TranslateTransform(0, iImageLeft.Height);
                        e.Graphics.FillRectangle(brushWindow, 0, iImageUpperLeft.Height, iImageLeft.Width, height);
                    }
                    using (TextureBrush brushWindow = new TextureBrush(iImageRight))
                    {
                        brushWindow.TranslateTransform(base.ClientRectangle.Width - iImageRight.Width, iImageUpperRight.Height);
                        e.Graphics.FillRectangle(brushWindow, base.ClientRectangle.Width - iImageRight.Width, iImageUpperRight.Height, iImageRight.Width, height);
                    }
                }

                width = base.ClientRectangle.Width - iImageLeft.Width - iImageRight.Width;
                if (width > 0 && height > 0)
                {
                    using (TextureBrush brushWindow = new TextureBrush(iImageBackground))
                    {
                        brushWindow.TranslateTransform(iImageLeft.Width, iImageUpperLeft.Height);
                        e.Graphics.FillRectangle(brushWindow, iImageLeft.Width, iImageUpperLeft.Height, width, height);
                    }
                }
            }
        }

        private void SetSizeType(ESizeType aSizeType)
        {
            switch (aSizeType)
            {
                case ESizeType.eLeft:
                case ESizeType.eRight:
                    Cursor.Current = Cursors.SizeWE;
                    break;
                case ESizeType.eTop:
                case ESizeType.eBottom:
                    Cursor.Current = Cursors.SizeNS;
                    break;
                case ESizeType.eTopLeft:
                case ESizeType.eBottomRight:
                    Cursor.Current = Cursors.SizeNWSE;
                    break;
                case ESizeType.eTopRight:
                case ESizeType.eBottomLeft:
                    Cursor.Current = Cursors.SizeNESW;
                    break;
                case ESizeType.eNone:
                    Cursor.Current = Cursors.Arrow;
                    break;
            }

            iSizeType = aSizeType;
        }

        private void SetClientRectangle()
        {
            if (iImageLeft != null && iImageUpperLeft != null && iImageRight != null && iImageUpper != null && iImageLower != null)
            {
                iClientRectangle = new Rectangle(iImageLeft.Width, iImageUpperLeft.Height, base.Size.Width - iImageLeft.Width - iImageRight.Width, base.Size.Height - iImageUpper.Height - iImageLower.Height);
                iClientSize = iClientRectangle.Size;
            }
        }

        private void MoveForm(Point aLocation)
        {
            Point delta = new Point(aLocation.X - iLastMouseLocation.X, aLocation.Y - iLastMouseLocation.Y);
            Point location = new Point(Location.X + delta.X, Location.Y + delta.Y);

            iLastMouseLocation = aLocation;
            Location = location;
        }

        private void ResizeForm(Point aLocation)
        {
            Point delta = new Point(aLocation.X - iLastMouseLocation.X, aLocation.Y - iLastMouseLocation.Y);
            Point location = new Point(Location.X + delta.X, Location.Y + delta.Y);

            switch (iSizeType)
            {
                case ESizeType.eLeft:
                    Location = new Point(location.X, Location.Y);
                    Size = new Size(Size.Width + -delta.X, Size.Height);
                    break;
                case ESizeType.eRight:
                    Size = new Size(Size.Width + delta.X, Size.Height);
                    break;
                case ESizeType.eTop:
                    Location = new Point(Location.X, location.Y);
                    Size = new Size(Size.Width, Size.Height + -delta.Y);
                    break;
                case ESizeType.eBottom:
                    Size = new Size(Size.Width, Size.Height + delta.Y);
                    break;
                case ESizeType.eTopLeft:
                    Location = new Point(location.X, location.Y);
                    Size = new Size(Size.Width + -delta.X, Size.Height + -delta.Y);
                    break;
                case ESizeType.eTopRight:
                    Location = new Point(Location.X, location.Y);
                    Size = new Size(Size.Width + delta.X, Size.Height + -delta.Y);
                    break;
                case ESizeType.eBottomLeft:
                    Location = new Point(location.X, Location.Y);
                    Size = new Size(Size.Width + -delta.X, Size.Height + delta.Y);
                    break;
                case ESizeType.eBottomRight:
                    Location = new Point(Location.X, Location.Y);
                    Size = new Size(Size.Width + delta.X, Size.Height + delta.Y);
                    break;
            }

            iLastMouseLocation = aLocation;
        }

        private void ButtonClose_EventClick(object sender, EventArgs e)
        {
            Close();
        }

        private void ButtonMinimize_EventClick(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void ButtonMaximize_EventClick(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            ButtonMaximize.Visible = false;
            ButtonRestore.Visible = true;
            Refresh();
        }

        private void ButtonRestore_EventClick(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            ButtonMaximize.Visible = true;
            ButtonRestore.Visible = false;
            Refresh();
        }
    }
}
