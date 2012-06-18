import os.path
import SCons.Builder
import SCons.Node.FS
import SCons.Util
import CsProg

resgencom = "$RESGEN -usesourcepath $SOURCES ${TARGET.abspath}"

ResgenBuilder = SCons.Builder.Builder(action = '$RESGENCOM',
                                      suffix = '$RESGENSUFFIX',
                                      src_suffix = ['.txt', '.resx'],
                                      single_source = 1)

def generate(env):
    env['BUILDERS']['Resgen'] = ResgenBuilder

    env['RESGEN']         = env.Detect('resgen2')
    env['RESGENCOM']      = SCons.Action.Action(resgencom)
    env['RESGENSUFFIX']   = '.resources'

def exists(env):
    return env.Detect('resgen2')
    
