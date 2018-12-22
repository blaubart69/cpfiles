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
        public long CreateDirectoryCalled;

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

                int checkArgs = 0;
                if (!File.Exists(opts.FilenameWithFiles))
                {
                    Console.Error.WriteLine($"input file does not exists. [{opts.FilenameWithFiles}]");
                    checkArgs = 2;
                }

                if ( !Misc.IsDirectory(FullSrcDir) )
                {
                    Console.Error.WriteLine($"source base is not a directory. [{FullSrcDir}]");
                    checkArgs += 2;
                }

                if (!Misc.IsDirectory(FullTrgDir))
                {
                    Console.Error.WriteLine($"target base is not a directory. [{FullTrgDir}]");
                    checkArgs += 2;
                }

                if (checkArgs > 0)
                {
                    return checkArgs;
                }

                Console.Error.WriteLine($"I: setting ThreadPool.SetMinThreads() to {opts.MaxThreads}");
                ThreadPool.SetMinThreads(opts.MaxThreads, opts.MaxThreads);


                Console.Error.WriteLine(
                    $"source base: {FullSrcDir}\n"
                 +  $"target base: {FullTrgDir}");

                Stats stats = new Stats();
                Task<bool> copyFileTask = null;

                Task.Run(() => CopyFiles.ReadFilesAndSizes(opts.FilenameWithFiles, out stats.totalFiles, out stats.totalBytes)).ConfigureAwait(false);

                DateTime started = DateTime.Now;
                using (TextWriter errorWriter = TextWriter.Synchronized(new StreamWriter(@".\cpError.txt",  append: false, encoding: System.Text.Encoding.UTF8)))
                {
                    copyFileTask = CopyFiles.Start(FullSrcDir, FullTrgDir,
                        files: CopyFiles.ReadInputfile( CopyFiles.ReadLines(opts.FilenameWithFiles) ),
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
                        OnDirectoryCreated: () =>
                        {
                            Interlocked.Increment(ref stats.CreateDirectoryCalled);
                        },
                        MaxThreads: opts.MaxThreads,
                        dryRun:     opts.dryrun);

                    Spi.StatusLineWriter linewriter = new StatusLineWriter(Console.Error);
                    DateTime StartTime = DateTime.Now;
                    WaitUntilFinished(copyFileTask, 2000, 
                        ExecEvery: () => linewriter.Write( GetStatsLine(stats, StartTime) ), 
                        ExecAtEnd: () => Console.Error.WriteLine() );
                }

                WriteEndReport(started, stats);

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
        private static void WriteEndReport(DateTime started, Stats stats)
        {
            DateTime ended = DateTime.Now;

            string copiedBytes = stats.totalBytes.HasValue ? Misc.GetPrettyFilesize(stats.copiedBytes) : "n/a";

            Console.WriteLine(
                  $"\nTime elapsed:   {Misc.NiceDuration(ended-started)}"
                + $"\ncopied files:   {stats.copiedCount:N0} ({copiedBytes})"
                + $"\nerrors:         {stats.errorsCount:N0}"
                + $"\nCreDirCalled:   {stats.CreateDirectoryCalled:N0}"
                );
        }

        static string GetStatsLine(Stats stats, DateTime StartingTime)
        {
            TimeSpan elapsed = DateTime.Now - StartingTime;
            string AvgSpeedPerSec = "n/a";
            ulong ETAseconds = 0;
            ulong AvgBytesPerSec = 0;

            if ( elapsed.Ticks > 0)
            {
                AvgBytesPerSec = Convert.ToUInt64((double)stats.copiedBytes / elapsed.TotalSeconds);
                AvgSpeedPerSec = Misc.GetPrettyFilesize(AvgBytesPerSec);
            }

            string bytes = "n/a";
            if ( stats.totalBytes.HasValue )
            {
                bytes = String.Format("copied/done/all - {0}/{1}%/{2}",
                    Misc.GetPrettyFilesize((ulong)stats.copiedBytes),
                    GetPercentString(stats.totalBytes.Value, (ulong)stats.copiedBytes),
                    Misc.GetPrettyFilesize(stats.totalBytes.Value)
                    );

                if (AvgBytesPerSec > 0)
                {
                    ulong remainigBytesToCopy = stats.totalBytes.Value - (ulong)stats.copiedBytes;
                    ETAseconds = remainigBytesToCopy / AvgBytesPerSec;
                }
            }

            string donePercent = GetPercentString(stats.totalFiles, (ulong)stats.copiedCount);

            string ETA = ETAseconds == 0 ? "n/a" : Misc.NiceDuration(TimeSpan.FromSeconds((double)ETAseconds)); 

            return
                $"copied/done/all/err/CreDir - {stats.copiedCount:N0}/{donePercent}%/{stats.totalFiles:N0}/{stats.errorsCount:N0}/{stats.CreateDirectoryCalled:N0}"
              + $" | bytes: {bytes} | avg/s: {AvgSpeedPerSec} | ETA: {ETA} | elapsed: {Misc.NiceDuration(elapsed)}";
        }
        static void WaitUntilFinished(Task TaskToWaitFor, int milliseconds, Action ExecEvery, Action ExecAtEnd)
        {
            while ( ! TaskToWaitFor.Wait(milliseconds) )
            {
                ExecEvery();
            }
            ExecEvery();
            ExecAtEnd?.Invoke();
        }
        static string GetPercentString(ulong total, ulong done)
        {
            string result = "n/a";
            if (total > 0)
            {
                float donePercent = (float)done / (float)total * 100;
                result = $"{donePercent:0.##}";
            }
            else
            {
                result = "n/a";
            }

            return result;
        }
    }
}
