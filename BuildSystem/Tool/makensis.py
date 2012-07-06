import os
import os.path
import SCons.Builder
import SCons.Node.FS
import SCons.Util

makensiscom = "$MAKENSIS $MAKENSISFLAGS $SOURCES"
signtoolcom1 = '$SIGNTOOL $SIGNTOOLFLAGS $CERT $SIGNFILES'
signtoolcom2 = '$SIGNTOOL $SIGNTOOLFLAGS $CERT $TARGET'

NsisBuilder = SCons.Builder.Builder(action = ['$SIGNTOOLCOM1', '$MAKENSISCOM', '$SIGNTOOLCOM2'],
                                  source_factory = SCons.Node.FS.default_fs.Entry,
                                  src_suffix = '.nsis',
                                  prefix = '$MAKENSISPROGPREFIX',
                                  suffix = '$MAKENSISPROGSUFFIX')

def generate(env):
    env['BUILDERS']['MakeNsis'] = NsisBuilder

    env['MAKENSIS']           = 'makensis'
    env['MAKENSISFLAGS']      = SCons.Util.CLVar('-V2')
    env['MAKENSISCOM']        = SCons.Action.Action(makensiscom)
    env['MAKENSISPROGPREFIX'] = ''
    env['MAKENSISPROGSUFFIX'] = '.exe'
    
    env['SIGNTOOL']           = env.Detect('signtool')
    env['SIGNTOOLFLAGS']      = SCons.Util.CLVar('sign /sm /v /a /t http://timestamp.verisign.com/scripts/timestamp.dll')
    if os.sys.platform == 'win32' and env['codesign']:
        env['SIGNTOOLCOM1']       = SCons.Action.Action(signtoolcom1)
        env['SIGNTOOLCOM2']       = SCons.Action.Action(signtoolcom2)

def exists(env):
    return env.Detect('makensis')
    
