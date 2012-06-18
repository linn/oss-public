import os
import os.path
import shutil
import SCons.Builder
import SCons.Node.FS
import SCons.Util


INFO_PLIST_TEMPLATE = """<?xml version=\"1.0\" encoding=\"UTF-8\"?>
<!DOCTYPE plist PUBLIC \"-//Apple Computer//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">
<plist version=\"1.0\">
<dict>
    <key>CFBundleIdentifier</key>
    <string>uk.co.linn.%s</string>

    <key>CFBundleName</key>
    <string>%s</string>

    <key>CFBundleVersion</key>
    <string>%s</string>

    <key>CFBundleShortVersionString</key>
    <string>%s</string>

    <key>CFBundleExecutable</key>
    <string>%s</string>

    <key>MonoBundleExecutable</key>
    <string>%s</string>

    <key>CFBundleIconFile</key>
    <string>%s</string>

    <key>LSMinimumSystemVersion</key>
    <string>10.6.8</string>

    <key>CFBundleDevelopmentRegion</key>
    <string>English</string>

    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>

    <key>CFBundlePackageType</key>
    <string>APPL</string>

    <key>CFBundleSignature</key>
    <string>????</string>

    <key>NSHumanReadableCopyright</key>
    <string>Copyright 2011 Linn</string>
    
    <key>LSApplicationCategoryType</key>
    <string>public.app-category.music</string>
</dict>
</plist>
"""


def CopyFile(src, dst):
    print 'CopyFile(%s -> %s)' % (src, dst)
    shutil.copyfile(src, dst)


def CopyTree(src, dst):
    print 'CopyTree(%s -> %s)' % (src, dst)
    shutil.copytree(src, dst)


def CopyDictionary(dict, rootDstFolder):
    for (relDstFolder, srcs) in dict.items():
        # delete existing dst folder and create an empty one
        absDstFolder = os.path.join(rootDstFolder, relDstFolder)
        shutil.rmtree(absDstFolder, True)
        os.mkdir(absDstFolder)

        # copy files/folders into the dst folder
        for src in srcs:
            if os.path.isdir(str(src)):
                CopyTree(str(src), os.path.join(absDstFolder, src.name))
            else:
                CopyFile(str(src), os.path.join(absDstFolder, src.name))
        

def postBuildStep(target, source, env):
    
    targetDirNode = target[0]
    targetAppName = os.path.splitext(os.path.split(str(targetDirNode))[1])[0]
    sourceFileNode = source[0]
    sourceFilename = os.path.split(sourceFileNode.name)[1]
    iconFilename = env.subst(env.get('ICON', ''))

    contentsFolder = os.path.join(str(targetDirNode), 'Contents')    
    resFolder = os.path.join(contentsFolder, 'Resources')
    exeFolder = os.path.join(contentsFolder, 'MacOs')
    monoFolder = os.path.join(contentsFolder, 'MonoBundle')

    # create the Info.plist file
    iconFile = os.path.split(iconFilename)[1]
    version = env.get('VERSION', '')
    buildVersion = env.subst('$svn_rev')

    f = open(os.path.join(contentsFolder, "Info.plist"), 'w')
    f.write(INFO_PLIST_TEMPLATE % (targetAppName, targetAppName, buildVersion, version, targetAppName, sourceFilename, iconFile))
    f.close()

    # copy resource files
    resources = env['RESOURCES']
    CopyDictionary(resources, resFolder)

    # copy exe resource files
    exeResources = env['EXERESOURCES']
    CopyDictionary(exeResources, monoFolder)

    # copy the icon file
    CopyFile(str(iconFilename), os.path.join(resFolder, os.path.basename(iconFilename)))

    # copy native libs alongside the executable
    nativeLibs = env.get('NATIVELIBS', [])
    for nativeLib in nativeLibs:
        CopyFile(str(nativeLib), os.path.join(exeFolder, nativeLib.name))
    

mmpcom = "$MMP --nolink -o ${TARGET.dir} -n ${TARGET.filebase} $_CLILIBS $_NATIVELIBS $_NIBS $SOURCE"
codesigncom = '$CODESIGN $CODESIGNFLAGS -s $APPCERT $TARGET'

MmpBuilder = SCons.Builder.Builder(action = ['$MMPCOM', postBuildStep, '$CODESIGNCOM'],
                                   source_factory = SCons.Node.FS.default_fs.File,
                                   target_factory = SCons.Node.FS.default_fs.Dir,
                                   prefix = '$MMPPREFIX',
                                   suffix = '$MMPSUFFIX')

def _parseargs(prefix, libs):
    result = ''
    for l in libs:
        result += prefix + str(l) + ' '
    return result

def _parsearg(prefix, arg):
    return prefix + arg
        
    
def generate(env):
    env['BUILDERS']['Mmp'] = MmpBuilder

    env['MMP']             = env.Detect('mmp')
    env['_CLILIBS']        = "${_parseargs('-a ', CLILIBS)}"
    env['_NATIVELIBS']     = "${_parseargs('-r ', NATIVELIBS)}"
    env['_NIBS']           = "${_parseargs('-r ', NIBS)}"
    env['MMPCOM']          = SCons.Action.Action(mmpcom)
    env['MMPPREFIX']       = ''
    env['MMPSUFFIX']       = '.app'
    env['_parseargs']      = _parseargs
    env['_parsearg']       = _parsearg

    env['CODESIGN']        = env.Detect('codesign')
    env['CODESIGNFLAGS']   = SCons.Util.CLVar('-v -f')
    env['CODESIGNCOM']     = SCons.Action.Action(codesigncom)
    
    env.PrependENVPath('PATH', '/Library/Frameworks/Mono.framework/Versions/Current/bin')


def exists(env):
    return env.Detect('mmp')
