using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading;
using System.Xml;

using Linn.Doktor;

namespace DoktorKinsky
{
    /// <summary>
    /// Interaction logic for WindowRun.xaml
    /// </summary>

    public partial class WindowRun : Window
    {
        public WindowRun(ITest aTest)
        {
            iTest = aTest;
            iMutex = new Mutex();
            iThread = new Thread(Run);

            InitializeComponent();

            buttonSend.IsEnabled = false;
            buttonCancel.IsEnabled = true;
            buttonOk.IsEnabled = false;

            buttonSend.Click += new RoutedEventHandler(buttonSendClick);
            buttonCancel.Click += new RoutedEventHandler(buttonCancelClick);
            buttonOk.Click += new RoutedEventHandler(buttonOkClick);

            iThread.Start();
        }

        private void Run()
        {
            try
            {
                iReport = new Report();

                iReport.EventChanged += ReportChanged;

                iTest.Execute(iReport);

                Dispatcher.Invoke(new Action(Succeed));
            }
            catch (ThreadAbortException)
            {
            }
            catch(Exception e)
            {
                iException = e;
                Dispatcher.Invoke(new Action(Fail));
            }
        }

        void ReportChanged(object obj, EventArgs e)
        {
            Dispatcher.Invoke(new Action(ReportUpdated));
        }

        private void Complete()
        {
            buttonSend.IsEnabled = true;
            buttonCancel.IsEnabled = false;
            buttonOk.IsEnabled = true;
        }

        private void Succeed()
        {
            Complete();
        }

        private void Fail()
        {
            Complete();

            MessageBox.Show(iException.Message);
        }

        private string Counter(uint aValue)
        {
            if (aValue > 0)
            {
                return (aValue.ToString());
            }

            return ("-");
        }

        private void ReportUpdated()
        {
            uint high;
            uint medium;
            uint low;

            iReport.Counters(out high, out medium, out low);

            textBlockHigh.Text = Counter(high);
            textBlockMedium.Text = Counter(medium);
            textBlockLow.Text = Counter(low);

            IList<Error> list = iReport.Errors;
            listBoxReport.ItemsSource = list;
        }

        void buttonOkClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void buttonCancelClick(object sender, RoutedEventArgs e)
        {
            iThread.Abort();
            iThread.Join();
            Close();
        }

        void buttonSendClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        ITest iTest;
        Mutex iMutex;
        Thread iThread;
        Exception iException;
        Report iReport;
    }
}
