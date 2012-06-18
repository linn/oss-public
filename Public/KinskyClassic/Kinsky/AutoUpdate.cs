using System;
using Linn;
using System.IO;
using System.Security;      // SecurityException
using System.Reflection;    // Assembly loading
using System.Net;           // WebClient
using System.Windows.Forms;
using System.Collections.Generic;

namespace Linn {

class AutoUpdate : IDisposable
{
    public AutoUpdate(string aBaseDirectory, string aUpdateLocation, string aUpdateFilename, int aUpdateInterval) {
        iBaseDirectory = Path.GetFullPath(aBaseDirectory);
        iUpdateLocation = aUpdateLocation;
        iUpdateFilename = aUpdateFilename;
        iUpdateInterval = aUpdateInterval;
        iUpdateTimer = new Timer();
        iUpdateTimer.AutoReset = false;
        iUpdateTimer.Interval = aUpdateInterval;
        iUpdateTimer.Elapsed += OnCheckForUpdate;
        iUpdateTimer.Start();
    }

    public void Dispose() {
        if(iUpdateTimer != null) {
            iUpdateTimer.Stop();
            iUpdateTimer = null;
        }
    }
    
    public string UpdateLocation {
        get {
            return iUpdateLocation;
        }
        set {
            iUpdateTimer.Stop();
            iUpdateLocation = value;
            iUpdateTimer.Start();
        }
    }
    
    public string UpdateFilename {
        get {
            return iUpdateFilename;
        }
        set {
            iUpdateTimer.Stop();
            iUpdateFilename = value;
            iUpdateTimer.Start();
        }
    }
    
    public int UpdateInterval {
        get {
            return iUpdateInterval;
        }
        set {
            iUpdateTimer.Stop();
            iUpdateTimer.Interval = value;
            iUpdateTimer.Start();
        }
    }

    private void OnCheckForUpdate(object aSender, EventArgs aArgs) {
        Trace.WriteLine(Trace.kLinnGui, ">AutoUpdate.OnCheckForUpdate");
        CleanupTemporaryFiles();        // do some housekeeping
        if(File.Exists(iUpdateFilename)) {
            ApplyUpdate();
        } else {
            DownloadUpdate();
            if(File.Exists(iUpdateFilename)) {
                ApplyUpdate();
            }
        }
        iUpdateTimer.Start();       // start the check again
    }

    private void DownloadUpdate() {
        int startingPoint = 0;
        string tempFile = iUpdateFilename + ".part";
        if (File.Exists(tempFile)) {
            startingPoint = (int)(new FileInfo(tempFile).Length);
        }

        try {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Path.Combine(iUpdateLocation, iUpdateFilename));
            request.AddRange(startingPoint);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseSteam = response.GetResponseStream();
            FileStream fileStream = null;
            if(startingPoint == 0) {
                fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            } else {
                fileStream = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            }

            int bytesSize;
            //long fileSize = response.ContentLength;
            byte[] downloadBuffer = new byte[kPacketLength];

            while((bytesSize = responseSteam.Read(downloadBuffer, 0, downloadBuffer.Length)) > 0) {
                fileStream.Write(downloadBuffer, 0, bytesSize);
            }

            if(fileStream != null) {
                fileStream.Close();
                fileStream.Dispose();
            }
            if(response != null) {
                response.Close();
            }
            File.Move(tempFile, iUpdateFilename);
        } catch(Exception e) {
            InvalidUpdateFile();
            System.Console.WriteLine(e);
        }
    }

    private void ApplyUpdate() {
        string updateText = "An application update is available, would you like to apply the update?";
        if(MessageBox.Show(updateText, "Update Available", MessageBoxButtons.YesNo) == DialogResult.Yes) {
            try {
                // Load the update assembly
                string filename = Path.GetFileNameWithoutExtension(iUpdateFilename);
                //filename = string.Format("{0}, PublicKeyToken={1}", filename, strongKey);
                Assembly updateAssembly = Assembly.Load(filename);
                ExtractFiles(updateAssembly);
            } catch(FileLoadException) {
                InvalidUpdateFile();
            } catch (FileNotFoundException) {
                InvalidUpdateFile();
            }
        }
    }

    private void ExtractFiles(Assembly aUpdateAssembly) {
        // Get resource names from update assembly
        string[] resources = aUpdateAssembly.GetManifestResourceNames();
        Dictionary<string, string> renameLog = new Dictionary<string, string>();
        try {
            foreach(string resource in resources) {
                // If a current file exists with the same name, rename it
                if(File.Exists(resource)) {
                    string tempName = CreateTemporaryFilename();
                    File.Move(resource, tempName);
                    renameLog[tempName] = resource;
                }
                // Copy the resource out into the new file
                // this does not take into consideration file dates and other similar
                // attributes (but probobly should).
                using(Stream res = aUpdateAssembly.GetManifestResourceStream(resource), file = new FileStream(resource, FileMode.CreateNew)) {
                    Int32 pseudoByte;
                    while((pseudoByte = res.ReadByte()) != -1) {
                        file.WriteByte((Byte)pseudoByte);
                    }
                }
            }
            // If we made it this far, it is safe to rename the update assembly
            MoveUpdateToTemporaryFile();
        } catch {
            // Unwind failed operation
            foreach(KeyValuePair<string, string> rename in renameLog) {
                string filename = rename.Value as string;
                if(File.Exists(filename)) {
                    File.Delete(filename);
                }
                File.Move(rename.Key as string, filename);
            }
            throw; // rethrow whatever went wrong
        }
    }

    private void InvalidUpdateFile() {
        MoveUpdateToTemporaryFile();
    }

    private void MoveUpdateToTemporaryFile() {
        string tempFile = CreateTemporaryFilename();
        try {
            File.Move(iUpdateFilename, tempFile);
        } catch(IOException e) {
            System.Console.WriteLine("Cannot move {0} to {1}", iUpdateFilename, tempFile);
            System.Console.WriteLine(e);
        }
    }

    private string CreateTemporaryFilename() {
        return Guid.NewGuid().ToString() + ".utmp";
    }

    private void CleanupTemporaryFiles() {
        string[] files = Directory.GetFiles(iBaseDirectory, "*.utmp");
        foreach(string file in files) {
            try {
                File.Delete(file);
            }
            catch(IOException) {}
            catch(UnauthorizedAccessException) {}
            catch(SecurityException) {}
        }
    }

    private string iBaseDirectory = "";
    private string iUpdateLocation = "";
    private string iUpdateFilename = "";
    private int iUpdateInterval = 0;
    private Timer iUpdateTimer = null;
    private static const int kPacketLength = 2048;
}

} // Linn

