using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace DataPacker.Readers
{
    public abstract class BaseReader : BaseData, IBaseReader
    {
        public List<Entry> Entries { get; } = new();
        public Dictionary<string, Entry> NamedEntries { get; }

        protected readonly bool named;
        protected Encoding encoding = Encoding.Unicode;

        protected BaseReader(Stream stream, bool named, Encoding? stringEncoding = null) : base(stream)
        {
            this.named = named;
            if (named) NamedEntries = new Dictionary<string, Entry>();
            if (stringEncoding != null) encoding = stringEncoding;
        }

        public virtual void Dispose() => throw new NotImplementedException();
        public Entry this[int index] => Entries[index];
        public Entry this[string name] => NamedEntries[name];

        public int ReadEntries() => Entries.Count;
        public abstract void Read(bool closeStream);
    }
}
