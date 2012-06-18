import sys

class IUpdateObject:
    """Interface for updating."""
    def Tick(self, aDeltaTime):
        sys.stderr.write("Unimplemented pure virtual ObjectUpdate.Tick")
        sys.exit(-1)
        
        