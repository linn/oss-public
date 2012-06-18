(function($) {    
    var kImageFolder = "/Layouts/Desktop/Images/Widgets/";
    
    var container, volumeControl, sourceSelector, transportControl, playMode, browser, roomSelector, mediaTime, playlist, breadcrumb, upDirectory;
    var kExpiryTimeMilliseconds = 10000;
    var kPageSize = 1000;
    var kPagerButtonCount = 10;
        var roomChanged = function(){
               debug.log("Room Changed Event: current room = " + roomSelector.room);
            
			    upDirectory.closeServices();   
                breadcrumb.closeServices();    
                mediaTime.closeServices();    
                transportControl.closeServices();   
                playlist.closeServices();   
                playMode.closeServices();   
                wasteBin.closeServices();   
                track.closeServices();
                
                 
            
                /* wire up volume control to room change event */
                if (volumeControl.servicesOpened){
				    volumeControl.closeServices(function(){	
				        openVolumeControl();
				    });
				}else{
				    openVolumeControl();
				}
    			
			    /* wire up source selector to room change event */
                if (sourceSelector.servicesOpened){
                    sourceSelector.closeServices(function(){
				        openSourceSelector();
				    });
				}else{
				    openSourceSelector();
				}
	    };
        
        var sourceChanged = function(){
		debug.log("Source Changed Event: current source = " + sourceSelector.source);

			/* wire up transport control to source change event */
            if (transportControl.servicesOpened){
    		    transportControl.closeServices(function(){	
				    openTransportControl();
			    });
		    }else{
			    openTransportControl();
		    }
		    
			/* wire up play mode to source change event */
            if (playMode.servicesOpened){
    		    playMode.closeServices(function(){	
				    openPlayMode();
			    });
		    }else{
			    openPlayMode();
		    }
			
			/* wire up media time  to source change event */
            if (mediaTime.servicesOpened){
			    mediaTime.closeServices(function(){	
			        openMediaTime();
			    });
		    }else{
				openMediaTime();
			}
			
			/* wire up track to source change event */
            if (track.servicesOpened){
			    track.closeServices(function(){	
			        openTrack();
			    });
		    }else{
				openTrack();
			}
			
			/* wire up playlist to source change event */
            if (playlist.servicesOpened){
			    playlist.closeServices(function(){	
			        openPlaylist();
			    });
		    }else{
				openPlaylist();
			}
			
			/* wire up waste bin to source change event */
            if (wasteBin.servicesOpened){
			    wasteBin.closeServices(function(){	
			        openWasteBin();
			    });
		    }else{
				openWasteBin();
			}
        }
        
        var openVolumeControl = function(){
			volumeControl.openServices(roomSelector.room);
        }
        
        var openSourceSelector = function (){
             sourceSelector.openServices(roomSelector.room);
        }
        
        var openTransportControl = function (){
			transportControl.openServices(roomSelector.room, sourceSelector.source, sourceSelector.sourceType);
        }
        
        var openPlayMode = function (){
			playMode.openServices(roomSelector.room, sourceSelector.source, sourceSelector.sourceType);
        }
        
        var openMediaTime = function (){
			mediaTime.openServices(roomSelector.room, sourceSelector.source);
        }
        
        var openTrack = function (){
			track.openServices(roomSelector.room, sourceSelector.source);
        }
        
        var openPlaylist = function (){
			playlist.openServices(roomSelector.room, sourceSelector.source, sourceSelector.sourceType);
        }
        
        var openWasteBin = function (){
			wasteBin.openServices(roomSelector.room, sourceSelector.source, sourceSelector.sourceType);
        }
    
    
    var openContainer = function(){
        container.openServices(function(){
			upDirectory.closeServices();   
            breadcrumb.closeServices();   
            volumeControl.closeServices();   
            mediaTime.closeServices();   
            sourceSelector.closeServices();   
            transportControl.closeServices();   
            playlist.closeServices();   
            playMode.closeServices();   
            wasteBin.closeServices();   
            track.closeServices();   
            roomSelector.openServices();
			browser.openServices();
		},
		function(){
		    setTimeout(function() {openContainer();}, 1000);
		});
    }

    $(window).bind("beforeunload", function(e) {
        if (container){
            container.closeServices();        
        }
    });
    

    $(document).ready(function() {		
        container = new $.KinskyWidgetContainer($("#Container"), kExpiryTimeMilliseconds);
        
        var browserLocation = [];
        
        browser = new $.Browser($("#Browser"), container, browserLocation, "#Playlist #BrowserContent", kPageSize, kPagerButtonCount, false, kImageFolder, false);
        browser.render();
        container.add(browser);  
              
        upDirectory = new $.UpDirectory($("#UpDirectory"), container, browser);
        upDirectory.render();
        container.add(upDirectory);     
        
        breadcrumb = new $.Breadcrumb($("#Breadcrumb"), container, browser);
        breadcrumb.render();   
        container.add(breadcrumb);   
        
        
        var imageCoords = {};
        imageCoords["muteButton"] = "21,21,20";
        imageCoords["downButton"] = "49,12,49,26,40,29,33,34,29,40,27,45,27,59,32,65,36,69,40,71,46,73,50,73,50,86,42,84,35,81,29,78,23,73,18,67,15,60,13,54,13,43,15,38,20,30,24,24,31,19,39,15,46,14,49,13";
        imageCoords["upButton"] = "1,12,10,13,19,17,24,21,29,26,34,34,37,42,38,46,38,54,37,61,34,68,29,74,22,81,13,86,5,88,1,89,0,89,0,69,7,68,13,64,18,58,19,51,19,46,18,41,15,37,12,34,8,33,6,31,0,31,0,12";
        
	    volumeControl = new $.VolumeControl($("#VolumeControl"), container, true, imageCoords, kImageFolder);
        volumeControl.render();   
		container.add(volumeControl);		
		
		imageCoords = {};
        imageCoords["backButton"] = "49,12,49,26,40,29,33,34,29,40,27,45,27,59,32,65,36,69,40,71,46,73,50,73,50,86,42,84,35,81,29,78,23,73,18,67,15,60,13,54,13,43,15,38,20,30,24,24,31,19,39,15,46,14,49,13";
        imageCoords["forwardButton"] = "1,12,10,13,19,17,24,21,29,26,34,34,37,42,38,46,38,54,37,61,34,68,29,74,22,81,13,86,5,88,1,89,0,89,0,69,7,68,13,64,18,58,19,51,19,46,18,41,15,37,12,34,8,33,6,31,0,31,0,12";
		mediaTime = new $.MediaTime($("#MediaTime"), container, true, imageCoords, true, kImageFolder);
        mediaTime.render();   
        container.add(mediaTime);

        sourceSelector = new $.SourceSelector($("#SourceSelector"), container);
        sourceSelector.render();   
		container.add(sourceSelector);
		
		imageCoords = {};
        imageCoords["transportButton"] = "37,48,35";
        imageCoords["previousButton"] = "17,18,14";
        imageCoords["nextButton"] = "17,18,14";
		transportControl = new $.TransportControl($("#TransportControl"), container, true, imageCoords, kImageFolder);
        transportControl.render();   
        container.add(transportControl);      
        browser.setTransportControl(transportControl);  

		track = new $.Track($("#Track"), container);
        track.render();   
        container.add(track);

        roomSelector = new $.RoomSelector($("#RoomSelector"), container);
        roomSelector.render();   
        container.add(roomSelector);
        
        playlist = new $.Playlist($("#Playlist"), container, kPageSize, kPagerButtonCount, false, kImageFolder, false);
        playlist.render();   
        container.add(playlist);
        
		playMode = new $.PlayMode($("#PlayMode"), container, playlist);
        playMode.render();   
        container.add(playMode);
        
		wasteBin = new $.WasteBin($("#WasteBin"), container, playlist);
        wasteBin.render();   
        container.add(wasteBin);
        
	standby = new $.StandbyButton($("#Standby"), container, roomSelector, sourceSelector);
        standby.render();   
        container.add(standby);
        roomSelector.domElement.bind("evtRoomSelectorRoomChanged", roomChanged);
        sourceSelector.domElement.bind("evtSourceSelectorSourceChanged", sourceChanged);
	standby.domElement.bind("evtStandby", function(){
            sourceSelector.standby();
			roomSelector.standby();
			upDirectory.closeServices();   
            breadcrumb.closeServices();   
            volumeControl.closeServices();   
            mediaTime.closeServices();   
            sourceSelector.closeServices();   
            transportControl.closeServices();   
            playlist.closeServices();   
            playMode.closeServices();   
            wasteBin.closeServices();   
            track.closeServices(); 
        });
        
        container.domElement.bind("evtServerClosed", function(){
            debug.log("Server closed event");
            $("#DisconnectOverlay").show();
            if (!container.serverClosed){  
                openContainer();
            }
        });

        container.domElement.bind("evtServerDown", function(){
            debug.log("Server down event");
            $("#DisconnectOverlay").show();
        });
        
        container.domElement.bind("evtServerUp", function(){
            debug.log("Server up event");
            $("#DisconnectOverlay").hide();
        });
        
		openContainer();
        new SplitPane("Browser", 49, "Playlist", 49, 49, { active: true });        
        resize();
    });    
    
    $(window).bind("resize", function(){ 
        resize();
   });
    
   var resize = function(){
        var windowHeight = $(window).height();
        if (windowHeight){
            $(".BrowserContent").css("min-height", windowHeight / 2 + "px");
            $(".BrowserContent").css("max-height", windowHeight / 2 + "px");
            $(".LoadProgress").css("min-height", windowHeight / 2 + "px");
            $(".LoadProgress").css("max-height", windowHeight / 2 + "px");            
        }
   }
})(jQuery);
