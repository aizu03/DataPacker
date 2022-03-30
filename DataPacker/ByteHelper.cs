#pragma warning disable CS8603

using System;
using System.Text;

namespace DataPacker
{
    internal class ByteHelper
    {
        /// <summary>
        /// Primitive to byte array
        /// </summary>
        /// <returns></returns>
        public static byte[] Generate(object obj, Encoding? encoding)
        {
            return obj switch
            {
                byte b => new[] { b },
                bool b => new[] { b ? (byte)1 : (byte)0 },
                char c => BitConverter.GetBytes(c),
                string s => encoding.GetBytes(s),
                short s => BitConverter.GetBytes(s),
                ushort us => BitConverter.GetBytes(us),
                int i => BitConverter.GetBytes(i),
                uint ui => BitConverter.GetBytes(ui),
                nint ni => BitConverter.GetBytes(ni),
                nuint nui => BitConverter.GetBytes(nui),
                long l => BitConverter.GetBytes(l),
                ulong ul => BitConverter.GetBytes(ul),
                float s => BitConverter.GetBytes(s),
                double d => BitConverter.GetBytes(d),
                decimal m => BitConverter.GetBytes((double)m), // that's dirty
                _ => null
            };
        }


        /// <summary>
        /// Byte array back to primitive type
        /// </summary>
        public static T Cast<T>(ref byte[] array, Encoding? encoding = null)
        {
            if (typeof(T) == typeof(string)) return (T)(object)encoding!.GetString(array);

            return default(T) switch
            {
                int _ => (T)(object)BitConverter.ToInt32(array, 0),
                long _ => (T)(object)BitConverter.ToInt64(array, 0),
                bool _ => (T)(object)(array[0] == 1),
                byte _ => (T)(object)array[0],
                float _ => (T)(object)BitConverter.ToSingle(array, 0),
                double _ => (T)(object)BitConverter.ToDouble(array, 0),
                ulong _ => (T)(object)BitConverter.ToUInt64(array, 0),
                short _ => (T)(object)BitConverter.ToInt16(array, 0),
                ushort _ => (T)(object)BitConverter.ToUInt16(array, 0),
                uint _ => (T)(object)BitConverter.ToUInt32(array, 0),
                char _ => (T)(object)BitConverter.ToChar(array, 0),
                nint _ => (T)(object)BitConverter.ToInt64(array, 0),
                nuint _ => (T)(object)BitConverter.ToUInt64(array, 0),
                decimal _ => (T)(object)BitConverter.ToDouble(array, 0),
                _ => default
            };
        }

        public static object Cast2(Type type, ref byte[] array, Encoding encoding)
        {
            if (type == typeof(int)) return BitConverter.ToInt32(array, 0);
            if (type == typeof(long)) return BitConverter.ToInt64(array, 0);
            if (type == typeof(bool)) return array[0] == 1;
            if (type == typeof(byte)) return array[0];
            if (type == typeof(float)) return BitConverter.ToSingle(array, 0);
            if (type == typeof(double)) return BitConverter.ToDouble(array, 0);
            if (type == typeof(string)) return encoding.GetString(array);
            if (type == typeof(uint)) return BitConverter.ToUInt32(array, 0);
            if (type == typeof(ulong)) return BitConverter.ToUInt64(array, 0);
            if (type == typeof(ushort)) return BitConverter.ToUInt16(array, 0);
            if (type == typeof(short)) return BitConverter.ToInt16(array, 0);
            if (type == typeof(decimal)) return BitConverter.ToDouble(array, 0);
            if (type == typeof(nint)) return (nint)BitConverter.ToInt64(array, 0); // pointers
            if (type == typeof(nuint)) return (nuint)BitConverter.ToUInt64(array, 0); 
            if (type == typeof(char)) return BitConverter.ToChar(array, 0);
            return null;
        }
    }
}
