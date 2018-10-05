using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Spi;

namespace cp
{
    struct Stats
    {
        public long copiedCount;
        public long errorsCount;

        public long  copiedBytes;

        public UInt64? totalBytes;
        public UInt64  totalFiles;
    }
    class Program
    {
        static int Main(string[] args)
        {
            int rc = 99;

            try
            {
                Opts opts;
                if ((opts = CommandlineOpts.GetOpts(args)) == null)
                {
                    return 1;
                }

                string FullSrcDir = Misc.GetLongFilenameNotation(Path.GetFullPath(opts.SrcBase));
                string FullTrgDir = Misc.GetLongFilenameNotation(Path.GetFullPath(opts.TrgBase));

                Stats stats = new Stats();
                Task<bool> copyFileTask = null;

                Task.Run(() => Misc.ReadFilesAndSizes(opts.FilenameWithFiles, out stats.totalFiles, out stats.totalBytes));

                using (TextWriter errorWriter = TextWriter.Synchronized(new StreamWriter(@".\cpError.txt",  append: false, encoding: System.Text.Encoding.UTF8)))
                {
                    copyFileTask = CopyFiles.Start(FullSrcDir, FullTrgDir,
                        files: CopyFiles.ReadInputfile( File.ReadLines(opts.FilenameWithFiles) ),
                        OnCopy: (string filename, UInt64? filesize) =>
                        {
                            Interlocked.Increment(ref stats.copiedCount);
                            if ( filesize.HasValue )
                            {
                                Interlocked.Add(ref stats.copiedBytes, (long)filesize.Value);
                            }
                        },
                        OnWin32Error: (LastErrorCode, Api, Message) =>
                        {
                            errorWriter.WriteLine($"{LastErrorCode}\t{Api}\t{Message}");
                            Interlocked.Increment(ref stats.errorsCount);
                        },
                        MaxThreads: opts.MaxThreads,
                        dryRun:     opts.dryrun);

                    WaitUntilFinished(copyFileTask, 2000, () => PrintStatsLine(stats) );
                }

                rc = copyFileTask.Result ? 8 : 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Na so wirst ned ooohhooiidd... so dastehst Di boohooid...");
                ConsoleAndFileWriter.WriteException(ex, Console.Error);
                rc = 999;
            }

            return rc;
        }
        static void PrintStatsLine(Stats stats)
        {
            string done = "n/a";
            if ( stats.totalFiles > 0 )
            {
                float fDone = (float)stats.copiedCount / (float)stats.totalFiles * 100;
                done = $"{fDone:0.##}";
            }

            Console.Error.WriteLine(
                $"copied: {stats.copiedCount:N0}, errors: {stats.errorsCount:N0}, done: {done}, all: {stats.totalFiles:N0}");
        }
        static void WaitUntilFinished(Task TaskToWaitFor, int milliseconds, Action ExecEvery)
        {
            while ( ! TaskToWaitFor.Wait(milliseconds) )
            {
                ExecEvery();
            }
            ExecEvery();
        }
        
    }
}
