(function($) {    
    var kImageFolder = "/Layouts/Portrait/Medium/Images/Widgets/";
    
    
    var container, volumeControl, sourceSelector, transportControl, playMode, browser, roomSelector, mediaTime, playlist, breadcrumb, upDirectory, standby;
    var kExpiryTimeMilliseconds = 30000;
    var kFailedCallbackAttemptsLimit = 5;
    var kPageSize = 40;
    var kPagerButtonCount = 6;
    
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
		    setTimeout(function() {
		    openContainer();}, 1000
		    );
		});
    }

    $(window).bind("beforeunload", function(e) {
        if (container){
            container.closeServices();        
        }
    });
    
   $(document).ready(function() {	
   
        
		
        container = new $.KinskyWidgetContainer($("#Container"), kExpiryTimeMilliseconds, kFailedCallbackAttemptsLimit);
        
        var browserLocation = [];
        
        browser = new $.Browser($("#Browser"), container, browserLocation, "#Playlist #BrowserContent", kPageSize, kPagerButtonCount, true, kImageFolder, true);
        browser.render();
        container.add(browser);  
              
        upDirectory = new $.UpDirectory($("#UpDirectory"), container, browser);
        upDirectory.render();
        container.add(upDirectory);     
        
        breadcrumb = new $.BreadcrumbMobile($("#Breadcrumb"), container, browser);
        breadcrumb.render();   
        container.add(breadcrumb);   
        
        
       
	    volumeControl = new $.VolumeControlMobile($("#VolumeControl"), container, false, null, kImageFolder, 10);
        volumeControl.render();   
		container.add(volumeControl);		
		
		mediaTime = new $.MediaTimeMobile($("#MediaTime"), container, false, null, false, kImageFolder, 10);
        mediaTime.render();   
        container.add(mediaTime);

        sourceSelector = new $.SourceSelectorMobile($("#SourceSelector"), container, kImageFolder);
        sourceSelector.render();   
		container.add(sourceSelector);
		
		transportControl = new $.TransportControl($("#TransportControl"), container, false, null, kImageFolder);
        transportControl.render();   
        container.add(transportControl);        
        browser.setTransportControl(transportControl);

		track = new $.Track($("#Track"), container);
        track.render();   
        container.add(track);

        roomSelector = new $.RoomSelectorMobile($("#RoomSelector"), container, kImageFolder);
        roomSelector.render();   
        container.add(roomSelector);
        
        playlist = new $.Playlist($("#Playlist"), container, kPageSize, kPagerButtonCount, true, kImageFolder, true);
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
        sourceSelector.domElement.bind("evtHideSourceSelection", function(){
            $("#Track, #Controls, #RoomSelector, #Standby").show();
		    window.scrollTo(0, 1);
        });
        sourceSelector.domElement.bind("evtShowSourceSelection", function(){
            $("#Track, #Controls, #RoomSelector, #Standby").hide();
		    window.scrollTo(0, 1);
        });
        roomSelector.domElement.bind("evtHideRoomSelection", function(){
            $(this).css("max-width", "80%");
            $("#Track, #Controls, #SourceSelector, #Standby").show();
		    window.scrollTo(0, 1);
        });
        roomSelector.domElement.bind("evtShowRoomSelection", function(){
            $(this).css("max-width", "100%");
            if ($(this).width()){
                $(this).find(".ListItem").css("max-width", $(this).width() + "px");
            }
            $("#Track, #Controls, #SourceSelector, #Standby").hide();
		    window.scrollTo(0, 1);
        });        
        breadcrumb.domElement.bind("evtHideBreadcrumbSelection", function(){
            $("#Browser, #UpDirectory").show();
		    window.scrollTo(0, 1);
        });
        breadcrumb.domElement.bind("evtShowBreadcrumbSelection", function(){
            $("#Browser, #UpDirectory").hide();
        });
        
        container.domElement.bind("evtServerClosed", function(){
            $("#DisconnectOverlay").show();
            if (!container.serverClosed){  
                openContainer();
            }
        });
        container.domElement.bind("evtServerDown", function(){
            $("#DisconnectOverlay").show();
        });
        container.domElement.bind("evtServerUp", function(){
            $("#DisconnectOverlay").hide();
        });
        
		openContainer();
		
		//Get all the links with rel as panel
		$('a[rel=Panel]').click(function (e) {	
		    e.preventDefault();
			//Set class for the selected item
			$('a[rel=Panel]').removeClass('Selected');
			var href = $(this).attr("href");
			$.each($(".TabContainer a"), function(){
			    if ($(this).attr("href") == href){			        
			        $(this).addClass('Selected');
			    }
			});
			$(".Panel").removeClass('Selected');
			$(href).addClass('Selected');
			$(".contextMenu").hide();
			//Discard the link default behavior
			return false;
		});
		if (!debugConsoleEnabled){
	        $("#DebugTab").hide();
	    }else{
	        $("#DebugClear").click(function(){
	            $("#DebugConsole").empty();
	        });
	        $("#DebugPause").click(function(){
	            debugConsoleEnabled = !debugConsoleEnabled;
	            if (debugConsoleEnabled){
	                $(this).text("Pause");
	            }else{
	                $(this).text("Restart");
	            }
	        });
	    }
    });    
   
   $(window).load(function(){   
        if (isIPhone){
		    hideURLBar();
		}
   });
   var hideURLBar = function(){
        if(window.innerHeight < (window.outerHeight+20)) {  $('html').css({'min-height':(window.outerHeight+20)+'px'}); }
		setTimeout(function() { if(window.pageYOffset<1) { window.scrollTo(0, 1); hideURLBar(); } }, 1000);
   }

})(jQuery);
