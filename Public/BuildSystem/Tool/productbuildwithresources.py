import os
import os.path
import shutil
import SCons.Builder
import SCons.Node.FS
import SCons.Util
#import xml.etree.ElementTree
from xml.dom.minidom import parseString

#def iterParent(tree):
#for parent in tree.getiterator()
#for child in parent:
#yield parent, child

#for parent, child in iterParent(root):
#    print(child)
#    #dir(child)


def setupEnvironment(target, source, env):
    if 'PRODUCTNAME' in env:
        productName = env['PRODUCTNAME']
    else:
        productName = os.path.splitext(os.path.split(str(source[0]))[1])[0]

    targetDir = target[0].dir.abspath
    pkgInternalName = 'uk.co.linn.' + productName + '.pkg'    
    distributionDist = os.path.join(targetDir, 'distribution.dist')
    resourcesDir = os.path.join(targetDir, "Resources")
    if os.path.exists(resourcesDir): 
        shutil.rmtree(resourcesDir, True)
    os.mkdir(resourcesDir)
    pkgBuildDir = os.path.join(targetDir, pkgInternalName)
    
    env['PKGBUILDPRODUCTNAME'] = productName
    env['PKGBUILDINTERMEDIATEOUTPUT'] = pkgBuildDir
    env['PRODUCTBUILDDISTFILE'] = distributionDist
    env['PRODUCTBUILDRESOURCES'] = resourcesDir
    if 'INSTALLERRESOURCES' not in env:
        env['INSTALLERRESOURCES'] = {}

def copyResources(target, source, env):
    resourcesDir = env['PRODUCTBUILDRESOURCES']
    
    # copy license file if present
    if 'LICENSEFILE' in env['INSTALLERRESOURCES']:
        shutil.copyfile(env['INSTALLERRESOURCES']['LICENSEFILE'], os.path.join(resourcesDir, 'License.txt'))



def populateDistFile(target, source, env):
    # open the distribution.dist file
    f = open(env['PRODUCTBUILDDISTFILE'], 'r')
    distFile = f.read()
    f.close()

    doc = parseString(distFile)
    root = doc.documentElement    

    titleElem = doc.createElement('title')
    titleElem.appendChild(doc.createTextNode(env['PKGBUILDPRODUCTNAME']))
    root.appendChild(titleElem)
    
    # insert a license entry if present
    if 'LICENSEFILE' in env['INSTALLERRESOURCES']:
        licenseElem = doc.createElement('license')
        licenseElem.setAttribute('file', 'License.txt')
        root.appendChild(licenseElem)

    # add restart require flag and version
    reqRestart = env.get('REQUIRERESTART', False);

    pkgElems = root.getElementsByTagName('pkg-ref')
    for elem in pkgElems:
        if reqRestart and elem.hasAttribute('onConclusion'):
            elem.setAttribute('onConclusion', 'RequireRestart')
        if elem.hasAttribute('version'):
            elem.setAttribute('version', env['VERSION'])
    
    f = open(env['PRODUCTBUILDDISTFILE'], 'w')
    f.write(doc.toxml())
    #f.write(xml.etree.ElementTree.tostring(root))
    f.close()

    
pkgbuildcomponentcom = "$PKGBUILD $SOURCEOPTIONNAME $SOURCE --install-location ${_parseInstallDir(__env__)} ${_parseScripts(__env__)} ${_parseIdentifier(__env__)} ${_parseComponentPlist(__env__)} $PKGBUILDINTERMEDIATEOUTPUT"
productbuildsynthesisecom = "$PRODUCTBUILD --synthesize --package $PKGBUILDINTERMEDIATEOUTPUT $PRODUCTBUILDDISTFILE"
productbuilddistcom = "$PRODUCTBUILD --distribution $PRODUCTBUILDDISTFILE --resources $PRODUCTBUILDRESOURCES --package-path ${TARGET.dir} --sign $PKGCERT $TARGET"


ProductBuildWithResourcesBuilder = SCons.Builder.Builder(action = [setupEnvironment, '$PKGBUILDCOMPONENTCOM', '$PRODUCTBUILDSYNTHESISECOM', copyResources, populateDistFile,  '$PRODUCTBUILDDISTCOM'],
                                            source_factory = SCons.Node.FS.default_fs.Dir,
                                            target_factory = SCons.Node.FS.default_fs.File,
                                            prefix = '',
                                            suffix = '.pkg')

def _parseInstallDir(env):
    return env.get('INSTALLDIR', '/Applications')


def _parseScripts(env):
    if 'SCRIPTSDIR' in env:
        return '--scripts ' + str(env['SCRIPTSDIR'][0])
    else:
        return ''


def _parseIdentifier(env):
    if 'IDENTIFIER' in env:
        return '--identifier ' + env['IDENTIFIER']
    else:
        return ''


def _parseComponentPlist(env):
    if 'COMPONENTPLIST' in env:
        return '--component-plist ' + str(env['COMPONENTPLIST'])
    else:
        return ''


def generate(env):
    env['BUILDERS']['ProductBuildWithResources'] = ProductBuildWithResourcesBuilder

    env['PRODUCTBUILD']              = env.Detect('productbuild')
    env['PKGBUILD']                  = env.Detect('pkgbuild')
    env['PKGBUILDCOMPONENTCOM']      = SCons.Action.Action(pkgbuildcomponentcom)
    env['PRODUCTBUILDSYNTHESISECOM'] = SCons.Action.Action(productbuildsynthesisecom)
    env['PRODUCTBUILDDISTCOM']       = SCons.Action.Action(productbuilddistcom)
    env['_parseInstallDir']          = _parseInstallDir
    env['_parseScripts']             = _parseScripts
    env['_parseIdentifier']          = _parseIdentifier
    env['_parseComponentPlist']      = _parseComponentPlist


def exists(env):
    return env.Detect('productbuild') and env.Detect('pkgbuild')

