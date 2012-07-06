using System;
using System.Net;

// pretty
using Linn.ControlPoint;
using Linn.ControlPoint.Upnp;
using System.IO;

namespace Linn.ProductSupport
{

public class SysLogEntry {

    public const int kSysLogEntryBytes = 16;

    public SysLogEntry( byte[] aBinaryEntryData, int aOffset ) {
        Set( aBinaryEntryData, aOffset );
    }

    public void Set( byte[] aBinaryEntryData, int aOffset ) {

        Int32 key           = System.BitConverter.ToInt32( aBinaryEntryData, aOffset );
        Int32 payload       = System.BitConverter.ToInt32( aBinaryEntryData, aOffset + 4 );
        Int64 timestampUs   = System.BitConverter.ToInt64( aBinaryEntryData, aOffset + 8 );

        iKey            = (UInt32) IPAddress.NetworkToHostOrder( key );
        iPayload        = (UInt32) IPAddress.NetworkToHostOrder( payload );
        iTimestampUs    = (UInt64) IPAddress.NetworkToHostOrder( timestampUs );
    }

    public UInt32 Key           { get { return iKey; } }
    public UInt32 Payload       { get { return iPayload; } }
    public UInt64 TimestampUs   { get { return iTimestampUs; } }

    private UInt32 iKey;
    private UInt32 iPayload;
    private UInt64 iTimestampUs;
}

public class SysLog {

    public SysLog( byte[] aBinarySysLogData ) {
    
        iEntries = aBinarySysLogData.Length / SysLogEntry.kSysLogEntryBytes;
        iLog = new SysLogEntry[iEntries];

        for ( int i = 0 ; i < iEntries ; i++ ) {

            iLog[i] = new SysLogEntry( aBinarySysLogData, i * SysLogEntry.kSysLogEntryBytes );
        }
    }

    public int Entries { get { return iEntries; } }

    public SysLogEntry At( int aIndex ) {

        return iLog[aIndex];
    }

    private SysLogEntry[] iLog;
    private int iEntries;
}

public class SysLogPretty {
    public SysLogPretty(string aApplicationStartupPath) {
        iApplicationStartupPath = aApplicationStartupPath;
    }

    public void Refresh(Device aDevice, DSysLogComplete aSysLogCallback) {
        iSysLogCallback = aSysLogCallback;
        iServiceDiagnostics = new ServiceDiagnostics(aDevice);
        iActionSysLog = iServiceDiagnostics.CreateAsyncActionSysLog();
        iActionSysLog.EventResponse += SysLogResponse;
        iActionSysLog.SysLogBegin();
    }

    private void SysLogResponse(object obj, ServiceDiagnostics.AsyncActionSysLog.EventArgsResponse e) {
        iActionSysLog.EventResponse -= SysLogResponse;
        iSysLogCallback(GetSysLogPretty(e.aSysLog, iApplicationStartupPath));
    }

    private string GetSysLogPretty(byte[] aSysLogResponse, string aApplicationStartupPath) {
        string sysLogString = "";
        try {
            SysLog sys = new SysLog(aSysLogResponse);

            Tags tags = null;
            // try the default installer location
            string fullpath = Path.GetFullPath(Path.Combine(aApplicationStartupPath, "Tags.xml"));
            if (File.Exists(fullpath)) {
                tags = new Tags(File.ReadAllText(fullpath));
            }
            else {
                // try the default build location
                fullpath = Path.GetFullPath(Path.Combine(aApplicationStartupPath, "../../share/Linn/ProductSupport/Tags.xml"));
                if (File.Exists(fullpath)) {
                    tags = new Tags(File.ReadAllText(fullpath));
                }
                else {
                    return "System Log Error: Could not find the file 'Tags.xml'" + Environment.NewLine;
                }
            }

            int maxSize = 0;
            for (int i = 0; i < sys.Entries; i++) {
                SysLogEntry entry = sys.At(i);
                string var = tags.Tag(entry.Key);
                if (var.Length > maxSize) {
                    maxSize = var.Length;
                }
            }
            for (int i = 0; i < sys.Entries; i++) {
                SysLogEntry entry = sys.At(i);
                ulong left = entry.TimestampUs / 1000000;
                ulong right = entry.TimestampUs % 1000000;
                sysLogString += left.ToString().PadLeft(8) + "." + right.ToString().PadLeft(6, '0') + "s: " + tags.Tag(entry.Key).PadLeft(maxSize);
                sysLogString += " [0x" + entry.Payload.ToString("x").PadLeft(8, '0') + " " + entry.Payload + "]" + Environment.NewLine;
            }
        }
        catch (Exception e) {
            return "System Log Error: " + e.Message + Environment.NewLine;
        }

        return sysLogString;
    }

    public delegate void DSysLogComplete(string aSysLogPretty);

    private ServiceDiagnostics iServiceDiagnostics;
    private ServiceDiagnostics.AsyncActionSysLog iActionSysLog;
    private DSysLogComplete iSysLogCallback;
    private string iApplicationStartupPath;
}

} //namespace Linn
