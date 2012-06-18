(function($) {
    $.Playlist = function(domElement, container, pageSize, pagerButtonCount, pagerTop, imageFolder, mobileContextMenuView) {   
        if (domElement){
            this.init(domElement, container, pageSize, pagerButtonCount, pagerTop, imageFolder, mobileContextMenuView);
        }
    }
    $.Playlist.prototype = new $.Browser;
    $.Playlist.prototype.constructor = $.Playlist;
    $.Playlist.superclass = $.Browser.prototype;
    
    
    $.Playlist.prototype.init = function(domElement, container, pageSize, pagerButtonCount, pagerTop, imageFolder, mobileContextMenuView) {   
        $.Playlist.superclass.init.call(this, domElement, container, null, null, pageSize, pagerButtonCount, pagerTop, imageFolder, mobileContextMenuView);
        this.services["viewPlaylist"] = new $.Service("ViewPlaylist");
        this.services["controllerPlaylist"] = new $.Service("ControllerPlaylist");
        this.source = null;
        this.room = null;
        this.sourceType = null;
        this.allowRepeatedActivation = true;
        this.draggableSet = false;       
    }
    
    $.Playlist.prototype.onServiceStateChanged = function(service) {     
        $.Playlist.superclass.onServiceStateChanged.call(this,service);
        if (service == this.services.viewPlaylist){
            this.getCurrentPlaylistItem();
        }
    }
    
    $.Playlist.prototype.connectBrowser = function() {
        if (this.sourceType == $.SourceSelector.ESourceType.ePlaylist || 
            this.sourceType == $.SourceSelector.ESourceType.eRadio){
            $.Playlist.superclass.connectBrowser.call(this);
        }else{
            this.cancelConnect();
        }
    }
    
    $.Playlist.prototype.getCurrentPlaylistItem = function(){
        var thisObj = this;
        var requestData = {aWidgetID:this.services.viewPlaylist.serviceID};
        $.JSONRequest.sendRequest(this.services.viewPlaylist.serviceName,
                                      "CurrentItem",
                                      requestData,
                                      function (responseData){
                                        thisObj.onGetCurrentPlaylistItem(responseData);
                                      });
    }
    
    $.Playlist.prototype.onGetCurrentPlaylistItem = function(responseData) {
        if (responseData){
                    this.domElement.find(".BrowserItem .img").removeClass("Playing");
                    this.domElement.find("#BrowserItem_" + responseData.ID + " .img").addClass("Playing");
        }
    }
    
    $.Playlist.prototype.getServiceOpenRequest = function(svc) {
        if (svc == this.services.browser){
            return { aContainerID: this.container.containerID, aLocation:this.location };
        }else{
            return { aContainerID: this.container.containerID, aRoom:this.room, aSource:this.source };
        }
    }
    
    $.Playlist.prototype.getContextMenu = function(){
        var thisObj = this;
        return [
          {action:"PlayNow", title:"Play Now", separator:false, callback:function (action, el, pos){
            thisObj.activate($(el).parent());
            return false;
          }},
          {action:"MoveToEnd", title:"Move To End", separator:false, callback:function (action, el, pos){
            thisObj.moveToEnd([thisObj.getContextMenuItem(el)]);
            return false;            
          }},
          {action:"Delete", title:"Delete", separator:true, callback:function (action, el, pos){
            thisObj.deleteItems([thisObj.getContextMenuItem(el)]);
            return false;
          }},
          {action:"Cancel", title:"Cancel", separator:true, callback:function (action, el, pos){
            return false;
          }}        
        ];        
    }
    
    $.Playlist.prototype.onItemsDropped = function (evt, dragContainer, dropContainer, dragItems, dropIndex, dragSource){
        this.showLoading(true);
        var children = [];
		for (var i=0;i<dragItems.length;i++){
		    var childID = $(dragItems[i]).attr("ID").replace(/BrowserItem_/,"");
		    children[children.length] = dragSource.cachedTableData[childID];
		}
		
		var requestData;
		var insertAfterItemID;
		if (dropIndex >=1){
		    var insertAfterItem = dropContainer.children().eq(dropIndex - 1);
		     insertAfterItemID = insertAfterItem.attr("ID").replace(/BrowserItem_/,"");
		}
		
		if (dragSource === this){
		    requestData = {aWidgetID:this.services.controllerPlaylist.serviceID, 
		                       aChildren: children,
		                       aInsertAfterItem: (insertAfterItemID)?this.cachedTableData[insertAfterItemID]:null};
            $.JSONRequest.sendRequest(this.services.controllerPlaylist.serviceName,
                                      "Move",
                                      requestData);
		
		}else{
		    requestData = {aWidgetID:this.services.controllerPlaylist.serviceID, 
		                       aContainerSourceID: dragSource.services.browser.serviceID,
		                       aChildren: children,
		                       aInsertAfterItem: (insertAfterItemID)?this.cachedTableData[insertAfterItemID]:null};
            $.JSONRequest.sendRequest(this.services.controllerPlaylist.serviceName,
                                      "Insert",
                                      requestData);
		}		
    }
    
    $.Playlist.prototype.openServices = function(room, source, sourceType, callback) {
        this.room = room;
        this.source = source;
        this.sourceType = sourceType;
        if (this.room && this.source){
            this.showLoading(true);
            this.location = ["Home","Rooms",this.room, this.source];
            
            var itemsContainer = this.domElement.find("#BrowserContent");
            
		    if (this.sourceType == $.SourceSelector.ESourceType.ePlaylist && !this.draggableSet){
			    this.dropTargetSelector = "#" + this.domElement.attr("ID") + " #BrowserContent";  
                this.createDraggable(itemsContainer, 
								 this.dropTargetSelector,  
								 ".BrowserItem.ui-state-highlight",
								 this)
				.bind("evtItemsDragged", this.itemsDragged)
				.bind("evtItemsDropped", this.itemsDropped);
				this.draggableSet = true;  
            }else if (this.sourceType != $.SourceSelector.ESourceType.ePlaylist && this.draggableSet){
                    this.destroyDraggable(itemsContainer)
			        .unbind("evtItemsDragged", this.itemsDragged)
			        .unbind ("evtItemsDropped", this.itemsDropped);
				    this.draggableSet = false;            
                }
            $.Playlist.superclass.openServices.call(this, callback);
        }else{
            this.showLoading(false);      
        }
    }
    
    $.Playlist.prototype.onLazyLoadChildrenCompleted = function() {    
        $.Playlist.superclass.onLazyLoadChildrenCompleted.call(this);
        this.getCurrentPlaylistItem();        
    }
    
    
    $.Playlist.prototype.deleteItems = function(items) {  
        var requestData = {aWidgetID:this.services.controllerPlaylist.serviceID};
        if (items){
            requestData["aChildren"] = items;
        }
        $.JSONRequest.sendRequest(this.services.controllerPlaylist.serviceName,
                    items?"Delete":"DeleteAll",
                    requestData);
        this.showLoading(true);
    }
    
    $.Playlist.prototype.moveToEnd = function(items) {  
        var requestData = {aWidgetID:this.services.controllerPlaylist.serviceID, aChildren:items};
        $.JSONRequest.sendRequest(this.services.controllerPlaylist.serviceName,
                    "MoveToEnd",
                    requestData);
        this.showLoading(true);
    }
    
})(jQuery);