﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Alphaleonis.Win32.Security;

using RawCopy;
using Registry;
using Registry.Abstractions;
using Serilog;

#if NET462
using Directory = Alphaleonis.Win32.Filesystem.Directory;
using File = Alphaleonis.Win32.Filesystem.File;
using Path = Alphaleonis.Win32.Filesystem.Path;
#else
using Path = System.IO.Path;
using Directory = System.IO.Directory;
using File = System.IO.File;
#endif

namespace AppCompatCache
{
    public class AppCompatCache
    {
        public enum Execute
        {
            Yes,
            No,
            NA
        }

        [Flags]
        public enum InsertFlag
        {
            Unknown1 = 0x00000001,
            Executed = 0x00000002,
            Unknown4 = 0x00000004,
            Unknown8 = 0x00000008,
            Unknown10 = 0x00000010,
            Unknown20 = 0x00000020,
            Unknown40 = 0x00000040,
            Unknown80 = 0x00000080,
            Unknown10000 = 0x00010000,
            Unknown20000 = 0x00020000,
            Unknown30000 = 0x00030000,
            Unknown40000 = 0x00040000,
            Unknown100000 = 0x00100000,
            Unknown200000 = 0x00200000,
            Unknown400000 = 0x00400000,
            Unknown800000 = 0x00800000
        }

        public enum OperatingSystemVersion
        {
            WindowsXP,
            WindowsVistaWin2k3Win2k8,
            Windows7x86,
            Windows7x64_Windows2008R2,
            Windows80_Windows2012,
            Windows81_Windows2012R2,
            Windows10,
            Windows10Creators,
            Unknown
        }


        public AppCompatCache(byte[] rawBytes, int controlSet, bool is32Bit)
        {
            Caches = new List<IAppCompatCache>();
            var cache = Init(rawBytes, is32Bit, controlSet);
            Caches.Add(cache);
        }

        public AppCompatCache(string filename, int controlSet, bool noLogs)
        {
            byte[] rawBytes = null;
            Caches = new List<IAppCompatCache>();

            var controlSetIds = new List<int>();

            RegistryKey subKey = null;

            var isLiveRegistry = string.IsNullOrEmpty(filename);

            if (isLiveRegistry)
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    throw new NotSupportedException("Live Registry support is not supported on non-Windows platforms.");
                }

                var keyCurrUser = Microsoft.Win32.Registry.LocalMachine;
                var subKey2 =
                    keyCurrUser.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\AppCompatCache");

                if (subKey2 == null)
                {
                    subKey2 =
                        keyCurrUser.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\AppCompatibility");

                    if (subKey2 == null)
                    {
                        Console.WriteLine(
                            @"'CurrentControlSet\Control\Session Manager\AppCompatCache' key not found! Exiting");
                        return;
                    }
                }

                rawBytes = (byte[])subKey2.GetValue("AppCompatCache", null);

                subKey2 = keyCurrUser.OpenSubKey(@"SYSTEM\Select");
                ControlSet = (int)subKey2.GetValue("Current");

                var is32Bit = Is32Bit(filename, null);

                var cache = Init(rawBytes, is32Bit, ControlSet);

                Caches.Add(cache);

                return;
            }

            RegistryHive reg;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Privilege[] privileges = { Privilege.EnableDelegation, Privilege.Impersonate, Privilege.Tcb };
                using var enabler = new PrivilegeEnabler(Privilege.Backup, privileges);
            }

            ControlSet = controlSet;

            if (File.Exists(filename) == false && Helper.RawFileExists(filename) == false)
            {
                throw new FileNotFoundException($"File not found ({filename})!");
            }

            var dirname = Path.GetDirectoryName(Path.GetFullPath(filename));
            var hiveBase = Path.GetFileName(filename);



            List<RawCopyReturn> rawFiles = null;

            try
            {
                reg = new RegistryHive(filename)
                {
                    RecoverDeleted = true
                };
            }
            catch (IOException)
            {
                //file is in use

                if (Helper.IsAdministrator() == false)
                {
                    throw new UnauthorizedAccessException("Administrator privileges not found!");
                }

                Log.Warning("'{Filename}' is in use. Rerouting...", filename);

                var files = new List<string>();
                files.Add(filename);

                var logFiles = Directory.GetFiles(dirname, $"{hiveBase}.LOG?").ToList();

                var log1 = $"{dirname}\\{hiveBase}.LOG1";
                var log2 = $"{dirname}\\{hiveBase}.LOG2";

                if (logFiles.Count == 0)
                {
                    if (Helper.RawFileExists(log1))
                    {
                        logFiles.Add(log1);
                    }

                    if (Helper.RawFileExists(log2))
                    {
                        logFiles.Add(log2);
                    }
                }

                foreach (var logFile in logFiles)
                {
                    files.Add(logFile);
                }

                rawFiles = Helper.GetFiles(files);
                var b = new byte[rawFiles.First().FileStream.Length];

                rawFiles.First().FileStream.Read(b, 0, (int)rawFiles.First().FileStream.Length);


                reg = new RegistryHive(b, rawFiles.First().InputFilename);
            }

            if (reg.Header.PrimarySequenceNumber != reg.Header.SecondarySequenceNumber)
            {
                if (string.IsNullOrEmpty(dirname))
                {
                    dirname = ".";
                }

                var logFiles = Directory.GetFiles(dirname, $"{hiveBase}.LOG?").ToList();

                var log1 = $"{dirname}\\{hiveBase}.LOG1";
                var log2 = $"{dirname}\\{hiveBase}.LOG2";

                if (logFiles.Count == 0)
                {
                    if (File.Exists(log1))
                    {
                        logFiles.Add(log1);
                    }

                    if (File.Exists(log2))
                    {
                        logFiles.Add(log2);
                    }
                }

                if (logFiles.Count == 0)
                {
                    if (Helper.IsAdministrator())
                    {
                        if (Helper.RawFileExists(log1))
                        {
                            logFiles.Add(log1);
                        }

                        if (Helper.RawFileExists(log2))
                        {
                            logFiles.Add(log2);
                        }
                    }
                    else
                    {
                        Log.Fatal("Log files not found and no administrator access to look for them!");
                    }

                }


                if (logFiles.Count == 0)
                {
                    if (noLogs == false)
                    {
                        Log.Warning(
                            "Registry hive is dirty and no transaction logs were found in the same directory! LOGs should have same base name as the hive. Aborting!!");
                        throw new Exception(
                            "Sequence numbers do not match and transaction logs were not found in the same directory as the hive. Aborting");
                    }

                    Log.Warning(
                        "Registry hive is dirty and no transaction logs were found in the same directory. Data may be missing! Continuing anyways...");
                }
                else
                {
                    if (noLogs == false)
                    {
                        if (rawFiles != null)
                        {
                            var lt = new List<TransactionLogFileInfo>();
                            foreach (var rawCopyReturn in rawFiles.Skip(1).ToList())
                            {
                                var b = new byte[rawCopyReturn.FileStream.Length];

                                var tt = new TransactionLogFileInfo(rawCopyReturn.InputFilename,
                                    b);
                                lt.Add(tt);
                            }

                            reg.ProcessTransactionLogs(lt, true);
                        }
                        else
                        {
                            reg.ProcessTransactionLogs(logFiles.ToList(), true);
                        }
                    }
                    else
                    {
                        Log.Warning(
                            "Registry hive is dirty and transaction logs were found in the same directory, but --nl was provided. Data may be missing! Continuing anyways...");
                    }
                }
            }


            reg.ParseHive();

            if (controlSet == -1)
            {
                for (var i = 0; i < 10; i++)
                {
                    subKey = reg.GetKey($@"ControlSet00{i}\Control\Session Manager\AppCompatCache");

                    if (subKey == null)
                    {
                        subKey = reg.GetKey($@"ControlSet00{i}\Control\Session Manager\AppCompatibility");
                    }

                    if (subKey != null)
                    {
                        controlSetIds.Add(i);
                    }
                }

                if (controlSetIds.Count > 1)
                {
                    Log.Warning(
                        "***The following ControlSet00x keys will be exported: {Cs}. Use -c to process keys individually", string.Join(",", controlSetIds));
                }
            }
            else
            {
                //a control set was passed in
                subKey = reg.GetKey($@"ControlSet00{ControlSet}\Control\Session Manager\AppCompatCache");

                if (subKey == null)
                {
                    subKey = reg.GetKey($@"ControlSet00{ControlSet}\Control\Session Manager\AppCompatibility");
                }

                if (subKey == null)
                {
                    throw new Exception($"Could not find ControlSet00{ControlSet}. Exiting");
                }

                controlSetIds.Add(ControlSet);
            }


            var is32 = Is32Bit(filename, reg);


            Log.Debug("**** Found {Count} ids to process", controlSetIds.Count);


            foreach (var id in controlSetIds)
            {
                Log.Debug("**** Processing id {Id}", id);

                //  var hive2 = new RegistryHiveOnDemand(filename);

                subKey = reg.GetKey($@"ControlSet00{id}\Control\Session Manager\AppCompatCache");

                if (subKey == null)
                {
                    Log.Debug("**** Initial subkey null, getting appCompatability key");
                    subKey = reg.GetKey($@"ControlSet00{id}\Control\Session Manager\AppCompatibility");
                }

                Log.Debug("**** Looking for AppCompatcache value");

                var val = subKey?.Values.SingleOrDefault(c => c.ValueName == "AppCompatCache");

                if (val != null)
                {
                    Log.Debug("**** Found AppCompatcache value");
                    rawBytes = val.ValueDataRaw;
                }

                if (rawBytes == null)
                {
                    Log.Error("'AppCompatCache' value not found for 'ControlSet00{Id}'! Exiting", id);
                }

                var cache = Init(rawBytes, is32, id);

                Caches.Add(cache);
            }
        }

        public int ControlSet { get; }

        public List<IAppCompatCache> Caches { get; }
        public OperatingSystemVersion OperatingSystem { get; private set; }

        //https://github.com/libyal/winreg-kb/wiki/Application-Compatibility-Cache-key
        //https://dl.mandiant.com/EE/library/Whitepaper_ShimCacheParser.pdf

        private IAppCompatCache Init(byte[] rawBytes, bool is32, int controlSet)
        {
            IAppCompatCache appCache = null;
            OperatingSystem = OperatingSystemVersion.Unknown;

            string signature;


            var sigNum = BitConverter.ToUInt32(rawBytes, 0);


            //TODO check minimum length of rawBytes and throw exception if not enough data

            signature = Encoding.ASCII.GetString(rawBytes, 128, 4);

            Log.Debug("**** Signature {Signature}, Sig num {SigNum}", signature, $"0x{sigNum:X}");

            if (sigNum == 0xDEADBEEF) //DEADBEEF, WinXp
            {
                OperatingSystem = OperatingSystemVersion.WindowsXP;

                Log.Debug("**** Processing XP hive");

                appCache = new WindowsXP(rawBytes, is32, controlSet);
            }
            else if (sigNum == 0xbadc0ffe)
            {
                OperatingSystem = OperatingSystemVersion.WindowsVistaWin2k3Win2k8;
                appCache = new VistaWin2k3Win2k8(rawBytes, is32, controlSet);
            }
            else if (sigNum == 0xBADC0FEE) //BADC0FEE, Win7
            {
                if (is32)
                {
                    OperatingSystem = OperatingSystemVersion.Windows7x86;
                }
                else
                {
                    OperatingSystem = OperatingSystemVersion.Windows7x64_Windows2008R2;
                }

                appCache = new Windows7(rawBytes, is32, controlSet);
            }

            else if (signature == "00ts")
            {
                OperatingSystem = OperatingSystemVersion.Windows80_Windows2012;
                appCache = new Windows8x(rawBytes, OperatingSystem, controlSet);
            }
            else if (signature == "10ts")
            {
                OperatingSystem = OperatingSystemVersion.Windows81_Windows2012R2;
                appCache = new Windows8x(rawBytes, OperatingSystem, controlSet);
            }
            else
            {
                //is it windows 10?

                var offsetToEntries = BitConverter.ToInt32(rawBytes, 0);

                OperatingSystem = OperatingSystemVersion.Windows10;

                if (offsetToEntries == 0x34)
                {
                    OperatingSystem = OperatingSystemVersion.Windows10Creators;
                }

                signature = Encoding.ASCII.GetString(rawBytes, offsetToEntries, 4);
                if (signature == "10ts")
                {
                    appCache = new Windows10(rawBytes, controlSet);
                }
            }

            if (appCache == null)
            {
                throw new Exception(
                    "Unable to determine operating system! Please send the hive to saericzimmerman@gmail.com");
            }


            return appCache;
        }

        public static bool Is32Bit(string fileName, RegistryHive reg)
        {
            if (reg == null)
            {
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    throw new NotSupportedException("'Filename' is required on non-Windows platforms.");
                }
                var keyCurrUser = Microsoft.Win32.Registry.LocalMachine;
                var subKey = keyCurrUser.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment");

                var val = subKey?.GetValue("PROCESSOR_ARCHITECTURE");

                if (val != null)
                {
                    return val.ToString().Equals("x86");
                }
            }
            else
            {
                try
                {
                    var subKey1 = reg.GetKey("Select");

                    var currentCtlSet = int.Parse(subKey1.Values.Single(c => c.ValueName == "Current").ValueData);

                    subKey1 = reg.GetKey($"ControlSet00{currentCtlSet}\\Control\\Session Manager\\Environment");

                    var val = subKey1?.Values.SingleOrDefault(c => c.ValueName == "PROCESSOR_ARCHITECTURE");

                    if (val != null)
                    {
                        return val.ValueData.Equals("x86");
                    }
                }
                catch (Exception)
                {
                    var l = new List<string>();
                    l.Add(fileName);

                    var ff = Helper.GetFiles(l);

                    var b = new byte[ff.First().FileStream.Length];

                    var hive = new RegistryHiveOnDemand(b, fileName);
                    var subKey = hive.GetKey("Select");

                    var currentCtlSet = int.Parse(subKey.Values.Single(c => c.ValueName == "Current").ValueData);

                    subKey = hive.GetKey($"ControlSet00{currentCtlSet}\\Control\\Session Manager\\Environment");

                    var val = subKey?.Values.SingleOrDefault(c => c.ValueName == "PROCESSOR_ARCHITECTURE");

                    if (val != null)
                    {
                        return val.ValueData.Equals("x86");
                    }
                }
            }

            throw new NullReferenceException("Unable to determine CPU architecture!");
        }
    }
}