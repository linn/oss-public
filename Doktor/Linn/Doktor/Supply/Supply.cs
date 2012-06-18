using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Linn.Doktor
{
    public interface ISupplyHandler
    {
        void Add(INode aNode);
        void Remove(INode aNode);
    }
    
    public interface ISupply
    {
        void Open();
        void Close();
        void Subscribe(ISupplyHandler aHandler);
        void Unsubscribe(ISupplyHandler aHandler);
    }
    
    public abstract class Supply : ISupply
    {
        public static IList<ISupply> CreateSupplies(string aId)
        {
            List<ISupply> list = new List<ISupply>();

            list.Add(new SupplyUpnp());
            
            return (list);
        }

        protected Supply()
        {
            iMutex = new Mutex();
            iNodes = new List<INode>();
            iHandlers = new List<ISupplyHandler>();
        }
        
        public abstract void Open();
        public abstract void Close();
        
        public void Subscribe(ISupplyHandler aHandler)
        {
            Lock();
            
            foreach (INode node in iNodes)
            {
                aHandler.Add(node);
            }
            
            iHandlers.Add(aHandler);
            
            Unlock();
        }
        
        public void Unsubscribe(ISupplyHandler aHandler)
        {
            Lock();
            
            foreach (INode node in iNodes)
            {
                aHandler.Remove(node);
            }
            
            iHandlers.Add(aHandler);
            
            Unlock();
        }
        
        protected void Add(INode aNode)
        {
            Lock();
            
            foreach (ISupplyHandler handler in iHandlers)
            {
                handler.Add(aNode);
            }
            
            iNodes.Add(aNode);
            
            Unlock();
        }
        
        protected void Remove(INode aNode)
        {
            Lock();
            
            foreach (ISupplyHandler handler in iHandlers)
            {
                handler.Remove(aNode);
            }
            
            iNodes.Remove(aNode);
            
            Unlock();
        }
        
        protected IList<INode> CreateSnapshot()
        {
            Lock();
            
            List<INode> snapshot = new List<INode>(iNodes);
            
            Unlock();
            
            return (snapshot);
        }
        
        private void Lock()
        {
            iMutex.WaitOne();
        }
        
        private void Unlock()
        {
            iMutex.ReleaseMutex();
        } 
        
        Mutex iMutex;
        List<INode> iNodes;
        List<ISupplyHandler> iHandlers;
    }
}
