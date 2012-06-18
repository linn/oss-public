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

using Linn.Doktor;

namespace DoktorKinsky
{
    /// <summary>
    /// Interaction logic for WindowExpert.xaml
    /// </summary>

    public partial class WindowExpert : Window, ISupplyHandler
    {
        public WindowExpert()
        {
            InitializeComponent();

            string id = "DoktorKinsky";

            iMutex = new Mutex();
            iTests = Test.CreateTests(id);
            iSupplies = Supply.CreateSupplies(id);

            iNodes = new List<INode>();

            listBoxTests.ItemsSource = iTests;
            listBoxTests.SelectionChanged += new SelectionChangedEventHandler(listBoxTestsSelectionChanged);

            buttonReset.IsEnabled = false;
            buttonRun.IsEnabled = false;

            buttonReset.Click += new RoutedEventHandler(buttonResetClick);
            buttonRun.Click += new RoutedEventHandler(buttonRunClick);

            foreach (ISupply supply in iSupplies)
            {
                supply.Open();
                supply.Subscribe(this);
            }
        }

        void buttonRunClick(object sender, RoutedEventArgs e)
        {
            WindowRun win = new WindowRun(iTest);
            win.Owner = this;
            win.ShowDialog();
        }

        private void buttonResetClick(object sender, RoutedEventArgs e)
        {
            ResetParameters();
        }

        private void listBoxTestsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            iTest = listBoxTests.SelectedItem as ITest;
            ResetParameters();
        }

        private void ResetParameters()
        {
            if (iTest != null)
            {
                PopulateParameters(iTest);
                buttonReset.IsEnabled = true;
            }
            else
            {
                listBoxParameters.ItemsSource = null;
                buttonReset.IsEnabled = false;
            }
        }

        private void PopulateParameters(ITest aTest)
        {
            Lock();

            List<INode> nodes = new List<INode>(iNodes);

            Unlock();

            ParameterList list = new ParameterList(buttonRun, aTest.Parameters, nodes);

            listBoxParameters.ItemsSource = list.Parameters;
        }

        public void Add(INode aNode)
        {
            Lock();

            iNodes.Add(aNode);

            Unlock();
        }

        public void Remove(INode aNode)
        {
            Lock();

            iNodes.Remove(aNode);

            Unlock();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            foreach (ISupply supply in iSupplies)
            {
                supply.Unsubscribe(this);
                supply.Close();
            }
        }

        private void Lock()
        {
            iMutex.WaitOne();
        }

        private void Unlock()
        {
            iMutex.ReleaseMutex();
        }


        private Mutex iMutex;
        private ITest iTest;
        private IList<ITest> iTests;
        private IList<ISupply> iSupplies;

        private List<INode> iNodes;
    }

    public class ParameterList
    {
        public ParameterList(Control aControl, IList<IParameter> aParameters, IList<INode> aNodes)
        {
            iControl = aControl;
            iValidCount = 0;
            iParameters = new List<ParameterItem>();

            foreach (IParameter parameter in aParameters)
            {
                parameter.Init(aNodes);

                if (parameter.Valid)
                {
                    iValidCount++;
                }

                ParameterItem p = new ParameterItem(parameter);

                p.PropertyChanged += new PropertyChangedEventHandler(ParameterPropertyChanged);

                iParameters.Add(p);
            }

            iControl.IsEnabled = (iValidCount == iParameters.Count);
        }

        void ParameterPropertyChanged(object obj, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Valid")
            {
                ParameterItem item = obj as ParameterItem;

                if (item.Valid)
                {
                    if (++iValidCount == iParameters.Count)
                    {
                        iControl.IsEnabled = true;
                    }
                }
                else
                {
                    if (iValidCount-- == iParameters.Count)
                    {
                        iControl.IsEnabled  = false;
                    }
                }
            }
        }

        public IList<ParameterItem> Parameters
        {
            get
            {
                return (iParameters);
            }
        }

        private Control iControl;
        private int iValidCount;
        private List<ParameterItem> iParameters;
    }

    public class ParameterItem : INotifyPropertyChanged
    {
        public ParameterItem(IParameter aParameter)
        {
            iParameter = aParameter;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String aProperty)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(aProperty));
            }
        }


        public string Type
        {
            get
            {
                return (iParameter.Type);
            }
        }

        public string Name
        {
            get
            {
                return (iParameter.Name);
            }
        }

        public string Description
        {
            get
            {
                return (iParameter.Description);
            }
        }

        public string Kind
        {
            get
            {
                return (iParameter.Kind);
            }
        }

        public string Value
        {
            get
            {
                return (iParameter.Value);
            }

            set
            {
                bool valid = iParameter.Valid;

                iParameter.Value = value;

                if (valid != iParameter.Valid)
                {
                    NotifyPropertyChanged("Valid");
                }
            }
        }

        public bool Valid
        {
            get
            {
                return (iParameter.Valid);
            }
        }

        public IList<string> AllowedValues
        {
            get
            {
                return (iParameter.AllowedValues);
            }
        }

        IParameter iParameter;
    }
}
