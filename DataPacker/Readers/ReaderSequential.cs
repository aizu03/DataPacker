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
            var bytes = GetStreamBytes();
            var length = bytes.Length;
            var offset = 0;

            do
            {
                string? name = null;

                if (named)
                {
                    // Read name
                    var nameLength = BitConverter.ToInt32(bytes, offset);
                    offset += sizeof(int);
                    var nameBytes = new byte[nameLength];
                    Buffer.BlockCopy(bytes, offset, nameBytes, 0, nameLength);
                    name = encoding.GetString(nameBytes);

                    // Jump to data
                    offset += nameLength;
                }

                // Read data
                var dataLength = BitConverter.ToInt32(bytes, offset);
                offset += sizeof(int);
                var data = new byte[dataLength];
                Buffer.BlockCopy(bytes, offset, data, 0, dataLength);
                var entry = new Entry(data, dataLength, encoding, name);
          
                if (name != null) NamedEntries[name] = entry;
                Entries.Add(entry);

                // Jump to next
                offset += dataLength;

            } while (offset < length);


            if (closeStream) stream.Close();
        }
    }
}
