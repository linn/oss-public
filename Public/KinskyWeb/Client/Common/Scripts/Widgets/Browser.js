(function($) {
    $.Browser = function(domElement, container, location, dropTargetSelector, pageSize, pagerButtonCount, pagerTop, imageFolder, mobileContextMenuView) {
        if (domElement){
            this.init(domElement, container, location, dropTargetSelector, pageSize, pagerButtonCount, pagerTop, imageFolder, mobileContextMenuView);
        }
    }
    $.Browser.prototype = new $.KinskyWidget;
    $.Browser.prototype.constructor = $.Browser;
    $.Browser.superclass = $.KinskyWidget.prototype; 
    
    $.Browser.EStaticImages = {
        kImageNoAlbumArt: "NoAlbumArt.png",
        kImageIconAlbum : "Album.png",
        kImageIconAlbumError  : "AlbumError.png",
        kImageIconArtist  : "Artist.png",
        kImageIconDirectory  : "Directory.png",
        kImageIconError  : "Error.png",
        kImageIconPlaylist  : "Playlist.png",
        kImageIconRadio  : "Radio.png",
        kImageIconServer  : "Library.png",
        kImageIconStar  : "Star.png",
        kImageIconTrack  : "Track.png",
        kImageIconVideo  : "Video.png"
    }    

    $.Browser.prototype.init = function(domElement, container, location, dropTargetSelector, pageSize, pagerButtonCount, pagerTop, imageFolder, mobileContextMenuView) {
        $.Browser.superclass.init.call(this, domElement, container, imageFolder);
        this.services["browser"] = new $.Service("Browser");
        this.connected = false;
        this.loaded = false;
        this.pendingRequests = [];
        this.totalChildCount = 0;
        this.loadedChildCount = 0;
        this.pageSize = pageSize ? pageSize : 1000;
        this.cachedTableData = {};
        this.location = location?location:["Home"];
        this.breadcrumbTrail = [];
        this.dropTargetSelector = dropTargetSelector;
        this.currentPageIndex = 0;
        this.pagerButtonCount = pagerButtonCount ? pagerButtonCount : 10;
        this.pagerTop = pagerTop;
        this.mobileContextMenuView = mobileContextMenuView;
        this.contextMenuID = this.domElement.attr("id") + "ContextMenu";
        var thisObj = this;
        this.itemsDragged = function(evt, dragContainer, dropContainer, dragItems, dropIndex, dragSource){
            thisObj.onItemsDragged(evt, dragContainer, dropContainer, dragItems, dropIndex, dragSource);
        }
        this.itemsDropped = function(evt, dragContainer, dropContainer, dragItems, dropIndex, dragSource){
            thisObj.onItemsDropped(evt, dragContainer, dropContainer, dragItems, dropIndex, dragSource);
        }
        this.allowRepeatedActivation = false;
    }
    
    $.Browser.prototype.setTransportControl = function (transportControl){
        this.transportControl = transportControl;    
    }
    
    $.Browser.prototype.openServices = function (callback){        
        $.Browser.superclass.openServices.call(this,callback);
    }
    
    $.Browser.prototype.onItemsDragged = function (evt, dragContainer, dropContainer, dragItems, dropIndex, dragSource){
    }
    
    $.Browser.prototype.onItemsDropped = function (evt, dragContainer, dropContainer, dragItems, dropIndex, dragSource){
    }
  
    $.Browser.prototype.getServiceOpenRequest = function(svc) {
        return { aContainerID: this.container.containerID, aLocation:this.location };
    }
    
    $.Browser.prototype.showLoading = function(loading, aborted){
        var thisObj = this;
        if (loading){
            this.domElement.find("#LoadProgress").show();  
            //this.domElement.css("cursor","wait");
            this.domElement.trigger("evtLocationChanging");
        }else{        
            this.domElement.find("#LoadProgress").hide();  
            //this.domElement.css("cursor","normal");
            if (aborted){                
                thisObj.domElement.trigger("evtLocationChangingAborted"); 
            }
        }
    }
    
    $.Browser.prototype.up = function(numberLevels) {  
        this.showLoading(true);
        var requestData = { aWidgetID: this.services.browser.serviceID, aNumberLevels:numberLevels };
        var thisObj = this;
        $.JSONRequest.sendRequest(this.services.browser.serviceName,
            "Up",
            requestData,
            function(){},
            function(){
                thisObj.showLoading(false, true);
            });
    }
  
    $.Browser.prototype.onServiceStateChanged = function(service) {
        if (service == this.services.browser){
            $.Browser.superclass.onServiceStateChanged.call(this, service);
            this.currentPageIndex = 0;
            this.connectBrowser();
        }
    }
    
    $.Browser.prototype.getLocation = function(){
        var thisObj = this;
        var requestData = {aWidgetID:this.services.browser.serviceID};
	    $.JSONRequest.sendRequest(this.services.browser.serviceName,
            "CurrentLocation",
            requestData,
            function (responseData){
                thisObj.onGetLocationResponse(responseData);
            });
    }
    
    $.Browser.prototype.onGetLocationResponse = function(responseData){
        var thisObj = this;
        var tmpLocation = [];
        var tmpBreadcrumbTrail = [];
        if (!responseData.ErrorOccured){
            $.each(responseData.Location, function(i, locn){
                tmpLocation[i] = locn.ID;
                tmpBreadcrumbTrail[i] = locn.BreadcrumbText;
            });
            this.location = tmpLocation;
            this.breadcrumbTrail = tmpBreadcrumbTrail;
            this.domElement.trigger("evtLocationChanged",[this.location, this.breadcrumbTrail]);
            if (this.location.length == 2 && this.location[1] == "Library"){
                $.each(this.domElement.find(".albumart"), function(){
                    $(this).attr("src",thisObj.imageFolder + $.Browser.EStaticImages.kImageIconServer);
                });
            }
        }
    }
    
    $.Browser.prototype.render = function(){
        var thisObj = this;
        var panel = $("<div id='BrowserPanel' class='ui-widget ui-corner-all'/>");
        if (this.pagerTop){
            panel.append("<div id='BrowserPager'/>");
        }
        panel.append("<div id='BrowserContent' class='UpnpDropTarget ui-widget-content ui-corner-bottom BrowserContent'/>");
        if (!this.pagerTop){
            panel.append("<div id='BrowserPager'/>");
        }
        panel.append("<div id='LoadProgress' class='LoadProgress'/>");
        var contextMenu = $("<ul id='" + this.contextMenuID + "' class='contextMenu ui-corner-all'/>");
        $.each(this.getContextMenu(), function(i, item){
            contextMenu.append($("<li" + (item.separator?" class='separator'":"") + "><a href='#" + item.action + "'>" + item.title + "</a></li>"));
        });
        this.domElement.find("#BrowserPager").hide();
        this.domElement.append(panel);        
        $("body").append(contextMenu);        
        
        this.domElement.find("#LoadProgress").hide();
		var itemsContainer = this.domElement.find("#BrowserContent");
        if (this.dropTargetSelector){
		    this.createDraggable(itemsContainer, 
					this.dropTargetSelector,  
					".BrowserItem.ui-state-highlight",
			        this)
			.bind("evtItemsDragged", this.itemsDragged)
		    .bind("evtItemsDropped", this.itemsDropped);                
		} 
        $.Browser.superclass.render.call(this);
    }
    
    $.Browser.prototype.getContextMenu = function(){
        var thisObj = this;
        return [
          {action:"PlayNow", title:"Play Now", separator:true, callback:function (action, el, pos){
            thisObj.sendMenuTransportCommand([thisObj.getContextMenuItem(el)], action);
            return false;
          }},
          {action:"PlayNext", title:"Play Next", separator:false, callback:function (action, el, pos){
            thisObj.sendMenuTransportCommand([thisObj.getContextMenuItem(el)], action);
            return false;
          }},
          {action:"PlayLater", title:"Play Later", separator:false, callback:function (action, el, pos){
            thisObj.sendMenuTransportCommand([thisObj.getContextMenuItem(el)], action);
            return false;
          }},
          {action:"Cancel", title:"Cancel", separator:true, callback:function (action, el, pos){
            return false;
          }}        
        ];        
    }
    
    $.Browser.prototype.getContextMenuItem = function(element){
        if (this.mobileContextMenuView){
            element = $(element).parent();
        }
        var browserItem = element.attr("ID").replace(/BrowserItem_/,"");
		return this.cachedTableData[browserItem];
    }
    
    $.Browser.prototype.sendMenuTransportCommand = function(media, command){
        if (media && this.transportControl){         
            if (typeof(media.length)=="undefined"){
                media = [media];
            }   
		    this.transportControl.sendTransportCommand(media, command, this.services.browser.serviceID);
        }
    }
    
    $.Browser.prototype.cancelConnect = function(){
        if (this.connected){
            this.connected = false;
            $.each(this.pendingRequests, function(i, req)
            {
                req.aborted = true;
                req.abort();
            });
        }            
        var browserContent = this.domElement.find("#BrowserContent");
        browserContent.children().remove();
        this.cachedTableData = {};
        this.totalChildCount = 0;
        this.setPagerButtonState();
        this.showLoading(false);      
        this.loaded = false;
    }
    
      $.Browser.prototype.connectBrowser = function() {
          if (this.servicesOpened){
                this.cancelConnect();
                this.showLoading(true);
                var thisObj = this;        
                
                
                var requestData = {aWidgetID:this.services.browser.serviceID};
                this.pendingRequests[this.pendingRequests.length] = $.JSONRequest.sendRequest(this.services.browser.serviceName,
                                          "Connected",
                                          requestData,
                                          function(responseData) {
                                              thisObj.onConnectBrowserResponse(this, responseData);                                      
                                              thisObj.pendingRequests.remove($.inArray(thisObj.pendingRequests,this));
                                          },
                                          function() {
                                              // retry on error
                                              thisObj.connectBrowser();
                                              thisObj.pendingRequests.remove($.inArray(thisObj.pendingRequests,this));
                                          });
          }
      }
    
    
      $.Browser.prototype.onConnectBrowserResponse = function(request, responseData) {
        if (!request.aborted){
              var thisObj = this;
              if (responseData.ErrorOccured){
                  this.onError();
              }else{
                  if (!responseData.Connected){
                    // retry connect
                    setTimeout(function(){
                        thisObj.connectBrowser();
                    },500);
                  }else{
                    this.connected = true;
                    this.getTotalChildCount();
                  }
              }
        }
      }
      
      
      $.Browser.prototype.getTotalChildCount = function() {
            var thisObj = this;
            var requestData = {aWidgetID:this.services.browser.serviceID};
            this.pendingRequests[this.pendingRequests.length] = $.JSONRequest.sendRequest(this.services.browser.serviceName,
                                      "ChildCount",
                                      requestData,
                                      function(responseData) {
                                          thisObj.onGetTotalChildCountResponse(this, responseData);
                                          thisObj.pendingRequests.remove($.inArray(thisObj.pendingRequests,this));
                                      },
                                      function() {
                                          // retry on error
                                          thisObj.getTotalChildCount();
                                          thisObj.pendingRequests.remove($.inArray(thisObj.pendingRequests,this));
                                      });   
     }
    
      $.Browser.prototype.onGetTotalChildCountResponse = function(request, responseData) {
        if (!request.aborted){
            if (responseData != null){
                if (responseData.ErrorOccured){
                    this.onError();
                }else{
                    this.loadedChildCount = 0;
                    this.totalChildCount = responseData.ChildCount;
                    this.setPagerButtonState();
                    this.setPageIndex(this.currentPageIndex);
                }
            }else{
                // retry on error
                this.getTotalChildCount();
            }
        }
      }

      $.Browser.prototype.setPagerButtonState = function(){
        var thisObj = this;
        var container = thisObj.domElement.find("#BrowserPager");
        container.empty();
        if (this.totalChildCount > this.pageSize){
            var pageCount =  Math.ceil(this.totalChildCount / this.pageSize);
            if (this.currentPageIndex > 0){
                container.append("<span id='" + (this.currentPageIndex - 1) + "'>Previous</span>");
            }
            var minIndex = Math.max(this.currentPageIndex - (this.pagerButtonCount / 2), 0);
            var maxIndex = Math.min(this.currentPageIndex + (this.pagerButtonCount / 2), pageCount - 1);
            for (var i=minIndex;i<=maxIndex;i++){
               container.append($("<span id='" + i + "'" + (i==this.currentPageIndex?"class='Selected'":"") + ">" + (i + 1) + "</span>"));
            }
            if (this.currentPageIndex < pageCount - 1){
                container.append("<span id='" + (this.currentPageIndex + 1) + "'>Next</span>");
            }
            container.find("span").click(function(){ 
                thisObj.setPageIndex($(this).attr("id"));
            });
            container.show();
        }else{
            container.hide();
        }
      }     
      
      $.Browser.prototype.setPageIndex = function(index){
        this.loadedChildCount = 0;
        var browserContent = this.domElement.find("#BrowserContent");
        browserContent.children().remove();
        this.cachedTableData = {};
        this.currentPageIndex = index * 1;
        this.showLoading(true);
        this.lazyLoadChildren(index * this.pageSize);
      }
      
    $.Browser.prototype.lazyLoadChildren = function(startIndex) {
            var thisObj = this;
            var requestCount = this.pageSize - this.loadedChildCount;
            if (this.totalChildCount < startIndex + requestCount) {
                requestCount = this.totalChildCount - startIndex;
            }
            if (requestCount > 0 && this.connected) {
                var requestData = { aWidgetID: this.services.browser.serviceID, aStartIndex: startIndex, aCount: requestCount };
                this.pendingRequests[this.pendingRequests.length] = $.JSONRequest.sendRequest(this.services.browser.serviceName,
                                          "GetChildren",
                                          requestData,
                                          function(responseData) {
                                              thisObj.onLazyLoadChildren(this, responseData, startIndex);
                                              thisObj.pendingRequests.remove($.inArray(thisObj.pendingRequests,this));
                                          },
                                          function() {
                                              // retry on error
                                              thisObj.lazyLoadChildren();
                                              thisObj.pendingRequests.remove($.inArray(thisObj.pendingRequests,this));
                                          });
            } else {
                this.onLazyLoadChildrenCompleted();            
            }
    }
    
    
    $.Browser.prototype.onLazyLoadChildrenCompleted = function() {  
            var thisObj = this;
            this.loaded = true;
            this.showLoading(false);       
            
            if (this.totalChildCount == 0){
                this.domElement.find("#BrowserContent").append(this.createBrowserItem("","No items found.",this.imageFolder + $.Browser.EStaticImages.kImageNoAlbumArt));
            }else{    

            // hack for android - bug with selection of items - shows context menu on clicking on BrowserItemBody if there is only one element in the list
            this.domElement.find("#BrowserContent").append("<div/>");

            this.domElement.find(".BrowserItem").hover(function(){
                $(".BrowserItem").removeClass("ui-state-hover");
				$(this).addClass("ui-state-hover");
            }, function(){
				$(this).removeClass("ui-state-hover");
            }).find("img").error(function(){
				$(this).attr("src", thisObj.imageFolder + $.Browser.EStaticImages.kImageIconAlbumError);
            });
            
            this.domElement.find('.BrowserItemBody, .BrowserItemOverlay').click(function(event){
                var parent = $(this).parent();
				var contextMenuOpen = $("#" + thisObj.contextMenuID).css("display").toLowerCase() != "none";
                if (!parent.hasClass("dragging") && event.button == 0 && !contextMenuOpen){    
		            thisObj.activate(parent);		
		        }
            });
            
            var selectString = this.mobileContextMenuView?".img":".BrowserItem";
                this.domElement.find(selectString).contextMenu({
                        menu: this.contextMenuID,
                        leftButton: this.mobileContextMenuView,
                        cancelBubble: this.mobileContextMenuView,
                        inSpeed: (this.mobileContextMenuView)?"0":null,
                        outSpeed: (this.mobileContextMenuView)?"0":null
                    },
                    function(action, el, pos) {
                        var callback;
                        
                        $.each(thisObj.getContextMenu(), function(i, item){
                            if (item.action == action){
                                callback = item.callback;
                                return;
                            }
                        });
                        if (callback) callback.call(this, action, el, pos);
                });
            
            }
            this.setWidth();
            this.getLocation();
            this.setPagerButtonState();
            window.scrollTo(0, 1);
    }
    
    $.Browser.prototype.setWidth = function(){    
            if (this.domElement.width()){
                this.domElement.find(".BrowserItem").width(this.domElement.width());
                var imgWidth = this.domElement.find(".img").outerWidth(true);
                var downArrowWidth = this.domElement.find(".BrowseDownImage").outerWidth(true);
                var titleWidth = this.domElement.width() - imgWidth - downArrowWidth - 5;
                this.domElement.find(".title").width(titleWidth);
            }
    }
    
    $.Browser.prototype.activate = function(element){
            var thisObj = this;
            var elementID = element.attr("ID").replace(/BrowserItem_/,"");             
            var rowData = thisObj.cachedTableData[elementID];
            var playNow = false;
            if (this.constructor === $.Browser){
                var didlLite = $.xml2json(rowData.DidlLite);
                if (didlLite.item){
                    playNow = true;
                }            
            }
            if (playNow){
                this.sendMenuTransportCommand(rowData, "PlayNow");
            }else{
                if (!thisObj.allowRepeatedActivation){
                    this.showLoading(true);
                }
                thisObj.domElement.trigger("evtLocationChanging");   
                var requestData = {aWidgetID:thisObj.services.browser.serviceID, aChild:rowData};
	                $.JSONRequest.sendRequest(thisObj.services.browser.serviceName,
                        "Activate",
                        requestData,
                        function(){},
                        function(){
                            thisObj.showLoading(false, true);
                        });
            }
    }
    
    $.Browser.prototype.onError = function(){
        this.cancelConnect();
        this.domElement.find("#BrowserContent").append(this.createBrowserItem("","An error occured browsing to this location.",""));      
        this.setWidth();
        this.getLocation();
        this.setPagerButtonState();
        window.scrollTo(0, 1);  
    }
    
    $.Browser.prototype.onLazyLoadChildren = function(request, responseData, startIndex) {    
        if (this.connected && !request.aborted){
            if (responseData.ErrorOccured){
                this.onError();    
            }else{
                var browserContent = this.domElement.find("#BrowserContent");
                var childCount = responseData.Children.length;
                var child;
                for (var i = 0; i < childCount; i++) {            
                    child = responseData.Children[i];
                    var didlLite = $.xml2json(child.DidlLite);
                    var existingItem = browserContent.find(".BrowserItem").eq(startIndex + i);
                    browserContent.append(this.getTableRow(didlLite, child.ID));
                    this.cachedTableData[child.ID] = child;
                }
                this.loadedChildCount += childCount;
                this.lazyLoadChildren(startIndex + this.loadedChildCount);
            }
        }
    }
    
    $.Browser.prototype.getTableRow = function(didlLite, id) {
    
        var content, title, albumArt;
        if (didlLite.container){        
            content = didlLite.container;
        }else if (didlLite.item){
            content = didlLite.item;
        }
        if (content){
            if (content.title){
                title = content.title;        
            }
            if (content.albumArtURI){
                albumArt = content.albumArtURI;
            }else if (content.icon){
                albumArt = content.icon;
            }
        }else{
            debug.log("Invalid didllite content: " + didlLite);
        }
        
        if (!albumArt){
            albumArt = this.getDefaultAlbumArt(didlLite);
        }
        
        return  this.createBrowserItem(id, title, albumArt, didlLite.container);
    }
    $.Browser.prototype.createBrowserItem = function(id, title, albumArt, isContainer) {  
        albumArt = "<img src=\"" + albumArt + "\" class=\"albumart\" />"        
        var imgLink = "<span class=\"img\" >" + albumArt + "</span>";
        if (!title){
            title="Unknown content";
        } 
        var titleLink = "<span unselectable='on' class='title'>" + title + "</span>";
        var browseImageLink = isContainer?"<span class='BrowseDownImage'/>":"";
        return  $("<div><div class='BrowserItem' id='BrowserItem_" + id + "'><div class='BrowserItemOverlay'/>" + imgLink + "<div class='BrowserItemBody'>" + titleLink + browseImageLink + "</div></div></div>").html();
    }

    $.Browser.prototype.closeServices = function(callback) {   
        this.cancelConnect();
        $.Browser.superclass.closeServices.call(this, callback);     
    }
    
    $.Browser.prototype.isContainer = function(didlLite){
        return didlLite.container;
    }
    $.Browser.prototype.getDefaultAlbumArt = function(didlLite){
        var albumArt = this.imageFolder;
        if (didlLite.container){ 
            if (didlLite.container["class"] == "object.container.album.musicAlbum"){
                albumArt += $.Browser.EStaticImages.kImageNoAlbumArt;
            }else if (didlLite.container["class"] == "object.container.person"){
                albumArt += $.Browser.EStaticImages.kImageIconArtist;
            }else if (didlLite.container["class"] == "object.container.playlistContainer"){
                albumArt += $.Browser.EStaticImages.kImageIconPlaylist;
            }else{
                albumArt += $.Browser.EStaticImages.kImageIconDirectory;
            }
        }else if (didlLite.item){            
            if (didlLite.item["class"] == "object.item.audioItem.audioBroadcast"){
                albumArt += $.Browser.EStaticImages.kImageIconRadio;
            }else if (didlLite.item["class"] == "object.item.videoItem"){
                albumArt += $.Browser.EStaticImages.kImageIconVideo;
            }else if (didlLite.item.Title == "Access denied"){
                albumArt += $.Browser.EStaticImages.kImageIconError;
            }else{
                albumArt += $.Browser.EStaticImages.kImageIconTrack;
            }
        }
        return albumArt;
    }
    
    
})(jQuery);
