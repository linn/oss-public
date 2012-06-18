(function($) {    
    
    $.StandbyButton = function(domElement, container, roomSelector, sourceSelector) {    
        if (domElement){
            this.init(domElement, container, roomSelector, sourceSelector);
        }
    }
    $.StandbyButton.prototype = new $.KinskyWidget;
    $.StandbyButton.prototype.constructor = $.StandbyButton;
    $.StandbyButton.superclass = $.KinskyWidget.prototype;

    $.StandbyButton.prototype.init = function(domElement, container, roomSelector, sourceSelector) {
        $.StandbyButton.superclass.init.call(this, domElement, container);
        this.roomSelector = roomSelector;
        this.sourceSelector = sourceSelector;
        var thisObj = this;
        roomSelector.domElement.bind("evtRoomSelectorRoomChanged", function (){
            thisObj.setButtonState(roomSelector.room != null);
        });
    }

    $.StandbyButton.prototype.render = function() {        
        this.domElement.append('<div id="StandbyButton" />');
        this.setButtonState(false);
        $.StandbyButton.superclass.render.call(this);        
    } 

    $.StandbyButton.prototype.setButtonState  = function(enabled){
        var thisObj = this;
        if (enabled){
            this.domElement.find("#StandbyButton")
            .unbind("mouseenter")
            .unbind("mouseleave")
            .hover(function(){    
                $(this).addClass("Hover");
            },function () {
                $(this).removeClass("Hover");
            })
            .unbind("click")
            .click(function(){
                thisObj.domElement.trigger("evtStandby", thisObj);
			    $(this).removeClass("Hover");
            })
            .addClass("Active");
        }else{            
            this.domElement.find("#StandbyButton")
            .unbind("click")
            .unbind("mouseenter")
            .unbind("mouseleave")
            .removeClass("Active");
        }
    }
    
    $.StandbyButton.prototype.openServices = function(room, source, sourceType, callback) {
        $.StandbyButton.superclass.openServices.call(this, callback);
    }
    
    $.StandbyButton.prototype.closeServices = function(callback) {
        this.setButtonState(false);
        $.StandbyButton.superclass.closeServices.call(this, callback);
    }
    
})(jQuery);