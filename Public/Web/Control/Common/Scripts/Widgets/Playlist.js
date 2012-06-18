var WidgetPlaylist = function(){
	this.iPlaylistItems = [];
	this.iRadioItems = [];
	var thisObj = this;
	this.iPlaylistIdArrayListener = function(value){
		var idList = "";
		var idArray = value;
		for (var i=0;i<idArray.length;i++){
			idList += ((idList.length > 0) ? " " : "") + idArray[i];
		}
		thisObj.Container().Services().Playlist.ReadList(idList, function(result){
			var metaDataList = jQuery.xml2json(result.TrackList);
			var items = [];
			if (metaDataList.Entry){
				items = metaDataList.Entry;
			}
			thisObj.iPlaylistItems = items;
		},function(message){
			debug.log("error reading playlist: " + message);
		});
		if (thisObj.IsPlaylistSource()){
			thisObj.Refresh();
		}
	};
	
	this.iRadioIdArrayListener = function(value){
		var idList = "";
		var idArray = value;
		for (var i=0;i<idArray.length;i++){
			idList += ((idList.length > 0) ? " " : "") + idArray[i];
		}
		thisObj.Container().Services().Radio.ReadList(idList, function(result){
			var metaDataList = jQuery.xml2json(result.ChannelList);
			if (!metaDataList.Entry instanceof Array){
				metaDataList.Entry = [metaDataList.Entry];
			}
			var itemsLookup = {};
			var items = [];					
			for (var i=0;i<metaDataList.Entry.length;i++){
				itemsLookup[metaDataList.Entry[i].Id] = metaDataList.Entry[i];
			}
			var lastPresetIndex;
			for (var i=0;i<idArray.length;i++){
				if (idArray[i] > 0){
					items[items.length] = itemsLookup[idArray[i]];
					lastPresetIndex = i;
				}else{
					items[items.length] = null;
				}
			}
			if (lastPresetIndex < items.length - 1){
				items.splice(lastPresetIndex + 1, items.length - lastPresetIndex - 1);
			}
			thisObj.iRadioItems = items;
		},function(message){
			debug.log("error reading playlist: " + message);
		});
		if (thisObj.IsRadioSource()){
			thisObj.Refresh();
		}
	};
	
	this.iPlaylistRepeatListener = function(value){
		if (thisObj.IsPlaylistSource()){
			thisObj.iDomElements.Repeat.toggleClass("Active", value);
		}
	};
	
	this.iPlaylistShuffleListener = function(value){
		if (thisObj.IsPlaylistSource()){
			thisObj.iDomElements.Shuffle.toggleClass("Active", value);
		}
	};
	
	this.iTrackIdListener = function(value){	
		thisObj.SetCurrentItem();
	};
	
	this.iRadioIdListener = function(value){	
		thisObj.SetCurrentItem();
	};
};
WidgetPlaylist.inheritsFrom( WidgetPagedList );
	
WidgetPlaylist.prototype.Render = function(){
	var thisObj = this;	
	this.superclass.Render.call(this);
		
	this.iDomElements.Repeat.click(function(){
		if (thisObj.IsPlaylistSource()){
			thisObj.Container().Services().Playlist.SetRepeat(!jQuery(this).hasClass("Active"), function(){				
					thisObj.Container().GetServiceChanges(thisObj.Container().Services().Playlist);
			});
		}
	});
	this.iDomElements.Shuffle.click(function(){
		if (thisObj.IsPlaylistSource()){
			thisObj.Container().Services().Playlist.SetShuffle(!jQuery(this).hasClass("Active"), function(){				
					thisObj.Container().GetServiceChanges(thisObj.Container().Services().Playlist);
			});
		}
	});
	this.OnSourceChanged();
}

WidgetPlaylist.prototype.OnSourceChanged = function(){
	this.Refresh();
	if (this.IsPlaylistSource()){
		this.Container().Services().Radio.Variables().IdArray.RemoveListener(this.iRadioIdArrayListener);
		this.Container().Services().Radio.Variables().Id.RemoveListener(this.iRadioIdListener);
		this.Container().Services().Playlist.Variables().IdArray.AddListener(this.iPlaylistIdArrayListener);
		this.Container().Services().Playlist.Variables().Repeat.AddListener(this.iPlaylistRepeatListener);
		this.Container().Services().Playlist.Variables().Shuffle.AddListener(this.iPlaylistShuffleListener);
		this.Container().Services().Playlist.Variables().Id.AddListener(this.iTrackIdListener);
		this.iDomElements.Repeat.toggleClass("Active", this.Container().Services().Playlist.Variables().Repeat.Value()).toggleClass("Hidden", false);
		this.iDomElements.Shuffle.toggleClass("Active", this.Container().Services().Playlist.Variables().Shuffle.Value()).toggleClass("Hidden", false);
	}else if (this.IsRadioSource()){
		this.Container().Services().Radio.Variables().IdArray.AddListener(this.iRadioIdArrayListener);
		this.Container().Services().Radio.Variables().Id.AddListener(this.iRadioIdListener);
		this.Container().Services().Playlist.Variables().IdArray.RemoveListener(this.iPlaylistIdArrayListener);
		this.Container().Services().Playlist.Variables().Repeat.RemoveListener(this.iPlaylistRepeatListener);
		this.Container().Services().Playlist.Variables().Shuffle.RemoveListener(this.iPlaylistShuffleListener);
		this.Container().Services().Playlist.Variables().Id.RemoveListener(this.iTrackIdListener);
		this.iDomElements.Repeat.toggleClass("Hidden", true);
		this.iDomElements.Shuffle.toggleClass("Hidden", true);
	}else{	
		this.Container().Services().Radio.Variables().IdArray.RemoveListener(this.iRadioIdArrayListener);
		this.Container().Services().Radio.Variables().Id.RemoveListener(this.iRadioIdListener);
		this.Container().Services().Playlist.Variables().IdArray.RemoveListener(this.iPlaylistIdArrayListener);
		this.Container().Services().Playlist.Variables().Repeat.RemoveListener(this.iPlaylistRepeatListener);
		this.Container().Services().Playlist.Variables().Shuffle.RemoveListener(this.iPlaylistShuffleListener);
		this.Container().Services().Playlist.Variables().Id.RemoveListener(this.iTrackIdListener);
		this.iDomElements.Repeat.toggleClass("Hidden", true);
		this.iDomElements.Shuffle.toggleClass("Hidden", true);
	}
}

WidgetPlaylist.prototype.Refresh = function(){
	var thisObj = this;
	if (this.IsPlaylistSource()){
		this.SetContent(this.iPlaylistItems);
    }else if (this.IsRadioSource()){
        this.SetContent(this.iRadioItems);
    }else{
        this.SetContent([]);
    }
}

WidgetPlaylist.prototype.LoadItems = function(aIndex, aCount){
	WidgetPlaylist.prototype.superclass.LoadItems.call(this, aIndex, aCount);	
}

WidgetPlaylist.prototype.OnItemsLoaded = function(){
	this.SetCurrentItem();	
	this.iDomElements.ItemsContainer.find("*").toggleClass("Radio", this.IsRadioSource());
	this.iDomElements.ItemsContainer.find(".ListItem img").error(function(){
		jQuery(this).attr("src", DidlLiteParser.EStaticImages.kImageNoAlbumArt);
    });
}

WidgetPlaylist.prototype.OnItemClick = function(aItem){
	var thisObj = this;
	var item = this.iItems[jQuery(aItem).attr("ID")];
	if (this.IsPlaylistSource()){
		if (item){ 
			this.Container().Services().Playlist.SeekId(item.Id, function(){
				thisObj.Container().Services().Playlist.Play(function(){								
					thisObj.Container().GetServiceChanges(thisObj.Container().Services().Playlist);
					thisObj.Container().GetServiceChanges(thisObj.Container().Services().Info);
				});
			});
		}
	}else if (this.IsRadioSource()){
		if (item){
			var uri = new DidlLiteParser(item.Metadata).Uri();
			if (uri){
				this.Container().Services().Radio.SetId(item.Id, uri, function(){
					thisObj.Container().Services().Radio.Play(function(){
						thisObj.Container().GetServiceChanges(thisObj.Container().Services().Radio);
						thisObj.Container().GetServiceChanges(thisObj.Container().Services().Info);
					});
				});
			}
		}
	}
	this.iDomElements.ItemsContainer.trigger("evtItemSelected");
}

WidgetPlaylist.prototype.SetCurrentItem = function() {
	this.iDomElements.ItemsContainer.find(".ListItem").toggleClass("Active", false);
	if (this.IsPlaylistSource()){			
		var playlistService = this.Container().Services().Playlist;
		var idArray = playlistService.Variables().IdArray.Value();
		var id = playlistService.Variables().Id.Value();
		if (idArray && id){
			var index = idArray.indexOf(id);
			this.iDomElements.ItemsContainer.find("#" + index).toggleClass("Active", true);
		}
	}else if (this.IsRadioSource()){
		var radioService = this.Container().Services().Radio;
		var idArray = radioService.Variables().IdArray.Value();
		var id = radioService.Variables().Id.Value();
		if (idArray && id){
			var index = idArray.indexOf(id);
			this.iDomElements.ItemsContainer.find("#" + index).toggleClass("Active", true);
		}
	}
}




WidgetPlaylist.prototype.CreateItemHtml = function(aItemContent, aIndex) {
	var artwork, title, album, artist, duration;
	if (aItemContent){
	var parser = new DidlLiteParser(aItemContent.Metadata);	
		artwork = parser.Artwork();
		title = parser.Title();
		album = parser.Album();
		artist = parser.Artist();		
		duration = parser.Duration();
		if (duration){
			duration = this.FormatTime(duration);
		}else{
			duration = "";
		}
	}else{
		artwork = DidlLiteParser.EStaticImages.kImageNoAlbumArt;
		title = "Empty";
	}
	var albumArt = "<img src=\"" + artwork + "\" />"        
	var imgLink = "<div class=\"PlaylistArtwork\" >" + albumArt + "</div>";
	
	var containerStart = "<div class='PlaylistContainer'>";
	var containerEnd = "</div>";
	var titleLink = "<div unselectable='on' class='PlaylistTitle'>" + title + "</div>";
	var positionLink = "<div unselectable='on' class='PlaylistPosition'>" + (aIndex + 1) + ".</div>";
	var artistLink = "";
	if (artist){
		artistLink = "<div unselectable='on' class='PlaylistArtist'>" + artist + "</div>";
	}
	var albumLink = "";
	if (album){
		albumLink = "<div unselectable='on' class='PlaylistArtist'>" + album + "</div>";
	}
	var durationLink = "";
	if (duration){
		durationLink = "<div unselectable='on' class='PlaylistArtist'>" + duration + "</div>";
	}
	return imgLink + containerStart + positionLink + titleLink + artistLink + albumLink + durationLink + containerEnd;
}


WidgetPlaylist.prototype.FormatTime = function(seconds) { 
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
