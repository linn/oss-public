/* base class for widgets */
(function($) {

    var kCallbackTimeout = window["AjaxTimeout"]?window["AjaxTimeout"]:10000;

    $.Service = function(serviceName, callbackOperationName, callbackTimerFrequency, callbackOperationDataFunction){
        this.serviceName = serviceName;
        
        this.callbackOperationName = callbackOperationName;
        this.callbackTimerFrequency = callbackTimerFrequency?callbackTimerFrequency:1000;
        var thisObj = this;
        this.callbackOperationDataFunction = callbackOperationDataFunction?callbackOperationDataFunction:function(widget) { return {aWidgetID:thisObj.serviceID}; };
        
        this.serviceID = null;
        this.opened = false;
    }


    $.KinskyWidget = function() {}
    $.KinskyWidget.prototype.init = function(domElement, container, imageFolder) {
        this.services = {};
        this.domElement = domElement;
        this.container = container;
        this.servicesOpened = false;     
        this.servicesClosing = false;    
        this.servicesOpening = false; 
        this.imageFolder = imageFolder;  
        this.rendered = false;
    }
    
    $.KinskyWidget._dragTargetsRegistry = {};
    
    $.KinskyWidget.prototype.openServices = function(callback, failureCallback) {
        var thisObj = this;
        this.servicesOpened = this.services.length == 0;
        this.servicesOpening = this.services.length > 0;
        // ensure services have not been closed in the interim
        if (!this.servicesClosing){
            $.each(this.services, function(i, svc){
                $.JSONRequest.sendRequest(svc.serviceName,
                                      "Open",
                                      thisObj.getServiceOpenRequest(svc),
                                      function(responseData) { thisObj.onServiceOpened(svc, responseData, callback); },
                                      function(){thisObj.onServiceOpenFailed(svc, failureCallback);});
            });
        }
    }
    $.KinskyWidget.prototype.getServiceOpenRequest = function(svc) {
        return {aContainerID:this.container.containerID};
    }
    
    $.KinskyWidget.prototype.onServiceOpenFailed = function(service, failureCallback) {        
        debug.log("Widget service " + service.serviceName + " has failed to open.");        
        this.servicesOpening = false;
        if (failureCallback) {
            failureCallback();
        }
    }
    
    $.KinskyWidget.prototype.onServiceOpened = function(service, responseData, callback) {
        debug.log("Widget service " + service.serviceName + " has opened.");
        service.opened = true;
        service.serviceID = responseData;        
        
        if (service.callbackOperationName) {
            this.startCallbackTimer(service);
        }   
        if (this.container){
			this.container.mapServiceID(service.serviceID, {service:service,widget:this});
		}
		this.domElement.trigger("evtServiceOpened", this, service);
        var allServicesOpen = true;
        $.each(this.services, function(){
            allServicesOpen &= this.opened;
            if (!allServicesOpen) return false;
        });
        if (allServicesOpen){          
                this.servicesOpening = false;
				this.servicesOpened = true;
                if (callback){
					callback();
				}   
				this.domElement.trigger("evtServicesOpened", this); 
        }
    };
    
    
    $.KinskyWidget.prototype.render = function() {
        debug.log("Widget " + this.domElement.attr("id") + " has rendered.");
        this.rendered = true;
	    this.domElement.trigger("evtRendered", this);
    } 
        
    $.KinskyWidget.prototype.closeServices = function(callback) {
            debug.log("Widget " + this.domElement.attr("id") + " is closing.");
            this.servicesClosing = true;
            this.servicesOpened = false;
            var thisObj = this;
            $.each(this.services, function(i, svc){
                if (svc.opened){         
                    debug.log("Widget service " + svc.serviceName + " is closing.");
                    if (svc.callbackOperationName) {
                        thisObj.stopCallbackTimer(svc);
                    }
                    $.JSONRequest.sendRequest(svc.serviceName,
                                          "Close",
                                          thisObj.getServiceCloseRequest(svc),
                                          function(responseData) { thisObj.onServiceClosed(svc, responseData, callback); });
                }else{
                    thisObj.onServiceClosed(svc, null, callback);
                }
            });
    };
    
    $.KinskyWidget.prototype.getServiceCloseRequest = function(svc) {
        return {aWidgetID:svc.serviceID};
    }
    
    $.KinskyWidget.prototype.onServiceClosed = function(service, responseData, callback) {
        debug.log("Widget service " + service.serviceName + " has closed.");
        service.opened= false;
	    this.domElement.trigger("evtServiceClosed", this, service);
	    var allServicesClosed = true;
        $.each(this.services, function(){
            allServicesClosed &= !this.opened;
            if (!allServicesClosed) return false;
        });
        if (allServicesClosed){                  
                debug.log("Widget " + this.domElement.attr("id") + " has closed.");
                this.servicesClosing = false;
                this.servicesOpened = false;
				if (callback){
					callback();
				}   
				this.domElement.trigger("evtServicesClosed", this); 
        }
    };
    
    $.KinskyWidget.prototype.startCallbackTimer = function(service) {
        var thisObj = this;
        if (service.callbackOperationName) {
            this.stopCallbackTimer(service);
            // new timer
            service.isCallingBack = false;
            var lastCallbackTime;
            this.domElement.everyTime(service.callbackTimerFrequency, service.serviceName, function() { 
				 if (!service.isCallingBack){
					 service.isCallingBack = true;	
					 lastCallbackTime = new Date();				
					 $.JSONRequest.sendRequest(service.serviceName, service.callbackOperationName, service.callbackOperationDataFunction(thisObj), function(responseData) {            
						service.isCallingBack = false;
						thisObj.onServiceCallback(service, responseData);
					}, function(responseData) {            
						service.isCallingBack = false;
						thisObj.onServiceCallbackError(service, responseData);
					});
				 }else{
				    // bug on htc browser - connections fail to time out, need to add a timeout to callbacks in case they get dropped
				    var difference = new Date() - lastCallbackTime;
				    if (difference > kCallbackTimeout){
                        debug.log("Widget service " + service.serviceName + " has timed out whilst calling back.");
                        service.isCallingBack = false;		 
				    }
				 }
            });
        }
    }
    $.KinskyWidget.prototype.onServiceCallback = function(service, responseData) {
    }
    $.KinskyWidget.prototype.onServiceCallbackError = function(service, responseData) {
    }    
    
    $.KinskyWidget.prototype.stopCallbackTimer = function(service) {
        // just in case we have a timer already running
        this.domElement.stopTime(service.serviceName);
    }
    
    $.KinskyWidget.prototype.onServiceStateChanged = function(service) {    
        debug.log("Widget service " + service.serviceName + " has changed state.");
    }
    

    $.KinskyWidget.prototype.destroyDraggable = function(element){
        element.sortable("destroy");
        return element;
    }
    
    $.KinskyWidget.prototype.createDraggable = function(element, dropTargetSelector, draggableItemsSelector, dragSource){
            element.sortable({ 
					opacity: 0.6, 
					cursor: 'move', 
					connectWith: dropTargetSelector,
					delay: 200,
					start: function (event, ui) { 
                        $.KinskyWidget._dragItems = ui.helper;
                        $.KinskyWidget._dragContainer = $(this);
                        ui.item.show();
                        ui.helper.addClass('dragging');
                    },
                    sort: function (event,ui) {
						$.KinskyWidget._dropContainer = ui.placeholder.parent();
						$.KinskyWidget._dropIndex = ui.placeholder.prevAll().length;
                    },
					update: function(event, ui) {
					    var sender = ui.sender;
					    if (!sender) {
					        sender = $(this);
					    }
					    sender.sortable("cancel");
				    },
				    stop: function(event, ui){
						var evtArgs = [$.KinskyWidget._dragContainer, $.KinskyWidget._dropContainer, $.KinskyWidget._dragItems, $.KinskyWidget._dropIndex, dragSource];
						$.KinskyWidget._dragContainer.trigger("evtItemsDragged", evtArgs);
						$.KinskyWidget._dropContainer.trigger("evtItemsDropped", evtArgs);
						var sourceElem = ui.item;
						setTimeout(function(){
						    sourceElem.removeClass('dragging');
						},100);  
				    },
					
					containment:'window',
					dropOnEmpty:'true'
			    });       
			    return element;
    }

})(jQuery);