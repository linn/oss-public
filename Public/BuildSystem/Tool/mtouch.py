import os
import os.path
import shutil
import SCons.Builder
import SCons.Node.FS
import SCons.Util

def copy_resources(target, source, env):
    for f in env.Flatten(env['RESOURCES']):
        print os.path.abspath(str(f))
        shutil.copyfile(str(f), os.path.join(str(target[0]), os.path.basename(str(f))))

mtouchcom = "$MTOUCH $MTOUCHFLAGS $TARGET $SOURCES $_CLILIBS"
optimisecom = "$OPTIMISE $TARGET"
codesigncom = "$CODESIGN $CODESIGNFLAGS $TARGET"

MtouchBuilder = SCons.Builder.Builder(action = [SCons.Defaults.Mkdir('$TARGET'), '$MTOUCHCOM', copy_resources, '$OPTIMISECOM', '$CODESIGNCOM'],
                                      source_factory = SCons.Node.FS.default_fs.File,
                                      target_factory = SCons.Node.FS.default_fs.Dir,
                                      prefix = '$MTOUCHPREFIX',
                                      suffix = '$MTOUCHSUFFIX')

def generate(env):
    env['BUILDERS']['Mtouch'] = MtouchBuilder

    env['MTOUCH']         = env.Detect('/Developer/MonoTouch/usr/bin/mtouch')
    env['_CLILIBS']       = "${_stripixes('-r:$CLILIBPREFIX', CLILIBS, '$CLILIBSUFFIX', '-r', '', __env__)}"
    env['MTOUCHFLAGS']    = SCons.Util.CLVar('-v --nomanifest --nosign -linksdkonly -sdk="$ios_sdk" -targetver="3.2" -aot "nimt-trampolines=2048"')
    env['MTOUCHCOM']      = SCons.Action.Action(mtouchcom)
    # these should be under platform specific file
    env['MTOUCHPREFIX']   = ''
    env['MTOUCHSUFFIX']   = '.app'
    
    env['OPTIMISE']       = env.Detect('/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer/usr/bin/iphoneos-optimize')
    if (env['OPTIMISE'] == None):
       env['OPTIMISE']       = env.Detect('/Developer/Platforms/iPhoneOS.platform/Developer/usr/bin/iphoneos-optimize')
    env['OPTIMISECOM']    = SCons.Action.Action(optimisecom)
    
    env['CODESIGN']       = env.Detect('codesign')
    env['CODESIGNFLAGS']  = SCons.Util.CLVar('-v -f -s "$IDENTITY" --resource-rules="$TARGET/ResourceRules.plist" --entitlements "$ENTITLEMENTS"')
    env['CODESIGNCOM']    = SCons.Action.Action(codesigncom)
    
    env['ENV']['HOME'] = os.environ['HOME']
    #env['ENV'] = os.environ
    
    if env['variant'] == "debug":
        env.Append(MTOUCHFLAGS = '-debug')
    if env['hardware'] == "Ios":
        env.Append(MTOUCHFLAGS = '-dev')
    if env['hardware'] == "IosSim":
        env.Append(MTOUCHFLAGS = '-sim')

def exists(env):
    return env.Detect('/Developer/MonoTouch/usr/bin/mtouch') and env.Detect('/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer/usr/bin/iphoneos-optimize') and evn.Detect('codesign')
    
