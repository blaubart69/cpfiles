using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace cp
{
    class CopyFiles
    {
        public delegate void Win32ApiErrorCallback(int LastError, string Apiname, string Text);

        public static bool Run(string relativeFilename, string srcDir, string trgDir, Action<string> OnCopy, Win32ApiErrorCallback OnWin32Error, bool dryrun)
        {
            string FullSrc = Path.Combine(srcDir, relativeFilename);
            string FullTrg = Path.Combine(trgDir, relativeFilename);

            if (dryrun)
            {
                Console.Out.WriteLine($"[{FullSrc}]<<>>[{FullTrg}]");
                return true;
            }

            if ( Native.CopyFile(lpExistingFileName: FullSrc, lpNewFileName: FullTrg, bFailIfExists: false) )
            {
                OnCopy?.Invoke(relativeFilename);
                return true;
            }

            int LastError = Marshal.GetLastWin32Error();

            OnWin32Error?.Invoke(LastError, "CopyFileW", $"{FullSrc}<<>>{FullTrg}");

            return false;
        }
    }
}
