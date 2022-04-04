using System;

namespace DataPacker.Writers
{
    internal interface IBaseWriter : IDisposable
    {
        void Add(byte[] data);
        void Add(object data);
        void Add(string name, byte[] data);
        void Add(string name, object data);
        int Size();
        void Clear();
    }
}