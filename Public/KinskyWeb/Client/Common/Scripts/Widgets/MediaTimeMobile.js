/* media time widget */
(function($) {
    
    
    $.MediaTimeMobile = function(domElement, container, useImageMap, imageCoords, useMouseDownTimers, imageFolder, defaultIncrement, shiftKeyIncrement, ctrlKeyIncrement) {
        if (domElement){
            this.init(domElement, container, useImageMap, imageCoords, useMouseDownTimers, imageFolder, defaultIncrement, shiftKeyIncrement, ctrlKeyIncrement);
        }
    }
    $.MediaTimeMobile.prototype = new $.MediaTime;
    $.MediaTimeMobile.prototype.constructor = $.MediaTimeMobile;
    $.MediaTimeMobile.superclass = $.MediaTime.prototype;

    $.MediaTimeMobile.prototype.render = function() {               
        var thisObj = this;        
        var mediaTimeContainer = $('<div id="MediaTimeContainer"/>');
        this.domElement.append(mediaTimeContainer);        
        var secondsDisplay = $('<div id="MediaTimeSeconds"><span/></div>');
        mediaTimeContainer.append(secondsDisplay);                
        $.MediaTimeMobile.superclass.render.call(this);
    }
    
    
    $.MediaTimeMobile.prototype.evaluateButtonState = function(){
        var secondsDisplay = this.domElement.find("#MediaTimeSeconds");
        secondsDisplay.text(this.connected?this.getMediaTime(this.seconds):"");
    }
    
    
    
})(jQuery);