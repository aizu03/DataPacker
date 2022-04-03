using System;
using System.IO;
using System.Text;
using static DataPacker.ByteHelper;

namespace DataPacker.Writers
{
    internal class WriterSequential : BaseWriter
    {
        protected internal WriterSequential(Stream stream,  Encoding stringEncoding) : base(stream, false, stringEncoding) { }

        public override void Dispose()
        {
            Clear();
        }

        public override void Flush(bool closeStream)
        {
            foreach (var obj in objects)
            {
                // Write length and the object bytes
                stream.Write(BitConverter.GetBytes(obj.Length));
                stream.Write(obj);
            }

            Clear();
            if (closeStream) stream.Close();
        }
    }
}