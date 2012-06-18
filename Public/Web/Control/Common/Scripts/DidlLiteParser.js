var DidlLiteParser = function(aDidlLite){
	this.iDidlLite = jQuery.xml2json(aDidlLite);
}

DidlLiteParser.EStaticImages = {
	kImageNoAlbumArt: "Images/NoAlbumArt.png",
	kImageIconAlbum : "Images/Album.png",
	kImageIconAlbumError  : "Images/AlbumError.png",
	kImageIconArtist  : "Images/Artist.png",
	kImageIconDirectory  : "Images/Directory.png",
	kImageIconError  : "Images/Error.png",
	kImageIconPlaylist  : "Images/Playlist.png",
	kImageIconRadio  : "Images/Radio.png",
	kImageIconServer  : "Images/Library.png",
	kImageIconStar  : "Images/Star.png",
	kImageIconTrack  : "Images/Track.png",
	kImageIconVideo  : "Images/Video.png"
}   


DidlLiteParser.prototype.Title = function(){
	var content;
	if (this.iDidlLite.container){        
		content = this.iDidlLite.container;
	}else if (this.iDidlLite.item){
		content = this.iDidlLite.item;
	}
	if (content && content.title){
		return content.title.toString();
	}else{
		return "";
	}
}

DidlLiteParser.prototype.Artist = function(){
	var content;
	if (this.iDidlLite.container){        
		content = this.iDidlLite.container;
	}else if (this.iDidlLite.item){
		content = this.iDidlLite.item;
	}
	if (content && content.artist){
		return content.artist.toString();
	}else{
		return "";
	}
}

DidlLiteParser.prototype.Album = function(){
	var content;
	if (this.iDidlLite.container){        
		content = this.iDidlLite.container;
	}else if (this.iDidlLite.item){
		content = this.iDidlLite.item;
	}
	if (content && content.album){
		return content.album.toString();
	}else{
		return "";
	}
}

DidlLiteParser.prototype.Uri = function(){
	var content;
	if (this.iDidlLite.container){        
		content = this.iDidlLite.container;
	}else if (this.iDidlLite.item){
		content = this.iDidlLite.item;
	}
	if (content && content.res && content.res.text){
		return content.res.text;
	}else if (content && content.res && content.res instanceof Array && content.res.length){
		for (var i=0;i<content.res.length;i++){
			if (content.res[i].text){
				return content.res[i].text;
			}	
		}
		return null;
	}else{
		return null;
	}
}

DidlLiteParser.prototype.Duration = function(){
	var content;
	if (this.iDidlLite.container){        
		content = this.iDidlLite.container;
	}else if (this.iDidlLite.item){
		content = this.iDidlLite.item;
	}
	if (content && content.res && content.res.duration){
		var timeArray = content.res.duration.split(":");
		return (timeArray[0] * 3600) + (timeArray[1] * 60) + (timeArray[2] * 1);
	}else if (content && content.res && content.res instanceof Array && content.res.length){
		for (var i=0;i<content.res.length;i++){
			if (content.res[i].duration){
				var timeArray = content.res[i].duration.split(":");
				return (timeArray[0] * 3600) + (timeArray[1] * 60) + (timeArray[2] * 1);
			}
		}
		return 0;
	}else{
		return 0;
	}
}

DidlLiteParser.prototype.Artwork = function(){
	var artwork, content;
	if (this.iDidlLite.container){        
		content = this.iDidlLite.container;
	}else if (this.iDidlLite.item){
		content = this.iDidlLite.item;
	}
	if (content){
		if (content.albumArtURI){
			artwork = content.albumArtURI.toString();
		}else if (content.icon){
			artwork = content.icon.toString();
		}else{
			if (this.iDidlLite.container){ 
				if (this.iDidlLite.container["class"] == "object.container.album.musicAlbum"){
					artwork = DidlLiteParser.EStaticImages.kImageNoAlbumArt;
				}else if (this.iDidlLite.container["class"] == "object.container.person"){
					artwork = DidlLiteParser.EStaticImages.kImageIconArtist;
				}else if (this.iDidlLite.container["class"] == "object.container.playlistContainer"){
					artwork = DidlLiteParser.EStaticImages.kImageIconPlaylist;
				}else{
					artwork = DidlLiteParser.EStaticImages.kImageIconDirectory;
				}
			}else if (this.iDidlLite.item){            
				if (this.iDidlLite.item["class"] == "object.item.audioItem.audioBroadcast"){
					artwork = DidlLiteParser.EStaticImages.kImageIconRadio;
				}else if (this.iDidlLite.item["class"] == "object.item.videoItem"){
					artwork = DidlLiteParser.EStaticImages.kImageIconVideo;
				}else if (this.iDidlLite.item.title == "Access denied"){
					artwork = DidlLiteParser.EStaticImages.kImageIconError;
				}else{
					artwork = DidlLiteParser.EStaticImages.kImageIconTrack;
				}
			}
		}
	}else{
		artwork = DidlLiteParser.EStaticImages.kImageNoAlbumArt;
	}
	return artwork;
}

DidlLiteParser.prototype.IsContainer = function(){
	return this.iDidlLite.container;
}
