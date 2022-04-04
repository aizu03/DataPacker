namespace DataPacker.Readers
{
    internal interface IReaderIndexed
    {
        int Available();
        void Read(bool closeStream);
        int Read(int index, bool closeStream);
        int Read(int indexBegin, int indexEnd, bool closeStream);
        int ReadOne(int index, bool closeStream);
    }
}