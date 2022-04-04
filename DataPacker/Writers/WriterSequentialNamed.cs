using System;
using System.IO;
using System.Text;
using static DataPacker.ByteHelper;

namespace DataPacker.Writers
{
    internal class WriterSequentialNamed : BaseWriter
    {
        protected internal WriterSequentialNamed(Stream stream, Encoding stringEncoding) : base(stream, true, stringEncoding) { }

        public override void Dispose()
        {
            Clear();
        }

        public override void Flush()
        {
            foreach (var (key, obj) in objectsNamed)
            {
                var name = Generate(key, encoding);

                // Write length and the name bytes
                stream.Write(BitConverter.GetBytes(name.Length));
                stream.Write(name);

                // Write length and the object bytes
                stream.Write(BitConverter.GetBytes(obj.Length));
                stream.Write(obj);
            }

            Clear();
        }
    }
}