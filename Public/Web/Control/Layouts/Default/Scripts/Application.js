var container, proxyContainer, sourceType, topPanels;



var setOrientation = function () {
    var isLandscape;
    if (isIPhone || isIPad) {
        isLandscape = (window.orientation == -90 || window.orientation == 90);
    } else {
        isLandscape = isAndroid && jQuery(document).height() < jQuery(document).width();
    }
    jQuery("html *").toggleClass("Landscape", isLandscape);
    if (!isIPhone && !isIPad && !isLandscape) {
        var height = jQuery(window).height();
        topPanels.css("height", height - 160 + "px");
    }
    if (isLandscape) {
        self.scrollTo(0, 0);
    }
    if (isIPhone) {
        hideURLBar();
    }
}


var hideURLBar = function () {
    if (window.innerHeight < (window.outerHeight + 20)) { jQuery('html').css({ 'min-height': (window.outerHeight + 20) + 'px' }); }
    setTimeout(function () { if (window.pageYOffset < 1) { window.scrollTo(0, 1); hideURLBar(); } }, 1000);
}

jQuery(document).ready(function () {

    var preCacheImages = [
		"Images/Album.png",
		"Images/AlbumError.png",
		"Images/Artist.png",
		"Images/Directory.png",
		"Images/Error.png",
		"Images/Library.png",
		"Images/nav.png",
		"Images/nav_divider.png",
		"Images/nav_active.png",
		"Images/nav_divider_active.png",
		"Images/NoAlbumArt.png",
		"Images/Radio.png",
		"Images/Track.png",
		"Images/Video.png",
		"Images/Standby.png",
		"Images/StandbyRollover.png",
		"Images/StandbyDown.png",
		"Images/StandbyOn.png",
		"Images/StandbyOnRollover.png",
		"Images/StandbyOnDown.png",
		"Images/Repeat.png",
		"Images/RepeatRollover.png",
		"Images/RepeatDown.png",
		"Images/RepeatOn.png",
		"Images/RepeatOnRollover.png",
		"Images/RepeatOnDown.png",
		"Images/Shuffle.png",
		"Images/ShuffleRollover.png",
		"Images/ShuffleDown.png",
		"Images/ShuffleOn.png",
		"Images/ShuffleOnRollover.png",
		"Images/ShuffleOnDown.png",
		"Images/AuxSource.png",
		"Images/CD.png",
		"Images/PlaylistSource.png",
		"Images/Spdif.png",
		"Images/TosLink.png",
		"Images/UPNP.png",
		"Images/Close.png",
		"Images/NextPage.png",
		"Images/PreviousPage.png",
		"Images/SkipBack.png",
		"Images/SkipForward.png",
		"Images/PlayOverlay.png",
		"Images/Numpad.png",
		"Images/NumpadActive.png",
		"Images/Volume.png",
		"Images/VolumeActive.png",
		"Images/Playing.png",
		"Images/Mute.png",
		"Images/MuteActive.png",
		"Images/VolumeUp.png",
		"Images/VolumeDown.png",
		"Images/VolumeUpMed.png",
		"Images/VolumeDownMed.png",
		"Images/VolumeUpSmall.png",
		"Images/VolumeDownSmall.png",
		"Images/DeleteButton.png",
		"Images/EnterButton.png",
		"Images/Play.png",
		"Images/PlayRollover.png",
		"Images/PlayDown.png",
		"Images/Stop.png",
		"Images/StopRollover.png",
		"Images/StopDown.png",
		"Images/Pause.png",
		"Images/PauseRollover.png",
		"Images/PauseDown.png",
		"Images/Button.png",
		"Images/ButtonRollover.png",
		"Images/ButtonDown.png",
		"Images/Buffering.gif"
	];

    var img;
    for (var i = 0; i < preCacheImages.length; i++) {
        img = new Image;
        img.src = preCacheImages[i];
    }
    var qs = GetQueryString();
    var service = "Ds";
    if (qs.service){
        service = qs.service;
    }
    
    var services = [
		{ Name: "Playlist", ServiceID: service },
		{ Name: "Product", ServiceID: service },
		{ Name: "Radio", ServiceID: service },
		{ Name: "Info", ServiceID: service },
		{ Name: "Jukebox", ServiceID: service },
		{ Name: "Volume", ServiceID: service },
		{ Name: "Receiver", ServiceID: service }
	];
    var widgets = [
		{ Class: "WidgetPlaylist", Name: "Playlist", DomElements: {
		    ItemsContainer: jQuery("#Playlist"),
		    PreviousPage: jQuery("#PlaylistPrevious"),
		    NextPage: jQuery("#PlaylistNext"),
		    PageLinksContainer: jQuery("#PlaylistLinks"),
		    Repeat: jQuery("#RepeatButton"),
		    Shuffle: jQuery("#ShuffleButton")
		}
		},
		{ Class: "WidgetStandby", Name: "Standby", DomElements: { StandbyButton: jQuery("#StandbyButton")} },
		{ Class: "WidgetSourceSelection", Name: "SourceSelection", DomElements: {
		    ItemsContainer: jQuery("#SourceSelectionContainer"),
		    PageCountDisplay: jQuery("#SourceSelectionDisplay"),
		    PreviousPage: jQuery("#SourceSelectionPreviousPage"),
		    NextPage: jQuery("#SourceSelectionNextPage"),
		    CurrentSourceDisplay: jQuery("#CurrentSource")
		}
		},
		{ Class: "WidgetTrack", Name: "Track", DomElements: {
		    AlbumArt: jQuery("#TrackAlbumArt"),
		    Title: jQuery("#TrackTitle"),
		    Artist: jQuery("#TrackArtist"),
		    Album: jQuery("#TrackAlbum"),
		    BitRate: jQuery("#TrackBitRate"),
		    SampleRate: jQuery("#TrackSampleRate"),
		    Codec: jQuery("#TrackCodec")
		}
		},
		{ Class: "WidgetTransportControl", Name: "TransportControl", DomElements: {
		    TransportButton: jQuery("#TransportControlToggleState, #TransportButton"),
		    PreviousTrack: jQuery("#TransportControlPrevious"),
		    NextTrack: jQuery("#TransportControlNext"),
		    TransportControlDisplay: jQuery("#TransportControlDisplay")
		}
		},
		{ Class: "WidgetNumpad", Name: "Numpad", DomElements: {
		    Container: jQuery("#NumpadContainer"),
		    Enter: jQuery("#NumpadEnter"),
		    Clear: jQuery("#NumpadClear"),
		    Display: jQuery("#NumpadDisplay")
		}
		},
		{ Class: "WidgetVolumeControl", Name: "VolumeControl", DomElements: {
		    VolumeDisplay: jQuery("#VolumeDisplay"),
		    VolumeUp: jQuery("#VolumeUp"),
		    VolumeDown: jQuery("#VolumeDown"),
		    VolumeMute: jQuery("#VolumeMute"),
		    VolumeDisplayTooltip: jQuery("#VolumeDisplayTooltip")
		}
		}
	];

    container = new WidgetContainer(services, widgets);
	proxyContainer = new ServiceCollection();
	proxyContainer.AddService(new ServiceVolume("Preamp"));

    container.Widgets().SourceSelection.SetPageSize(9);
    container.Widgets().SourceSelection.DomElements().ItemsContainer.bind("evtItemSelected", function () {
        toggleSourceSelectionState(false);
    });

    container.Widgets().Numpad.DomElements().Enter.bind("evtNumpadEnterClick", function () {
        toggleNumpadState(false);
    });

    container.Widgets().Playlist.DomElements().ItemsContainer.bind("evtItemSelected", function () {
        togglePlaylistState(false);
    });


    container.Widgets().Playlist.SetPageSize(100);


    container.Services().Product.Variables().Standby.AddListener(function (value) {
        jQuery("html *").toggleClass("Standby", value);
    });

    jQuery("#ToggleSourceSelection").click(function () {
        toggleSourceSelectionState(!(jQuery(this).hasClass("Active")));
    });

    jQuery("#ToggleNumpad").click(function () {
        toggleNumpadState(!(jQuery(this).hasClass("Active")));
    });

    jQuery("#ToggleVolumeControl").click(function () {
        if (!jQuery("#VolumeControlImage").hasClass("Inactive")) {
            toggleVolumeState(!(jQuery(this).hasClass("Active")));
        }
    });

    jQuery("#PlaylistClose").click(function () {
        togglePlaylistState(false);
    });

    jQuery("#TransportControlDisplay").click(function () {
        togglePlaylistState(!(jQuery(this).hasClass("Active")));
    });

    container.Services().Product.Variables().SourceIndex.AddListener(function (value) {
        sourceChanged();
    });

    // if we get a volume response then we can assume the device has a volume service
    container.Services().Volume.Variables().Volume.AddListener(function (value) {
        jQuery("#VolumeControlImage").toggleClass("Inactive", false);
    });
	
	// if we get a proxy volume response then we can assume the device has a proxy preamp
    proxyContainer.Services().Volume.Variables().Volume.AddListener(function (value) {
        jQuery("#VolumeControlImage").toggleClass("Inactive", false);
		container.Services().Volume = proxyContainer.Services().Volume;
		container.Widgets().VolumeControl.UpdateVolumeService();
    });

    topPanels = jQuery("#NowPlayingContainer, #SourceSelectionContainer, #NumpadContainer, #VolumeDisplayContainer");
    setOrientation();
    jQuery("html *").toggleClass("IPhone", isIPhone).toggleClass("IPad", isIPad);
    container.Start();
    proxyContainer.Start();

    container.Services().Product.Variables().SourceXml.AddListener(function (value) {
        sourceChanged();
        // only listen once for sourcexml 
        // need to add this listener after container start so that any widgets interested in product source xml get notified first
        container.Services().Product.Variables().SourceXml.Listeners().clear();
    });
    self.scrollTo(0, 0);
    jQuery("#StandbyButton, #TransportButton, #ShuffleButton, #RepeatButton, #TransportButtonImage, #TransportControlPrevious, #TransportControlDisplay, #TransportControlNext, #SourceSelectionPreviousPage, #SourceSelectionDisplay, #SourceSelectionNextPage, #VolumeDown, #VolumeMute, #VolumeUp, #NumpadClear, #NumpadEnter, #NumpadImage, #CurrentSource, #VolumeControlImage").hover(
        function(){
            jQuery(this).toggleClass("Hover",true);
        },function(){
            jQuery(this).toggleClass("Hover",false);
        }).mousedown(function(){
            jQuery(this).toggleClass("Mouse", true);
        }).mouseup(function(){
            jQuery(this).toggleClass("Mouse", false);
        });
    container.Services().Product.Standby(function(value){
		if (value.Value){
			container.Services().Product.SetStandby(false);
		}
	});
});

function togglePlaylistState(active) {
    if (!active || container.Widgets().Playlist.IsPlaylistSource() || container.Widgets().Playlist.IsRadioSource()) {
        jQuery("#ListViewButton, #PlaylistContainer").toggleClass("Active", active);
        jQuery("#TabContainers, #NowPlayingContainer").toggleClass("Active", !active);
        jQuery("#NumpadContainer, #ToggleNumpad, #VolumeControlTabs, #NumpadTabs, #ToggleVolumeControl, #ToggleSourceSelection, #SourceSelectionContainer, #VolumeDisplayContainer").toggleClass("Active", false);
        if (active) {
            container.Widgets().Playlist.Show();
        } else {
            container.Widgets().Playlist.Hide();
            self.scrollTo(0, 0);
        }
    }
}

function toggleSourceSelectionState(active) {
    jQuery("#SourceSelectionContainer, #SourceSelectionTabs, #ToggleSourceSelection").toggleClass("Active", active);
    jQuery("#TransportControlTabs, #NowPlayingContainer").toggleClass("Active", !active);
    jQuery("#NumpadContainer, #ToggleNumpad, #NumpadTabs, #VolumeControlTabs, #ToggleVolumeControl, #VolumeDisplayContainer").toggleClass("Active", false);
    if (active) {
        container.Widgets().SourceSelection.Show();
    } else {
        container.Widgets().SourceSelection.Hide();
    }
}

function toggleNumpadState(active) {
    jQuery("#NumpadContainer, #NumpadTabs, #ToggleNumpad").toggleClass("Active", active);
    jQuery("#TransportControlTabs, #NowPlayingContainer").toggleClass("Active", !active);
    jQuery("#SourceSelectionContainer, #ToggleSourceSelection, #SourceSelectionTabs, #VolumeControlTabs, #ToggleVolumeControl, #VolumeDisplayContainer").toggleClass("Active", false);
    if (active) {
        container.Widgets().Numpad.Clear();
    }
}

function toggleVolumeState(active) {
    jQuery("#VolumeControlTabs, #ToggleVolumeControl, #VolumeDisplayContainer").toggleClass("Active", active);
    jQuery("#TransportControlTabs, #NowPlayingContainer").toggleClass("Active", !active);
    jQuery("#SourceSelectionContainer, #ToggleSourceSelection, #SourceSelectionTabs, #NumpadContainer, #ToggleNumpad, #NumpadTabs").toggleClass("Active", false);
}

function sourceChanged() {
    var sourceIndex = container.Services().Product.Variables().SourceIndex.Value();
    var sourceXml = container.Services().Product.Variables().SourceXml.Value();
    var source = null;
    if (sourceIndex != null && sourceXml != null) {
        source = jQuery.xml2json(sourceXml).Source[sourceIndex];
    }
    var widgets = container.WidgetNames();
    for (var i = 0; i < widgets.length; i++) {
        container.Widgets()[widgets[i]].SetSource(source);
    }
}

touchMove = function (event) {
    if (!jQuery("#PlaylistContainer").hasClass("Active")) {
        event.preventDefault();
    }
}

if (isIPhone || isIPad) {
    window.onorientationchange = function () {
        setOrientation();
    }
} else {
    jQuery(window).bind("resize", function () {
        setOrientation();
    });
}

