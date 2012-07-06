#!/usr/bin/python -tt

import os
import optparse
import sys
import subprocess
import shutil

### editable settings for each release #####################################################
# FIXME -- need to factor this out into separate XML config file

kWizardVersion =                    '4.1.2'
kBundleWizard =                     'B07260102'                 # for Wizard@4.1.2

kSwPartWizardWindows =              'S08600102.exe'             # for Wizard@4.1.2
kSwPartWizardDebian =               '_S'                        # for Wizard@4.1.2
kSwPartWizardMac =                  'S08610102.pkg'             # for Wizard@4.1.2

kSongboxVersion =                   '4.1.13'
kBundleSongbox =                    'B07240113'                 # for Songbox@4.1.13

kSwPartSongboxWindows =             'S08550113.exe'             # for Songbox@4.1.13
kSwPartSongboxDebian =              '_S08560101.deb'            # for Songbox@4.1.13
kSwPartSongboxMac =                 'S08570113.pkg'             # for Songbox@4.1.13

kSongcastVersion =                  '4.2.6'
kBundleSongcast =                   'B07220115'                 # for Songcast@4.2.6

kSwPartSongcastWindows =            'S08520114.exe'             # for Songcast@4.2.6
kSwPartSongcastDebian =             '_S'                        # for Songcast@4.2.6
kSwPartSongcastMac =                'S08530114.dmg'             # for Songcast@4.2.6

kKinskyVersion =                    '4.3.3'
kBundleKinsky =                     'B07190110'                 # for Kinsky@4.3.3

kSwPartKinskyAppStore =             'S08450114.ipa'             # for Kinsky@4.3.3 (iOS AppStore)
kSwPartKinskyAdHoc =                'S08470112.ipa'             # for Kinsky@4.3.3 (iOS AdHoc)
kSwPartKinskyManifest =             'S08460114.plist'           # for Kinsky@4.3.3 (Manifest)
kSwPartKinskyAppStoreDsym =         'S08480108.app.dSYM.zip'    # for Kinsky@4.3.3 (iOS AppStore dSYM)
kSwPartKinskyAdHocDsym =            'S08490108.app.dSYM.zip'    # for Kinsky@4.3.3 (iOS AdHoc dSYM)

kSwPartKinskyWindows =              'S08230144.exe'             # for Kinsky@4.3.3 (Windows)
kSwPartKinskyDebian =               '_S08270140.deb'            # for Kinsky@4.3.3 (Debian)
kSwPartKinskyMac =                  'S08280152.pkg'             # for Kinsky@4.3.3 (Mac)
kSwPartKinskyAndroid =              '_S08580101.apk'             # for Kinsky@4.2.8 (Android)

kSwPartKinskyActiveSync =           'S08210126.exe'             # for Kinsky@4.2.5 (ActiveSync)
kSwPartKinskyWindowsCe =            'S08220126.cab'             # for Kinsky@4.2.5 (WinCE)

kKinskyClassicVersion =             '3.3.6'

kBundleKinskyClassic =              'B07060115'                 # for KinskyClassic@3.3.6
kSwPartKinskyClassicWindows =       'S08000176.exe'             # for KinskyClassic@3.3.6 (Windows)
kSwPartKinskyClassicDebian =        'S08350107.deb'             # for KinskyClassic@3.3.6 (Debian)
kSwPartKinskyClassicMac =           'S08360107.app.tgz'         # for KinskyClassic@3.3.6 (Mac)

kKinskyJukeboxVersion =             '4.1.2'

kBundleKinskyJukebox =              'B07150109'                 # for KinskyJukebox@4.1.2
kSwPartKinskyJukeboxWindows =       'S08320110.exe'             # for KinskyJukebox@4.1.2 (Windows)
kSwPartKinskyJukeboxDebian =        'S08330110.deb'             # for KinskyJukebox@4.1.2 (Debian)
kSwPartKinskyJukeboxMac =           '_S08340110.app.tgz'         # for KinskyJukebox@4.1.2 (Mac)

kLinnSysTrayVersion =               '4.1.1'

kOssKinskyMppBbc =                  'OssKinskyMppBbc.kpz'
kOssKinskyMppItunes =               'OssKinskyMppItunes.kpz'
kOssKinskyMppMovieTrailers =        'OssKinskyMppMovieTrailers.kpz'
kOssKinskyMppRadio =                'OssKinskyMppRadio.kpz'
kOssKinskyMppShoutcast =            'OssKinskyMppShoutcast.kpz'
kOssKinskyMppWfmu =                 'OssKinskyMppWfmu.kpz'

kBundleKinskyPronto =               'B07130107'                 # for KinskyPronto@4.1.1

kOssKinskyProntoProj9600 =          'KinskyPronto3.1.3.xcf'
kOssKinskyProntoMod9600 =           'KinskyPronto3.1.3.xgf'
kOssKinskyProntoProj9400 =          'KinskyPronto3.1.3.xcf'
kOssKinskyProntoMod9400 =           'KinskyPronto3.1.3.xgf'
kSwPartKinskyProntoProj9600 =       'S08380100.xcf'             # for KinskyPronto@3.1.3
kSwPartKinskyProntoMod9600 =        'S08390100.xgf'             # for KinskyPronto@3.1.3
kSwPartKinskyProntoProj9400 =       'S08400100.xcf'             # for KinskyPronto@3.1.3
kSwPartKinskyProntoMod9400 =        'S08410100.xgf'             # for KinskyPronto@3.1.3
############################################################################################

kFamily = 'Davaar'
kPluginVersion = 'V8'

# official location for software parts:
#     \\eng.linn.co.uk\share\components\
kDirCmpnt = '/local/share/components'
kDirOss = '/local/share/oss/Releases'
kDirBundle = '/local/share/releases'
kSvnReleaseTemplates = 'svn://eng.linn.co.uk/ReleaseTemplates'


class ToolError(Exception):
    """Failure to execute external tool."""

    def __init__(self, aCmdLine):
        """Initialise exception with additional debug info."""
        self.iCmdLine = aCmdLine

    def __str__(self):
        """Return string representation of this object."""
        return 'failed to execute external tool:  \'%s\'' % (' '.join(self.iCmdLine))


def RetrieveReleaseTextFile(aProduct):
    """Retrieve release.txt from SVN repository."""
    cmdLineSvn = ['svn', 'export', '--quiet', '--force', 'svn://eng.linn.co.uk/ReleaseTemplates/%s/release.txt' % aProduct, 'release.txt']
    p = subprocess.Popen(cmdLineSvn)
    retVal = p.wait()
    if retVal: raise ToolError(cmdLineSvn)
    
def RetrieveBundleNsiTemplate():
    """Retrieve BundleNsiTemplate.txt from SVN repository."""
    cmdLineSvn = ['svn', 'export', '--quiet', '--force', 'svn://eng.linn.co.uk/ReleaseTemplates/BundleNsiTemplate.txt', 'BundleNsiTemplate.txt']
    p = subprocess.Popen(cmdLineSvn)
    retVal = p.wait()
    if retVal: raise ToolError(cmdLineSvn)
    
def BuildInstaller():
    """Build NullSoft installer from NSI script with well-known filename."""
    # cross-compile Windows installer (using 'nsis' Debian package)
    cmdLineNsi = ['makensis', '-V0', 'Bundle.nsi']
    p = subprocess.Popen(cmdLineNsi)
    retVal = p.wait()
    if retVal: raise ToolError(cmdLineNsi)     

def Publish(aSwPart, aDestDir, aSrcFile, aDryRun):
    """Publish given Software Part in well-known location unless conflict detected."""
    def Md5Hash(aFile):
        if sys.platform == "darwin":
            cmdLineMd5 = ['/sbin/md5', aFile]
        else:
            cmdLineMd5 = ['/usr/bin/md5sum', aFile]
        p = subprocess.Popen(args = cmdLineMd5, stdout = subprocess.PIPE)
        md5Hash = p.stdout.read().split()[0]  # disregard filename
        retVal = p.wait()
        if retVal: raise ToolError(cmdLineMd5)
        return md5Hash
    
    if not os.path.exists(aSrcFile):
        print '[SKIP]  %s: no such file' % aSrcFile
        return False 
        
    # place into \\eng.linn.co.uk\share\components\ for Samba access
    if(aSwPart.startswith('_')):
        swPartPubl = os.path.join(aDestDir, aSwPart[1:])
    else:
        swPartPubl = os.path.join(aDestDir, aSwPart)
    published = True  # optimistic
    if os.path.exists(swPartPubl):
        if Md5Hash(aSrcFile) == Md5Hash(swPartPubl):
            print '[OK]    identical file already exists: %s' % swPartPubl
        else:
            print '[FAIL]  DIFFERENT file already exists: %s' % swPartPubl
        published = False
    else:
        if not aDryRun:
            shutil.copy2(aSrcFile, swPartPubl)
        print '[OK]    file published: %s' % swPartPubl
    return published

def PublishBundle(aProduct, aBundle, aFiles, aDryRun):
    file = aBundle + '.exe'
    
    d = {
         'product'  : aProduct,
         'bundle'   : aBundle,
         'install'  : []
        }
    
    if(not aDryRun):
        d['install'].append("File release.txt")
        for f in aFiles:
            if(f[0] != '_'):
                full = os.path.join(kDirCmpnt, f)
                if not os.path.exists(full):
                    print '[SKIP]  %s: %s missing' % (file, full)
                    return False 
                d['install'].append("File " + full)
                
    d['install'] = '\n  '.join(d['install'])
    
    templateFile = open('BundleNsiTemplate.txt', 'rb')
    template = templateFile.read()
    templateFile.close()
    
    f = open('Bundle.nsi', 'wb')
    f.write(template % d)
    f.close()
    
    RetrieveReleaseTextFile(aProduct)
    
    BuildInstaller()
    
    os.remove('release.txt')
    os.remove('Bundle.nsi')
    
    Publish(file, kDirBundle, file, aDryRun)
    
    os.remove(file)
    
    return True
    
def XformWizard(aProduct, aBaseName, aDirOss, aDryRun):
    dir = os.path.join(aDirOss, kFamily)
        
    file = (aBaseName % '/InstallerWizard') + '.exe'
    Publish(kSwPartWizardWindows, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_win.exe' % (aProduct, kWizardVersion), dir, file, aDryRun)
    
    #file = (aBaseName % '/InstallerWizard') + '.deb'
    #Publish(kSwPartWizardDebian, kDirCmpnt, file, aDryRun)
    #Publish('%s_%s-1_all.deb' % (aProduct.lower(), kWizardVersion), dir, file, aDryRun)
    
    file = (aBaseName % '/InstallerWizard') + '.pkg'
    Publish(kSwPartWizardMac, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_osx.pkg' % (aProduct, kWizardVersion), dir, file, aDryRun)
        
    file = (aBaseName % '/UpdaterWizard_win') + '.dll'
    Publish('%s_%s_win.dll' % (aProduct, kWizardVersion), dir, file, aDryRun)
    
    file = (aBaseName % '/UpdaterWizard_osx') + '.dll'
    Publish('%s_%s_osx.dll' % (aProduct, kWizardVersion), dir, file, aDryRun)
    
    files = [kSwPartWizardWindows, kSwPartWizardDebian, kSwPartWizardMac]
    PublishBundle(aProduct, kBundleWizard, files, aDryRun)

def XformSongbox(aProduct, aBaseName, aDirOss, aDryRun):
    dir = os.path.join(aDirOss, kFamily)
        
    file = (aBaseName % '/InstallerSongbox') + '.exe'
    Publish(kSwPartSongboxWindows, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_win.exe' % (aProduct, kSongboxVersion), dir, file, aDryRun)
    
    #file = (aBaseName % '/InstallerSongbox') + '.deb'
    #Publish(kSwPartSongboxDebian, kDirCmpnt, file, aDryRun)
    #Publish('%s_%s-1_all.deb' % (aProduct.lower(), kSongboxVersion), dir, file, aDryRun)
    
    file = (aBaseName % '/InstallerSongbox') + '.pkg'
    Publish(kSwPartSongboxMac, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_osx.pkg' % (aProduct, kSongboxVersion), dir, file, aDryRun)
        
    file = (aBaseName % '/UpdaterSongbox_win') + '.dll'
    Publish('%s_%s_win.dll' % (aProduct, kSongboxVersion), dir, file, aDryRun)
    
    file = (aBaseName % '/UpdaterSongbox_osx') + '.dll'
    Publish('%s_%s_osx.dll' % (aProduct, kSongboxVersion), dir, file, aDryRun)
    
    files = [kSwPartSongboxWindows, kSwPartSongboxDebian, kSwPartSongboxMac]
    PublishBundle(aProduct, kBundleSongbox, files, aDryRun)

def XformSongcast(aProduct, aBaseName, aDirOss, aDryRun):
    dir = os.path.join(aDirOss, kFamily)
        
    file = (aBaseName % '/InstallerSongcast') + '.exe'
    Publish(kSwPartSongcastWindows, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_win.exe' % (aProduct, kSongcastVersion), dir, file, aDryRun)
    
    #file = (aBaseName % '/InstallerSongcast') + '.deb'
    #Publish(kSwPartSongcastDebian, kDirCmpnt, file, aDryRun)
    #Publish('%s_%s-1_all.deb' % (aProduct.lower(), kSongcastVersion), dir, file, aDryRun)
    
    file = (aBaseName % '/InstallerSongcast') + '.dmg'
    Publish(kSwPartSongcastMac, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_osx.dmg' % (aProduct, kSongcastVersion), dir, file, aDryRun)
        
    file = (aBaseName % '/UpdaterSongcast_win') + '.dll'
    Publish('%s_%s_win.dll' % (aProduct, kSongcastVersion), dir, file, aDryRun)
    
    file = (aBaseName % '/UpdaterSongcast_osx') + '.dll'
    Publish('%s_%s_osx.dll' % (aProduct, kSongcastVersion), dir, file, aDryRun)
    
    files = [kSwPartSongcastWindows, kSwPartSongcastDebian, kSwPartSongcastMac]
    PublishBundle(aProduct, kBundleSongcast, files, aDryRun)

def XformKinsky(aProduct, aBaseName, aDirOss, aDryRun):
    dir = os.path.join(aDirOss, kFamily)
    
    file = (aBaseName % '/AppStore/InstallerKinsky') + '.ipa'
    Publish(kSwPartKinskyAppStore, kDirCmpnt, file, aDryRun)
    
    file = (aBaseName % '/AppStore/InstallerKinsky') + '.app.dSYM.zip'
    Publish(kSwPartKinskyAppStoreDsym, kDirCmpnt, file, aDryRun)
    
    file = (aBaseName % '/AdHoc/InstallerKinsky') + '.ipa'
    Publish(kSwPartKinskyAdHoc, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_all.ipa' % ('Kinsky', kKinskyVersion), dir, file, aDryRun)
    
    file = (aBaseName % '/AdHoc/InstallerKinsky') + '.app.dSYM.zip'
    Publish(kSwPartKinskyAdHocDsym, kDirCmpnt, file, aDryRun)
    
    file = (aBaseName % '/AdHoc/InstallerKinsky') + '.plist'
    Publish(kSwPartKinskyManifest, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_all.plist' % ('Kinsky', kKinskyVersion), dir, file, aDryRun)
        
    file = (aBaseName % '/InstallerKinsky') + '.exe'
    Publish(kSwPartKinskyWindows, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_win.exe' % (aProduct, kKinskyVersion), dir, file, aDryRun)
    
    file = (aBaseName % '/InstallerKinsky') + '.deb'
    Publish(kSwPartKinskyDebian, kDirCmpnt, file, aDryRun)
    Publish('%s_%s-1_all.deb' % (aProduct.lower(), kKinskyVersion), dir, file, aDryRun)
    
    file = (aBaseName % '/InstallerKinsky') + '.pkg'
    Publish(kSwPartKinskyMac, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_osx.pkg' % (aProduct, kKinskyVersion), dir, file, aDryRun)
	    
    file = (aBaseName % '/Pda/InstallerKinsky') + '.exe'
    Publish(kSwPartKinskyActiveSync, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_wm6.exe' % (aProduct, kKinskyVersion), dir, file, aDryRun)
    
    file = (aBaseName % '/Pda/InstallerKinsky') + '.cab'
    Publish(kSwPartKinskyWindowsCe, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_wm6.cab' % (aProduct, kKinskyVersion), dir, file, aDryRun)
	
    file = (aBaseName % '/UpdaterKinsky_win') + '.dll'
    Publish('%s_%s_win.dll' % (aProduct, kKinskyVersion), dir, file, aDryRun)
	
    file = (aBaseName % '/UpdaterKinsky_osx') + '.dll'
    Publish('%s_%s_osx.dll' % (aProduct, kKinskyVersion), dir, file, aDryRun)
	
    file = (aBaseName % '/uk.co.linn.kinsky-Signed') + '.apk'
    Publish(kSwPartKinskyAndroid, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_and.apk' % (aProduct, kKinskyVersion), dir, file, aDryRun)
	
    files = [kSwPartKinskyAppStore, kSwPartKinskyAppStoreDsym, kSwPartKinskyAdHoc, kSwPartKinskyAdHocDsym, kSwPartKinskyManifest, kSwPartKinskyWindows, kSwPartKinskyDebian, kSwPartKinskyMac, kSwPartKinskyActiveSync, kSwPartKinskyWindowsCe, kSwPartKinskyAndroid]
    PublishBundle(aProduct, kBundleKinsky, files, aDryRun)

	
def XformKinskyClassic(aProduct, aBaseName, aDirOss, aDryRun):
    dir = os.path.join(aDirOss, kFamily)
    
    file = aBaseName + '.exe'
    Publish(kSwPartKinskyClassicWindows, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_all.exe' % (aProduct, kKinskyClassicVersion), dir, file, aDryRun)
    
    file = aBaseName + '.deb'
    Publish(kSwPartKinskyClassicDebian, kDirCmpnt, file, aDryRun)
    Publish('%s_%s-1_all.deb' % (aProduct.lower(), kKinskyClassicVersion), dir, file, aDryRun)
    
    file = aBaseName + '.app.tgz'
    Publish(kSwPartKinskyClassicMac, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_all.app.tgz' % (aProduct, kKinskyClassicVersion), dir, file, aDryRun)
    
    files = [kSwPartKinskyClassicWindows, kSwPartKinskyClassicDebian, kSwPartKinskyClassicMac]
    PublishBundle(aProduct, kBundleKinskyClassic, files, aDryRun)
    
def XformKinskyJukebox(aProduct, aBaseName, aDirOss, aDryRun):
    dir = os.path.join(aDirOss, kFamily)
    
    file = aBaseName + '.exe'
    Publish(kSwPartKinskyJukeboxWindows, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_all.exe' % (aProduct, kKinskyJukeboxVersion), dir, file, aDryRun)
    
    file = aBaseName + '.deb'
    Publish(kSwPartKinskyJukeboxDebian, kDirCmpnt, file, aDryRun)
    Publish('%s_%s-1_all.deb' % (aProduct.lower(), kKinskyJukeboxVersion), dir, file, aDryRun)
    
    file = aBaseName + '.app.tgz'
    Publish(kSwPartKinskyJukeboxMac, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_all.app.tgz' % (aProduct, kKinskyJukeboxVersion), dir, file, aDryRun)
    
    files = [kSwPartKinskyJukeboxWindows, kSwPartKinskyJukeboxDebian, kSwPartKinskyJukeboxMac]
    PublishBundle(aProduct, kBundleKinskyJukebox, files, aDryRun)
    
def XformLinnSysTray(aProduct, aBaseName, aDirOss, aDryRun):
    dir = os.path.join(aDirOss, kFamily)
    
    file = aBaseName + '.exe'
    Publish('%s_%s_all.exe' % (aProduct, kLinnSysTrayVersion), dir, file, aDryRun)
    
    file = aBaseName + '.deb'
    Publish('%s_%s-1_all.deb' % (aProduct.lower(), kLinnSysTrayVersion), dir, file, aDryRun)
    
    file = aBaseName + '.app.tgz'
    Publish('%s_%s_all.app.tgz' % (aProduct, kLinnSysTrayVersion), dir, file, aDryRun)
    
def XformKinskyMppBbc(aProduct, aBaseName, aDirOss, aDryRun):
    dir = os.path.join(aDirOss, kPluginVersion)
    
    file = aBaseName + '.kpz'
    Publish(kOssKinskyMppBbc, dir, file, aDryRun)
    
def XformKinskyMppItunes(aProduct, aBaseName, aDirOss, aDryRun):
    dir = os.path.join(aDirOss, kPluginVersion)
    
    file = aBaseName + '.kpz'
    Publish(kOssKinskyMppItunes, dir, file, aDryRun)
    
def XformKinskyMppMovieTrailers(aProduct, aBaseName, aDirOss, aDryRun):
    dir = os.path.join(aDirOss, kPluginVersion)
    
    file = aBaseName + '.kpz'
    Publish(kOssKinskyMppMovieTrailers, dir, file, aDryRun)

def XformKinskyMppRadio(aProduct, aBaseName, aDirOss, aDryRun):
    dir = os.path.join(aDirOss, kPluginVersion)
    
    file = aBaseName + '.kpz'
    Publish(kOssKinskyMppRadio, dir, file, aDryRun)
    
def XformKinskyMppShoutcast(aProduct, aBaseName, aDirOss, aDryRun):
    dir = os.path.join(aDirOss, kPluginVersion)
    
    file = aBaseName + '.kpz'
    Publish(kOssKinskyMppShoutcast, dir, file, aDryRun)
    
def XformKinskyMppWfmu(aProduct, aBaseName, aDirOss, aDryRun):
    dir = os.path.join(aDirOss, kPluginVersion)
    
    file = aBaseName + '.kpz'
    Publish(kOssKinskyMppWfmu, dir, file, aDryRun)
    
def XformKinskyPronto(aProduct, aBaseName, aDirOss, aDryRun):
    dir = os.path.join(aDirOss, kFamily)
    baseName = aBaseName.replace('release/', '')
    
    file = baseName + '9600.xcf'
    Publish(kSwPartKinskyProntoProj9600, kDirCmpnt, file, aDryRun)
    Publish(kOssKinskyProntoProj9600, dir, file, aDryRun)
    
    file = baseName + '9600.xgf'
    Publish(kSwPartKinskyProntoMod9600, kDirCmpnt, file, aDryRun)
    Publish(kOssKinskyProntoMod9600, dir, file, aDryRun)
    
    file = baseName + '9400.xcf'
    Publish(kSwPartKinskyProntoProj9400, kDirCmpnt, file, aDryRun)
    Publish(kOssKinskyProntoProj9400, dir, file, aDryRun)
    
    file = baseName + '9400.xgf'
    Publish(kSwPartKinskyProntoMod9400, kDirCmpnt, file, aDryRun)
    Publish(kOssKinskyProntoMod9400, dir, file, aDryRun)
    
    files = [kSwPartKinskyProntoProj9600, kSwPartKinskyProntoMod9600, kSwPartKinskyProntoProj9400, kSwPartKinskyProntoMod9400]
    PublishBundle(aProduct, kBundleKinskyPronto, files, aDryRun)
    
    
gkXforms = [
    ('share/Wizard%s',                                  'Wizard',               XformWizard),
    ('share/Songbox%s',                                 'Songbox',              XformSongbox),
    ('share/Songcast%s',                                'Songcast',             XformSongcast),
    ('share/Kinsky%s',                                  'Kinsky',               XformKinsky),
    ('share/KinskyClassic/InstallerKinskyClassic',      'KinskyClassic',        XformKinskyClassic),
    ('share/KinskyJukebox/InstallerKinskyJukebox',      'KinskyJukebox',        XformKinskyJukebox),
    ('share/LinnSysTray/InstallerLinnSysTray',          'LinnSysTray',          XformLinnSysTray),
    ('share/Kinsky/OssKinskyMppBbc',                    'KinskyPlugins',        XformKinskyMppBbc),
    ('share/Kinsky/OssKinskyMppItunes',                 'KinskyPlugins',        XformKinskyMppItunes),
    ('share/Kinsky/OssKinskyMppMovieTrailers',          'KinskyPlugins',        XformKinskyMppMovieTrailers),
    ('share/Kinsky/OssKinskyMppRadio',                  'KinskyPlugins',        XformKinskyMppRadio),
    ('share/Kinsky/OssKinskyMppShoutcast',              'KinskyPlugins',        XformKinskyMppShoutcast),
    ('share/Kinsky/OssKinskyMppWfmu',                   'KinskyPlugins',        XformKinskyMppWfmu),
    ('share/KinskyPronto/KinskyPronto',                 'KinskyPronto',         XformKinskyPronto),
]


def Usage():
    """Print usage info and exit with error code."""
    gOptParser.print_help()
    sys.exit(1)

gOptParser = None
gHardware = ['Linux', 'MacOsX', 'Windows', 'PocketPc', 'Ios', 'Android']

def Main():
    """Entry point into this utility."""
    global gOptParser, kDirCmpnt, kDirOss, kDirBundle
    gOptParser = optparse.OptionParser(usage = """%prog <INSTALL_DIR>""")
    gOptParser.add_option('--dry-run', action = 'store_true', dest = 'dry_run',
        default = False, help = 'try release but make no change')
    gOptParser.add_option('--hardware', action = 'store', dest = 'hardware',
        default = None, help = 'hardware platform: ' + gHardware.__str__())
    gOptParser.add_option('--release-path', action = 'store', dest  = 'release_path',
        default = '/local/share', help = 'release path')
    (opts, args) = gOptParser.parse_args()
    if len(args) == 0 or not gHardware.__contains__(opts.hardware):
        # must specify an install directory
        Usage()
        
    kDirCmpnt = os.path.join(opts.release_path, 'components')
    kDirOss = os.path.join(opts.release_path, 'oss/Releases')
    kDirBundle = os.path.join(opts.release_path, 'releases')
    
    rel = os.path.join(args[0], os.path.join(opts.hardware, 'release'))
    if not os.path.exists(rel):
        rel = args[0]
        
    RetrieveBundleNsiTemplate()
        
    for (baseName, product, cmdXform) in gkXforms:
        cmdXform(product, os.path.join(rel, baseName), os.path.join(kDirOss, product), opts.dry_run)
        
    os.remove('BundleNsiTemplate.txt')
    
if __name__ == '__main__':
    try:
        Main()
    except ToolError, e:
        os.remove('BundleNsiTemplate.txt')
        
        print >> sys.stderr, e
        sys.exit(2)
        
