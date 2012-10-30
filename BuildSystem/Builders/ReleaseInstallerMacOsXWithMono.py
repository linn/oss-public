
import os.path
Import('env')


def ReleaseInstallerMacOsXWithMono(target, source, **kw):

    # build the Mac OSX .app
    app = env.Mmp(kw['APPNAME'], source, **kw)

    # install the Mac OSX .app
    appInst = env.Install(env.subst(kw['APPINSTFOLDER']), app)

    # build the Mac OSX .pkg
    if 'INSTALLERRESOURCES' in kw and len(kw['INSTALLERRESOURCES']) > 0:
        kw['SOURCEOPTIONNAME'] = '--component'
        pkg = env.ProductBuildWithResources(target, app, **kw)
    else:
        pkg = env.ProductBuild(target, app, **kw)

    # install the Mac OSX .pkg
    pkgInst = env.Install(env.subst(kw['PKGINSTFOLDER']), pkg)
    
    return [pkgInst, appInst]


env.ReleaseInstallerMacOsXWithMono = ReleaseInstallerMacOsXWithMono

Return('env')
