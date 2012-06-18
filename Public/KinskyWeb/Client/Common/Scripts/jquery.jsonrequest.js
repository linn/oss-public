Ajax.Request.prototype.abort = function() {
    // prevent and state change callbacks from being issued
    this.transport.onreadystatechange = Prototype.emptyFunction;
    // abort the XHR
    this.transport.abort();
    // update the request counter
    Ajax.activeRequestCount--;
};

// Register global responders that will occur on all AJAX requests

    Ajax.Responders.register({
        onCreate: function(request) {
            request['timeoutId'] = window.setTimeout(
                function() {
                    if (request.options['onSuccess']){
                        // cancel success function
                        request.options['onSuccess'] = function(){};
                    }
                    debug.log("Request to " + request.url + " has timed out.");
                    request.transport.abort();
                    // Run the onFailure method if we set one up when creating the AJAX object
                    if (request.options['onFailure']) {
                        request.options['onFailure'](request.transport, request.json);
                    }
                },
                window["AjaxTimeout"]?window["AjaxTimeout"]:10000
            );
        },
        onComplete: function(request) {
            // Clear the timeout, the request completed ok
            window.clearTimeout(request['timeoutId']);
        }
    });


jQuery.JSONRequest = {
    sendRequest: function(serviceName, methodName, requestData, callback, errorFunction) {
            if (serviceName != "WidgetContainer" && methodName != "UpdateState"){
                debug.log("Send request to " + serviceName + "." + methodName); 
            }
        	return new Ajax.Request(//TODO: when mono supports wcf add the following: "/Services/" + 
        	"/" + serviceName + "/Json/" + methodName , {
			method: 'post', 
			postBody: Object.toJSON(requestData), 
			onSuccess: function(transport) {			
			    if (transport.status){
				    var result = transport.responseJSON;
                    if (callback) { 
                        try { 
                            if (result){
                                callback(result[methodName + "Result"]); 
                            }else{
                                callback(null);
                            }
                        }catch (e) {
	                        debug.log("Exception caught in callback eval." + e); 
                        } 
                    }
                }else{                
                    debug.log("Request has no transport status: " + serviceName + "." + methodName);       
                    if (errorFunction) { errorFunction(transport); };
                }
            },
            onFailure : function (transport){     
                debug.log("Request failed: " + serviceName + "." + methodName);       
                if (errorFunction) { errorFunction(transport); };
            },
            onException : function (transport){     
                debug.log("Request exception: " + serviceName + "." + methodName);       
                if (errorFunction) { errorFunction(transport); };
            },
            contentType: "application/json"        }); 
    }
};