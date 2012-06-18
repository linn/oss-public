import os.path
import SCons.Builder
import SCons.Node.FS
import SCons.Util
import CsProg

ibtcom = "$IBT $IBTFLAGS --output-format $IBTOUTFORMAT --compile $TARGET $SOURCES"

IbtBuilder = SCons.Builder.Builder(action = '$IBTCOM',
                                   source_factory = SCons.Node.FS.default_fs.Entry,
                                   prefix = '$IBTOOLPREFIX',
                                   suffix = '$IBTOOLSUFFIX')

def generate(env):
    env['BUILDERS']['IbTool'] = IbtBuilder

    env['IBT']            = env.Detect('ibtool')
    env['IBTFLAGS']       = SCons.Util.CLVar('--errors --warnings --notices')
    env['IBTOUTFORMAT']   = SCons.Util.CLVar('human-readable-text')
    env['IBTCOM']         = SCons.Action.Action(ibtcom)
    env['IBTOOLPREFIX']  = ''
    env['IBTOOLSUFFIX']  = '.nib'
    
def exists(env):
    return env.Detect('ibtool')
    
