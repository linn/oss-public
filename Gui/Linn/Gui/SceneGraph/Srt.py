from Ft.Xml import XPath
import Linn.Gui.Serialize

class Vector3d(Linn.Gui.Serialize.ISerialize):
    def __init__(self, aX=0, aY=0, aZ=0):
        self.iX = aX
        self.iY = aY
        self.iZ = aZ
        
    def Load(self, aStream):
        x = aStream.LoadNode('X')
        if len(x) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<X>', len(x), 1)
        self.iX = int(x[0].firstChild.nodeValue)
        y = aStream.LoadNode('Y')
        if len(y) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Y>', len(y), 1)
        self.iY = int(y[0].firstChild.nodeValue)
        z = aStream.LoadNode('Z')
        if len(z) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Z>', len(z), 1)
        self.iZ = int(z[0].firstChild.nodeValue)
    
    def Save(self, aStream):
        elem = aStream.CreateElement('X')
        elem.appendChild(aStream.CreateTextNode(str(self.iX)))
        aStream.SaveNode(elem)
        elem = aStream.CreateElement('Y')
        elem.appendChild(aStream.CreateTextNode(str(self.iY)))
        aStream.SaveNode(elem)
        elem = aStream.CreateElement('Z')
        elem.appendChild(aStream.CreateTextNode(str(self.iZ)))
        aStream.SaveNode(elem)
        
    def __add__(self, aVec):
        return Vector3d(self.iX+aVec.iX, self.iY+aVec.iY, self.iZ+aVec.iZ)
    
    def __sub__(self, aVec):
        return Vector3d(self.iX-aVec.iX, self.iY-aVec.iY, self.iZ-aVec.iZ)
    
    def __eq__(self, aVec):
        return self.iX == aVec.iX and self.iY == aVec.iY and self.iZ and aVec.iZ
    
    def __str__(self):
        return '(%s, %s, %s)' % (self.iX, self.iY, self.iZ)
    
    
class Matrix(Linn.Gui.Serialize.ISerialize):
    def __init__(self):
        self.iMx = [[],[],[],[]]
        for i in range(4):
            for j in range(4):
                if i == j:
                    self.iMx[i].append(1.0)
                else:
                    self.iMx[i].append(0.0)
                    
    def __mul__(self, aMx):
        m = Matrix()

        m.iMx[0][0] = self.iMx[0][0] * aMx.iMx[0][0] + self.iMx[0][1] * aMx.iMx[1][0] + self.iMx[0][2] * aMx.iMx[2][0] + self.iMx[0][3] * aMx.iMx[3][0];
        m.iMx[0][1] = self.iMx[0][0] * aMx.iMx[0][1] + self.iMx[0][1] * aMx.iMx[1][1] + self.iMx[0][2] * aMx.iMx[2][1] + self.iMx[0][3] * aMx.iMx[3][1];
        m.iMx[0][2] = self.iMx[0][0] * aMx.iMx[0][2] + self.iMx[0][1] * aMx.iMx[1][2] + self.iMx[0][2] * aMx.iMx[2][2] + self.iMx[0][3] * aMx.iMx[3][2];
        m.iMx[0][3] = self.iMx[0][0] * aMx.iMx[0][3] + self.iMx[0][1] * aMx.iMx[1][3] + self.iMx[0][2] * aMx.iMx[2][3] + self.iMx[0][3] * aMx.iMx[3][3];

        m.iMx[1][0] = self.iMx[1][0] * aMx.iMx[0][0] + self.iMx[1][1] * aMx.iMx[1][0] + self.iMx[1][2] * aMx.iMx[2][0] + self.iMx[1][3] * aMx.iMx[3][0];
        m.iMx[1][1] = self.iMx[1][0] * aMx.iMx[0][1] + self.iMx[1][1] * aMx.iMx[1][1] + self.iMx[1][2] * aMx.iMx[2][1] + self.iMx[1][3] * aMx.iMx[3][1];
        m.iMx[1][2] = self.iMx[1][0] * aMx.iMx[0][2] + self.iMx[1][1] * aMx.iMx[1][2] + self.iMx[1][2] * aMx.iMx[2][2] + self.iMx[1][3] * aMx.iMx[3][2];
        m.iMx[1][3] = self.iMx[1][0] * aMx.iMx[0][3] + self.iMx[1][1] * aMx.iMx[1][3] + self.iMx[1][2] * aMx.iMx[2][3] + self.iMx[1][3] * aMx.iMx[3][3];

        m.iMx[2][0] = self.iMx[2][0] * aMx.iMx[0][0] + self.iMx[2][1] * aMx.iMx[1][0] + self.iMx[2][2] * aMx.iMx[2][0] + self.iMx[2][3] * aMx.iMx[3][0];
        m.iMx[2][1] = self.iMx[2][0] * aMx.iMx[0][1] + self.iMx[2][1] * aMx.iMx[1][1] + self.iMx[2][2] * aMx.iMx[2][1] + self.iMx[2][3] * aMx.iMx[3][1];
        m.iMx[2][2] = self.iMx[2][0] * aMx.iMx[0][2] + self.iMx[2][1] * aMx.iMx[1][2] + self.iMx[2][2] * aMx.iMx[2][2] + self.iMx[2][3] * aMx.iMx[3][2];
        m.iMx[2][3] = self.iMx[2][0] * aMx.iMx[0][3] + self.iMx[2][1] * aMx.iMx[1][3] + self.iMx[2][2] * aMx.iMx[2][3] + self.iMx[2][3] * aMx.iMx[3][3];

        m.iMx[3][0] = self.iMx[3][0] * aMx.iMx[0][0] + self.iMx[3][1] * aMx.iMx[1][0] + self.iMx[3][2] * aMx.iMx[2][0] + self.iMx[3][3] * aMx.iMx[3][0];
        m.iMx[3][1] = self.iMx[3][0] * aMx.iMx[0][1] + self.iMx[3][1] * aMx.iMx[1][1] + self.iMx[3][2] * aMx.iMx[2][1] + self.iMx[3][3] * aMx.iMx[3][1];
        m.iMx[3][2] = self.iMx[3][0] * aMx.iMx[0][2] + self.iMx[3][1] * aMx.iMx[1][2] + self.iMx[3][2] * aMx.iMx[2][2] + self.iMx[3][3] * aMx.iMx[3][2];
        m.iMx[3][3] = self.iMx[3][0] * aMx.iMx[0][3] + self.iMx[3][1] * aMx.iMx[1][3] + self.iMx[3][2] * aMx.iMx[2][3] + self.iMx[3][3] * aMx.iMx[3][3];

        return m
    
    
class Quaternion(Linn.Gui.Serialize.ISerialize):
    def __init__(self, aX=0, aY=0, aZ=0, aW=1):
        self.iX = aX
        self.iY = aY
        self.iZ = aZ
        self.iW = aW
        
    def Matrix(self):
        pass
    

class Srt(Linn.Gui.Serialize.ISerialize):
    def __init__(self, aScale=None, aRotation=None, aTranslation=None):
        if aScale:
            self.iScale = aScale
        else:
            self.iScale = Vector3d(1, 1, 1)
        #self.iRotation = aRotation
        if aTranslation:
            self.iTranslation = aTranslation
        else:
            self.iTranslation = Vector3d()

    def Load(self, aStream):
        scale = aStream.LoadNode('Scale')
        if len(scale) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Scale>', len(scale), 1)
        self.iScale = Vector3d()
        aStream.PushContextNode(scale[0])
        self.iScale.Load(aStream)
        aStream.PopContextNode()
        
        translation = aStream.LoadNode('Translation')
        if len(translation) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Translation>', len(translation), 1)
        self.iTranslation = Vector3d()
        aStream.PushContextNode(translation[0])
        self.iTranslation.Load(aStream)
        aStream.PopContextNode()
        #print self.iTranslation
    
    def Save(self, aStream):
        srt = aStream.CreateElement('Srt')
        aStream.PushContextNode(srt)
        
        scale = aStream.CreateElement('Scale')
        aStream.PushContextNode(scale)
        self.iScale.Save(aStream)
        aStream.PopContextNode()
        aStream.SaveNode(scale)
        
        translation = aStream.CreateElement('Translation')
        aStream.PushContextNode(translation)
        self.iTranslation.Save(aStream)
        aStream.PopContextNode()
        aStream.SaveNode(translation)
        aStream.PopContextNode()
        aStream.SaveNode(srt)
    
    def SetTranslation(self, aTranslation):
        self.iTranslation = aTranslation
        
    def Translation(self):
        return self.iTranslation
    
    def Scale(self):
        return self.iScale
    
    #def Rotation(self):
    #    return self.iRotation
        
        