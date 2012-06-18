using System;
using System.Net;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Linn
{
	
    //NetworkInformation class can only return IPv4 addresses as it uses the GetAdaptersInfo Win32 function
	public class NetworkInfo
	{
		[DllImport("iphlpapi.dll")]
        private static extern int GetAdaptersInfo(IntPtr pAdapterInfo, ref Int64 pBufOutLen);
        
        [DllImport("coredll.dll", EntryPoint="LocalAlloc")]
        private static extern IntPtr CFLocalAlloc(uint flags, UIntPtr byteCount);
		
		[DllImport("coredll.dll", SetLastError=true, EntryPoint="LocalFree")]
		private static extern IntPtr CFLocalFree(IntPtr hMem);
		
		[DllImport("coredll.dll", EntryPoint="LocalReAlloc")]
		private static extern IntPtr CFLocalReAlloc(IntPtr hMem, UIntPtr uBytes, uint uFlags);
		
		[DllImport("kernel32.dll", EntryPoint="LocalAlloc")]
		private static extern IntPtr XPLocalAlloc(uint flags, UIntPtr byteCount);
		
		[DllImport("kernel32.dll", SetLastError=true, EntryPoint="LocalFree")]
		private static extern IntPtr XPLocalFree(IntPtr hMem);
		
		[DllImport("kernel32.dll", EntryPoint="LocalReAlloc")]
		private static extern IntPtr XPLocalReAlloc(IntPtr hMem, UIntPtr uBytes, uint uFlags);
		
		private const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
        private const int ERROR_BUFFER_OVERFLOW = 111;
        private const int MAX_ADAPTER_NAME_LENGTH = 256;
        private const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
        private const int MIB_IF_TYPE_OTHER = 1;
        private const int MIB_IF_TYPE_ETHERNET = 6;
        private const int MIB_IF_TYPE_TOKENRING = 9;
        private const int MIB_IF_TYPE_FDDI = 15;
        private const int MIB_IF_TYPE_PPP = 23;
        private const int MIB_IF_TYPE_LOOPBACK = 24;
        private const int MIB_IF_TYPE_SLIP = 28;

        [StructLayout(LayoutKind.Sequential)]
        private struct IP_ADDRESS_STRING
        {            
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            internal byte[] Address;            
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IP_ADDR_STRING
        {
            internal IntPtr Next;
            internal IP_ADDRESS_STRING IpAddress;
            internal IP_ADDRESS_STRING IpMask;
            internal Int32 Context;
        }
        
        [Flags]
		private enum LocalMemoryFlags {
    		LMEM_FIXED = 0x0000,
		    LMEM_MOVEABLE = 0x0002,
		    LMEM_NOCOMPACT = 0x0010,
		    LMEM_NODISCARD = 0x0020,
		    LMEM_ZEROINIT = 0x0040,
		    LMEM_MODIFY = 0x0080,
		    LMEM_DISCARDABLE = 0x0F00,
		    LMEM_VALID_FLAGS = 0x0F72,
		    LMEM_INVALID_HANDLE = 0x8000,
		    LHND = (LMEM_MOVEABLE | LMEM_ZEROINIT),
		    LPTR = (LMEM_FIXED | LMEM_ZEROINIT),
		    NONZEROLHND = (LMEM_MOVEABLE),
		    NONZEROLPTR = (LMEM_FIXED)
		}

        public static bool GetIsNetworkAvailable()
        {
            return true;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IP_ADAPTER_INFO
        {
            internal IntPtr Next;
            internal Int32 ComboIndex;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_NAME_LENGTH + 4)]
            internal byte[] AdapterName;    
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_DESCRIPTION_LENGTH + 4)]
            internal byte[] AdapterDescription;
            internal UInt32 AddressLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
            internal byte[] Address;
            internal Int32 Index;
            internal UInt32 Type;
            internal UInt32 DhcpEnabled;
            internal IntPtr CurrentIpAddress;
            internal IP_ADDR_STRING IpAddressList;
            internal IP_ADDR_STRING GatewayList;
            internal IP_ADDR_STRING DhcpServer;
            internal bool HaveWins;
            internal IP_ADDR_STRING PrimaryWinsServer;
            internal IP_ADDR_STRING SecondaryWinsServer;
            internal Int32 LeaseObtained;
            internal Int32 LeaseExpires;
        }

        public static List<NetworkInfoModel> GetAllNetworkInterfaces()
        {
            //byte array must be converted to ascii
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            List<NetworkInfoModel> adaptors = new List<NetworkInfoModel>();

            int structSize = Marshal.SizeOf(typeof(IP_ADAPTER_INFO));

            IntPtr pArray;
            //This code should run on the compact framework as well as desktops. compact framework does not have Marshal.ReAllocHGlobal
            if (Environment.OSVersion.Platform == PlatformID.WinCE)
            {
                pArray = CFLocalAlloc((uint)LocalMemoryFlags.LPTR, new UIntPtr((uint)structSize));
            }
            else
            {
                pArray = XPLocalAlloc((uint)LocalMemoryFlags.LPTR, new UIntPtr((uint)structSize));
            }

            long longStruct = 0;
            int ret = GetAdaptersInfo(pArray, ref longStruct);

            // Buffer was too small, reallocate the correct size for the buffer
            if (ret == ERROR_BUFFER_OVERFLOW) // ERROR_BUFFER_OVERFLOW == 111
            {
                if (Environment.OSVersion.Platform == PlatformID.WinCE)
                {
                    pArray = CFLocalReAlloc(pArray, new UIntPtr((uint)longStruct), 2);
                }
                else
                {
                    pArray = XPLocalReAlloc(pArray, new UIntPtr((uint)longStruct), 2);
                }

                ret = GetAdaptersInfo(pArray, ref longStruct);
            }

            if (ret == 0)
            {
                // Call Succeeded
                IntPtr pEntry = pArray;

                do
                {
                    string macAddress;
                    ENetworkInterfaceType networkInterfaceType;
                    string adapterName;
                    string adapterDescription;
                    IPAddress ipAddress = null;

                    // Retrieve the adapter info from the memory address
                    IP_ADAPTER_INFO entry = (IP_ADAPTER_INFO)Marshal.PtrToStructure(pEntry, typeof(IP_ADAPTER_INFO));

                    switch (entry.Type)
                    {
                        case MIB_IF_TYPE_ETHERNET: networkInterfaceType = ENetworkInterfaceType.eEthernet; break;
                        case MIB_IF_TYPE_TOKENRING: networkInterfaceType = ENetworkInterfaceType.eTokenRing; break;
                        case MIB_IF_TYPE_FDDI: networkInterfaceType = ENetworkInterfaceType.eFddi; break;
                        case MIB_IF_TYPE_PPP: networkInterfaceType = ENetworkInterfaceType.ePpp; break;
                        case MIB_IF_TYPE_LOOPBACK: networkInterfaceType = ENetworkInterfaceType.eLoopBack; break;
                        case MIB_IF_TYPE_SLIP: networkInterfaceType = ENetworkInterfaceType.eSlip; break;
                        default: networkInterfaceType = ENetworkInterfaceType.eUnknown; break;
                    }

                    //fixed byte array size will be pattded with ascii 0 null char
                    char nullChar = Convert.ToChar(0);

                    adapterName = enc.GetString(entry.AdapterName, 0, entry.AdapterName.Length);
                    adapterName = adapterName.TrimEnd(new char[] { nullChar });
                    adapterDescription = enc.GetString(entry.AdapterDescription, 0, entry.AdapterDescription.Length);
                    adapterDescription = adapterDescription.TrimEnd(new char[] { nullChar });
                    ipAddress = GetIPAddress(entry.IpAddressList.IpAddress.Address);

                    // MAC Address (data is in a byte[])
                    string tmpString = "";
                    for (int i = 0; i < entry.AddressLength - 1; i++)
                    {
                        tmpString += string.Format("{0:X2}-", entry.Address[i]);
                    }
                    macAddress = string.Format("{0}{1:X2}", tmpString, entry.Address[entry.AddressLength - 1]);

                    NetworkInfoModel adaptor = new NetworkInfoModel(adapterDescription
                        , adapterName
                        , networkInterfaceType
                        , ipAddress
                        , true
                        , ENetworkInterfaceComponent.eIPv4
                        , macAddress
                        , EOperationalStatus.eUnknown
                        , null
                        , null
                        , null
                        , null
                        , null
                        , null
                        , null
                        , new List<IPAddress>());

                    adaptors.Add(adaptor);

                    // Get next adapter (if any)
                    pEntry = entry.Next;

                    Trace.WriteLine(Trace.kCore, "Network Adapter found -   " + adapterName + " " + adapterDescription);
                }
                while (pEntry.ToInt32() != 0);

                if (Environment.OSVersion.Platform == PlatformID.WinCE)
                {
                    CFLocalFree(pArray);
                }
                else
                {
                    XPLocalFree(pArray);
                }
            }
            else
            {
                if (Environment.OSVersion.Platform == PlatformID.WinCE)
                {
                    CFLocalFree(pArray);
                }
                else
                {
                    XPLocalFree(pArray);
                }

                throw new InvalidOperationException("GetAdaptersInfo failed: " + ret);
            }

            return adaptors;
        }
        
        private static IPAddress GetIPAddress(byte[] aIPAddress) {
        	
        	//fixed byte array size will be pattded with ascii 0 null char
        	System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            char nullChar = Convert.ToChar(0);
        	
        	string ipAddress = enc.GetString(aIPAddress, 0, aIPAddress.Length);
        	ipAddress = ipAddress.TrimEnd(new char[] {nullChar});
        	return IPAddress.Parse(ipAddress);
        }			
	}
}
