import os
import os.path
import shutil
import SCons.Builder
import SCons.Node.FS
import SCons.Util
import Utils

def preBuildStep(target, source, env):
    # remove the existing .app folder
    shutil.rmtree(str(target[0]), True)
    
def copy_resources(target, source, env):
    resources = env.get('RESOURCES', [])
    if resources != []:
        Utils.CopyDictionary(resources, str(target[0]))

mtouchcom = "$MTOUCH $MTOUCHFLAGS $TARGET $SOURCES $_CLILIBS"
optimisecom = "$OPTIMISE $TARGET"
codesigncom = "$CODESIGN $CODESIGNFLAGS $TARGET"

MtouchBuilder = SCons.Builder.Builder(action = [preBuildStep, SCons.Defaults.Mkdir('$TARGET'), '$MTOUCHCOM', copy_resources, '$OPTIMISECOM', '$CODESIGNCOM'],
                                      source_factory = SCons.Node.FS.default_fs.File,
                                      target_factory = SCons.Node.FS.default_fs.Dir,
                                      prefix = '$MTOUCHPREFIX',
                                      suffix = '$MTOUCHSUFFIX')

def _parselibs(prefix, list, suffix, stripprefix, stripsuffix, libpath, env, c=None):
    if not list:
        return list

    if SCons.Util.is_List(list):
        list = SCons.Util.flatten(list)

    stripped = []
    for l in SCons.PathList.PathList(list).subst_path(env, None, None):
        if isinstance(l, SCons.Node.FS.File):
            stripped.append(l)
            continue
        if not SCons.Util.is_String(l):
            l = str(l)
        stripped.append(l)

    libs = []
    for s in stripped:
        for p in libpath:
            f = os.path.join(env.subst(p), s + env.subst(suffix))
            if os.path.exists(f):
                libs.append(env.subst(prefix) + f)
                continue

    return libs


def generate(env):
    env['BUILDERS']['Mtouch'] = MtouchBuilder

    env['MTOUCH']         = env.Detect('/Developer/MonoTouch/usr/bin/mtouch')
    env['_CLILIBS']       = "${_parselibs('-r:$CLILIBPREFIX', CLILIBS, '$CLILIBSUFFIX', '-r', '', CLILIBPATH + ['/Developer/MonoTouch/usr/lib/mono/2.1'], __env__)}"
    env['MTOUCHFLAGS']    = SCons.Util.CLVar('-v --nomanifest --nosign -linksdkonly -sdk="$ios_sdk" -targetver="3.2" -aot "nimt-trampolines=2048"')
    env['MTOUCHCOM']      = SCons.Action.Action(mtouchcom)
    # these should be under platform specific file
    env['MTOUCHPREFIX']   = ''
    env['MTOUCHSUFFIX']   = '.app'
    env['_parselibs']     = _parselibs
    
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
    
