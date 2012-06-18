import time

class Timer:
    def __init__(self):
        self.iTime = 0
        self.Reset()
        
    def Reset(self):
        self.iTime = time.time()
        
    def Seconds(self):
        return ( time.time() - self.iTime )