import os.path
import SCons.Builder
import SCons.Node.FS
import SCons.Util
import CsProg

#CsProgramScanner = Linn.BuildSystem.Scanner.CsProg.CsProgramScanner()
CsProgramScanner = CsProg.CsProgramScanner()

csccom = "$CSC $CSCFLAGS -t:$CSCTARGET -out:${TARGET.abspath} $SOURCES $_CSCLIBPATH $_CSCLIBS $_CSCRESOURCES $_CSCWINICON"
csclibcom = "$CSC -t:library $CSCLIBFLAGS -out:${TARGET.abspath} $SOURCES $_CSCLIBPATH $_CSCLIBS $_CSCRESOURCES $_CSCWINICON"

CsBuilder = SCons.Builder.Builder(action = '$CSCCOM',
                                  source_factory = SCons.Node.FS.default_fs.Entry,
                                  prefix = '$CLIPROGPREFIX',
                                  suffix = '$CLIPROGSUFFIX',
                                  target_scanner = CsProgramScanner)

CsLibBuilder = SCons.Builder.Builder(action = '$CSCLIBCOM',
                                     source_factory = SCons.Node.FS.default_fs.Entry,
                                     prefix = '$CLILIBPREFIX',
                                     suffix = '$CLILIBSUFFIX',
                                     target_scanner = CsProgramScanner)

def _parsecscres(prefix, resources):
    result = []
    for r in resources:
        if isinstance(r, SCons.Node.FS.File):
            result.append(prefix + str(r))
            continue
        if not SCons.Util.is_String(r):
            result.append(prefix + r)
    return result

def _parsewinicon(prefix, icon):
    return prefix + icon

def generate(env):
    env['BUILDERS']['CliProgramV2'] = CsBuilder
    env['BUILDERS']['CliLibraryV2'] = CsLibBuilder

    env['CSC']            = 'csc'
    env['_CSCLIBS']       = "${_stripixes('-r:$CLILIBPREFIX', CLILIBS, '$CLILIBSUFFIX', '-r', '', __env__)}"
    env['_CSCRESOURCES']  = "${_parsecscres('-res:', CLIRESOURCES)}"
    env['_CSCLIBPATH']    = "${_stripixes('-lib:', CLILIBPATH, '', '-r', '', __env__)}"
    env['_CSCWINICON']    = "${_parsewinicon('-win32icon:', WINICON)}"
    env['CSCFLAGS']       = SCons.Util.CLVar('')
    env['CSCLIBFLAGS']    = SCons.Util.CLVar('')
    env['CSCCOM']         = SCons.Action.Action(csccom)
    env['CSCLIBCOM']      = SCons.Action.Action(csclibcom)
    env['CSCTARGET']      = 'exe'
    # these should be under platform specific file
    env['CLIPROGPREFIX']   = ''
    env['CLIPROGSUFFIX']   = '.exe'
    env['CLILIBPREFIX']   = ''
    env['CLILIBSUFFIX']   = '.dll'
    env['CLILIBPREFIXES'] = [ '$CLILIBPREFIX' ]
    env['CLILIBSUFFIXES'] = [ '$CLILIBSUFFIX' ]
    env['_parsecscres'] = _parsecscres
    env['_parsewinicon'] = _parsewinicon

    env.Append(CSCFLAGS = '-nologo')
    env.Append(CSCLIBFLAGS = '-nologo')
    
    if env['variant'] == "debug":
        env.Append(CSCFLAGS = '-debug+')
        env.Append(CSCFLAGS = '-debug:full')
        env.Append(CSCLIBFLAGS = '-debug+')
        env.Append(CSCLIBFLAGS = '-debug:full')
        env.Append(CSCFLAGS = '-define:DEBUG')
        env.Append(CSCLIBFLAGS = '-define:DEBUG')
        env.Append(CSCFLAGS = '-define:TRACE')
        env.Append(CSCLIBFLAGS = '-define:TRACE')
    if env['variant'] == "trace":
        env.Append(CSCFLAGS = '-optimize+')
        env.Append(CSCLIBFLAGS = '-optimize+')
        env.Append(CSCFLAGS = '-define:TRACE')
        env.Append(CSCLIBFLAGS = '-define:TRACE')
    if env['variant'] == "release":
        env.Append(CSCFLAGS = '-optimize+')
        env.Append(CSCLIBFLAGS = '-optimize+')
    if env['variant'] == "":
        env.Append(CSCFLAGS = '-optimize+')
        env.Append(CSCLIBFLAGS = '-optimize+')

def exists(env):
    return env.Detect('csc')
    
