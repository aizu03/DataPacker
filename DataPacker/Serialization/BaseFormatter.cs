
using System;
using System.Collections.Generic;
using System.Text;

namespace DataPacker.Serialization
{
    public abstract class BaseFormatter : IFormatter, IDisposable
    {
        private protected readonly Encoding urlEncoding = Encoding.ASCII;
        private protected readonly Encoding stringEncoding;

        private protected readonly Dictionary<object, int?> objectIndexes = new(); // id of object, index of object 
        private protected readonly List<object> objects = new();

        private protected readonly Dictionary<string, Type> typeTable = new(); // GetType() is very slow. lookup table 3x faster

        protected BaseFormatter(Encoding stringEncoding)
        {
            this.stringEncoding = stringEncoding;
        }

        protected BaseFormatter()
        {
            stringEncoding = Encoding.Unicode;
        }

        public abstract byte[] Serialize(object clazz);
        public abstract T Deserialize<T>(byte[] bytes);

        public void Dispose()
        {
            typeTable.Clear();
        }
    }
}