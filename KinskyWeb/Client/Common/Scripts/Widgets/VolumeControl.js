/* volume control widget */
(function($) {
    $.VolumeControl = function(domElement, container, useImageMap, imageCoords, imageFolder, defaultIncrement, shiftKeyIncrement, ctrlKeyIncrement) {        
        if (domElement){
            this.init(domElement, container, useImageMap, imageCoords, imageFolder, defaultIncrement, shiftKeyIncrement, ctrlKeyIncrement);
        }
    }
    $.VolumeControl.prototype = new $.KinskyWidget;
    $.VolumeControl.prototype.constructor = $.VolumeControl;
    $.VolumeControl.superclass = $.KinskyWidget.prototype;


    $.VolumeControl.prototype.init = function(domElement, container, useImageMap, imageCoords, imageFolder, defaultIncrement, shiftKeyIncrement, ctrlKeyIncrement) {
        $.VolumeControl.superclass.init.call(this, domElement, container, imageFolder);
        this.services["viewWidget"] = new $.Service("ViewVolumeControl"),
        this.services["controllerWidget"] = new $.Service("ControllerVolumeControl")
        this.room = null;
        this.volumeLimit = 0;
        this.volume = 0;
        this.mute = false;
        this.volumeChanging = false;
        this.connected = false;
        this.imageCoords = imageCoords;
        this.useImageMap = useImageMap;
        this.defaultIncrement = defaultIncrement?defaultIncrement:1;
        this.shiftKeyIncrement = shiftKeyIncrement?shiftKeyIncrement:5;
        this.ctrlKeyIncrement = ctrlKeyIncrement?ctrlKeyIncrement:10;
    }

    $.VolumeControl.prototype.getServiceOpenRequest = function(svc) {
        return { aContainerID: this.container.containerID, aRoom: this.room };
    }

    $.VolumeControl.prototype.render = function() {        
         var thisObj = this;
        
        var volumeControlContainer = $('<div id="VolumeControlContainer"/>');
        this.domElement.append(volumeControlContainer);
        var muteButton = $('<img id="VolumeControlButtonMute" ' + (this.useImageMap?'usemap="#VolumeControlButtonMuteMap"':'') + ' src="' + this.imageFolder + 'Wheel.png" />');
        volumeControlContainer.append(muteButton);
        var upButton = $('<img id="VolumeControlButtonUp" ' + (this.useImageMap?'usemap="#VolumeControlButtonUpMap"':'') + ' src="' + this.imageFolder + 'WheelVolumeUp.png" />');
        volumeControlContainer.append(upButton);
        var downButton = $('<img id="VolumeControlButtonDown" ' + (this.useImageMap?'usemap="#VolumeControlButtonDownMap"':'') + ' src="' + this.imageFolder + 'WheelVolumeDown.png" />');
        volumeControlContainer.append(downButton);
        var volumeDisplay = $('<span id="VolumeControlVolumeDisplay"/>');
        volumeControlContainer.append(volumeDisplay); 
        
        var downButtonSelector, muteButtonSelector, upButtonSelector;
                
        if (this.useImageMap){        
            var downButtonMap = $('<map name="VolumeControlButtonDownMap">' +
                                            '<area shape="poly" coords="' + this.imageCoords.downButton + '"  />' + 
                                            '</map>');
            volumeControlContainer.append(downButtonMap); 
            var muteButtonMap = $('<map name="VolumeControlButtonMuteMap">' +
                                            '<area shape="circle" coords="' + this.imageCoords.muteButton + '" />' + 
                                            '</map>');
            volumeControlContainer.append(muteButtonMap); 
                 
            var upButtonMap = $('<map name="VolumeControlButtonUpMap">' +
                                            '<area shape="poly" coords="' + this.imageCoords.upButton + '" />' + 
                                            '</map>');
            volumeControlContainer.append(upButtonMap);   
            
            downButtonSelector = downButtonMap;
            muteButtonSelector = $.merge(muteButtonMap,volumeDisplay);
            upButtonSelector = upButtonMap;
        }else{
            downButtonSelector = downButton;
            muteButtonSelector = volumeDisplay;
            upButtonSelector = upButton;
        }
           
        upButtonSelector.hover(function(evt){  
            upButton.addClass("Mouse");
            thisObj.evaluateButtonState();            
        },function (evt) {
            upButton.removeClass("Mouse");
            upButton.removeClass("Touch");
            thisObj.evaluateButtonState();
        }).mousedown(function(evt){        
            upButton.addClass("Touch");
            thisObj.evaluateButtonState();
        }).mouseup(function(evt){      
            upButton.removeClass("Mouse");  
            upButton.removeClass("Touch");
            thisObj.evaluateButtonState();
        }).click(function(evt) {
            var increment = thisObj.defaultIncrement;
            if (evt.ctrlKey){
                increment = thisObj.ctrlKeyIncrement;
            }else if (evt.shiftKey){
                increment = thisObj.shiftKeyIncrement;
            }
            var newVolume = thisObj.volume + increment;
            newVolume = Math.min(newVolume, thisObj.volumeLimit);
            thisObj.setVolume(newVolume);
            thisObj.evaluateButtonState();
            if (thisObj.mute){
                thisObj.setMute(false);
            }
        });
            
        downButtonSelector.hover(function(evt){  
            downButton.addClass("Mouse");
            thisObj.evaluateButtonState();            
        },function (evt) {
            downButton.removeClass("Mouse");
            downButton.removeClass("Touch");
            thisObj.evaluateButtonState();
        }).mousedown(function(evt){        
            downButton.addClass("Touch");
            thisObj.evaluateButtonState();
        }).mouseup(function(evt){        
            downButton.removeClass("Mouse");
            downButton.removeClass("Touch");
            thisObj.evaluateButtonState();
        }).click(function(evt) {
            var increment = thisObj.defaultIncrement;
            if (evt.ctrlKey){
                increment = thisObj.ctrlKeyIncrement;
            }else if (evt.shiftKey){
                increment = thisObj.shiftKeyIncrement;
            }
            var newVolume = thisObj.volume - increment;
            newVolume = Math.max(newVolume, 0);
            thisObj.setVolume(newVolume);
            thisObj.evaluateButtonState();            
            if (thisObj.mute){
                thisObj.setMute(false);
            }

        });      
        
        muteButtonSelector.hover(function(evt){  
            muteButton.addClass("Mouse");
            thisObj.evaluateButtonState();            
        },function (evt) {
            muteButton.removeClass("Mouse");
            muteButton.removeClass("Touch");
            thisObj.evaluateButtonState();
        }).mousedown(function(evt){     
            muteButton.addClass("Touch");
            thisObj.evaluateButtonState();
        }).click(function(evt){  
            muteButton.removeClass("Mouse");
            muteButton.removeClass("Touch");
            thisObj.setMute(!thisObj.mute);
            thisObj.evaluateButtonState();
        })
        $.VolumeControl.superclass.render.call(this);
    }
    
    $.VolumeControl.prototype.evaluateButtonState = function(){
        var muteButton = this.domElement.find("#VolumeControlButtonMute");
        var upButton = this.domElement.find("#VolumeControlButtonUp");
        var downButton = this.domElement.find("#VolumeControlButtonDown");
        var upDownButtonModifier = "";
        var muteButtonModifier = "";
        var opacity = "1.0";
        if (this.connected){
            if (upButton.hasClass("Touch") || downButton.hasClass("Touch")){
                upDownButtonModifier = "Touch";
            }else if (upButton.hasClass("Mouse") || downButton.hasClass("Mouse")){
                upDownButtonModifier = "Mouse";
            }
            muteButtonModifier = (upDownButtonModifier == ""?(muteButton.hasClass("Mouse")?"InnerGlow":""):"InnerGlow");
            if (this.mute){
                muteButtonModifier = "";
            }
            opacity = this.mute?"0.5":"1.0";
        }
        muteButton.attr("src", this.imageFolder + "Wheel" + muteButtonModifier + ".png").css("opacity", opacity);
        upButton.attr("src", this.imageFolder + "WheelVolumeUp" + upDownButtonModifier + ".png").css("opacity", opacity);
        downButton.attr("src", this.imageFolder + "WheelVolumeDown" + upDownButtonModifier + ".png").css("opacity", opacity);
        var volumeDisplay = this.domElement.find("#VolumeControlVolumeDisplay");
        volumeDisplay.text(this.connected?this.volume:"");
    }
    
    
    $.VolumeControl.prototype.setMute = function(mute) {
        var thisObj = this;
        this.mute = mute;
        var requestData = { aWidgetID: this.services.controllerWidget.serviceID, aMute: this.mute };     
        $.JSONRequest.sendRequest(this.services.controllerWidget.serviceName,
                                  "SetMute",
                                  requestData);

    }
    $.VolumeControl.prototype.onServiceStateChanged = function(service) {
        $.VolumeControl.superclass.onServiceStateChanged.call(this, service);
        if (service.serviceName == "viewWidget"){
            var thisObj = this;
            var requestData = {aWidgetID:this.services.viewWidget.serviceID};
            $.JSONRequest.sendRequest(this.services.viewWidget.serviceName,
                                      "GetState",
                                      requestData,
                                      function(responseData) {
                                          thisObj.onStateChangeResponse(responseData);
                                      });
        }
    }
    
    $.VolumeControl.prototype.onStateChangeResponse = function(responseData){
            this.connected = responseData.Connected;
			this.mute = responseData.Mute;
            var newVolumeLimit = responseData.VolumeLimit;
            var newVolume = responseData.Volume;
                this.volumeLimit = (newVolumeLimit == 0)?((newVolume == 0)?100:newVolume*2):newVolumeLimit;
                this.volume = newVolume;
            this.evaluateButtonState();
    }

    $.VolumeControl.prototype.setVolume = function(volume) {
        //communicate volume changed to server
        var requestData = { aWidgetID: this.services.controllerWidget.serviceID, aVolume: volume };
        var thisObj = this;
        this.volume = volume;
        $.JSONRequest.sendRequest(this.services.controllerWidget.serviceName,
                                  "SetVolume",
                                  requestData);
    }
    
    
    $.VolumeControl.prototype.openServices = function(room, callback) {
        this.room = room;
        $.VolumeControl.superclass.openServices.call(this, callback);
    }

    $.VolumeControl.prototype.closeServices = function(callback) {    
        this.volumeLimit = 0;
        this.volume = 0;
        this.connected = false;
        this.evaluateButtonState();
        $.VolumeControl.superclass.closeServices.call(this, callback);     
    }
    
})(jQuery);
