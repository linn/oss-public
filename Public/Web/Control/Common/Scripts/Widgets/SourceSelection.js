var WidgetSourceSelection = function(){
	this.iSources = [];
}; 
WidgetSourceSelection.inheritsFrom( WidgetPagedList );


WidgetSourceSelection.prototype.Render = function(){
	var thisObj = this;
	this.superclass.Render.call(this);
	this.Container().Services().Product.Variables().SourceXml.AddListener(function(value){
		var sourceList = jQuery.xml2json(value);
		var items = [];
		if (!sourceList.Source instanceof Array){
			sourceList.Source = [sourceList.Source];
		}
		thisObj.iSources = sourceList.Source;
		for (var i=0;i<thisObj.iSources.length;i++){
			if (thisObj.iSources[i].Visible == "true"){
				items[items.length] = thisObj.iSources[i];
			}
		}
		thisObj.SetContent(items);
		thisObj.SourceIndexChanged();
	});	
	this.Container().Services().Product.Variables().SourceIndex.AddListener(function(value){
		thisObj.iSourceIndex = value;
		thisObj.SourceIndexChanged();
	});
}

WidgetSourceSelection.prototype.OnItemsLoaded = function(){
	this.SourceIndexChanged();
}

WidgetSourceSelection.prototype.SourceIndexChanged = function(){
	if (typeof this.iDomElements.CurrentSourceDisplay != "undefined"){
		if (this.iSourceIndex < this.iSources.length){
			this.iDomElements.CurrentSourceDisplay.text(this.iSources[this.iSourceIndex].Name);
		}
	}
	var counter = 0;
	var sourceIndex;
	for (var i=0;i<this.iSources.length;i++){
		if (i == this.iSourceIndex){
			sourceIndex = counter;
		}
		if (this.iSources[i].Visible == "true"){
			counter++;
		}
	}
	this.iDomElements.ItemsContainer.find(".ListItem").toggleClass("Active",false);
	this.iDomElements.ItemsContainer.find("#" + sourceIndex).toggleClass("Active", true);
}

WidgetSourceSelection.prototype.OnItemClick = function (aItem) {
    var thisObj = this;
    var itemIndex = jQuery(aItem).attr("id") * 1;
    var item = this.iItems[itemIndex];
    var sourceIndex = this.iSources.indexOf(item);
    if (sourceIndex >= 0) {
        this.Container().Services().Product.SetSourceIndex(sourceIndex, function () {
            thisObj.Container().GetServiceChanges(thisObj.Container().Services().Product);
        });
    }
    this.iDomElements.ItemsContainer.trigger("evtItemSelected");
}

WidgetSourceSelection.prototype.CreateItemHtml = function(aItemContent){
	var sourceType = aItemContent.Type;
	var imgLink = "<img src='" + this.GetSourceImageLink(sourceType) + "'/>";
	var contentLink = "<span>" + aItemContent.Name + "</span>";
	return imgLink + contentLink;
}
