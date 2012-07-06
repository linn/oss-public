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
    
    externalProgram = 'scons'

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
        sys.exit(retcode)

build(['Docs'], 'Lib')
build(['External'], 'Lib')
build(['Xml'], 'Lib')
build(['Kodegen'], 'Lib')
build(['LibUpnpCil','Core'], 'Lib')
build(['LibUpnpCil','Toolkit'], 'OssToolkitWinForms')
build(['LibUpnpCil','SysLib'], 'Lib')
build(['LibUpnpCil','Control'], 'Lib')
build(['LibUpnpCil','Services'], 'Lib')
build(['LibUpnpCil','DidlLite'], 'Lib')
build(['LibUpnpCil','Topology'], 'Lib')
build(['Kinsky'], 'Lib')
build(['ProductSupport'], 'Lib')

if (sys.platform == 'win32') or (sys.platform == 'darwin'):
     print 'Warning: Web app not built; this is only possible under linux.'
else:
     build(['Web'], 'Lib') 
    
build(['Gui'], 'Lib')
build(['Layouts'], 'Lib')
build(['Kinsky'], 'Lib')
