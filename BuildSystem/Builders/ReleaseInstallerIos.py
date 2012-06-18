import os
import SCons.Node
Import('env')

MANIFEST_TEMPLATE = """<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
   <!-- array of downloads. -->
   <key>items</key>
   <array>
       <dict>
           <!-- an array of assets to download -->
           <key>assets</key>
           <array>
               <!-- software-package: the ipa to install. -->
               <dict>
                   <!-- required.  the asset kind. -->
                   <key>kind</key>
                   <string>software-package</string>
                   <!-- required.  the URL of the file to download. -->
                   <key>url</key>
                   <string>http://oss.linn.co.uk/%s/%s/Davaar/%s</string>
               </dict>
               <!-- display-image: the icon to display during download .-->
               <dict>
                   <key>kind</key>
                   <string>display-image</string>
                   <!-- optional.  indicates if icon needs shine effect applied. -->
                   <key>needs-shine</key>
                   <true/>
                   <key>url</key>
                   <string>http://oss.linn.co.uk/%s/Davaar/IconSmall.png</string>
               </dict>
               <!-- full-size-image: the large 512x512 icon used by iTunes. -->
               <dict>
                   <key>kind</key>
                   <string>full-size-image</string>
                   <key>needs-shine</key>
                   <true/>
                   <key>url</key>
                   <string>http://oss.linn.co.uk/%s/Davaar/IconLarge.png</string>
               </dict>
           </array><key>metadata</key>
           <dict>
               <!-- required -->
               <key>bundle-identifier</key>
               <string>uk.co.linn.%s</string>
               <!-- optional (software only) -->
               <key>bundle-version</key>
               <string>%s</string>
               <!-- required.  the download kind. -->
               <key>kind</key>
               <string>software</string>
               <!-- optional. displayed during download; typically company name -->
               <key>subtitle</key>
               <string>Linn Products Ltd</string>
               <!-- required.  the title to display during the download. -->
               <key>title</key>
               <string>%s</string>
           </dict>
       </dict>
   </array>
</dict>
</plist>
"""

BUNDLE_TEMPLATE = """<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDevelopmentRegion</key>
    <string>English</string>
    <key>CFBundleDisplayName</key>
    <string>%s</string>
    <key>CFBundleExecutable</key>
    <string>%s</string>
    <key>CFBundleIconFile</key>
    <string>KinskyLogoIphone.png</string>
    <key>CFBundleIconFiles</key>
    <array>
        <string>KinskyLogoIphone.png</string>
        <string>KinskyLogo.png</string>
        <string>KinskyLogoSpotlightIphone.png</string>
        <string>KinskyLogoSpotlight.png</string>
    </array>
    <key>CFBundleIdentifier</key>
    <string>uk.co.linn.%s</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundleName</key>
    <string>%s</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleResourceSpecification</key>
    <string>ResourceRules.plist</string>
    <key>CFBundleSignature</key>
    <string>????</string>
    <key>CFBundleSupportedPlatforms</key>
    <array>
        <string>iPhoneOS</string>
    </array>
    <key>CFBundleVersion</key>
    <string>%s</string>
    <key>DTCompiler</key>
    <string>4.2</string>
    <key>DTPlatformBuild</key>
    <string>8C134</string>
    <key>DTPlatformName</key>
    <string>iphoneos</string>
    <key>DTPlatformVersion</key>
    <string>4.2 Seed 2</string>
    <key>DTSDKName</key>
    <string>iphoneos4.2</string>
    <key>DTXcode</key>
    <string>0325</string>
    <key>DTXcodeBuild</key>
    <string>10M2423</string>
    <key>LSRequiresIPhoneOS</key>
    <true/>
    <key>MinimumOSVersion</key>
    <string>3.2</string>
    <key>NSMainNibFile</key>
    <string>MainWindowIphone</string>
    <key>NSMainNibFile~ipad</key>
    <string>MainWindowIpad</string>
    <key>UIPrerenderedIcon</key>
    <true/>
    <key>UIDeviceFamily</key>
    <array>
        <integer>1</integer>
        <integer>2</integer>
    </array>
    <key>CFBundleURLTypes</key>
    <array>
        <dict>
            <key>CFBundleURLName</key>
            <string>uk.co.linn.%s</string>
            <key>CFBundleURLSchemes</key>
            <array>
                <string>%s</string>
            </array>
        </dict>
    </array>
    <key>UIFileSharingEnabled</key>
    <string>YES</string>
    <key>UIRequiresPersistentWiFi</key>
    <true/>
    <key>UISupportedInterfaceOrientations</key>
    <array>
        <string>UIInterfaceOrientationPortrait</string>
        <string>UIInterfaceOrientationPortraitUpsideDown</string>
        <string>UIInterfaceOrientationLandscapeLeft</string>
        <string>UIInterfaceOrientationLandscapeRight</string>
    </array>
    <key>UISupportedInterfaceOrientations~</key>
    <array>
        <string>UIInterfaceOrientationPortrait</string>
        <string>UIInterfaceOrientationPortraitUpsideDown</string>
        <string>UIInterfaceOrientationLandscapeLeft</string>
        <string>UIInterfaceOrientationLandscapeRight</string>
    </array>
</dict>
</plist>
"""

RESOURCERULES_TEMPLATE = """<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>rules</key>
    <dict>
        <key>.*</key>
        <true/>
        <key>Info.plist</key>
        <dict>
            <key>omit</key>
            <true/>
            <key>weight</key>
            <real>10</real>
        </dict>
        <key>ResourceRules.plist</key>
        <dict>
            <key>omit</key>
            <true/>
            <key>weight</key>
            <real>100</real>
        </dict>
    </dict>
</dict>
</plist>"""

ENTITLEMENTS_TEMPLATE = """<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>application-identifier</key>
    <string>%s.uk.co.linn.%s</string>
    <key>keychain-access-groups</key>
    <array>
        <string>%s.uk.co.linn.%s</string>
    </array>
    <key>get-task-allow</key>
    <%s/>
</dict>
</plist>
"""

def FindProvisioningProfile(profile):
    dir = os.path.expanduser("~/Library/MobileDevice/Provisioning Profiles/")
    files = os.listdir(dir)
    for file in files:
        filename = os.path.join(dir, file)
        f = open(filename, 'r')
        d = f.read()
        f.close()
        
        if d.find("<string>%s</string>" % (profile)) > -1:
            return filename
    return None

def FindApplicationIdentifierPrefix(filename):
    f = open(filename, 'r')
    d = f.read()
    f.close()
    i1 = d.find("<string>")
    i2 = d.find("</string>")
    if i1 > -1 and i2 > -1:
        return d[i1+8:i2]
    return None

def ReleaseInstallerIos(target, source, **kw):
    try:
        username = os.environ['LOGNAME']
    except:
        username = os.environ['USERNAME']
        
    version = kw.get('VERSION', "").replace('development', '0.%s.0' % env.subst('$svn_rev'))
    product = kw.get('PRODUCT', 'Unknown') #os.path.splitext(target)[0]
    bundleId = kw.get('BUNDLEID', 'Unknown')
    clilibs = kw.get('CLILIBS', [])
    resources = kw.get('RESOURCES', [])
    identity = kw.get('IDENTITY', "")
    dist = os.path.basename(str(Dir(env.subst(target))))
    profile_src = FindProvisioningProfile(kw.get('PROFILE', ""))
    id = FindApplicationIdentifierPrefix(profile_src)
    
    payload = os.path.join(dist, os.path.join('Itunes/Payload', product))
    if os.path.splitext(payload)[1] == '':
        payload = payload + '.app/'
        
    def make_bundleinfo(target, source, env):
        f = open(str(target[0]), 'wt')
        f.write(BUNDLE_TEMPLATE % (product, product, bundleId, product, version, bundleId, product))
        f.close()
        
    info = env.Command(os.path.join(dist, 'Info.plist'), source, make_bundleinfo)
    
    def make_resourcerules(target, source, env):
        f = open(str(target[0]), 'wt')
        f.write(RESOURCERULES_TEMPLATE)
        f.close()
        
    rules = env.Command(os.path.join(dist, 'ResourceRules.plist'), source, make_resourcerules)
    
    def make_entitlements(target, source, env):
        f = open(str(target[0]), 'wt')
        f.write(ENTITLEMENTS_TEMPLATE %(id, bundleId, id, bundleId, str(env['variant'] == 'debug').lower()))
        f.close()
            
    profile = env.Command(os.path.join(dist, 'embedded.mobileprovision'), profile_src, Copy('$TARGET', '$SOURCE'))
    entitlements = env.Command(os.path.join(dist, product + '.xcent'), source, make_entitlements)
    
    def make_manifest(target, source, env):
        f = open(str(target[0]), 'wt')
        build = 'Releases'
        if kw.get('VERSION', "") == 'development':
            build = 'NightlyBuilds'
            file = os.path.basename(str(source[0]))
        else:
            file = product + '_' + version + '_all.ipa'
        f.write(MANIFEST_TEMPLATE %(build, product, file, product, product, bundleId, version, product))
        f.close()
    
    kw['RESOURCES'] = [info, rules, profile, resources]
    kw['ENTITLEMENTS'] = entitlements
    temp = env.Mtouch(os.path.join(dist, product), source, **kw)
    env.Depends(temp, info + rules + profile + entitlements)
    
    bin  = env.Command(Dir(env.subst('$install_dir/bin/') + product + dist + '.app'), Dir(temp), [Delete('$TARGET'), Copy('$TARGET', '$SOURCE')])
    bin += env.Command(Dir(payload), Dir(temp), [Delete('$TARGET'), Copy('$TARGET', '$SOURCE')])
    
    itunes_res = []
    for file in kw.get('ITUNESRESOURCES', []):
        itunes_res += env.Install(os.path.join(dist, 'Itunes'), file)

    install = os.path.join(dist, 'Installer' + product + '.ipa')
    zip = env.Command(install, [Dir(payload).dir] + itunes_res, [Delete('$TARGET'), 'cd ${SOURCE.dir}; $ZIP -y -r $TARGET ${SOURCES.file}'])
    env.Depends(zip, bin + itunes_res)
    
    install = os.path.join(dist, 'Installer' + product + '.plist')
    manifest = env.Command(install, zip, make_manifest)
    
    install = os.path.join(dist, 'Installer' + product + '.app.dSYM.zip')
    zip1 = env.Command(install, os.path.join(dist, product + '.app.dSYM'), [Delete('$TARGET'), 'cd ${SOURCE.dir}; $ZIP -r $TARGET ${SOURCES.file}'])
    
    installer = env.Install(target, zip + zip1)
    installer += env.Install(target, manifest)
    
    return installer
    
env.ReleaseInstallerIos = ReleaseInstallerIos

Return('env')
