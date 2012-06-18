var WidgetTransportControl = function(){
	var thisObj = this;
	
	this.iPlaylistIdArrayListener = function(value){
		thisObj.EvaluateButtonState();
	};
	
	this.iRadioIdArrayListener = function(value){
		for (var i=0;i<value.length && i <= WidgetTransportControl.kMaxRadioPresets;i++){
			if (value[i] != 0){
				thisObj.iLastValidRadioPresetIndex = i;
			}
		}
		thisObj.EvaluateButtonState();
	};
	
	this.iRadioTransportStateListener = function(value){
		if (thisObj.IsRadioSource()){
			thisObj.iTransportState = value;
			thisObj.EvaluateButtonState();
		}
	};
	
	this.iDsTransportStateListener = function(value){	
		if (thisObj.IsPlaylistSource()){
			thisObj.iTransportState = value;
			thisObj.EvaluateButtonState();
		}
	};
	
	this.iReceiverTransportStateListener = function(value){	
		if (thisObj.IsReceiverSource()){
			thisObj.iTransportState = value;
			thisObj.EvaluateButtonState();
		}
	};
	
	this.iRadioIdListener = function(value){	
		thisObj.EvaluateButtonState();
	};
	
	this.iDsTrackIdListener = function(value){	
		thisObj.EvaluateButtonState();
	};
}; 
WidgetTransportControl.inheritsFrom( WidgetBase );

WidgetTransportControl.kMaxRadioPresets = 100;

WidgetTransportControl.prototype.Render = function(){
	var thisObj = this;

	this.iDomElements.PreviousTrack.click(function(){
		if (thisObj.IsPlaylistSource()){			
			thisObj.Container().Services().Playlist.Previous(function(){
				thisObj.Container().GetServiceChanges(thisObj.Container().Services().Playlist);
				thisObj.Container().GetServiceChanges(thisObj.Container().Services().Info);
			});
		}else if (thisObj.IsRadioSource()){
			var idArray = thisObj.Container().Services().Radio.Variables().IdArray.Value();
			if (idArray && idArray.length){
				var currentID = thisObj.Container().Services().Radio.Variables().Id.Value();
				if (currentID){
					var position = idArray.indexOf(currentID);
					if (position > 0){							
						thisObj.SetRadioPreset(idArray[position-1]);
					}else{					
						thisObj.SetRadioPreset(idArray[idArray.length - 1]);
					}
				}
			}
		}
	});
	this.iDomElements.NextTrack.click(function(){
		if (thisObj.IsPlaylistSource()){			
			thisObj.Container().Services().Playlist.Next(function(){
				thisObj.Container().GetServiceChanges(thisObj.Container().Services().Playlist);
				thisObj.Container().GetServiceChanges(thisObj.Container().Services().Info);
			});
		}else if (thisObj.IsRadioSource()){
			var idArray = thisObj.Container().Services().Radio.Variables().IdArray.Value();
			if (idArray && idArray.length){
				var currentID = thisObj.Container().Services().Radio.Variables().Id.Value();
				if (currentID){
					var position = idArray.indexOf(currentID);
					if (position < idArray.length - 1){							
						thisObj.SetRadioPreset(idArray[position+1]);
					}else{					
						thisObj.SetRadioPreset(idArray[0]);
					}
				}
			}
		}
	});
	this.iDomElements.TransportButton.click(function(){
		if (thisObj.IsPlaylistSource()){
		    if (thisObj.iTransportState != ServicePlaylist.kTransportStatePlaying && thisObj.iTransportState != ServicePlaylist.kTransportStateBuffering) {
				thisObj.Container().Services().Playlist.Play(function(){
				    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Playlist);
				    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Info);
				});
			}else{
				thisObj.Container().Services().Playlist.Pause(function(){
				    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Playlist);
				    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Info);
				});
			}
		}else if (thisObj.IsRadioSource()){
			if (thisObj.iTransportState != ServiceRadio.kTransportStatePlaying && thisObj.iTransportState != ServiceRadio.kTransportStateBuffering){
				thisObj.Container().Services().Radio.Play(function(){
				    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Radio);
				    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Info);
				});
			}else{
				thisObj.Container().Services().Radio.Stop(function(){
				    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Radio);
				    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Info);
				});
			}
		}else if (thisObj.IsReceiverSource()){
			if (thisObj.iTransportState != ServiceReceiver.kTransportStatePlaying && thisObj.iTransportState != ServiceReceiver.kTransportStateBuffering && thisObj.iTransportState != ServiceReceiver.kTransportStateWaiting){
				thisObj.Container().Services().Receiver.Play(function(){
				    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Receiver);
				    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Info);
				});
			}else{
				thisObj.Container().Services().Receiver.Stop(function(){
				    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Receiver);
				    thisObj.Container().GetServiceChanges(thisObj.Container().Services().Info);
				});
			}
		}
	}); 
	this.Container().Services().Info.Variables().Metadata.AddListener(function(value){
		thisObj.EvaluateButtonState();
	});
}

WidgetTransportControl.prototype.SetRadioPreset = function(id){
	var thisObj = this;
	this.Container().Services().Radio.Read(id, function(result){
		var uri = new DidlLiteParser(result.Metadata).Uri();
		if (uri){
			var transportState = thisObj.iTransportState;
			thisObj.Container().Services().Radio.SetId(id, uri, function(){
				if (transportState == ServiceRadio.kTransportStatePlaying || 
					transportState == ServiceRadio.kTransportStateBuffering){ 
					thisObj.Container().Services().Radio.Play(function(){
						thisObj.Container().GetServiceChanges(thisObj.Container().Services().Radio);
						thisObj.Container().GetServiceChanges(thisObj.Container().Services().Info);
					});
				}else{
					thisObj.EvaluateButtonState();
					thisObj.Container().GetServiceChanges(thisObj.Container().Services().Radio);
					thisObj.Container().GetServiceChanges(thisObj.Container().Services().Info);
				}
			});
		}
	});
}

WidgetTransportControl.prototype.EvaluateButtonState = function(){
	var thisObj = this;
	this.iDomElements.TransportButton.toggleClass(ServicePlaylist.kTransportStatePlaying, false);
	this.iDomElements.TransportButton.toggleClass(ServicePlaylist.kTransportStatePaused, false);
	this.iDomElements.TransportButton.toggleClass(ServicePlaylist.kTransportStateStopped, false);
	this.iDomElements.TransportButton.toggleClass(ServicePlaylist.kTransportStateBuffering, false);	
	this.iDomElements.TransportButton.toggleClass(this.iTransportState, this.iTransportState && (this.IsRadioSource() || this.IsPlaylistSource() || this.IsReceiverSource()));
	if (this.IsPlaylistSource()){			
		var playlistService = this.Container().Services().Playlist;
		var idArray = playlistService.Variables().IdArray.Value();
		var id = playlistService.Variables().Id.Value();
		if (idArray && id){
			var index = idArray.indexOf(id);
			if (index != -1){
				this.iDomElements.TransportControlDisplay.text("Track " + (index + 1) + " of " + idArray.length);
			}else{
				this.iDomElements.TransportControlDisplay.text(idArray && idArray.length?"Select Track":"");
			}
		}else{
			this.iDomElements.TransportControlDisplay.text(idArray && idArray.length?"Select Track":"");
		}	        
        this.iDomElements.TransportButton.toggleClass("Pausable", true);
	}else if (this.IsRadioSource()){
		var radioService = this.Container().Services().Radio;
		var idArray = radioService.Variables().IdArray.Value();
		var id = radioService.Variables().Id.Value();
		if (idArray && id){
			var index = idArray.indexOf(id);
			if (index != -1){
				this.iDomElements.TransportControlDisplay.text("Station " + (index + 1) + " of " + (thisObj.iLastValidRadioPresetIndex + 1));
			}else{
				this.iDomElements.TransportControlDisplay.text(thisObj.iLastValidRadioPresetIndex?"Select Station":"");
			}
		}else{
			this.iDomElements.TransportControlDisplay.text(thisObj.iLastValidRadioPresetIndex?"Select Station":"");
		}
        this.iDomElements.TransportButton.toggleClass("Pausable", false);
	}else if (this.IsReceiverSource()){	
		this.Container().Services().Receiver.Sender(function(result){			
			thisObj.iDomElements.TransportControlDisplay.text(new DidlLiteParser(result.Metadata).Title());
		});
        this.iDomElements.TransportButton.toggleClass("Pausable", false);
	}else{
		this.iDomElements.TransportControlDisplay.text("");
	}
}

WidgetTransportControl.prototype.OnSourceChanged = function(){
	if (this.IsPlaylistSource()){			
		this.Container().Services().Playlist.Variables().IdArray.AddListener(this.iPlaylistIdArrayListener);
		this.Container().Services().Playlist.Variables().Id.AddListener(this.iDsTrackIdListener);
		this.Container().Services().Playlist.Variables().TransportState.AddListener(this.iDsTransportStateListener);
		this.Container().Services().Radio.Variables().IdArray.RemoveListener(this.iRadioIdArrayListener);
		this.Container().Services().Radio.Variables().Id.RemoveListener(this.iRadioIdListener);
		this.Container().Services().Radio.Variables().TransportState.RemoveListener(this.iRadioTransportStateListener);
		this.Container().Services().Receiver.Variables().TransportState.RemoveListener(this.iReceiverTransportStateListener);
		this.iTransportState = this.Container().Services().Playlist.Variables().TransportState.Value();
	}else if (this.IsRadioSource()){
		this.Container().Services().Playlist.Variables().IdArray.RemoveListener(this.iPlaylistIdArrayListener);
		this.Container().Services().Playlist.Variables().Id.RemoveListener(this.iDsTrackIdListener);
		this.Container().Services().Playlist.Variables().TransportState.RemoveListener(this.iDsTransportStateListener);
		this.Container().Services().Radio.Variables().IdArray.AddListener(this.iRadioIdArrayListener);
		this.Container().Services().Radio.Variables().Id.AddListener(this.iRadioIdListener);
		this.Container().Services().Radio.Variables().TransportState.AddListener(this.iRadioTransportStateListener);
		this.Container().Services().Receiver.Variables().TransportState.RemoveListener(this.iReceiverTransportStateListener);
		this.iTransportState = this.Container().Services().Radio.Variables().TransportState.Value();
	}else if (this.IsReceiverSource()){
		this.Container().Services().Playlist.Variables().IdArray.RemoveListener(this.iPlaylistIdArrayListener);
		this.Container().Services().Playlist.Variables().Id.RemoveListener(this.iDsTrackIdListener);
		this.Container().Services().Playlist.Variables().TransportState.RemoveListener(this.iDsTransportStateListener);
		this.Container().Services().Radio.Variables().IdArray.RemoveListener(this.iRadioIdArrayListener);
		this.Container().Services().Radio.Variables().Id.RemoveListener(this.iRadioIdListener);
		this.Container().Services().Radio.Variables().TransportState.RemoveListener(this.iRadioTransportStateListener);
		this.Container().Services().Receiver.Variables().TransportState.AddListener(this.iReceiverTransportStateListener);
		this.iTransportState = this.Container().Services().Receiver.Variables().TransportState.Value();
	}else{
		this.Container().Services().Playlist.Variables().IdArray.RemoveListener(this.iPlaylistIdArrayListener);
		this.Container().Services().Playlist.Variables().Id.RemoveListener(this.iDsTrackIdListener);
		this.Container().Services().Playlist.Variables().TransportState.RemoveListener(this.iDsTransportStateListener);
		this.Container().Services().Radio.Variables().IdArray.RemoveListener(this.iRadioIdArrayListener);
		this.Container().Services().Radio.Variables().Id.RemoveListener(this.iRadioIdListener);
		this.Container().Services().Radio.Variables().TransportState.RemoveListener(this.iRadioTransportStateListener);
		this.Container().Services().Receiver.Variables().TransportState.RemoveListener(this.iReceiverTransportStateListener);
		this.iTransportState = null;
	}
	this.EvaluateButtonState();
}
