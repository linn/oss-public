////
//
// pronto.js
//
// Pronto 9400 & 9600/9800 specific constants.
//
// These are only needed to obtain the Pronto's screen
// resolution & from there how many lines we can display
// per screen type (Library or playlist)
//
// From Pronto v7.2.18 we can determine if the screen
// resolution of the Pronto Panel.
//
// For a 9600 / 9800 you have w=640, h=480 (VGA)
// for a 9400 we're talking QVGA
//
// NOTE: We use the check 'GUI.width == 640' to determine
// Pronto type; this will not work it we are trying to
// test a 9400 panel on a 9600.
//
////


var pronto = {};

// The default font to use is modified in the [PARAMETERS] section of the Pronto code
// Change the 'Font To Use' to be the font required.
//
#if 0
pronto.font = CF.widget("FONT", "PARAMETERS").font;
#endif

pronto.VARIANT_TYPE = {DS:0, RADIO:1, PREAMP:2};

#if (VARIANT_DS)

  pronto.variant = pronto.VARIANT_TYPE.DS;

#elif (VARIANT_RADIO)

  pronto.variant = pronto.VARIANT_TYPE.RADIO;

#elif (VARIANT_PREAMP)

  pronto.variant = pronto.VARIANT_TYPE.PREAMP;

#endif



#if (MODEL_9600)

  // Behold we are a 9600 (or a 9800)
  //
  // Previously use the test case:
  //
  //   if (GUI.width == 640) {...
  //
  Diagnostics.log("Pronto = 9600/9800.");

  pronto.is9600 = true;
  pronto.is9400 = false;

  pronto.yOffset = 60;
  pronto.characterHeight = 30;
  pronto.uiLibraryPageSize = 12;
  pronto.uiPlaylistPageSize = 8;

#else

  // And yes, we are a 9400
  //
  Diagnostics.log("Pronto = 9400.");

  pronto.is9600 = false;
  pronto.is9400 = true;

  pronto.yOffset = 30;
  pronto.characterHeight = 17;
  pronto.uiLibraryPageSize = 15;
  pronto.uiPlaylistPageSize = 11;

#endif
