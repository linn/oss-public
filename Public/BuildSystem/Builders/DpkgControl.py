Import('env')

import os
import SCons.Node

CONTROLTEMPLATE = """
Package: %s
Priority: extra
Section: misc
Installed-Size: %s
Maintainer: %s
Architecture: %s
Version: %s-1
Depends: %s
Description: %s

"""

def DpkgControlBuilder(target, source, env): 
    installed_size = 0
    for f in source:
        if isinstance(f, SCons.Node.FS.File):
            installed_size += os.stat(str(env.File(f)))[6]
        if isinstance(f, SCons.Node.FS.Dir):
            installed_size += os.stat(str(env.Dir(f)))[6]
    
    control_info = CONTROLTEMPLATE % (env['PRODUCT'].lower(),
                                      installed_size,
                                      env['MAINTAINER'],
                                      env['ARCHITECTURE'],
                                      env['VERSION'],
                                      env['DEPENDENCIES'],
                                      env['DESCRIPTION'])
    
    f = open(str(target[0]), 'w')
    f.write(control_info)
    f.close()
    
    return 0

def DpkgControlBuilderString(target, source, env):
    return 'Generating dpkg control script: "%s"' % target[0]

dpkgControlAction = Action(DpkgControlBuilder, DpkgControlBuilderString)
env['BUILDERS']['DpkgControl'] = Builder(action = dpkgControlAction, suffix = '')

Return('env')