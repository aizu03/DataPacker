using System.IO;
using System.Text;
using DataPacker.Writers;
namespace DataPacker
{
    public class SequenceWriter : IBaseWriter
    {
        private readonly BaseWriter writer;

        public SequenceWriter(Stream stream, DataStructure structure = DataStructure.Sequential, Encoding? stringEncoding = null, SequenceReader? appendReader = null)
        {
            stringEncoding ??= Encoding.Unicode;
            switch(structure)
            {
                case DataStructure.Sequential:

                    writer = new WriterSequential(stream, stringEncoding);
                    break;

                case DataStructure.SequentialNamed:

                    writer = new WriterSequentialNamed(stream, stringEncoding);
                    break;

                case DataStructure.Indexed:

                    writer = new WriterIndexed(stream, false, stringEncoding, appendReader?.readerIndexed);
                    break;

                case DataStructure.IndexedNamed:
                    writer = new WriterIndexed(stream, true, stringEncoding, appendReader?.readerIndexed);
                    break;
            }
        }

        public void Dispose() => writer.Dispose();
        public int Size() => writer.Size();
        public void Add(byte[] data) => writer.Add(data);
        public void Add(object data) => writer.Add(data);
        public void Add(string name, byte[] data) => writer.Add(name, data);
        public void Add(string name, object data) => writer.Add(name, data);
        public void Flush(bool closeStream = true) => writer.Flush(closeStream);
        public void Clear() => writer.Clear();
    }
}
