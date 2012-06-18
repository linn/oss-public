
import os.path
Import('env')


def MacOsXPkgBuild(target, source, **kw):
    # get the name of the source .app
    appFullpath = str(source[0])
    appFilename = os.path.split(appFullpath)[1]
    appName = os.path.splitext(appFilename)[0]

    # create the dst folder for packaging - use this custom command builder rather than a simple
    # Install. This is for 2 reasons:
    #  1. This builder preserves symlinks in the .app whereas Install resolves the symlinks to their
    #     target files
    #  2. The output target of this builder is the actual .dst folder, which is what we want - if
    #     Install is used the output target is the copied .app folder
    dst = env.Command(appName + '.dst', source[0], [Mkdir('$TARGET'), Copy(os.path.join('$TARGET', appFilename) , '$SOURCE')])

    # create the component plist
    def make_component_plist(target, source, env):
        f = open(str(target[0]), 'w')
        f.write(COMPONENT_PLIST_TEMPLATE % appFilename)
        f.close()
    plist = env.Command(appName + 'Component.plist', source[0], make_component_plist)

    # create the pkg
    pkg = env.PkgBuild(target, dst,
                       ID = 'uk.co.linn.' + appName,
                       INSTALL_PATH = '/Applications',
                       COMPONENT_PLIST = plist)

    env.Depends(pkg, [dst, plist])

    return pkg


COMPONENT_PLIST_TEMPLATE = """<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<array>
    <dict>
        <key>BundleHasStrictIdentifier</key>
        <true/>
        <key>BundleIsRelocatable</key>
        <true/>
        <key>BundleIsVersionChecked</key>
        <true/>
        <key>BundleOverwriteAction</key>
        <string>upgrade</string>
        <key>RootRelativeBundlePath</key>
        <string>%s</string>
    </dict>
</array>
</plist>
"""


env.MacOsXPkgBuild = MacOsXPkgBuild

Return('env')


