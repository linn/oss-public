import os.path
import SCons.Builder
import SCons.Node.FS
import SCons.Util

kodegencom = "mono $KODEGEN $TARGET $SOURCES $ARGS"

def kodegen_emitter(target, source, env):
    source.insert(0, env.subst('$hardware_dir/share/Kodegen/$KODE'))
    return (target, source)

KodegenBuilder = SCons.Builder.Builder(action = '$KODEGENCOM',
                                  source_factory = SCons.Node.FS.default_fs.Entry,
                                  emitter = kodegen_emitter)

def generate(env):
    env['BUILDERS']['Kodegen'] = KodegenBuilder
    env['KODEGEN'] = '$install_dir/bin/Kodegen.exe'
    env['KODEGENCOM'] = SCons.Action.Action(kodegencom)

def exists(env):
    return env.Detect('$install_dir/bin/Kodegen.exe')    
    
