import Linn.Gui.Serialize

class NoNamespace(Exception):
    def __init__(self, aFullname):
        self.iFullname = aFullname
        
    def __str__(self):
        return '%s has no namespace' % self.iFullname


# Base class for all GUI message types
class Msg(Linn.Gui.Serialize.ISerialize):
    def __init__(self, aFullname):
        self.iFullname = aFullname
        
    def Namespace(self):
        index = self.iFullname.find('.')
        if index:
            return self.iFullname[0:self.iFullname.find('.')]
        raise NoNamespace(self.iFullname)
        
    def Fullname(self):
        return self.iFullname
    
    def Load(self, aStream):
        fullname = aStream.LoadNode('Fullname')
        if len(fullname) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Fullname>', len(fullname), 1)
        self.iFullname = fullname[0].firstChild.nodeValue
        #print self.iFullname
    
    def Save(self, aStream):
        fullname = aStream.CreateElement('Fullname')
        fullname.appendChild(aStream.CreateTextNode(self.Fullname()))
        aStream.SaveNode(fullname)
        
    def Link(self):
        pass
    
    def Type(self):
        return 'Msg'
    
    def __eq__(self, aOther):
        if aOther.__class__ == self.__class__:
            if aOther.Fullname() == self.iFullname:
                return True
        return False
    
    def __ne__(self, aOther):
        return not self.__eq__(aOther)
    
    def __str__(self):
        return '\tFullname: %s\n' % self.iFullname

# Node SetActive message
class MsgSetActive(Msg):
    def __init__(self, aFullname='', aActive=False):
        Msg.__init__(self, aFullname)
        self.iActive = aActive
    
    def Active(self):
        return self.iActive
    
    def Load(self, aStream):
        Msg.Load(self, aStream)
        
        active = aStream.LoadNode('Active')
        if len(active) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Active>', len(active), 1)
        self.iActive = active[0].firstChild.nodeValue.lower()
        if(self.iActive == '1' or self.iActive == 'true'):
            self.iActive = True
        else:
            self.iActive = False
        #print self.iActive
    
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'MsgSetActive')
            
        Msg.Save(self, aStream)
            
        active = aStream.CreateElement('Active')
        active.appendChild(aStream.CreateTextNode(str(self.Active()).lower()))
        aStream.SaveNode(active)
        
    def Type(self):
        return 'MsgSetActive'
        
    def __eq__(self, aOther):
        if Msg.__eq__(self, aOther):
            if aOther.Active() == self.iActive:
                return True
        return False
    
    def __str__(self):
        result  = 'MsgSetActive:\n'
        result += Msg.__str__(self)
        result += '\tActive: %s\n' % self.iActive
        return result
    
# NodeDiscovery DeviceDiscovered message
class MsgDeviceDiscovered(Msg):
    def __init__(self, aFullname='', aDevice=None):
        Msg.__init__(self, aFullname)
        self.iDevice = aDevice
    
    def Device(self):
        return self.iDevice
    
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'MsgDeviceDiscovered')
        
        Msg.Save(self, aStream)
        
    def Type(self):
        return 'MsgDeviceDiscovered'

    def __eq__(self, aOther):
        if Msg.__eq__(self, aOther):
            if aOther.Device() == self.iDevice:
                return True
        return False
    
    def __str__(self):
        result  = 'MsgDeviceDiscovered:\n'
        result += Msg.__str__(self)
        result += self.iDevice.__str__()
        return result

# NodeDiscovery DeviceRemoved message    
class MsgDeviceRemoved(Msg):
    def __init__(self, aFullname='', aDevice=None):
        Msg.__init__(self, aFullname)
        self.iDevice = aDevice
    
    def Device(self):
        return self.iDevice
    
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'MsgDeviceRemoved')
        
        Msg.Save(self, aStream)

    def Type(self):
        return 'MsgDeviceRemoved'

    def __eq__(self, aOther):
        if Msg.__eq__(self, aOther):
            if aOther.Device() == self.iDevice:
                return True
        return False
    
    def __str__(self):
        result  = 'MsgDeviceRemoved:\n'
        result += Msg.__str__(self)
        result += self.iDevice.__str__()
        return result
    
# NodeText SetText message
class MsgSetText(Msg):
    def __init__(self, aFullname='', aText=''):
        Msg.__init__(self, aFullname)
        self.iText = aText
    
    def Text(self):
        return self.iText
    
    def Load(self, aStream):
        Msg.Load(self, aStream)
        
        text = aStream.LoadNode('Text')
        if len(text) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<Text>', len(text), 1)
        self.iText = text[0].firstChild.nodeValue
        #print self.iText
    
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'MsgSetText')
            
        Msg.Save(self, aStream)
            
        text = aStream.CreateElement('Text')
        text.appendChild(aStream.CreateTextNode(self.Text()))
        aStream.SaveNode(text)
        
    def Type(self):
        return 'MsgSetText'

    def __eq__(self, aOther):
        if Msg.__eq__(self, aOther):
            if aOther.Text() == self.iText:
                return True
        return False
    
    def __str__(self):
        result  = 'MsgSetText:\n'
        result += Msg.__str__(self)
        result += '\tText: %s\n' % self.iText
        return result
    
# Monostable/Bistable StateChanged message
class MsgStateChanged(Msg):
    def __init__(self, aFullname='', aOldValue=False, aNewValue=False):
        Msg.__init__(self, aFullname)
        self.iOldValue = aOldValue
        self.iNewValue = aNewValue
    
    def OldValue(self):
        return self.iOldValue
    
    def NewValue(self):
        return self.iNewValue
    
    def Load(self, aStream):
        Msg.Load(self, aStream)
        
        oldValue = aStream.LoadNode('OldState')
        if len(oldValue) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<OldState>', len(oldValue), 1)
        self.iOldValue = oldValue[0].firstChild.nodeValue.lower()
        if(self.iOldValue == '1' or self.iOldValue == 'true'):
            self.iOldValue = True
        else:
            self.iOldValue = False
        #print self.iOldValue
        
        newValue = aStream.LoadNode('NewState')
        if len(newValue) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<NewState>', len(newValue), 1)
        self.iNewValue = newValue[0].firstChild.nodeValue.lower()
        if(self.iNewValue == '1' or self.iNewValue == 'true'):
            self.iNewValue = True
        else:
            self.iNewValue = False
        #print self.iNewValue
    
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'MsgStateChanged')
            
        Msg.Save(self, aStream)
            
        oldValue = aStream.CreateElement('OldState')
        oldValue.appendChild(aStream.CreateTextNode(str(self.OldValue()).lower()))
        aStream.SaveNode(oldValue)
        
        newValue = aStream.CreateElement('NewState')
        newValue.appendChild(aStream.CreateTextNode(str(self.NewValue()).lower()))
        aStream.SaveNode(newValue)

    def Type(self):
        return 'MsgStateChanged'
        
    def __eq__(self, aOther):
        if Msg.__eq__(self, aOther):
            if aOther.NewValue() == self.iNewValue:
                if aOther.OldValue() == self.iOldValue:
                    return True
        return False
    
    def __str__(self):
        result  = 'MsgStateChanged:\n'
        result += Msg.__str__(self)
        result += '\tOldValue: %s\n' % self.iOldValue
        result += '\tNewValue: %s\n' % self.iNewValue
        return result

# Bistable SetState message
class MsgSetState(Msg):
    def __init__(self, aFullname='', aValue=False):
        Msg.__init__(self, aFullname)
        self.iValue = aValue
    
    def Value(self):
        return self.iValue
    
    def Load(self, aStream):
        Msg.Load(self, aStream)
        
        value = aStream.LoadNode('State')
        if len(value) != 1:
            raise Linn.Gui.Serialize.InvalidLayoutFile('<State>', len(value), 1)
        self.iValue = value[0].firstChild.nodeValue.lower()
        if(self.iValue == '1' or self.iValue == 'true'):
            self.iValue = True
        else:
            self.iValue = False
        #print self.iValue
    
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'MsgSetState')
            
        Msg.Save(self, aStream)
            
        value = aStream.CreateElement('State')
        value.appendChild(aStream.CreateTextNode(str(self.Value()).lower()))
        aStream.SaveNode(value)

    def Type(self):
        return 'MsgSetState'
        
    def __eq__(self, aOther):
        if Msg.__eq__(self, aOther):
            if aOther.Value() == self.iValue:
                return True
        return False
    
    def __str__(self):
        result  = 'MsgSetState:\n'
        result += Msg.__str__(self)
        result += '\tValue: %s\n' % self.iValue
        return result
    
# Bistable ToggleState message    
class MsgToggleState(Msg):
    def __init__(self, aFullname=''):
        Msg.__init__(self, aFullname)
        
    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'MsgToggleState')
            
        Msg.Save(self, aStream)

    def Type(self):
        return 'MsgToggleState'
        
    def __str__(self):
        result  = 'MsgToggleState:\n'
        result += Msg.__str__(self)
        return result
    
# Counter count start message
class MsgCountStart(Msg):
    def __init__(self, aFullname=''):
        Msg.__init__(self, aFullname)

    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'MsgCountStart')
            
        Msg.Save(self, aStream)

    def Type(self):
        return 'MsgCountStart'
        
    def __str__(self):
        result  = 'MsgCountStart:\n'
        result += Msg.__str__(self)
        return result

# Counter count stop message
class MsgCountStop(Msg):
    def __init__(self, aFullname=''):
        Msg.__init__(self, aFullname)

    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'MsgCountEnd')
            
        Msg.Save(self, aStream)

    def Type(self):
        return 'MsgCountEnd'
        
    def __str__(self):
        result  = 'MsgCountEnd:\n'
        result += Msg.__str__(self)
        return result

# Counter count end message
class MsgCountEnd(Msg):
    def __init__(self, aFullname=''):
        Msg.__init__(self, aFullname)

    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'MsgCountEnd')
            
        Msg.Save(self, aStream)

    def Type(self):
        return 'MsgCountEnd'
        
    def __str__(self):
        result  = 'MsgCountEnd:\n'
        result += Msg.__str__(self)
        return result
        
class MsgInputBase(Msg):
    def __init__(self, aFullname='', aSpeed = 0):
        Msg.__init__(self, aFullname)
        self.iSpeed = aSpeed
        
    def Speed(self):
        return self.iSpeed

class MsgInputClockwise(MsgInputBase):
    def __init__(self, aFullname='', aSpeed = 0):
        MsgInputBase.__init__(self, aFullname, aSpeed)

    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'MsgInputClockwise')
            
        Msg.Save(self, aStream)

    def Type(self):
        return 'MsgInputClockwise'
        
    def __str__(self):
        result  = 'MsgInputClockwise:\n'
        result += Msg.__str__(self)
        return result

class MsgInputAntiClockwise(MsgInputBase):
    def __init__(self, aFullname='', aSpeed = 0):
        MsgInputBase.__init__(self, aFullname, aSpeed)

    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'MsgInputAntiClockwise')
            
        Msg.Save(self, aStream)

    def Type(self):
        return 'MsgInputAntiClockwise'
        
    def __str__(self):
        result  = 'MsgInputAntiClockwise:\n'
        result += Msg.__str__(self)
        return result

class MsgInputForwards(MsgInputBase):
    def __init__(self, aFullname='', aSpeed = 0):
        MsgInputBase.__init__(self, aFullname, aSpeed)

    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'MsgInputForwards')
            
        Msg.Save(self, aStream)

    def Type(self):
        return 'MsgInputForwards'
        
    def __str__(self):
        result  = 'MsgInputForwards:\n'
        result += Msg.__str__(self)
        return result
        
class MsgInputBackwards(MsgInputBase):
    def __init__(self, aFullname='', aSpeed = 0):
        MsgInputBase.__init__(self, aFullname, aSpeed)

    def Save(self, aStream):
        if aStream.ContextAttribute('class') == None:
            aStream.SetContextAttribute('class', 'MsgInputBackwards')
            
        Msg.Save(self, aStream)

    def Type(self):
        return 'MsgInputBackwards'
        
    def __str__(self):
        result  = 'MsgInputBackwards:\n'
        result += Msg.__str__(self)
        return result
    
class UnknownClassType(Exception):
    def __init__(self, aClassType):
        self.iClassType = aClassType
    def __str__(self):
        return 'Unknown class of type %s' % self.iClassType

def CreateMessage(aClassType, aStream=None):
    """Using the message XML create a message of correct type
       and initialise it with saved XML data."""
    #print 'Creating class of type', aClassType
    newClass = None
    if aClassType == 'MsgSetActive':
        newClass = MsgSetActive()
    elif aClassType == 'MsgDeviceDiscovered':
        newClass = MsgDeviceDiscovered()
    elif aClassType == 'MsgDeviceRemoved':
        newClass = MsgDeviceRemoved()
    elif aClassType == 'MsgSetText':
        newClass = MsgSetText()
    elif aClassType == 'MsgStateChanged':
        newClass = MsgStateChanged()
    elif aClassType == 'MsgSetState':
        newClass = MsgSetState()
    elif aClassType == 'MsgToggleState':
        newClass = MsgToggleState()
    elif aClassType == 'MsgCountStart':
        newClass = MsgCountStart()
    elif aClassType == 'MsgCountStop':
        newClass = MsgCountStop()
    elif aClassType == 'MsgCountEnd':
        newClass = MsgCountEnd()
    elif aClassType == 'MsgInputClockwise':
        newClass = MsgInputClockwise()
    elif aClassType == 'MsgInputAntiClockwise':
        newClass = MsgInputAntiClockwise()
    elif aClassType == 'MsgInputForwards':
        newClass = MsgInputForwards()
    elif aClassType == 'MsgInputBackwards':
        newClass = MsgInputBackwards()
    if newClass:
        if aStream:
            newClass.Load(aStream)
        return newClass
    raise UnknownClassType(aClassType)

        
        