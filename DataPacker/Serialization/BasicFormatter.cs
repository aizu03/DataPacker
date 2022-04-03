using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using DataPacker.Readers;
using DataPacker.Writers;
using static DataPacker.ByteHelper;

namespace DataPacker.Serialization
{
    public class BasicFormatter : IDisposable
    {
        private protected readonly Encoding stringEncoding;
        private protected readonly Dictionary<string, Type> typeTable = new(); // GetType() is very slow. lookup table 3x faster
        private protected readonly Dictionary<object, int?> objectIndexes = new(); // object, index of object 
        private protected readonly List<object> objects = new();

        public BasicFormatter(Encoding stringEncoding)
        {
            this.stringEncoding = stringEncoding;
        }

        public BasicFormatter()
        {
            stringEncoding = Encoding.Unicode;
        }

        public void Dispose()
        {
            typeTable.Clear();
        }

        #region Serialize

        public byte[] Serialize(object clazz)
        {
            if (clazz == null) throw new ArgumentException("Object is null!");
            var bytes = ClassToBytes(clazz);
            objectIndexes.Clear();
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
        private byte[] ClassToBytes(object clazz)
        {
            // Save current index of object
            // instead of using memory address of object, use the hash code.
            // https://stackoverflow.com/a/751146
            // Currently no checks for hash collision that's up to the user
            if (!objectIndexes.ContainsKey(clazz))
                objectIndexes[clazz] = objectIndexes.Count;

            // Debug.WriteLine($"Serialize.. {clazz.GetType().Name} {id:X}");

            using var ms = new MemoryStream();
            using var writer = new WriterSequential(ms, stringEncoding);

            foreach (var field in clazz.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var fieldValue = field.GetValue(clazz);

                // Add null entry
                if (fieldValue == null)
                {
                    writer.Add((byte)0); // field is null
                    continue;
                }

                // Add primitive types or strings
                var fieldType = field.FieldType;
                if (fieldType.IsPrimitive || fieldType == typeof(string))
                {
                    // Add [1, bytes] field is not null
                    var bytes = GenerateFieldEntry(fieldValue, stringEncoding);
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
                objectIndexes.TryGetValue(fieldValue, out var index);
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
                var classBytes = ClassToBytes(fieldValue);
                var len = classBytes.Length;
                var otherBytes = new byte[len + 1];

                // byte[1, bytes ..]
                otherBytes[0] = 1;
                Buffer.BlockCopy(classBytes, 0, otherBytes, 1, len);

                ;
                writer.Add(otherBytes);
                /*
                writer.Add(otherBytes);

         
                var url = fieldValue.GetType().FullName;
                var urlBytes = urlEncoding.GetBytes(url!);

                // Combine URL and class bytes into 1 sequence
                using var stream = new MemoryStream();

                // byte[1, ...]
                stream.WriteByte(1); // field is not null
                stream.Write(ClassToBytes(fieldValue));



                using var writerUrlClass = new WriterSequential(stream);
                writerUrlClass.Add(urlBytes);
                writerUrlClass.Add(classBytes);
                writerUrlClass.Flush(true);
                // byte[1, URL, bytes]

                writer.Add(stream.ToArray());*/
            }

            writer.Flush(true);
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
        private byte[] ArrayToBytes(Array array, Type type)
        {
            // TODO: Make this better?
            /*
             * If the array was initialized with a size of 0,
             * the array is returned as null for now.
             * This is dirty?
             */

            // byte[0]
            if (array.Length == 0) return new byte[] { 0 }; // field is null
            using var ms = new MemoryStream();
            // field is not null
            // byte[1, ...]
            ms.WriteByte(1); 

            using var arrayWriter = new WriterSequential(ms, stringEncoding);

            // Get array type
            var baseType = type.GetElementType();
            var isBasePrimitiveOrString = baseType.IsPrimitive || baseType == typeof(string);
            foreach (var arr in array)
            {
                if (arr == null)
                {
                    arrayWriter.Add((byte)0); // entry is null
                    arrayWriter.Add((byte)0); // zero data to keep alignment
                    continue;
                }

                arrayWriter.Add((byte)1); // entry is not null   

                // Check if it's another array and recursive call add it
                if (arr.GetType().IsArray)
                {
                    arrayWriter.Add(ArrayToBytes((Array)arr, baseType));
                    continue;
                }

                // Write primitive, string or object
                arrayWriter.Add(isBasePrimitiveOrString ? Generate(arr, stringEncoding) : ClassToBytes(arr));
            }

            // Write the sequence [is null, data]
            arrayWriter.Flush(true);

            return ms.ToArray();
        }

        #endregion

        #region Deserialize

        public T Deserialize<T>(byte[] bytes)
        {
            objects.Clear();
            var url = typeof(T).FullName;
            return (T)ClassFromBytes(ref url!, ref bytes);
        }

        private object ClassFromBytes(ref string url, ref byte[] classBytes)
        {
            // Create empty class
            var classType = GetType2(url);
            var clazz = FormatterServices.GetUninitializedObject(classType);
            var fields = classType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            objects.Add(clazz);

            // Empty class
            if (classBytes.Length == 0) return clazz;

            using var stream = new MemoryStream(classBytes);
            using var sequenceFields = new ReaderSequential(stream);
            sequenceFields.Read(true);

            for (var i = 0; i < fields.Length; i++)
            {
                // Each entry of the sequence is [0/1, field bytes..]
                var field = fields[i];
                var bytes = sequenceFields[i].data;
                if (bytes[0] == 0) continue; // field is null

                // Remove the null indicator
                var fieldBytes = new byte[bytes.Length - 1];
                Buffer.BlockCopy(bytes, 1, fieldBytes, 0, bytes.Length - 1);

                // Primitives or strings
                var fieldType = field.FieldType;
                if (fieldType.IsPrimitive || fieldType == typeof(string))
                {
                    // Parse bytes into primitive field type
                    field.SetValue(clazz, Cast2(fieldType, ref fieldBytes, Encoding.Unicode));
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
                    var idx = BitConverter.ToInt32(bytes, 1);
                    var target = objects[idx];
                    field.SetValue(clazz, target);
                    continue;
                }

                // Other classes
                var otherUrl = fieldType.FullName!;
                field.SetValue(clazz, ClassFromBytes(ref otherUrl, ref fieldBytes));
            }

            return clazz;
        }

        private object ArrayFromBytes(ref byte[] arrayBytes, Type type)
        {
            var baseType = type.GetElementType();
            var isBasePrimitiveOrString = baseType.IsPrimitive || baseType == typeof(string);
            var arrayTypeUrl = baseType.FullName!;
  
            // Read array entries [is null, data]
            using var ms2 = new MemoryStream(arrayBytes);
            using var sequenceReader = new ReaderSequential(ms2);
            sequenceReader.Read(true);

            // Create empty array
            var arraySize = sequenceReader.Entries.Count / 2;
            var arr = Array.CreateInstance(baseType, arraySize);

            // Fill array
            var index = 0;
            for (var j = 0; j < sequenceReader.Entries.Count; j++)
            {
                var entryBytes = sequenceReader.Entries[j].data;

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
                        Cast2(baseType, ref entryBytes, stringEncoding) :
                        ClassFromBytes(ref arrayTypeUrl, ref entryBytes), index++);
                }
            }

            return arr;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private Type GetType2(string url)
        {
            typeTable.TryGetValue(url, out var type);
            if (type != null) return type;
            type = Type.GetType(url);
            if (type != null)
            {
                typeTable[url] = type;
                return type;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(url);
                if (type != null)
                {
                    typeTable[url] = type;
                    return type;
                }
            }
            throw new ArgumentException($"Can't find class {url}");
        }
    }
}