using System.IO;

namespace DataPacker
{
    public abstract class BaseData
    {
        protected readonly Stream stream;

        protected BaseData(Stream stream)
        {
            this.stream = stream;
        }
    }
}
