#!/bin/sh

# Script used for copying source and binaries from the OpenHome build

if [ -z $1 ]
then
  echo Must specify location of ohSongcast repository root as an argument
  exit 1
fi

BUILD_ROOT="$1"/ohSongcast/Mac/Build/Obj/Mac/Release
SOURCE_ROOT="$1"/ohSongcast/Mac

cp -R $BUILD_ROOT/x86_64/App/ohSongcast.app App/
cp $SOURCE_ROOT/Driver/* Driver/
cp $SOURCE_ROOT/LaunchAgent.plist .
cp $SOURCE_ROOT/PkgInfo.plist .
cp -R $BUILD_ROOT/x86_64/Prefs/ohSongcast.prefPane Prefs/
cp -R $SOURCE_ROOT/Scripts .

