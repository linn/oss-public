#!/bin/sh

#-----------------------------------------------------
# Info on updating from OpenHome Songcast
#-----------------------------------------------------
#
# To update from OpenHome:
#
# 1. Checkout the ohnet, ohNetmon, ohSongcast, ohXen and ohXapp projects to the same folder e.g.:
#
#    ~/Work/OpenHome/ohnet
#                   /ohNetmon
#                   /ohSongcast
#                   /ohXen
#                   /ohXapp
#
# 2. Build **release** versions of all OpenHome code e.g.
#
#    > cd ~/Work/OpenHome/ohnet
#    > make release=1
#    > cd ../ohNetmon
#    > make release=1
#    > cd ../ohXen
#    > make release=1
#    > cd ../ohXapp
#    > make release=1
#    > cd ../ohSongcast
#    > make release=1
#    > cd ohSongcast/Mac
#    > make release=1
#
# 3. From this directory, run the update script:
#
#    > ./UpdateFromOpenHome.sh ~/Work/OpenHome
#
#    This will copy the released OpenHome code into this folder.
#
# 4. Typing "svn st" will list the changes that have occurred to the openhome release.
#
#
# When building the Linn Songcast app, the files in the ohSongcast folder fall into 3 categories:
#
# 1. Files that are copied into the build tree e.g. ohSongcast.net.dll
# 2. Files that are compiled into the build tree e.g. driver .cpp files
# 3. Files that are replaced with different versions that are stored in this folder
#
# If files in categories 1 & 2 change, no further action is required. If files in category 3 have changed, then these files
# must be diffed and the changes applied to their versions used by the builder. The files in this category and their replacements
# are:
#
# Original OpenHome file (relative to this folder)       Linn Songcast equivalent file (in OSS_ROOT/Songcast/Linn/Songcast/MacOsX)
# ----------------------------------------------------------------------------------------------
# LaunchAgent.plist                                      LaunchAgent.plist
# PkgInfo.plist                                          PkgInfo.plist
# Scripts/postflight                                     Scripts/postflight
# Scripts/preflight                                      Scripts/preflight
# Scripts/uninstall.tool                                 Scripts/uninstall.tool
# Driver/Info.plist                                      InfoDriver.plist
# Driver/Makefile                                        SConscript
#
# If any of the files in the left column change, their diffs have to be applied to the equivalent files on the right. Note that this
# might not be a simple matter of copy and paste. The reason these files are different are because the contain branding information
# for the software. So, for example, if a new XML field is added to the Driver/Info.plist file that has a value of "av.openhome.org",
# that new field will need to be added to the InfoDriver.plist file, but the value will probably be "linn.co.uk" or something similar.
# Also note that the last file in that list, the Makefile, does not correspond directly to the SConscript file. This is more to indictate
# that if the driver Makefile changes, there will be possible changes to the SConscript file required.

if [ -z $1 ]
then
  echo Must specify location of OpenHome repositories as an argument
  exit 1
fi

SOURCE_ROOT="$1"/ohSongcast/ohSongcast/Mac

cp -R $SOURCE_ROOT/Driver .
cp $SOURCE_ROOT/LaunchAgent.plist .
cp $SOURCE_ROOT/PkgInfo.plist .
cp -R $SOURCE_ROOT/Scripts .
cp -R "$1"/ohSongcast/Build/Obj/Mac-x86/Release/ohNet.net.dll .
cp -R "$1"/ohSongcast/Build/Obj/Mac-x86/Release/ohSongcast.net.dll .
cp -R "$1"/ohSongcast/Build/Obj/Mac-x86/Release/libohSongcast.so .
cp "$1"/ohXapp/Build/Obj/Mac-x86/Release/ohXapp.dll .
cp "$1"/ohXapp/Build/Obj/Mac-x86/Release/ohXappViewer.dll .
cp "$1"/ohXen/Build/Obj/Mac-x86/Release/ohXen.dll .


