import wx
import sys
import threading
import time
import Resources.Layout
import SceneGraph.Visitor

class IRenderer:
    def BeginFrame(self):
        sys.stderr.write("Unimplemented pure virtual IRenderer.BeginFrame")
        sys.exit(-1)
        
    def EndFrame(self):
        sys.stderr.write("Unimplemented pure virtual IRenderer.EndFrame")
        sys.exit(-1)
        
    def SetRenderTarget(self, aDc):
        sys.stderr.write("Unimplemented pure virtual IRenderer.SetRenderTarget")
        sys.exit(-1)
        
    def SetFont(self):
        sys.stderr.write("Unimplemented pure virtual IRenderer.SetFont")
        sys.exit(-1)
        
    def Render(self):
        sys.stderr.write("Unimplemented pure virtual IRenderer.Render")
        sys.exit(-1)
        
    def DrawPolygon(self):
        sys.stderr.write("Unimplemented pure virtual IRenderer.DrawPolygon")
        sys.exit(-1)
        
    def DrawText(self):
        sys.stderr.write("Unimplemented pure virtual IRenderer.DrawText")
        sys.exit(-1)


gRenderer = None

def GetRenderer():
    global gRenderer
    if gRenderer == None:
        gRenderer = RendererWx()
    return gRenderer
        
        
class RendererWx(IRenderer):
    def __init__(self):
        self.iTarget = None
        self.iDc = None
        self.iLock = threading.Lock()
        self.iBackgroundBrush = wx.Brush(wx.LIGHT_GREY)
        self.iBackgroundBrush.SetStipple(wx.Image("../share/Linn/Gui/Editor/Resources/Background.bmp", wx.BITMAP_TYPE_ANY).ConvertToBitmap())
        self.iBackgroundPen = wx.Pen(wx.Color(212, 208, 200), 1, wx.TRANSPARENT)
    
    def BeginFrame(self):
        self.iDc.Clear()
        self.SetDefaultFont()
        self.SetDefaultBackground()
        
    def EndFrame(self):
        self.iDc = None
    
    def SetRenderTarget(self, aTarget):
        self.iTarget = aTarget
        self.iTarget.Bind(wx.EVT_PAINT, self.OnPaint)
        
    def DefaultFont(self):
        return wx.Font(10, wx.FONTFAMILY_MODERN, wx.FONTSTYLE_NORMAL, wx.FONTWEIGHT_NORMAL,
                        False, 'Courier', wx.FONTENCODING_SYSTEM)
                        
    def SetDefaultFont(self):
        assert(self.iDc)
        self.iDc.SetFont(self.DefaultFont())
        
    def SetDefaultBackground(self):
        size = self.iTarget.GetSize()
        self.iDc.SetBrush(self.iBackgroundBrush)
        self.iDc.SetPen(self.iBackgroundPen)
        self.iDc.DrawRectangle(0, 0, size.GetWidth(), size.GetHeight())
        self.iDc.SetBrush(wx.NullBrush)
        self.iDc.SetPen(wx.NullPen)
        
    def SetDefaultFontColour(self):
        assert(self.iDc)
        self.iDc.SetTextForeground(wx.BLACK)
        
    def SetFont(self, aFont):
        assert(self.iDc)
        if aFont:
            self.iDc.SetFont(aFont)
        else:
            self.SetDefaultFont()
            
    def SetFontColour(self, aColour):
        assert(self.iDc)
        if aColour:
            self.iDc.SetTextForeground(aColour)
        else:
            self.SetDefaultFontColour()
            
    def DrawPolygon(self, aPolygon):
        assert(self.iDc)
        rs = aPolygon.RenderState()
        texture = rs.Texture()
        if texture and texture.Surface().Ok():
            memDc = wx.MemoryDC()
            image = texture.Surface()#.ConvertToImage()
            # scale image if required
            #memDc.SetUserScale(texture.Width() / float(aPolygon.iWidth),
            #                   texture.Height() / float(aPolygon.iHeight))
            if aPolygon.ScaleTexture():
                image.Rescale(aPolygon.iWidth, aPolygon.iHeight)
            surface = image.ConvertToBitmap()
            memDc.SelectObject(surface)
            
            pos = aPolygon.WorldSrt().Translation()
            width = texture.Width()
            height = texture.Height()
            self.iDc.Blit(pos.iX, pos.iY, aPolygon.iWidth, aPolygon.iHeight,
                          memDc, 0, 0, wx.COPY, True)
                          
    def DrawText(self, aText):
        assert(self.iDc)
        self.SetFont(aText.Font())
        self.SetFontColour(aText.FontColour())
        pos = aText.WorldSrt().Translation()
        self.iDc.DrawText(aText.Text(), pos.iX, pos.iY)
        
    def TextSize(self, aText):
        self.SetFont(aText.Font())
        return self.iDc.GetTextExtent(aText.Text())
    
    def Render(self):
        if self.iTarget:
            self.iTarget.Refresh(False)
    
    def OnPaint(self, event):
        #self.iLock.acquire()
        #t1 = time.time()
        if self.iTarget and Resources.Layout.gLayout:
            size = self.iTarget.GetSize()
            if size.GetWidth() > 0 and size.GetHeight() > 0:
                dc = wx.PaintDC(self.iTarget)
                self.iTarget.PrepareDC(dc)
                self.iDc = wx.BufferedDC(dc)
                self.iDc.SetBackground(wx.Brush(wx.Color(212, 208, 200)))
                self.BeginFrame()
                renderVisitor = SceneGraph.Visitor.RenderVisitor(self)
                #Resources.Layout.gLayout.Update()
                renderVisitor.Render(Resources.Layout.gLayout)
                self.EndFrame()
        #t2 = time.time()
        #print "rtime", t2 - t1
        #self.iLock.release()
        
                          
    

        