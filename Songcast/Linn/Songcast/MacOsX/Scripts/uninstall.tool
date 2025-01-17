#!/bin/sh


launchFile="/Library/LaunchAgents/uk.co.linn.songcast.plist"
driver="/System/Library/Extensions/LinnSongcast.kext"
prefs="/Library/PreferencePanes/LinnSongcast.prefPane"
app="/Library/Linn/LinnSongcast.app"
pkgId="uk.co.linn.songcast"


# stop the agent application

echo
echo Stopping application...
echo

if [ -e "$launchFile" ]
then
  launchctl unload "$launchFile" > /dev/null 2>&1
fi


# unload the driver

echo
echo Unloading driver...
echo

if [ -e "$driver" ]
then
  sudo kextunload "$driver" > /dev/null 2>&1
  if [ $? != 0 ]
  then
    sudo kextunload "$driver" > /dev/null 2>&1
  fi
fi


# remove installed files

echo
echo Removing files...
echo

sudo rm -rf "$launchFile"
sudo rm -rf "$driver"
sudo rm -rf "$prefs"
sudo rm -rf "$app"


# clean pkg install receipts

sudo pkgutil --forget "$pkgId" > /dev/null 2>&1


echo
echo -------------------------------------------------------
echo Please restart you machine to complete the installation
echo -------------------------------------------------------
echo


