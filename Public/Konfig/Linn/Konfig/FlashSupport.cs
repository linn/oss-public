using System;
using System.Net;
using System.IO;

using ICSharpCode.SharpZipLib.Zip;

using Linn.ProductSupport.Flash;

namespace Linn.Konfig
{
    public class FlashSupport
    {
        public class Reprogrammer : IConsole
        {
            public Reprogrammer(IPAddress aAdapterAddress, string aUglyname, string aFilename, bool aNoTrust, ChangedHandler aHandler)
            {
                iHandler = aHandler;

                iMessage = "Updating Software...";
                iProgress = 0;
                iFallbackToMain = false;
                iFallbackToFallback = false;
                iMainToMain = false;
                iRebooting = false;
                iCount = 0;
                iCountMax = kInitCount + kFallbackToMainCount + kRebootCount + kFallbackToFallbackCount + kRebootCount + kMainToMainCount + kRebootCount;

                if (iHandler != null)
                {
                    iHandler(iMessage, iProgress);
                }

                Linn.ProductSupport.Flash.Reprogrammer reprogrammer = new Linn.ProductSupport.Flash.Reprogrammer(aAdapterAddress, this, aUglyname, aFilename);
                reprogrammer.Fallback = false;
                reprogrammer.NoExec = false;
                reprogrammer.Wait = true; // wait to discover device after reprogramming
                reprogrammer.NoTrust = aNoTrust;

                string failMessage;
                bool result = reprogrammer.Execute(out failMessage);

                reprogrammer.Close();

                iProgress = 100;

                if (!result)
                {
                    iMessage = "Software Update Failed (" + failMessage + ")";
                }
                else
                {
                    iMessage = "Software Update Succeeded";
                }

                if (iHandler != null)
                {
                    iHandler(iMessage, iProgress);
                }
            }

            #region IConsole implementation
            public void Newline()
            {
            }

            public void Title(string aMessage)
            {
                if (aMessage == kStartFallbackToMain)
                {
                    iRebooting = false;
                    iFallbackToMain = true;
                    iCount = kInitCount;    // ensure we are at the correct count
                }
                else if (aMessage == kEndFallbackToMain)
                {
                    iRebooting = true;
                    iCount = kInitCount + kFallbackToMainCount;    // ensure we are at the correct count
                }
                else if (aMessage == kStartFallbackToFallback)
                {
                    // if we have not been required to reflash fallback recalculate the progress bar
                    if (!iFallbackToMain)
                    {
                        iCountMax = kInitCount + kFallbackToFallbackCount + kRebootCount + kMainToMainCount + kRebootCount;
                        iCount = kInitCount + kRebootCount;    // ensure we are at the correct count
                    }
                    else
                    {
                        iCount += kRebootCount;    // ensure we are at the correct count
                    }
                    iRebooting = false;
                    iFallbackToFallback = true;
                }
                else if (aMessage == kEndFallbackToFallback)
                {
                    iRebooting = true;
                    iCount = kInitCount + kFallbackToMainCount + kRebootCount + kFallbackToFallbackCount;    // ensure we are at the correct count
                }
                else if (aMessage == kStartMainToMain)
                {
                    // if we have not been required to reflash fallback recalculate the progress bar
                    if (!iFallbackToMain && !iFallbackToFallback)
                    {
                        iCountMax = kInitCount + kMainToMainCount + kRebootCount;
                        iCount = kInitCount + kRebootCount;    // ensure we are at the correct count
                    }
                    else
                    {
                        iCount += kRebootCount;    // ensure we are at the correct count
                    }

                    iRebooting = false;
                    iMainToMain = true;
                }
                else if (aMessage == kEndMainToMain)
                {
                    iRebooting = true;
                    iCount = iCountMax - kRebootCount;    // ensure we are at the correct count
                }
                else if (aMessage.EndsWith(kSuccess))
                {
                    iRebooting = false;
                    iCount += kRebootCount;
                }

                UpdateProgress();
            }

            public void Write(string aMessage)
            {
            }

            public void ProgressOpen(int aMax)
            {
                iLastValue = 0;
            }

            public void ProgressSetValue(int aValue)
            {
                if (!iRebooting)
                {
                    if (iFallbackToMain && !iFallbackToFallback && !iMainToMain)
                    {
                        if (iCount < kInitCount + kFallbackToMainCount)
                        {
                            iCount += (uint)(aValue - iLastValue);
                            UpdateProgress();
                        }
                    }
                    else if (iFallbackToFallback && !iMainToMain)
                    {
                        if (iCount < kInitCount + kFallbackToMainCount + kRebootCount + kFallbackToFallbackCount)
                        {
                            iCount += (uint)(aValue - iLastValue);
                            UpdateProgress();
                        }
                    }
                    else if (iMainToMain)
                    {
                        if (iCount < iCountMax - kRebootCount)
                        {
                            iCount += (uint)(aValue - iLastValue);
                            UpdateProgress();
                        }
                    }
                    else
                    {
                        iCount += (uint)(aValue - iLastValue);
                        UpdateProgress();
                    }
                }
                iLastValue = aValue;
            }

            public void ProgressClose()
            {
            }
            #endregion

            private void UpdateProgress()
            {
                uint oldProgress = iProgress;
                iProgress = (uint)((iCount * 100) / iCountMax);

                if (iProgress != oldProgress)
                {
                    if (iHandler != null)
                    {
                        iHandler(iMessage, iProgress);
                    }
                }
            }

            private const uint kInitCount = 262292;
            private const uint kFallbackToMainCount = 3899185;
            private const uint kFallbackToFallbackCount = 2824370;
            private const uint kMainToMainCount = 16755765;
            private const uint kRebootCount = 242056;

            private static readonly string kStartFallbackToMain = "[F->M] Load rom file";
            private static readonly string kEndFallbackToMain = "[F->M] Reboot to main";
            private static readonly string kStartFallbackToFallback = "[F->F] Load rom file";
            private static readonly string kEndFallbackToFallback = "[F->F] Reboot to fallback";
            private static readonly string kStartMainToMain = "[M->M] Load rom file";
            private static readonly string kEndMainToMain = "[M->M] Reboot to main";
            private static readonly string kSuccess = "reprogrammed successfully";

            private ChangedHandler iHandler;

            private uint iProgress;
            private string iMessage;

            private bool iFallbackToMain;
            private bool iFallbackToFallback;
            private bool iMainToMain;
            private bool iRebooting;
            private uint iCount;
            private uint iCountMax;
            private int iLastValue;
        }

        public class FactoryDefaulter : IConsole
        {
            public FactoryDefaulter(IPAddress aAdapterAddress, string aUglyname, ChangedHandler aHandler)
            {
                iHandler = aHandler;

                iMessage = "Restoring To Factory Defaults...";
                iProgress = 0;
                iRebooting = false;
                iSettingDefaults = false;
                iCount = 0;
                iCountMax = kSettingDefaultsCount + kRebootCount;

                if (iHandler != null)
                {
                    iHandler(iMessage, iProgress);
                }

                Linn.ProductSupport.Flash.FactoryDefaulter defaulter = new Linn.ProductSupport.Flash.FactoryDefaulter(aAdapterAddress, this, aUglyname);
                defaulter.NoExec = false;
                defaulter.Wait = true; // wait to discover device after reprogramming

                string failMessage;
                bool result = defaulter.Execute(out failMessage);

                defaulter.Close();

                iProgress = 100;

                if (!result)
                {
                    iMessage = "Resetting To Factory Defaults Failed (" + failMessage + ")";
                }
                else
                {
                    iMessage = "Resetting To Factory Defaults Succeeded";
                }

                if (iHandler != null)
                {
                    iHandler(iMessage, iProgress);
                }
            }

            #region IConsole implementation
            public void Newline()
            {
            }

            public void Title(string aMessage)
            {
                if (aMessage == kStartSettingDefaults)
                {
                    iRebooting = false;
                    iSettingDefaults = true;
                    iCount = 0;    // ensure we are at the correct count
                }
                else if (aMessage == kEndSettingDefaults)
                {
                    iRebooting = true;
                    iCount = iCountMax - kRebootCount;    // ensure we are at the correct count
                }
                else if (aMessage.EndsWith(kSuccess))
                {
                    iRebooting = false;
                    iCount += kRebootCount;
                }

                UpdateProgress();
            }

            public void Write(string aMessage)
            {
            }

            public void ProgressOpen(int aMax)
            {
                iLastValue = 0;
            }

            public void ProgressSetValue(int aValue)
            {
                if (!iRebooting)
                {
                    if (iSettingDefaults)
                    {
                        if (iCount < iCountMax - kRebootCount)
                        {
                            iCount += (uint)(aValue - iLastValue);
                            UpdateProgress();
                        }
                    }
                }
                iLastValue = aValue;
            }

            public void ProgressClose()
            {
            }
            #endregion

            private void UpdateProgress()
            {
                uint oldProgress = iProgress;
                iProgress = (uint)((iCount * 100) / iCountMax);

                if (iProgress != oldProgress)
                {
                    if (iHandler != null)
                    {
                        iHandler(iMessage, iProgress);
                    }
                }
            }

            private const uint kSettingDefaultsCount = 1361494;
            private const uint kRebootCount = 13753;

            private static readonly string kStartSettingDefaults = "Collect main rom directory";
            private static readonly string kEndSettingDefaults = "Reboot to main";
            private static readonly string kSuccess = "set to factory defaults successfully";

            private ChangedHandler iHandler;

            private uint iProgress;
            private string iMessage;

            private bool iRebooting;
            private bool iSettingDefaults;
            private uint iCount;
            private uint iCountMax;
            private int iLastValue;
        }

        public class GetRomFilenameFailed : Exception
        {
            public GetRomFilenameFailed(string aMessage)
                : base(aMessage)
            {
            }
        }

        public static string GetRomFilename(string aUnpackPath, string aZipFilename, string aVariant)
        {
            string mainDir = null;
            string unzippedDirectory = Path.Combine(aUnpackPath, Path.GetFileNameWithoutExtension(aZipFilename));
            lock (iLock)
            {
                // first check that supplied aZipFilename is not an unzipped directory
                if (File.Exists(aZipFilename))
                {
                    if (!Directory.Exists(aUnpackPath))
                    {
                        Directory.CreateDirectory(aUnpackPath);
                    }

                    // unzip firmware bundle to temp directory

                    if (!Directory.Exists(unzippedDirectory))
                    {
                        try
                        {
                            FastZip fz = new FastZip();
                            fz.ExtractZip(aZipFilename, unzippedDirectory, ""); // "" is for file filter
                        }
                        catch (ZipException)
                        {
                            throw new GetRomFilenameFailed(aZipFilename + " is not a valid zip file");
                        }
                        catch (FileNotFoundException)
                        {
                            throw new GetRomFilenameFailed("Could not find " + aZipFilename);
                        }
                        catch (Exception e)
                        {
                            throw new GetRomFilenameFailed(e.Message);
                        }
                    }
                }
                else
                {
                    if(Directory.Exists(aZipFilename))
                    {
                        mainDir = aZipFilename;
                    }
                    else
                    {
                        throw new GetRomFilenameFailed("Could not find " + aZipFilename);
                    }
                }
            }

            if(string.IsNullOrEmpty(mainDir))
            {
                // Only one main folder per zip file (i.e. Reprog_Date/SXXXXXXXX)
                string[] mainDirs = Directory.GetDirectories(unzippedDirectory, "*", SearchOption.TopDirectoryOnly);
                if (mainDirs.Length != 1)
                {
                    throw new GetRomFilenameFailed("File structure is invalid in " + unzippedDirectory);
                }

                mainDir = mainDirs[0];
            }

            // Directory below main directory must be the Variant directory (i.e. Reprog_Date/SXXXXXXXX/AkuarateDsMk1)
            string[] dirs = Directory.GetDirectories(mainDir, aVariant, SearchOption.TopDirectoryOnly);
            if (dirs.Length <= 0)
            {
                throw new GetRomFilenameFailed("Could not find " + aVariant + " directory in " + mainDir);
            }
            else if (dirs.Length > 1)
            {
                throw new GetRomFilenameFailed("Found multiple " + aVariant + " directories in " + mainDir);
            }

            // Only one collection file in the Variant directory (i.e. Reprog_Date/SXXXXXXXX/AkuarateDsMk1/bin/CollectionAkurateDs.xml)
            string[] files = Directory.GetFiles(dirs[0], "Collection*.xml", SearchOption.AllDirectories);
            if (files.Length <= 0)
            {
                throw new GetRomFilenameFailed("Could not find required Collection file for " + aVariant + " in " + dirs[0]);
            }
            else if (files.Length > 1)
            {
                throw new GetRomFilenameFailed("Found multiple Collection files for " + aVariant + " in " + dirs[0]);
            }

            UserLog.Write("FlashSupport: [ROM file selected: " + files[0] + "]");
            return files[0];
        }

        public delegate void ChangedHandler(string aMessage, uint aProgress);

        private static object iLock = new object();
    }
}
