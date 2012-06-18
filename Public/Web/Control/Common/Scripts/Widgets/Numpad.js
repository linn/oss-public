var WidgetNumpad = function(){}; 
WidgetNumpad.inheritsFrom( WidgetBase );

WidgetNumpad.prototype.Render = function(){
	var thisObj = this;
	this.iDomElements.Container.children().click(function(){
		var number = parseInt(jQuery(this).attr("ID"),10);
		if (!isNaN(number)){
			thisObj.iDomElements.Display.text(thisObj.iDomElements.Display.text() + number);
		}
	});
	this.iDomElements.Enter.click(function(){
		if (thisObj.IsPlaylistSource()){			
			var text = thisObj.iDomElements.Display.text();
			var isJukebox = text.length && text[0] == "0";
			var trackId = parseInt(thisObj.iDomElements.Display.text(),10);
			if (!isNaN(trackId) && trackId > 0 && !isJukebox){			
				var idArray = thisObj.Container().Services().Playlist.Variables().IdArray.Value();
				if (idArray && idArray.length && idArray.length >= trackId && idArray[trackId - 1] != 0){					
					thisObj.Container().Services().Playlist.SeekId(idArray[trackId - 1], function(){						
						var transportState = thisObj.Container().Services().Playlist.Variables().TransportState.Value();
						if (transportState != ServiceRadio.kTransportStatePlaying && 
							transportState != ServiceRadio.kTransportStateBuffering){							
							thisObj.Container().Services().Playlist.Play(function(){								
								thisObj.Container().GetServiceChanges(thisObj.Container().Services().Playlist);
								thisObj.Container().GetServiceChanges(thisObj.Container().Services().Info);
							});
						}
					});
				}
			}else if (!isNaN(trackId) && trackId > 0){
				thisObj.Container().Services().Jukebox.SetCurrentPreset(trackId, function(){					
					thisObj.Container().GetServiceChanges(thisObj.Container().Services().Info);
				});
			}
		}else if (thisObj.IsRadioSource()){
			var presetId = parseInt(thisObj.iDomElements.Display.text(),10);
			if (!isNaN(presetId) && presetId > 0){
				var idArray = thisObj.Container().Services().Radio.Variables().IdArray.Value();
				if (idArray && idArray.length && idArray.length >= presetId && idArray[presetId - 1] != 0){
					thisObj.SetRadioPreset(idArray[presetId - 1]);
				}
			}
		}
		jQuery(this).trigger("evtNumpadEnterClick");
		thisObj.iDomElements.Display.text("");
	});
	this.iDomElements.Clear.click(function(){
		thisObj.iDomElements.Display.text("");
	});
}

WidgetNumpad.prototype.SetRadioPreset = function(id){
	var thisObj = this;
	this.Container().Services().Radio.Read(id, function(result){
		var uri = new DidlLiteParser(result.Metadata).Uri();
		if (uri){
			thisObj.Container().Services().Radio.SetId(id, uri, function(){
				thisObj.Container().Services().Radio.Play(function(){
					thisObj.Container().GetServiceChanges(thisObj.Container().Services().Radio);
					thisObj.Container().GetServiceChanges(thisObj.Container().Services().Info);
				});
			});
		}
	});
}

WidgetNumpad.prototype.Clear = function(){
	this.iDomElements.Display.text("");
}