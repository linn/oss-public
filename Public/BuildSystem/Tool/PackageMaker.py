
import SCons.Node.FS
import SCons.Builder


# $TARGET is the name of the output pkg file
# $SOURCE is the .dst folder containing the stuff to package
#
# Requires the following params:
#
# TITLE           - the title to be displayed in the installer
# INFO_PLIST      - the .plist file for the package
# SCRIPT_DIR      - the folder containing the installer scripts
# VERSION         - the version of the application


PackageMakerCom = '$PACKAGEMAKER --root $SOURCE --out $TARGET --scripts $SCRIPT_DIR --info $INFO_PLIST --title $TITLE --version $VERSION --no-relocate'

PackageMakerBuilder = SCons.Builder.Builder(action = '$PACKAGEMAKERCOM',
                                            source_factory = SCons.Node.FS.default_fs.Dir,
                                            target_factory = SCons.Node.FS.default_fs.Dir,
                                            prefix = '$PACKAGEMAKERPREFIX',
                                            suffix = '$PACKAGEMAKERSUFFIX')

def generate(env):
    env['BUILDERS']['PackageMaker'] = PackageMakerBuilder

    env['PACKAGEMAKER']             = env.Detect('/Developer/Applications/Utilities/PackageMaker.app/Contents/MacOS/PackageMaker')
    env['PACKAGEMAKERCOM']          = SCons.Action.Action(PackageMakerCom)
    env['PACKAGEMAKERPREFIX']       = ''
    env['PACKAGEMAKERSUFFIX']       = '.pkg'


def exists(env):
    return env.Detect('/Developer/Applications/Utilities/PackageMaker.app/Contents/MacOS/PackageMaker')


