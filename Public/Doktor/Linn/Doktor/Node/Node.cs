using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Linn.Doktor
{
    public interface INode
    {
        string Type { get; }
        IDictionary<string, string> Attributes { get; }
        IList<INode> Children { get; }
    }
    
    public class Node : INode
    {
        public const string kTrue = "true";
        public const string kFalse = "false";
        
        
        public Node(string aType)
        {
            iType = aType;
            iAttributes = new Dictionary<string,string>();
            iChildren = new List<INode>();
        }
        
        public void Add(string aKey, string aValue)
        {
            iAttributes.Add(aKey, aValue);
        }

        public void Add(INode aChild)
        {
            iChildren.Add(aChild);
        }
        
        public string Type
        {
            get
            {
                return (iType);
            }
        }
        
        public IDictionary<string, string> Attributes
        {
            get
            {
                return (iAttributes);
            }
        }
        
        public IList<INode> Children
        {
            get
            {
                return (iChildren);
            }
        }
        
        string iType;
        Dictionary<string, string> iAttributes;
        List<INode> iChildren;
    }
}
