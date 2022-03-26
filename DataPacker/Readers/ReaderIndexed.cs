using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataPacker.Readers
{
    internal class ReaderIndexed : BaseReader, IReaderIndexed
    {
        protected internal readonly List<BookEntry> bookEntries = new();
        protected internal long bookLength;

        public ReaderIndexed(Stream stream, bool named, Encoding? stringEncoding = null)
            : base(stream, named, stringEncoding) => ReadBook();

        public int Available() => bookEntries.Count;

        public override void Dispose()
        {
            bookEntries.Clear();
            Entries.Clear();
            NamedEntries?.Clear();
            GC.SuppressFinalize(this);
        }

        private void ReadBook()
        {
            // Read length of book
            const int size = sizeof(long);
            var buffer = new byte[size];
            stream.Seek(-size, SeekOrigin.End);
            stream.Read(buffer, 0, size);
            bookLength = BitConverter.ToInt64(buffer, 0);

            // Read book
            var book = new byte[bookLength];
            stream.Seek(-bookLength - size, SeekOrigin.Current);
            stream.Read(book, 0, (int)bookLength);

            // Save entry positions
            var begin = 0;
            var offset = 0; // book offset 
            do
            {
                var entryPosition = BitConverter.ToInt32(book, offset);
                offset += sizeof(int);
                bookEntries.Add(new BookEntry(begin, entryPosition));
                begin = entryPosition;

            } while (offset < bookLength);
            ;
        }

        /// <summary>
        /// Read all entries
        /// </summary>
        public override void Read(bool closeStream) => ReadRange(0, bookEntries.Count - 1, ref closeStream);

        /// <summary>
        /// Read all entries starting at index
        /// </summary>
        public int Read(int index, bool closeStream) => ReadRange(index, bookEntries.Count - 1, ref closeStream);

        /// <summary>
        /// Read all entries from start index to stop index
        /// </summary>
        public int Read(int indexBegin, int indexEnd, bool closeStream) => ReadRange(indexBegin, indexEnd, ref closeStream);

        public int ReadOne(int index, bool closeStream) => Read(index, index, closeStream);

        private int ReadRange(int start, int stop, ref bool close)
        {
            // Jump to start entry
            stream.Seek(bookEntries[start].begin, SeekOrigin.Begin);

            for (var i = start; i <= stop; i++)
            {
                var info = bookEntries[i];
                var totalLength = info.end - info.begin;

                if (named) // length = name len + name + data
                {
                    // Read name first
                    var nameLenBytes = new byte[sizeof(int)];
                    stream.Read(nameLenBytes, 0, sizeof(int));
                    var nameLen = BitConverter.ToInt32(nameLenBytes);
                    var nameBytes = new byte[nameLen];
                    stream.Read(nameBytes, 0, nameLen);
                    var name = encoding.GetString(nameBytes);

                    // Read data
                    var bytesRead = nameLen + sizeof(int);
                    var dataLen = totalLength - bytesRead;
                    var data = new byte[dataLen];
                    stream.Read(data, 0, dataLen);
                    var entry = new Entry(data, totalLength, encoding, name);

                    Entries.Add(entry);
                    NamedEntries[name] = entry;
                }
                else // length = data
                {
                    var bytes = new byte[totalLength];
                    stream.Read(bytes, 0, totalLength);
                    Entries.Add(new Entry(bytes, totalLength, encoding));
                }
            }

            if (close) stream.Close();
            return Entries.Count;
        }
    }
}
