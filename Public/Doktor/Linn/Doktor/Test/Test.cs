using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Linn.Doktor
{
    public interface ITest
    {
        string Name { get; }
        string Type { get; }
        string Description { get; }
        IList<IParameter> Parameters { get; }
        void Execute(IReport aReport);
    }

    public abstract class TestBase : ITest
    {
        protected TestBase(string aName, string aType, string aDescription)
        {
            iName = aName;
            iType = aType;
            iDescription = aDescription;
            iParameters = new List<IParameter>();
        }
        
        public string Name
        {
            get
            {
                return (iName);
            }
        }

        public string Type
        {
            get
            {
                return (iType);
            }
        }

        public string Description
        {
            get
            {
                return (iDescription);
            }
        }

        public IList<IParameter> Parameters
        {
            get
            {
                return (iParameters);
            }
        }

        public void Add(IParameter aParameter)
        {
            // check unique name

            foreach (IParameter parameter in iParameters)
            {
                Assert.Check(parameter.Name != aParameter.Name);
            }

            iParameters.Add(aParameter);
        }

        public abstract void Execute(IReport aReport);

        private string iName;
        private string iType;
        private string iDescription;
        private List<IParameter> iParameters;
    }

    public abstract class Test : TestBase
    {
        public static IList<ITest> CreateTests(string aId)
        {
            List<ITest> list = new List<ITest>();

            list.Add(new TestUpnp());
            
            if (aId == "DoktorTest" || aId == "DoktorKinsky")
            {
                list.Add(new TestTestParameter());
                list.Add(new TestTestReport());
            }
            
            return (list);
        }
        
        protected Test(string aName, string aType, string aDescription)
            : base(aName, aType, aDescription)
        {
        }

        private void ReportContext()
        {
            Context("Test", Name);

            foreach (IParameter parameter in Parameters)
            {
                Context(parameter.Name, parameter.Value);
            }

        }

        public override void Execute(IReport aReport)
        {
            iReport = aReport;
            ReportContext();
            Execute();
        }

        protected void Reference(string aReference)
        {
            iReport.Reference(aReference);
        }

        protected void Context<T>(string aKey, T aValue)
        {
            iReport.Context(aKey, aValue.ToString());
        }

        protected void High(string aDescription)
        {
            iReport.High(aDescription);
            ReportContext();
        }

        protected void Medium(string aDescription)
        {
            iReport.Medium(aDescription);
            ReportContext();
        }

        protected void Low(string aDescription)
        {
            iReport.Low(aDescription);
            ReportContext();
        }

        protected abstract void Execute();

        private IReport iReport;
    }

    public class TestSuite : TestBase
    {
        class SubTest
        {
            public SubTest(ITest aTest, Dictionary<string, IParameter> aAssociations)
            {
                iTest = aTest;
                iAssociations = aAssociations;
            }

            public ITest Test
            {
                get
                {
                    return (iTest);
                }
            }

            public Dictionary<string, IParameter> Associations
            {
                get
                {
                    return (iAssociations);
                }
            }

            ITest iTest;

            Dictionary<string, IParameter> iAssociations;
        }

        protected TestSuite(string aName, string aType, string aDescription)
            : base(aName, aType, aDescription)
        {
            iAssociations = new Dictionary<string, IParameter>();
            iSubTests = new List<SubTest>();
        }

        private bool Match(IParameter aParameter)
        {
            if (iAssociations.ContainsKey(aParameter.Name))
            {
                return (true);
            }

            foreach (IParameter parameter in Parameters)
            {
                if (parameter.Name == aParameter.Name)
                {
                    Associate(parameter.Name, parameter);
                    return (true);
                }
            }

            return (false);
        }

        protected void Associate(string aTargetParameter, IParameter aSourceParameter)
        {
            Assert.Check(!iAssociations.ContainsKey(aTargetParameter));
            Assert.Check(!iAssociations.ContainsValue(aSourceParameter));
            Assert.Check(Parameters.Contains(aSourceParameter));
            iAssociations.Add(aTargetParameter, aSourceParameter);
        }

        protected void Add(ITest aTest)
        {
            foreach (IParameter parameter in aTest.Parameters)
            {
                Assert.Check(Match(parameter));
            }

            iSubTests.Add(new SubTest(aTest, iAssociations));

            iAssociations = new Dictionary<string, IParameter>();
        }

        public override void Execute(IReport aReport)
        {
            iReport = aReport;

            foreach (SubTest sub in iSubTests)
            {
                ITest test = sub.Test;

                foreach (IParameter parameter in test.Parameters)
                {
                    IParameter source = sub.Associations[parameter.Name];

                    List<INode> nodes = new List<INode>();

                    INode node = source.Node;

                    if (node != null)
                    {
                        nodes.Add(node);
                    }

                    parameter.Init(nodes);

                    parameter.Value = source.Value;

                    Assert.Check(parameter.Valid);
                }

                test.Execute(iReport);
            }
        }

        private Dictionary<string, IParameter> iAssociations;
        private List<SubTest> iSubTests;

        private IReport iReport;
    }
}
