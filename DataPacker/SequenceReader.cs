using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DataPacker.Readers;

namespace DataPacker
{
    public class SequenceReader : IBaseReader, IReaderIndexed
    {
        private readonly BaseReader reader;
        internal ReaderIndexed? readerIndexed;

        public SequenceReader(Stream stream, DataStructure structure = DataStructure.Sequential, Encoding? 
            stringEncoding = null, bool autoRead = false, bool closeStream = true)
        {
            switch (structure)
            {
                case DataStructure.Sequential:

                    reader = new ReaderSequential(stream, false, stringEncoding);
                    break;

                case DataStructure.SequentialNamed:

                    reader = new ReaderSequential(stream, true, stringEncoding);
                    break;

                case DataStructure.Indexed:
          
                    readerIndexed = new ReaderIndexed(stream, false, stringEncoding);
                    reader = readerIndexed;
                    break;

                case DataStructure.IndexedNamed:

                    readerIndexed = new ReaderIndexed(stream, true, stringEncoding);
                    reader = readerIndexed;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(structure), structure, null);
            }

            if (autoRead) reader.Read(closeStream);
        }

        public void Dispose() => reader.Dispose();

        public List<Entry> Entries => reader.Entries;
        public Dictionary<string, Entry> NamedEntries => reader.NamedEntries;

        public Entry this[int index] => reader[index];
        public Entry this[string name] => reader[name];

        /// <summary>
        /// How many entries have been read
        /// </summary>
        /// <returns></returns>
        public int ReadEntries() => reader.ReadEntries();

        /// <summary>
        /// Read all entries
        /// </summary>
        public void Read(bool closeStream = true) => reader.Read(closeStream);

        /// <summary>
        /// Read all entries starting from index
        /// </summary>
        public int Read(int index, bool closeStream = true) => ((IReaderIndexed)reader).Read(index, closeStream);

        /// <summary>
        /// Read all entries starting from index to another index
        /// </summary>
        public int Read(int indexBegin, int indexEnd, bool closeStream = true) => ((IReaderIndexed)reader).Read(indexBegin, indexEnd, closeStream);
       
        /// <summary>
        /// Read one entry at a specific index
        /// </summary>
        public int ReadOne(int index, bool closeStream = true) => Read(index, index, closeStream);

        /// <summary>
        /// Total entries available to read in a <see cref="DataStructure.Indexed"/>
        /// </summary>
        public int Available() => ((IReaderIndexed)reader).Available();
    }
}
