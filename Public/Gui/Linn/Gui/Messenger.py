import wx
import sys
import threading
import Resources.Layout

gMessenger = None

def GetMessenger():
    global gMessenger
    if not gMessenger:
        gMessenger = Messenger()
    return gMessenger

class IMessageReceiver:
    def Receive(self, aMessage):
        """Interface for receiving GUI messages."""
        sys.stderr.write("Unimplemented pure virtual IMessageReceiver.Receive")
        sys.exit(-1)

class Messenger:
    def __init__(self):
        self.iCallback = None
        
    def SetCallback(self, aCallback):
        self.iCallback = aCallback
    
    def ApplicationMessage(self, aMessage, aTimeDelay=0):
        if self.iCallback:
            self.iCallback.Receive(aMessage)
    
    def PresentationMessage(self, aMessage, aTimeDelay=0):
        """Starts the message propagation through the scene graph."""
        if Resources.Layout.gLayout:
            Resources.Layout.gLayout.ReceiveMessage(aMessage)
        
        
        