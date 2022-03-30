using System.Text;
using System;
using System.IO;
using static DataPacker.ByteHelper;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DataPacker.Writers
{
    public abstract class BaseWriter : BaseData, IBaseWriter
    {
        protected internal readonly List<byte[]> objects = new();
        protected internal readonly Dictionary<string, byte[]> objectsNamed = new();
        protected internal readonly bool named;

        protected Encoding encoding = Encoding.Unicode;

        protected BaseWriter(Stream stream) : base(stream) { }

        protected BaseWriter(Stream stream, bool named, Encoding? stringEncoding = null) : base(stream)
        {
            this.named = named;
            if (stringEncoding != null) encoding = stringEncoding;
        }

        public int Size() => named ? objectsNamed.Count : objects.Count;

        public void Add(object data)
        {
            if (named) throw new ArgumentException("Can't add data without name in a named sequence");
            objects.Add(Verify(data));
        }

        public void Add(byte[] data)
        {
            if (named) throw new ArgumentException("Can't add data without name in a named sequence");
            if (data == null) throw new ArgumentException("The data can't be null");
            objects.Add(data);
        }

        public void Add(string name, object data)
        {
            if (!named) throw new ArgumentException("Can't add a name to the data in a non named sequence");
            objectsNamed[name] = Verify(data);
        }

        public void Add(string name, byte[] data)
        {
            if (!named) throw new ArgumentException("Can't add a name to the data in a non named sequence");
            if (data == null) throw new ArgumentException("The data can't be null");
            objectsNamed[name] = data;
        }

        private byte[] Verify(object data)
        {
            if (data == null) throw new ArgumentException("The data can't be null");
            var bytes = Generate(data, encoding);
            if (bytes == null) throw new ArgumentException($"Unknown data type! Only primitive types are supported!");
            return bytes;
        }

        public abstract void Flush(bool closeStream);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            objects.Clear();
            objectsNamed.Clear();
        }

        public virtual void Dispose() { throw new NotImplementedException(); }
    }
}
