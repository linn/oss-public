using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Xml;
using System.Xml.XPath;
using System.Net;
using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

using SneakyRadio.AVTransport;
using SneakyRadio.RenderingControl;

namespace SneakyRadio
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            LastAngle = 0;
            LastPause = false;

            iBindingSet = new SetAVTransportURIBinding();
            iBindingPlay = new PlayBinding();
            iBindingPause = new PauseBinding();
            iBindingStop = new StopBinding();
            iBindingVolume = new SetVolumeBinding();
            iBindingMute = new SetMuteBinding();

            iStations = new List<IStation>();
            iStations.Add(new StationWfmu());
            iStations.Add(new StationBbcRadio4());
            iStations.Add(new StationBbcRadio3());
            //iStations.Add(new StationResonanceFM());

            iShows = new BindingList<Show>();

            DataContext = iShows;

            StationSelect(0);

            InitializeComponent();

            UiKnobRotate.Angle = 0;
            UiList.SelectedIndex = 0;

            iLoadedShows = false;
            iLoadedSemaphore = new Semaphore(0, 1);

            iLoadThread = new Thread(new ThreadStart(LoadShows));
            iLoadThread.Start();
        }

        private void LoadShows()
        {
            // ensure shows on all stations have been collected

            foreach (IStation station in iStations)
            {
                List<IShow> shows = station.Shows();
            }

            iLoadedShows = true;
            iLoadedSemaphore.Release();
        }

        private void StationSelect(int aIndex)
        {
            iStation = aIndex;

            IStation station = iStations[iStation];

            // Set dynamic logo resource
            Resources["Logo"] = station.Logo();

            // Set dynamic colour resources
            Resources["BrushFill"] = new SolidColorBrush(station.Fill());
            Resources["BrushBack"] = new SolidColorBrush(station.Back());
            Resources["BrushStroke"] = new SolidColorBrush(station.Stroke());

            // Fix colours of the listbox highlight and highlight text

            Resources[SystemColors.HighlightBrushKey] = new SolidColorBrush(station.Fill());
            Resources[SystemColors.ControlBrushKey] = new SolidColorBrush(station.Fill());
            Resources[SystemColors.HighlightTextBrushKey] = new SolidColorBrush(station.Back());
            Resources[SystemColors.ControlTextBrushKey] = new SolidColorBrush(station.Back());

            iShows.Clear();

            foreach (IShow show in station.Shows())
            {
                iShows.Add(new Show(show));
            }
        }

        private void StationNext()
        {
            if (!iLoadedShows)
            {
                iLoadedSemaphore.WaitOne();
            }

            if (++iStation < iStations.Count)
            {
                StationSelect(iStation);
            }
            else
            {
                StationSelect(0);
            }

            UiList.SelectedIndex = 0;
            UiList.ScrollIntoView(UiList.SelectedItem);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private SetAVTransportURIBinding iBindingSet;
        private PlayBinding iBindingPlay;
        private PauseBinding iBindingPause;
        private StopBinding iBindingStop;
        private SetVolumeBinding iBindingVolume;
        private SetMuteBinding iBindingMute;

        private int iStation;

        private List<IStation> iStations;

        private BindingList<Show> iShows;

        private Point LastMousePos;
        private double LastAngle;
        private bool LastPause;

        private uint CurrentVolume;

        private Thread iLoadThread;
        private bool iLoadedShows;
        private Semaphore iLoadedSemaphore; 


        private void OnAddressTextChanged(object sender, TextChangedEventArgs e)
        {
            iBindingSet.Url = UiAddress.Text + "/AVTransport/control";
            iBindingPlay.Url = UiAddress.Text + "/AVTransport/control";
            iBindingPause.Url = UiAddress.Text + "/AVTransport/control";
            iBindingStop.Url = UiAddress.Text + "/AVTransport/control";
            iBindingVolume.Url = UiAddress.Text + "/RenderingControl/control";
            iBindingMute.Url = UiAddress.Text + "/RenderingControl/control";
        }

        private void IncrementVolume()
        {
            if (CurrentVolume < 100)
            {
                CurrentVolume++;
                UiKnobVolume.Text = CurrentVolume.ToString();
                iBindingVolume.SetVolumeAsync(0, "Master", Convert.ToUInt16(CurrentVolume));
            }

            if (LastPause)
            {
                Unpause();
            }
        }

        private void DecrementVolume()
        {
            if (CurrentVolume > 0)
            {
                CurrentVolume--;
                UiKnobVolume.Text = CurrentVolume.ToString();
                iBindingVolume.SetVolumeAsync(0, "Master", Convert.ToUInt16(CurrentVolume));
            }

            if (LastPause)
            {
                Unpause();
            }
        }

        private void Unpause()
        {
            LastPause = false;
            iBindingPlay.PlayAsync(0, "1");
            UiKnobStop.Opacity = 0;
            UiKnobPause.Opacity = 0;
            UiKnobVolume.Opacity = 1;
        }

        private void Pause()
        {
            if (LastPause)
            {
                Unpause();
            }
            else
            {
                LastPause = true;
                iBindingPause.PauseAsync(0, "1");
                UiKnobStop.Opacity = 0;
                UiKnobVolume.Opacity = 0;
                UiKnobPause.Opacity = 1;
            }
        }

        private void Play()
        {
            if (UiList.SelectedItems.Count > 0)
            {
                Uri uri = iShows[UiList.SelectedIndex].Uri;
                iBindingSet.SetAVTransportURI(0, uri.ToString(), "");
                LastPause = false;
                iBindingPlay.PlayAsync(0, "1");
                UiKnobStop.Opacity = 0;
                UiKnobPause.Opacity = 0;
                UiKnobVolume.Opacity = 1;
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                if (UiList.IsMouseOver)
                {
                    Play();
                    return;
                }
            }
            if (UiLogo.IsMouseOver)
            {
                StationNext();
            }
            else if (UiKnobCentre.IsMouseOver || UiKnobContent.IsMouseOver)
            {
                Pause();
            }
            else if (UiKnobRing.IsMouseOver || UiKnobNodule.IsMouseOver)
            {
                CaptureMouse();
                PreviewMouseMove += Knob_PreviewMouseMove;
                UpdateAngle();
            }
            else if (UiMinimise.IsMouseOver)
            {
                WindowState = WindowState.Minimized;
            }
            else if (UiExit.IsMouseOver)
            {
                Close();
            }
            else if (!UiList.IsMouseOver && !UiAddress.IsMouseOver)
            {
                CaptureMouse();
                LastMousePos = PointToScreen(Mouse.GetPosition(this));
                PreviewMouseMove += Window_PreviewMouseMove;
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            PreviewMouseMove -= Window_PreviewMouseMove;
            PreviewMouseMove -= Knob_PreviewMouseMove;
        }

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point mouse = PointToScreen(Mouse.GetPosition(this));
            Vector delta = mouse - LastMousePos;
            LastMousePos = mouse;

            Point pos = new Point(Left + delta.X, Top + delta.Y);

            // limit position

            Rect work = System.Windows.SystemParameters.WorkArea;

            if (pos.X < 0)
            {
                pos.X = 0;
            }

            double maxX = work.Width - Width;

            if (pos.X > maxX)
            {
                pos.X = maxX;
            }

            if (pos.Y < 0)
            {
                pos.Y = 0;
            }

            double maxY = work.Height - Height;

            if (pos.Y > maxY)
            {
                pos.Y = maxY;
            }

            Left = pos.X;
            Top = pos.Y;
        }

        #region Mouse Events

        private void Knob_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            UpdateAngle();
        }

        private void UpdateAngle()
        {
            Point mouse = Mouse.GetPosition(UiKnobFixed);

            double angle = ((Math.Atan2(mouse.Y - 50, mouse.X - 50) * 180) / Math.PI) + 270;

            if (angle > 360)
            {
                angle -= 360;
            }

            UiKnobRotate.Angle = angle;

            double delta = angle - LastAngle;

            // normalise angle delta

            if (delta > 180)
            {
                delta -= 360;
            }

            if (delta < -180)
            {
                delta += 360;
            }

            // update volume according to angle delta

            if (delta > 30)
            {
                IncrementVolume();
                LastAngle = angle;
            }

            if (delta < -30)
            {
                DecrementVolume();
                LastAngle = angle;
            }
        }

        #endregion

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Play();
            }
            else if (e.Key == Key.Space || e.Key == Key.Pause)
            {
                Pause();
            }
            else if (e.Key == Key.Add || e.Key == Key.VolumeUp)
            {
                IncrementVolume();
            }
            else if (e.Key == Key.Subtract || e.Key == Key.VolumeDown)
            {
                DecrementVolume();
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (UiKnobCentre.IsMouseOver
                || UiKnobContent.IsMouseOver
                || UiKnobRing.IsMouseOver
                || UiKnobNodule.IsMouseOver)
            {
                if (e.Delta > 0)
                {
                    IncrementVolume();
                }
                else if (e.Delta < 0)
                {
                    DecrementVolume();
                }
            }
        }
    }

    public class Show
    {
        public Show(IShow aShow)
        {
            iShow = aShow;
        }

        public string Name
        {
            get
            {
                return (iShow.Name());
            }
        }

        public string Details
        {
            get
            {
                return (iShow.Details());
            }
        }

        public Uri Uri
        {
            get
            {
                return (iShow.Uri());
            }
        }

        private IShow iShow;
    }
}
