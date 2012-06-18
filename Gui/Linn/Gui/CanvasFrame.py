#Boa:Frame:CanvasFrame

import wx
import Linn.Path
import Linn.Gui.Renderer
import Linn.Gui.Messenger
import Linn.Gui.SceneGraph.Srt
import Linn.Gui.SceneGraph.Visitor
import Linn.Gui.Resources.Layout
import Linn.Gui.Resources.Configuration

[wxID_CANVASFRAME, wxID_CANVAS
] = [wx.NewId() for _init_ctrls in range(2)]

class CanvasFrame(wx.Frame):
    def _init_ctrls(self, aParent, aTitle):
        # generated method, don't edit
        width = Linn.Gui.Resources.Layout.gLayout.Width()
        height = Linn.Gui.Resources.Layout.gLayout.Height()
        wx.Frame.__init__(self, id=wxID_CANVASFRAME, name='', parent=aParent,
              pos=wx.DefaultPosition, size=wx.Size(width, height),
              style=wx.SYSTEM_MENU | wx.MINIMIZE_BOX | wx.CAPTION | wx.CLOSE_BOX | wx.CLIP_CHILDREN,
              title=aTitle)
        self.SetClientSize(wx.Size(width, height))
        self.SetMaxSize(wx.Size(width, height))
        self.SetMinSize(wx.Size(width, height))
        self.SetIcon(wx.Icon('../share/Linn/Gui/Linn.ico', wx.BITMAP_TYPE_ICO))
        self.Bind(wx.EVT_CLOSE, self.OnClose)
        
        self.Canvas = wx.Panel(id=wxID_CANVAS,
              name=u'Canvas', parent=self, pos=wx.Point(-1, -1),
              size=wx.Size(width, height), style=0)
        self.Canvas.SetMinSize(wx.Size(-1, -1))
        #self.Canvas.SetBackgroundColour(wx.Colour(0, 0, 0))
        self.Canvas.Bind(wx.EVT_LEFT_UP, self.OnLeftUp)
        self.Canvas.Bind(wx.EVT_LEFT_DOWN, self.OnLeftDown)
        self.Canvas.Bind(wx.EVT_MOTION, self.OnMotion)
        self.Canvas.Bind(wx.EVT_RIGHT_UP, self.OnRightUp)
        self.Canvas.Bind(wx.EVT_RIGHT_DOWN, self.OnRightDown)
        self.Canvas.Bind(wx.EVT_KEY_DOWN, self.OnKeyDown)
        self.Canvas.Bind(wx.EVT_KEY_UP, self.OnKeyUp)
        self.Canvas.Bind(wx.EVT_CHAR, self.OnChar)
        self.Canvas.Bind(wx.EVT_KILL_FOCUS, self.OnKillFocus)
        
        
    def __init__(self, aParent, aTitle, aLayout):
        # create the gui layout for rendering
        layout = Linn.Gui.Resources.Layout.Layout(Linn.Gui.Resources.Configuration.ProjectPath()+'/'+aLayout)
        Linn.Gui.Resources.Layout.gLayout = layout
        
        self._init_ctrls(aParent, aTitle)
        
        # set the render target
        self.iRenderer = Linn.Gui.Renderer.GetRenderer()
        self.iRenderer.SetRenderTarget(self.Canvas)
        self.iLeftMbDown = False
        self.iClickNode = None
        
    def OnClose(self, event):
        event.Skip()
        
    def OnLeftUp(self, event):
        self.iLeftMbDown = False
        if self.iClickNode:
            self.iClickNode.UnHit()
            self.iClickNode = None
            Linn.Gui.Renderer.GetRenderer().Render()

    def OnLeftDown(self, event):
        self.iLeftMbDown = True
        pos = Linn.Gui.SceneGraph.Srt.Vector3d(event.GetPosition().x, event.GetPosition().y, 0)
        node = self.FindNode(pos)
        self.iClickNode = node
        if node:
            node.Hit(pos)
            Linn.Gui.Renderer.GetRenderer().Render()
        event.Skip()
        
    def OnMotion(self, event):
        if self.iClickNode:
            pos = Linn.Gui.SceneGraph.Srt.Vector3d(event.GetPosition().x, event.GetPosition().y, 0)
            self.iClickNode.Motion(pos)
            Linn.Gui.Renderer.GetRenderer().Render()
        event.Skip()
        
    def OnRightUp(self, event):
        event.Skip()
        
    def OnRightDown(self, event):
        event.Skip()
        
    def OnKeyUp(self, event):
        event.Skip()
        
    def OnKeyDown(self, event):
        event.Skip()
        
    def OnChar(self, event):
        event.Skip()
        
    def OnKillFocus(self, event):
        if self.iClickNode:
            self.iClickNode.UnHit()
            self.iClickNode = None
            Linn.Gui.Renderer.GetRenderer().Render()
        event.Skip()
        
    def FindNode(self, aPosition):
        hitVisitor = Linn.Gui.SceneGraph.Visitor.HitProxyVisitor(aPosition)
        return hitVisitor.Hit(Linn.Gui.Resources.Layout.gLayout)


    
    
    
    
    