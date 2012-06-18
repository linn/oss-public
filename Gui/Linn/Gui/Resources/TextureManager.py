import re
import os
import Texture
import Configuration

gTextureManager = None

def GetTextureManager():
    global gTextureManager
    if not gTextureManager:
        gTextureManager = TextureManagerWx()
    return gTextureManager


class TextureManager:
    def __init__(self):
        self.iTextureList = []
        
    def TextureByNameOrLoad(self, aName):
        """Find the texture in texture cache, attempt to load if not found."""
        # remove upto the root directory (if present)
        aName = aName.replace('\\','/')
        aName = aName.replace(Configuration.DefaultPath(), '')
        aName = aName.replace(Configuration.TexturePath(), '')
        for texture in self.iTextureList:
            if texture.Filename() == aName:
                return texture
        #print '%s not in texture cache...' % aName
        texture = self.AttemptLoad(aName)
        self.iTextureList.append(texture)
        return texture
        
    def AttemptLoad(self, aName):
        try:
            #print 'attempting to load from projdir...'
            open(Configuration.TexturePath()+aName)
            return self.CreateTexture(Configuration.TexturePath()+aName)
        except Exception, e:
            try:
                #print 'attempting to load from defaultdir...'
                open(Configuration.DefaultPath()+aName)
                return self.CreateTexture(Configuration.DefaultPath()+aName)
            except Exception, e:
                print '%s not found...' % aName 
                default = Configuration.DefaultPath()+'/Default/NotFound.bmp'
                open(default)
                return self.TextureByNameOrLoad(default)
    
    def CreateTexture(self, aFilename):
        sys.stderr.write("Unimplemented pure virtual TextureManager.CreateTexture")
        sys.exit(-1)
    
    def IsInCache(self, aTexture):
        for texture in self.iTextureList:
            if aTexture.Fullname() == texture.Fullname():
                return True
        return False
        
    def NumTextures(self):
        return len(self.iTextureList)
    
    def FlushCache(self):
        self.iTextureList = []
        #print 'Texture cache flushed...'
        
    def Refresh(self):
        #print 'Refreshing texture cache...'
        for texture in self.iTextureList:
            texture.Refresh()
        

class TextureManagerWx(TextureManager):
    def __init__(self):
        TextureManager.__init__(self)
        
    def CreateTexture(self, aFilename):
        """Load specific texture type."""
        return Texture.TextureWx(aFilename)



