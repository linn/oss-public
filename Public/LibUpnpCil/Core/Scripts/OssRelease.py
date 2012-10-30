#!/usr/bin/python -tt

import os
import optparse
import sys
import subprocess
import shutil

### editable settings for each release #####################################################
# FIXME -- need to factor this out into separate XML config file

kKonfigVersion =                    '4.10.8'
kBundleKonfig =                     'B07070186'                 # for Konfig@4.10.8

kSwPartKonfigWindows =              'S08010257.exe'             # for Konfig@4.10.8
kSwPartKonfigDebian32 =             'S08250255.deb'             # for Konfig@4.10.8
kSwPartKonfigDebian64 =             'S08640103.deb'             # for Konfig@4.10.8
kSwPartKonfigMac =                  'S08260257.pkg'             # for Konfig@4.10.8

kWizardVersion =                    '4.1.3'
kBundleWizard =                     'B07260103'                 # for Wizard@4.1.3

kSwPartWizardWindows =              'S08600103.exe'             # for Wizard@4.1.3
kSwPartWizardDebian =               '_S'                        # for Wizard@4.1.3
kSwPartWizardMac =                  'S08610103.pkg'             # for Wizard@4.1.3

kSongboxVersion =                   '4.2.1'
kBundleSongbox =                    'B07240114'                 # for Songbox@4.2.1

kSwPartSongboxWindows =             'S08550114.exe'             # for Songbox@4.2.1
kSwPartSongboxDebian =              '_S08560101.deb'            # for Songbox@4.2.1
kSwPartSongboxMac =                 'S08570114.pkg'             # for Songbox@4.2.1

kSongcastVersion =                  '4.3.1'
kBundleSongcast =                   'B07220116'                 # for Songcast@4.3.1

kSwPartSongcastWindows =            'S08520115.exe'             # for Songcast@4.3.1
kSwPartSongcastDebian =             '_S'                        # for Songcast@4.3.1
kSwPartSongcastMac =                'S08530115.dmg'             # for Songcast@4.3.1

kKinskyVersion =                    '4.3.8'
kBundleKinsky =                     'B07190115'                 # for Kinsky@4.3.8

kSwPartKinskyAppStore =             'S08450118.ipa'             # for Kinsky@4.3.8 (iOS AppStore)
kSwPartKinskyAdHoc =                'S08470116.ipa'             # for Kinsky@4.3.8 (iOS AdHoc)
kSwPartKinskyManifest =             'S08460118.plist'           # for Kinsky@4.3.8 (Manifest)
kSwPartKinskyAppStoreDsym =         'S08480112.app.dSYM.zip'    # for Kinsky@4.3.8 (iOS AppStore dSYM)
kSwPartKinskyAdHocDsym =            'S08490112.app.dSYM.zip'    # for Kinsky@4.3.8 (iOS AdHoc dSYM)

kSwPartKinskyWindows =              'S08230147.exe'             # for Kinsky@4.3.8 (Windows)
kSwPartKinskyDebian =               '_S08270142.deb'            # for Kinsky@4.3.8 (Debian)
kSwPartKinskyMac =                  'S08280155.pkg'             # for Kinsky@4.3.8 (Mac)
kSwPartKinskyAndroid =              'S08580104.apk'             # for Kinsky@4.3.8 (Android)

kKinskyJukeboxVersion =             '4.1.2'

kBundleKinskyJukebox =              'B07150109'                 # for KinskyJukebox@4.1.2
kSwPartKinskyJukeboxWindows =       'S08320110.exe'             # for KinskyJukebox@4.1.2 (Windows)
kSwPartKinskyJukeboxDebian =        'S08330110.deb'             # for KinskyJukebox@4.1.2 (Debian)
kSwPartKinskyJukeboxMac =           '_S08340110.app.tgz'         # for KinskyJukebox@4.1.2 (Mac)

kOssKinskyMppBbc =                  'OssKinskyMppBbc.kpz'
kOssKinskyMppItunes =               'OssKinskyMppItunes.kpz'
kOssKinskyMppMovieTrailers =        'OssKinskyMppMovieTrailers.kpz'
kOssKinskyMppRadio =                'OssKinskyMppRadio.kpz'
kOssKinskyMppShoutcast =            'OssKinskyMppShoutcast.kpz'
kOssKinskyMppWfmu =                 'OssKinskyMppWfmu.kpz'
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
        elif sys.platform == "win32":
            cmdLineMd5 = ['C:\\bin\\md5sum.exe', os.path.abspath(aFile)]
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
    
def XformKonfig(aProduct, aBaseName, aDirOss, aDryRun):
    dir = os.path.join(aDirOss, kFamily)
        
    file = (aBaseName % '/InstallerKonfig') + '.exe'
    Publish(kSwPartKonfigWindows, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_win.exe' % (aProduct, kKonfigVersion), dir, file, aDryRun)
    
    file = (aBaseName % '/InstallerKonfig') + '.deb'
    Publish(kSwPartKonfigDebian32, kDirCmpnt, file, aDryRun)
    Publish('%s_%s-1_i386.deb' % (aProduct.lower(), kKonfigVersion), dir, file, aDryRun)
    
    file = (aBaseName % '/x64/InstallerKonfig') + '.deb'
    Publish(kSwPartKonfigDebian64, kDirCmpnt, file, aDryRun)
    Publish('%s_%s-1_amd64.deb' % (aProduct.lower(), kKonfigVersion), dir, file, aDryRun)
    
    file = (aBaseName % '/InstallerKonfig') + '.pkg'
    Publish(kSwPartKonfigMac, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_osx.pkg' % (aProduct, kKonfigVersion), dir, file, aDryRun)
        
    file = (aBaseName % '/UpdaterKonfig_win') + '.dll'
    Publish('%s_%s_win.dll' % (aProduct, kKonfigVersion), dir, file, aDryRun)
    
    file = (aBaseName % '/UpdaterKonfig_osx') + '.dll'
    Publish('%s_%s_osx.dll' % (aProduct, kKonfigVersion), dir, file, aDryRun)
    
    files = [kSwPartKonfigWindows, kSwPartKonfigDebian32, kSwPartKonfigDebian64, kSwPartKonfigMac]
    PublishBundle(aProduct, kBundleKonfig, files, aDryRun)
    
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
	
    file = (aBaseName % '/UpdaterKinsky_win') + '.dll'
    Publish('%s_%s_win.dll' % (aProduct, kKinskyVersion), dir, file, aDryRun)
	
    file = (aBaseName % '/UpdaterKinsky_osx') + '.dll'
    Publish('%s_%s_osx.dll' % (aProduct, kKinskyVersion), dir, file, aDryRun)
	
    file = (aBaseName % '/uk.co.linn.kinsky-Signed') + '.apk'
    Publish(kSwPartKinskyAndroid, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_and.apk' % (aProduct, kKinskyVersion), dir, file, aDryRun)
	
    files = [kSwPartKinskyAppStore, kSwPartKinskyAppStoreDsym, kSwPartKinskyAdHoc, kSwPartKinskyAdHocDsym, kSwPartKinskyManifest, kSwPartKinskyWindows, kSwPartKinskyDebian, kSwPartKinskyMac, kSwPartKinskyAndroid]
    PublishBundle(aProduct, kBundleKinsky, files, aDryRun)

    
def XformKinskyJukebox(aProduct, aBaseName, aDirOss, aDryRun):
    dir = os.path.join(aDirOss, kFamily)
    
    file = (aBaseName % '/InstallerKinskyJukebox') + '.exe'
    Publish(kSwPartKinskyJukeboxWindows, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_win.exe' % (aProduct, kKinskyJukeboxVersion), dir, file, aDryRun)
    
    file = (aBaseName % '/InstallerKinskyJukebox') + '.deb'
    Publish(kSwPartKinskyJukeboxDebian, kDirCmpnt, file, aDryRun)
    Publish('%s_%s-1_all.deb' % (aProduct.lower(), kKinskyJukeboxVersion), dir, file, aDryRun)
    
    file = (aBaseName % '/InstallerKinskyJukebox') + '.pkg'
    Publish(kSwPartKinskyJukeboxMac, kDirCmpnt, file, aDryRun)
    Publish('%s_%s_osx.pkg' % (aProduct, kKinskyJukeboxVersion), dir, file, aDryRun)
    
    files = [kSwPartKinskyJukeboxWindows, kSwPartKinskyJukeboxDebian, kSwPartKinskyJukeboxMac]
    PublishBundle(aProduct, kBundleKinskyJukebox, files, aDryRun)
    
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
    
    
gkXforms = [
    ('share/Konfig%s',                                  'Konfig',               XformKonfig),
    ('share/Wizard%s',                                  'Wizard',               XformWizard),
    ('share/Songbox%s',                                 'Songbox',              XformSongbox),
    ('share/Songcast%s',                                'Songcast',             XformSongcast),
    ('share/Kinsky%s',                                  'Kinsky',               XformKinsky),
    ('share/KinskyJukebox%s',                           'KinskyJukebox',        XformKinskyJukebox),
    ('share/Kinsky/OssKinskyMppBbc',                    'KinskyPlugins',        XformKinskyMppBbc),
    ('share/Kinsky/OssKinskyMppItunes',                 'KinskyPlugins',        XformKinskyMppItunes),
    ('share/Kinsky/OssKinskyMppMovieTrailers',          'KinskyPlugins',        XformKinskyMppMovieTrailers),
    ('share/Kinsky/OssKinskyMppRadio',                  'KinskyPlugins',        XformKinskyMppRadio),
    ('share/Kinsky/OssKinskyMppShoutcast',              'KinskyPlugins',        XformKinskyMppShoutcast),
    ('share/Kinsky/OssKinskyMppWfmu',                   'KinskyPlugins',        XformKinskyMppWfmu),
]


def Usage():
    """Print usage info and exit with error code."""
    gOptParser.print_help()
    sys.exit(1)

gOptParser = None
gHardware = ['Linux', 'MacOsX', 'Windows', 'Ios', 'Android']

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
        
