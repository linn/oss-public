#!/bin/sh

# Script used for copying source and binaries from OpenHome

SCRIPTDIRECTORY=`dirname "$0"`

wget http://www.openhome.org/releases/ohSongcast.dll -nv -O $SCRIPTDIRECTORY/ohSongcast.dll
wget http://www.openhome.org/releases/ohSongcast.net.dll -nv -O $SCRIPTDIRECTORY/ohSongcast.net.dll

wget http://www.openhome.org/releases/Install32.exe -nv -O $SCRIPTDIRECTORY/Driver/Install32.exe
wget http://www.openhome.org/releases/Install64.exe -nv -O $SCRIPTDIRECTORY/Driver/Install64.exe
wget http://www.openhome.org/releases/ohSongcast32.sys -nv -O $SCRIPTDIRECTORY/Driver/ohSongcast32.sys
wget http://www.openhome.org/releases/ohSongcast64.sys -nv -O $SCRIPTDIRECTORY/Driver/ohSongcast64.sys

