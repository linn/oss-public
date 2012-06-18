
import os.path
Import('env')


def MacOsXDmgBuild(target, source, **kw):

    dmg = env.Command(target + '.dmg', source, 'hdiutil create -ov $TARGET -volname ' + kw['VOLNAME'] + ' -fs HFS+ -srcfolder $SOURCE')

    return dmg


env.MacOsXDmgBuild = MacOsXDmgBuild

Return('env')


