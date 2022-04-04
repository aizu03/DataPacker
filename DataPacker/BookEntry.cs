namespace DataPacker
{
    internal struct BookEntry
    {
        public int begin, end;

        internal BookEntry(int begin, int end)
        {
            this.begin = begin;
            this.end = end;
        }
    }
}
