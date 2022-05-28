using Marvel.Enum;
using Marvel.Model;
using Marvel.Utilities;
using Prefetch;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Marvel.Commands
{
    public class ExecutableExtractor
    {
        private const string EXE_FORMAT_LOWER_CASE = ".exe";
        private const string EXE_FORMAT_UPPER_CASE = ".EXE";
        private const string PF_FORMAT = "*.pf";
        private const string PREFETCH_FOLDER = @"\Prefetch";
        private const string PREFETCH_DIRECTORY = @"C:\Windows\Prefetch";
        private const string C_DIRECTORY = "C:";

        public string ExtractFiles(Host host, string toDirectory, ProtocolsEnum selectedProtocol)
        {
            string folderName = host.IP.Replace('.', '_');
            string path = toDirectory + '\\' + folderName;

            DirectoryInfo hostDirectoryInfo = Directory.CreateDirectory(path);

            PrefetchExtractor(host, path, selectedProtocol);

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

            IPrefetch pf;

            foreach (string file in Directory.GetFiles(toDirectory, PF_FORMAT))
            {
                try
                {
                    pf = PrefetchFile.Open(file);
                    string executableName = pf.Header.ExecutableFilename;
                    string executablePath = "";

                    foreach (var fileName in pf.Filenames)
                    {
                        if (!fileName.Contains(EXE_FORMAT_LOWER_CASE) && !fileName.Contains(EXE_FORMAT_UPPER_CASE))
                        {
                            continue;
                        }

                        if (fileName.Contains(executableName))
                        {
                            executablePath += fileName;

                            int indexToSplit = executablePath.IndexOf('}') + 1;
                            executablePath = C_DIRECTORY + executablePath[indexToSplit..];
                            break;
                        }
                    }

                    CommandUtilities.Instance.RunCommand(selectedProtocol, host, executablePath, toDirectory, CommandsEnum.ReceiveItem);
                    //PrefetchFile.SavePrefetch(path, pf);
                }
                catch (Exception e)
                {
                    continue;
                }
                finally
                {
                    File.Delete(file);
                }



                //PrefetchFile.SavePrefetch(@"D:\Ruppin\Year D\FinalProject\Marvel\MarvelPrefetch\text.csv", pf);

                //csv.WriteRecords(pf.Filenames);

                //string json = JsonSerializer.Serialize(pf);
                //File.WriteAllText(@"path.json", json);





                //pf.Should().NotBe(null);

                //pf.Filenames.Count.Should().Be(pf.FileMetricsCount);

                //pf.VolumeCount.Should().Be(pf.VolumeInformation.Count);

                //pf.Header.Signature.Should().Be("SCCA");
            }

            return $"Prefetch executables downloaded to: {toDirectory}";
        }
    }
}
