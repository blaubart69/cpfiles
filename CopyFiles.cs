using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cp
{
    public struct CopyItem
    {
        public readonly string  relativeFilename;
        public readonly UInt64? filesize;

        public CopyItem(string relativeFilename, UInt64? filesize)
        {
            this.relativeFilename = relativeFilename;
            this.filesize = filesize;
        }
    }
    public class CopyFiles
    {
        public static Task<bool> Start(
            string FullSrc, string FullTrg, IEnumerable<CopyItem> files,
            Action<string,UInt64?> OnCopy,
            Action OnDirectoryCreated,
            Spi.Native.Win32ApiErrorCallback OnWin32Error,
            int MaxThreads,
            bool dryRun)
        {
            Console.Error.WriteLine($"starting with MaxDegreeOfParallelism of {MaxThreads}");

            return
                Task.Run(() =>
                {
                    bool hasErrors = false;

                    Parallel.ForEach(
                        source: files,
                        parallelOptions: new ParallelOptions() { MaxDegreeOfParallelism = MaxThreads },
                        body: (itemToCopy) =>
                        {
                            if (!CopyFile.Run(
                                itemToCopy.relativeFilename
                                , itemToCopy.filesize
                                , FullSrc
                                , FullTrg
                                , OnCopy
                                , OnDirectoryCreated
                                , OnWin32Error
                                , dryRun))
                            {
                                hasErrors = true;
                            }
                        });

                    return hasErrors;
                });
        }
        public static IEnumerable<CopyItem> ReadInputfile(IEnumerable<string> inputlines)
        {
            foreach (string line in inputlines)
            {
                string[] cols = line.Split('\t');
                if ( cols.Length == 1)
                {
                    yield return new CopyItem(line, null);
                }
                else if (cols.Length > 1)
                {
                    UInt64? filesize = null;
                    try
                    {
                        filesize = Convert.ToUInt64(cols[cols.Length - 2]);
                    }
                    catch { }

                    yield return new CopyItem(cols[cols.Length - 1], filesize);
                }
            }
        }
        public static void ReadFilesAndSizes(string filename, out UInt64 numberFiles, out UInt64? totalFilesize)
        {
            numberFiles = 0;
            totalFilesize = null;

            foreach (CopyItem ci in CopyFiles.ReadInputfile(System.IO.File.ReadLines(filename)))
            {
                ++numberFiles;

                if (ci.filesize.HasValue)
                {
                    if (!totalFilesize.HasValue)
                    {
                        totalFilesize = 0;
                    }
                    totalFilesize += ci.filesize.Value;
                }
            }
        }
    }
}
