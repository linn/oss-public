import Plugin
import TextureManager

class TextureArray(Plugin.Plugin):
    def __init__(self, aName=None, aTextureList=None, aFps=None, aLoop=None):
        if aName == None:
            aName = self.UniqueName('TextureArray')
        Plugin.Plugin.__init__(self, aName)
        if aTextureList:
            self.iTextureList = aTextureList
        else:
            self.iTextureList = []
        self.iNumTextures = len(self.iTextureList)
        self.iTextureIndex = 0
    
    def Load(self, aStream):
        Plugin.Plugin.Load(self, aStream)
        
        textures = aStream.LoadNode('Texture')
        for texture in textures:
            self.iTextureList.append(texture.firstChild.nodeValue)
    
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'TextureArray')
            
        Plugin.Plugin.Save(self, aStream)
        
        for texture in self.iTextureList:
            textureRef = aStream.CreateElement('Texture')
            textureRef.appendChild(aStream.CreateTextNode(texture.Filename()))
            aStream.SaveNode(textureRef)
        
    def Link(self):
        #print 'Linking', self.Fullname(), '[TextureArray]'
        Plugin.Plugin.Link(self)
        
        numTextures = len(self.iTextureList)
        for i in range(numTextures):
            texture = self.iTextureList[i]
            self.iTextureList[i] = TextureManager.GetTextureManager().TextureByNameOrLoad(texture)
        self.iLinked = True
               
    def Vector3d(self, aVector3d):
        self.iTextureIndex = aVector3d.iX
        if self.iTextureIndex > len(self.iTextureList) - 1:
            self.iTextureIndex = len(self.iTextureList) - 1
        if self.iTextureIndex < 0:
            self.iTextureIndex = 0
        self.iPlugin.Texture(self.iTextureList[self.iTextureIndex])
        
    def TextureAtIndex(self, aTextureIndex):
        return self.iTextureList[aTextureIndex]
    
    def SetTexture(self, aTexture, aTextureIndex=0):
        self.iTextureList[aTextureIndex] = aTexture
        if aTextureIndex == self.iTextureIndex:
            self.iPlugin.Texture(aTexture)
        
    def NumTextures(self):
        return len(self.iTextureList)
    
    def SetNumTextures(self, aNumTextures):
        if aNumTextures < self.NumTextures():
            self.iTextureList = self.iTextureList[0:aNumTextures]
        else:
            numToAdd = aNumTextures - self.NumTextures()
            for i in range(numToAdd):
                self.iTextureList.append('')
        if self.iTextureIndex > aNumTextures:
            self.iTextureIndex = aNumTextures - 1
            self.iPlugin.Texture(self.iTextureList[self.iTextureIndex])
        self.ObserverUpdate()

        
class TextureArrayFixed(TextureArray):
    def __init__(self, aName=None, aTextureList=None, aFps=None, aLoop=None):
        TextureArray.__init__(self, aName, aTextureList, aFps, aLoop)
        
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'TextureArrayFixed')
            
        TextureArray.Save(self, aStream)
        
        
        
        