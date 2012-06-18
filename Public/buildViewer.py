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

def build(path):
    
    externalProgram = 'scons'

    #navigate to folder to build at
    for folder in path:
        os.chdir(folder)

    #pass command arguments
    for i in range(1, len(sys.argv)):
        externalProgram +=  ' ' + sys.argv[i]       

    #add target for OssCore, default is Test
    if len(path) == 2 and path[0] == 'LibUpnpCil' and path[1] == 'Core':
        externalProgram += ' ' + 'Lib'


    retcode = subprocess.call(externalProgram, shell=True)

    #return to path before function call
    for folder in path:
        os.chdir('..')

    if not retcode == 0:
        exit()

build(['Upnp'])
build(['ZappControl'])
build(['ZappServices'])
build(['ZappTopology'])
build(['External'])
build(['LibUpnpCil','Core'])
build(['Viewer'])
