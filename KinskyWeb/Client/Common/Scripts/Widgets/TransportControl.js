/* transport control widget */
(function($) {
    
    
    $.TransportControl = function(domElement, container, useImageMap, imageCoords, imageFolder) {    
        if (domElement){
            this.init(domElement, container, useImageMap, imageCoords, imageFolder);
        }
    }
    $.TransportControl.prototype = new $.KinskyWidget;
    $.TransportControl.prototype.constructor = $.TransportControl;
    $.TransportControl.superclass = $.KinskyWidget.prototype;

    $.TransportControl.ETransportState = {
        eBuffering:0,
        ePlaying:1,
        ePaused:2,
        eStopped:3,
        eUnknown:4
    }

    $.TransportControl.prototype.init = function(domElement, container, useImageMap, imageCoords, imageFolder) {
        $.TransportControl.superclass.init.call(this, domElement, container, imageFolder);
        this.services["viewWidget"] = new $.Service("ViewTransportControl"),
        this.services["controllerWidget"] = new $.Service("ControllerTransportControl")
        this.room = null;
        this.source = null;
        this.sourceType = null;
        this.transportState = null;
        this.connected = false;
        this.useImageMap = useImageMap;
        this.imageCoords = imageCoords;
    }
    
    

    $.TransportControl.prototype.render = function() {   
        var threeKArray = $('<div id="TransportControlThreeKArray" />');
        this.domElement.append(threeKArray);
        
                  
        var transportButton = $('<img id="TransportControlTransportButton" ' + (this.useImageMap?'usemap="#TransportControlTransportButtonMap"':'') + ' />');
        threeKArray.append(transportButton);
        
              
        var previousTrackButton = $('<img id="TransportControlPreviousButton" ' + (this.useImageMap?'usemap="#TransportControlPreviousButtonMap"':'') + ' />');
        threeKArray.append(previousTrackButton);
        
        
        var nextTrackButton = $('<img id="TransportControlNextButton" ' + (this.useImageMap?'usemap="#TransportControlNextButtonMap"':'') + ' />');
        threeKArray.append(nextTrackButton);
        
        var thisObj = this;
        
        var nextTrackSelector, previousTrackSelector, transportSelector;
        
        if (this.useImageMap){        
            var nextTrackButtonMap = $('<map name="TransportControlNextButtonMap"><area shape="circle" coords="' + this.imageCoords.nextButton + '" /></map>');
            threeKArray.append(nextTrackButtonMap);
            var previousTrackButtonMap = $('<map name="TransportControlPreviousButtonMap"><area shape="circle" coords="' + this.imageCoords.previousButton + '" /></map>');
            threeKArray.append(previousTrackButtonMap);
            var transportButtonMap = $('<map name="TransportControlTransportButtonMap"><area shape="circle" coords="' + this.imageCoords.transportButton + '" /></map>');
            threeKArray.append(transportButtonMap);
        
            nextTrackSelector = nextTrackButtonMap;
            previousTrackSelector = previousTrackButtonMap;
            transportSelector = transportButtonMap;
        }else{        
            nextTrackSelector = nextTrackButton;
            previousTrackSelector = previousTrackButton;
            transportSelector = transportButton;
        }
        
        var downEvent = "mousedown";
		var upEvent = "mouseup";
		if (hasTouchSupport){
			downEvent = "touchstart";
			upEvent = "touchend";
		}
        
        
        transportSelector.hover(function(){  
            transportButton.addClass("Mouse");
            thisObj.evaluateButtonState();            
        },function () {
            transportButton.removeClass("Mouse");
            transportButton.removeClass("Touch");
            thisObj.evaluateButtonState();
        }).bind(downEvent, function(){        
            transportButton.addClass("Touch");
            thisObj.evaluateButtonState();
        }).bind(upEvent,function(){      
            if (hasTouchSupport){  
                transportButton.removeClass("Mouse");
            }
            transportButton.removeClass("Touch");
            thisObj.evaluateButtonState();
        }).click(function(){
            thisObj.transportButtonClick();
        });
        
        
        previousTrackSelector.hover(function(){  
            previousTrackButton.addClass("Mouse");
            thisObj.evaluateButtonState();            
        },function () {
            previousTrackButton.removeClass("Mouse");
            previousTrackButton.removeClass("Touch");
            thisObj.evaluateButtonState();
        }).bind(downEvent,function(){           
            if (hasTouchSupport){  
                previousTrackButton.removeClass("Mouse");
            }  
            previousTrackButton.addClass("Touch");
            thisObj.evaluateButtonState();
        }).bind(upEvent,function(){        
            previousTrackButton.removeClass("Touch");
            thisObj.evaluateButtonState();
        }).click(function(){
            thisObj.previousTrack();
        });
        
        nextTrackSelector.hover(function(){  
            nextTrackButton.addClass("Mouse");
            thisObj.evaluateButtonState();            
        },function () {
            nextTrackButton.removeClass("Mouse");
            nextTrackButton.removeClass("Touch");
            thisObj.evaluateButtonState();
        }).bind(downEvent,function(){        
            nextTrackButton.addClass("Touch");
            thisObj.evaluateButtonState();
        }).bind(upEvent,function(){             
            if (hasTouchSupport){  
                nextTrackButton.removeClass("Mouse");
            }   
            nextTrackButton.removeClass("Touch");
            thisObj.evaluateButtonState();
        }).click(function(){
            thisObj.nextTrack();
        });
        
        this.evaluateButtonState();
        $.TransportControl.superclass.render.call(this);
    }
    
    $.TransportControl.prototype.sendTransportCommand = function(media, command, containerSourceID){
        var requestData = {aWidgetID:this.services.controllerWidget.serviceID, 
		                       aContainerSourceID: containerSourceID,
		                       aMedia: media};
            $.JSONRequest.sendRequest(this.services.controllerWidget.serviceName,
                                      command,
                                      requestData);
    }
    
    $.TransportControl.prototype.getServiceOpenRequest = function(svc) {
        return { aContainerID: this.container.containerID, aRoom: this.room, aSource:this.source };
    }
    
    $.TransportControl.prototype.onServiceStateChanged = function(service) {
        $.TransportControl.superclass.onServiceStateChanged.call(this, service);
        if (service == this.services.viewWidget){
            var thisObj = this;
            var requestData = {aWidgetID:this.services.viewWidget.serviceID};
            $.JSONRequest.sendRequest(this.services.viewWidget.serviceName,
                                      "TransportState",
                                      requestData,
                                      function(responseData) {
                                          thisObj.onStateChangeResponse(responseData);
                                      },function(responseData) {
                                            debug.log("TransportControl: Error in get transport state call.");
                                      });
        }
    }
    
    $.TransportControl.prototype.transportButtonClick = function(){
            var command = this.getTransportButtonCommand();
            if (this.servicesOpened && command){           
			    var thisObj = this;
			    var requestData = { aWidgetID: this.services.controllerWidget.serviceID };
			    $.JSONRequest.sendRequest(this.services.controllerWidget.serviceName,
									      command,
									      requestData);
			}
    }
    
    $.TransportControl.prototype.evaluateButtonState = function() {   
            var imageName = this.getTransportButtonCommand();
	        var button = this.domElement.find("#TransportControlTransportButton");
	        if (imageName && this.connected){
	            button.attr("src", this.imageFolder + imageName + (button.hasClass("Touch")?"Touch":(button.hasClass("Mouse")?"Mouse":"")) + ".png");
	            button.show();
	        }else{	        
	            button.hide();
	        }
	        var previous = this.domElement.find("#TransportControlPreviousButton");
	        var next = this.domElement.find("#TransportControlNextButton");
	        if (this.connected && this.room && this.source){	        
	            previous.attr("src", this.imageFolder + "SkipBack" + (previous.hasClass("Touch")?"Touch":(previous.hasClass("Mouse")?"Mouse":"")) + ".png");
	            next.attr("src", this.imageFolder + "SkipForward" + (next.hasClass("Touch")?"Touch":(next.hasClass("Mouse")?"Mouse":"")) + ".png");
	            previous.show();
	            next.show();	            
	        }else{
	            previous.hide();
	            next.hide();	            
	        }
    }
    
    $.TransportControl.prototype.getTransportButtonCommand = function() {
        var command
        if (this.sourceType == $.SourceSelector.ESourceType.ePlaylist 
            || this.sourceType == $.SourceSelector.ESourceType.eRadio 
            || this.sourceType == $.SourceSelector.ESourceType.eUpnpAv 
            || this.sourceType == $.SourceSelector.ESourceType.eDisc){
             
        
            if (this.transportState == $.TransportControl.ETransportState.ePlaying){
                command = (this.sourceType == $.SourceSelector.ESourceType.eRadio)?"Stop":"Pause";
            }
            if (this.transportState == $.TransportControl.ETransportState.ePaused
                || this.transportState == $.TransportControl.ETransportState.eStopped
                || this.transportState == $.TransportControl.ETransportState.eUnknown){
                command = "Play";
            }
        }
        return command;    
    }
            
    
    
    $.TransportControl.prototype.onStateChangeResponse = function(responseData){
        this.connected = responseData.Connected;
        this.transportState = responseData.TransportState;
        
            this.domElement.find("#TransportControlPreviousButton, #TransportControlNextButton, #TransportControlTransportButton")
                .removeClass("Mouse")
                .removeClass("Touch");
        this.evaluateButtonState();
    }
    
    
    $.TransportControl.prototype.previousTrack = function() {
        if (this.servicesOpened){
			var thisObj = this;
			var requestData = { aWidgetID: this.services.controllerWidget.serviceID };
			$.JSONRequest.sendRequest(this.services.controllerWidget.serviceName,
									  "Previous",
									  requestData);
        }
    }
    $.TransportControl.prototype.nextTrack = function() {
        if (this.servicesOpened){
			var thisObj = this;
			var requestData = { aWidgetID: this.services.controllerWidget.serviceID };
			$.JSONRequest.sendRequest(this.services.controllerWidget.serviceName,
									  "Next",
									  requestData);
        }
    }
    
    $.TransportControl.prototype.openServices = function(room, source, sourceType, callback) {
        this.room = room;
        this.source = source;
        this.sourceType = sourceType;
        $.TransportControl.superclass.openServices.call(this, callback);
    }

    $.TransportControl.prototype.closeServices = function(callback) {    
        this.room = null;
        this.source = null;
        this.sourceType = null;
        this.evaluateButtonState();
        $.TransportControl.superclass.closeServices.call(this, callback);     
    }
    
})(jQuery);