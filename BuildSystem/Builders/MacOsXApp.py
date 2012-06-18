
import os.path
Import('env')


# Contains builders for building Mac OS X .app bundles with and without mono included in the
# bundle. Both buidlers take the following keyword defined variables
#
# APPCERT      - Name of the certificate with which to sign the app.
# CLILIBS      - A list of non-standard C# assemblies that the app uses.
# NATIVELIBS   - A list of non-standard native dylibs that the app uses.
# NIBS         - A list of the nib files used by the app.
# RESOURCES    - Dictionary of resource files to add to the Resources folder in
#                the app. In the dictionary, keys are subfolders of the Resources
#                folder and values are lists of files to go in those subfolders.
# EXERESOURCES - Dictionary of resource required to be alongside the compiled code
#                rather than in the resources folder. Dictionary takes the same
#                format as for RESOURCES.
# ICON         - Path the the icon file for the app.
# VERSION      - Version number for the app.


def MacOsXAppWithMono(target, source, **kw):
    return env.Mmp(target, source, **kw)


def MacOsXApp(target, source, **kw):
    # the name of the target is used to construct various files in the bundle
    targetName = target
    # the name of the exe is also required
    exeFullpath = str(source[0])
    exeFilename = os.path.split(exeFullpath)[1]
    exeName = os.path.splitext(exeFilename)[0]

    # create the bash script that is executed with the app
    def make_script(target, source, env):
        f = open(str(target[0]), 'w')
        f.write(SCRIPT_TEMPLATE % (targetName, targetName, targetName, exeName, targetName))
        f.close()
    script = env.Command(targetName + 'MacOsX.sh', source, make_script)

    # create the Info.plist for the bundle
    version = kw.get('VERSION', '')
    buildVersion = env.subst('$svn_rev')
    iconName = os.path.split(kw.get('ICON', ''))[1]

    def make_infoplist(target, source, env):
        f = open(str(target[0]), 'w')
        f.write(INFO_PLIST_TEMPLATE % (targetName, targetName, buildVersion, version, exeName, iconName))
        f.close()
    infoplist = env.Command(targetName + 'Info.plist', source, make_infoplist)

    # define folders in the app
    folderApp = target + '.app'
    folderContents = os.path.join(folderApp, 'Contents')
    folderMacOs = os.path.join(folderContents, 'MacOS')
    folderResources = os.path.join(folderContents, 'Resources')

    # copy over files into the .app hierarchy
    app = env.Command(folderApp, source, [Mkdir('$TARGET')])

    appContents = env.Command(os.path.join(folderMacOs, exeName), script, [Copy('$TARGET', '$SOURCE'), Chmod('$TARGET', 0755)])
    appContents += env.InstallAs(os.path.join(folderContents, 'Info.plist'), infoplist)
    appContents += env.Install(folderResources, source)
    appContents += env.Install(folderResources, kw.get('ICON', ''))

    for lib in kw.get('CLILIBS', []):
        appContents += env.Install(folderResources, lib)
    for lib in kw.get('NATIVELIBS', []):
        appContents += env.Install(folderResources, lib)
    for nib in kw.get('NIBS', []):
        appContents += env.Install(folderResources, nib)
    for (subfolder, files) in kw.get('RESOURCES', {}).items():
        for f in files:
            appContents += env.Install(os.path.join(folderResources, subfolder), f)
    for (subfolder, files) in kw.get('EXERESOURCES', {}).items():
        for f in files:
            appContents += env.Install(os.path.join(folderResources, subfolder), f)

    # make the link the mono installation
    def make_link(target, source, env):
        os.symlink(str(source[0]), str(target[0]))
    link1 = env.Command(os.path.join('/Users/Shared', targetName), '/usr/bin/mono', make_link)
    appContents += env.Command(os.path.join(folderApp, targetName), link1, make_link)

    # return the list of created nodes - the first is the node for the base .app folder
    return app


SCRIPT_TEMPLATE = """#!/bin/sh

# The purposes of this script are:
# - launching a Mono application stored within a bundle
# - naming correctly the launched application
# - be independant from the Mono installation folder


# Find the base folder of the bundle
BASE_PATH=`echo "$0" | awk -F"/" '{ for(i = 1; i <= NF - 3; i++) { printf("%%s/", $i); } }'`
cd "$BASE_PATH"
BASE_PATH=`pwd`

# Find the resource folder of the bundle
RESOURCES_PATH=$BASE_PATH/Contents/Resources

# Export some environment variable for Mono runtime
export MONO_GDIP_USE_COCOA_BACKEND=1
export DYLD_LIBRARY_PATH=$RESOURCES_PATH:$DYLD_LIBRARY_PATH

# The link inside the .app is used to execute the mono assembly
# This link must be created at install time and is set to target a link
# that is present in a writable location - /Users/Shared - this secondary
# link targets the current mono installation and can be changed from within
# this script. Remove that link here as it is recreated below
EXE_LINK="./%s"
MONO_LINK="/Users/Shared/%s"
if [ -f "$MONO_LINK" ]; then rm -f "$MONO_LINK" ; fi

# Check for mono installation
MONO_PATH=`which mono`
if [ "X$MONO_PATH" == "X" ]; then
    RESULT="`osascript -e 'tell app \"Finder\" to display dialog \"Application requires Mono.\" with icon stop with title \"Launch Failed\" default button 1 buttons {\"Get Mono...\", \"Cancel\"}'`"
    case "$RESULT" in
        *Mono*) osascript -e 'open location "http://mono-project.com/"';;
        *) ;;
    esac
else
    ln -s "$MONO_PATH" "$MONO_LINK"
    mkdir -p $HOME/Library/Logs/%s
    exec "$EXE_LINK" "$RESOURCES_PATH/%s.exe" 2> $HOME/Library/Logs/%s/Error.txt
fi
"""


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

    <key>CFBundleIconFile</key>
    <string>%s</string>

    <key>LSMinimumSystemVersion</key>
    <string>10.6.0</string>

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


env.MacOsXApp = MacOsXApp
env.MacOsXAppWithMono = MacOsXAppWithMono

Return('env')

