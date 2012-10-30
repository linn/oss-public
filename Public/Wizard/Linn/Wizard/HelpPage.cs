using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;


namespace Linn.Wizard
{
    using Linn.ProductSupport;
    using Linn.ProductSupport.Diagnostics;
    using Linn.ProductSupport.Ticketing;

    public class HelpPage : BasePage
    {
        public HelpPage(PageControl aPageControl, PageDefinitions.Page aPageDefinition)
            : base(aPageControl, aPageDefinition)
        {
        }


        protected override void OnReceive(Session aSession, string aName, string aValue)
        {
            switch (aName)
            {
                case "Submit":
                    
                    string validationReport = "";


                    Send("Disable", "FormCloseButton");
                    Send("Unhide", "SubmitWaitSpinner");

                    if (!SubmitTicket(aSession, aValue, out validationReport))
                    {
                        // post failed
                        aSession.Send("SubmissionFailed", validationReport);
                    }
                    else
                    {
                        string ticketSubmissionReport = "";
                        // post successful
                        XmlTextReader reader = new XmlTextReader(new StringReader(validationReport));
                        reader.MoveToContent();
                        reader.ReadToDescendant("ticketId");
                        ticketSubmissionReport = reader.ReadElementContentAsString()+",";
                        reader.ReadToNextSibling("message");
                        ticketSubmissionReport += reader.ReadElementContentAsString();

                        aSession.Send("ReportSubmitted", ticketSubmissionReport);
                    }

                    Send("Hide", "SubmitWaitSpinner");
                    Send("Enable", "FormCloseButton");

                    break;
                case "HelpMenuItem1":
                    if (GetActionValue(iPageDefinition, "MenuItem1") != "")
                    {
                        aSession.Navigator.NextPageNoSave(aSession, GetActionValue(iPageDefinition, "MenuItem1"));
                    }
                    break;
                case "HelpMenuItem2":
                    if (GetActionValue(iPageDefinition, "MenuItem2") != "")
                    {
                        aSession.Navigator.NextPageNoSave(aSession, GetActionValue(iPageDefinition, "MenuItem2"));
                    }
                    break;
                case "HelpMenuItem3":
                    if (GetActionValue(iPageDefinition, "MenuItem3") != "")
                    {
                        aSession.Navigator.NextPageNoSave(aSession, GetActionValue(iPageDefinition, "MenuItem3"));
                    }
                    break;
                case "HelpMenuItem4":
                    if (GetActionValue(iPageDefinition, "MenuItem4") != "")
                    {
                        aSession.Navigator.NextPageNoSave(aSession, GetActionValue(iPageDefinition, "MenuItem4"));
                    }
                    break;
                case "HelpMenuItem5":
                    if (GetActionValue(iPageDefinition, "MenuItem5") != "")
                    {
                        aSession.Navigator.NextPageNoSave(aSession, GetActionValue(iPageDefinition, "MenuItem5"));
                    }
                    break;
                case "HelpMenuItem6":
                    if (GetActionValue(iPageDefinition, "MenuItem6") != "")
                    {
                        aSession.Navigator.NextPageNoSave(aSession, GetActionValue(iPageDefinition, "MenuItem6"));
                    }
                    break;
                case "HelpMenuItem7":
                    if (GetActionValue(iPageDefinition, "MenuItem7") != "")
                    {
                        aSession.Navigator.NextPageNoSave(aSession, GetActionValue(iPageDefinition, "MenuItem7"));
                    }
                    break;
                case "HelpMenuItem8":
                    if (GetActionValue(iPageDefinition, "MenuItem8") != "")
                    {
                        aSession.Navigator.NextPageNoSave(aSession, GetActionValue(iPageDefinition, "MenuItem8"));
                    }
                    break;
                case "HelpMenuItem9":
                    if (GetActionValue(iPageDefinition, "MenuItem9") != "")
                    {
                        aSession.Navigator.NextPageNoSave(aSession, GetActionValue(iPageDefinition, "MenuItem9"));
                    }
                    break;
                case "HelpMenuItem10":
                    if (GetActionValue(iPageDefinition, "MenuItem10") != "")
                    {
                        aSession.Navigator.NextPageNoSave(aSession, GetActionValue(iPageDefinition, "MenuItem10"));
                    }
                    break;
                default:
                    base.OnReceive(aSession, aName, aValue);
                    break;
            }
        }

   
        private bool SubmitTicket(Session aSession, string aValue, out string aValidationReport)
        {
            string entryPoint = aSession.Navigator.PreviousPageId;
            string productModel = aSession.Model.SelectedProduct;
            string thisAplicationRevision = "1.0.0";

            // get contents of the 6 text boxes from ticket page...
            char delim = ',';
            char escape = '/';
            string[] fields = SplitEscaped(aValue, delim, escape);

            string firstName = fields[0];
            string lastName = fields[1];
            string email = fields[2];
            string phone = fields[3];
            string contact = fields[4];
            string description = fields[5];

            bool success = false;
            aValidationReport = "";

            string xmlData; // only needed for debug
            success = Ticket.SubmitTicket(thisAplicationRevision, productModel, entryPoint, firstName, lastName, email, phone, contact, description,
                                            ModelInstance.Instance.Diagnostics, aSession.Model.SelectedBox, out aValidationReport, out xmlData);

            return(success);
        }

        private string[] SplitEscaped(string aString, char aDelimiter, char aEscapeChar)
        {
            // Does what standard Split method does but also deals with ...
            // ...escaped occurrences of the specified delimiting char
            // also removes the escape char from the string(s)
            string escapeSeq = "";
            escapeSeq += aEscapeChar;
            escapeSeq += aDelimiter;

            if (!aString.Contains(escapeSeq))
            {
                return ((aString.Split(aDelimiter)));
            }
            else
            {
                List<string> splitStr = new List<string>();

                string str = "";

                for (int i = 0; i < aString.Length; i++)
                {
                    char c = aString[i];
                    if (i == (aString.Length - 1))
                    {
                        // final char 
                        if (c != aDelimiter)
                        {
                            str += c;
                        }
                        else
                        {
                            splitStr.Add(str);
                        }
                        break;
                    }

                    if (aString.Substring(i, escapeSeq.Length) == escapeSeq)
                    {
                        // found an escaped delmiter... 
                        i++;                // discard escape char...
                        str += aString[i];  // and treat delimiter as normal char
                    }
                    else
                    {
                        if (c == aDelimiter)
                        {
                            splitStr.Add(str);
                            str = "";
                        }
                        else
                        {
                            str += c;
                        }
                    }
                }

                splitStr.Add(str); // add the final string to the list

                return (splitStr.ToArray());
            }
    
            
        }
    }
}
