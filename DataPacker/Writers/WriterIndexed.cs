using System.Text;
using DataPacker.Readers;
using System;
using System.IO;
using static DataPacker.ByteHelper;
using System.Collections.Generic;

namespace DataPacker.Writers
{
    internal class WriterIndexed : BaseWriter
    {
        private readonly ReaderIndexed? appendReader;

        // Not appending
        public WriterIndexed(Stream stream, bool named, Encoding stringEncoding, ReaderIndexed? appendReader = null)
            : base(stream, named, stringEncoding)     
        {
            if (appendReader != null)     
                this.appendReader = appendReader;
        }

        public override void Dispose()
        {
            Clear();
        }

        public override void Flush(bool closeStream)
        {
            var offsets = new List<int>();
            if (appendReader != null)
            {
                // Create book from previous offsets
                foreach (var entry in appendReader.bookEntries) 
                    offsets.Add(entry.end);

                // Go to end of last entry
                stream.Seek(-sizeof(long) - appendReader.bookLength, SeekOrigin.End);
            }

            if (named)
            {
                foreach (var (key, obj) in objectsNamed)
                {
                    var name = Generate(key, encoding);

                    // Write name and data
                    stream.Write(BitConverter.GetBytes(name.Length));
                    stream.Write(name);
                    stream.Write(obj);

                    // Store offset from each entry in the book: 0, 19, 48, 90 etc.
                    offsets.Add((int)stream.Position);
                }

            } else
            {
                foreach (var obj in objects)
                {
                    stream.Write(obj);

                    // Store end position of each entry
                    // -> ends at 10, 42, 60, 90 .. etc.
                    offsets.Add((int)stream.Position);
                }
            }

            // Write the book
            var bookBytesTotal = offsets.Count * sizeof(int);
            foreach (var offset in offsets)
                stream.Write(BitConverter.GetBytes(offset));

            // Write the book length
            stream.Write(BitConverter.GetBytes((long)bookBytesTotal));

            Clear();
            if (closeStream) stream.Close();
        }
    }
}
