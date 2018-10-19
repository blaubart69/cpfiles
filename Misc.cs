using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Spi
{
    public class Misc
    {
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
        public static bool CreatePath(string PathToCreate, Native.Win32ApiErrorCallback OnWin32Error, Action OnCreateDirectory)
        {
            if (IsDirectory(PathToCreate))
            {
                return true;
            }
            if (Spi.Native.CreateDirectoryW(PathToCreate, IntPtr.Zero))
            {
                return true;
            }
            OnCreateDirectory?.Invoke();

            bool ok = false;
            if (Marshal.GetLastWin32Error() == (int)Native.Win32Error.ERROR_PATH_NOT_FOUND)
            {
                // not found. try to create the parent dir.
                int LastPos = PathToCreate.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
                if (LastPos != -1)
                {
                    if (ok = CreatePath(PathToCreate.Substring(0, LastPos), OnWin32Error, OnCreateDirectory))
                    {
                        // parent dir exist/was created
                        ok = Spi.Native.CreateDirectoryW(PathToCreate, IntPtr.Zero);
                        OnCreateDirectory?.Invoke();
                        if ( !ok )
                        {
                            if (Marshal.GetLastWin32Error() == 183) // ERROR_ALREADY_EXISTS
                            {
                                ok = true;
                            }
                            else
                            {
                                OnWin32Error?.Invoke(Marshal.GetLastWin32Error(), "CreateDirectoryW", PathToCreate);
                            }
                        }
                    }
                }
            }
            else if (Marshal.GetLastWin32Error() == 183) // ERROR_ALREADY_EXISTS
            {
                ok = true;
            }
            else
            {
                OnWin32Error?.Invoke(Marshal.GetLastWin32Error(), "CreateDirectoryW", PathToCreate);
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
        public static string GetLastTsvColumn(string TsvValues)
        {
            int LastTab = TsvValues.LastIndexOf('\t');

            if ( LastTab == -1 )
            {
                return TsvValues;
            }
            else
            {
                return TsvValues.Substring(LastTab + 1);
            }
        }
        public static string GetPrettyFilesize(long Filesize)
        {
            return GetPrettyFilesize((ulong)Filesize);
        }
        public static string GetPrettyFilesize(ulong Filesize)
        {
            StringBuilder sb = new StringBuilder(32);
            Spi.Native.StrFormatByteSize((long)Filesize, sb, 32);
            return sb.ToString();
        }
        public static string NiceDuration(TimeSpan ts)
        {
            string res;
            if (ts.TotalHours >= 24)
            {
                res = String.Format("{0}d {1}h {2}m {3}s {4}ms", ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            }
            else if (ts.TotalMinutes >= 60)
            {
                res = String.Format("{0}h {1}m {2}s {3}ms", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            }
            else if (ts.TotalSeconds >= 60)
            {
                res = String.Format("{0}m {1}s {2}ms", ts.Minutes, ts.Seconds, ts.Milliseconds);
            }
            else if (ts.TotalMilliseconds >= 1000)
            {
                res = String.Format("{0}s {1}ms", ts.Seconds, ts.Milliseconds);
            }
            else
            {
                res = String.Format("{0}ms", ts.Milliseconds);
            }
            return res;
        }
    }
}
