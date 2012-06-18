import sys

class InvalidLayoutFile(Exception):
    def __init__(self, aTag, aNum, aReq):
        self.iTag = aTag
        self.iNum = aNum
        self.iReq = aReq
    def __str__(self):
        return ('Error in layout file: class has %d %s nodes (requires %d)' % (self.iNum, self.iTag, self.iReq))

class ISerialize:
    def Link(self):
        sys.stderr.write("Unimplemented pure virtual ISerialize.Link")
        sys.exit(-1)
        
    def Load(self, aStream):
        sys.stderr.write("Unimplemented pure virtual ISerialize.Load")
        sys.exit(-1)
        
    def Save(self, aStream):
        sys.stderr.write("Unimplemented pure virtual ISerialize.Save")
        sys.exit(-1)
        

        
        