using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SneakyRadio
{
    public interface IShow
    {
        string Name();
        string Details();
        Uri Uri();
    }

    public interface IStation
    {
        ImageSource Logo();
        Color Fill();
        Color Back();
        Color Stroke();
        List<IShow> Shows();
    }
}
