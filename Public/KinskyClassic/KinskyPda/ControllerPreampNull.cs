using System;

namespace Linn {
namespace KinskyPda {

public class ControllerPreampNull : IDisposable
{
    public ControllerPreampNull() {
        iViewPreamp = new ViewPreampPhantom();
    }
    
    public void Dispose() {}
    
    private ViewPreampPhantom iViewPreamp = null;
}

} // KinskyPda
} // Linn
