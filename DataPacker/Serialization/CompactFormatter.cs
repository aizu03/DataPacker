using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using DataPacker.Readers;
using DataPacker.Writers;

namespace DataPacker.Serialization
{
    public class CompactFormatter
    {
        private static readonly Encoding UrlEncoding = Encoding.ASCII;
        private static readonly Encoding StringEncoding = Encoding.Unicode;

        private static readonly Dictionary<object, int?> ObjectIndexes = new(); // id of object, index of object
        private static readonly List<object> Objects = new();

        // TODO: For future
        /*
        Store path names in a dictionary and use index instead of URL
        byte[1, URL <- index]
          
        To slightly compress everything   
        Split them into two: CompactFormatter, CompressedFormatter ?
        */

        #region Serialize

        public static byte[] Serialize(object clazz)
        {
            if (clazz == null) throw new ArgumentException("Object is null!");
            var bytes = ClassToBytes(clazz);
            ObjectIndexes.Clear();
            return bytes;
        }

        /// <summary>
        ///
        /// The class is converted into a basic <see cref="SequenceWriter"/>
        /// Each entry in the sequence represents the field value as a byte[]
        /// 
        /// If the field is a primitive or string:
        /// byte[is null, value..]
        /// 
        /// If it's an object:
        /// byte[is null, url, bytes of object]
        /// 
        /// If it's a reference to an object currently serializing:
        /// byte[2, index of object]
        ///
        /// 0 = null
        /// 1 = not null
        /// 2 = reference
        /// 
        /// </summary>
        private static byte[] ClassToBytes(object clazz)
        {
            // Save current index of object
            // instead of using memory address of object, use the hash code.
            // https://stackoverflow.com/a/751146
            // Currently no checks for hash collision that's up to the user
            if (!ObjectIndexes.ContainsKey(clazz))
                ObjectIndexes[clazz] = ObjectIndexes.Count;

            // Debug.WriteLine($"Serialize.. {clazz.GetType().Name} {id:X}");

            using var ms = new MemoryStream();
            using var writer = new WriterSequential(ms, false);

            foreach (var field in clazz.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var fieldType = field.FieldType;
                var fieldValue = field.GetValue(clazz);

                // Add null entry
                if (fieldValue == null)
                {
                    writer.Add((byte)0); // field is null
                    continue;
                }

                // Add primitive types or strings
                if (fieldType.IsPrimitive || fieldType == typeof(string))
                {
                    var fieldBytes = ByteHelper.Generate(fieldValue, StringEncoding);

                    // [1, bytes..]
                    var bytes = new byte[fieldBytes.Length + 1];
                    bytes[0] = 1; // field not null
                    Buffer.BlockCopy(fieldBytes, 0, bytes, 1, fieldBytes.Length);

                    writer.Add(bytes);
                    continue;
                }

                // Add arrays
                if (fieldType.IsArray)
                {
                    writer.Add(ArrayToBytes((Array)fieldValue, fieldType));
                    continue;
                }

                // Add reference by checking if the field value is a class currently serializing
                ObjectIndexes.TryGetValue(fieldValue, out var index);
                if (index != null)
                {
                    // .. just save the index
                    // [2, index]
                    var bytes = new byte[5];
                    bytes[0] = 2;

                    unsafe
                    {
                        var i = (int)index;
                        var pi = (byte*)&i;
                        bytes[1] = pi[0];
                        bytes[2] = pi[1];
                        bytes[3] = pi[2];
                        bytes[4] = pi[3];
                    }

                    writer.Add(bytes);
                    continue;
                }

                // Add other classes
                var url = fieldValue.GetType().FullName;
                var urlBytes = UrlEncoding.GetBytes(url);
                var classBytes = ClassToBytes(fieldValue);

                // Combine URL and class bytes into 1 sequence
                using var stream = new MemoryStream();

                // byte[1, ...]
                stream.WriteByte(1); // field is not null

                using var writerUrlClass = new WriterSequential(stream, false);
                writerUrlClass.Add(urlBytes);
                writerUrlClass.Add(classBytes);
                writerUrlClass.Write(true);
                // byte[1, URL, bytes]

                writer.Add(stream.ToArray());
            }

            writer.Write(true);
            return ms.ToArray();
        }

        /// <summary>
        /// 
        /// byte[1, sequence[URL, sequence[is null, bytes]]] 
        /// 
        /// 1. 1 = is not null
        /// 2. a sequence of the url and the array data
        /// 3. containing entry is null or the bytes
        /// 
        /// </summary>
        /// <returns></returns>
        private static byte[] ArrayToBytes(Array array, Type type)
        {
            // TODO: Make this better?
            /*
             * If the array was initialized with a size of 0,
             * the array is returned as null for now.
             * This is dirty?
             */

            // byte[0]
            if (array.Length == 0) return new byte[] { 0 }; // field is null

            // Get array type
            var baseType = type.GetElementType();
            var isBasePrimitiveOrString = baseType.IsPrimitive || baseType == typeof(string);
            var url = baseType.FullName;

            using var ms1 = new MemoryStream();

            // byte[1, ...]
            ms1.WriteByte(1); // field is not null

            using var urlSequenceWriter = new WriterSequential(ms1, false);
            urlSequenceWriter.Add(UrlEncoding.GetBytes(url));

            using var ms2 = new MemoryStream();
            using var isNullBytesWriter = new WriterSequential(ms2, false);

            foreach (var arr in array)
            {
                if (arr == null)
                {
                    isNullBytesWriter.Add((byte)0); // entry is null
                    isNullBytesWriter.Add((byte)0); // zero data
                    continue;
                }

                isNullBytesWriter.Add((byte)1); // entry is not null   

                // Check if it's another array and recursive call add it
                if (arr.GetType().IsArray)
                {
                    isNullBytesWriter.Add(ArrayToBytes((Array)arr, baseType));
                    continue;
                }

                // Write primitive, string or object
                isNullBytesWriter.Add(isBasePrimitiveOrString ? ByteHelper.Generate(arr, StringEncoding) : ClassToBytes(arr));
            }

            // Write the sequence of (1, data, 1, data, 0, 0, 1, data, 0, 0, 0, 0...]
            isNullBytesWriter.Write(true);

            // Write the sequence [URL, sequence[is null, data]]
            urlSequenceWriter.Add(ms2.ToArray());
            urlSequenceWriter.Write(true);

            return ms1.ToArray();
        }

        #endregion

        #region Deserialize

        public static T Deserialize<T>(byte[] bytes)
        {
            Objects.Clear();
            var url = typeof(T).FullName;
            return (T)ClassFromBytes(ref url, ref bytes);
        }

        private static object ClassFromBytes(ref string url, ref byte[] classBytes)
        {
            // Create empty class
            var classType = GetType2(url);
            var clazz = FormatterServices.GetUninitializedObject(classType);
            var fields = classType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            Objects.Add(clazz);

            // Empty class
            if (classBytes.Length == 0) return clazz;

            using var stream = new MemoryStream(classBytes);
            using var sequenceFields = new ReaderSequential(stream, false);
            sequenceFields.Read(true);

            for (var i = 0; i < fields.Length; i++)
            {
                // Each entry of the sequence is [0/1, field bytes..]
                var field = fields[i];
                var bytes = sequenceFields[i].data;
                if (bytes[0] == 0) continue; // field is null

                var fieldType = field.FieldType;

                // Remove the null indicator
                var fieldBytes = new byte[bytes.Length - 1];
                Buffer.BlockCopy(bytes, 1, fieldBytes, 0, bytes.Length - 1);

                // Primitives or strings
                if (fieldType.IsPrimitive || fieldType == typeof(string))
                {
                    // Parse bytes into primitive field type
                    field.SetValue(clazz, ByteHelper.Cast2(fieldType, ref fieldBytes, Encoding.Unicode));
                    continue;
                }

                // Arrays
                if (fieldType.IsArray)
                {
                    field.SetValue(clazz, ArrayFromBytes(ref fieldBytes, fieldType));
                    continue;
                }

                // References
                if (bytes[0] == 2)
                {
                    // Get object by index
                    var target = Objects[BitConverter.ToInt32(bytes, 1)];
                    field.SetValue(clazz, target);
                    continue;
                }

                // Other classes
                using var ms = new MemoryStream(fieldBytes);
                using var sequenceUrlBytes = new ReaderSequential(ms, false);
                sequenceUrlBytes.Read(true);

                var otherUrl = UrlEncoding.GetString(sequenceUrlBytes[0].data);
                var otherBytes = sequenceUrlBytes[1].data;

                field.SetValue(clazz, ClassFromBytes(ref otherUrl, ref otherBytes));
            }

            return clazz;
        }

        private static object ArrayFromBytes(ref byte[] arrayBytes, Type type)
        {
            // sequence[URL, sequence[is null, bytes]]

            var baseType = type.GetElementType();
            var isBasePrimitiveOrString = baseType.IsPrimitive || baseType == typeof(string);

            // Read URL and sequence bytes
            using var ms1 = new MemoryStream(arrayBytes);
            using var urlSequenceReader = new ReaderSequential(ms1, false);
            urlSequenceReader.Read(true);
            var arrayTypeUrl = urlSequenceReader[0].ToString(UrlEncoding);
            var sequenceBytes = urlSequenceReader[1].data;

            // Read array entries [0/1, data]
            using var ms2 = new MemoryStream(sequenceBytes);
            using var isNullBytesReader = new ReaderSequential(ms2, false);
            isNullBytesReader.Read(true);

            // Create empty array
            var arraySize = isNullBytesReader.Entries.Count / 2;
            var arr = Array.CreateInstance(baseType, arraySize);

            // Fill array
            var index = 0;
            for (var j = 0; j < isNullBytesReader.Entries.Count; j++)
            {
                var entryBytes = isNullBytesReader.Entries[j].data;

                // Skip null entries
                if ((j & 1) == 0)
                {
                    if (entryBytes[0] == 0) // value is null
                    {
                        ++index;
                        ++j;
                    }

                    continue;
                }

                // Check if entry of the array is another array
                if (baseType.IsArray)
                {
                    // [0/1, array bytes]
                    if (entryBytes[0] == 0) continue; // the array is null

                    // Remove null indicator
                    var nullRemoved = new byte[entryBytes.Length - 1];
                    Buffer.BlockCopy(entryBytes, 1, nullRemoved, 0, entryBytes.Length - 1);

                    // Convert [array bytes] with type
                    arr.SetValue(ArrayFromBytes(ref nullRemoved, baseType), index++);
                }
                else
                {
                    // Convert [entry bytes] to primitive, string or class object
                    arr.SetValue(isBasePrimitiveOrString ?
                        ByteHelper.Cast2(baseType, ref entryBytes, StringEncoding) :
                        ClassFromBytes(ref arrayTypeUrl, ref entryBytes), index++);
                }
            }

            return arr;
        }

        #endregion

        // not good. object gets moved by clr in memory causes multiple serializations of an object
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IntPtr GetObjectId2(object clazz)
        {
            //var a = __makeref(clazz);
            //var b = **(IntPtr**)&a;

            var clazzPtr = IntPtr.Zero;
            // https://stackoverflow.com/a/10861731 not pinning .__.
            return clazzPtr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Type GetType2(string url)
        {
            var type = Type.GetType(url); // very slow in performance testing
            if (type != null) return type;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(url);
                if (type != null) return type;
            }
            throw new ArgumentException($"Can't find class {url}");
        }
    }
}
