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

function GoWaf {
	echo GoWaf $3
	cd $1
	cd $3
	debug=""
	if [ "$4" == "0" ] ; then
		 debug="--debug"
	fi
    #TODO: BUG IN utilfuncs prevents this working until AW/JH have fixed this (v15?)
    #./go --clean --all
	./waf configure --dest-platform=Mac-x86 --ohnet=../ohNet $debug
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

	#songbox
	cp ohNet/Build/Obj/Mac-x86/$variant/ohNet.net.dll $2/Songbox/OpenHome/MacOsX
	cp ohNet/Build/Obj/Mac-x86/$variant/libohNet.dylib $2/Songbox/OpenHome/MacOsX
	cp ohNet/Build/Obj/Mac-x86/$variant/DvAvOpenhomeOrgPlaylistManager1.net.dll $2/Songbox/OpenHome/MacOsX
	cp ohNet/Build/Obj/Mac-x86/$variant/DvUpnpOrgConnectionManager1.net.dll $2/Songbox/OpenHome/MacOsX
	cp ohNet/Build/Obj/Mac-x86/$variant/DvUpnpOrgContentDirectory1.net.dll $2/Songbox/OpenHome/MacOsX
	cp ohGit/Build/Obj/Mac-x86/$variant/ohGit.dll $2/Songbox/OpenHome/MacOsX
	cp ohXapp/Build/Obj/Mac-x86/$variant/ohXapp.dll $2/Songbox/OpenHome/MacOsX
	cp ohXapp/Build/Obj/Mac-x86/$variant/ohXappViewer.dll $2/Songbox/OpenHome/MacOsX	
	cp ohXen/Build/Obj/Mac-x86/$variant/ohXen.dll $2/Songbox/OpenHome/MacOsX
	cp ohPlaylistManager/Build/Obj/Mac-x86/$variant/ohPlaylistManager.dll $2/Songbox/OpenHome/MacOsX
	cp ohMediaToolbox/Build/Obj/Mac-x86/$variant/ohMediaServer.dll $2/Songbox/OpenHome/MacOsX

	#songcast
	cp ohXen/Build/Obj/Mac-x86/$variant/ohXen.dll $2/Songcast/OpenHome/MacOsX
	cp ohXapp/Build/Obj/Mac-x86/$variant/ohXapp.dll $2/Songcast/OpenHome/MacOsX
	cp ohXapp/Build/Obj/Mac-x86/$variant/ohXappViewer.dll $2/Songcast/OpenHome/MacOsX
	cp -R ohSongcast/ohSongcast/Mac/Driver $2/Songcast/OpenHome/MacOsX
	cp ohSongcast/ohSongcast/Mac/LaunchAgent.plist $2/Songcast/OpenHome/MacOsX
	cp ohSongcast/ohSongcast/Mac/PkgInfo.plist $2/Songcast/OpenHome/MacOsX
	cp -R ohSongcast/ohSongcast/Mac/Scripts $2/Songcast/OpenHome/MacOsX
	cp ohSongcast/Build/Obj/Mac-x86/$variant/ohSongcast.net.dll $2/Songcast/OpenHome/MacOsX
	cp ohSongcast/Build/Obj/Mac-x86/$variant/libohsongcast.so $2/Songcast/OpenHome/MacOsX

	
	#wizard
	cp ohXen/Build/Obj/Mac-x86/$variant/ohXen.dll $2/Wizard/OpenHome/MacOsX
	cp ohXapp/Build/Obj/Mac-x86/$variant/ohXapp.dll $2/Wizard/OpenHome/MacOsX
	cp ohXapp/Build/Obj/Mac-x86/$variant/ohXappViewer.dll $2/Wizard/OpenHome/MacOsX
	
    #konfig
    cp ohNet/Build/Obj/Mac-x86/$variant/CpAvOpenhomeOrgProduct1.net.dll $2/Konfig/OpenHome/MacOsX
    cp ohNet/Build/Obj/Mac-x86/$variant/libOhNet.dylib $2/Konfig/OpenHome/MacOsX     
    cp ohNet/Build/Obj/Mac-x86/$variant/ohNet.net.dll $2/Konfig/OpenHome/MacOsX    
    #TODO: not built in ohNet cp ohNet/Build/Obj/Mac-x86/$variant/CpLinnCoUkDiagnostics1.net.dll $2/Konfig/OpenHome/MacOsX
	#TODO: not built in ohNet cp ohNet/Build/Obj/Mac-x86/$variant/CpLinnCoUkVolkano1.net.dll $2/Konfig/OpenHome/MacOsX
	cp ohXen/Build/Obj/Mac-x86/$variant/ohXen.dll $2/Konfig/OpenHome/MacOsX
	cp ohXapp/Build/Obj/Mac-x86/$variant/ohXapp.dll $2/Konfig/OpenHome/MacOsX
	cp ohXapp/Build/Obj/Mac-x86/$variant/ohXappViewer.dll $2/Konfig/OpenHome/MacOsX
	cd $2
}


cwd=$( pwd )

if [ "$buildOhNet" == "1" ] ; then
    Make $openHomeDir $cwd ohNet $release
fi
Make $openHomeDir $cwd ohGit $release
Make $openHomeDir $cwd ohXen $release
Make $openHomeDir $cwd ohXapp $release
GoWaf $openHomeDir $cwd ohTopology $release
Make $openHomeDir $cwd ohPlaylistManager $release
Make $openHomeDir $cwd ohMediaToolbox $release
Make $openHomeDir $cwd ohNetmon $release
Make $openHomeDir $cwd ohSongcast $release

CopyFiles $openHomeDir $cwd $release

