/* volume control widget */
(function($) {
    $.VolumeControlMobile = function(domElement, container, useImageMap, imageCoords, imageFolder, defaultIncrement, shiftKeyIncrement, ctrlKeyIncrement) {        
        if (domElement){
            this.init(domElement, container, useImageMap, imageCoords, imageFolder, defaultIncrement, shiftKeyIncrement, ctrlKeyIncrement);
        }
    }
    $.VolumeControlMobile.prototype = new $.VolumeControl;
    $.VolumeControlMobile.prototype.constructor = $.VolumeControlMobile;
    $.VolumeControlMobile.superclass = $.VolumeControl.prototype;

    $.VolumeControlMobile.prototype.render = function() {        
         var thisObj = this;
        
        var volumeControlContainer = $('<div id="VolumeControlContainer"/>');
        this.domElement.append(volumeControlContainer);
        var downButton = $('<img id="VolumeControlButtonDown" src="' + this.imageFolder + 'VolumeDown.png" />');
        volumeControlContainer.append(downButton);
        var upButton = $('<img id="VolumeControlButtonUp" src="' + this.imageFolder + 'VolumeUp.png" />');
        volumeControlContainer.append(upButton);
        var volumeDisplay = $('<span id="VolumeControlVolumeDisplay"/>');
        volumeControlContainer.append(volumeDisplay); 
		
		var downEvent = "mousedown";
		var upEvent = "mouseup";
		if (hasTouchSupport){
			downEvent = "touchstart";
			upEvent = "touchend";
		}
        
		upButton.bind(downEvent,function(evt){    
            upButton.addClass("Touch");
            thisObj.evaluateButtonState();
        }).bind(upEvent,function(evt){   
            upButton.removeClass("Touch");
            thisObj.evaluateButtonState();
        }).click(function(evt) {
            var increment = thisObj.defaultIncrement;
            var newVolume = thisObj.volume + increment;
            newVolume = Math.min(newVolume, thisObj.volumeLimit);
            thisObj.setVolume(newVolume);
            thisObj.evaluateButtonState();
            if (thisObj.mute){
                thisObj.setMute(false);
            }
        });
            
        downButton.bind(downEvent, function(evt){        
            downButton.addClass("Touch");
            thisObj.evaluateButtonState();
        }).bind(upEvent,function(evt){        
            downButton.removeClass("Touch");
            thisObj.evaluateButtonState();
        }).click(function(evt) {
            var increment = thisObj.defaultIncrement;
            var newVolume = thisObj.volume - increment;
            newVolume = Math.max(newVolume, 0);
            thisObj.setVolume(newVolume);
            thisObj.evaluateButtonState();            
            if (thisObj.mute){
                thisObj.setMute(false);
            }

        });      
        
        volumeDisplay.click(function(evt){  
            thisObj.setMute(!thisObj.mute);
            thisObj.evaluateButtonState();
        })
        $.KinskyWidget.prototype.render.call(this);
    }
    
    $.VolumeControlMobile.prototype.evaluateButtonState = function(){
        var upButton = this.domElement.find("#VolumeControlButtonUp");
        var downButton = this.domElement.find("#VolumeControlButtonDown");
		var opacity = "1.0";
        if (this.connected){
            opacity = this.mute?"0.5":"1.0";
			upButton.attr("src", this.imageFolder + "VolumeUp" + (upButton.hasClass("Touch")?"Touch":"") + ".png").css("opacity", opacity).show();
			downButton.attr("src", this.imageFolder + "VolumeDown" + (downButton.hasClass("Touch")?"Touch":"") + ".png").css("opacity", opacity).show();
        }else{
			upButton.attr("src", this.imageFolder + "VolumeEmpty.png");
			downButton.attr("src", this.imageFolder + "VolumeEmpty.png");
		}
        var volumeDisplay = this.domElement.find("#VolumeControlVolumeDisplay");
        volumeDisplay.text(this.connected?this.volume:"").css("opacity", opacity);
    }
    
})(jQuery);
