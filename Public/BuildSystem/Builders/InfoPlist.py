Import('env')

import plistlib
import re


def InfoPlistBuilder(target, source, env): 
    # load the source plist
    infoplist = plistlib.readPlist(str(source[0]))

    # get version information
    shortVersion = env.get('VERSION', '')
    buildVersion = re.match("[0-9]+", env.subst('$svn_rev')).group()

    # update some of the dictionary values
    infoplist['CFBundleInfoDictionaryVersion'] = '6.0'
    infoplist['CFBundleShortVersionString'] = shortVersion
    infoplist['CFBundleVersion'] = buildVersion

    # save the target
    plistlib.writePlist(infoplist, str(target[0]))
    
    return 0


def InfoPlistBuilderString(target, source, env):
    return 'Building .plist file: "%s" -> "%s"' % (source[0], target[0])


infoPlistAction = Action(InfoPlistBuilder, InfoPlistBuilderString)

env['BUILDERS']['InfoPlist'] = Builder(action = infoPlistAction, suffix = '')

Return('env')

