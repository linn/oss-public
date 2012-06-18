/* current track widget */
(function($) {
    
    
    $.Track = function(domElement, container) {    
        if (domElement){
            this.init(domElement, container);
        }
    }
    $.Track.prototype = new $.KinskyWidget;
    $.Track.prototype.constructor = $.Track;
    $.Track.superclass = $.KinskyWidget.prototype;

    $.Track.prototype.init = function(domElement, container) {
        $.Track.superclass.init.call(this, domElement, container);
        this.services["viewWidget"] = new $.Service("ViewTrack")
        this.room = null;
        this.source = null;
    }
    
    $.Track.prototype.render = function() {      
        var container = $('<div id="TrackContainer"></div>');
        container.append('<img id="Artwork"/>');        
        container.append('<div id="TrackInfo"><p id="Title"/><p id="Album" /><p id="Artist" /></div>');
        container.append('<div id="TrackEncoding"><p id="Bitrate" /><p id="SampleRate" /><p id="Codec" /></div>');
        this.domElement.append(container);
        this.domElement.find("#Artwork").error(function(){
				$(this).hide();
            }).attr("src", "/Artwork");
            /* TODO: reinstate this once mono wcf supports streams
            .attr("src", "/"  
	 	        + this.services.viewWidget.serviceName + "/Json" + 
	 	        + "/Artwork"  
            */
        $.Track.superclass.render.call(this);
    }
    
    
    $.Track.prototype.setHeight = function(height) {      
        this.domElement.height(height);
    }
    
    
    $.Track.prototype.getServiceOpenRequest = function(svc) {
        return { aContainerID: this.container.containerID, aRoom: this.room, aSource:this.source };
    }
    
    $.Track.prototype.onServiceStateChanged = function(service) {
        $.Track.superclass.onServiceStateChanged.call(this, service);
        var thisObj = this;
        var requestData = {aWidgetID:this.services.viewWidget.serviceID};
        $.JSONRequest.sendRequest(this.services.viewWidget.serviceName,
                                  "GetState",
                                  requestData,
                                  function(responseData) {
                                      thisObj.onStateChangeResponse(responseData);
                                  },function(responseData) {
                                        debug.log("Track: Error in get state call.");
                                  });
    }
    
    $.Track.prototype.onStateChangeResponse = function(responseData){        
        if (this.servicesOpened && responseData && responseData.Connected){    
            this.domElement.find("#Title").text(responseData.Title);
            if (responseData.Album){
                this.domElement.find("#Album").text(responseData.Album);
            }else{
                this.domElement.find("#Album").text("");
            }
            if (responseData.Artist){                
                this.domElement.find("#Artist").text(responseData.Artist);
            }else{
                this.domElement.find("#Artist").text("");
            }
            if (responseData.Bitrate){
                this.domElement.find("#Bitrate").text(responseData.Bitrate + " kbps");
            }else{
                this.domElement.find("#Bitrate").text("");
            }
            if (responseData.SampleRate){
                this.domElement.find("#SampleRate").text(responseData.SampleRate + " kHz");
            }else{        
                this.domElement.find("#SampleRate").text("");
            }
            this.domElement.find("#Codec").text(responseData.Codec);
                /* TODO: reinstate this once mono wcf supports streams
                this.domElement.find("#Artwork").show().attr("src", "/"  
	 	                                               + this.services.viewWidget.serviceName + "/Json" + 
	 	                                               + "/Artwork?aWidgetID="	 	                                            
                                                       + this.services.viewWidget.serviceID
                                                       + "&unique=" + new Date().getTime());  
                */
            this.domElement.find("#Artwork").show().attr("src", "/Artwork?aWidgetID=" 
                                               + this.services.viewWidget.serviceID
                                               + "&unique=" + new Date().getTime());
        }else{
            this.domElement.find("#Title").text("");
            this.domElement.find("#Album").text("");
            this.domElement.find("#Artist").text("");
            this.domElement.find("#Bitrate").text("");
            this.domElement.find("#SampleRate").text("");
            this.domElement.find("#Codec").text("");
            this.domElement.find("#Artwork").attr("src", "/Artwork");
        }
    }
    
    $.Track.prototype.openServices = function(room, source, callback) {
        this.room = room;
        this.source = source;
        $.Track.superclass.openServices.call(this, callback);
    }
    
    $.Track.prototype.closeServices = function(callback) {
        this.room = null;
        this.source = null;
        this.onStateChangeResponse(null);
        $.Track.superclass.closeServices.call(this, callback);
    }
    
})(jQuery);