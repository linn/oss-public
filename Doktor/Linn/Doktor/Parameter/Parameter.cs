using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

using Linn;

namespace Linn.Doktor
{
    public interface IParameter
    {
        string Name { get; }
        string Type { get; }
        string Description { get; }
        string Kind { get; }
        IList<string> AllowedValues { get; }
        IDictionary<string, string> Attributes { get; }
        bool Valid { get; }
        string Value { get; set; }
        void Init(IList<INode> aNodes);
        INode Node { get; }
    }
    
    public abstract class Parameter : IParameter
    {
        public const string kKindNormal = "Normal";
        public const string kKindEnumerated = "Enumerated";
        public const string kKindNodal = "Nodal";

        public const string kAttributeDefault = "Default";
        
        protected Parameter(string aName, string aType, string aDescription, string aKind)
        {
            iName = aName;
            iType = aType;
            iDescription = aDescription;
            iKind = aKind;
            iValid = false;
            iAttributes = new Dictionary<string, string>();
        }

        protected void Add(string aAttribute, string aValue)
        {
            iAttributes.Add(aAttribute, aValue);
        }

        public void Init(IList<INode> aNodes)
        {
            iValue = Initialise(aNodes);
            iValid = (iValue != null);
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

        public string Kind
        {
            get
            {
                return (iKind);
            }
        }

        public string Value
        {
            get
            {
                return (iValue);
            }
            set
            {
                iValue = value;
                iValid = ValueChanged(iValue);
            }
        }

        public bool Valid
        {
            get
            {
                return (iValid);
            }
        }

        public IDictionary<string, string> Attributes
        {
            get
            {
                return (iAttributes);
            }
        }

        public virtual IList<string> AllowedValues
        {
            get
            {
                return (null);
            }
        }

        public virtual INode Node
        {
            get
            {
                return (null);
            }
        }

        protected abstract string Initialise(IList<INode> aNodes);
        protected abstract bool ValueChanged(string aValue);

        private string iName;
        private string iType;
        private string iDescription;
        private string iKind;
        private bool iValid;
        private string iValue;
        private Dictionary<string, string> iAttributes;
    }

    public class ParameterNormal : Parameter
    {
        protected ParameterNormal(string aName, string aType, string aDescription, string aDefault)
            : base(aName, aType, aDescription, kKindNormal)
        {
            iDefault = aDefault;
            Add(kAttributeDefault, iDefault);
        }

        protected ParameterNormal(string aName, string aType, string aDescription)
            : base(aName, aType, aDescription, kKindNormal)
        {
        }

        protected override string Initialise(IList<INode> aNodes)
        {
            return (iDefault);
        }

        protected override bool ValueChanged(string aValue)
        {
            if (aValue != null)
            {
                if (aValue.Length > 0)
                {
                    return (true);
                }
            }

            return (false);
        }

        string iDefault;
    }

    public class ParameterEnumerated : Parameter
    {
        protected ParameterEnumerated(string aName, string aType, string aDescription)
            : base(aName, aType, aDescription, kKindEnumerated)
        {
            iAllowedValues = new List<string>();
            iDefaultIndex = 0;
        }

        public void Add(string aAllowedValue)
        {
            iAllowedValues.Add(aAllowedValue);
        }

        public void AddDefault(string aAllowedValue)
        {
            iAllowedValues.Add(aAllowedValue);
            iDefaultIndex = iAllowedValues.Count - 1;
        }

        public override IList<string> AllowedValues
        {
            get
            {
                return (iAllowedValues);
            }
        }

        protected override string Initialise(IList<INode> aNodes)
        {
            if (iAllowedValues.Count > iDefaultIndex)
            {
                return (iAllowedValues[iDefaultIndex]);
            }

            return (null);
        }

        protected override bool ValueChanged(string aValue)
        {
            return (iAllowedValues.Contains(aValue));
        }

        private IList<string> iAllowedValues;
        private int iDefaultIndex;
    }

    public abstract class ParameterNodal : Parameter
    {
        protected ParameterNodal(string aName, string aType, string aDescription)
            : base(aName, aType, aDescription, kKindNodal)
        {
        }

        public override IList<string> AllowedValues
        {
            get
            {
                return (iAllowedValues);
            }
        }

        protected override string Initialise(IList<INode> aNodes)
        {
            iNodes = new List<INode>();
            iAllowedValues = new List<string>();

            foreach (INode node in aNodes)
            {
                IList<string> names = Recognise(node);

                if (names != null)
                {
                    iNodes.Add(node);
                    iAllowedValues.Add(names[0]);
                }
            }

            if (iAllowedValues.Count > 0)
            {
                iNode = iNodes[0];
                Recognise(iNode);
                return (iAllowedValues[0]);
            }

            iNode = null;
            return (null);
        }

        public IList<IList<string>> Names(IList<INode> aNodes)
        {
            IList<IList<string>> list = new List<IList<string>>();

            foreach (INode node in aNodes)
            {
                IList<string> names = Recognise(node);

                if (names != null)
                {
                    list.Add(names);
                }
            }

            return (list);
        }

        protected override bool ValueChanged(string aValue)
        {
            foreach (INode node in iNodes)
            {
                IList<string> names = Recognise(node);

                foreach (string name in names)
                {
                    if (name == aValue)
                    {
                        iNode = node;
                        Recognise(iNode);
                        return (true);
                    }
                }
            }

            iNode = null;
            return (false);
        }

        public override INode Node
        {
            get
            {
                return (iNode);
            }
        }

        protected abstract IList<string> Recognise(INode aNode);

        private INode iNode;
        private List<INode> iNodes;
        private List<string> iAllowedValues;
    }
}
