(function($) {

    var kPleaseSelectMessage = "--- Please Select Source ---";

    $.SourceSelector = function(domElement, container) {   
        if (domElement){
            this.init(domElement, container);
        }
    }
    $.SourceSelector.prototype = new $.KinskyWidget;
    $.SourceSelector.prototype.constructor = $.SourceSelector;
    $.SourceSelector.superclass = $.KinskyWidget.prototype;
    
    $.SourceSelector.ESourceType = {
        eUnknown : 0,
        ePlaylist : 1,
        eRadio : 2,
        eUpnpAv : 3,
        eAnalog : 4,
        eSpdif : 5,
        eDisc : 6,
        eTuner : 7,
        eAux : 8,
        eToslink : 9
    }

    $.SourceSelector.prototype.init = function(domElement, container) {
        $.SourceSelector.superclass.init.call(this, domElement, container);
        this.services["sourceList"] = new $.Service("ViewSourceList");
        this.services["viewRoom"] = new $.Service("ViewRoom");
        this.services["controllerRoom"] = new $.Service("ControllerRoom");
        this.room = null;
        this.source = null;
        this.sourceType = null;
    }

    $.SourceSelector.prototype.getServiceOpenRequest = function(svc) {
        return { aContainerID: this.container.containerID, aRoom:this.room };
    }

    $.SourceSelector.prototype.render = function() {
        //this.domElement.append('<label>Sources:</label>');
        var sourceList = $('<select id="SourceSelectorSourceList" class="ui-state-default"/>');
        sourceList.addOption(kPleaseSelectMessage,kPleaseSelectMessage,true);
        var thisObj = this;
        
        this.change = function(){
			if ($(this).selectedValues().length && $(this).selectedValues()[0] != kPleaseSelectMessage){
		        $(this).removeOption(kPleaseSelectMessage);
		        thisObj.changeSource($(this).selectedValues()[0]);
			}else{
			    thisObj.source = null;
		    }
        };
        sourceList.bind("change", this.change);
        this.domElement.append(sourceList);    
	    $.SourceSelector.superclass.render.call(this);        
    }
    
    $.SourceSelector.prototype.onServiceStateChanged = function(service) {
        $.SourceSelector.superclass.onServiceStateChanged.call(this,service);
        var thisObj = this;
        if (service == this.services.sourceList){
            this.getSourceList();
        }else if (service == this.services.viewRoom){
            this.getCurrentSource();
        }
    }
    
    $.SourceSelector.prototype.getCurrentSource = function(){  
        if (this.servicesOpened){
            var thisObj = this; 
            var requestData = {aWidgetID:this.services.viewRoom.serviceID};
            $.JSONRequest.sendRequest(this.services.viewRoom.serviceName,
                                      "CurrentSource",
                                      requestData,
                                      function(responseData) {
                                          thisObj.onGetCurrentSource(responseData);
                                      });
        }
    }
    $.SourceSelector.prototype.onGetCurrentSource = function(responseData){
        if (this.servicesOpened){
            var sourceList = this.domElement.find("#SourceSelectorSourceList");
            if (responseData.Name != this.source){
                this.source = responseData.Name;       
                this.sourceType = responseData.Type;       
                this.domElement.trigger("evtSourceSelectorSourceChanged", this);             
            }
                //remove change handler
		        sourceList.unbind("change", this.change);    
    		        
                //select current source
                if (responseData.Name){
		            sourceList.removeOption(kPleaseSelectMessage);
                    sourceList.selectOptions(responseData.Name);            
                }else{                
                    sourceList.selectOptions(kPleaseSelectMessage);
                }
                
                //re-add change handler
                sourceList.bind("change", this.change);   
        }
    }
    
    
    $.SourceSelector.prototype.getSourceList = function(){
        if (this.servicesOpened){
            var thisObj = this;
            var requestData = {aWidgetID:this.services.sourceList.serviceID};
            $.JSONRequest.sendRequest(this.services.sourceList.serviceName,
                                      "Sources",
                                      requestData,
                                      function(responseData) {
                                          thisObj.onGetSourceList(responseData);
                                      });
        }
    }
    
    $.SourceSelector.prototype.onGetSourceList = function(responseData){
        if (this.servicesOpened){
            var sourceList = this.domElement.find("#SourceSelectorSourceList");
            
            //remove change handler
		    sourceList.unbind("change", this.change);
    		
		    // get current selection
            var currentSelected = this.source;
            
            //clear existing values
            sourceList.removeOption(/./);
            
            //add new values
            var found = false;
		    $.each(responseData, function(i, value){
			    if (value==currentSelected){
			        found = true;
			    }
		    });
            if (!found || !currentSelected){
	            sourceList.addOption(kPleaseSelectMessage,kPleaseSelectMessage,true);
		        this.source = null;
	        }
		    $.each(responseData, function(i, value){
			    sourceList.addOption(value.Name,value.Name,(value.Name==currentSelected));
		    });

            //re-add change handler
            sourceList.bind("change", this.change);   
            
            // get the source
            this.getCurrentSource();
        }
    }
    
    $.SourceSelector.prototype.changeSource = function(source){
        var thisObj = this;
		var requestData = {aWidgetID:this.services.controllerRoom.serviceID, aSource:source};
        $.JSONRequest.sendRequest(this.services.controllerRoom.serviceName,
            "SelectSource",
            requestData);
    }
    
    $.SourceSelector.prototype.closeServices = function(callback) {
        this.source = null;
        this.room = null;
        var sourceList = this.domElement.find("#SourceSelectorSourceList");
        sourceList.removeOption(/./);
	    sourceList.addOption(kPleaseSelectMessage,kPleaseSelectMessage,true);
        $.SourceSelector.superclass.closeServices.call(this, callback);    
    }
    
    $.SourceSelector.prototype.openServices = function(room, callback) {
        this.room = room;
        this.source = null;
		if (!this.room){
            var sourceList = this.domElement.find("#SourceSelectorSourceList");
            sourceList.removeOption(/./);
	        sourceList.addOption(kPleaseSelectMessage,kPleaseSelectMessage,true);
        }else{
            $.SourceSelector.superclass.openServices.call(this, callback);
        }
    }

    $.SourceSelector.prototype.standby = function(){  
        if (this.servicesOpened){
            this.room = null;
            this.source = null;
            this.sourceType = null;
            var requestData = {aWidgetID:this.services.controllerRoom.serviceID};
            $.JSONRequest.sendRequest(this.services.controllerRoom.serviceName,
                                      "Standby",
                                      requestData);
        }
    }
    
})(jQuery);