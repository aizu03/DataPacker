using System.Text;
using DataPacker.Serialization;
using static DataPacker.ByteHelper;

namespace DataPacker
{
    public struct Entry
    {
        public string? name;
        public byte[] data;
        public int length;
        private readonly Encoding encoding;

        public Entry(byte[] data, int length, Encoding encoding, string? name = null)
        {
            this.data = data;
            this.length = length;
            this.encoding = encoding;
            this.name = name;
        }

        public T Deserialize<T>() => CompactFormatter.Deserialize<T>(data);
        public T Deserialize<T>(BasicFormatter formatter) => formatter.Deserialize<T>(data);

        public byte ToByte() => Cast<byte>(ref data);
        public bool ToBool() => Cast<bool>(ref data);
        public short ToInt16() => Cast<short>(ref data);
        public int ToInt32() => Cast<int>(ref data);
        public long ToInt64() => Cast<long>(ref data);
        public float ToSingle() => Cast<float>(ref data);
        public double ToDouble() => Cast<double>(ref data);
        public char ToChar() => Cast<char>(ref data);
        public new string ToString() => Cast<string>(ref data, encoding);
        public string ToString(Encoding encoding) => Cast<string>(ref data, encoding);
    }
}
