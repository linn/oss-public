var kEnvelopeStart = '<?xml version="1.0"?>' +
        '<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/">' +
        '<s:Body s:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/"><u:';
		

var SoapRequest = function(aAction, aUrl, aDomain, aType, aVersion){
	this.iAction = aAction;
	this.iUrl = aUrl;
	this.iType = aType;
	this.iVersion = aVersion;
	this.iDomain = aDomain;
	this.iEnvelope = "";	
	this.WriteEnvelopeStart(aAction);
}

SoapRequest.prototype.WriteEnvelopeStart = function() {
	this.iEnvelope += kEnvelopeStart;
	this.iEnvelope += this.iAction;
	this.iEnvelope += ' xmlns:u="urn:';
	this.iEnvelope += this.iDomain;
	this.iEnvelope += ':service:';
	this.iEnvelope += this.iType;
	this.iEnvelope += ':';
	this.iEnvelope += this.iVersion;
	this.iEnvelope += '">';
}

SoapRequest.prototype.WriteEnvelopeEnd = function(){
	this.iEnvelope += "</u:";
	this.iEnvelope += this.iAction;
	this.iEnvelope += "></s:Body></s:Envelope>";
}
	
SoapRequest.prototype.GetSoapAction = function(){
	var soapAction = 'urn:';
	soapAction += this.iDomain;
	soapAction += ':service:';
	soapAction += this.iType;
	soapAction += ':';
	soapAction += this.iVersion;
	soapAction += '#';
	soapAction += this.iAction;
	return soapAction;
}
	
SoapRequest.prototype.CreateAjaxRequest = function(aCallbackFunction, aErrorFunction) {
	var thisObj = this;
    
    return $.ajax({
        url: this.iUrl,
        success: function(data, textStatus, jqXHR) {			
			if (data){
				if (aCallbackFunction) { 
					try { 
						aCallbackFunction(jqXHR); 
					}catch (e) {
						debug.log("Exception caught in callback" + e.message);      
						if (aErrorFunction) { aErrorFunction(jqXHR); };
					}
				}
			}else{                
				debug.log("Request has no data: " + thisObj.iUrl);       
				if (aErrorFunction) { aErrorFunction(jqXHR); };
			}
		},
        error: function (jqXHR, textStatus, errorThrown){     
			debug.log("Request exception: " + thisObj.iUrl);       
			if (aErrorFunction) { aErrorFunction(jqXHR); };
		},
        type: "POST",
        data: this.iEnvelope,
        contentType: "text/xml",
        dataType:"xml",        
		headers: {"SOAPAction": this.GetSoapAction()}
    });    
};
	
SoapRequest.prototype.getElementsByTagNameNS = function(tagName, ns, scope){
	var elementListForReturn = scope.getElementsByTagName(ns+":"+tagName);
	if(elementListForReturn.length == 0){
		elementListForReturn = scope.getElementsByTagName(tagName);
		if(elementListForReturn.length == 0 && document.getElementsByTagNameNS){
			elementListForReturn = scope.getElementsByTagNameNS(ns, tagName);
		}
	}     
	return elementListForReturn;
}

SoapRequest.prototype.ReadIntParameter = function(aValue) {
	return aValue * 1;
}

SoapRequest.prototype.ReadBoolParameter = function(aValue) {
	return (aValue == "1") ? true : false;
}

SoapRequest.prototype.ReadStringParameter = function(aValue) {
	return aValue;
}

SoapRequest.prototype.ReadBinaryParameter = function(aValue) {
	return aValue;
}

SoapRequest.prototype.WriteIntParameter = function(aTagName, aValue) {
	this.WriteParameter(aTagName, "" + (aValue?aValue:"0"));
}

SoapRequest.prototype.WriteBoolParameter = function(aTagName, aValue) {
	this.WriteParameter(aTagName, aValue ? "1" : "0");
}

SoapRequest.prototype.WriteStringParameter = function(aTagName, aValue) {
	this.WriteParameter(aTagName, (aValue?aValue:""));
}

SoapRequest.prototype.WriteBinaryParameter = function(aTagName, aValue) {
	this.WriteParameter(aTagName, (aValue?aValue:""));
}

SoapRequest.prototype.WriteParameter = function(aTagName, aValue) {
	this.iEnvelope += "<" + aTagName + ">" + aValue.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;") + "</" + aTagName + ">";
}

SoapRequest.prototype.GetTransportErrorMessage = function(transport) {
	var errorString = "Error:";
	try{
		errorString += "\nstatus: " + transport.statusText;
	}catch(e){}
	try{
        if (transport && transport.responseXML){
            if (transport.responseXML.getElementsByTagName("faultcode").length){				
                errorString += "\nfaultcode: " + transport.responseXML.getElementsByTagName("faultcode")[0].childNodes[0].nodeValue;
            }
            if (transport.responseXML.getElementsByTagName("faultstring").length){				
                errorString += "\nfaultstring: " + transport.responseXML.getElementsByTagName("faultstring")[0].childNodes[0].nodeValue;
            }
            if (transport.responseXML.getElementsByTagName("errorCode").length){				
                errorString += "\nerrorCode: " + transport.responseXML.getElementsByTagName("errorCode")[0].childNodes[0].nodeValue;
            }
            if (transport.responseXML.getElementsByTagName("errorDescription").length){				
                errorString += "\nerrorDescription: " + transport.responseXML.getElementsByTagName("errorDescription")[0].childNodes[0].nodeValue;
            }
        }
	}catch(e){
        // IE dies on attempting to access transport.responseXML
        errorString = "An unknown error occurred in request.";
    }
	return errorString;
}

SoapRequest.prototype.Send = function(onSuccess, onError) {
	var thisObj = this;
	this.WriteEnvelopeEnd();
	return this.CreateAjaxRequest(
		function(transport){
			if(transport.responseXML.getElementsByTagName("faultcode").length > 0){
				onError(thisObj.GetTransportErrorMessage(transport), transport);
			}else{
				var outParameters = thisObj.getElementsByTagNameNS(thisObj.iAction + "Response", "u", transport.responseXML)[0].childNodes;
				var result = {};
				for(var i=0;i<outParameters.length;i++){
					var nodeValue = "";
					var childNodes = outParameters[i].childNodes;
					for (var j=0;j<childNodes.length;j++){
						nodeValue += childNodes[j].nodeValue;
					}
					result[outParameters[i].nodeName.replace(/.*:/, "")] = (nodeValue!=""?nodeValue:null);
				}
				onSuccess(result);
			}				
		},
		function(transport){
			onError(thisObj.GetTransportErrorMessage(transport), transport);
		}
	);
}