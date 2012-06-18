package uk.co.linn.gwt.services.client;

import com.google.gwt.core.client.GWT;
import com.google.gwt.http.client.Request;
import com.google.gwt.http.client.RequestBuilder;
import com.google.gwt.http.client.RequestCallback;
import com.google.gwt.http.client.RequestException;
import com.google.gwt.http.client.Response;
import com.google.gwt.user.client.rpc.AsyncCallback;
import com.google.gwt.xml.client.Document;
import com.google.gwt.xml.client.Element;
import com.google.gwt.xml.client.XMLParser;
import com.google.gwt.xml.client.Node;

public class Service {
    
    private final String kDomainUpnpOrg = "upnp.org";
    private final String kDomainSchemasUpnpOrg = "schemas.upnp.org";

    public final AsyncCallback kCallbackNull = new AsyncCallback() {
		public void onFailure(Throwable caught) {
		}

		public void onSuccess(Object result) {
		}
	};
    
    protected Service(String aDomain, String aType, int aVersion, String aId) {
        iDomain = aDomain;
        iType = aType;
        iVersion = aVersion;
        iId = aId;
        
        iUriDomain = GWT.getModuleBaseURL();
        iUriDomain = iUriDomain.substring(iUriDomain.indexOf("//") + 2); // remove http://
        iUriDomain = iUriDomain.substring(0, iUriDomain.indexOf("/")); // leave domain
        iUri = "http://" + iUriDomain + "/" + iId + "/" + iType + "/control";
        
        if (iDomain.equals(kDomainUpnpOrg)) {
            iDomain = kDomainSchemasUpnpOrg;
        }
        
        iDomain = iDomain.replace('.', '-');
    }
    
    protected class ReadEnvelope {
        public ReadEnvelope(String aAction, String aBody) {
        	iBody = ReadElement("s:Body", aBody);
        }
        
        public String ReadElement(String aName, String aXml) {
        	String startTag = "<" + aName;
        	String endTag = "</" + aName + ">";
        	int startIndex = aXml.indexOf(startTag);
        	int endIndex = aXml.indexOf(endTag);
        	if (startIndex > 0) {
        		if (endIndex > startIndex) {
        			String withstart = aXml.substring(startIndex, endIndex);
        			startIndex = withstart.indexOf(">");
        			if (startIndex > 0) {
        				return (withstart.substring(startIndex + 1));
        			}
        		}
        	}
        	return ("");
        }

        public int ReadInt(String aName) {
        	String element = ReadElement(aName, iBody);
        	
        	try
        	{
        		return (Integer.parseInt(element));
        	}
        	catch (NumberFormatException e)
        	{
        	}
        	
    		return (0);
        }

        public int ReadUint(String aName) {
        	String element = ReadElement(aName, iBody);
        	
        	try
        	{
        		return (Integer.parseInt(element));
        	}
        	catch (NumberFormatException e)
        	{
        	}
        	
    		return (0);
        }

        public boolean ReadBool(String aName) {
        	String element = ReadElement(aName, iBody);
        	
        	if (element == "1")
        	{
        		return (true);
        	}
        	
        	if (element == "true") {
        		return (true);
        	}
        	
    		return (false);
        }
        
        public String ReadString(String aName) {
        	return (Unescape(ReadElement(aName, iBody)));
        }
        
        public String ReadBinary(String aName) {
        	return (ReadElement(aName, iBody));
        }
        
        private String Unescape(String aValue) {
            char[] value = aValue.toCharArray();
            
            int bytes = value.length;
            
            int j = 0;
            
            for (int i = 0; i < bytes; i++) {
                if (value[i] != '&') {
                    value[j++] = value[i];
                    continue;
                }
                if (++i < bytes) {
                    if (value[i] == 'l') {
                        if (++i < bytes) {
                            if (value[i] == 't') {
                                if (++i < bytes) {
                                    if (value[i] == ';') {
                                        value[j++] = '<';
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                    else if (value[i] == 'g') {
                        if (++i < bytes) {
                            if (value[i] == 't') {
                                if (++i < bytes) {
                                    if (value[i] == ';') {
                                        value[j++] = '>';
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                    else if (value[i] == 'a') {
                        if (++i < bytes) {
                            if (value[i] == 'm') {
                                if (++i < bytes) {
                                    if (value[i] == 'p') {
                                        if (++i < bytes) {
                                            if (value[i] == ';') {
                                                value[j++] = '&';
                                                continue;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (value[i] == 'p') {
                                if (++i < bytes) {
                                    if (value[i] == 'o') {
                                        if (++i < bytes) {
                                            if (value[i] == 's') {
                                                if (++i < bytes) {
                                                    if (value[i] == ';') {
                                                        value[j++] = '\'';
                                                        continue;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (value[i] == 'q') {
                        if (++i < bytes) {
                            if (value[i] == 'u') {
                                if (++i < bytes) {
                                    if (value[i] == 'o') {
                                        if (++i < bytes) {
                                            if (value[i] == 't') {
                                                if (++i < bytes) {
                                                    if (value[i] == ';') {
                                                        value[j++] = '\"';
                                                        continue;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return (new String(value, 0, j));
        }
        
        String iBody; 
    }
    
    protected class WriteEnvelope {
        private final int kEnvelopeBytes = 400;
        private final int kSoapActionBytes = 400;
        
        private final String kEnvelopeStart = "<?xml version=\"1.0\"?>" +
        "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
        "<s:Body s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"><u:";
                
        public WriteEnvelope(String aAction) {
            iAction = aAction;
            
            iEnvelope = new StringBuffer(kEnvelopeBytes);
            iEnvelope.append(kEnvelopeStart);
            iEnvelope.append(iAction);
            iEnvelope.append(" xmlns:u=\"urn:");
            iEnvelope.append(iDomain);
            iEnvelope.append(":service:");
            iEnvelope.append(iType);
            iEnvelope.append(":");
            iEnvelope.append(iVersion);
            iEnvelope.append("\">");
        }
        
        public void WriteTagStart(String aName) {
            iEnvelope.append('<');
            iEnvelope.append(aName);
            iEnvelope.append('>');
        }
        
        public void WriteTagEnd(String aName) {
            iEnvelope.append('<');
            iEnvelope.append('/');
            iEnvelope.append(aName);
            iEnvelope.append('>');
        }
        
        public void WriteInt(String aName, int aValue) {
            WriteTagStart(aName);
            iEnvelope.append(aValue);
            WriteTagEnd(aName);
        }

        public void WriteBool(String aName, boolean aValue) {
            WriteTagStart(aName);
            iEnvelope.append(aValue ? 1 : 0);
            WriteTagEnd(aName);
        }
        
        public void WriteString(String aName, String aValue) {
            WriteTagStart(aName);
            iEnvelope.append(aValue);
            WriteTagEnd(aName);
        }
        
        public void WriteBinary(String aName, String aValue) {
            WriteTagStart(aName);
            iEnvelope.append(aValue);
            WriteTagEnd(aName);
        }
        
        public void Send(final AsyncCallback aCallback) {
            iEnvelope.append("</u:");
            iEnvelope.append(iAction);
            iEnvelope.append("></s:Body></s:Envelope>");
            
            // POST path of control URL HTTP/1.1
            // HOST: host of control URL:port of control URL
            // CONTENT-LENGTH: bytes in body
            // CONTENT-TYPE: text/xml; charset="utf-8"
            // SOAPACTION: "urn:schemas-upnp-org:service:serviceType:v#actionName"
 
            RequestBuilder builder = new RequestBuilder(RequestBuilder.POST, iUri);
            //builder.setHeader("CONTENT-LENGTH",String.valueOf(iEnvelope.length()));
            builder.setHeader("CONTENT-TYPE", "text/xml; charset=\"utf-8\"");
            
            StringBuffer soapaction = new StringBuffer(kSoapActionBytes);
            
            soapaction.append("\"");
            soapaction.append("urn:");
            soapaction.append(iDomain);
            soapaction.append(":service:");
            soapaction.append(iType);
            soapaction.append(":");
            soapaction.append(iVersion);
            soapaction.append("#");
            soapaction.append(iAction);
            soapaction.append("\"");
            
            builder.setHeader("SOAPACTION", soapaction.toString());
            
            try {
                builder.sendRequest(iEnvelope.toString(), new RequestCallback() {
                    public void onResponseReceived(Request request, Response response) {
                        int status = response.getStatusCode();
                        
                        if (status == 200) {
                            // HTTP/1.1 200 OK
                            // CONTENT-LENGTH: bytes in body
                            // CONTENT-TYPE: text/xml; charset="utf-8"
                            // DATE: when response was generated
                            // EXT:
                            // SERVER: OS/version UPnP/1.0 product/version
                            // <?xml version="1.0"?>
                            // <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
                            // <s:Body s:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/">
                            // <u:actionNameResponse xmlns:u="urn:schemas-upnp-org:service:serviceType:v">
                            // <argumentName>out arg value</argumentName>
                            // other out args and their values go here, if any
                            // </u:actionNameResponse>
                            // </s:Body>
                            // </s:Envelope>
                            ReadEnvelope envelope = new ReadEnvelope(iAction, response.getText());
                            aCallback.onSuccess(envelope);
                        }
                        else if (status == 500) {
                            // <?xml version="1.0"?>
                            // <s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">
                            // <s:Body s:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/">
                            // <s:Fault>
                            // <faultcode>s:Client</faultcode>
                            // <faultstring>UPnPError</faultstring>
                            // <detail>
                            // <UPnPError xmlns="urn:schemas-upnp-org:control-1-0">
                            // <errorCode>error code</errorCode>
                            // <errorDescription>error string</errorDescription>
                            // </UPnPError>
                            // </detail>
                            // </s:Fault>
                            // </s:Body>
                            //</s:Envelope>
                            
                            Document xml = XMLParser.parse(response.getText());
                            
                            Element envelope = xml.getDocumentElement();
                            
                            if (envelope != null) {
                                Node body = envelope.getFirstChild();
                                if (body != null) {
                                    Node fault = body.getFirstChild();
                                    if (fault != null) {
                                        Node detail = fault.getChildNodes().item(2);
                                        if (detail !=null) {
                                            Node error = detail.getFirstChild();
                                            if (error != null) {
                                                Node errorCode = error.getChildNodes().item(0);
                                                if (errorCode != null) {
                                                    Node textCode = errorCode.getFirstChild();
                                                    if (textCode != null) {
                                                        int code = 0;
                                                        try {
                                                            code = Integer.parseInt(textCode.getNodeValue());
                                                            Node errorDescription = error.getChildNodes().item(1);
                                                            if (errorDescription != null) {
                                                                Node textDescription = errorDescription.getFirstChild();
                                                                if (textDescription != null) {
                                                                    String description = textDescription.getNodeValue();
                                                                    aCallback.onFailure(new ServiceException(code, description));
                                                                    return;
                                                                }
                                                            }
                                                        }
                                                        catch (NumberFormatException e) {
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            aCallback.onFailure(new ServiceException(301, "Xml Error"));
                        }
                        else {
                            aCallback.onFailure(new ServiceException(status + 1000, response.getStatusText()));
                        }
                    }

                    public void onError(Request request, Throwable exception) {
                        aCallback.onFailure(new ServiceException(300, "Communications Error"));
                    }
                });
            }
            catch (RequestException e) {
                
            }
        }

        private String iAction;
        private StringBuffer iEnvelope;
    }

    private String iDomain;
    private String iType;
    private int iVersion;
    private String iId;
    public String iUri;
    public String iUriDomain;
}


