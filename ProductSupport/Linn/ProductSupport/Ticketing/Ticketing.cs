namespace Linn.ProductSupport.Ticketing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Xml;
    using System.Xml.Serialization;
    using Linn.Tickets.Resources;
    using System.ComponentModel.DataAnnotations;
    using Linn;
    using Linn.ProductSupport.Diagnostics;

    public class Ticket
    {

        ValidationContext iValidationContext;
        List<ValidationResult> iValidationResults;
        TicketRequestResource iTicketData;
        string iTicketXmlData = "";
        bool iValid;
        string iValidationInfo;

        public Ticket(string aInstallerVersion, string aOperatingSystem, string aTimeZoneId, string aProductName, string aEntryPoint,
                      string aFirstName, string aLastName, string aEmail, string aPhoneNumber, string aContactNotes, string aFaultDescription,
                      string aProductId, string aProductFirmware, string aProductMacAddress, ListOfCategoryResource aListOfCategory, ListOfTestResource aListOfTest)
        {

            // Create the Ticket
            iTicketData = new TicketRequestResource
            {
                InstallerVersion = aInstallerVersion,
                TimeZoneId = aTimeZoneId,
                OperatingSystem = aOperatingSystem,
                ProductName = aProductName,
                FirstName = aFirstName,
                LastName = aLastName,
                Email = aEmail,
                PhoneNumber = aPhoneNumber,
                FaultDescription = aFaultDescription,
                ContactNotes = aContactNotes,
                ProductId = aProductId,
                ProductFirmware = aProductFirmware,
                ProductMacAddress = aProductMacAddress,
                InstallerReport = CreateInstallerReport(aEntryPoint, aListOfCategory, aListOfTest)

            };

            // Validate the Ticket's data
            iValidationContext = new ValidationContext(iTicketData, null, null);
            iValidationResults = new List<ValidationResult>();
            iValid = Validator.TryValidateObject(iTicketData, iValidationContext, iValidationResults, true);

            // Copy the validation report into a string
            foreach (ValidationResult vr in iValidationResults)
            {
                iValidationInfo += (vr.ErrorMessage);
            }

            // now call the validate function to get any fails that aren't determined by the attributes
            IEnumerable<ValidationResult> results = iTicketData.Validate(iValidationContext);
            foreach (ValidationResult vr2 in results)
            {
                iValidationInfo += (vr2.ErrorMessage);
            }

        }


        // Call this when a device (Box) has been selected in the list of discovered devices
        public static bool SubmitTicket(string aInstallerVersion, string aProductName, string aEntryPoint, string aFirstName, string aLastName,
                                        string aEmail, string aPhoneNumber, string aContactNotes, string aFaultDescription,
                                        Diagnostics aDiagnostics, Box aBox, out string aValidationReport, out string aTicketXmlData)
        {
            // get the test results from Diagnostics (a List of Tests)
            while (!aDiagnostics.AllComplete())
            {
                System.Threading.Thread.Sleep(1000);
            }            
            
            ListOfTestResource testList = new ListOfTestResource();

            var testResults = aDiagnostics.Results();
            foreach (TestResult result in testResults)
            {
                TestResourceResult passFailWarn;

                switch (result.result)
                {
                    case "PASS":
                        passFailWarn = TestResourceResult.Pass;
                        break;
                    case "WARN":
                        passFailWarn = TestResourceResult.Warning;
                        break;
                    default:
                    case "FAIL":
                        passFailWarn = TestResourceResult.Fail;
                        break;
                }

                CreateTest(testList, result.title, passFailWarn, result.description, result.startTime, result.endTime, result.content);
            }


            // Get the other (non-test) stuff (a List of Categories)
            ListOfCategoryResource categoryList = new ListOfCategoryResource();
            var info = new DiagnosticInfo(aBox);

            var categoryDeviceInfo = Ticket.CreateCategory(categoryList, "DeviceInfo");
            Ticket.CreateItem(categoryDeviceInfo, "All Device Info", info.DeviceInfo);

            var categorySystemInfo = Ticket.CreateCategory(categoryList, "SystemInfo");
            Ticket.CreateItem(categorySystemInfo, "All System Info", info.SystemInfo);

            var categoryUserLog = Ticket.CreateCategory(categoryList, "UserLog");
            Ticket.CreateItem(categoryUserLog, "Full UserLog", info.UserLog);


            return(SubmitTicket(aInstallerVersion, aProductName, aEntryPoint, aFirstName, aLastName,
                                        aEmail, aPhoneNumber, aContactNotes, aFaultDescription, 
                                        info.ProductId, info.SoftwareVersion, info.MacAddress, categoryList, testList,
                                        out aValidationReport, out aTicketXmlData));
        }

           
        public static bool SubmitTicket(string aInstallerVersion, string aProductName, string aEntryPoint, string aFirstName, string aLastName,
                                        string aEmail, string aPhoneNumber, string aContactNotes, string aFaultDescription, 
                                        string aProductId, string aSoftwareVersion, string aMacAddress, ListOfCategoryResource aCategoryList, ListOfTestResource aTestList,
                                        out string aValidationReport, out string aTicketXmlData)
        {
            string timeZoneId = System.TimeZoneInfo.Local.Id;
            string operatingSystem = Linn.SystemInfo.VersionString;
            

            Ticket ticket = new Ticket(aInstallerVersion, operatingSystem, timeZoneId, aProductName, aEntryPoint,
                                        aFirstName, aLastName, aEmail, aPhoneNumber, aContactNotes, aFaultDescription,
                                        aProductId, aSoftwareVersion, aMacAddress, aCategoryList, aTestList);

            aValidationReport = "";
            aTicketXmlData = "";


            if (ticket.Valid())
            {
                bool success = ticket.Submit(out aValidationReport);
                aTicketXmlData = ticket.iTicketXmlData;
                return(success);
            }
            else
            {
                aValidationReport = ticket.ValidationInfo();
                aTicketXmlData = ticket.iTicketXmlData;
                return (false);
            }

        }


        private InstallerReportResource CreateInstallerReport(string aEntryPoint, ListOfCategoryResource aListOfCategory, ListOfTestResource aListOfTest)
        {
            return new InstallerReportResource
            {
                EntryPoint = aEntryPoint,
                Information = aListOfCategory,
                Tests = aListOfTest
            };
        }


        private static void CreateTest(ListOfTestResource aTestList, string aTitle, TestResourceResult aResult, string aResultDescription, DateTime aStartTime, DateTime aEndTime, string aContent)
        {
            TestResource test = new TestResource
            {
                Title = aTitle,
                Result = aResult,
                ResultDescription = aResultDescription,
                Content = new XmlDocument().CreateCDataSection(aContent),
                StartedUtc = aStartTime,
                FinishedUtc = aEndTime
            };

            aTestList.Tests.Add(test);
        }


        private static void CreateItem(CategoryResource aCategory, string aTitle, string aInfo)
        {
            ItemResource item = new ItemResource
            {
                Title = aTitle,
                Content = new XmlDocument().CreateCDataSection(aInfo),
            };

            aCategory.Items.Add(item);
        }


        private static CategoryResource CreateCategory(ListOfCategoryResource CategoryList, string aTitle)
        {
            CategoryResource category = new CategoryResource
            {
                Title = aTitle
            };

            CategoryList.Categories.Add(category);
            return (category);
        }


        private string ValidationInfo()
        {
            return(iValidationInfo);
        }


        private bool Valid()
        {
            return(iValid);
        }


        private bool Submit(out string aResponse)
        {
            bool success = false;
            aResponse = "";

            if (iValid)
            {
                iTicketXmlData = iTicketData.ToXmlString();
                success = PostTicket(iTicketXmlData, out aResponse);
                Console.WriteLine(iTicketXmlData);
                Console.WriteLine("\n\n\n" + aResponse);
            }
            else
            {
                // we should perhaps throw here
            }

            return(success);
        }



        // Posts the data to the predefined URI
        // The data is posted raw (XML serialisation is assumed to have been done already)
        private bool PostTicket(string aData, out string aResponse)
        {
            const string kSubmitUri = "http://www.linn.co.uk/api/ticket-requests/";  // production URI : must change over to this URI before release
            //const string kSubmitUri = "http://www-sys.linn.co.uk/api/ticket-requests/"; // test URI
            
            StreamReader reader = null;
            Stream dataStream = null;
            WebResponse response = null;
            bool success = false;
            aResponse = "";
            byte[] byteArray = Encoding.UTF8.GetBytes(aData);  // encode the data

            try
            {
                // Create a webRequest using a URL that can receive a post.
                WebRequest webRequest = WebRequest.Create(new Uri(kSubmitUri));
                webRequest.Method = "POST";
                webRequest.Credentials = CredentialCache.DefaultCredentials;
                webRequest.ContentLength = byteArray.Length;
                webRequest.ContentType = "application/xml";

                HttpWebRequest httpWebRequest = (HttpWebRequest)webRequest;
                httpWebRequest.Accept = "application/json, application/xml, text/json, text/x-json, text/javascript, text/xml";
                httpWebRequest.Host = "www.linn.co.uk"; // production URI : must change over to this URI before release
                //httpWebRequest.Host = "www-sys.linn.co.uk"; // test URI

                httpWebRequest.UserAgent = "RestSharp 102.0.0.0";
                httpWebRequest.ProtocolVersion = new Version(1, 1);
                httpWebRequest.KeepAlive = true;

                // Microsoft bug (when using HTTP 1.1)
                // httpWebRequest.Expect always gets reset to "100Continue"
                // we need it to be NULL (which is supposed to be its default value)
                // next line stops the low level code from resetting it to 100Continue
                ServicePointManager.Expect100Continue = false;

                dataStream = httpWebRequest.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                response = httpWebRequest.GetResponse();

                if (((HttpWebResponse)response).StatusCode != HttpStatusCode.Created)
                {
                    throw new HttpPostFailed();
                }

                dataStream = response.GetResponseStream();
                reader = new StreamReader(dataStream);
                aResponse = reader.ReadToEnd();
                success = true;
            }
            catch (Exception exc)
            {
                success = false;
                aResponse = "Could not send the data to Linn.\n\nPlease check your internet connection\n" + exc.Message;
            }
            finally
            {
                // Clean up the streams.
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                if (dataStream != null)
                {
                    dataStream.Close();
                    dataStream.Dispose();
                }
                if (response != null)
                {
                    response.Close();
                }
            }

            return success;
        }


        private class HttpPostFailed : System.Exception
        {
            public HttpPostFailed() : base("HTTP Post Failed") { }
        }



    }

}

