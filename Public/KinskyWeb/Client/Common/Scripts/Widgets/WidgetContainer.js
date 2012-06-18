(function($) {
    $.KinskyWidgetContainer = function(domElement, expiryTimeout, failedCallbackAttemptsLimit) {    
        if (domElement){
            this.init(domElement, expiryTimeout, failedCallbackAttemptsLimit);
        }
    }
    $.KinskyWidgetContainer.prototype = new $.KinskyWidget;
    $.KinskyWidgetContainer.prototype.constructor = $.KinskyWidgetContainer;
    $.KinskyWidgetContainer.superclass = $.KinskyWidget.prototype;


    $.KinskyWidgetContainer.prototype.init = function(domElement, expiryTimeout, failedCallbackAttemptsLimit) {
        $.KinskyWidgetContainer.superclass.init.call(this, domElement);
        var thisObj = this;
        this.services["container"] = new $.Service("WidgetContainer","UpdateState",500, function(){ return {aContainerID:thisObj.containerID};})
        this.widgets = {};
        this.mappedServiceIDs = {};
        this.containerID = null;
        this.serverClosed = false;
        this.reopeningContainer = false;
        this.expiryTimeout = expiryTimeout;
        this.failedCallbackAttemptsLimit = failedCallbackAttemptsLimit;
        if (!this.failedCallbackAttemptsLimit){
            this.failedCallbackAttemptsLimit = 1;
        }
        this.failedCallbackAttempts = 0;
    }

    $.KinskyWidgetContainer.prototype.add = function(widget, name) {
		this.widgets[name] = widget;
    }

    $.KinskyWidgetContainer.prototype.remove = function(name) {
        var widget = this.widgets[name];
        if (widget){
		    delete this.widgets[name];
		    var thisObj = this;
		    $.each(this.mappedServiceIDs, function(i, mappedService){
		        if (mappedService.widget == widget){
		            delete thisObj.mappedServiceIDs[mappedService.service];
		        } 
		    });
		}
    }
    
    $.KinskyWidgetContainer.prototype.onServiceCallbackError = function(service, responseData) {  
        this.domElement.trigger("evtServerDown");
    }
    $.KinskyWidgetContainer.prototype.onServiceCallback = function(service, responseData) {
        var thisObj = this;
        this.failedCallbackAttempts = 0;
        $.KinskyWidgetContainer.superclass.onServiceCallback.call(this, service, responseData);
        var timedOut = (!responseData || responseData.TimedOut);
        if (timedOut){
            if (!responseData){
                debug.log("Server is down.");
                this.onServiceCallbackError(service, responseData);
            }else{
                debug.log("Container has timed out on server");   
                if (!this.serverClosed){                 
                    this.stopCallbackTimer(service);
                    this.domElement.trigger("evtServerClosed");
                }
            } 
        }else{
            this.domElement.trigger("evtServerUp");
            var updatedServices = responseData.UpdatedWidgets;
            setTimeout(function(){thisObj.updateServices(updatedServices);},0);            
        }
    }
    
    $.KinskyWidgetContainer.prototype.updateServices = function(updatedServices) { 
            var thisObj = this;
            var widgetOpening = false;
            $.each(this.widgets, function(i, w){
                if (w.servicesOpening){
                    debug.log("widget" + w.domElement.attr("id") + " is still opening in update services")
                    widgetOpening = true;
                    return;
                }
            });        
            if (widgetOpening){
                setTimeout(function() {thisObj.updateServices(updatedServices);},100);
            }else{
                $.each(updatedServices, function(i, serviceID){
                    if (thisObj.mappedServiceIDs[serviceID]){
                        var updatedWidget = thisObj.mappedServiceIDs[serviceID].widget;
                        var updatedService = thisObj.mappedServiceIDs[serviceID].service;
                        if (updatedWidget && updatedService){
                            updatedWidget.onServiceStateChanged(updatedService);
                        }
                    }
                });
        }
     }
    
    $.KinskyWidgetContainer.prototype.getServiceOpenRequest = function(svc) { 
        return {aRequestedTimeoutMilliseconds:this.expiryTimeout};
    }
    
    $.KinskyWidgetContainer.prototype.getServiceCloseRequest = function(svc) {
        return {aContainerID:svc.serviceID};
    }
    
    $.KinskyWidgetContainer.prototype.onServiceOpenFailed = function(service, failureCallback) {                    
        if (!this.serverClosed){ 
            this.serverClosed = true;                    
            this.domElement.trigger("evtServerClosed");
        }
        $.KinskyWidgetContainer.superclass.onServiceOpenFailed.call(this, service, failureCallback);
    }
    
    $.KinskyWidgetContainer.prototype.onServiceOpened = function(service, responseData,callback) {
        this.failedCallbackAttempts = 0;
        this.containerID = responseData;
        if (responseData){
            this.serverClosed = false;
            this.domElement.trigger("evtServerUp");
        }
        $.KinskyWidgetContainer.superclass.onServiceOpened.call(this, service, responseData,callback);
    }
    
    $.KinskyWidgetContainer.prototype.mapServiceID = function(serviceID, map) {
        this.mappedServiceIDs[serviceID] = map;
    }
    
    $.KinskyWidgetContainer.prototype.closeServices = function() {        
        $.each(this.widgets, function(i, widget) {            
            widget.closeServices();  
        });
        $.KinskyWidgetContainer.superclass.closeServices.call(this);
    }
    

})(jQuery);
