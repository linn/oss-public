/* media time widget */
(function($) {
    
    
    $.MediaTime = function(domElement, container, useImageMap, imageCoords, useMouseDownTimers, imageFolder, defaultIncrement, shiftKeyIncrement, ctrlKeyIncrement) {
        if (domElement){
            this.init(domElement, container, useImageMap, imageCoords, useMouseDownTimers, imageFolder, defaultIncrement, shiftKeyIncrement, ctrlKeyIncrement);
        }
    }
    $.MediaTime.prototype = new $.KinskyWidget;
    $.MediaTime.prototype.constructor = $.MediaTime;
    $.MediaTime.superclass = $.KinskyWidget.prototype;

    $.MediaTime.ETransportState = {
        eBuffering:0,
        ePlaying:1,
        ePaused:2,
        eStopped:3,
        eUnknown:4
    }

    $.MediaTime.prototype.init = function(domElement, container, useImageMap, imageCoords, useMouseDownTimers, imageFolder, defaultIncrement, shiftKeyIncrement, ctrlKeyIncrement) {
        $.MediaTime.superclass.init.call(this, domElement, container, imageFolder);
        var thisObj = this;
        this.services["viewWidget"] = new $.Service("ViewMediaTime","GetState",10000, function(){ 
                    return {aWidgetID:thisObj.services.viewWidget.serviceID};
                });
        this.services["controllerWidget"] = new $.Service("ControllerMediaTime");
        this.room = null;
        this.source = null;
        this.transportState = null;
        this.duration = 0;
        this.seconds = 0;         
        this.seeking = false;        
        this.imageCoords = imageCoords;
        this.useImageMap = useImageMap;
        this.connected = false;
        this.useMouseDownTimers = useMouseDownTimers;
        this.defaultIncrement = defaultIncrement?defaultIncrement:1;
        this.shiftKeyIncrement = shiftKeyIncrement?shiftKeyIncrement:5;
        this.ctrlKeyIncrement = ctrlKeyIncrement?ctrlKeyIncrement:10;
    }
    
    $.MediaTime.prototype.onServiceCallback = function(service, responseData) {
        $.MediaTime.superclass.onServiceCallback.call(this, service, responseData);
        this.onStateChangeResponse(responseData);
    }

    $.MediaTime.prototype.render = function() {   
            
         var thisObj = this;
        
        var mediaTimeContainer = $('<div id="MediaTimeContainer"/>');
        this.domElement.append(mediaTimeContainer);
        var secondsBackground = $('<img id="MediaTimeSecondsBackground" src="' + this.imageFolder + 'Wheel.png" />');
        mediaTimeContainer.append(secondsBackground);
        var skipBackButton = $('<img id="MediaTimeButtonSkipBack" ' + (this.useImageMap?'usemap="#MediaTimeButtonSkipBackMap"':'') + ' src="' + this.imageFolder + 'WheelSkipBack.png" />');
        mediaTimeContainer.append(skipBackButton);
        var skipForwardButton = $('<img id="MediaTimeButtonSkipForward" ' + (this.useImageMap?'usemap="#MediaTimeButtonSkipForwardMap"':'') + ' src="' + this.imageFolder + 'WheelSkipForward.png" />');
        mediaTimeContainer.append(skipForwardButton);
        var secondsDisplay = $('<div id="MediaTimeSeconds"><span/></div>');
        mediaTimeContainer.append(secondsDisplay);
           
        var skipBackSelector, skipForwardSelector;
        if (this.useImageMap){                         
            var skipBackMap = $('<map name="MediaTimeButtonSkipBackMap">' +
                                            '<area shape="poly" coords="' + this.imageCoords.backButton + '" />' + 
                                            '</map>');
            mediaTimeContainer.append(skipBackMap); 
            
            var skipForwardMap = $('<map name="MediaTimeButtonSkipForwardMap">' +
                                            '<area shape="poly" coords="' + this.imageCoords.forwardButton + '" />' + 
                                            '</map>');
            mediaTimeContainer.append(skipForwardMap);    
            
            skipBackSelector = skipBackMap
            skipForwardSelector = skipForwardMap;       
        }else{
        
            skipBackSelector = skipBackButton
            skipForwardSelector = skipForwardButton;   
        }
       
        skipBackSelector.hover(function(evt){  
            skipBackButton.addClass("Mouse");
            thisObj.evaluateButtonState();            
        },function (evt) {
            thisObj.domElement.stopTime("BackwardsSeek");
            thisObj.seeking = false;   
            skipBackButton.removeClass("Mouse");
            skipBackButton.removeClass("Touch");
            thisObj.evaluateButtonState();
        })
        
        if (this.useMouseDownTimers){
            skipBackSelector.mousedown(function(evt){              
                thisObj.seeking = true;           
                thisObj.domElement.everyTime(100, "BackwardsSeek", function(){
                    var increment = thisObj.defaultIncrement;
                    if (evt.ctrlKey){
                        increment = thisObj.ctrlKeyIncrement;
                    }else if (evt.shiftKey){
                        increment = thisObj.shiftKeyIncrement;
                    }
                    var value = thisObj.seconds - increment;
                    if (value < 0 ){
                        value = 0;
                    }                
                    thisObj.setSeconds(value);
                    thisObj.evaluateButtonState();
                });      
                skipBackButton.addClass("Touch");
                thisObj.evaluateButtonState();   
            }).mouseup(function(evt){          
                thisObj.domElement.stopTime("BackwardsSeek");
                thisObj.seeking = false;   
                skipBackButton.removeClass("Touch");
                thisObj.evaluateButtonState();
            });
        }else{
                skipBackSelector.click(function(evt){
                    var increment = thisObj.defaultIncrement;
                    if (evt.ctrlKey){
                        increment = thisObj.ctrlKeyIncrement;
                    }else if (evt.shiftKey){
                        increment = thisObj.shiftKeyIncrement;
                    }
                    var value = thisObj.seconds - increment;
                    if (value < 0 ){
                        value = 0;
                    }                
                    thisObj.setSeconds(value);
                    thisObj.evaluateButtonState();
                });
        }
        
          
        
        skipForwardSelector.hover(function(evt){  
            skipForwardButton.addClass("Mouse");
            thisObj.evaluateButtonState();            
        },function (evt) {
            thisObj.domElement.stopTime("ForwardsSeek");
            thisObj.seeking = false;   
            skipForwardButton.removeClass("Mouse");
            skipForwardButton.removeClass("Touch");
            thisObj.evaluateButtonState();
        })
            
        if (this.useMouseDownTimers){
            skipForwardSelector.mousedown(function(evt){              
            thisObj.seeking = true;           
            thisObj.domElement.everyTime(100, "ForwardsSeek", function(){
                    var increment = thisObj.defaultIncrement;
                    if (evt.ctrlKey){
                        increment = thisObj.ctrlKeyIncrement;
                    }else if (evt.shiftKey){
                        increment = thisObj.shiftKeyIncrement;
                    }
                    var value = thisObj.seconds + increment;
                    if (value > thisObj.duration ){
                        value = thisObj.duration;
                    }                
                    thisObj.setSeconds(value);
                    thisObj.evaluateButtonState();
                
                });         
                skipForwardButton.addClass("Touch");
                thisObj.evaluateButtonState();
            }).mouseup(function(evt){          
                thisObj.domElement.stopTime("ForwardsSeek");
                thisObj.seeking = false;   
                skipForwardButton.removeClass("Touch");
                thisObj.evaluateButtonState();
            });
        }else{
            skipForwardSelector.click(function(evt){    
                    var increment = thisObj.defaultIncrement;
                    if (evt.ctrlKey){
                        increment = thisObj.ctrlKeyIncrement;
                    }else if (evt.shiftKey){
                        increment = thisObj.shiftKeyIncrement;
                    }
                    var value = thisObj.seconds + increment;
                    if (value > thisObj.duration ){
                        value = thisObj.duration;
                    }                
                    thisObj.setSeconds(value);
                    thisObj.evaluateButtonState();
            });
        }
        $.MediaTime.superclass.render.call(this);
    }
    
    
    $.MediaTime.prototype.evaluateButtonState = function(){
        var backButton = this.domElement.find("#MediaTimeButtonSkipBack");
        var forwardButton = this.domElement.find("#MediaTimeButtonSkipForward");
        var buttonModifier = "";
        if (this.connected){
            if (backButton.hasClass("Touch") || forwardButton.hasClass("Touch")){
                buttonModifier = "Touch";
            }else if (backButton.hasClass("Mouse") || forwardButton.hasClass("Mouse")){
                buttonModifier = "Mouse";
            }
        }
        backButton.attr("src", this.imageFolder + "WheelSkipBack" + buttonModifier + ".png");
        forwardButton.attr("src", this.imageFolder + "WheelSkipForward" + buttonModifier + ".png");        
        var secondsDisplay = this.domElement.find("#MediaTimeSeconds");
        secondsDisplay.text(this.connected?this.getMediaTime(this.seconds):"");
    }
    
    $.MediaTime.prototype.getServiceOpenRequest = function(svc) {
        return { aContainerID: this.container.containerID, aRoom: this.room, aSource:this.source };
    }
    
    $.MediaTime.prototype.onServiceStateChanged = function(service) {
        $.MediaTime.superclass.onServiceStateChanged.call(this, service);
        if (service == this.services.viewWidget){
            var thisObj = this;
            var requestData = {aWidgetID:this.services.viewWidget.serviceID};
            $.JSONRequest.sendRequest(this.services.viewWidget.serviceName,
                                      "GetState",
                                      requestData,
                                      function(responseData) {
                                          thisObj.onStateChangeResponse(responseData);
                                      },function(responseData) {
                                            debug.log(" MediaTime: error in get state call.");
                                      });
        }
    }
    
    $.MediaTime.prototype.onStateChangeResponse = function(responseData){
            this.connected = responseData.Connected;
            if (!this.seeking && responseData && (responseData.TransportState != this.transportState ||  
                responseData.Duration != this.duration ||  
                responseData.Seconds != this.seconds)) {
                this.transportState = responseData.TransportState;
                this.duration = responseData.Duration;
                this.seconds = responseData.Seconds;
                this.evaluateButtonState();
            }
    }
    
    $.MediaTime.prototype.setSeconds = function(value){    
           this.seconds = value;
           var requestData = { aWidgetID: this.services.controllerWidget.serviceID, aSeconds: this.seconds };
             $.JSONRequest.sendRequest(this.services.controllerWidget.serviceName,
                                      "SetSeconds",
                                      requestData);                                      
    }

    $.MediaTime.prototype.closeServices = function(callback){
        this.domElement.stopTime("MediaTimerTick");
        this.duration = 0;
        this.seconds = 0;
        this.transportState = null;
        this.evaluateButtonState();
        $.MediaTime.superclass.closeServices.call(this, callback);
    }
    
    $.MediaTime.prototype.openServices = function(room, source, callback) {
        this.room = room;
        this.source = source;
            var thisObj = this;
            var timerFunction = function() {            
                if (thisObj.transportState == $.MediaTime.ETransportState.ePlaying && 
                    (thisObj.duration == 0 || thisObj.duration >= thisObj.seconds) && 
                    !thisObj.seeking){
                    thisObj.seconds += 1;
                    thisObj.evaluateButtonState();
		        }
            }        
            this.domElement.stopTime("MediaTimerTick");
            this.domElement.everyTime(1000, "MediaTimerTick", timerFunction);
            $.MediaTime.superclass.openServices.call(this, callback);
    }
    
    $.MediaTime.prototype.getMediaTime = function(seconds) {        
        var mins = Math.floor(seconds / 60) + "";
        if (mins.length == 1) {
            mins = "0" + mins;
        }
        var secs = Math.floor(seconds % 60) + "";
        if (secs.length == 1) {
            secs = "0" + secs;
        }
        return mins + ":" + secs;
    }
    
})(jQuery);