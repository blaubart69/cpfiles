using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cp
{
    class Opts
    {
        public string FilenameWithFiles;
        public string SrcBase;
        public string TrgBase;
        public bool   dryrun;
        public int    MaxThreads = 16;
    }
    class CommandlineOpts
    {
        static void ShowHelp(Mono.Options.OptionSet p)
        {
            Console.Error.WriteLine("Usage: cp [OPTIONS] {sourceBase} {targetBase} {filename}"
            + "\ncopies files given by name in a textfile relative to {sourceBase} and {targetBase}."
            + "\n- when the lines are tab-separated the last column is used as filename."
            + "\n- when the columen left next to the filename can by converted to a number, it is treated as the filesize to show the progress"
            );
            Console.Error.WriteLine("\nOptions:");
            p.WriteOptionDescriptions(Console.Error);
        }
        public static Opts GetOpts(string[] args)
        {
            bool show_help = false;
            Opts tmpOpts = new Opts();
            Opts resultOpts = null;
            var p = new Mono.Options.OptionSet() {
                { "t=|threads",  $"how many copies should be run in parallel (default: {tmpOpts.MaxThreads})",
                                                                                v => tmpOpts.MaxThreads = Convert.ToInt32(v)  },
                { "n|dryrun",   "show what would be copied",                    v => tmpOpts.dryrun = (v != null)             },
                { "h|help",     "show this message and exit",                   v => show_help = v != null }                  };

            try
            {
                List<string> parsedArgs = p.Parse(args);
                if (parsedArgs.Count != 3)
                {
                    show_help = true;
                }
                else
                {
                    resultOpts = tmpOpts;
                    resultOpts.SrcBase = parsedArgs[0];
                    resultOpts.TrgBase = parsedArgs[1];
                    resultOpts.FilenameWithFiles = parsedArgs[2];
                }
            }
            catch (Mono.Options.OptionException oex)
            {
                Console.WriteLine(oex.Message);
            }
            if (show_help)
            {
                ShowHelp(p);
                return null;
            }
            return resultOpts;
        }
    }
}
