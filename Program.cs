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

                string FullSrcDir = Misc.GetLongFilenameNotation(Path.GetFullPath(opts.SrcBase));
                string FullTrgDir = Misc.GetLongFilenameNotation(Path.GetFullPath(opts.TrgBase));

                bool hasErrors = false;
                ulong copiedCount = 0;
                ulong errorCount = 0;
                using (TextWriter cpWriter = new StreamWriter(@".\cpCopied.txt", append: false, encoding: System.Text.Encoding.UTF8))
                using (TextWriter errorWriter = new StreamWriter(@".\cpError.txt", append:false, encoding: System.Text.Encoding.UTF8))
                {
                    foreach (string relativeFilename in Misc.ReadLines(opts.FilenameWithFiles))
                    {
                        hasErrors = CopyFiles.Run(relativeFilename, FullSrcDir, FullTrgDir,
                            OnCopy:       (filename)                    => { cpWriter.WriteLine(filename); copiedCount += 1; },
                            OnWin32Error: (LastErrorCode, Api, Message) => { errorWriter.WriteLine($"{LastErrorCode}\t{Api}\t{Message}"); errorCount += 1; },
                            dryrun:       opts.dryrun);
                    }
                    if (!opts.dryrun)
                    {
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
