using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.Net;
using System.IO;
using System.Globalization;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SneakyRadio
{
    public class StationBbcRadio3 : StationBbc, IStation
    {
        public StationBbcRadio3()
            : base("radio3")
        {
            iLogo = new BitmapImage(new Uri("pack://application:,,,/BbcRadio3/BbcRadio3.png"));
        }

        public ImageSource Logo()
        {
            return (iLogo);
        }

        public Color Fill()
        {
            return (kFill);
        }

        public Color Back()
        {
            return (kBack);
        }

        public Color Stroke()
        {
            return (kStroke);
        }

        private static readonly Color kFill = (Color)ColorConverter.ConvertFromString("#d3021d");
        private static readonly Color kBack = Colors.White;
        private static readonly Color kStroke = Colors.Black;

        private ImageSource iLogo;
    }
}
