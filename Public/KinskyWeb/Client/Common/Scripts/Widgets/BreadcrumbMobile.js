(function($) {
    
    
    $.BreadcrumbMobile = function(domElement, container, browser) {
        if (domElement){
            this.init(domElement, container, browser);
        }
    }
    $.BreadcrumbMobile.prototype = new $.Breadcrumb;
    $.BreadcrumbMobile.prototype.constructor = $.BreadcrumbMobile;
    $.BreadcrumbMobile.superclass = $.Breadcrumb.prototype;

    
    $.BreadcrumbMobile.prototype.render = function() {       
		var thisObj = this;
        var toggleViewButton = $('<div id="BreadcrumbToggleView"><span id="BreadcrumbToggleViewText"/></div>');
        var panel = $('<div id="BreadcrumbPanel"/>');
        panel.append($('<div id="BreadcrumbItemList" />'));    
        this.domElement.append(toggleViewButton);
		toggleViewButton.click(function(e){
		    thisObj.showBreadcrumbSelection();
		});
        this.domElement.append(panel);     
		this.hideBreadcrumbSelection();     
    } 
     $.BreadcrumbMobile.prototype.hideBreadcrumbSelection = function(){
        this.domElement.find("#BreadcrumbPanel").hide();
        this.domElement.find("#BreadcrumbToggleView").show();
        this.domElement.trigger("evtHideBreadcrumbSelection");
    }
    
    $.BreadcrumbMobile.prototype.showBreadcrumbSelection = function(){    
        this.domElement.find("#BreadcrumbPanel").show();
        this.domElement.find("#BreadcrumbToggleView").hide();
        this.domElement.trigger("evtShowBreadcrumbSelection");
    }
    
    $.BreadcrumbMobile.prototype.createItem = function(value, id) {
		    return  $("<div><div class='ListItem' id='ListItem_" + id + "'><span class='ListItemBody'>" + value + "</span></div></div>").html();    
	}
	
    $.BreadcrumbMobile.prototype.onLocationChanged = function(location, breadcrumbTrail){
        var thisObj = this;
        var container = this.domElement.find("#BreadcrumbItemList");
		container.empty();
        $.each(breadcrumbTrail, function(i, text){	
            var newElement = $(thisObj.createItem(text, i));
            var position = breadcrumbTrail.length - i - 1;
            newElement.click(function(e){
                if (position != 0){
                    thisObj.browser.up(position);
                }
                thisObj.hideBreadcrumbSelection();
            });
            container.append(newElement);
        });
        this.domElement.find("#BreadcrumbToggleViewText").text(breadcrumbTrail[breadcrumbTrail.length - 1]);
        var toggleViewButton = this.domElement.find("#BreadcrumbToggleView");
        toggleViewButton.unbind("click");
        if (breadcrumbTrail.length > 1){
            toggleViewButton.click(function(e){
		        thisObj.showBreadcrumbSelection();
		    });
        }
    }
    
    $.BreadcrumbMobile.prototype.closeServices = function(callback) {    
        this.domElement.find("#BreadcrumbItemList").empty();
        $.BreadcrumbMobile.superclass.closeServices.call(this, callback);     
    }
    
})(jQuery);