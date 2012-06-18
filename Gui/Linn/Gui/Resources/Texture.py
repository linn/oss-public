import wx
import TextureManager
import Configuration
import Linn.Gui.Renderer


class TextureWx:
    def __init__(self, aFilename):
        self.iFilename = aFilename
        self.iImage = wx.Image(self.iFilename, wx.BITMAP_TYPE_ANY)
        self.iTexture = self.iImage#self.iImage.ConvertToBitmap()
        if self.iFilename.find(Configuration.DefaultPath()) != -1:
            self.iPath = Configuration.DefaultPath()
            self.iFilename = self.iFilename.replace(Configuration.DefaultPath(), '')
        elif self.iFilename.find(Configuration.ProjectPath()) != -1:
            self.iPath = Configuration.ProjectPath()
            self.iFilename = self.iFilename.replace(Configuration.ProjectPath(), '')
        
    def __deepcopy__(self, memo={}):
        return self
        
    def Surface(self):
        return self.iTexture
    
    def Width(self):
        return self.iTexture.GetWidth()
    
    def Height(self):
        return self.iTexture.GetHeight()
        
    def Filename(self):
        return self.iFilename
    
    def Refresh(self):
        self.iImage = wx.Image(self.iPath + self.iFilename, wx.BITMAP_TYPE_ANY)
        self.iTexture = self.iImage#self.iImage.ConvertToBitmap()



    