using System;
using System.Collections.Generic;

namespace DataPacker.Readers
{
    public interface IBaseReader : IDisposable
    {
        public List<Entry> Entries {  get; }
        public Dictionary<string, Entry> NamedEntries {  get; }
        Entry this[int index] { get; }
        Entry this[string name] { get; }
        void Read(bool closeStream);
        int ReadEntries();
    }
}