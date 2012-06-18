import os
import os.path
import sys

def Install():
    f = sys.modules[__name__].__file__
    # assuming install tree location install/lib/Linn/Path.py
    return os.path.dirname(os.path.dirname(os.path.dirname(f)))
def Build():
    return os.path.normpath(os.path.join(Install(), '..', 'build'))
def Volkano():
    return Source()

def Source():
    return os.path.normpath(os.path.join(Install(), '..'))

def Share():
    return os.path.normpath(Install() + '/share')

def ToolsInstall():
    return os.path.normpath(Install() + '/lib/Tools')

def TestInstall():
    return os.path.normpath(Install() + '/bin/Test')

def BinInstall():
    return os.path.normpath(Install() + '/bin')

def LibInstall():
    return os.path.normpath(Install() + '/lib')

def IncludeInstall():
    return os.path.normpath(Install() + '/include')

def OptInstall():
    return os.path.normpath(Install() + '/opt')

def RtemsInstall():
    return OptInstall() + '/rtems'

def XmlSchema():
    return '/share/Linn/XmlSchemas'

def XmlSchemaInstall():
    return os.path.normpath(Install() + XmlSchema())

def XmlTags():
    return os.path.normpath(Install() + XmlTagsRelative())

def XmlTagsRelative():
    return '/include/Linn/Tags.xml'

def XmlTagsRepoPath():
    return 'svn://eng.linn.co.uk/Tags'

def AbsPathFromSource(aPath):
    return os.path.normpath(Source() + aPath)

def AbsPathFromBuild(aPath):
    return os.path.normpath(Build() + aPath)

def AbsPathFromInstall(aPath):
    return os.path.normpath(Install() + aPath)
