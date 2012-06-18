(function($) {
    
    
    $.Breadcrumb = function(domElement, container, browser) {
        if (domElement){
            this.init(domElement, container, browser);
        }
    }
    $.Breadcrumb.prototype = new $.KinskyWidget;
    $.Breadcrumb.prototype.constructor = $.Breadcrumb;
    $.Breadcrumb.superclass = $.KinskyWidget.prototype;

    $.Breadcrumb.prototype.init = function(domElement, container, browser) {
        $.Breadcrumb.superclass.init.call(this, domElement, container);
        this.browser = browser;
        var thisObj = this;
        this.locationChanging = false;
        browser.domElement.bind("evtLocationChanging", function (evt){
            thisObj.locationChanging = true;
        });
        browser.domElement.bind("evtLocationChangingAborted", function (evt){
            thisObj.locationChanging = false;
        });
        browser.domElement.bind("evtLocationChanged", function (evt, location, breadcrumbTrail){
            thisObj.locationChanging = false;
            thisObj.onLocationChanged(location, breadcrumbTrail);
        });
    }
    
    $.Breadcrumb.prototype.render = function() {        
        this.domElement.append('<ul id="BreadcrumbContainer"/>');
        $.Breadcrumb.superclass.render.call(this);        
    } 
    
    $.Breadcrumb.prototype.onLocationChanged = function(location, breadcrumbTrail){
        var container = this.domElement.find("#BreadcrumbContainer")
        container.empty();
        var thisObj = this;
        $.each(breadcrumbTrail, function(i, text){
            var item = $("<li class='BreadcrumbItem'>" + text + ((i == breadcrumbTrail.length - 1)?"":" >") + "</li>");
            container.append(item);
            if (i != breadcrumbTrail.length - 1){
                item.click(function(){
                   if (!thisObj.locationChanging){
                    thisObj.browser.up(breadcrumbTrail.length - i - 1);
                   }
                });
            }
        });
        if (this.domElement.css("max-width")){
            var maxWidth = this.domElement.css("max-width").replace(/px/i,"");
            var totalWidth = 0;
            var removeList = [];
            for (var i=breadcrumbTrail.length - 1;i>=0;i--){
                var current = this.domElement.find("#BreadcrumbContainer li:eq(" + i + ")");
                totalWidth += current.outerWidth();
                if (totalWidth>=maxWidth){
                    removeList[removeList.length] = current;
                }
            }
            $.each(removeList, function(){
                this.remove();
            });
        }
    }
    
    $.Breadcrumb.prototype.closeServices = function(callback) {    
        this.domElement.find("#BreadcrumbContainer").empty();
        $.Breadcrumb.superclass.closeServices.call(this, callback);     
    }
    
})(jQuery);