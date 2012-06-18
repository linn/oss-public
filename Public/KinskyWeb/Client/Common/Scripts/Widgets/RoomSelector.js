(function($) {
    
    var kPleaseSelectMessage = "--- Please Select Room ---";

    $.RoomSelector = function(domElement, container) {    
        if (domElement){
            this.init(domElement, container);
        }
    }
    $.RoomSelector.prototype = new $.KinskyWidget;
    $.RoomSelector.prototype.constructor = $.RoomSelector;
    $.RoomSelector.superclass = $.KinskyWidget.prototype;


    $.RoomSelector.prototype.init = function(domElement, container) {
        $.RoomSelector.superclass.init.call(this, domElement, container);
        this.services["roomList"] = new $.Service("ViewRoomList")
    }

    $.RoomSelector.prototype.getServiceOpenRequest = function(svc) {
        return { aContainerID: this.container.containerID };
    }

    $.RoomSelector.prototype.render = function() {
        //this.domElement.append('<label>Rooms:</label>');        
        this.domElement.append('<select id="RoomSelectorRoomList" class="ui-state-default"/>');
        var roomList = this.domElement.find("#RoomSelectorRoomList");
	    roomList.addOption(kPleaseSelectMessage,kPleaseSelectMessage,true);
        var thisObj = this;
        
        this.change = function(){
		    if ($(this).selectedValues().length && $(this).selectedValues()[0] != kPleaseSelectMessage){
		        $(this).removeOption(kPleaseSelectMessage);
		        thisObj.room = $(this).selectedValues()[0];
		        thisObj.lastSelectedRoom = $(this).selectedValues()[0];
			}else{
			    thisObj.room = null;
		        thisObj.lastSelectedRoom = null;
		    }
		    if (thisObj.room){
		        thisObj.domElement.trigger("evtRoomSelectorRoomChanged", thisObj);
		    }
        };
        this.domElement.find("#RoomSelectorRoomList").bind("change", this.change);
        $.RoomSelector.superclass.render.call(this);   
    }
    
    $.RoomSelector.prototype.onServiceStateChanged = function(service) {
        $.RoomSelector.superclass.onServiceStateChanged.call(this, service);
        var thisObj = this;
        var requestData = {aWidgetID:this.services.roomList.serviceID};
        $.JSONRequest.sendRequest(this.services.roomList.serviceName,
                                  "Rooms",
                                  requestData,
                                  function(responseData) {
                                      thisObj.onStateChangeResponse(responseData);
                                  });
    }
    
    $.RoomSelector.prototype.onStateChangeResponse = function(responseData){
        var thisObj = this;
        var roomList = this.domElement.find("#RoomSelectorRoomList");
        //remove change handler
        roomList.unbind("change", this.change);
        
        //clear existing values
        roomList.removeOption(/./);
        
        //add new values
		var found = false;
		$.each(responseData, function(i, value){
			if (value==thisObj.lastSelectedRoom){
			    found = true;
			}
		});
        if (!found || !this.lastSelectedRoom){
	        roomList.addOption(kPleaseSelectMessage,kPleaseSelectMessage,true);
	    }
	    if (!found){
		    this.room = null;
		    this.domElement.trigger("evtRoomSelectorRoomChanged", this);
		}else if (this.lastSelectedRoom && (this.lastSelectedRoom != this.room)){
            this.room = this.lastSelectedRoom;
		    this.domElement.trigger("evtRoomSelectorRoomChanged", this);
        }
	    
		$.each(responseData, function(i, value){
			roomList.addOption(value,value,(value==thisObj.room));
		});
		
        //re-add change handler
        roomList.bind("change", this.change);
    }
    
    $.RoomSelector.prototype.standby = function(){
        this.room = null;
        this.lastSelectedRoom = null;
    }

    $.RoomSelector.prototype.closeServices = function(callback) {
        this.room = null;
        var roomList = this.domElement.find("#RoomSelectorRoomList");
        roomList.removeOption(/./);
	    roomList.addOption(kPleaseSelectMessage,kPleaseSelectMessage,true);
        $.RoomSelector.superclass.closeServices.call(this, callback);    
    }
    $.RoomSelector.prototype.openServices = function(callback) {
        this.room = null;
        $.RoomSelector.superclass.openServices.call(this, callback);       
    }
    
})(jQuery);