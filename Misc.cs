using System.IO;
using System.Collections.Generic;

namespace Spi
{
    class Misc
    {
        public static IEnumerable<string> ReadLines(string FilenameWithFiles)
        {
            using (TextReader rdr = new StreamReader(FilenameWithFiles, detectEncodingFromByteOrderMarks: true))
            {
                string line;
                while ((line = rdr.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
        public static string GetLongFilenameNotation(string Filename)
        {
            if (Filename.StartsWith(@"\\?\"))
            {
                return Filename;
            }

            if (Filename.Length >= 2 && Filename[1] == ':')
            {
                return @"\\?\" + Filename;
            }
            else if (Filename.StartsWith(@"\\") && !Filename.StartsWith(@"\\?\"))
            {
                return @"\\?\UNC\" + Filename.Remove(0, 2);
            }
            return Filename;
        }
    }
}
