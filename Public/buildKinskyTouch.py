#! /usr/bin/env python

import os
import sys
import subprocess

def usage():
    print "usage:"
    print "$0 variant [target] [-c]"
    print "variant: scons configuration (ex variant=release)"
    print "target: optional target for all subdirs (ex Lib)"
    print "-c: optional clean"
    
if (sys.argv[1] == "--help"): 
    usage()
    sys.exit()

if (sys.argv[1] == "-h"): 
    usage()
    sys.exit()

def build(path, args=''):
    
    externalProgram = 'scons hardware=Ios'

    #navigate to folder to build at
    for folder in path:
        os.chdir(folder)

    if (args != ''):
        externalProgram += ' ' + args

    #pass command arguments
    for i in range(1, len(sys.argv)):
        externalProgram += ' ' + sys.argv[i]
    retcode = subprocess.call(externalProgram, shell=True)

    #return to path before function call
    for folder in path:
        os.chdir('..')

    if not retcode == 0:
        exit()

build(['Docs'])
build(['Xml'])
build(['Kodegen'])
build(['External'])
build(['LibUpnpCil','Core'])
build(['LibUpnpCil','Toolkit'], 'OssToolkitIos')
build(['LibUpnpCil','SysLib'])
build(['LibUpnpCil','Control'])
build(['LibUpnpCil','Services'])
build(['LibUpnpCil','DidlLite'])
build(['LibUpnpCil','Topology'])
build(['Kinsky'])
#build(['MediaProviderPlugins'])
build(['Layouts'])
build(['KinskyDesktop'])
