using System;
using System.IO;
using System.Text;

namespace DataPacker.Readers
{
    internal class ReaderSequential : BaseReader
    {
        public ReaderSequential(Stream stream, bool named, Encoding? stringEncoding = null) : base(stream, named, stringEncoding) { }

        public override void Dispose()
        {
            Entries.Clear();
            NamedEntries?.Clear();
            GC.SuppressFinalize(this);
        }

        public override void Read(bool closeStream)
        {
            var length = stream.Length;
     
            do
            {
                string? name = null;

                if (named)
                {
                    // Read name
                    var lenBytesName = new byte[sizeof(int)];
                    stream.Read(lenBytesName, 0, sizeof(int));
                    var nameLength = BitConverter.ToInt32(lenBytesName);
                    var nameBytes = new byte[nameLength];
                    stream.Read(nameBytes, 0, nameLength);
                    name = encoding.GetString(nameBytes);
                }

                // Read data
                var lenBytes = new byte[sizeof(int)];
                stream.Read(lenBytes, 0, sizeof(int));
                var dataLength = BitConverter.ToInt32(lenBytes);
                var data = new byte[dataLength];
                stream.Read(data, 0, dataLength);
                var entry = new Entry(data, dataLength, encoding, name);
                if (name != null) NamedEntries[name] = entry;
                Entries.Add(entry);

            } while (stream.Position < length);

            if (closeStream) stream.Close();
        }
    }
}
