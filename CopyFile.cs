using System;
using System.IO;
using System.Runtime.InteropServices;

namespace cp
{
    class CopyFile
    {
        public static bool Run(string relativeFilename, UInt64? filesize, string srcDir, string trgDir, Action<string, UInt64?> OnCopy,
            Spi.Native.Win32ApiErrorCallback OnWin32Error, bool dryrun)
        {
            string FullSrc = Path.Combine(srcDir, relativeFilename);
            string FullTrg = Path.Combine(trgDir, relativeFilename);

            if (dryrun)
            {
                Console.Out.WriteLine($"\"{FullSrc}\" \"{FullTrg}\"");
                return true;
            }

            bool ok = DoCopyFile(FullSrc, FullTrg, OnWin32Error);
            if (ok)
            {
                OnCopy?.Invoke(relativeFilename, filesize);
            }

            return ok;
        }
        public static bool DoCopyFile(string FullSrc, string FullTrg, Spi.Native.Win32ApiErrorCallback OnWin32Error)
        {
            if ( Spi.Native.CopyFile(lpExistingFileName: FullSrc, lpNewFileName: FullTrg, bFailIfExists: false) )
            {
                return true;
            }

            bool ok = false;
            int LastError = Marshal.GetLastWin32Error();
            if ( LastError == (int)Spi.Native.Win32Error.ERROR_PATH_NOT_FOUND )
            {
                //string FullTargetDirectoryname = System.IO.Path.GetDirectoryName(FullTrg);
                string FullTargetDirectoryname = Spi.Misc.GetDirectoryName(FullTrg);
                if ( ! Spi.Misc.CreatePath(FullTargetDirectoryname, OnWin32Error))
                {
                    OnWin32Error?.Invoke(Marshal.GetLastWin32Error(), "CreateDirectoryW", FullTargetDirectoryname);
                    return false;
                }
            }
            else if ( LastError == (int)Spi.Native.Win32Error.ERROR_ACCESS_DENIED )
            {
                if ( ! Spi.Native.SetFileAttributes(FullTrg, Spi.FileAttributes.Normal))
                {
                    OnWin32Error?.Invoke(Marshal.GetLastWin32Error(), "SetFileAttributesW", FullTrg);
                    return false;
                }
            }

            if (Spi.Native.CopyFile(lpExistingFileName: FullSrc, lpNewFileName: FullTrg, bFailIfExists: false))
            {
                ok = true;
            }

            if ( !ok )
            {
                OnWin32Error?.Invoke(Marshal.GetLastWin32Error(), "CopyFileW", $"\"{FullSrc}\" \"{FullTrg}\"");
            }

            return ok;
        }
    }
}
