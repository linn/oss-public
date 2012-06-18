(function($) {
    
    
    $.UpDirectory = function(domElement, container, browser) {
        if (domElement){
            this.init(domElement, container, browser);
        }
    }
    $.UpDirectory.prototype = new $.KinskyWidget;
    $.UpDirectory.prototype.constructor = $.UpDirectory;
    $.UpDirectory.superclass = $.KinskyWidget.prototype;

    $.UpDirectory.prototype.init = function(domElement, container, browser) {
        $.UpDirectory.superclass.init.call(this, domElement, container);
        this.browser = browser;
        var thisObj = this;
        browser.domElement.bind("evtLocationChanging", function (evt){
            thisObj.disableUpButton();
        });
        browser.domElement.bind("evtLocationChangingAborted", function (evt){
            thisObj.enableUpButton();
        });
        browser.domElement.bind("evtLocationChanged", function (evt, location, breadcrumbTrail){
            thisObj.onLocationChanged(location, breadcrumbTrail);
        });
    }
    
    $.UpDirectory.prototype.render = function() {        
        this.domElement.append('<div id="UpDirectoryButton" />');
        this.domElement.append('<div id="HomeButton" />');
        $.UpDirectory.superclass.render.call(this);        
    } 
    
    $.UpDirectory.prototype.closeServices = function(callback) {    
        this.disableUpButton();
        $.UpDirectory.superclass.closeServices.call(this, callback);     
    }
    
    $.UpDirectory.prototype.onLocationChanged = function(location, breadcrumbTrail){
        if (location.length > 1){
            this.enableUpButton();
        }else{            
            this.disableUpButton();
        }
        this.location = location;
    }
    
    $.UpDirectory.prototype.enableUpButton = function(){
        var thisObj = this;
        this.domElement.find("#UpDirectoryButton, #HomeButton").unbind("click").unbind("mouseenter").unbind("mouseleave").removeClass("Hover")      
        .hover(function(){             
            $(this).addClass("Hover");
        },function(){        
            $(this).removeClass("Hover");
        });
        this.domElement.find("#UpDirectoryButton").click(function(){    
            thisObj.browser.up(1);
        });
        this.domElement.find("#HomeButton").click(function(){                   
            thisObj.home();
        });
    }

    $.UpDirectory.prototype.home = function(){
        if (this.location.length > 1){
            this.browser.up(this.location.length - 1);
        }
    }
    
    $.UpDirectory.prototype.disableUpButton = function(){
        this.domElement.find("#UpDirectoryButton, #HomeButton").unbind("click").unbind("mouseenter").unbind("mouseleave").removeClass("Hover");
    }
    
})(jQuery);