using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
namespace KinskyDesktopWpf
{
    public class BreadcrumbPanel : Panel
    {
        protected override Size MeasureOverride(Size aAvailableSize)
        {
            Size resultSize = new Size(0, 0);

            foreach (UIElement child in Children)
            {
                try { child.Measure(aAvailableSize); }
                catch { }
                resultSize.Width += child.DesiredSize.Width;
                resultSize.Height = Math.Max(resultSize.Height, child.DesiredSize.Height);
            }

            resultSize.Width = double.IsPositiveInfinity(aAvailableSize.Width) ? resultSize.Width : aAvailableSize.Width;
            resultSize.Width = Math.Min(resultSize.Width, aAvailableSize.Width);

            return resultSize;
        }

        protected override Size ArrangeOverride(Size aFinalSize)
        {
            if (this.Children == null || this.Children.Count == 0)
            {
                return aFinalSize;
            }

            double finalWidth = aFinalSize.Width;
            double totalWidth = 0;

            for (int i = Children.Count - 1; i >= 0; i--)
            {
                Children[i].Measure(aFinalSize);
            }

            for (int i = Children.Count - 1; i >= 0; i--)
            {
                if (totalWidth + Children[i].DesiredSize.Width <= finalWidth || i == Children.Count - 1)
                {
                    totalWidth += Children[i].DesiredSize.Width;
                }
                else
                {
                    break;
                }
            }
            double currentWidth = Math.Min(totalWidth, finalWidth) + 1;

            for (int i = Children.Count - 1; i >= 0; i--)
            {
                UIElement child = Children[i];
                Rect arrangeRect;
                if (currentWidth >= 0 || i == Children.Count - 1)
                {
                    if (currentWidth < child.DesiredSize.Width)
                    {
                        arrangeRect = new Rect(0, 0, currentWidth, child.DesiredSize.Height);
                    }
                    else
                    {
                        arrangeRect = new Rect(currentWidth - child.DesiredSize.Width, 0, child.DesiredSize.Width, aFinalSize.Height);
                    }
                    child.Arrange(arrangeRect);
                }
                else
                {
                    child.Arrange(new Rect(0, 0, 0, 0));
                }
                currentWidth -= child.DesiredSize.Width;
            }

            return aFinalSize;
        }
    }
}