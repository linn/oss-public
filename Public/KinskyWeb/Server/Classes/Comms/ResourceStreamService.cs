using System;
using System.IO;
using System.Reflection;
using System.ServiceModel;
//using System.ServiceModel.Web;
using System.ServiceModel.Channels;
using System.Threading;
using System.Text;
using KinskyWeb.Kinsky;
using System.Drawing;
using System.Drawing.Imaging;
using KinskyWeb.Helpers.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using KinskyWeb.Services;
using Linn;
using System.Text.RegularExpressions;
using HttpServer;
using System.Net;

namespace KinskyWeb.Comms
{
    // TODO: remove this once mono wcf stack is stable
    class ResourceStreamServer
    {
        private HttpServer.HttpListener iListener;
        private bool iDisableCaching;
        private Dictionary<string, SelfHostingServiceHost> iServiceHosts;
        private const int kMaxRequests = 50;

        public ResourceStreamServer(System.Net.IPAddress ipAddress, uint port, bool aDisableCaching, SelfHostingServiceHost[] aServiceHosts)
        {
            iListener = HttpServer.HttpListener.Create(ipAddress, (int)port);

            iDisableCaching = aDisableCaching;
            iServiceHosts = new Dictionary<string, SelfHostingServiceHost>();
            foreach (SelfHostingServiceHost host in aServiceHosts)
            {
                iServiceHosts.Add(host.EndpointName.ToLower(), host);
            }
        }

        public void Start()
        {
            iListener.RequestReceived += ProcessRequest;
            iListener.Start(kMaxRequests);
        }

        public void Stop()
        {
            iListener.RequestReceived -= ProcessRequest;
            iListener.Stop();
        }

        private void ProcessRequest(object sender, RequestEventArgs aListenerContext)
        {
            IHttpClientContext context = (IHttpClientContext)sender;
            byte[] response = null;
            IHttpResponse responseContext = aListenerContext.Request.CreateResponse(context);
            string filename = aListenerContext.Request.Uri.AbsolutePath;
            //Trace.WriteLine(Trace.kKinskyWeb, String.Format("Request: {0}", filename));
            Stream resource = null;
            try
            {

                // hack: mono wcf is currently unstable - intercept json messages and invoke the service method manually
                if (aListenerContext.Request.Headers["Content-Type"] != null &&
                    aListenerContext.Request.Headers["Content-Type"].ToString().ToLower().IndexOf("application/json") != -1 &&
                    aListenerContext.Request.Uri.Segments.Length > 3)
                {


                    //todo: parse this from request
                    Encoding encoding = System.Text.Encoding.UTF8;
                    responseContext.AddHeader("Content-Type", "application/json");
                    String endpoint = aListenerContext.Request.Uri.Segments[1].Replace("/", "");
                    String methodName = aListenerContext.Request.Uri.Segments[3].Replace("/", "");
                    if (endpoint != "WidgetContainer" && methodName != "UpdateState")
                    {
                        Trace.Write(Trace.kKinskyWeb, "Request made to " + endpoint + "." + methodName + "\n");
                    }
                    if (iServiceHosts.ContainsKey(endpoint.ToLower()))
                    {
                        JContainer json;
                        using (StreamReader sReader = new StreamReader(aListenerContext.Request.Body, encoding))
                        {
                            using (JsonTextReader jReader = new JsonTextReader(sReader))
                            {
                                json = (JContainer)new JsonSerializer().Deserialize(jReader, typeof(JContainer));
                            }
                        }

                        Dictionary<string, JProperty> parameterMap = new Dictionary<string, JProperty>();
                        foreach (JProperty token in json.Children())
                        {
                            string parameterName = token.Name;
                            parameterMap[parameterName] = token;
                        }

                        SelfHostingServiceHost host = iServiceHosts[endpoint.ToLower()];
                        Type implType = host.ServiceImplementationType;
                        Type interfaceType = host.ServiceInterfaceType;

                        Object implObject = Activator.CreateInstance(implType);
                        MethodInfo reflectMethod = implType.GetInterface(interfaceType.Name).GetMethod(methodName);
                        ParameterInfo[] reflectParamInfo = reflectMethod.GetParameters();
                        List<Object> actualParameters = new List<Object>();
                        foreach (ParameterInfo reflectParam in reflectParamInfo)
                        {
                            if (!reflectParam.IsRetval)
                            {
                                if (parameterMap.ContainsKey(reflectParam.Name))
                                {
                                    object param = null;
                                    try
                                    {
                                        param = JsonConvert.DeserializeObject(parameterMap[reflectParam.Name].Value.ToString(), reflectParam.ParameterType);
                                    }
                                    catch
                                    {
                                        if (typeof(System.Guid).IsAssignableFrom(reflectParam.ParameterType) &&
                                            parameterMap[reflectParam.Name].Value.ToString().IndexOf("null") != -1)
                                        {
                                            // ignore: json library has a bug deserializing null guids
                                        }
                                        else
                                        {
                                            throw;
                                        }
                                    }
                                    actualParameters.Add(param);
                                }
                                else
                                {
                                    actualParameters.Add(null);
                                }
                            }
                        }
                        Object methodResult = reflectMethod.Invoke(implObject, actualParameters.ToArray());
                        using (MemoryStream memStream = new MemoryStream())
                        {
                            using (StreamWriter sWriter = new StreamWriter(memStream, encoding))
                            {
                                using (JsonTextWriter jWriter = new JsonTextWriter(sWriter))
                                {
                                    // wrapped response like WCF does
                                    jWriter.WriteStartObject();
                                    jWriter.WritePropertyName(String.Format("{0}Result", methodName));
                                    new JsonSerializer().Serialize(jWriter, methodResult);
                                    jWriter.WriteEndObject();
                                    jWriter.Flush();
                                    response = new byte[memStream.Length];
                                    memStream.Position = 0;
                                    memStream.Read(response, 0, response.Length);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (aListenerContext.Request.Uri.Query.ToLower().StartsWith("?file="))
                    {
                        string url = String.Format("http://{0}:{1}/Kinsky{2}", 
                            KinskyStack.GetDefault().Helper.Stack.Status.Interface.IPAddress,
                            Linn.Kinsky.HttpServer.kPortKinskyWeb,
                            aListenerContext.Request.Uri.Query);
                        HttpWebRequest webRequest = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
                        HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                        using (Stream responseStream = webResponse.GetResponseStream())
                        {
                            resource = new MemoryStream();
                            responseStream.CopyTo(resource);
                            resource.Position = 0;
                        }
                        responseContext.AddHeader("Content-Type", webResponse.ContentType);
                        responseContext.AddHeader("Cache-Control", "no-cache");
                    }
                    else
                    {

                        switch (filename.ToLower())
                        {
                            case "/":
                            case "/desktop":
                                filename = "Layouts/Desktop/kinsky.html";
                                resource = new ResourceStreamService().GetResourceStream(filename);
                                if (System.Diagnostics.Debugger.IsAttached || iDisableCaching)
                                {
                                    // use no-cache when debugging
                                    responseContext.AddHeader("Cache-Control", "no-cache");
                                }
                                responseContext.AddHeader("Content-Type", "text/html");
                                break;
                            case "/mobile":
                                filename = "Layouts/Portrait/Medium/kinsky.html";
                                resource = new ResourceStreamService().GetResourceStream(filename);
                                if (System.Diagnostics.Debugger.IsAttached || iDisableCaching)
                                {
                                    // use no-cache when debugging
                                    responseContext.AddHeader("Cache-Control", "no-cache");
                                }
                                responseContext.AddHeader("Content-Type", "text/html");
                                break;
                            case "/artwork":
                                Regex regex = new Regex("[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

                                Guid widgetID = Guid.Empty;
                                if (regex.IsMatch(aListenerContext.Request.Uri.Query))
                                {
                                    try
                                    {
                                        widgetID = new Guid(regex.Match(aListenerContext.Request.Uri.Query).Value);
                                    }
                                    catch (System.FormatException) { }
                                }
                                IViewKinskyTrack widget = WidgetRegistry.GetDefault().Get(widgetID) as IViewKinskyTrack;
                                Image artwork = null;
                                if (widget != null)
                                {
                                    artwork = widget.Artwork();
                                }
                                if (artwork == null)
                                {
                                    artwork = Linn.Kinsky.Properties.Resources.NoAlbumArt;
                                }
                                lock (artwork)
                                {
                                    resource = artwork.GetStream(ImageFormat.Png);
                                }
                                responseContext.AddHeader("Content-Type", "image/png");
                                responseContext.AddHeader("Cache-Control", "no-cache");
                                break;
                            default:
                                if (System.Diagnostics.Debugger.IsAttached || iDisableCaching)
                                {
                                    // use no-cache when debugging
                                    responseContext.AddHeader("Cache-Control", "no-cache");
                                }
                                ResourceStreamService rs = new ResourceStreamService();
                                resource = rs.GetResourceStream(filename.Substring(1));

                                responseContext.AddHeader("Content-Type", rs.GetContentType(Path.GetExtension(filename)));
                                break;
                        }
                    }


                }
                if (resource != null)
                {
                    response = new byte[resource.Length];
                    resource.Read(response, 0, response.Length);
                    resource.Close();
                    resource.Dispose();
                }
                responseContext.Status = System.Net.HttpStatusCode.OK;
                responseContext.Body.Write(response, 0, response.Length);
                responseContext.Body.Flush();
                responseContext.Send();

            }
            catch (ResourceStreamService.ResourceNotFoundException)
            {
                try
                {
                    responseContext.Status = System.Net.HttpStatusCode.NotFound;
                    response = Encoding.UTF8.GetBytes(String.Format("Resource '{0}' not found.", filename));
                    responseContext.Body.Write(response, 0, response.Length);
                    responseContext.Body.Flush();
                    responseContext.Send();
                    UserLog.WriteLine("Resource not found.  Resource: " + filename);
                }
                catch (Exception ex)
                {
                    UserLog.WriteLine("Unhandled exception writing resource not found response to client.");
                    UserLog.WriteLine(ex.Message + ex.StackTrace);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    responseContext.Status = System.Net.HttpStatusCode.InternalServerError;
                    response = Encoding.UTF8.GetBytes("Error in request handler.  See logs for further details.");
                    responseContext.Body.Write(response, 0, response.Length);
                    responseContext.Body.Flush();
                    responseContext.Send();
                    UserLog.WriteLine("Error in request handler.  Resource: " + filename);
                    UserLog.WriteLine(ex.Message + ex.StackTrace);
                }
                catch (Exception ex2)
                {
                    UserLog.WriteLine("Unhandled exception writing error response to client.");
                    UserLog.WriteLine(ex2.Message + ex2.StackTrace);
                }
            }
        }

    }




    /* TODO: reinstate this once mono wcf stack is stable
        [ServiceContract]
        interface IResourceStreamService
        {
            [OperationContract, WebGet(UriTemplate = "*")]
            Stream GetResourceStream();
            [OperationContract, WebGet(UriTemplate = "KinskyWeb")]
            Stream GetLandingPage();
        }
*/

    public class ResourceStreamService // : IResourceStreamService
    {

        /* TODO: reinstate this once mono wcf stack is stable
        #region IResourceStreamService Members

        public System.IO.Stream GetResourceStream()
        {
            // substring(1) accounts for forward slash appended onto baseURI
            return GetWCFStream(WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri.AbsolutePath.Substring(1));
        }

        public System.IO.Stream GetLandingPage()
        {
            return GetWCFStream("default.html");
        }

        private System.IO.Stream GetWCFStream(string name)
        {
            try
            {
                if (System.Diagnostics.Debugger.IsAttached())
                {
                    WebOperationContext.Current.OutgoingResponse.Headers["Cache-Control"] = "no-cache";
                }
                WebOperationContext.Current.OutgoingResponse.ContentType = GetContentType(Path.GetExtension(name));
                return GetHttpResourceStream(name);
            }
            catch (ResourceNotFoundException)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.NotFound;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = String.Format("Resource '{0}' not found.", name);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(String.Format("Resource '{0}' not found.", name));
                Stream result = new MemoryStream(bytes);
                result.Position = 0;
                return result;
            }
            catch (Exception ex)
            {
                //todo: write exception to response stream
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                WebOperationContext.Current.OutgoingResponse.StatusDescription = String.Format("Error caught: {0}\n{1}", ex.Message, ex.StackTrace);
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(String.Format("Error caught: {0}\n{1}", ex.Message, ex.StackTrace));
                Stream result = new MemoryStream(bytes);
                result.Position = 0;
                return result;
            }
        }
        #endregion
        
        */

        public static string ResourceLocation { get; set; }

        public Stream GetResourceStream(string filename)
        {
            Stream resource;
            // if we're debugging it's a lot easier to develop client side without rebuilding
            if (ResourceLocation != null)
            {
                String baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                String resourceDirectory = Path.GetDirectoryName(Path.Combine(baseDirectory, ResourceLocation));
                resource = GetFileResourceStream(resourceDirectory, filename);
            }
            else
            {
                resource = GetManifestResourceStream(filename);
            }
            return resource;
        }

        public class ResourceNotFoundException : ApplicationException
        {
            public ResourceNotFoundException() : base() { }
            public ResourceNotFoundException(string message) : base(message) { }
        }

        public Stream GetFileResourceStream(string baseDirectory, string fileName)
        {
            string resourceName = Path.Combine(baseDirectory, fileName);
            if (!File.Exists(resourceName))
            {
                throw new ResourceNotFoundException(String.Format("Resource {0} not found.", fileName));
            }
            // security check: make sure we never serve anything below the baseDirectory
            string resourceDirectory = Path.GetDirectoryName(resourceName);
            if (!resourceDirectory.StartsWith(baseDirectory))
            {
                throw new ArgumentException("Security violation.  Attempt to request a file from a subdirectory of base directory.");
            }

            Stream s = new FileInfo(resourceName).OpenRead();
            s.Position = 0;
            return s;
        }

        public Stream GetManifestResourceStream(string name)
        {
            object res = Resources.ResourceManager.GetObject(name, Resources.Culture);
            if (res != null)
            {
                MemoryStream ms = new MemoryStream((byte[])res);
                ms.Position = 0;
                return ms;
            }
            else
            {
                throw new ResourceNotFoundException(String.Format("Resource {0} not found.", name));
            }
        }

        public string GetContentType(string fileExtension)
        {
            String contentType;
            switch (fileExtension.ToLower())
            {
                case ".htm":
                case ".html":
                    contentType = "text/html";
                    break;
                case ".xsl":
                case ".xslt":
                case ".xml":
                    contentType = "text/xml";
                    break;
                case ".xap":
                    contentType = "application/x-silverlight-2";
                    break;
                case ".css":
                    contentType = "text/css";
                    break;
                case ".js":
                    contentType = "application/javascript";
                    break;
                case ".gif":
                    contentType = "image/gif";
                    break;
                case ".png":
                    contentType = "image/png";
                    break;
                case ".ico":
                    contentType = "image/vnd.microsoft.icon";
                    break;
                case ".jpg":
                case ".jpeg":
                    contentType = "image/jpeg";
                    break;
                default: contentType = "text/plain";
                    break;
            }
            return contentType;
        }

    }
}