Import('env')

import SCons.Node
import os
import os.path

def NsiScriptBuilder(target, source, env):
    
    d = {
         'product'   : env['PRODUCT'],
         'version'   : env['VERSION'],
         'outfile'   : env['OUTFILE'],
         'install'   : [],
         'delete'    : []
        }

    try:
        d['launch'] = os.path.split(os.path.normpath(env['LAUNCHFILE']))[1]
        d['launchDir'] = os.path.dirname(os.path.normpath(env['LAUNCHFILE']))
    except:
        pass
    try:
        for k in env['RESOURCES'].keys():
            env['RESOURCES'][k] = os.path.normpath(env['RESOURCES'][k])
        d.update(env['RESOURCES'])
    except:
        pass
    
    templateFile = open(str(env['TEMPLATE']), 'rb')
    template = templateFile.read()
    templateFile.close()
    
    for f in source:
        if isinstance(f, SCons.Node.FS.File):
            d['install'].append("SetOutPath \"$INSTDIR\\" + env['BUNDLE_TARGET_DIRS'][repr(f)] + "\"")
            d['install'].append("File " + str(f))
        if isinstance(f, SCons.Node.FS.Dir):
            d['install'].append("SetOutPath \"$INSTDIR\\" + env['BUNDLE_TARGET_DIRS'][repr(f)] + "\"")
            d['install'].append("File /r " + str(f))
    d['install'] = '\n  '.join(d['install'])
    for f in source:
        if isinstance(f, SCons.Node.FS.File):
            d['delete'].append("Delete \"$INSTDIR\\" + env['BUNDLE_TARGET_DIRS'][repr(f)] + "\\" + os.path.split(str(f))[1] + "\"")
        if isinstance(f, SCons.Node.FS.Dir):
            d['delete'].append("RMDir /r \"$INSTDIR\\" + env['BUNDLE_TARGET_DIRS'][repr(f)] + "\\" + os.path.split(str(f))[1] + "\"")
    
    dir = []
    for p in env['BUNDLE_TARGET_DIRS'].values():
        temp = os.path.normpath(p)
        while 1:
            if not dir.__contains__(temp):
                dir.append(temp)
                d['delete'].append("RMDir \"$INSTDIR\\" + os.path.normpath(temp) + "\"")
            temp = os.path.dirname(temp)
            if(temp == ""):
                break
    d['delete'] = '\n  '.join(d['delete'])
    
    file = open(str(target[0]), 'wb')
    file.write(template % d)
    file.close()
    return 0

def NsiScriptBuilderString(target, source, env):
    return 'Generating NSI script: "%s"' % target[0]

nsiScriptAction = Action(NsiScriptBuilder, NsiScriptBuilderString)
env['BUILDERS']['NsiScript'] = Builder(action = nsiScriptAction, suffix = '.nsi')

Return('env')
