var WidgetVolumeControl = function(aMinIncrement, aMaxIncrement, aVerticalSlider){
	this.iMinIncrement = aMinIncrement?aMinIncrement:WidgetVolumeControl.kMinVolumeIncrement;
	this.iMaxIncrement = aMaxIncrement?aMaxIncrement:WidgetVolumeControl.kMaxVolumeIncrement;
}; 
WidgetVolumeControl.inheritsFrom( WidgetBase );

WidgetVolumeControl.kDefaultVolumeIncrementSmall = 1;
WidgetVolumeControl.kDefaultVolumeIncrementLarge = 5;
WidgetVolumeControl.kMinVolumeIncrement = 1;
WidgetVolumeControl.kMaxVolumeIncrement = 10;
	
WidgetVolumeControl.prototype.Render = function(){
	var thisObj = this;
	WidgetVolumeControl.prototype.superclass.Render.call(this);
	
	this.Container().Services().Volume.Variables().Mute.AddListener(function(value){
		thisObj.UpdateButtonState();
	});
	this.Container().Services().Volume.Variables().Volume.AddListener(function(value){
		thisObj.UpdateButtonState();				
	});
    this.Container().Services().Volume.Variables().VolumeLimit.AddListener(function (value) {
		thisObj.UpdateButtonState();
		thisObj.Container().Services().Volume.Variables().VolumeLimit.Listeners().clear();
	});
	
	this.iDomElements.VolumeMute.click(function(){
	    thisObj.Container().Services().Volume.SetMute(!thisObj.Container().Services().Volume.Variables().Mute.Value(), function () {
	        thisObj.Container().GetServiceChanges(thisObj.Container().Services().Volume);
		});
	});
	
	if (isIPad){
		this.iDomElements.VolumeUp.append(jQuery("<img src=\"Images/VolumeUp.png\"></img>"));
		this.iDomElements.VolumeDown.append(jQuery("<img src=\"Images/VolumeDown.png\"></img>"));
	}
	
	if (isAndroid){
		jQuery("<span id=\"VolumeUpSmall\"></span>").insertBefore(this.iDomElements.VolumeUp).click(function(aEvent){
		    var currentVolume = thisObj.Container().Services().Volume.Variables().Volume.Value();
		    var maxVolume = thisObj.Container().Services().Volume.Variables().VolumeLimit.Value();		
			currentVolume = Math.min(currentVolume + WidgetVolumeControl.kDefaultVolumeIncrementSmall, maxVolume);
			thisObj.iDomElements.VolumeDisplayTooltip.text("+" + WidgetVolumeControl.kDefaultVolumeIncrementSmall).show();
			thisObj.Container().Services().Volume.SetVolume(currentVolume, function () {
			    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Volume);
				setTimeout(function(){					
					thisObj.iDomElements.VolumeDisplayTooltip.hide();
				},1000);
			});
		}).toggleClass("Android", true);
		this.iDomElements.VolumeUp.click(function(aEvent){
		    var currentVolume = thisObj.Container().Services().Volume.Variables().Volume.Value();
		    var maxVolume = thisObj.Container().Services().Volume.Variables().VolumeLimit.Value();	
			thisObj.iDomElements.VolumeDisplayTooltip.text("+" + WidgetVolumeControl.kDefaultVolumeIncrementLarge).show();
			currentVolume = Math.min(currentVolume + WidgetVolumeControl.kDefaultVolumeIncrementLarge, maxVolume);
			thisObj.Container().Services().Volume.SetVolume(currentVolume, function () {
			    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Volume);
				setTimeout(function(){					
					thisObj.iDomElements.VolumeDisplayTooltip.hide();
				},1000);
			});
		}).toggleClass("Android", true);
		
		jQuery("<span id=\"VolumeDownSmall\"></span>").insertAfter(this.iDomElements.VolumeDown).click(function(aEvent){
		    var currentVolume = thisObj.Container().Services().Volume.Variables().Volume.Value();
		    var maxVolume = thisObj.Container().Services().Volume.Variables().VolumeLimit.Value();	
			thisObj.iDomElements.VolumeDisplayTooltip.text("-" + WidgetVolumeControl.kDefaultVolumeIncrementSmall).show();
			currentVolume = Math.max(currentVolume - WidgetVolumeControl.kDefaultVolumeIncrementSmall, 0);
			thisObj.Container().Services().Volume.SetVolume(currentVolume, function () {
			    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Volume);
				setTimeout(function(){					
					thisObj.iDomElements.VolumeDisplayTooltip.hide();
				},1000);
			});
		}).toggleClass("Android", true);
		this.iDomElements.VolumeDown.click(function(aEvent){
		    var currentVolume = thisObj.Container().Services().Volume.Variables().Volume.Value();
			thisObj.iDomElements.VolumeDisplayTooltip.text("-" + WidgetVolumeControl.kDefaultVolumeIncrementLarge).show();
			currentVolume = Math.max(currentVolume - WidgetVolumeControl.kDefaultVolumeIncrementLarge, 0);
			thisObj.Container().Services().Volume.SetVolume(currentVolume, function () {
			    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Volume);
				setTimeout(function(){					
					thisObj.iDomElements.VolumeDisplayTooltip.hide();
				},1000);
			});
		}).toggleClass("Android", true);
	}else{
		if (hasTouchSupport){
			this.iDomElements.VolumeUp[0].addEventListener("touchstart", function(aEvent){
				thisObj.OnVolumeMouseMove(aEvent.targetTouches[0].clientX, aEvent.targetTouches[0].clientY, aEvent);
				aEvent.preventDefault();
			}, false);
			
			this.iDomElements.VolumeUp[0].addEventListener("touchmove", function(aEvent){
				thisObj.OnVolumeMouseMove(aEvent.targetTouches[0].clientX, aEvent.targetTouches[0].clientY, aEvent);
				aEvent.preventDefault();
			}, false);
			
			this.iDomElements.VolumeUp[0].addEventListener("touchend", function(aEvent){
			    var currentVolume = thisObj.Container().Services().Volume.Variables().Volume.Value();
			    var maxVolume = thisObj.Container().Services().Volume.Variables().VolumeLimit.Value();
				if (thisObj.iCurrentVolumeIncrement){
					var newVolume = Math.min(Math.max(currentVolume + thisObj.iCurrentVolumeIncrement, 0), maxVolume);
					thisObj.Container().Services().Volume.SetVolume(newVolume, function () {
					    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Volume);
					});		
				}
				thisObj.iDomElements.VolumeDisplayTooltip.hide();
			}, false);
			
			this.iDomElements.VolumeDown[0].addEventListener("touchstart", function(aEvent){	
				thisObj.OnVolumeMouseMove(aEvent.targetTouches[0].clientX, aEvent.targetTouches[0].clientY, aEvent);	
				aEvent.preventDefault();
			}, false);

			this.iDomElements.VolumeDown[0].addEventListener("touchmove", function(aEvent){
				thisObj.OnVolumeMouseMove(aEvent.targetTouches[0].clientX, aEvent.targetTouches[0].clientY, aEvent);
				aEvent.preventDefault();
			}, false);

			this.iDomElements.VolumeDown[0].addEventListener("touchend", function(aEvent){
			    var currentVolume = thisObj.Container().Services().Volume.Variables().Volume.Value();
			    var maxVolume = thisObj.Container().Services().Volume.Variables().VolumeLimit.Value();
				if (thisObj.iCurrentVolumeIncrement){
					var newVolume = Math.min(Math.max(currentVolume + thisObj.iCurrentVolumeIncrement, 0), maxVolume);
					thisObj.Container().Services().Volume.SetVolume(newVolume, function () {
					    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Volume);
					});		
				}
				thisObj.iDomElements.VolumeDisplayTooltip.hide();
			}, false);
			
		}else{	
			this.iDomElements.VolumeUp.bind("mousedown",function(aEvent){
				thisObj.OnVolumeMouseMove(aEvent.clientX, aEvent.clientY, aEvent);
			}).bind("mousemove",function(aEvent){	
					thisObj.OnVolumeMouseMove(aEvent.clientX, aEvent.clientY, aEvent);
			}).bind("mouseup",function(aEvent){
			    var currentVolume = thisObj.Container().Services().Volume.Variables().Volume.Value();
			    var maxVolume = thisObj.Container().Services().Volume.Variables().VolumeLimit.Value();
				if (thisObj.iCurrentVolumeIncrement){
					var newVolume = Math.min(Math.max(currentVolume + thisObj.iCurrentVolumeIncrement, 0), maxVolume);
					thisObj.Container().Services().Volume.SetVolume(newVolume, function () {
					    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Volume);
					});		
				}
				thisObj.iDomElements.VolumeDisplayTooltip.hide();
			}).bind("mouseout",function(aEvent){		
				thisObj.iDomElements.VolumeDisplayTooltip.hide();
			});
			
			this.iDomElements.VolumeDown.bind("mousedown",function(aEvent){
				thisObj.OnVolumeMouseMove(aEvent.clientX, aEvent.clientY, aEvent);
			}).bind("mousemove",function(aEvent){	
				thisObj.OnVolumeMouseMove(aEvent.clientX, aEvent.clientY, aEvent);
			}).bind("mouseup",function(aEvent){
			    var currentVolume = thisObj.Container().Services().Volume.Variables().Volume.Value();
			    var maxVolume = thisObj.Container().Services().Volume.Variables().VolumeLimit.Value();
				if (thisObj.iCurrentVolumeIncrement){
					var newVolume = Math.min(Math.max(currentVolume + thisObj.iCurrentVolumeIncrement, 0), maxVolume);
					thisObj.Container().Services().Volume.SetVolume(newVolume, function () {
					    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Volume);
					});		
				}
				thisObj.iDomElements.VolumeDisplayTooltip.hide();
			}).bind("mouseout",function(aEvent){	
				thisObj.iDomElements.VolumeDisplayTooltip.text("").hide();
			});
		}
	}	

}

WidgetVolumeControl.prototype.OnVolumeMouseMove = function(aClientX, aClientY, aEvent){
		var upElement = this.iDomElements.VolumeUp[0];
		var downElement = this.iDomElements.VolumeDown[0];
		if (!this.OnVolumeMouseMoveElement(upElement, aClientX, aClientY, aEvent, true)){
			if (!this.OnVolumeMouseMoveElement(downElement, aClientX, aClientY, aEvent, false)){	
				this.iCurrentVolumeIncrement = 0;
				this.iDomElements.VolumeDisplayTooltip.text("").hide();
			}
		}
}

WidgetVolumeControl.prototype.OnVolumeMouseMoveElement = function(aElement, aClientX, aClientY, aEvent, increment){
		var offset = this.GetOffset(aElement);
		var height = jQuery(aElement).height();
		var width = jQuery(aElement).width();
		var offsetY = aClientY - offset.t;
		var offsetX = aClientX - offset.l;
		// bounds test
		if (offsetY >= 0 && offsetY <= height && offsetX >= 0 && offsetX <= width){
			var distance = 	increment?offsetX:width - offsetX;
			var multiplier = distance / width;
			var exponentiationFactor = isAndroid?1:2;
			if (exponentiationFactor){
				multiplier = Math.pow(multiplier, exponentiationFactor);
			}
			this.iCurrentVolumeIncrement = Math.round(this.iMinIncrement + ((this.iMaxIncrement - this.iMinIncrement) * multiplier));
			if (!increment){
				this.iCurrentVolumeIncrement = this.iCurrentVolumeIncrement * -1;
			}
			this.iDomElements.VolumeDisplayTooltip.text((this.iCurrentVolumeIncrement > 0?"+":"") + (this.iCurrentVolumeIncrement?this.iCurrentVolumeIncrement:"")).show();
			return true;
		}else{
			return false;
		}
}

WidgetVolumeControl.prototype.UpdateButtonState = function(){
    var mute = this.Container().Services().Volume.Variables().Mute.Value();
	this.iDomElements.VolumeUp.toggleClass("Mute",mute);
	this.iDomElements.VolumeDown.toggleClass("Mute",mute);
	this.iDomElements.VolumeMute.toggleClass("Mute",mute);
	this.iDomElements.VolumeDisplay.toggleClass("Mute",mute);
	this.iDomElements.VolumeDisplay.text(this.Container().Services().Volume.Variables().Volume.Value());
}


WidgetVolumeControl.prototype.GetOffset = function(aElement){
	for(var r = {l: aElement.offsetLeft, t: aElement.offsetTop, r: aElement.offsetWidth, b: aElement.offsetHeight};
		aElement = aElement.offsetParent; r.l += aElement.offsetLeft, r.t += aElement.offsetTop);
	return r.r += r.l, r.b += r.t, r;
}

WidgetVolumeControl.prototype.UpdateVolumeService = function(){
	var thisObj = this;
	this.Container().Services().Volume.Variables().Mute.Listeners().clear();
	this.Container().Services().Volume.Variables().Volume.Listeners().clear();
	this.Container().Services().Volume.Variables().VolumeLimit.Listeners().clear();
	
	this.Container().Services().Volume.Variables().Mute.AddListener(function(value){
		thisObj.UpdateButtonState();
	});
	this.Container().Services().Volume.Variables().Volume.AddListener(function(value){
		thisObj.UpdateButtonState();				
	});
    this.Container().Services().Volume.Variables().VolumeLimit.AddListener(function (value) {
		thisObj.UpdateButtonState();
		thisObj.Container().Services().Volume.Variables().VolumeLimit.Listeners().clear();
	});
	this.UpdateButtonState();
}