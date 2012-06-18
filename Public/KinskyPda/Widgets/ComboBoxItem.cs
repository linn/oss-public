using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace KinskyPda.Widgets
{
    public class ComboBoxItem
    {
        public ComboBoxItem(Image aImage, string aName, object aTag)
        {
            Image = aImage;
            Name = aName;
            Tag = aTag;
        }

        public override string ToString()
        {
            return Name;
        }

        public Image Image;
        public string Name;
        public object Tag;
    }
}
