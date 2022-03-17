using System;

namespace DataPacker.Writers
{
    public interface IBaseWriter : IDisposable
    {
        void Add(byte[] data);
        void Add(object data);
        void Add(string name, byte[] data);
        void Add(string name, object data);
        int Size();
        void Write(bool closeStream);
    }
}