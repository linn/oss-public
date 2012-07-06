using System;
using System.IO;

namespace Linn.ProductSupport.Flash
{
    public class FuseMap {

        // public exceptions

        public class LoadFailure : System.Exception {
            public LoadFailure() : base("Could not parse the input file.") {}
        }

        // private member vars

        private bool[] iFuseMap = null;

        // ctor
        
        public FuseMap(string aFilename) {

            LoadJed(aFilename);
        }
        
        public byte[] AsBinary()
        {
            byte[] data = new byte[iFuseMap.Length/8];
            uint byteCount = 0;
            
            for ( uint idx = 0 ; idx < iFuseMap.Length ; idx += 8 ) {

                byte b = 0x00;
                for ( int i = 0 ; i < 8 ; i++ )
                    if ( iFuseMap[idx+i] )
                        b |= (byte)(0x1 << (7-i));
                
                data[byteCount++] = b;
            }
        
            return data;
        }
        
        // private load functions
        
        private void LoadJed(string aFilename) {
            
            iFuseMap = null;
            
            TextReader jedFile = new StreamReader(aFilename);
            string s = jedFile.ReadLine();
            uint bitCount = 0;
            
            while ( s != null ) {
                
                if ( s.Contains("QF") ) { // as xilinx tools seem to add 0x02 (STX) at front of this line... 
                    
                    s = s.Remove(s.IndexOf("*"));
                    s = s.Substring(s.IndexOf("QF")+2);
                    bitCount = UInt32.Parse(s);
                    
                    iFuseMap = new bool[bitCount];
                }
                else if ( s.StartsWith("L") ) {
                    
                    string header, bits;
                    
                    header = s.Substring(1, s.IndexOf(" ")-1);
                    bits = s.Substring(s.IndexOf(" ")+1);
                    bits = bits.Remove(bits.IndexOf("*"));
                    
                    uint bitIdx = UInt32.Parse(header);
                    
                    foreach( char c in bits ) {
                        
                        if ( c == '0' ) {
                            iFuseMap[bitIdx++] = false;
                        }
                        else if ( c == '1' ) {
                            iFuseMap[bitIdx++] = true;
                        }
                    }
                }
                
                s = jedFile.ReadLine();
            }
        }
    }
}
