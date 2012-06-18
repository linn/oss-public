import os
import os.path
import shutil
import SCons.Builder
import SCons.Node.FS
import SCons.Util

productbuildcom = "$PRODUCTBUILD --component $SOURCE $INSTALLDIR $_PKGCERT $TARGET"

ProductBuildBuilder = SCons.Builder.Builder(action = ['$PRODUCTBUILDCOM'],
                                            source_factory = SCons.Node.FS.default_fs.Dir,
                                            target_factory = SCons.Node.FS.default_fs.File,
                                            prefix = '$PRODUCTBUILDPREFIX',
                                            suffix = '$PRODUCTBUILDSUFFIX')

def _parsearg(prefix, arg):
    return prefix + arg

def generate(env):
    env['BUILDERS']['ProductBuild'] = ProductBuildBuilder

    env['PRODUCTBUILD']             = env.Detect('productbuild')
    env['PRODUCTBUILDCOM']          = SCons.Action.Action(productbuildcom)
    env['INSTALLDIR']               = '/Applications'
    env['_PKGCERT']                 = "${_parsearg('--sign ', PKGCERT)}"

    env['PRODUCTBUILDPREFIX']       = ''
    env['PRODUCTBUILDSUFFIX']       = '.pkg'
    env['_parsearg']                = _parsearg

def exists(env):
    return env.Detect('productbuild')

