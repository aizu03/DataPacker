
using System;
using System.IO;
using System.Text;

namespace DataPacker.Readers
{
    public class ReaderSequentialNamed : BaseReader
    {
        public ReaderSequentialNamed(Stream stream, Encoding? stringEncoding = null) : base(stream, true, stringEncoding) { }

        public override void Dispose()
        {
            Entries.Clear();
            NamedEntries?.Clear();
        }

        public override void Read(bool closeStream)
        {
            var length = stream.Length;

            do
            {
                // Read name
                var lenBytesName = new byte[sizeof(int)];
                stream.Read(lenBytesName, 0, sizeof(int));
                var nameLength = BitConverter.ToInt32(lenBytesName);
                var nameBytes = new byte[nameLength];
                stream.Read(nameBytes, 0, nameLength);
                var name = encoding.GetString(nameBytes);

                // Read data
                var lenBytes = new byte[sizeof(int)];
                stream.Read(lenBytes, 0, sizeof(int));
                var dataLength = BitConverter.ToInt32(lenBytes);
                var data = new byte[dataLength];
                stream.Read(data, 0, dataLength);
                var entry = new Entry(data, dataLength, encoding, name);
                Entries.Add(entry);

                NamedEntries[name] = entry;

            } while (stream.Position < length);

            if (closeStream) stream.Close();
        }
    }
}