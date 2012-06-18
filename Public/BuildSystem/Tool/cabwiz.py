import os.path
import SCons.Builder
import SCons.Node.FS
import SCons.Util

cabwizcom = "$CABWIZ $SOURCES /dest ${TARGET.dir}"

CabWizBuilder = SCons.Builder.Builder(action = '$CABWIZCOM',
                                      src_suffix = '.inf',
                                      prefix = '$CABWIZPROGPREFIX',
                                      suffix = '$CABWIZPROGSUFFIX')

def generate(env):
    env['BUILDERS']['CabWiz'] = CabWizBuilder

    env['CABWIZ']           = 'cabwiz'
    env['CABWIZCOM']        = SCons.Action.Action(cabwizcom)
    env['CABWIZPROGPREFIX'] = ''
    env['CABWIZPROGSUFFIX'] = '.cab'

    env.PrependENVPath('PATH', r'C:\Program Files\Microsoft Visual Studio 9.0\SmartDevices\SDK\SDKTools')
    env.PrependENVPath('PATH', r'C:\Program Files (x86)\Microsoft Visual Studio 9.0\SmartDevices\SDK\SDKTools')

def exists(env):
    return env.Detect('cabwiz')
    
