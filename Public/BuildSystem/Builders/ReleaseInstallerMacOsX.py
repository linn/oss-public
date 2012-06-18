import os
import time
import sys
import SCons.Node
Import('env')

BUNDLE_TEMPLATE = """<?xml version=\"1.0\" encoding=\"UTF-8\"?>
<!DOCTYPE plist PUBLIC \"-//Apple Computer//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">
<plist version=\"1.0\">
<dict>
    <key>CFBundleGetInfoString</key>
    <string>%s</string>
    <key>CFBundleIdentifier</key>
    <string>uk.co.linn.%s</string>
    <key>CFBundleName</key>
    <string>%s</string>
    <key>CFBundleExecutable</key>
    <string>%s</string>
    <key>CFBundleIconFile</key>
    <string>%s</string>
    <key>CFBundleVersion</key>
    <string>%s</string>
    <key>NSHumanReadableCopyright</key>
    <string>Copyright 2010 Linn</string>
</dict>
</plist>
"""

SCRIPT_TEMPLATE = """#!/bin/sh

# The purposes of this script are:
# - launching a Mono application stored within a bundle
# - naming correctly the launched application
# - be independant from the Mono installation folder
# --------------------------------------------------------------------------------

# Normalize the current path and command, to find the MacOS folder of the bundle
# --------------------------------------------------------------------------------
PWD=`pwd`
MACOS_PATH=`echo "$0" | awk -F"/" '{ for(i = 1; i <= NF - 1; i++) { printf("%s/", $i); } }'`
cd "$MACOS_PATH"
MACOS_PATH=`pwd`

# Find the base folder of the bundle
# --------------------------------------------------------------------------------
BASE_PATH=`echo "$MACOS_PATH" | awk -F"/" '{ for(i = 1; i <= NF - 2; i++) { printf("%s/", $i); } }'`

# Find the resource folder of the bundle
# --------------------------------------------------------------------------------
RESOURCES_PATH=`echo "$MACOS_PATH" | awk -F"/" '{ for(i = 1; i <= NF - 1; i++) { printf("%s/", $i); } printf("Resources"); }'`

# Export some environment variable for Mono runtime
# --------------------------------------------------------------------------------
export MONO_GDIP_USE_COCOA_BACKEND=1
export DYLD_LIBRARY_PATH=$MACOS_PATH:$RESOURCES_PATH:$DYLD_LIBRARY_PATH

# Extract the application and the main assembly name
# --------------------------------------------------------------------------------
APP_NAME=`echo $0 | awk -F"/" '{ printf("%s", $NF); }'`
ASSEMBLY=`echo $0 | awk -F"/" '{ printf("%s.exe", $NF); }'`

# Go to the resources folder
# Remove a symbolic link named from the application if it exists 
# Create a symbolic link named from the application to the mono command
# Launch the application by using the symbolic link.
# --------------------------------------------------------------------------------
cd "$BASE_PATH"
EXEC_PATH="./$APP_NAME"
if [ -f "$EXEC_PATH" ]; then rm -f "$EXEC_PATH" ; fi
MONO_PATH=`which mono`
if [ "X$MONO_PATH" == "X" ]; then
    RESULT="`osascript -e 'tell app \"Finder\" to display dialog \"Application requires Mono.\" with icon stop with title \"Launch Failed\" default button 1 buttons {\"Get Mono...\", \"Cancel\"}'`"
    case "$RESULT" in
        *Mono*) osascript -e 'open location "http://mono-project.com/"';;
        *) ;;
    esac
else
    ln -s "$MONO_PATH" "$EXEC_PATH"
    exec "$EXEC_PATH" "$RESOURCES_PATH/$ASSEMBLY" 2> Error.txt
fi
"""

def ReleaseInstallerMacOsX(target, source, **kw):  
    files = kw.get('FILES', {})
    dirs = kw.get('DIRS', {})
    
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
        
    version = kw.get('VERSION', "").replace('development', '0.%s.0' % env.subst('$svn_rev'))
    
    def make_script(target, source, env):
        f = open(str(target[0]), 'w')
        f.write(SCRIPT_TEMPLATE)
        f.close()
        
    script = env.Command(os.path.splitext(launchfile)[0] + "MacOsX.sh", launchfile, make_script)
    
    description = kw.get('DESCRIPTION', {})
    product = kw.get('PRODUCT', {})
    iconFile = os.path.split(kw.get('ICON', ''))[1]
    
    def make_bundle(target, source, env):
        f = open(str(target[0]), 'w')
        f.write(BUNDLE_TEMPLATE % (description, product, product, os.path.splitext(launchfile)[0], iconFile, version))
        f.close()
    
    bundle = env.Command("Info.plist", launchfile, make_bundle)
    
    baseInstall = os.path.join(kw.get('PRODUCT', 'Unknown') + '.app')
    installContents = os.path.join(baseInstall, 'Contents')
    installMacOs = os.path.join(installContents, 'MacOS')
    installResources = os.path.join(installContents, 'Resources')

    f  = env.Command(os.path.join(installMacOs, os.path.splitext(launchfile)[0]), script, [Copy('$TARGET', '$SOURCE'), Chmod('$TARGET', 0755)])
    f += env.Command(os.path.join(installContents, "Info.plist"), bundle, [Copy('$TARGET', '$SOURCE'), Chmod('$TARGET', 0755)])
    
    for (dest, sourceForDest) in files.items() + dirs.items():
        for src in sourceForDest:
            # Copy from the the build tree.
            f += env.Command(os.path.join(installResources, os.path.join(dest, src.name)), src, Copy('$TARGET', '$SOURCE'))
        f += env.Install(installResources, kw.get('ICON', ''))
    
    bin = Install(env.subst('$install_dir/bin'), os.path.join(kw.get('PRODUCT', 'Unknown') + '.app'))    
    tar = Command(source + '.app.tgz', '', "tar -C " + os.path.split(script[0].abspath)[0] + " -c -z -f $TARGET " + os.path.join(kw.get('PRODUCT', 'Unknown') + '.app'))
    env.Depends(tar, f)
    env.Depends(bin, f)
    installed = env.Install(target, tar)

    return [installed, bin]

env.ReleaseInstallerMacOsX = ReleaseInstallerMacOsX

Return('env')
