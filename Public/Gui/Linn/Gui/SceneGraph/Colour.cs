using System;

namespace Linn {
namespace Gui {

public sealed class Colour
{
    public Colour(int aAlpha, int aRed, int aGreen, int aBlue) {
        iAlpha = (byte)aAlpha;
        iRed = (byte)aRed;
        iGreen = (byte)aGreen;
        iBlue = (byte)aBlue;
    }
    
    public byte A {
        get {
            return iAlpha;
        }
    }
    
    public byte R {
        get {
            return iRed;
        }
    }
    
    public byte G {
        get {
            return iGreen;
        }
    }
    
    public byte B {
        get {
            return iBlue;
        }
    }
    
    public override bool Equals(Object obj) {
        return((obj is Colour) && (this == (Colour)obj));
    }
    
    public static bool operator ==(Colour lhs, Colour rhs) {
        return((lhs.A == rhs.A) && (lhs.R == rhs.R) && (lhs.G == rhs.G) && (lhs.B == rhs.B));
    }
    
    public static bool operator !=(Colour lhs, Colour rhs) {
        return((lhs.A != rhs.A) || (lhs.R != rhs.R) || (lhs.G != rhs.G) || (lhs.B != rhs.B));
    }
    
    public override int GetHashCode() {
        return base.GetHashCode();
    }
    
    private byte iAlpha = 255;
    private byte iRed = 0;
    private byte iGreen = 0;
    private byte iBlue = 0;
}

} // Gui
} // Linn
