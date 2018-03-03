using System;
using System.IO;


using System.Runtime.InteropServices;

namespace cp
{
    class CopyFiles
    {
        public static bool Run(string relativeFilename, string srcDir, string trgDir, Action<string> OnCopy, Spi.Native.Win32ApiErrorCallback OnWin32Error, bool dryrun)
        {
            string FullSrc = Path.Combine(srcDir, relativeFilename);
            string FullTrg = Path.Combine(trgDir, relativeFilename);

            if (dryrun)
            {
                Console.Out.WriteLine($"\"{FullSrc}\" \"{FullTrg}\"");
                return true;
            }

            if ( Spi.Native.CopyFile(lpExistingFileName: FullSrc, lpNewFileName: FullTrg, bFailIfExists: false) )
            {
                OnCopy?.Invoke(relativeFilename);
                return true;
            }

            bool ok = false;
            int LastError = Marshal.GetLastWin32Error();
            if ( LastError == (int)Spi.Native.Win32Error.ERROR_PATH_NOT_FOUND )
            {
                string FullTargetDirectoryname = System.IO.Path.GetDirectoryName(FullTrg);
                if ( ! Spi.Misc.CreatePath(FullTargetDirectoryname, OnWin32Error))
                {
                    OnWin32Error?.Invoke(Marshal.GetLastWin32Error(), "CreateDirectoryW", FullTargetDirectoryname);
                    return false;
                }

                if (Spi.Native.CopyFile(lpExistingFileName: FullSrc, lpNewFileName: FullTrg, bFailIfExists: false))
                {
                    OnCopy?.Invoke(relativeFilename);
                    ok = true;
                }
            }

            if ( !ok )
            {
                OnWin32Error?.Invoke(Marshal.GetLastWin32Error(), "CopyFileW", $"\"{FullSrc}\" \"{FullTrg}\"");
            }

            return ok;
        }
    }
}
