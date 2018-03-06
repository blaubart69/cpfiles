using System.IO;
using System.Collections.Generic;
using System;

namespace Spi
{
    public class Misc
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
        public static bool CreatePath(string PathToCreate, Spi.Native.Win32ApiErrorCallback OnWin32Error)
        {
            if (IsDirectory(PathToCreate))
            {
                return true;
            }
            if (Spi.Native.CreateDirectoryW(PathToCreate, IntPtr.Zero))
            {
                return true;
            }

            bool ok = false;
            if (System.Runtime.InteropServices.Marshal.GetLastWin32Error() == (int)Native.Win32Error.ERROR_PATH_NOT_FOUND)
            {
                // not found. try to create the parent dir.
                int LastPos = PathToCreate.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
                if (LastPos != -1)
                {
                    if (ok = CreatePath(PathToCreate.Substring(0, LastPos), OnWin32Error))
                    {
                        // parent dir exist/was created
                        ok = Spi.Native.CreateDirectoryW(PathToCreate, IntPtr.Zero);
                        if ( !ok)
                        {
                            OnWin32Error?.Invoke(System.Runtime.InteropServices.Marshal.GetLastWin32Error(), "CreateDirectoryW", PathToCreate);
                        }
                    }
                }
            }
            else
            {
                OnWin32Error?.Invoke(System.Runtime.InteropServices.Marshal.GetLastWin32Error(), "CreateDirectoryW", PathToCreate);
            }
            return ok;
        }
        public static bool IsDirectory(string dir)
        {
            uint rc = Spi.Native.GetFileAttributesW(dir);

            if (rc == uint.MaxValue)
            {
                //int LastError = Spi.Win32.GetLastWin32Error();
                return false;   // doesn't exist
            }
            /*
            FILE_ATTRIBUTE_DIRECTORY
            16 (0x10)
            The handle that identifies a directory.
            */
            //return (rc & 0x10) != 0;
            return (rc & (uint)Spi.FileAttributes.Directory) != 0;
        }
        public static string GetDirectoryName(string filename)
        {
            int lastIdxBackslash = filename.LastIndexOf('\\');
            if (lastIdxBackslash > 0)
            {
                return filename.Substring(0, lastIdxBackslash);
            }
            else
            {
                return filename;
            }
        }
    }
}
