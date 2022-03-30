using System;
using System.IO;
using System.Text;
using static DataPacker.ByteHelper;

namespace DataPacker.Writers
{
    public class WriterSequentialNamed : BaseWriter
    {
        public WriterSequentialNamed(Stream stream, Encoding? stringEncoding = null) : base(stream, true, stringEncoding) { }

        public override void Dispose()
        {
            Clear();
        }

        public override void Flush(bool closeStream)
        {
            foreach (var pair in objectsNamed)
            {
                var name = Generate(pair.Key, encoding);
                var obj = pair.Value;

                // Write length and the name bytes
                stream.Write(BitConverter.GetBytes(name.Length));
                stream.Write(name);

                // Write length and the object bytes
                stream.Write(BitConverter.GetBytes(obj.Length));
                stream.Write(obj);
            }

            Clear();
            if (closeStream) stream.Close();
        }
    }
}