Import('env')
import os

def RecursiveGlob(pathname, ondisk=True, source=False, strings=False):
     cwd = str(env.fs.getcwd())
     basename = os.path.basename(pathname)
     dirname = os.path.dirname(pathname)
     return RecursiveGlob1(dirname, basename, ondisk, source, strings, cwd)

def RecursiveGlob1(dirname, basename, ondisk, source, strings, cwd):
     files = env.Glob(os.path.normpath(os.path.join(dirname, basename)), ondisk, source, strings)
     if len(dirname)==0:
         fulldirname = cwd
     else:
         fulldirname = os.path.join(cwd, dirname)
     for f in os.listdir(fulldirname):
         if not f.startswith('.'):
             fullpath = os.path.join(fulldirname, f)
             if os.path.isdir(fullpath):
                 if len(dirname)==0:
                     subdirname = f
                 else:
                     subdirname = os.path.join(dirname, f)
                 files += RecursiveGlob1(subdirname, basename, ondisk, source, strings, cwd)
     return files

def TarEmitter(target, source, env):
    sources = []
    for src in source:
        sources += env.Glob(os.path.join(src.srcnode().abspath,"*"))
        files = RecursiveGlob(os.path.join(src.srcnode().abspath,"*"))
        for trg in target:
            env.Depends(trg, files)
    return (target, sources)

    
tarBuilder = Builder(    action          = '$TAR --exclude-vcs -c -f $TARGET -C ${SOURCE.srcdir} ${SOURCES.file}' ,
                         suffix          = '.tar',
                         ensure_suffix   = True,
                         emitter = TarEmitter,
                         source_factory=Entry)
                        

env['BUILDERS']['Tar'] = tarBuilder

Return('env')

