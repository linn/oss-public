import os
import ConfigParser

gConfigFile = None
# make sure that the default path is something sensible
# assuming that SetDefaultPath is never called!
gDefaultPath = os.getcwd().replace('\\','/')
gProjectPath = None
gTexturePath = None


class FileOutside(Exception):
    def __init__(self, aFilename):
        self.iFilename = aFilename
    def __str__(self):
        return '%s is outside directory %s' % (self.iFilename, ProjectPath())
        
def DefaultPath():
    return gDefaultPath

def SetDefaultPath(aPath):
    global gDefaultPath
    gDefaultPath = os.path.abspath(aPath)
    gDefaultPath = gDefaultPath.replace('\\','/') + '/'
        
def ProjectPath():
    global gProjectPath
    if gProjectPath == None:
        if gConfigFile:
            config = ConfigParser.ConfigParser()
            config.readfp(open(gConfigFile))
            gProjectPath = config.get('PATHS', 'ProjectDir').replace('\\','/') + '/'
            # project path is invalid.... set it to the default directory
            if not os.path.exists(gProjectPath):
                gProjectPath = gDefaultPath
        else:
            # hmmm make some stab at a valid path!
            gProjectPath = gDefaultPath
    return gProjectPath

def SetProjectPath(aPath):
    global gProjectPath
    gProjectPath = aPath
    gProjectPath = gProjectPath.replace('\\', '/')
    if gProjectPath[-1] != '/':
        gProjectPath = gProjectPath + '/'
    try:
        if gConfigFile:
            config = ConfigParser.ConfigParser()
            config.readfp(open(gConfigFile))
            if not config.has_section('PATHS'):
                config.add_section('PATHS')
            config.set('PATHS', 'ProjectDir', os.path.abspath(aPath))
            config.write(open('LinnGuiEd.cfg', 'wt'))
    except IOError:
        # thats fine we just don't have a configuration file that holds
        # our project directory....
        pass
    
def TexturePath():
    global gTexturePath
    if gTexturePath == None:
        if gConfigFile:
            config = ConfigParser.ConfigParser()
            config.readfp(open(gConfigFile))
            gTexturePath = config.get('PATHS', 'TextureCache').replace('\\','/') + '/'
            # project path is invalid.... set it to the default directory
            if not os.path.exists(gTexturePath):
                gTexturePath = ProjectPath()
        else:
            # hmmm make some stab at a valid path!
            gTexturePath = ProjectPath()
    return gTexturePath

def SetTexturePath(aPath):
    global gTexturePath
    gTexturePath = aPath
    gTexturePath = gTexturePath.replace('\\', '/')
    if gTexturePath[-1] != '/':
        gTexturePath = gTexturePath + '/'
    try:
        if gConfigFile:
            config = ConfigParser.ConfigParser()
            config.readfp(open(gConfigFile))
            if not config.has_section('PATHS'):
                config.add_section('PATHS')
            config.set('PATHS', 'TextureCache', os.path.abspath(aPath))
            config.write(open('LinnGuiEd.cfg', 'wt'))
    except IOError:
        # thats fine we just don't have a configuration file that holds
        # our project directory....
        pass

def CreateConfigFile(aFilename):
    """Create the default configuration file if none present."""
    global gConfigFile
    gConfigFile = aFilename
    try:
        open(gConfigFile)
    except IOError:
        # we dont have a config file already lets create one put in
        # a default project directory....
        config = ConfigParser.ConfigParser()
        config.add_section('PATHS')
        config.set('PATHS', 'ProjectDir', DefaultPath())
        config.set('PATHS', 'TextureCache', DefaultPath())
        config.write(open(gConfigFile, 'wt'))
        
        
        