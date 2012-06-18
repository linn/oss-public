/* waste bin widget */
(function($) {    
    
    $.WasteBin = function(domElement, container, playlist) {    
        if (domElement){
            this.init(domElement, container, playlist);
        }
    }
    $.WasteBin.prototype = new $.KinskyWidget;
    $.WasteBin.prototype.constructor = $.WasteBin;
    $.WasteBin.superclass = $.KinskyWidget.prototype;

    $.WasteBin.prototype.init = function(domElement, container, playlist) {
        $.WasteBin.superclass.init.call(this, domElement, container);
        this.room = null;
        this.source = null;
        this.sourceType = null;
        this.playlist = playlist;
        var thisObj = this;
        playlist.domElement.bind("evtLocationChanged", function (evt, location, breadcrumbTrail){
            thisObj.setButtonState(thisObj.sourceType == $.SourceSelector.ESourceType.ePlaylist);
        });
    }

    $.WasteBin.prototype.render = function() {        
        this.domElement.append('<div id="WasteBinButton" />');
        this.setButtonState(false);
        $.WasteBin.superclass.render.call(this);        
    } 

    $.WasteBin.prototype.setButtonState  = function(enabled){
        if (enabled){
            this.domElement.find("#WasteBinButton").hover(function(){    
                $(this).addClass("Hover");
            },function () {
                $(this).removeClass("Hover");
            });
            var thisObj = this;
            this.domElement.find("#WasteBinButton").click(function(){
			    thisObj.playlist.deleteItems();
			    $(this).removeClass("Hover");
            });
        }else{            
            this.domElement.find("#WasteBinButton")
            .unbind("click")
            .unbind("mouseenter");
        }
    }
    
    $.WasteBin.prototype.openServices = function(room, source, sourceType, callback) {
        this.room = room;
        this.source = source;
        this.sourceType = sourceType;
        $.WasteBin.superclass.openServices.call(this, callback);
    }
    
    $.WasteBin.prototype.closeServices = function(callback) {
        this.room = null;
        this.source = null;
        this.sourceType = null;
        this.setButtonState(false);
        $.WasteBin.superclass.closeServices.call(this, callback);
    }
    
})(jQuery);