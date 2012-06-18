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

def build(path, extraArgs=None):
    
    externalProgram = 'scons'

    #navigate to folder to build at
    for folder in path:
        os.chdir(folder)

    #pass args that are specific for build script
    if extraArgs:
        for i in range(0, len(extraArgs)):
           externalProgram +=  ' ' + extraArgs[i]

    #pass command arguments
    for i in range(1, len(sys.argv)):
        #if build arguemnts override command line arguements use build arguements 
        key = sys.argv[i].split('=')[0]
        if externalProgram.find(key) != -1:
            continue        
        externalProgram +=  ' ' + sys.argv[i]  
        
    retcode = subprocess.call(externalProgram, shell=True)

    #return to path before function call
    for folder in path:
        os.chdir('..')

    #if an error occurred in the build exit
    if not retcode == 0:
        exit()

#get hardware command line argument
hardware = 'hardware=Windows'
for i in range(1, len(sys.argv)):
    if sys.argv[i].startswith('hardware'):
        hardware = sys.argv[i]
        break

build(['Docs'])
build(['Xml'])
build(['Kodegen'])
build(['External'])
build(['LibUpnpCil','Core'],[hardware])
build(['LibUpnpCil','SysLib'],[hardware])
build(['LibUpnpCil','Control'],[hardware])
build(['LibUpnpCil','Services'],[hardware])
build(['LibUpnpCil','DidlLite'],[hardware])
