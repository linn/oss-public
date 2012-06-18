(function($) {

    $.SourceSelectorMobile = function(domElement, container, imageFolder) {   
        if (domElement){
            this.init(domElement, container, imageFolder);
        }
    }
    $.SourceSelectorMobile.prototype = new $.SourceSelector;
    $.SourceSelectorMobile.prototype.constructor = $.SourceSelectorMobile;
    $.SourceSelectorMobile.superclass = $.SourceSelector.prototype;  
    
    $.SourceSelectorMobile.EStaticImages = {
        kImageAuxSource : "AuxSource.png",
        kImageCdSource : "CD.png",
        kImageDsSource : "DS.png",
        kImageRadioSource : "Radio.png",
        kImageUpnpAvSource : "UPNP.png",
        kImageSpdifSource : "Spdif.png",
        kImageTosLinkSource : "TosLink.png"
    }  
    
    $.SourceSelectorMobile.prototype.init = function(domElement, container, imageFolder) {   
        $.SourceSelectorMobile.superclass.init.call(this, domElement, container);
        this.imageFolder = imageFolder;
    }
    
    $.SourceSelectorMobile.prototype.render = function() {
        var thisObj = this;
        var toggleViewButton = $('<div id="SourceSelectorToggleView"><img id="SourceSelectorToggleViewImage"/><span id="SourceSelectorToggleViewText"/></div>');
        var panel = $('<div id="SourceSelectorPanel"/>');
        var cancel = $('<div id="SourceSelectorCancel">Close</div>');
        panel.append(cancel);
        cancel.click(function(e){
		    thisObj.hideSourceSelection();
		});   
        panel.append($('<div id="SourceSelectorSourceList" />'));            
		this.domElement.append(toggleViewButton);
		toggleViewButton.click(function(e){
		    thisObj.showSourceSelection();
		});
        this.domElement.append(panel);          
		this.hideSourceSelection();
    }

    $.SourceSelectorMobile.prototype.hideSourceSelection = function(){
        this.domElement.find("#SourceSelectorPanel").hide();
        this.domElement.find("#SourceSelectorToggleView").show();
        this.domElement.trigger("evtHideSourceSelection");
    }
    
    $.SourceSelectorMobile.prototype.showSourceSelection = function(){   
        if (this.domElement.find("#SourceSelectorSourceList").children().length > 0){
            this.domElement.find("#SourceSelectorPanel").show();
            this.domElement.find("#SourceSelectorToggleView").hide();
            this.domElement.trigger("evtShowSourceSelection");
        }
    }
    
    $.SourceSelectorMobile.prototype.onGetCurrentSource = function(responseData){
		var thisObj = this;
        var toggleViewButton = thisObj.domElement.find("#SourceSelectorToggleView");
        if (this.servicesOpened){
            var sourceList = this.domElement.find("#SourceSelectorSourceList");
            if (responseData.Name != this.source){
                this.source = responseData.Name;       
                this.sourceType = responseData.Type;       
                this.domElement.trigger("evtSourceSelectorSourceChanged", this);             
            }
                //select current source
				$.each(sourceList.find(".ListItem"), function(i, el){
				    if ($(this).find(".ListItemBody").text() == thisObj.source){
				        $(this).addClass("selected");
				    }else{
				        $(this).removeClass("selected");
				    }
				});
				
                toggleViewButton.find("#SourceSelectorToggleViewText").text(this.source);
                toggleViewButton.find("#SourceSelectorToggleViewImage").attr("src", this.getSourceImageLink(this.sourceType));
                toggleViewButton.show();
        }else{            
                toggleViewButton.hide();
        }
    }
    $.SourceSelectorMobile.prototype.getSourceImageLink = function(sourceType){
        var imageFile = "";
        switch (sourceType){
            case $.SourceSelector.ESourceType.ePlaylist: {
                imageFile += $.SourceSelectorMobile.EStaticImages.kImageDsSource;
                break;
            }
            case $.SourceSelector.ESourceType.eRadio:
            case $.SourceSelector.ESourceType.eTuner: {
                imageFile += $.SourceSelectorMobile.EStaticImages.kImageRadioSource;
                break;
            }
            case $.SourceSelector.ESourceType.eDisc: {
                imageFile += $.SourceSelectorMobile.EStaticImages.kImageCdSource;
                break;
            }
            case $.SourceSelector.ESourceType.eUpnpAv: {
                imageFile += $.SourceSelectorMobile.EStaticImages.kImageUpnpAvSource;
                break;
            }
            case $.SourceSelector.ESourceType.eToslink: {
                imageFile += $.SourceSelectorMobile.EStaticImages.kImageTosLinkSource;
                break;
            }
            case $.SourceSelector.ESourceType.eSpdif: {
                imageFile += $.SourceSelectorMobile.EStaticImages.kImageSpdifSource;
                break;
            }
            default: {
                imageFile += $.SourceSelectorMobile.EStaticImages.kImageAuxSource;       
            }
       }
       return this.imageFolder + imageFile;
    }
    
    $.SourceSelectorMobile.prototype.onGetSourceList = function(responseData){
		var thisObj = this;
        if (this.servicesOpened){
            var sourceList = this.domElement.find("#SourceSelectorSourceList");            
    		
		    // get current selection
            var currentSelected = this.source;
            
            //clear existing values
            sourceList.empty();
				
            
            //add new values
            var found = false;
		    $.each(responseData, function(i, value){
			    if (value.Name==currentSelected){
			        found = true;
			    }
		    });
            if (!found || !currentSelected){
		        this.source = null;
	        }
		    $.each(responseData, function(i, value){	
                    var newElement = $(thisObj.createItem(value.Name, i));
                    newElement.find(".ListItemImage").attr("src", thisObj.getSourceImageLink(value.Type));
			        sourceList.append(newElement);
			        newElement.click(function(e){
			            if (thisObj.source != value.Name){
			                thisObj.changeSource(value.Name);
			            }
			            thisObj.hideSourceSelection();
	                });
	                if (value.Name == thisObj.source){
	                    newElement.addClass("selected");
	                }
                    if (thisObj.domElement.width()){
                        newElement.css("max-width", thisObj.domElement.width() + "px");
                    }		
		    });
            // get the source
            this.getCurrentSource();
        }
    }
    $.SourceSelectorMobile.prototype.createItem = function(value, id) {
		    return  $("<div><div class='ListItem' id='ListItem_" + id + "'><img class='ListItemImage'/><span class='ListItemBody'>" + value + "</span></div></div>").html();    
	}
	
    $.SourceSelectorMobile.prototype.closeServices = function(callback) {
        this.source = null;
        this.room = null;
        var sourceList = this.domElement.find("#SourceSelectorSourceList");
        sourceList.empty();
        this.domElement.find("#SourceSelectorToggleView").hide();
        $.KinskyWidget.prototype.closeServices.call(this, callback);    
    }
    
    $.SourceSelectorMobile.prototype.openServices = function(room, callback) {
        this.room = room;
        this.source = null;
		if (!this.room){
            var sourceList = this.domElement.find("#SourceSelectorSourceList");
            sourceList.empty();
        }else{
			$.KinskyWidget.prototype.openServices.call(this, callback);    
        }
    }

})(jQuery);