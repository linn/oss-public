import string
import SCons.Node
import SCons.Node.FS
import SCons.Scanner
import SCons.Util

# global, set by --debug=findlibs
print_find_libs = None

def CsProgramScanner(**kw):
    """Return a prototype Scanner instance for scanning C# executable
    files for assembly dependencies"""
    kw['path_function'] = SCons.Scanner.FindPathDirs('CLILIBPATH')
    ps = apply(SCons.Scanner.Base, [scan, "CsProgramScanner"], kw)
    return ps

def scan(node, env, libpath = ()):
    """
    This scanner scans program files for assembly
    dependencies.  It will search the CLILIBPATH environment variable
    for libraries specified in the CLILIBS variable, returning any
    files it finds as dependencies.
    """
    try:
        libs = env['CLILIBS']
    except KeyError:
        # There are no CLILIBS in this environment, so just return a null list:
        return []

    if SCons.Util.is_String(libs):
        libs = string.split(libs)
    elif SCons.Util.is_List(libs):
        libs = SCons.Util.flatten(libs)
    else:
        libs = [libs]

    try:
        prefix = env['CLILIBPREFIXES']
        if not SCons.Util.is_List(prefix):
            prefix = [ prefix ]
    except KeyError:
        prefix = [ '' ]

    try:
        suffix = env['CLILIBSUFFIXES']
        if not SCons.Util.is_List(suffix):
            suffix = [ suffix ]
    except KeyError:
        suffix = [ '' ]

    pairs = []
    for suf in map(env.subst, suffix):
        for pref in map(env.subst, prefix):
            pairs.append((pref, suf))

    result = []

    if callable(libpath):
        libpath = libpath()

    find_file = SCons.Node.FS.find_file
    adjustixes = SCons.Util.adjustixes
    for lib in libs:
        if SCons.Util.is_String(lib):
            lib = env.subst(lib)
            for pref, suf in pairs:
                l = adjustixes(lib, pref, suf)
                l = find_file(l, libpath, verbose=print_find_libs)
                if l:
                    result.append(l)
        else:
            result.append(lib)

    try:
        libs = env['CLIRESOURCES']
    except KeyError:
        # There are no CLIRESOURCES in this environment, so just return a the list so far
        return result
    if SCons.Util.is_String(libs):
        libs = string.split(libs)
    elif SCons.Util.is_List(libs):
        libs = SCons.Util.flatten(libs)
    else:
        libs = [libs]
        
    for lib in libs:
        if SCons.Util.is_String(lib):
            lib = env.subst(lib)
            l = find_file(lib, libpath, verbose=print_find_libs)
            if l:
                result.append(l)
        else:
            result.append(lib)

    return result
