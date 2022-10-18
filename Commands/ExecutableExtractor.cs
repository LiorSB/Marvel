using Amcache;
using ExtensionBlocks;
using JumpList.Automatic;
using Lnk.ExtraData;
using Marvel.Enum;
using Marvel.Model;
using Marvel.Utilities;
using Prefetch;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using SrumData;
using CsvHelper;


namespace Marvel.Commands
{
    public class ExecutableExtractor
    {
        private const string EXE_FORMAT_LOWER_CASE = ".exe";
        private const string EXE_FORMAT_UPPER_CASE = ".EXE";

        private const string PF_FORMAT = "*.pf";
        private const string PREFETCH_FOLDER = @"\Prefetch";
        private const string PREFETCH_DIRECTORY = @"C:\Windows\Prefetch";
        private const string PREFETCH_EXCEL = @"_Parsed_Prefetch_File.xls";

        private const string HIVE_FORMAT = ".hve";
        private const string AMCACHE_FOLDER = @"\Amcache";
        private const string AMCACHE_DIRECTORY = @"C:\Windows\AppCompat\Programs\Amcache.hve";

        private const string AUTOMATIC_DESTINATIONS_FORMAT = "*.automaticDestinations-ms";
        private const string AUTOMATIC_DESTINATIONS_FOLDER = @"\AutomaticDestinations";
        private const string JUMP_LIST_DIRECTORY = @"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Recent\AutomaticDestinations";
        private const string AUTOMATIC_DESTINATIONS_EXCEL = @"_Parsed_Automatic_Destinations.xls";

        private const string SHIMCACHE_FOLDER = @"\Shimcache";
        private const string REMOTE_SHIMCACHE_FOLDER = @"C:\Marvel\Shimcache";

        private const string SRUM_FOLDER = @"\Srum";
        private const string SRU_DIRECTORY = @"C:\Windows\System32\sru";
        private const string SRUDB_FILE = @"SRUDB.dat";
        private const string SRUDB_FILE_DIRECTORY = @"C:\Windows\System32\sru\SRUDB.dat";

        private const string C_DIRECTORY = "C:";


        private static readonly string[] PREFETCH_TABLE_COLUMNS = { 
            "SourceFile",
            "SourceCreated",
            "SourceModified",
            "SourceAccessed",
            "ExecutableName", 
            "Hash", 
            "Size", 
            "Version", 
            "RunCount",
            "LastRun",
            "PreviousRun0", 
            "PreviousRun1", 
            "PreviousRun2", 
            "PreviousRun3", 
            "PreviousRun4", 
            "PreviousRun5", 
            "PreviousRun6", 
            "Volume0Name",
            "Volume0Serial",
            "Volume0Created", 
            "Volume1Name", 
            "Volume1Serial",
            "Volume1Created", 
            "Directories",
            "FilesLoaded",
            "ParsingError"
        };
        private static readonly string[] AUTOMATIC_DESTINATION_TABLE_COLUMNS = {
            "SourceFile",
            "SourceCreated",
            "SourceModified",
            "SourceAccessed",
            "AppId",
            "AppIdDescription",
            "DestListVersion",
            "LastUsedEntryNumber",
            "MRU",
            "EntryNumber",
            "CreationTime",
            "LastModified",
            "Hostname",
            "MacAddress",
            "Path",
            "InteractionCount",
            "PinStatus",
            "FileBirthDroid",
            "FileDroid",
            "VolumeBirthDroid",
            "VolumeDroid",
            "TargetCreated",
            "TargetModified",
            "TargetAccessed",
            "FileSize",
            "RelativePath",
            "WorkingDirectory",
            "FileAttributes",
            "HeaderFlags",
            "DriveType",
            "VolumeSerialNumber",
            "VolumeLabel",
            "LocalPath",
            "CommonPath",
            "TargetIDAbsolutePath",
            "TargetMFTEntryNumber",
            "TargetMFTSequenceNumber",
            "MachineID",
            "MachineMACAddress",
            "TrackerCreatedOn",
            "ExtraBlocksPresent",
            "Arguments" 
        };

        public string ExtractFiles(Host host, string toDirectory, ProtocolsEnum selectedProtocol)
        {
            string folderName = host.IP.Replace('.', '_');
            string path = toDirectory + '\\' + folderName;

            DirectoryInfo hostDirectoryInfo = Directory.CreateDirectory(path);

            PrefetchExtractor(host, path, selectedProtocol);
            //AmcacheExtractor(host, path, selectedProtocol); // Bugged: Hive files can't be open for read, because they are locked by another resource.
            JumpListExtractor(host, path, selectedProtocol);
            //ShimcacheExtractor(host, path, selectedProtocol);
            SrumExtractor(host, path, selectedProtocol);

            return $"Executable files downloaded to: {path}";
        }

        public string PrefetchExtractor(Host host, string toDirectory, ProtocolsEnum selectedProtocol)
        {
            if (selectedProtocol != ProtocolsEnum.WinRM)
            {
                toDirectory += PREFETCH_FOLDER;
                /*DirectoryInfo pathInfo = */Directory.CreateDirectory(toDirectory);
            }

            Task continuation = CommandUtilities.Instance.RunCommand(selectedProtocol, host, PREFETCH_DIRECTORY, toDirectory, CommandsEnum.GetFolder);
            continuation.Wait();

            if (selectedProtocol == ProtocolsEnum.WinRM)
            {
                toDirectory += PREFETCH_FOLDER;
            }

            if (!Directory.Exists(toDirectory))
            {
                return $"Failed to pull {PREFETCH_DIRECTORY}";
            }

            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook excelWorkbook = excelApp.Workbooks.Add();
            Excel.Worksheet excelWorksheet = (Excel.Worksheet)excelWorkbook.Sheets.Add();

            foreach (var column in PREFETCH_TABLE_COLUMNS.Select((value, i) => new { i, value }))
            {
               excelWorksheet.Cells[1, column.i + 1] = column.value;
            }

            int row = 2;

            IPrefetch pf;

            foreach (string file in Directory.GetFiles(toDirectory, PF_FORMAT))
            {
                try
                {
                    pf = PrefetchFile.Open(file);

                    string executableName = pf.Header.ExecutableFilename;
                    string executablePath = "";

                    excelWorksheet.Cells[row, 1] = pf.SourceFilename;
                    excelWorksheet.Cells[row, 2] = pf.SourceCreatedOn.ToString();
                    excelWorksheet.Cells[row, 3] = pf.SourceModifiedOn.ToString();
                    excelWorksheet.Cells[row, 4] = pf.SourceAccessedOn.ToString();
                    excelWorksheet.Cells[row, 5] = pf.Header.ExecutableFilename;
                    excelWorksheet.Cells[row, 6] = pf.Header.Hash;
                    excelWorksheet.Cells[row, 7] = pf.Header.FileSize;
                    excelWorksheet.Cells[row, 8] = pf.Header.Version.ToString();
                    excelWorksheet.Cells[row, 9] = pf.RunCount;


                    int lastRunTimes = pf.LastRunTimes.Count;

                    int j = 0;

                    for (int i = 10; i < 10 + lastRunTimes; i++)
                    {
                        excelWorksheet.Cells[row, i] = pf.LastRunTimes[j++].ToString();
                    }


                    int numberOfVolumes = pf.VolumeInformation.Count;
                    string directoryNames = "";
                    j = 18;

                    for (int i = 0; i < numberOfVolumes; i++)
                    {
                        excelWorksheet.Cells[row, j] = pf.VolumeInformation[i].DeviceName;
                        excelWorksheet.Cells[row, j + 1] = pf.VolumeInformation[i].SerialNumber;
                        excelWorksheet.Cells[row, j + 2] = pf.VolumeInformation[i].CreationTime.ToString();

                        foreach (string directoryName in pf.VolumeInformation[i].DirectoryNames)
                        {
                            directoryNames += directoryName + " ";
                        }

                        j += 3;
                    }

                    excelWorksheet.Cells[row, 24] = directoryNames;

                    string fileNames = "";
                    bool exeFound = false;

                    foreach (string fileName in pf.Filenames)
                    {
                        fileNames += fileName + " ";

                        if (fileName.Contains(executableName) && !exeFound)
                        {
                            executablePath += fileName;

                            int indexToSplit = executablePath.IndexOf('}') + 1;
                            executablePath = C_DIRECTORY + executablePath[indexToSplit..];

                            exeFound = true;
                        }
                    }

                    excelWorksheet.Cells[row, 25] = fileNames;
                    excelWorksheet.Cells[row, 26] = pf.ParsingError;

                    row += 1;

                    CommandUtilities.Instance.RunCommand(selectedProtocol, host, executablePath, toDirectory, CommandsEnum.ReceiveItem);
                }
                catch (Exception e)
                {
                    continue;
                }
                finally
                {
                    File.Delete(file);
                }
            }

            excelApp.ActiveWorkbook.SaveAs(toDirectory.Replace(PREFETCH_FOLDER, "") + @$"\{DateTimeOffset.Now:yyyyMMddHHmmss}" + PREFETCH_EXCEL, Excel.XlFileFormat.xlWorkbookNormal);

            excelWorkbook.Close();
            excelApp.Quit();

            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(excelWorksheet);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(excelWorkbook);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(excelApp);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return $"Prefetch executables downloaded to: {toDirectory}";
        }

        public string AmcacheExtractor(Host host, string toDirectory, ProtocolsEnum selectedProtocol)
        {
            toDirectory += AMCACHE_FOLDER;
            Directory.CreateDirectory(toDirectory);

            Task continuation = CommandUtilities.Instance.RunCommand(selectedProtocol, host, AMCACHE_DIRECTORY, toDirectory, CommandsEnum.ReceiveItem);
            continuation.Wait();

            try
            {
                AmcacheNew amcache = new AmcacheNew(toDirectory + AMCACHE_FOLDER + HIVE_FORMAT, true, false);

                foreach (var fileEntry in amcache.UnassociatedFileEntries)
                {
                    CommandUtilities.Instance.RunCommand(selectedProtocol, host, fileEntry.FullPath, toDirectory, CommandsEnum.ReceiveItem);
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
            }

            return $"Amcache executables downloaded to: {toDirectory}";
        }

        public string JumpListExtractor(Host host, string toDirectory, ProtocolsEnum selectedProtocol)
        {
            if (selectedProtocol != ProtocolsEnum.WinRM)
            {
                toDirectory += AUTOMATIC_DESTINATIONS_FOLDER;
                /*DirectoryInfo pathInfo = */
                Directory.CreateDirectory(toDirectory);
            }

            Task continuation = CommandUtilities.Instance.RunCommand(selectedProtocol, host, JUMP_LIST_DIRECTORY, toDirectory, CommandsEnum.GetFolder);
            continuation.Wait();

            if (selectedProtocol == ProtocolsEnum.WinRM)
            {
                toDirectory += AUTOMATIC_DESTINATIONS_FOLDER;
            }

            if (!Directory.Exists(toDirectory))
            {
                return $"Failed to pull {JUMP_LIST_DIRECTORY}";
            }

            int numberOfFilesToDownload = 20;

            Excel.Application excelApp = new Excel.Application();
            Excel.Workbook excelWorkbook = excelApp.Workbooks.Add();
            Excel.Worksheet excelWorksheet = (Excel.Worksheet)excelWorkbook.Sheets.Add();

            foreach (var column in AUTOMATIC_DESTINATION_TABLE_COLUMNS.Select((value, i) => new { i, value }))
            {
                excelWorksheet.Cells[1, column.i + 1] = column.value;
            }

            int row = 2;

            foreach (string file in Directory.GetFiles(toDirectory, AUTOMATIC_DESTINATIONS_FORMAT))
            {
                try
                {
                    AutomaticDestination ad = JumpList.JumpList.LoadAutoJumplist(file);

                    if (numberOfFilesToDownload <= 0)
                    {
                        CommandUtilities.Instance.RunCommand(selectedProtocol, host, ad.DestListEntries[0].Path, toDirectory, CommandsEnum.ReceiveItem);
                    }

                    numberOfFilesToDownload -= 1;

                    FileInfo sourceInfo = new(ad.SourceFile);

                    foreach (AutoDestList destEntry in ad.DestListEntries)
                    {
                        excelWorksheet.Cells[row, 1] = ad.SourceFile;
                        excelWorksheet.Cells[row, 2] = sourceInfo.CreationTime.ToString();
                        excelWorksheet.Cells[row, 3] = sourceInfo.LastWriteTime.ToString();
                        excelWorksheet.Cells[row, 4] = sourceInfo.LastAccessTime.ToString();
                        excelWorksheet.Cells[row, 5] = ad.AppId.AppId;
                        excelWorksheet.Cells[row, 6] = ad.AppId.Description;
                        excelWorksheet.Cells[row, 7] = ad.DestListVersion;
                        excelWorksheet.Cells[row, 8] = ad.LastUsedEntryNumber;
                        excelWorksheet.Cells[row, 9] = destEntry.MRUPosition;
                        excelWorksheet.Cells[row, 10] = destEntry.EntryNumber;
                        excelWorksheet.Cells[row, 11] = destEntry.CreatedOn.ToString(); ;
                        excelWorksheet.Cells[row, 12] = destEntry.LastModified.ToString();
                        excelWorksheet.Cells[row, 13] = destEntry.Hostname;
                        excelWorksheet.Cells[row, 14] = destEntry.MacAddress;
                        excelWorksheet.Cells[row, 15] = destEntry.Path;
                        excelWorksheet.Cells[row, 16] = destEntry.InteractionCount;
                        excelWorksheet.Cells[row, 17] = destEntry.Pinned;
                        excelWorksheet.Cells[row, 18] = destEntry.FileBirthDroid.ToString();
                        excelWorksheet.Cells[row, 19] = destEntry.FileDroid.ToString();
                        excelWorksheet.Cells[row, 20] = destEntry.VolumeBirthDroid.ToString();
                        excelWorksheet.Cells[row, 21] = destEntry.VolumeDroid.ToString();
                        excelWorksheet.Cells[row, 22] = destEntry.Lnk?.Header.TargetCreationDate.ToString();
                        excelWorksheet.Cells[row, 23] = destEntry.Lnk?.Header.TargetModificationDate.ToString();
                        excelWorksheet.Cells[row, 24] = destEntry.Lnk?.Header.TargetLastAccessedDate.ToString();
                        excelWorksheet.Cells[row, 25] = destEntry.Lnk?.Header.FileSize ?? 0;
                        excelWorksheet.Cells[row, 26] = destEntry.Lnk?.RelativePath;
                        excelWorksheet.Cells[row, 27] = destEntry.Lnk?.WorkingDirectory;
                        excelWorksheet.Cells[row, 28] = destEntry.Lnk?.Header.FileAttributes.ToString();
                        excelWorksheet.Cells[row, 29] = destEntry.Lnk?.Header.DataFlags.ToString();
                        excelWorksheet.Cells[row, 30] = destEntry.Lnk?.VolumeInfo?.DriveType;
                        excelWorksheet.Cells[row, 31] = destEntry.Lnk?.VolumeInfo?.VolumeSerialNumber;
                        excelWorksheet.Cells[row, 32] = destEntry.Lnk?.VolumeInfo?.VolumeLabel;
                        excelWorksheet.Cells[row, 33] = destEntry.Lnk?.LocalPath;
                        excelWorksheet.Cells[row, 34] = destEntry.Lnk?.CommonPath;

                        string targetIDAbsolutePath = GetAbsolutePathFromTargetIDs(destEntry.Lnk.TargetIDs);

                        if (targetIDAbsolutePath.Length == 0)
                        {
                            if (destEntry.Lnk.NetworkShareInfo != null)
                            {
                                targetIDAbsolutePath = @$"{destEntry.Lnk.NetworkShareInfo.NetworkShareName}\{destEntry.Lnk.CommonPath}";
                            }
                            else
                            {
                                targetIDAbsolutePath = @$"{destEntry.Lnk.LocalPath}\{destEntry.Lnk.CommonPath}";
                            }
                        }

                        excelWorksheet.Cells[row, 35] = targetIDAbsolutePath;

                        if (destEntry.Lnk?.TargetIDs?.Count > 0)
                        {
                            Lnk.ShellItems.ShellBag si = destEntry.Lnk?.TargetIDs.Last();

                            if (si.ExtensionBlocks?.Count > 0)
                            {
                                IExtensionBlock eb = si.ExtensionBlocks.LastOrDefault(t => t is Beef0004);

                                if (eb is Beef0004)
                                {
                                    Beef0004 eb4 = eb as Beef0004;

                                    if (eb4.MFTInformation.MFTEntryNumber != null)
                                    {
                                        excelWorksheet.Cells[row, 36] = $"0x{eb4.MFTInformation.MFTEntryNumber.Value:X}";
                                    }

                                    if (eb4.MFTInformation.MFTSequenceNumber != null)
                                    {
                                        excelWorksheet.Cells[row, 37] = $"0x{eb4.MFTInformation.MFTSequenceNumber.Value:X}";
                                    }
                                }
                            }
                        }

                        ExtraDataBase tnb = destEntry.Lnk?.ExtraBlocks.SingleOrDefault(t => t.GetType().Name.ToUpper() == "TRACKERDATABASEBLOCK");

                        if (tnb != null)
                        {
                            TrackerDataBaseBlock tnbBlock = tnb as TrackerDataBaseBlock;

                            excelWorksheet.Cells[row, 38] = tnbBlock?.MachineId;
                            excelWorksheet.Cells[row, 39] = tnbBlock?.MacAddress;
                            excelWorksheet.Cells[row, 40] = tnbBlock?.CreationTime.ToString();
                        }

                        if ((destEntry.Lnk?.Header.DataFlags & Lnk.Header.DataFlag.HasArguments) == Lnk.Header.DataFlag.HasArguments)
                        {
                            excelWorksheet.Cells[row, 42] = destEntry.Lnk?.Arguments ?? string.Empty;
                        }

                        row += 1;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
                finally
                {
                    File.Delete(file);
                }
            }

            excelApp.ActiveWorkbook.SaveAs(toDirectory.Replace(AUTOMATIC_DESTINATIONS_FOLDER, "") + @$"\{DateTimeOffset.Now:yyyyMMddHHmmss}" + AUTOMATIC_DESTINATIONS_EXCEL, Excel.XlFileFormat.xlWorkbookNormal);

            excelWorkbook.Close();
            excelApp.Quit();

            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(excelWorksheet);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(excelWorkbook);
            System.Runtime.InteropServices.Marshal.FinalReleaseComObject(excelApp);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return $"Jump List files downloaded to: {toDirectory}";
        }

        public string ShimcacheExtractor(Host host, string toDirectory, ProtocolsEnum selectedProtocol)
        {
            Directory.CreateDirectory(REMOTE_SHIMCACHE_FOLDER);
            File.WriteAllBytes($"{REMOTE_SHIMCACHE_FOLDER}/AppCompatCacheParser.zip" , FileStore.Resource.AppCompatCacheParser);
            return "";
        }

        public string SrumExtractor(Host host, string toDirectory, ProtocolsEnum selectedProtocol)
        {
            toDirectory += SRUM_FOLDER;
            /*DirectoryInfo pathInfo = */
            Directory.CreateDirectory(toDirectory);

            Task continuation = CommandUtilities.Instance.RunCommand(selectedProtocol, host, $"{SRU_DIRECTORY}\\{SRUDB_FILE}", toDirectory, CommandsEnum.ReceiveItem);
            continuation.Wait();

            string csv = toDirectory;
            string dt = "yyyy-MM-dd HH:mm:ss";
            Srum sr = null;
            var ts = DateTimeOffset.UtcNow;

            try
            {
                sr = new Srum($"{toDirectory}\\{SRUDB_FILE}", null);
            }
            catch (Exception e)
            {
                return $"Error processing file! Message: {e.Message}." +
                    "\r\n\r\nThis almost always means the database is dirty and must be repaired." +
                    " This can be verified by running 'esentutl.exe /mh SRUDB.dat' and examining the 'State' property.\n" +
                    "If the database is dirty, **make a copy of your files**, ensure all files in the directory are not Read-only," +
                    " open a PowerShell session as an admin," +
                    " and repair by using the following commands (change directories to the location of SRUDB.dat first):\r\n\r\n" +
                    "'esentutl.exe /r sru /i'\r\n'esentutl.exe /p SRUDB.dat'\r\n\r\n";
            }

            if (csv.IsNullOrEmpty() == false)
            {
                string outName;
                string outFile;

                StreamWriter swCsv;
                CsvWriter csvWriter;

                try
                {
                    outName = $"{ts:yyyyMMddHHmmss}_SrumECmd_EnergyUsage_Output.csv";
                    outFile = Path.Combine(csv, outName);

                    swCsv = new StreamWriter(outFile, false, Encoding.UTF8);
                    csvWriter = new CsvWriter(swCsv, CultureInfo.InvariantCulture);

                    var foo = csvWriter.Context.AutoMap<EnergyUsage>();

                    foo.Map(t => t.Timestamp).Convert(t =>
                        $"{t.Value.Timestamp:yyyy-MM-dd HH:mm:ss}");
                    foo.Map(t => t.EventTimestamp).Convert(t =>
                        $"{t.Value.EventTimestamp?.ToString(dt)}");

                    csvWriter.Context.RegisterClassMap(foo);
                    csvWriter.WriteHeader<EnergyUsage>();
                    csvWriter.NextRecord();

                    csvWriter.WriteRecords(sr.EnergyUsages.Values);

                    csvWriter.Flush();
                    swCsv.Flush();
                }
                catch (Exception e)
                {
                    host.UpdateHostDetails($"Error exporting 'EnergyUsage' data! Error: {e.Message}");
                }

                try
                {
                    outName = $"{ts:yyyyMMddHHmmss}_SrumECmd_Unknown312_Output.csv";
                    outFile = Path.Combine(csv, outName);

                    swCsv = new StreamWriter(outFile, false, Encoding.UTF8);
                    csvWriter = new CsvWriter(swCsv, CultureInfo.InvariantCulture);

                    var foo = csvWriter.Context.AutoMap<TimelineProvider>();

                    foo.Map(t => t.Timestamp).Convert(t =>
                        $"{t.Value.Timestamp:yyyy-MM-dd HH:mm:ss}");
                    foo.Map(t => t.EndTime).Convert(t =>
                        $"{t.Value.EndTime.ToString(dt)}");

                    csvWriter.Context.RegisterClassMap(foo);
                    csvWriter.WriteHeader<TimelineProvider>();
                    csvWriter.NextRecord();

                    csvWriter.WriteRecords(sr.TimelineProviders.Values);

                    csvWriter.Flush();
                    swCsv.Flush();
                }
                catch (Exception e)
                {
                    host.UpdateHostDetails($"Error exporting 'Unknown312' data! Error: {e.Message}");
                }

                try
                {
                    outName = $"{ts:yyyyMMddHHmmss}_SrumECmd_UnknownD8F_Output.csv";
                    outFile = Path.Combine(csv, outName);

                    swCsv = new StreamWriter(outFile, false, Encoding.UTF8);
                    csvWriter = new CsvWriter(swCsv, CultureInfo.InvariantCulture);

                    var foo = csvWriter.Context.AutoMap<Vfuprov>();

                    foo.Map(t => t.Timestamp).Convert(t =>
                        $"{t.Value.Timestamp:yyyy-MM-dd HH:mm:ss}");
                    foo.Map(t => t.EndTime).Convert(t =>
                        $"{t.Value.EndTime.ToString(dt)}");
                    foo.Map(t => t.StartTime).Convert(t =>
                        $"{t.Value.StartTime.ToString(dt)}");

                    csvWriter.Context.RegisterClassMap(foo);
                    csvWriter.WriteHeader<Vfuprov>();
                    csvWriter.NextRecord();

                    csvWriter.WriteRecords(sr.Vfuprovs.Values);

                    csvWriter.Flush();
                    swCsv.Flush();
                }
                catch (Exception e)
                {
                    host.UpdateHostDetails($"Error exporting 'UnknownD8F' data! Error: {e.Message}");
                }

                try
                {
                    outName = $"{ts:yyyyMMddHHmmss}_SrumECmd_AppResourceUseInfo_Output.csv";
                    outFile = Path.Combine(csv, outName);

                    swCsv = new StreamWriter(outFile, false, Encoding.UTF8);
                    csvWriter = new CsvWriter(swCsv, CultureInfo.InvariantCulture);

                    var foo = csvWriter.Context.AutoMap<AppResourceUseInfo>();

                    foo.Map(t => t.Timestamp).Convert(t =>
                        $"{t.Value.Timestamp:yyyy-MM-dd HH:mm:ss}");

                    csvWriter.Context.RegisterClassMap(foo);
                    csvWriter.WriteHeader<AppResourceUseInfo>();
                    csvWriter.NextRecord();

                    csvWriter.WriteRecords(sr.AppResourceUseInfos.Values);

                    csvWriter.Flush();
                    swCsv.Flush();
                }
                catch (Exception e)
                {
                    host.UpdateHostDetails($"Error exporting 'AppResourceUseInfo' data! Error: {e.Message}");
                }

                try
                {
                    outName = $"{ts:yyyyMMddHHmmss}_SrumECmd_NetworkConnections_Output.csv";

                    outFile = Path.Combine(csv, outName);
                    swCsv = new StreamWriter(outFile, false, Encoding.UTF8);

                    csvWriter = new CsvWriter(swCsv, CultureInfo.InvariantCulture);

                    var foo = csvWriter.Context.AutoMap<NetworkConnection>();

                    foo.Map(t => t.Timestamp).Convert(t =>
                        $"{t.Value.Timestamp:yyyy-MM-dd HH:mm:ss}");
                    foo.Map(t => t.ConnectStartTime).Convert(t =>
                        $"{t.Value.ConnectStartTime.ToString(dt)}");

                    csvWriter.Context.RegisterClassMap(foo);
                    csvWriter.WriteHeader<NetworkConnection>();
                    csvWriter.NextRecord();

                    csvWriter.WriteRecords(sr.NetworkConnections.Values);

                    csvWriter.Flush();
                    swCsv.Flush();
                }
                catch (Exception e)
                {
                    host.UpdateHostDetails($"Error exporting 'NetworkConnection' data! Error: {e.Message}");
                }

                try
                {
                    outName = $"{ts:yyyyMMddHHmmss}_SrumECmd_NetworkUsages_Output.csv";
                    outFile = Path.Combine(csv, outName);

                    swCsv = new StreamWriter(outFile, false, Encoding.UTF8);
                    csvWriter = new CsvWriter(swCsv, CultureInfo.InvariantCulture);

                    var foo = csvWriter.Context.AutoMap<NetworkUsage>();

                    foo.Map(t => t.Timestamp).Convert(t =>
                        $"{t.Value.Timestamp:yyyy-MM-dd HH:mm:ss}");

                    csvWriter.Context.RegisterClassMap(foo);
                    csvWriter.WriteHeader<NetworkUsage>();
                    csvWriter.NextRecord();

                    csvWriter.WriteRecords(sr.NetworkUsages.Values);

                    csvWriter.Flush();
                    swCsv.Flush();
                }
                catch (Exception e)
                {
                    host.UpdateHostDetails($"Error exporting 'NetworkUsage' data! Error: {e.Message}");
                }

                try
                {
                    outName = $"{ts:yyyyMMddHHmmss}_SrumECmd_PushNotifications_Output.csv";
                    outFile = Path.Combine(csv, outName);

                    swCsv = new StreamWriter(outFile, false, Encoding.UTF8);
                    csvWriter = new CsvWriter(swCsv, CultureInfo.InvariantCulture);

                    var foo = csvWriter.Context.AutoMap<PushNotification>();

                    foo.Map(t => t.Timestamp).Convert(t =>
                        $"{t.Value.Timestamp:yyyy-MM-dd HH:mm:ss}");

                    csvWriter.Context.RegisterClassMap(foo);
                    csvWriter.WriteHeader<PushNotification>();
                    csvWriter.NextRecord();

                    csvWriter.WriteRecords(sr.PushNotifications.Values);

                    csvWriter.Flush();
                    swCsv.Flush();
                }
                catch (Exception e)
                {
                    host.UpdateHostDetails($"Error exporting 'PushNotification' data! Error: {e.Message}");
                }
            }

            return $"Srum information parsed in: {toDirectory}";
        }

        private static string GetAbsolutePathFromTargetIDs(List<Lnk.ShellItems.ShellBag> ids)
        {
            if (ids == null)
            {
                return "(No target IDs present)";
            }

            string absPath = string.Empty;

            if (ids.Count == 0)
            {
                return absPath;
            }

            foreach (Lnk.ShellItems.ShellBag shellBag in ids)
            {
                absPath += shellBag.Value + @"\";
            }

            absPath = absPath[0..^1];

            return absPath;
        }

    }
}
