var kPagedListDefaultPageSize = 100;
var kPagedListDefaultPagerButtonCount = 6;

var WidgetPagedList = function(){
	this.iCurrentPageIndex = 0;
	this.iItems = [];
	this.iPageSize = kPagedListDefaultPageSize;
	this.iPagerButtonCount = kPagedListDefaultPagerButtonCount;
	this.iVisible = false;
}; 
WidgetPagedList.inheritsFrom( WidgetBase );

WidgetPagedList.prototype.SetPageSize = function(aPageSize){	
	this.iPageSize = aPageSize;	
}

WidgetPagedList.prototype.SetPagerButtonCount = function(aPagerButtonCount){	
	this.iPagerButtonCount = aPagerButtonCount;
}

WidgetPagedList.prototype.Render = function(){	
	var thisObj = this;
	if (typeof this.iDomElements.PreviousPage != "undefined"){
		this.iDomElements.PreviousPage.click(function(){
			if (jQuery(this).hasClass("Active")){
				thisObj.SetPageIndex(thisObj.iCurrentPageIndex - 1);
			}
		});
	}
	if (typeof this.iDomElements.NextPage != "undefined"){
		this.iDomElements.NextPage.click(function(){
			if (jQuery(this).hasClass("Active")){
				thisObj.SetPageIndex(thisObj.iCurrentPageIndex + 1);
			}
		});
	}
}

WidgetPagedList.prototype.SetContent = function(aItems){
	this.iItems = (aItems && !aItems instanceof Array)?[aItems]:aItems;
	if (this.iCurrentPageIndex * this.iPageSize > aItems.length){
		this.SetPageIndex(0);
	}else{
		this.SetPageIndex(this.iCurrentPageIndex);
	}
}

WidgetPagedList.prototype.SetPageIndex = function(index){
	this.iCurrentPageIndex = index * 1;
	this.LoadItems(index * this.iPageSize, this.iPageSize);
	this.RefreshPagingControls();
}

WidgetPagedList.prototype.RefreshPagingControls = function(){
	var thisObj = this;
	var pageCount =  Math.ceil(this.iItems.length / this.iPageSize);
	if (typeof this.iDomElements.PreviousPage != "undefined"){
		this.iDomElements.PreviousPage.toggleClass("Active",this.iCurrentPageIndex > 0);
		this.iDomElements.PreviousPage.toggleClass("Inactive", pageCount <= 1);
	}
	if (typeof this.iDomElements.NextPage != "undefined"){
		this.iDomElements.NextPage.toggleClass("Active",this.iCurrentPageIndex < pageCount - 1);
		this.iDomElements.NextPage.toggleClass("Inactive", pageCount <= 1);
	}
	var container = this.iDomElements.PageLinksContainer;
	if (typeof container != "undefined"){
		container.empty();
		if (this.iItems.length > this.iPageSize){		
			if (typeof container != "undefined"){
				var minIndex = Math.max(this.iCurrentPageIndex - (this.iPagerButtonCount / 2), 0);
				var maxIndex = Math.min(this.iCurrentPageIndex + (this.iPagerButtonCount / 2), pageCount - 1);
				for (var i=minIndex;i<=maxIndex;i++){
				   container.append(jQuery("<span id='" + i + "'" + (i==this.iCurrentPageIndex?"class='Active'":"") + ">" + (i + 1) + "</span>"));
				}
				container.find("span").click(function(){ 
					thisObj.SetPageIndex(jQuery(this).attr("id"));
				});
				container.show();
			}
		}else{
			container.hide();
		}
	}
	var pageCountDisplay = this.iDomElements.PageCountDisplay;
	if (typeof pageCountDisplay != "undefined"){
		pageCountDisplay.text("Page " + (this.iCurrentPageIndex + 1) + " of " + pageCount);
	}
}  

WidgetPagedList.prototype.LoadItems = function(aIndex, aCount){
	var thisObj = this;
	this.iDomElements.ItemsContainer.empty();
	if (this.iVisible){
		setTimeout(function(){
			if (thisObj.iItems && thisObj.iItems.length){
				for (var i=aIndex;i<thisObj.iItems.length && i<(aIndex + aCount);i++){
					thisObj.iDomElements.ItemsContainer.append(jQuery("<div id='" + i + "' class='ListItem'>"+thisObj.CreateItemHtml(thisObj.iItems[i], i)+"</div>"));
				}
			}else{
				thisObj.iDomElements.ItemsContainer.append(jQuery("<div class='ListItem'>"+thisObj.CreateEmptyItemHtml(i)+"</div>"));
			}			
			thisObj.iDomElements.ItemsContainer.find(".ListItem").click(function(){
				thisObj.OnItemClick(this);
			})
			.find("*").toggleClass("IPhone", isIPhone)
			.find("*").toggleClass("IPad", isIPad);
			thisObj.OnItemsLoaded();
		},100);
	}
}
WidgetPagedList.prototype.OnItemsLoaded = function(){

}

WidgetPagedList.prototype.OnItemClick = function(aItem) {}

WidgetPagedList.prototype.CreateItemHtml = function(aItemContent, aIndex){
	return aItemContent.toString();
}

WidgetPagedList.prototype.CreateEmptyItemHtml = function(aIndex){
	return "No items found.";
}

WidgetPagedList.prototype.Show = function(){
	this.iVisible = true;
	this.iCurrentPageIndex = 0;
	this.Refresh();
}
WidgetPagedList.prototype.Hide = function(){
	this.iVisible = false;
	this.Refresh();
}

WidgetPagedList.prototype.Refresh = function(){
	this.SetContent(this.iItems);
}