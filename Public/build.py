#!/usr/bin/python -tt

import subprocess
import optparse
import types
import os.path
import sys


gLibUpnp = [
    'LibUpnpCil/Core',
    'LibUpnpCil/SysLib',
    'LibUpnpCil/Control',
    'LibUpnpCil/Services',
    'LibUpnpCil/DidlLite',
    'LibUpnpCil/Topology',
]

# XXX -- Python 2.5+ only
# gWebLinuxOnly = ['Web'] if sys.platform.startswith('linux') else []
gWebLinuxOnly = sys.platform.startswith('linux') and ['Web'] or []

gTopLevelCmpnts = {
    'Volkano': {
        'components': ['External', 'Xml', 'Kodegen'] + gLibUpnp \
                    + gWebLinuxOnly + ['Gui', 'Layouts', 'Kinsky']
    },

    'LinnSysTray': {
        'components': ['Xml', 'Kodegen'] + gLibUpnp + ['LinnSysTray']
    },

    'SneakyLastFm': {
        'components': ['Xml', 'Kodegen'] + gLibUpnp + ['SneakyLastFm']
    },

    'SneakyMedia': {
        'components': ['Xml', 'Kodegen'] + gLibUpnp + ['SneakyMedia']
    },

    'KinskyDesktop': {
        'components': ['External', 'Xml', 'Kodegen'] + gLibUpnp \
                    + ['Kinsky', 'MediaProviderPlugins', ('Layouts', 'Kinsky'), 'KinskyDesktop']
    },

    'KinskyPda': {
        'components': ['External', 'Xml', 'Kodegen'] + gLibUpnp \
                    + ['Kinsky', 'MediaProviderPlugins', ('Layouts', 'Kinsky'), 'KinskyDesktop'],
        'toolchain': 'PocketPc'
    },

    'KinskyClassic': {
        'components': ['External', 'Xml', 'Kodegen'] + gLibUpnp + ['Gui', 'Kinsky', 'MediaProviderPlugins', 'Layouts', 'KinskyClassic']
    },

    'KinskyWeb': {
        'components': ['External', 'Xml', 'Kodegen'] + gLibUpnp + ['Kinsky', 'MediaProviderPlugins', 'KinskyWeb']
    },

    'LibUpnpCil': {
        'components': ['External', 'Xml', 'Kodegen'] + gLibUpnp
    },
    
    'KinskyJukebox': {
        'components': ['External', 'Xml', 'Kodegen'] + gLibUpnp + ['Kinsky', 'KinskyJukebox']
    },
    
    'DoktorKinsky': {
        'components': ['External', 'Xml', 'Kodegen'] + gLibUpnp + ['Doktor', 'DoktorKinsky']
    },

    'LinnSetup': {
        'components': ['Xml', 'Kodegen'] + gLibUpnp + ['LinnSetup']
    },

    'KinskyPronto': {
        'components': ['KinskyPronto']
    },

    'Jed2Bin': {
        'components': ['Jed2Bin']
    },

    'Services': {
        'components': ['Xml', 'Kodegen', 'LibUpnpCil/Core', 'LibUpnpCil/SysLib', 'LibUpnpCil/Control', 'LibUpnpCil/Services']
    },

    'Kodegen': {
        'components': ['Xml', 'Kodegen']
    },

    'External': {
        'components': ['External']
    },

    'Layouts': {
        'components': ['Layouts']
    },
}

gUsage = """builds any one of well-known top-level OSS components:
%(cmd)s <TOP_CMPNT> [<SCONS_BLDSYS_ARG> ...]

%(cmd)s LinnSysTray variant=trace Lib
%(cmd)s Layouts -c"""
    

def Build(aArgs, aRelPath, aToolchain, aDryRun):
    """Build software component given via relative path with given arguments."""
    cmdLine = ['scons', aToolchain]
    cmdLine.extend(aArgs)
    cmdLine = ' '.join(cmdLine)
    if aDryRun:
        print 'cd %s; %s; cd -' % (aRelPath, cmdLine)
        return
    p = subprocess.Popen(args = cmdLine, cwd = aRelPath, shell = True)
    retVal = p.wait()
    if retVal != 0:
        print >> sys.stderr, '%s: exited with %d' % (cmdLine, retVal)
        sys.exit(retVal)


def List():
    """List all known top-level components that can be built and exit cleanly."""
    for cmpnt in sorted(gTopLevelCmpnts.keys()):
        print cmpnt
    sys.exit(0)


# entry point
if __name__ == '__main__':
    cmd = {'cmd': os.path.basename(sys.argv[0])}
    optParser = optparse.OptionParser(usage = gUsage % cmd)
    optParser.add_option('-n', '--dry-run', action = 'store_true', default = False, dest = 'iDryRun',
                        help = 'print, but do not execute build steps')
    optParser.add_option('-l', '--list', action = 'store_true', default = False, dest = 'iList',
                        help = 'list all known components that can be built')
    optParser.add_option('-c', '--clean', action = 'store_true', default = False, dest = 'iClean',
                        help = 'clean rather than build given top-level component')
    (opts, args) = optParser.parse_args()
    if opts.iList: List()
    if not args: optParser.error('missing top-level component')
    if opts.iClean: args += ['-c']
    topLvlCmpnt = args.pop(0)
    
    # find top-level component based on flimsy input string...
    
    c = gTopLevelCmpnts.keys()
    
    found = 0
    component = ''
    for cm in c:
        if cm.lower().find(topLvlCmpnt.lower()) != -1:
            found = found + 1
            component = cm
    if found == 1:
        print '=============================================='
        print 'Matched a top level component [', component, ']'
        print '=============================================='
        topLvlCmpnt = component
    
    bldSpecs = gTopLevelCmpnts.get(topLvlCmpnt, '')

    
    if not bldSpecs: optParser.error('unknown top-level component')
    toolchain = bldSpecs.get('toolchain', 'Windows')
    for cmpnt in bldSpecs['components']:
        if isinstance(cmpnt, types.TupleType):
            cmpnt, target = cmpnt
            Build(args + [target], cmpnt, 'hardware=%s' % toolchain, opts.iDryRun)
        else:
            Build(args, cmpnt, 'hardware=%s' % toolchain, opts.iDryRun)
