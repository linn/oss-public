/* play mode widget */
(function($) {
    
    
    $.PlayMode = function(domElement, container, playlist) {    
        if (domElement){
            this.init(domElement, container, playlist);
        }
    }
    $.PlayMode.prototype = new $.KinskyWidget;
    $.PlayMode.prototype.constructor = $.PlayMode;
    $.PlayMode.superclass = $.KinskyWidget.prototype;

    $.PlayMode.prototype.init = function(domElement, container, playlist) {
        $.PlayMode.superclass.init.call(this, domElement, container);
        this.services ["viewWidget"] = new $.Service("ViewPlayMode"),
        this.services ["controllerWidget"] = new $.Service("ControllerPlayMode")
        this.room = null;
        this.source = null;
        this.sourceType = null;
        this.shuffle = null;
        this.repeat = null;
        this.connected = false;
        this.playlist = playlist;
        var thisObj = this;
        playlist.domElement.bind("evtLocationChanged", function (evt, location, breadcrumbTrail){
            thisObj.setButtonState(thisObj.sourceType == $.SourceSelector.ESourceType.eDisc || 
                                   thisObj.sourceType == $.SourceSelector.ESourceType.ePlaylist || 
                                   thisObj.sourceType == $.SourceSelector.ESourceType.eUpnpAv);
        });
      }

    $.PlayMode.prototype.render = function() {        
        this.domElement.append('<div id="PlayModeToggleRepeat" class="PlayModeButton ToggleRepeat"/>');
        this.domElement.append('<div id="PlayModeToggleShuffle" class="PlayModeButton ToggleShuffle"/>');
        this.setButtonState(false);
        $.PlayMode.superclass.render.call(this);
    }
    
    $.PlayMode.prototype.setButtonState = function(enabled){
        if (enabled){
            this.domElement.find(".PlayModeButton").hover(function(){    
                $(this).addClass("Hover");
            },function () {
                $(this).removeClass("Hover");
            });
            var thisObj = this;
            this.domElement.find("#PlayModeToggleShuffle").click(function(){
			    thisObj.toggleShuffle();
            });
            this.domElement.find("#PlayModeToggleRepeat").click(function(){
			    thisObj.toggleRepeat();
            });
        }else{            
            this.domElement.find(".PlayModeButton")
            .unbind("click")
            .unbind("mouseenter")
            .toggleClass("Active",false);
        }
    }
    
    $.PlayMode.prototype.getServiceOpenRequest = function(svc) {
        return { aContainerID: this.container.containerID, aRoom: this.room, aSource:this.source };
    }
    
    $.PlayMode.prototype.onServiceStateChanged = function(service) {
        $.PlayMode.superclass.onServiceStateChanged.call(this, service);
        var thisObj = this;
        var requestData = {aWidgetID:this.services.viewWidget.serviceID};
        $.JSONRequest.sendRequest(this.services.viewWidget.serviceName,
                                  "GetState",
                                  requestData,
                                  function(responseData) {
                                      thisObj.onStateChangeResponse(responseData);
                                  },function(responseData) {
                                        debug.log("PlayMode: Error in get state call.");
                                  });
    }
    
    $.PlayMode.prototype.onStateChangeResponse = function(responseData){
            this.connected = responseData.Connected;
            if (responseData.Shuffle != this.shuffle) {
                this.shuffle = responseData.Shuffle;
            }
		        this.domElement.find("#PlayModeToggleShuffle")
			    .toggleClass("Active", (this.shuffle == true && this.connected));
            if (responseData.Repeat != this.repeat) {
                this.repeat = responseData.Repeat;
            }
		        this.domElement.find("#PlayModeToggleRepeat")
			    .toggleClass("Active", (this.repeat == true && this.connected)).removeClass("Hover") ;
    } 
                    
    
    $.PlayMode.prototype.toggleShuffle = function() {
			var thisObj = this;
			var requestData = { aWidgetID: this.services.controllerWidget.serviceID };
			$.JSONRequest.sendRequest(this.services.controllerWidget.serviceName,
									  "ToggleShuffle",
									  requestData);		
    }   
    
    $.PlayMode.prototype.toggleRepeat = function() {
			var thisObj = this;
			var requestData = { aWidgetID: this.services.controllerWidget.serviceID };
			$.JSONRequest.sendRequest(this.services.controllerWidget.serviceName,
									  "ToggleRepeat",
									  requestData);		
    }    
    
    $.PlayMode.prototype.openServices = function(room, source, sourceType, callback) {
        this.room = room;
        this.source = source;
        this.sourceType = sourceType;
        $.PlayMode.superclass.openServices.call(this, callback);
    }
    
    $.PlayMode.prototype.closeServices = function(callback) {
        this.room = null;
        this.source = null;
        this.sourceType = null;
        this.setButtonState(false);
        $.PlayMode.superclass.closeServices.call(this, callback);
    }
    
})(jQuery);