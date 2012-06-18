
using System;

using Linn;


namespace KinskyDesktop
{
    public class OptionPageFonts : OptionPage
    {
        public OptionPageFonts()
            : base("Fonts")
        {
            iSize = new OptionEnum("fontsize", "Font size", "The size of fonts to use in the application");
            iSize.AddDefault("small");
            iSize.Add("medium");
            iSize.Add("large");
            Add(iSize);

            iSmallSizes = new float[3];
            iMediumSizes = new float[3];
            iLargeSizes = new float[3];

            iSmallSizes[0] = 9.0f;
            iMediumSizes[0] = 11.0f;
            iLargeSizes[0] = 18.0f;

            for (int i = 1; i < 3; i++)
            {
                iSmallSizes[i] = iSmallSizes[i - 1] * 1.1f;
                iMediumSizes[i] = iMediumSizes[i - 1] * 1.1f;
                iLargeSizes[i] = iLargeSizes[i - 1] * 1.1f;
            }
        }

        public Single SmallSize
        {
            get { return iSmallSizes[GetIndex()]; }
        }

        public Single MediumSize
        {
            get { return iMediumSizes[GetIndex()]; }
        }

        public Single LargeSize
        {
            get { return iLargeSizes[GetIndex()]; }
        }

        private int GetIndex()
        {
            switch (iSize.Value)
            {
                case "small":
                    return 0;
                case "medium":
                    return 1;
                case "large":
                    return 2;
                default:
                    return -1;
            }
        }

        private OptionEnum iSize;
        private Single[] iSmallSizes;
        private Single[] iMediumSizes;
        private Single[] iLargeSizes;
    }
}


