(function($) {

    $.RoomSelectorMobile = function(domElement, container, imageFolder) {   
        if (domElement){
            this.init(domElement, container, imageFolder);
        }
    }
    $.RoomSelectorMobile.prototype = new $.RoomSelector;
    $.RoomSelectorMobile.prototype.constructor = $.RoomSelectorMobile;
    $.RoomSelectorMobile.superclass = $.RoomSelector.prototype;    
    
    $.RoomSelectorMobile.prototype.init = function(domElement, container, imageFolder) {   
        $.RoomSelectorMobile.superclass.init.call(this, domElement, container);
        this.imageFolder = imageFolder;
    }
    
    $.RoomSelectorMobile.prototype.render = function() {
        var thisObj = this;
        var toggleViewButton = $('<div id="RoomSelectorToggleView"><img id="RoomSelectorToggleViewImage"/><span id="RoomSelectorToggleViewText"/></div>');
        var panel = $('<div id="RoomSelectorPanel"/>');
        var cancel = $('<div id="RoomSelectorCancel">Close</div>');
        panel.append(cancel);
        cancel.click(function(e){
		    thisObj.hideRoomSelection();
		});   
        panel.append($('<div id="RoomSelectorRoomList" />')); 
        this.domElement.append(toggleViewButton);
		toggleViewButton.click(function(e){
		    thisObj.showRoomSelection();
		});
        this.domElement.append(panel);    
        this.domElement.find("#RoomSelectorToggleViewImage").attr("src", this.imageFolder + "/Room.png");   
		this.hideRoomSelection();		
    }

    $.RoomSelectorMobile.prototype.hideRoomSelection = function(){
        this.domElement.find("#RoomSelectorPanel").hide();
        this.domElement.find("#RoomSelectorToggleView").show();
        this.domElement.trigger("evtHideRoomSelection");
    }
    
    $.RoomSelectorMobile.prototype.showRoomSelection = function(){    
        if (this.domElement.find("#RoomSelectorRoomList").children().length > 0){
            this.domElement.find("#RoomSelectorPanel").show();
            this.domElement.find("#RoomSelectorToggleView").hide();
            this.domElement.trigger("evtShowRoomSelection");
        }
    }
    
    $.RoomSelectorMobile.prototype.onStateChangeResponse = function(responseData){
		var thisObj = this;
        if (this.servicesOpened){
            var roomList = this.domElement.find("#RoomSelectorRoomList");       
            
            //clear existing values
            roomList.empty();
				
            
            //add new values
            var found = false;
		    $.each(responseData, function(i, value){
			    if (value==thisObj.lastSelectedRoom){
			        found = true;
			    }
		    });
            if (!found){
		        this.room = null;
		        this.domElement.trigger("evtRoomSelectorRoomChanged", this);
	        }else if (this.lastSelectedRoom && (this.lastSelectedRoom != this.room)){
                this.room = this.lastSelectedRoom;
		        this.domElement.trigger("evtRoomSelectorRoomChanged", this);
            }  
	        var toggleViewButton = thisObj.domElement.find("#RoomSelectorToggleViewText");
		    $.each(responseData, function(i, value){
                    var newElement = $(thisObj.createItem(value, i));
			        roomList.append(newElement);
			        newElement.click(function(e){
			            if (thisObj.room != value){
			                thisObj.room = value;
			                thisObj.lastSelectedRoom = value;
	                        thisObj.domElement.find(".ListItem").removeClass("selected");
    				        $(this).addClass("selected");
				            toggleViewButton.text(thisObj.room);
                            thisObj.domElement.trigger("evtRoomSelectorRoomChanged", thisObj);   
                        }
			            thisObj.hideRoomSelection();
	                });
	                if (value == thisObj.room){
	                    newElement.addClass("selected");
	                }
                    if (thisObj.domElement.width()){
                        newElement.css("max-width", thisObj.domElement.width() + "px");
                    }		                
		    });
		    var text = (this.room?this.room:(responseData.length?"Please select a room":"No rooms found"));
            toggleViewButton.text(text);
        }
    }
    $.RoomSelectorMobile.prototype.createItem = function(value, id) {
		    return  $("<div><div class='ListItem' id='ListItem_" + id + "'><span class='ListItemBody'>" + value + "</span></div></div>").html();    
	}
	
    $.RoomSelectorMobile.prototype.closeServices = function(callback) {
        this.room = null;
        var roomList = this.domElement.find("#RoomSelectorRoomList");
        roomList.empty();
        $.KinskyWidget.prototype.closeServices.call(this, callback);    
    }    

})(jQuery);