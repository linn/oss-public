function WidgetContainer(aServiceList, aWidgetList){
	this.iWidgets = {};
	for (var i=0;i<aServiceList.length;i++){
		var service = eval("new Service" + aServiceList[i].Name + "('" + aServiceList[i].ServiceID + "');");
		this.AddService(service);
	} 
	for (var i=0;i<aWidgetList.length;i++){
		var widget = eval("new " + aWidgetList[i].Class + "();");	
		widget.SetName(aWidgetList[i].Name);
		widget.SetDomElements(aWidgetList[i].DomElements);
		widget.SetContainer(this);	
		this.AddWidget(widget);
	}
	return this;
}
WidgetContainer.inheritsFrom( ServiceCollection );

WidgetContainer.prototype.AddWidget = function(aWidget){
	this.iWidgets[aWidget.Name()] = aWidget;
}

WidgetContainer.prototype.RemoveWidget = function(aWidget){
	delete this.iWidgets[aWidget.Name()];
}

WidgetContainer.prototype.Widgets = function(){
	return this.iWidgets;
}

WidgetContainer.prototype.WidgetNames = function(){
	var result = [];
	for (var widget in this.iWidgets){
		if (this.iWidgets.hasOwnProperty(widget)){
			result[result.length] = widget;
		}
	}
	return result;
}

WidgetContainer.prototype.Start = function(){
	var widgets = this.WidgetNames();
	for (var i=0;i<widgets.length;i++){
		this.iWidgets[widgets[i]].Render();
	}
	this.superclass.Start.call(this);
}