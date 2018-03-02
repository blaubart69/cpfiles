using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spi
{
    public class ConsoleAndFileWriter : IDisposable
    {
        private readonly    TextWriter              ConsoleWriter;
        private             TextWriter              FileWriter;
        private readonly    string                  Filename;
        private readonly    System.Text.Encoding    _encoding;

        public ConsoleAndFileWriter(TextWriter ConsoleWriter, string Filename, Encoding encoding)
        {
            this.ConsoleWriter = ConsoleWriter;
            this.Filename = Filename;
            this._encoding = encoding;
        }
        public ConsoleAndFileWriter(TextWriter ConsoleWriter, string Filename)
            : this(ConsoleWriter, Filename, Encoding.UTF8)
        {
        }
        public ConsoleAndFileWriter(string Filename)
            : this(null, Filename, Encoding.UTF8)
        {
        }
        public static void WriteException(Exception ex, TextWriter writer)
        {
            writer.WriteLine(ex.Message);
            writer.WriteLine(ex.StackTrace);
            if (ex.InnerException != null)
            {
                writer.WriteLine("--- inner exception ---");
                writer.WriteLine(ex.InnerException.Message);
                writer.WriteLine(ex.InnerException.StackTrace);
                writer.WriteLine("--- inner exception ---");
            }
        }
        public void WriteLine(string Format, params object[] args)
        {
            _internal_WriteLine(ConsoleWriter, Format, args);

            if (String.IsNullOrEmpty(Filename))
            {
                return;
            }

            if (FileWriter == null)
            {
                lock (this)
                {
                    if (FileWriter == null)
                    {
                        FileWriter = new StreamWriter(
                            path: Filename,
                            append: false,
                            encoding: this._encoding);

                        FileWriter = TextWriter.Synchronized(FileWriter);
                    }
                }
            }

            _internal_WriteLine(FileWriter, Format, args);
        }
        public bool hasDataWritten()
        {
            return FileWriter != null;
        }
        public void Dispose()
        {
            if ( FileWriter != null )
            {
                FileWriter.Close();
            }
        }
        /// <summary>
        /// This functions exists for the following problem:
        /// If you pass no "args" (args==null) it would be a WriteLine(Format,null).
        /// Then if the string (Format) you passed has "{" "}" in it, the call will mostly crash because of a bad c# format string.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="Format"></param>
        /// <param name="args"></param>
        private void _internal_WriteLine(TextWriter writer, string Format, params object[] args)
        {
            if ( writer == null )
            {
                return;
            }

            if (args == null || args.Length == 0)
            {
                writer.WriteLine(Format);
            }
            else
            {
                writer.WriteLine(Format, args);
            }
        }
    }
}
