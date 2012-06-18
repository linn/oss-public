using Linn.Gui.Resources;
using System.Xml;
using System.ComponentModel;
using System;

namespace Linn.Gui
{
    public class SingletonAlreadyExists : Exception { }
    public class SingletonDoesntExist : SystemException { }


    public interface IUpdateObject
    {
        void Tick(float aDeltaTime);
    }

    public interface ISerialiseObject
    {
        void Load(XmlNode aXmlNode);
        void Link();
        void Save(XmlTextWriter aWriter);
    }

    public class ReferenceObject<T>
    {
        public ReferenceObject()
        {
        }

        public ReferenceObject(string aName, T aObject)
        {
            iName = aName;
            iObject = aObject;
        }

        public string Name
        {
            get
            {
                return iName;
            }
            set
            {
                iName = value;
                Link();
            }
        }

        public T Object
        {
            get
            {
                return iObject;
            }
            /*set {
                iObject = value;
            }*/
        }

        public virtual void Link()
        {
        }

        protected string iName = "";
        protected T iObject;
    }
} // Linn.Gui
