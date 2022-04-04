using System;
using System.IO;
using System.Text;

namespace DataPacker.Readers
{
    internal class ReaderSequential : BaseReader
    {
        protected internal ReaderSequential(Stream stream, Encoding? stringEncoding = null) : base(stream, false, stringEncoding) { }

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
                // Read data
                var lenBytes = new byte[sizeof(int)];
                stream.Read(lenBytes, 0, sizeof(int));
                var dataLength = BitConverter.ToInt32(lenBytes);
                var data = new byte[dataLength];
                stream.Read(data, 0, dataLength);
                var entry = new Entry(data, dataLength, encoding);
                Entries.Add(entry);

            } while (stream.Position < length);

            if (closeStream) stream.Close();
        }
    }
}
