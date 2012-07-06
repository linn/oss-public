import os.path
import sys
import SCons.Builder
import SCons.Node.FS
import SCons.Util
import CsProg

CsProgramScanner = CsProg.CsProgramScanner()

msbuildcom = "$MSBuild $SOURCES /p:Configuration=$Configuration $MSBuildFlags"

MSBuildExeBuilder = SCons.Builder.Builder(action = '$MSBuildCOM', 
                                  suffix = '$MSPROGSUFFIX',
                                  target_scanner = CsProgramScanner)
MSBuildLibBuilder = SCons.Builder.Builder(action = '$MSBuildCOM', 
                                  suffix = '$MSLIBSUFFIX',
                                  target_scanner = CsProgramScanner)
MSBuildApkBuilder = SCons.Builder.Builder(action = '$MSBuildCOM', 
                                  suffix = '$APKSUFFIX',
                                  target_scanner = CsProgramScanner)

def generate(env):
    env['BUILDERS']['MSBuildExeBuilder'] = MSBuildExeBuilder	
    env['BUILDERS']['MSBuildLibBuilder'] = MSBuildLibBuilder
    env['BUILDERS']['MSBuildApkBuilder'] = MSBuildApkBuilder	
    env['MSBuild']            = 'MSBuild' if sys.platform == 'win32' else 'mdtool build' if sys.platform == 'darwin' else 'xbuild'
    env['MSBuildFlags']       = SCons.Util.CLVar('')
    env['MSBuildCOM']         = SCons.Action.Action(msbuildcom)
    env['MSPROGSUFFIX']   = '.exe'
    env['MSLIBSUFFIX']   = '.dll'
    env['APKSUFFIX']   = '.apk'
    if env['variant'] == "debug":
        env['Configuration'] = 'Debug'
    if env['variant'] == "trace":
        env['Configuration'] = 'Trace'
    if env['variant'] == "release":
        env['Configuration'] = 'Release'
    if env['variant'] == "":
        env['Configuration'] = 'Release'

def exists(env):
    return env.Detect('MSBuild')
