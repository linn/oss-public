using Linn;
using Linn.Gui;
using System.Drawing;
using System.IO;
using System.Net;
using System.Drawing.Imaging;
using System;

namespace Linn.Gui.Resources
{

    public class TextureGdiReflection : TextureGdi
    {
        public TextureGdiReflection(string aName, TextureGdi aTexture)
            : base(aName)
        {
            iSourceTexture = aTexture;
            //Refresh();
        }

        public override void Refresh()
        {
            /*Bitmap source = new Bitmap(iSourceTexture.Surface);
            source.RotateFlip(RotateFlipType.RotateNoneFlipY);
            Bitmap dest = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);*/
            Bitmap dest = new Bitmap(0, 0);
            /*for (int y = 0; y < source.Height; ++y)
            {
                for (int x = 0; x < source.Width; ++x)
                {
                    int alpha = (int)Math.Round(((double)255 / source.Height) * (source.Height - y)) - 150;

                    if (alpha < 0) alpha = 0;
                    if (alpha > 255) alpha = 255;

                    Color color = source.GetPixel(x, y);
                    if (color.A == 0)
                        continue;

                    int pixelAlpha = (int)Math.Round((alpha / (double)255) * color.A);
                    dest.SetPixel(x, y, Color.FromArgb(pixelAlpha, color.R, color.G, color.B));
                }
            }*/
            iImage = dest;
        }

        private TextureGdi iSourceTexture;
    }

} // Linn.Gui.Resources

