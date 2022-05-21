using Marvel.Enum;
using Marvel.Model;
using Marvel.Utilities;
using Prefetch;
using System.IO;
using System.Threading.Tasks;

namespace Marvel.Commands
{
    class ExecutableExtractor
    {
        public string ExtractFiles(Host host, string fromDirectory, string toDirectory, ProtocolsEnum selectedProtocol)
        {
            string folderName = host.IP.Replace('.', '_');
            string path = toDirectory + '\\' + folderName;

            DirectoryInfo hostDirectoryInfo = Directory.CreateDirectory(path);

            PrefetchExtractor(host, fromDirectory, path, selectedProtocol);

            return $"Executable files downloaded to: {path}";
        }

        public string PrefetchExtractor(Host host, string fromDirectory, string toDirectory, ProtocolsEnum selectedProtocol)
        {
            string path = toDirectory + @"\Prefetch";
            DirectoryInfo pathInfo = Directory.CreateDirectory(path);

            Task continuation = CommandUtilities.Instance.RunCommand(selectedProtocol, host, fromDirectory, path, CommandsEnum.GetFolder);
            continuation.Wait();

            IPrefetch pf;

            foreach (var file in Directory.GetFiles(path, "*.pf"))
            {
                try
                {
                    pf = PrefetchFile.Open(file);
                    string executableName = pf.Header.ExecutableFilename;
                    string executablePath = "";

                    foreach (var fileName in pf.Filenames)
                    {
                        if (!fileName.Contains(".exe") && !fileName.Contains(".EXE"))
                        {
                            continue;
                        }

                        if (fileName.Contains(executableName))
                        {
                            executablePath += fileName;

                            int indexToSplit = executablePath.IndexOf('}') + 1;
                            executablePath = @"C:" + executablePath[indexToSplit..];
                            break;
                        }
                    } 

                    CommandUtilities.Instance.RunCommand(selectedProtocol, host, executablePath, path, CommandsEnum.ReceiveItem);
                    //PrefetchFile.SavePrefetch(path, pf);
                }
                catch
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

            return $"Prefetch executables downloaded to: {path}";
        }
    }
}
