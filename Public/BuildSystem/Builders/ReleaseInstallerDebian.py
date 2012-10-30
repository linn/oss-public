import os
import time
import sys
import SCons.Node
Import('env')

SCRIPT_TEMPLATE = """#!/bin/sh
exec /usr/bin/cli /usr/lib/%s "$@"
"""

MENU_TEMPLATE = """?package(%s):command="/usr/bin/%s" \\
    icon="/usr/share/pixmaps/%s" \\
    needs="X11" \\
    section="Applications/Sound" \\
    title="%s"
"""

DESKTOP_TEMPLATE = """[Desktop Entry]
Type=Application
Encoding=UTF-8
Name=%s
GenericName=%s
Comment=%s
Icon=%s
TryExec=%s
Exec=%s
Terminal=%s
Categories=%s
"""

def ReleaseInstallerDebian(target, source, **kw):
    if sys.platform != 'linux2':
        return []
    
    files = kw.get('FILES', {})
    dirs = kw.get('DIRS', {})
    
    product = kw.get('PRODUCT')
    
    launchfile = os.path.split(kw.get('LAUNCHFILE', ''))[1]
    
    # concatenate all sources from different lists (dict values)
    # ... while remembering into which directory they belong (dict keys)
    sourceAll = []
    bundleTargetDirs = {}
    for (dest, sourceForDest) in files.items() + dirs.items():
        sourceAll.extend(sourceForDest)
        for src in sourceForDest:
            # repr() includes object's unique memory address in its string output
            bundleTargetDirs[repr(src.disambiguate())] = dest.replace('/', '\\')
    
    try:
        username = os.environ['LOGNAME']
    except:
        username = os.environ['USERNAME']
        
    def make_script(target, source, env):
        f = open(str(target[0]), 'w')
        f.write(SCRIPT_TEMPLATE % os.path.join(product, source[0].name))
        f.close()
    
    script = env.Command(os.path.splitext(launchfile)[0] + "Linux.sh", launchfile, make_script)
    
    def make_desktop(target, source, env):
        f = open(str(target[0]), 'w')
        f.write(DESKTOP_TEMPLATE % (product, product, kw.get('DESCRIPTION', ''), os.path.splitext(os.path.split(env.subst(kw.get('ICON', '')))[1])[0], product, product, env['variant'] != 'release', kw.get('CATEGORIES', 'Audio;')))
        f.close()
        
    desktop = env.Command(product.lower() + ".desktop", launchfile, make_desktop)
    
    architecture = kw.get('ARCHITECTURE', 'all')
    packagepath = os.path.join(architecture, 'DebianPackage')
    version = kw.get('VERSION', {})
    if (kw.get('TYPE', {}) == 'development' or kw.get('TYPE', {}) == 'nightly') and env.subst('$svn_rev') != "0":
        version = '0.%s.0' % env.subst('$svn_rev')
        
    control = env.DpkgControl(os.path.join(packagepath, 'DEBIAN/control'), sourceAll + script,
                              MAINTAINER   = kw.get('MAINTAINER', {}),
                              ARCHITECTURE = architecture,
                              DEPENDENCIES = kw.get('DEPENDECIES', {}),
                              DESCRIPTION  = kw.get('DESCRIPTION', {}),
                              PRODUCT      = kw.get('PRODUCT', {}),
                              VERSION      = version)
    env.Depends(control, env.Value(version))

    release = env.DpkgDeb(source, control)
    installed = env.Install(target, release)

    f = env.Command(os.path.join(os.path.join(packagepath, 'usr/bin'), product), script, [Copy('$TARGET', '$SOURCE'), Chmod('$TARGET', 0755)])
    env.Depends(control, f)
    
    f = env.Command(os.path.join(os.path.join(packagepath, 'usr/share/applications'), desktop[0].name), desktop, Copy('$TARGET', '$SOURCE'))
    env.Depends(control, f)
    
    f = env.Command(os.path.join(packagepath, 'usr/share/doc/' + product + '/copyright'), kw.get('COPYRIGHT', '$variant_dir/share/Linn/Core/copyright'), Copy('$TARGET', '$SOURCE'))
    env.Depends(control, f)
    
    def make_menu(target, source, env):
        f = open(str(target[0]), 'w')
        f.write(MENU_TEMPLATE % (product.lower(), source[0].name, os.path.split(kw.get('ICON', ''))[1], product))
        f.close()
        
    menu = env.Command(os.path.join(os.path.join(packagepath, 'usr/share/menu'), product.lower()), f, make_menu)
    env.Depends(control, menu)
    
    for (dest, sourceForDest) in files.items() + dirs.items():
        for src in sourceForDest:
            # Copy from the the source tree.
            f = env.Command(os.path.join(os.path.join(packagepath, dest), src.name), src, Copy('$TARGET', '$SOURCE'))
            # The .deb package will depend on this file
            env.Depends(release, f)
            # The control file also depends on each source because we'd like
            # to know the total installed size of the package
            env.Depends(control, f)
        f = env.Install(os.path.join(packagepath, 'usr/share/pixmaps'), kw.get('ICON', ''))
        env.Depends(control, f)
    
    env.Alias(source, installed)
    return installed

env.ReleaseInstallerDebian = ReleaseInstallerDebian

Return('env')
