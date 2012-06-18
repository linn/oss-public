import os
import time
import SCons.Node
Import('env')

def ReleaseInstallerWin32(target, source, **kw):
    files = kw.get('FILES', {})
    dirs = kw.get('DIRS', {})
    # concatenate all sources from different lists (dict values)
    # ... while remembering into which directory they belong (dict keys)
    sourceAll = []
    bundleTargetDirs = {}
    for (dest, sourceForDest) in files.items() + dirs.items():
        sourceAll.extend(sourceForDest)
        for src in sourceForDest:
            # repr() includes object's unique memory address in its string output
            t = dest.replace('/', '\\')
            bundleTargetDirs[repr(src.disambiguate())] = t
    
    try:
        username = os.environ['LOGNAME']
    except:
        username = os.environ['USERNAME']
        
    signFiles = []
    for f in sourceAll:
        if f.suffix == '.exe' or f.suffix == '.cab':
            signFiles.append(f)

    launchfile = kw.get('LAUNCHFILE', {})
    if(launchfile != {}):
        launchfile.replace('\\', '/')
        
    version = kw.get('VERSION', "").replace('development', '0.%s.0' % env.subst('$svn_rev'))
        
    script = env.NsiScript(source, sourceAll,
                           OUTFILE = source + env.subst('$MAKENSISPROGSUFFIX'),
                           BUNDLE_TARGET_DIRS = bundleTargetDirs,
                           TEMPLATE   = File(kw.get('TEMPLATE', {})),
                           RESOURCES  = kw.get('RESOURCES', {}),
                           PRODUCT    = kw.get('PRODUCT', {}),
                           VERSION    = version,
                           LAUNCHFILE = launchfile)
    env.Depends(script, kw.get('TEMPLATE', {}))
    env.Depends(script, kw.get('RESOURCES', {}).values())
    env.Depends(script, env.Value(version))
    env.Depends(script, sourceAll)
    
    for d in bundleTargetDirs:
        env.Depends(script, Dir(d))
    
    kw['SIGNFILES'] = signFiles
    release = env.MakeNsis(source, script, **kw)
    env.Depends(release, sourceAll)
    
    for d in bundleTargetDirs:
        env.Depends(release, Dir(d))
    installed = env.Install(target, release)
    env.Alias(source, installed)
    
    return installed

env.ReleaseInstallerWin32 = ReleaseInstallerWin32

Return('env')
