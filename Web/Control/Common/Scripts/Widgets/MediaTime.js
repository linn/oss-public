var WidgetMediaTime = function(){}; 
WidgetMediaTime.inheritsFrom( WidgetBase );

WidgetMediaTime.prototype.Render = function(){
	var thisObj = this;
	this.superclass.Render.call(this);
	this.Container().Services().MediaTime.Variables().Seconds.AddListener(function(value){
		thisObj.iSeconds = value;
		thisObj.Refresh();
	});
	this.Container().Services().Info.Variables().Metadata.AddListener(function(value){
		var duration = new DidlLiteParser(value).Duration();
		thisObj.iDuration = duration;
		thisObj.Refresh();
	});
}


WidgetMediaTime.prototype.Refresh = function(){
	var seconds = this.iSeconds == 0?"":this.FormatTime(this.iSeconds);
	var duration = this.iDuration == 0?"":" / " + this.FormatTime(this.iDuration);
	if (seconds || duration){
		if (!seconds && duration){
			seconds = "00:00";
		}
		this.iDomElements.MediaTime.text(seconds + duration);	
	}else{
		this.iDomElements.MediaTime.text("00:00");	
	}
}


WidgetMediaTime.prototype.FormatTime = function(seconds) { 
	if (!isNaN(seconds)){
		var mins = Math.floor(seconds / 60) + "";
		if (mins.length == 1) {
			mins = "0" + mins;
		}
		if (isNaN(mins)){
			mins = "00";
		}
		var secs = Math.floor(seconds % 60) + "";
		if (secs.length == 1) {
			secs = "0" + secs;
		}
		if (isNaN(secs)){
			secs = "00";
		}
		return mins + ":" + secs;
	}else{
		return "0";
	}
}