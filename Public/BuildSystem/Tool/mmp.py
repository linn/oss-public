import os
import os.path
import shutil
import plistlib
import subprocess
import xml.etree.ElementTree
import SCons.Builder
import SCons.Node.FS
import SCons.Util


def CopyFile(src, dst):
    print 'CopyFile(%s -> %s)' % (src, dst)
    shutil.copyfile(src, dst)


def CopyTree(src, dst):
    print 'CopyTree(%s -> %s)' % (src, dst)
    shutil.copytree(src, dst, ignore=shutil.ignore_patterns('.svn'))


def CopyDictionary(dict, rootDstFolder):
    for (relDstFolder, srcs) in dict.items():
        # create the folder for the items
        absDstFolder = os.path.join(rootDstFolder, relDstFolder)
        try:
            os.mkdir(absDstFolder)
        except OSError:
            pass

        # copy files/folders into the dst folder
        for src in srcs:
            if os.path.isdir(str(src)):
                CopyTree(str(src), os.path.join(absDstFolder, src.name))
            else:
                CopyFile(str(src), os.path.join(absDstFolder, src.name))



def GetDylibDependencies(aDylib):
    # run otool to get the list of dylibs that this dylib depends on
    process = subprocess.Popen(['otool', '-L', aDylib], stdout=subprocess.PIPE)
    output, err = process.communicate()
    retcode = process.poll()
    if retcode:
        raise CalledProcessError(retcode, 'otool', output=output)
    lines = output.splitlines()

    depends = []

    # skip the first 2 lines of output and extract dependencies that are
    # part of mono
    for i in range(2, len(lines)):
        l = lines[i].strip()
        if l.startswith('/Library/Frameworks/Mono.framework'):
            depends.append(l.split(' ')[0])

    return depends


def GetDylibDependenciesRec(aDylib):
    # get the direct dependencies for this dylib
    depends = GetDylibDependencies(aDylib)

    # recursively get the dependencies for each of the direct dependencies
    depends2 = []
    for d in depends:
        depends2.extend(GetDylibDependenciesRec(d))

    # add these secondary dependecies to the list
    for d in depends2:
        if d not in depends:
            depends.append(d)

    return depends


def AddGdiPlus(monoBundleDir):
    # get the list of all dylibs required for gdi plus functionality
    srcGdiPlus = '/Library/Frameworks/Mono.framework/Versions/Current/lib/libgdiplus.dylib'
    srcDylibs = [ srcGdiPlus ]
    srcDylibs.extend(GetDylibDependenciesRec(srcGdiPlus))

    # copy all dylibs across to the target folder and create list of copied dylibs
    dstDylibs = []
    for dylib in srcDylibs:
        dst = os.path.join(monoBundleDir, os.path.split(dylib)[1])
        CopyFile(dylib, dst)
        dstDylibs.append(dst)

    # now relocate all dylibs
    for dylib in dstDylibs:
        # change the id of the dylib
        inst_name = '@executable_path/../MonoBundle/' + os.path.split(dylib)[1]
        subprocess.call(['install_name_tool', '-id', inst_name, dylib])

        # relocate all dependencies
        depends = GetDylibDependencies(dylib)
        for d in depends:
            inst_name = '@executable_path/../MonoBundle/' + os.path.split(d)[1]
            subprocess.call(['install_name_tool', '-change', d, inst_name, dylib])

    # load the mono config file
    f = open(os.path.join(monoBundleDir, 'config'), 'r')
    cfg = f.read()
    f.close()

    # add xml elements for the gdiplus libs
    root = xml.etree.ElementTree.fromstring(cfg)
    elem1 = xml.etree.ElementTree.Element('dllmap', {'dll' : 'gdiplus.dll', 'target' : 'libgdiplus.dylib'})
    elem2 = xml.etree.ElementTree.Element('dllmap', {'dll' : 'gdiplus', 'target' : 'libgdiplus.dylib'})
    root.append(elem1)
    root.append(elem2)
    cfg = xml.etree.ElementTree.tostring(root)

    # save out the config file
    f = open(os.path.join(monoBundleDir, 'config'), 'w')
    f.write(cfg)
    f.close()


def preBuildStep(target, source, env):
    # remove the existing .app folder
    shutil.rmtree(str(target[0]), True)


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

    infoPlist = {}
    infoPlist['CFBundleIdentifier'] = 'uk.co.linn.' + targetAppName
    infoPlist['CFBundleName'] = targetAppName
    infoPlist['CFBundleVersion'] = buildVersion
    infoPlist['CFBundleShortVersionString'] = version
    infoPlist['CFBundleExecutable'] = targetAppName
    infoPlist['MonoBundleExecutable'] = sourceFilename
    infoPlist['CFBundleIconFile'] = iconFile
    infoPlist['LSMinimumSystemVersion'] = '10.6.8'
    infoPlist['CFBundleDevelopmentRegion'] = 'English'
    infoPlist['CFBundleInfoDictionaryVersion'] = '6.0'
    infoPlist['CFBundlePackageType'] = 'APPL'
    infoPlist['CFBundleSignature'] = '????'
    infoPlist['NSHumanReadableCopyright'] = 'Copyright 2011 Linn'
    infoPlist['LSApplicationCategoryType'] = 'public.app-category.music'

    infoPlist.update(env.get('INFOPLIST', {}))

    plistlib.writePlist(infoPlist, os.path.join(contentsFolder, "Info.plist"))

    # copy resource files
    resources = env.get('RESOURCES', [])
    if resources != []:
        CopyDictionary(resources, resFolder)

    # copy exe resource files
    exeResources = env.get('EXERESOURCES', [])
    if exeResources != []:
        CopyDictionary(exeResources, monoFolder)

    # copy the other resources
    otherResources = env.get('OTHERRESOURCES', [])
    if otherResources != []:
        CopyDictionary(otherResources, contentsFolder)
    
    # copy gdiplus.dylib if MonoBundle directory contains Syste.Drawing assembly
    if os.path.exists(os.path.join(monoFolder, 'System.Drawing.dll')):
    	print 'System.Drawing exists'
        AddGdiPlus(monoFolder)

    # copy the icon file
    CopyFile(str(iconFilename), os.path.join(resFolder, os.path.basename(iconFilename)))

    # copy native libs alongside the executable
    nativeLibs = env.get('NATIVELIBS', [])
    for nativeLib in nativeLibs:
        CopyFile(str(nativeLib), os.path.join(exeFolder, nativeLib.name))
    

mmpcom = "$MMP --nolink -o ${TARGET.dir} -n ${TARGET.filebase} $_CLILIBS $_NATIVELIBS $_NIBS $SOURCE"
codesigncom = '$CODESIGN $CODESIGNFLAGS -s $APPCERT $TARGET'

MmpBuilder = SCons.Builder.Builder(action = [preBuildStep, '$MMPCOM', postBuildStep, '$CODESIGNCOM'],
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

def _parselibs(prefix, libs, libpaths, env):
    result = ''

    for l in libs:
        # define 'found' as the default filename to add to the libs if it is not found in libpaths
        found = l + '.dll '
        # search through each libpath to see if the file exists
        for p in libpaths:
            f = os.path.join(env.subst(p), l) + '.dll'
            if os.path.exists(f):
                found = f
                break
        result += prefix + found + ' '

    return result
        
    
def generate(env):
    env['BUILDERS']['Mmp'] = MmpBuilder

    env['MMP']             = env.Detect('mmp')
    env['_CLILIBS']        = "${_parselibs('-a ', CLILIBS, CLILIBPATH, __env__)}"
    env['_NATIVELIBS']     = "${_parseargs('-r ', NATIVELIBS)}"
    env['_NIBS']           = "${_parseargs('-r ', NIBS)}"
    env['MMPCOM']          = SCons.Action.Action(mmpcom)
    env['MMPPREFIX']       = ''
    env['MMPSUFFIX']       = '.app'
    env['_parseargs']      = _parseargs
    env['_parsearg']       = _parsearg
    env['_parselibs']      = _parselibs

    env['CODESIGN']        = env.Detect('codesign')
    env['CODESIGNFLAGS']   = SCons.Util.CLVar('-v -f')
    env['CODESIGNCOM']     = SCons.Action.Action(codesigncom)
    
    env.PrependENVPath('PATH', '/Library/Frameworks/Mono.framework/Versions/Current/bin')


def exists(env):
    return env.Detect('mmp')
