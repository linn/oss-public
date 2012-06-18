var ServiceCollection = function (aPollTimeSeconds){
	this.iServices = {};
	this.iPollTimeSeconds = aPollTimeSeconds?aPollTimeSeconds:ServiceCollection.kDefaultPollTimeSeconds;
}

ServiceCollection.kDefaultPollTimeSeconds = 5;

ServiceCollection.prototype.Start = function(){
	var thisObj = this;
	this.iCallbackTimer = new PeriodicalExecuter(function() { thisObj.GetChanges(); } , this.iPollTimeSeconds);
	this.GetChanges();
}

ServiceCollection.prototype.Stop = function(){
	if (this.iCallbackTimer){
		this.iCallbackTimer.stop();
		this.iCallbackTimer = null;		
	}
}

ServiceCollection.prototype.AddService = function(aService){
	this.iServices[aService.ServiceName()] = aService;
}

ServiceCollection.prototype.RemoveService = function(aService){
	delete this.iServices[aService.ServiceName()];
}

ServiceCollection.prototype.GetService = function(aServiceName){
	return this.iServices[aServiceName];
}

ServiceCollection.prototype.Services = function(){
	return this.iServices;
}

ServiceCollection.prototype.ServiceNames = function(){
	var result = [];
	for (var srv in this.iServices){
		if (this.iServices.hasOwnProperty(srv)){
			result[result.length] = srv;
		}
	}
	return result;
}

ServiceCollection.prototype.GetChanges = function(){
	var serviceNames = this.ServiceNames();
	for(var i=0;i<serviceNames.length;i++){
		this.GetServiceChanges(this.GetService(serviceNames[i]));		
	}
}

ServiceCollection.prototype.GetServiceChanges = function(aService){
	var thisObj = this;
	var varNames = aService.VariableNames();
		
	if (aService.ServiceName() == "Info"){	
		thisObj.CallInfoServiceGetters(aService);
	}else if (aService.ServiceName() == "Product"){
        thisObj.CallProductServiceGetters(aService);
    }else{
		var functionName;
		for (var j=0;j<varNames.length;j++){
			functionName = varNames[j];
			var requestProperty = true;			
            var variableName = "Value";
			if (aService.ServiceName() == "Playlist"){				
				switch (functionName){
					case "IdArray":
						/* ignore IdArray - only handle this on IdArrayChanged = true */
						requestProperty = false;
					default: break;
				}
			}
			if (aService.ServiceName() == "Radio"){				
				switch (functionName){
					case "IdArray":
						/* ignore IdArray - only handle this on IdArrayChanged = true */
						requestProperty = false;
					default: break;
				}
			}
			if (aService.ServiceName() == "Configuration"){				
				switch (functionName){
					case "ParameterXml":
                        variableName = "aParameterXml";
						break;
					case "ConfigurationXml":
                        variableName = "aConfigurationXml";
						break;
					default: requestProperty = false;
				}
			}
			if (requestProperty){
				switch (varNames[j]){
					case "IdArrayChanged":	{				
						thisObj.CallIdArrayChangedGetter(aService);
						break;
					}
					default:{
						if (typeof aService[functionName] == "function"){
							thisObj.CallGenericGetter(aService, functionName, aService.Variables()[varNames[j]], variableName);
						}
					}
				}
			}
		}
	}
}

ServiceCollection.prototype.CallProductServiceGetters = function(aService){
    var thisObj = this;
    this.CallGenericGetter(aService, "SourceIndex", aService.Variables()["SourceIndex"], "Value");
	this.CallGenericGetter(aService, "SourceXml", aService.Variables()["SourceXml"], "Value");
	this.CallGenericGetter(aService, "Standby", aService.Variables()["Standby"], "Value");
	this.CallGenericGetter(aService, "Attributes", aService.Variables()["Attributes"], "Value");
    if (aService.Variables().ManufacturerName.Listeners().length ||
        aService.Variables().ManufacturerInfo.Listeners().length ||
        aService.Variables().ManufacturerUrl.Listeners().length ||
        aService.Variables().ManufacturerImageUri.Listeners().length){
        aService.Manufacturer(function(result){
			aService.Variables().ManufacturerName.SetValue(result.Name);	
			aService.Variables().ManufacturerInfo.SetValue(result.Info);	
			aService.Variables().ManufacturerUrl.SetValue(result.Url);
			aService.Variables().ManufacturerImageUri.SetValue(result.ImageUri);
		},function(message){
			debug.log("Error caught in Manufacturer: " + message);
		});
    }
    if (aService.Variables().ModelName.Listeners().length ||
        aService.Variables().ModelInfo.Listeners().length ||
        aService.Variables().ModelUrl.Listeners().length ||
        aService.Variables().ModelImageUri.Listeners().length){
        aService.Model(function(result){
			aService.Variables().ModelName.SetValue(result.Name);	
			aService.Variables().ModelInfo.SetValue(result.Info);	
			aService.Variables().ModelUrl.SetValue(result.Url);
			aService.Variables().ModelImageUri.SetValue(result.ImageUri);
		},function(message){
			debug.log("Error caught in Model: " + message);
		});
    }
    if (aService.Variables().ProductRoom.Listeners().length ||
        aService.Variables().ProductName.Listeners().length ||
        aService.Variables().ProductInfo.Listeners().length ||
        aService.Variables().ProductUrl.Listeners().length ||
        aService.Variables().ProductImageUri.Listeners().length){
        aService.Product(function(result){
			aService.Variables().ProductRoom.SetValue(result.Room);	
			aService.Variables().ProductName.SetValue(result.Name);	
			aService.Variables().ProductInfo.SetValue(result.Info);	
			aService.Variables().ProductUrl.SetValue(result.Url);
			aService.Variables().ProductImageUri.SetValue(result.ImageUri);
		},function(message){
			debug.log("Error caught in Product: " + message);
		});
    }           
}

ServiceCollection.prototype.CallIdArrayChangedGetter = function(aService){
	var thisObj = this;
	if (aService.Variables().IdArray.Listeners().length){
		aService.IdArrayChanged(aService.Variables().IdArrayToken.Value(), function(result){
			aService.Variables().IdArrayChanged.SetValue(result.Value);
			if (result.Value){
				thisObj.CallIdArrayGetter(aService);
			}
		},function(message){
			debug.log("Error caught in CallIdArrayChangedGetter: " + message);
		});
	}
}

ServiceCollection.prototype.CallIdArrayGetter = function(aService){
	aService.IdArray(function(result){	
		aService.Variables().IdArray.SetValue(decode64(result.Array));
		aService.Variables().IdArrayToken.SetValue(result.Token);		
	},function(message){
		debug.log("Error caught in CallIdArrayGetter: " + message);
	});
} 

ServiceCollection.prototype.CallInfoServiceGetters = function(aService){
	var thisObj = this;
	if (this.HasTrackListeners(aService) || this.HasDetailsListeners(aService) || this.HasMetatextListeners(aService)){
		aService.Counters(function(result){
			var previousTrackCount = aService.Variables().TrackCount.Value();
			aService.Variables().TrackCount.SetValue(result.TrackCount);
			var trackCountChanged = previousTrackCount != aService.Variables().TrackCount.Value();
			if (trackCountChanged){
				thisObj.CallTrackGetter(aService);		
			}

			var previousDetailsCount = aService.Variables().DetailsCount.Value();
			aService.Variables().DetailsCount.SetValue(result.DetailsCount);
			var detailsCountChanged = previousDetailsCount != aService.Variables().DetailsCount.Value();
			if (trackCountChanged || detailsCountChanged){
				thisObj.CallDetailsGetter(aService);	
			}
			
			var previousMetatextCount = aService.Variables().MetatextCount.Value();		
			aService.Variables().MetatextCount.SetValue(result.MetatextCount);		
			var metatextCountChanged = previousMetatextCount != aService.Variables().MetatextCount.Value();
			if (trackCountChanged || metatextCountChanged){
				thisObj.CallGenericGetter(aService, "Metatext", aService.Variables().Metatext, "Value");	
			}
		},function(message){
			debug.log("Error caught in CallCountersGetter: " + message);
		});		
	}
}

ServiceCollection.prototype.HasTrackListeners = function(aService){
	return aService.Variables().TrackCount.Listeners().length || 		
		aService.Variables().Uri.Listeners().length || 
		aService.Variables().Metadata.Listeners().length;
}

ServiceCollection.prototype.CallTrackGetter = function(aService){
	if (this.HasTrackListeners(aService)){
		aService.Track(function(result){
			aService.Variables().Uri.SetValue(result.Uri);	
			aService.Variables().Metadata.SetValue(result.Metadata);	
		},function(message){		
			debug.log("Error caught in CallTrackGetter: " + message);
		});
	}
}

ServiceCollection.prototype.HasDetailsListeners = function(aService){
	return aService.Variables().DetailsCount.Listeners().length || 
		aService.Variables().Duration.Listeners().length || 
		aService.Variables().BitRate.Listeners().length || 
		aService.Variables().BitDepth.Listeners().length || 
		aService.Variables().SampleRate.Listeners().length || 
		aService.Variables().Lossless.Listeners().length || 
		aService.Variables().CodecName.Listeners().length;
}

ServiceCollection.prototype.CallDetailsGetter = function(aService){
	if (this.HasDetailsListeners(aService)){		
		aService.Details(function(result){
			aService.Variables().Duration.SetValue(result.Duration);	
			aService.Variables().BitRate.SetValue(result.BitRate);	
			aService.Variables().BitDepth.SetValue(result.BitDepth);	
			aService.Variables().SampleRate.SetValue(result.SampleRate);	
			aService.Variables().Lossless.SetValue(result.Lossless);	
			aService.Variables().CodecName.SetValue(result.CodecName);	
		},function(message){		
			debug.log("Error caught in CallDetailsGetter: " + message);
		});
	}
}

ServiceCollection.prototype.HasMetatextListeners = function(aService){
	return aService.Variables().MetatextCount.Listeners().length || 
		aService.Variables().Metatext.Listeners().length;
}

ServiceCollection.prototype.CallMetatextGetter = function(aService){
	if (this.HasMetatextListeners(aService)){		
		aService.Metatext(function(result){
			aService.Variables().Metatext.SetValue(result.aMetatext);	
		},function(message){		
			debug.log("Error caught in CallMetatextGetter: " + message);
		});
	}
}

ServiceCollection.prototype.CallGenericGetter = function(aService, aServiceFunctionName, aServiceVariable, aServiceVariableName){
	if (aServiceVariable.Listeners().length){
		aService[aServiceFunctionName].call(aService,function(result){
			debug.log("CallGenericGetter: " + aServiceFunctionName + ", " + aServiceVariableName);
			aServiceVariable.SetValue(result[aServiceVariableName]);
		},function(message, transport){
            try{
                if (transport && transport.status && transport.status == "404" && aService.ServiceName() == "Volume"){		
                    // preamp service is not always there - prevent repeated messages to non existent service
                    aServiceVariable.Listeners().clear();	
                }
            }catch(e){} //transport object can fail in IE
			debug.log("Error caught in CallGenericGetter: " + message);
		});
	}
}

var ServiceVariable = function(aName){
	this.iName = aName;
	this.iValue = null;
	this.iListeners = [];
}

ServiceVariable.prototype.Name = function(){
	return this.iName;
}

ServiceVariable.prototype.Value = function(){
	return this.iValue;
}

ServiceVariable.prototype.SetValue = function(aValue){
	if (this.iValue != aValue){
		this.iValue = aValue;
		for (var i=0;i<this.iListeners.length;i++){
			try{
				this.iListeners[i].call(this, aValue);
			}catch(e){
				debug.log("Error caught in SetValue: " + e);
			}			
		}
	}
}

ServiceVariable.prototype.AddListener = function(aListener){
	this.iListeners[this.iListeners.length] = aListener;
}

ServiceVariable.prototype.RemoveListener = function(aListener){	
	var idx = this.iListeners.indexOf(aListener);
	if (idx != -1){
		this.iListeners.remove(idx);
	}
}

ServiceVariable.prototype.Listeners = function(){
	return this.iListeners;
}
