#!/bin/bash

openHomeDir=../../openhome
release=1
buildOhNet=1

function Make {
	echo make $3 release=$4
	cd $1
	cd $3
	make clean release=$4
	make release=$4
	cd $2
}

function Waf {
	echo waf $3
	cd $1
	cd $3
	debug=""
	if [ "$4" == "0" ] ; then
		 debug="--debug"
	fi
    ./go --fetch --all
	./waf configure --dest-platform=Linux-x86 --ohnet=../ohNet $debug
	./waf clean build
	cd $2
}

function CopyFiles {
	echo Copying Libraries
	cd $1
	variant="Release"
	if [ "$3" == "0" ] ; then
		 variant="Debug"
	fi
	
	#konfig
    cp ohNet/Build/Obj/Posix/$variant/CpAvOpenhomeOrgProduct1.net.dll $2/Konfig/OpenHome/Linux
    cp ohNet/Build/Obj/Posix/$variant/libohNet.so $2/Konfig/OpenHome/Linux
    cp ohNet/Build/Obj/Posix/$variant/ohNet.net.dll $2/Konfig/OpenHome/Linux
    #TODO: not built in ohNet cp ohNet/Build/Obj/Posix/$variant/CpLinnCoUkDiagnostics1.net.dll $2/Konfig/OpenHome/Linux
	#TODO: not built in ohNet cp ohNet/Build/Obj/Linux/$variant/CpLinnCoUkVolkano1.net.dll $2/Konfig/OpenHome/Linux
	cp ohXen/Build/Obj/Posix/$variant/ohXen.dll $2/Konfig/OpenHome/Linux
	cp ohXapp/Build/Obj/Posix/$variant/ohXapp.dll $2/Konfig/OpenHome/Linux
	cp ohXapp/Build/Obj/Posix/$variant/ohXappViewer.dll $2/Konfig/OpenHome/Linux
	cd $2
}


cwd=$( pwd )

if [ "$buildOhNet" == "1" ] ; then
    Make $openHomeDir $cwd ohNet $release
fi
#Make $openHomeDir $cwd ohGit $release
Make $openHomeDir $cwd ohXen $release
Make $openHomeDir $cwd ohXapp $release
#Waf $openHomeDir $cwd ohTopology $release
#Make $openHomeDir $cwd ohPlaylistManager $release
#Make $openHomeDir $cwd ohMediaToolbox $release
#Make $openHomeDir $cwd ohNetmon $release
#Make $openHomeDir $cwd ohSongcast $release

CopyFiles $openHomeDir $cwd $release

