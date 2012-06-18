////
//
// uiVolumeEtc.js
//
////
var uiVolumeEtc = {};

uiVolumeEtc.status = {volume:0, mute: false};

uiVolumeEtc.refreshVolumeForPage = function(pageName, vol, mute){

  var page = CF.page(pageName);

  var volStr = "VOL";

  if (page == null) {
    GUI.alert("No page ["+pageName+"]");
    return;
  };

  var volumeImage = page.widget("volumeImage");

  if (!configuration.preAmp) {

     volumeImage.visible = false;
     page.widget("volume").label = "";
     if (pronto.is9400) {
       page.widget("volumeIcon9400").visible = false;
       page.widget("volumeImage9400").visible = false;     
     };
     return;
  };

  if (mute) {
    page.widget("volume").label = "";
    volumeImage.stretchImage = true;  
    volumeImage.visible = true;
    volumeImage.setImage(CF.widget("Mute", "VOL").getImage());
    if (pronto.is9400) {
      page.widget("volume").label = vol;
      page.widget("volumeMutedIcon9400").visible = true;
    };
    return;
  };

  
  if (pronto.is9400) {
    page.widget("volumeMutedIcon9400").visible = false;
  };

  page.widget("volume").label = vol;

#if (MODEL_9600)


    if (vol < 1) {
     volumeImage.visible = false;
     return;
    };
  
    volumeImage.stretchImage = true;  
    volumeImage.visible = true;
    volumeImage.setImage(kinskyUiElements.getVolumeImage(vol));

#else
  
  // 9400 specific
  volStr = "VOL9400";
  
  var volumeImage9400 = page.widget("volumeImage9400");
  
  volumeImage9400.stretchImage = true;  
  volumeImage9400.visible = true;
  volumeImage9400.setImage(kinskyUiElements.getVolumeImage9400(vol));


#endif
};


uiVolumeEtc.displayVolume = function(vol, mute){

#if (VARIANT_DS)

  uiVolumeEtc.refreshVolumeForPage("NowPlaying", vol, mute);
  uiHeader.setVolume(mute ? -vol : vol);

#elif (VARIANT_RADIO)

  uiVolumeEtc.refreshVolumeForPage("NowListening", vol, mute);
  uiHeader.setVolume(mute ? -vol : vol);

#elif (VARIANT_PREAMP)

  uiVolumeEtc.refreshVolumeForPage("PreAmpOnly", vol, mute);

#endif


};

uiVolumeEtc.callback = function(status)
{
  uiVolumeEtc.status = status;  
  uiVolumeEtc.update();  
};

uiVolumeEtc.update = function()
{
  uiVolumeEtc.displayVolume(uiVolumeEtc.status.volume, uiVolumeEtc.status.mute);
};

uiVolumeEtc._start = function()
{
  engineVolumeEtc.setCallback(uiVolumeEtc.callback);
};


elab.add("uiVolumeEtc", null, uiVolumeEtc._start);
