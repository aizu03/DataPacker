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
        public WriterIndexed(Stream stream, bool named, Encoding? stringEncoding = null, ReaderIndexed? appendReader = null)
            : base(stream, named, stringEncoding)     
        {
            if (appendReader != null)     
                this.appendReader = appendReader;
        }

        public override void Dispose()
        {
            objects.Clear();
            objectsNamed.Clear();
            GC.SuppressFinalize(this);
        }

        public override void Write(bool closeStream)
        {
            var origin = 0L;
            var offsets = new List<int>();

            // If append data, get the previous book offsets
            // and set stream position to the end of the existing data
            if (appendReader != null)
            {
                foreach (var entry in appendReader.bookEntries) offsets.Add(entry.end);
                stream.Seek(-sizeof(long) - appendReader.bookLength, SeekOrigin.End);

                origin = stream.Position;

                // After that write all the data normally, write book etc.
                // ...
            }

            if (named)
            {
                foreach (var pair in objectsNamed)
                {
                    var name = Generate(pair.Key, encoding);
                    var obj = pair.Value;

                    // Write name and data
                    stream.Write(BitConverter.GetBytes(name.Length));
                    stream.Write(name);
                    stream.Write(obj);

                    // Store end position of each entry
                    // -> ends at 10, 42, 60, 90 .. etc.
                    offsets.Add((int)(origin + stream.Position));
                }

            } else
            {
                foreach (var obj in objects)
                {
                    stream.Write(obj);

                    // Store end position of each entry
                    // -> ends at 10, 42, 60, 90 .. etc.
                    offsets.Add((int)(origin + stream.Position));
                }
            }

            // Write the book
            using var book = new MemoryStream();
            foreach (var offset in offsets)  
                book.Write(BitConverter.GetBytes(offset));

            stream.Write(book.ToArray());

            // Write the book length
            stream.Write(Generate(book.Position, null));

            if (closeStream) stream.Close();
        }
    }
}
