var WidgetTrack = function(){}; 
WidgetTrack.inheritsFrom( WidgetBase );

WidgetTrack.prototype.Render = function(){
	var thisObj = this;
	WidgetTrack.prototype.superclass.Render();
	this.Container().Services().Info.Variables().Metadata.AddListener(function(value){
		thisObj.UpdateState();
	});
	this.Container().Services().Info.Variables().BitRate.AddListener(function(value){
		thisObj.UpdateState();
	});
	this.Container().Services().Info.Variables().SampleRate.AddListener(function(value){
		thisObj.UpdateState();
	});
	this.Container().Services().Info.Variables().BitDepth.AddListener(function(value){
		thisObj.UpdateState();
	});
	this.Container().Services().Info.Variables().CodecName.AddListener(function(value){
		thisObj.UpdateState();
	});
}
WidgetTrack.prototype.OnSourceChanged = function(){
	this.UpdateState();
}

WidgetTrack.prototype.UpdateState = function(){
		var parser = new DidlLiteParser(this.Container().Services().Info.Variables().Metadata.Value());
		this.DomElements().AlbumArt.attr("src", parser.Artwork()).error(function(){
			jQuery(this).attr("src", DidlLiteParser.EStaticImages.kImageNoAlbumArt);
		});
		var info = [];
		var title = parser.Title();
		if (title){
			info.push(title);
		}
		var artist = parser.Artist();
		if (artist){
			info.push(artist);
		}
		var album = parser.Album();
		if (album){
			info.push(album);
		}
		var bitRate = this.Container().Services().Info.Variables().BitRate.Value();
		if (bitRate){
			info.push (Math.round(bitRate / 1000) + " kbps");
		}
		var sampleRate = this.Container().Services().Info.Variables().SampleRate.Value();
		if (sampleRate){
			sampleRate = Math.round(sampleRate / 1000) + " kHz"
		}
		var bitDepth = this.Container().Services().Info.Variables().BitDepth.Value();
		var lossless = this.Container().Services().Info.Variables().Lossless.Value();
		if (bitDepth && lossless){
				sampleRate += (sampleRate ? " / " : "") + bitDepth + " bits";
		}		
		if (sampleRate){
			info.push (sampleRate);
		}
		
		var codec = this.Container().Services().Info.Variables().CodecName.Value();
		if (codec && codec != ""){
			info.push (codec);
		}
		
		this.DomElements().Title.text(info.length?info[0]:"");
		this.DomElements().Artist.text(info.length>1?info[1]:"");
		this.DomElements().Album.text(info.length>2?info[2]:"");
		this.DomElements().BitRate.text(info.length>3?info[3]:"");
		this.DomElements().SampleRate.text(info.length>4?info[4]:"");
		this.DomElements().Codec.text(info.length>5?info[5]:"");
	
}