using System;
using System.Drawing;

using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace KinskyTouch
{
    partial class UIViewInfo : UIView
    {
        public UIViewInfo(IntPtr aInstance)
            : base(aInstance)
        {
            iHeader = string.Empty;
            iSubHeader1 = string.Empty;
            iSubHeader2 = string.Empty;
            iSubHeader3 = string.Empty;

            iAlignment = UITextAlignment.Left;
            iTopAlign = true;
        }

        public string Header
        {
            set
            {
                iHeader = value;
            }
        }

        public string SubHeader1
        {
            set
            {
                iSubHeader1 = value;
            }
        }

        public string SubHeader2
        {
            set
            {
                iSubHeader2 = value;
            }
        }

        public string SubHeader3
        {
            set
            {
                iSubHeader3 = value;
            }
        }

        public UITextAlignment Alignment
        {
            set
            {
                iAlignment = value;
            }
        }

        public bool TopAlign
        {
            set
            {
                iTopAlign = value;
            }
        }

        public override void Draw(RectangleF aRect)
        {
            base.Draw(aRect);

            SizeF size = SizeF.Empty;
            float height = 0.0f;
            UIColor.White.SetColor();

            if(!iTopAlign)
            {
                if(!string.IsNullOrEmpty(iHeader))
                {
                    size = StringSize(iHeader, UIFont.BoldSystemFontOfSize(HeaderFontSize), Bounds.Width, UILineBreakMode.TailTruncation);
                    height += size.Height;
                }

                if(!string.IsNullOrEmpty(iSubHeader1))
                {
                    size = StringSize(iSubHeader1, UIFont.SystemFontOfSize(SubHeaderFontSize), Bounds.Width, UILineBreakMode.TailTruncation);
                    height += size.Height;
                }

                if(!string.IsNullOrEmpty(iSubHeader2))
                {
                    size = StringSize(iSubHeader2, UIFont.SystemFontOfSize(SubHeaderFontSize), Bounds.Width, UILineBreakMode.TailTruncation);
                    height += size.Height;
                }

                if(!string.IsNullOrEmpty(iSubHeader3))
                {
                    size = StringSize(iSubHeader3, UIFont.SystemFontOfSize(TechnicalFontSize), Bounds.Width, UILineBreakMode.TailTruncation);
                    height += size.Height;
                }

                height = (Bounds.Height - height) * 0.5f;
            }

            if(!string.IsNullOrEmpty(iHeader))
            {
                size = StringSize(iHeader, UIFont.BoldSystemFontOfSize(HeaderFontSize), Bounds.Width, UILineBreakMode.TailTruncation);
                DrawString(iHeader, new RectangleF(0.0f, height, Bounds.Width, size.Height), UIFont.BoldSystemFontOfSize(HeaderFontSize), UILineBreakMode.TailTruncation, iAlignment);
                height += size.Height;
            }

            if(!string.IsNullOrEmpty(iSubHeader1))
            {
                size = StringSize(iSubHeader1, UIFont.SystemFontOfSize(SubHeaderFontSize), Bounds.Width, UILineBreakMode.TailTruncation);
                DrawString(iSubHeader1, new RectangleF(0.0f, height, Bounds.Width, size.Height), UIFont.SystemFontOfSize(SubHeaderFontSize), UILineBreakMode.TailTruncation, iAlignment);
                height += size.Height;
            }

            if(!string.IsNullOrEmpty(iSubHeader2))
            {
                size = StringSize(iSubHeader2, UIFont.SystemFontOfSize(SubHeaderFontSize), Bounds.Width, UILineBreakMode.TailTruncation);
                DrawString(iSubHeader2, new RectangleF(0.0f, height, Bounds.Width, size.Height), UIFont.SystemFontOfSize(SubHeaderFontSize), UILineBreakMode.TailTruncation, iAlignment);
                height += size.Height;
            }

            if(!string.IsNullOrEmpty(iSubHeader3))
            {
                size = StringSize(iSubHeader3, UIFont.SystemFontOfSize(TechnicalFontSize), Bounds.Width, UILineBreakMode.TailTruncation);
                DrawString(iSubHeader3, new RectangleF(0.0f, height, Bounds.Width, size.Height), UIFont.SystemFontOfSize(TechnicalFontSize), UILineBreakMode.TailTruncation, iAlignment);
            }
        }

        protected virtual float HeaderFontSize
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        protected virtual float SubHeaderFontSize
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        protected virtual float TechnicalFontSize
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        private string iHeader;
        private string iSubHeader1;
        private string iSubHeader2;
        private string iSubHeader3;

        private UITextAlignment iAlignment;
        private bool iTopAlign;
    }

    partial class UIViewInfoIpad : UIViewInfo
    {
        public UIViewInfoIpad(IntPtr aInstance)
            : base(aInstance)
        {
        }

        protected override float HeaderFontSize
        {
            get
            {
                return 18.0f;
            }
        }

        protected override float SubHeaderFontSize
        {
            get
            {
                return 15.0f;
            }
        }

        protected override float TechnicalFontSize
        {
            get
            {
                return 10.0f;
            }
        }
    }

    partial class UIViewInfoIphone : UIViewInfo
    {
        public UIViewInfoIphone(IntPtr aInstance)
            : base(aInstance)
        {
        }

        protected override float HeaderFontSize
        {
            get
            {
                return 13.0f;
            }
        }

        protected override float SubHeaderFontSize
        {
            get
            {
                return 10.0f;
            }
        }

        protected override float TechnicalFontSize
        {
            get
            {
                return 10.0f;
            }
        }
    }
}

