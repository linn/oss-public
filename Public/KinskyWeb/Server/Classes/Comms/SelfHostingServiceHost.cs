using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;

namespace KinskyWeb.Comms
{

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SelfHostingServiceAttribute : Attribute
    {
        public Type Interface { get; set; }
        public Type Implementation { get; set; }
        public String Endpoint { get; set; }
    }


    public class SelfHostingServiceHost : ServiceHost
    {

        public static SelfHostingServiceHost[] GetSelfHostingServices(string aBaseAddress)
        {
            List<SelfHostingServiceHost> hosts = new List<SelfHostingServiceHost>();
            Assembly assm = Assembly.GetExecutingAssembly();
            foreach (Type t in assm.GetTypes())
            {

                SelfHostingServiceAttribute attr = (SelfHostingServiceAttribute)Attribute.GetCustomAttribute(t, typeof(SelfHostingServiceAttribute));
                if (attr != null)
                {

                    Type interfaceType = attr.Interface;
                    Type implementationType = attr.Implementation;
                    String endpoint = attr.Endpoint;

                    if (interfaceType == null)
                    {
                        // search for first interface that is decorated with ServiceContractAttribute
                        Type[] interfaces = t.GetInterfaces();
                        foreach (Type i in interfaces)
                        {
                            ServiceContractAttribute sca = (ServiceContractAttribute)Attribute.GetCustomAttribute(i, typeof(ServiceContractAttribute));
                            if (sca != null)
                            {
                                interfaceType = i;
                                break;
                            }
                        }
                        if (interfaceType == null)
                        {
                            throw new ApplicationException("Could not autodiscover ServiceContractAttribute for self hosted service");
                        }
                    }
                    if (implementationType == null)
                    {
                        implementationType = t;
                    }
                    if (endpoint == null)
                    {
                        endpoint = t.Name;
                    }

                    SelfHostingServiceHost host = new SelfHostingServiceHost(
                                    implementationType,
                                    interfaceType,
                                    endpoint,
                                    new Uri[] { new Uri(Path.Combine(aBaseAddress, endpoint)) });
                    hosts.Add(host);
                }
            }
            return hosts.ToArray();
        }

        public SelfHostingServiceHost(Type aServiceImplementationType, Type aServiceInterfaceType, string aEndpointName, params Uri[] aBaseAddress)
        {
            base.InitializeDescription(aServiceImplementationType, new UriSchemeKeyedCollection(aBaseAddress));
            this.ServiceInterfaceType = aServiceInterfaceType;
            this.ServiceImplementationType = aServiceImplementationType;
            this.EndpointName = aEndpointName;
        }

        public Type ServiceImplementationType { get; set; }
        public Type ServiceInterfaceType { get; set; }
        public String EndpointName { get; set; }

        protected override void InitializeRuntime()
        {
            //TODO:  reinstate this once mono wcf stack is stable
            /*
            ServiceEndpoint jsonEndpoint = this.AddServiceEndpoint(ServiceInterfaceType,
                new WebHttpBinding(),
                "Json");
            WebHttpBehavior jsonBehaviour = new WebHttpBehavior();
            jsonBehaviour.DefaultBodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.Wrapped;
            jsonBehaviour.DefaultOutgoingRequestFormat = System.ServiceModel.Web.WebMessageFormat.Json;
            jsonBehaviour.DefaultOutgoingResponseFormat = System.ServiceModel.Web.WebMessageFormat.Json;
            jsonEndpoint.Behaviors.Add(jsonBehaviour);
             * */

            ServiceEndpoint soapEndpoint = this.AddServiceEndpoint(ServiceInterfaceType,
                new BasicHttpBinding(),
                "Soap");

            ServiceMetadataBehavior metadataBehavior = new ServiceMetadataBehavior();
            metadataBehavior.HttpGetEnabled = true;
            this.Description.Behaviors.Add(metadataBehavior);

            base.InitializeRuntime();
        }
    }

}

