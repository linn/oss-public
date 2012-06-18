// ie6 fix
try{document.execCommand('BackgroundImageCache', false, true);}catch(e){};

Array.prototype.remove = function(from, to) {
  var rest = this.slice((to || from) + 1 || this.length);
  this.length = from < 0 ? this.length + from : from;
  return this.push.apply(this, rest);
};


if (typeof console == "undefined") {
    window["console"] = {"log":function(){}};
}

var debugConsoleEnabled = /debug=true/i.test(location.search);

var debug = {
    log : function(message){
        if (debugConsoleEnabled){
            var debugConsole = jQuery("#DebugConsole");
            if (debugConsole){
                debugConsole.prepend("<p>" + message + "</p>");
                debugConsole.find("p:gt(100)").remove();
            }
        }
        console.log(message);
    },
    stop: function(){
        debugConsoleEnabled = false;
    },
    start: function(){
        debugConsoleEnabled = true;
    }
}

var hasTouchSupport = (function(){
    if("createTouch" in document){ // True on the iPhone
	    return true;
	}
	try{
	    var event = document.createEvent("TouchEvent"); // Should throw an error if not supported
		return !!event.initTouchEvent; // Check for existance of initialization method
	}catch(error){
	    return false;
	}
}());

var isIPhone = false;
if (navigator && navigator.userAgent){
    var agent = navigator.userAgent.toLowerCase();
    isIPhone = (/ipod/.test(agent) || 
               /iphone/.test(agent));
}
