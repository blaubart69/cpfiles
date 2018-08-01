﻿using System;
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
        public Int64 copiedCount;
        public Int64 errorsCount;
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

                using (TextWriter errorWriter = TextWriter.Synchronized(new StreamWriter(@".\cpError.txt",  append: false, encoding: System.Text.Encoding.UTF8)))
                {
                    copyFileTask = StartCopyFiles(FullSrcDir, FullTrgDir,
                        files: File.ReadLines(opts.FilenameWithFiles),
                        OnCopy: (filename) =>
                        {
                            Interlocked.Increment(ref stats.copiedCount);
                        },
                        OnWin32Error: (LastErrorCode, Api, Message) =>
                        {
                            errorWriter.WriteLine($"{LastErrorCode}\t{Api}\t{Message}");
                            Interlocked.Increment(ref stats.errorsCount);
                        },
                        MaxThreads: opts.MaxThreads,
                        dryRun:     opts.dryrun);

                    WaitUntilFinished(copyFileTask, 2000, () => Console.Error.WriteLine($"copied: {stats.copiedCount}, errors: {stats.errorsCount}") );
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
        static void WaitUntilFinished(Task TaskToWaitFor, int milliseconds, Action ExecEvery)
        {
            while ( ! TaskToWaitFor.Wait(milliseconds) )
            {
                ExecEvery();
            }
            ExecEvery();
        }
        static Task<bool> StartCopyFiles(
            string FullSrc, string FullTrg, IEnumerable<string> files, 
            Action<string> OnCopy, Native.Win32ApiErrorCallback OnWin32Error,
            int MaxThreads,
            bool dryRun)
        {
            return
                Task.Run(() =>
                {
                    bool hasErrors = false;

                    Parallel.ForEach(
                        source: files,
                        parallelOptions: new ParallelOptions() { MaxDegreeOfParallelism = MaxThreads },
                        body: (relativeFilename) =>
                        {
                            if (!CopyFile.Run(relativeFilename, FullSrc, FullTrg, OnCopy, OnWin32Error, dryRun))
                            {
                                hasErrors = true;
                            }
                        });

                    return hasErrors;
                });
        }
    }
}
