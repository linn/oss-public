var WidgetStandby = function(){}; 
WidgetStandby.inheritsFrom( WidgetBase );
	
WidgetStandby.prototype.Render = function(){
	var thisObj = this;
	this.Container().Services().Product.Variables().Standby.AddListener(function(value){
		thisObj.iDomElements.StandbyButton.toggleClass("Active", !value);
	});
	this.iDomElements.StandbyButton.click(function(){
		var productService = thisObj.Container().Services().Product;
		productService.SetStandby(!productService.Variables().Standby.Value(), function(){
			thisObj.Container().GetServiceChanges(thisObj.Container().Services().Product);
		}); 
	}); 
}