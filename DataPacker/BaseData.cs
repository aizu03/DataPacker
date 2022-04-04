using System.IO;

namespace DataPacker
{
    internal abstract class BaseData
    {
        protected internal readonly Stream stream;

        protected internal BaseData(Stream stream)
        {
            this.stream = stream;
        }
    }
}
