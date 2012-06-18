////
//
// powerManagement.js
//
// NOTE: Because of race conditions, this module has been modified so that it will not
// power down the DS for for first 5 seconds after startup.
//
////

function gotoHomePage()
{
  LOG("Switching LPEC off.");
  try
  {
    lpec.byebye();
  }
  catch (e)
  {
    LOG("Error ["+e+"] switching off LPEC");
  };
  LOG("Switching Pronto off.");

#if (VARIANT_DS)
  CF.page("NowPlaying").widget("GOTO_HOME_PAGE").executeActions();
#elif (VARIANT_RADIO)
  CF.page("NowListening").widget("GOTO_HOME_PAGE").executeActions();
#elif (VARIANT_PREAMP)
  CF.page("PreAmpOnly").widget("GOTO_HOME_PAGE").executeActions();
#endif

};

var powerManagement = {}

// We need 'switchingOff' to stop recursion as we can get switch off events whilst we are switching off.
//
powerManagement.switchingOff = false;
powerManagement.switchingOn = false;

powerManagement.switchOn = function()
{
  powerManagement.switchingOn = true;
  lpecMessages.dsSetStandby(0);
  
  // This is necessary - think Akurate Kontrol MkII preamp.
  //
  if (configuration.preAmpType == 2)
  {
    lpecMessages.preampSetStandby(0);
  };

  try
  {
    Activity.scheduleAfter(1000, powerManagement.switchedOn);
  } catch (e) {
    GUI.alert("Activity.scheduleAfter[7a] " + e);
    LOG("Activity.scheduleAfter[7a] " + e);
  };
}

powerManagement.switchedOn = function()
{
  powerManagement.switchingOn = false;
};

powerManagement.switchOff = function()
{
  lpecMessages.dsSetStandby(1);
  if (configuration.preAmpType == 2)
  {
    // This is necessary - think IP preamp.
    //
    lpecMessages.preampSetStandby(1);
  };
  
  // Time for bed.
  //
  gotoHomePage();
};


powerManagement.lpecCallback = function(result)
{
  LOG("Standby:" + result);
  
  if ((powerManagement.switchingOff) || (powerManagement.switchingOn))
  {
    return;
  };                                 
  
  if (result == "false")
  {
    return;
  };
  
  powerManagement.switchingOff = true;
  powerManagement.switchOff();
};

powerManagement.delayedStarup = function()
{
  lpec.addEventCallback(powerManagement.lpecCallback, "ProductStandby", "Ds", "Product");
};

powerManagement._start = function()
{
  powerManagement.switchOn();

  try
  {
    Activity.scheduleAfter(5000, powerManagement.delayedStarup);
  } catch (e) {
    GUI.alert("Activity.scheduleAfter[7b] " + e);
    LOG("Activity.scheduleAfter[7b] " + e);
  };

};

elab.add("powerManagement", null, powerManagement._start);
