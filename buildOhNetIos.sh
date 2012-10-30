#!/bin/bash

openHomeDir=../../openhome
release=1

function MakeOhNet {
	echo make $3 mac-arm=1 release=$4
	cd $1
	cd $3
	make clean mac-arm=1 release=$4
	make mac-arm=1 release=$4 # this will fail building the dylib, that's ok...
	make mac-arm=1 release=$4 ohNet.net.dll # add this to continue from where we left off
	make mac-arm=1 release=$4 CpProxyDotNetAssemblies # add this to continue from where we left off
	make mac-arm=1 release=$4 DvDeviceDotNetAssemblies # add this to continue from where we left off
	cd $2
}

function Make {
	echo make $3 ios-arm=1 release=$4
	cd $1
	cd $3
	make clean ios-arm=1 release=$4
	make ios-arm=1 release=$4
	cd $2
}

function CopyFiles {
	echo Copying Libraries
	cd $1
	variant="Release"
	if [ "$3" == "0" ] ; then
		 variant="Debug"
	fi

	
	#wizard
	cp ohXen/Build/Obj/Ios/$variant/ohXen.dll $2/Wizard/OpenHome/Ios
	cp ohXapp/Build/Obj/Ios/$variant/ohXapp.dll $2/Wizard/OpenHome/Ios
	cp ohXapp/Build/Obj/Ios/$variant/ohXappViewer.dll $2/Wizard/OpenHome/Ios
    
    #konfig
    cp ohNet/Build/Obj/Mac/arm/$variant/CpAvOpenhomeOrgProduct1.net.dll $2/Konfig/OpenHome/Ios
    cp ohNet/Build/Obj/Mac/arm/$variant/libohNetCore.a $2/Konfig/OpenHome/Ios    
    cp ohNet/Build/Obj/Mac/arm/$variant/libohNetDevices.a $2/Konfig/OpenHome/Ios    
    cp ohNet/Build/Obj/Mac/arm/$variant/libohNetProxies.a $2/Konfig/OpenHome/Ios    
    cp ohNet/Build/Obj/Mac/arm/$variant/ohNet.net.dll $2/Konfig/OpenHome/Ios    
    #TODO: not built in ohNet cp ohNet/Build/Obj/Mac/arm/$variant/CpLinnCoUkDiagnostics1.net.dll $2/Konfig/OpenHome/Ios
	#TODO: not built in ohNet cp ohNet/Build/Obj/Mac/arm/$variant/CpLinnCoUkVolkano1.net.dll $2/Konfig/OpenHome/Ios
	cp ohXen/Build/Obj/Ios/$variant/ohXen.dll $2/Konfig/OpenHome/Ios
	cp ohXapp/Build/Obj/Ios/$variant/ohXapp.dll $2/Konfig/OpenHome/Ios
	cp ohXapp/Build/Obj/Ios/$variant/ohXappViewer.dll $2/Konfig/OpenHome/Ios
	
}


cwd=$( pwd )

MakeOhNet $openHomeDir $cwd ohNet $release
Make $openHomeDir $cwd ohXen $release
Make $openHomeDir $cwd ohXapp $release

CopyFiles $openHomeDir $cwd $release

