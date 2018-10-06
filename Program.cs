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

                Task.Run(() =>
                {
                    CopyFiles.ReadFilesAndSizes(opts.FilenameWithFiles, out stats.totalFiles, out stats.totalBytes);
                    //string strTotalBytes = stats.totalBytes.HasValue ? Misc.GetPrettyFilesize(stats.totalBytes.Value) : "n/a";
                    //Console.Error.WriteLine($"found files/filesize\t{stats.totalFiles:N0}/{strTotalBytes}");
                });

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

                    Spi.StatusLineWriter linewriter = new StatusLineWriter(Console.Error);
                    DateTime LastTime = DateTime.MinValue;
                    ulong LastCopiedBytes = 0;
                    WaitUntilFinished(copyFileTask, 2000, 
                        ExecEvery: () => linewriter.Write( GetStatsLine(stats, ref LastTime, ref LastCopiedBytes) ), 
                        ExecAtEnd: () => Console.Error.WriteLine() );
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
        static string GetStatsLine(Stats stats, ref DateTime LastTime, ref ulong LastCopiedBytes)
        {
            string bytes = "n/a";
            if ( stats.totalBytes.HasValue )
            {
                bytes = String.Format("copied/done/all - {0}/{1}%/{2}",
                    Misc.GetPrettyFilesize((ulong)stats.copiedBytes),
                    GetPercentString(stats.totalBytes.Value, (ulong)stats.copiedBytes),
                    Misc.GetPrettyFilesize(stats.totalBytes.Value)
                    );
            }

            string donePercent = GetPercentString(stats.totalFiles, (ulong)stats.copiedCount); ;

            return
                $"files: copied/done/errors/all - {stats.copiedCount:N0}/{donePercent}%/{stats.errorsCount:N0}/{stats.totalFiles:N0} | bytes: {bytes}";
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
