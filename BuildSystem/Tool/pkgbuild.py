
import SCons.Node.FS
import SCons.Builder


# $TARGET is the name of the output pkg file
# $SOURCE is the .dst folder containing the .app to package
#
# Requires the following params:
#
# ID              - the identifier for the pkg e.g. uk.co.linn.KinskyDesktop
# INSTALL_PATH    - the path that the dst will be installed to on running the pkg
# COMPONENT_PLIST - the component plist file for the pkg
# SCRIPT_DIR      - the folder containing the post install script


pkgBuildCom = '$PKGBUILD --identifier $ID --root $SOURCE --install-location $INSTALL_PATH --component-plist $COMPONENT_PLIST --scripts $SCRIPT_DIR $TARGET'

PkgBuildBuilder = SCons.Builder.Builder(action = '$PKGBUILDCOM',
                                        source_factory = SCons.Node.FS.default_fs.Dir,
                                        target_factory = SCons.Node.FS.default_fs.File,
                                        prefix = '$PKGBUILDPREFIX',
                                        suffix = '$PKGBUILDSUFFIX')

def generate(env):
    env['BUILDERS']['PkgBuild'] = PkgBuildBuilder

    env['PKGBUILD']             = env.Detect('pkgbuild')
    env['PKGBUILDCOM']          = SCons.Action.Action(pkgBuildCom)
    env['PKGBUILDPREFIX']       = ''
    env['PKGBUILDSUFFIX']       = '.pkg'


def exists(env):
    return env.Detect('pkgbuild')


