using System;

namespace Linn {
namespace Kinsky {
    
public interface IControllerPreamp : IDisposable
{
    bool Standby{ set; }
}
    
} // Kinsky
} // Linn
