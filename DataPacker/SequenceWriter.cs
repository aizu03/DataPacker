using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using DataPacker.Writers;
namespace DataPacker
{
    public class SequenceWriter : IBaseWriter
    {
        private readonly BaseWriter writer;

        public SequenceWriter(Stream stream, DataStructure structure = DataStructure.Sequential, Encoding? encoding = null, SequenceReader? appendReader = null)
        {
            encoding ??= Encoding.Unicode;
            switch(structure)
            {
                case DataStructure.Sequential:

                    writer = new WriterSequential(stream, encoding);
                    break;

                case DataStructure.SequentialNamed:

                    writer = new WriterSequentialNamed(stream, encoding);
                    break;

                case DataStructure.Indexed:

                    writer = new WriterIndexed(stream, false, encoding, appendReader?.readerIndexed);
                    break;

                case DataStructure.IndexedNamed:
                    writer = new WriterIndexed(stream, true, encoding, appendReader?.readerIndexed);
                    break;
            }
        }

        public void Dispose() => writer.Dispose();
        public void Clear() => writer.Clear();
        public int Size() => writer.Size();
        public void Add(byte[] data) => writer.Add(data);
        public void Add(object data) => writer.Add(data);
        public void Add(string name, byte[] data) => writer.Add(name, data);
        public void Add(string name, object data) => writer.Add(name, data);
        public void Flush(bool closeStream = true)
        {
            writer.Flush();
            if (closeStream) writer.stream.Close();
        }
    }
}
