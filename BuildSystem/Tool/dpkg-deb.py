import os.path
import SCons.Builder
import SCons.Node.FS
import SCons.Util

dpkgdebcom = "$DPKGDEB $DPKGDEBFLAGS ${SOURCES.path.replace('/DEBIAN/control', '')} ${TARGET.abspath}"

DpkgDebBuilder = SCons.Builder.Builder(action = '$DPKGDEBCOM',
                                  source_factory = SCons.Node.FS.default_fs.Entry,
                                  src_suffix = '',
                                  prefix = '$DPKGDEBPROGPREFIX',
                                  suffix = '$DPKGDEBPROGSUFFIX')

def generate(env):
    env['BUILDERS']['DpkgDeb'] = DpkgDebBuilder

    env['DPKGDEB']            = 'fakeroot dpkg-deb'
    env['DPKGDEBFLAGS']       = SCons.Util.CLVar('-b')
    env['DPKGDEBCOM']         = SCons.Action.Action(dpkgdebcom)
    env['DPKGDEBPROGPREFIX']  = ''
    env['DPKGDEBPROGSUFFIX']  = '.deb'

def exists(env):
    return env.Detect('dpkg-deb')
    
