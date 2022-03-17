using System;
using System.IO;
using System.Text;
using static DataPacker.ByteHelper;

namespace DataPacker.Writers
{
    internal class WriterSequential : BaseWriter
    {
        protected internal WriterSequential(Stream stream, bool named, Encoding? stringEncoding = null) : base(stream, named, stringEncoding) { }

        public override void Dispose()
        {
            objects.Clear();
            objectsNamed.Clear();
            GC.SuppressFinalize(this);
        }

        public override void Write(bool closeStream)
        {
            // Write objects in a sequence to the pack
            using var pack = new MemoryStream();

            if (named)
            {
                foreach(var pair in objectsNamed)
                {
                    var name = Generate(pair.Key, encoding);
                    var obj = pair.Value;

                    // Write length and the name bytes
                    pack.Write(BitConverter.GetBytes(name.Length));
                    pack.Write(name);

                    // Write length and the object bytes
                    pack.Write(BitConverter.GetBytes(obj.Length));
                    pack.Write(obj);
                }

            } else
            {
                foreach (var obj in objects)
                {
                    // Write length and the object bytes
                    pack.Write(BitConverter.GetBytes(obj.Length));
                    pack.Write(obj);
                }
            }

            stream.Write(pack.ToArray());
            if (closeStream) stream.Close();
        }
    }
}