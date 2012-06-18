var WidgetBase = function(){
	this.iName = null;
	this.iDomElements = {};
	this.iContainer = null;
	this.iSource = null;
}

WidgetBase.EStaticImages = {
	kImageAuxSource : "Images/AuxSource.png",
	kImageCdSource : "Images/CD.png",
	kImagePlaylistSource: "Images/PlaylistSource.png",
	kImageRadioSource : "Images/Radio.png",
	kImageUpnpAvSource : "Images/UPNP.png",
	kImageSpdifSource : "Images/Spdif.png",
	kImageTosLinkSource : "Images/TosLink.png",
    kImageReceiverSource : "Images/Receiver.png"
}

WidgetBase.ESourceType = {
	eUnknown : "Unknown",
	ePlaylist : "Playlist",
	eRadio : "Radio",
	eUpnpAv : "UpnpAv",
	eAnalog : "Analog",
	eSpdif : "Spdif",
	eDisc : "Disc",
	eTuner : "Tuner",
	eAux : "Aux",
	eToslink: "Toslink",
    eReceiver: "Receiver"
}

WidgetBase.prototype.Name = function(){	
	return this.iName;
}

WidgetBase.prototype.SetName = function(aName){	
	this.iName = aName;
}

WidgetBase.prototype.DomElements = function(){	
	return this.iDomElements;
}

WidgetBase.prototype.SetDomElements = function(aDomElements){	
	this.iDomElements = aDomElements;
}

WidgetBase.prototype.Container = function(){	
	return this.iContainer;
}

WidgetBase.prototype.SetContainer = function(aContainer){	
	this.iContainer = aContainer;
}

WidgetBase.prototype.Source = function(){
	return this.iSource;
}

WidgetBase.prototype.SetSource = function(aSource){
	var sourceChanged = (aSource != this.iSource);
	this.iSource = aSource;
	if (sourceChanged){
		this.OnSourceChanged();
	}
}

WidgetBase.prototype.OnSourceChanged = function(){}

WidgetBase.prototype.Render = function(){
	debug.log("WidgetBase::Render");
}

WidgetBase.prototype.IsPlaylistSource = function(){
	return this.iSource && this.iSource.Type == "Playlist";
}

WidgetBase.prototype.IsRadioSource = function(){
	return this.iSource && this.iSource.Type == "Radio";
}

WidgetBase.prototype.IsReceiverSource = function(){
	return this.iSource && this.iSource.Type == "Receiver";
}

WidgetBase.prototype.GetSourceImageLink = function (aSourceType) {
    var imageFile;
    switch (aSourceType) {
        case WidgetBase.ESourceType.ePlaylist:
            {
                imageFile = WidgetBase.EStaticImages.kImagePlaylistSource;
                break;
            }
        case WidgetBase.ESourceType.eRadio:
        case WidgetBase.ESourceType.eTuner:
            {
                imageFile = WidgetBase.EStaticImages.kImageRadioSource;
                break;
            }
        case WidgetBase.ESourceType.eDisc:
            {
                imageFile = WidgetBase.EStaticImages.kImageCdSource;
                break;
            }
        case WidgetBase.ESourceType.eUpnpAv:
            {
                imageFile = WidgetBase.EStaticImages.kImageUpnpAvSource;
                break;
            }
        case WidgetBase.ESourceType.eToslink:
            {
                imageFile = WidgetBase.EStaticImages.kImageTosLinkSource;
                break;
            }
        case WidgetBase.ESourceType.eSpdif:
            {
                imageFile = WidgetBase.EStaticImages.kImageSpdifSource;
                break;
            }
        case WidgetBase.ESourceType.eReceiver:
            {
                imageFile = WidgetBase.EStaticImages.kImageReceiverSource;
                break;
            }
        default:
            {
                imageFile = WidgetBase.EStaticImages.kImageAuxSource;
            }
    }
    return imageFile;
}
