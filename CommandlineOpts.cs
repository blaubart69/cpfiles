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
    }
    class CommandlineOpts
    {
        static void ShowHelp(Mono.Options.OptionSet p)
        {
            Console.WriteLine("Usage: cp {sourceBase} {targetBase} {filename} [OPTIONS]");
            Console.WriteLine("copies files given in a file");
            Console.WriteLine();
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Error);
        }
        public static Opts GetOpts(string[] args)
        {
            bool show_help = false;
            Opts tmpOpts = new Opts();
            Opts resultOpts = null;
            var p = new Mono.Options.OptionSet() {
                { "n|dryrun",   "show what would be copied",                                           v => tmpOpts.dryrun = (v != null) },
                { "h|help",     "show this message and exit",                                          v => show_help = v != null }            };

            try
            {
                List<string> parsedArgs = p.Parse(args);
                if (parsedArgs.Count != 3)
                {
                    Console.Error.WriteLine("only one filename with filenames within the file");
                    show_help = true;
                }
                else
                {
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
            }
            return resultOpts;
        }
    }
}
