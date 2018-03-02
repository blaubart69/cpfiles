using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Spi;

namespace cp
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                Opts opts;
                if ((opts = CommandlineOpts.GetOpts(args)) == null)
                {
                    return 1;
                }

                bool hasErrors = false;
                {
                    ulong copiedCount = 0;
                    ulong errorCount = 0;
                    using (var cpWriter = new ConsoleAndFileWriter(@".\cpCopied.txt"))
                    using (var errorWriter = new ConsoleAndFileWriter(@".\cpError.txt"))
                    {
                        foreach (string relativeFilename in Misc.ReadLines(opts.FilenameWithFiles))
                        {
                            hasErrors = CopyFiles.Run(relativeFilename, opts.SrcBase, opts.TrgBase,
                               OnCopy:       (filename) => { cpWriter.WriteLine(filename); copiedCount += 1; },
                               OnWin32Error: (LastErrorCode, Api, Message) => { errorWriter.WriteLine($"{LastErrorCode}\t{Api}\t{Message}"); errorCount += 1; },
                               dryrun:       opts.dryrun);
                        }
                        Console.Error.WriteLine($"copied: {copiedCount}, errors: {errorCount}");
                    }
                }

                return hasErrors ? 8 : 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Na so wirst ned ooohhooiidd... so dastehst Di boohooid...");
                ConsoleAndFileWriter.WriteException(ex, Console.Error);
                return 999;
            }
        }
    }
}
