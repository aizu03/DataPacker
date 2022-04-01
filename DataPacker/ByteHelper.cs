using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace DataPacker
{
    internal class ByteHelper
    {
        /// <summary>
        /// Primitive to byte array
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] Generate(object obj, Encoding encoding)
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
                decimal m => BitConverter.GetBytes((double)m), // dirty
                _ => throw new ArgumentException("Unknown Type")
            };
        }

        /// <summary>
        /// Generate field entry [1, bytes..]
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] GenerateFieldEntry(object obj, Encoding encoding)
        {
            unsafe
            {
                switch (obj)
                {
                    case byte b: return new[] { (byte)1, b };
                    case bool b: return new[] { (byte)1, b ? (byte)1 : (byte)0 };
                    case string s:

                        var sEBytes = encoding.GetBytes(s);
                        var sBytes = new byte[sEBytes.Length + 1];
                        sBytes[0] = 1;
                        Buffer.BlockCopy(sEBytes, 0, sBytes, 1, sEBytes.Length);
                        return sBytes;

                    case char c:

                        var b0 = new byte[3];
                        var pChar = (byte*)&c;
                        b0[0] = 1;
                        b0[1] = pChar[0];
                        b0[2] = pChar[1];
                        return b0;

                    case short s:

                        var b1 = new byte[3];
                        var pShort = (byte*)&s;
                        b1[0] = 1;
                        b1[1] = pShort[0];
                        b1[2] = pShort[1];
                        return b1;

                    case ushort us:

                        var b2 = new byte[3];
                        var pUShort = (byte*)&us;
                        b2[0] = 1;
                        b2[1] = pUShort[0];
                        b2[2] = pUShort[1];
                        return b2;

                    case int i:

                        var b3 = new byte[5];
                        var pInt = (byte*)&i;
                        b3[0] = 1;
                        b3[1] = pInt[0];
                        b3[2] = pInt[1];
                        b3[3] = pInt[2];
                        b3[4] = pInt[3];
                        return b3;

                    case uint ui:

                        var b4 = new byte[5];
                        var pUInt = (byte*)&ui;
                        b4[0] = 1;
                        b4[1] = pUInt[0];
                        b4[2] = pUInt[1];
                        b4[3] = pUInt[2];
                        b4[4] = pUInt[3];
                        return b4;

                    case float s:

                        var b5 = new byte[5];
                        var pFloat = (byte*)&s;
                        b5[0] = 1;
                        b5[1] = pFloat[0];
                        b5[2] = pFloat[1];
                        b5[3] = pFloat[2];
                        b5[4] = pFloat[3];
                        return b5;

                    case double d:
                     
                        var b6 = new byte[9];
                        var pDouble = (byte*)&d;
                        b6[0] = 1;
                        b6[1] = pDouble[0];
                        b6[2] = pDouble[1];
                        b6[3] = pDouble[2];
                        b6[4] = pDouble[3];
                        b6[5] = pDouble[4];
                        b6[6] = pDouble[5];
                        b6[7] = pDouble[6];
                        b6[8] = pDouble[7];
                        return b6;

                    case long l:

                        var b9 = new byte[9];
                        var pLong = (byte*)&l;
                        b9[0] = 1;
                        b9[1] = pLong[0];
                        b9[2] = pLong[1];
                        b9[3] = pLong[2];
                        b9[4] = pLong[3];
                        b9[5] = pLong[4];
                        b9[6] = pLong[5];
                        b9[7] = pLong[6];
                        b9[8] = pLong[7];
                        return b9;

                    case nint ni:

                        var b7 = new byte[9];
                        var pNInt = (byte*)&ni;
                        b7[0] = 1;
                        b7[1] = pNInt[0];
                        b7[2] = pNInt[1];
                        b7[3] = pNInt[2];
                        b7[4] = pNInt[3];
                        b7[5] = pNInt[4];
                        b7[6] = pNInt[5];
                        b7[7] = pNInt[6];
                        b7[8] = pNInt[7];
                        return b7;

                    case nuint nui:

                        var b8 = new byte[9];
                        var pNUInt = (byte*)&nui;
                        b8[0] = 1;
                        b8[1] = pNUInt[0];
                        b8[2] = pNUInt[1];
                        b8[3] = pNUInt[2];
                        b8[4] = pNUInt[3];
                        b8[5] = pNUInt[4];
                        b8[6] = pNUInt[5];
                        b8[7] = pNUInt[6];
                        b8[8] = pNUInt[7];
                        return b8;

                    case ulong ul:

                        var b10 = new byte[9];
                        var pULong = (byte*)&ul;
                        b10[0] = 1;
                        b10[1] = pULong[0];
                        b10[2] = pULong[1];
                        b10[3] = pULong[2];
                        b10[4] = pULong[3];
                        b10[5] = pULong[4];
                        b10[6] = pULong[5];
                        b10[7] = pULong[6];
                        b10[8] = pULong[7];
                        return b10;

                    case decimal m:

                        var doubleDec = (double)m; // dirty

                        var b11 = new byte[9];
                        var pDecimal = (byte*)&doubleDec;
                        b11[0] = 1;
                        b11[1] = pDecimal[0];
                        b11[2] = pDecimal[1];
                        b11[3] = pDecimal[2];
                        b11[4] = pDecimal[3];
                        b11[5] = pDecimal[4];
                        b11[6] = pDecimal[5];
                        b11[7] = pDecimal[6];
                        b11[8] = pDecimal[7];
                        return b11;

                    default:
                        throw new ArgumentException("Unknown Type");
                }
            }
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
                _ => throw new ArgumentException("Unknown Type")
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            throw new ArgumentException("Unknown Type");
        }
    }
}
