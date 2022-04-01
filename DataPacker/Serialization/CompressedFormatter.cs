using System.Text;

namespace DataPacker.Serialization
{
    internal class CompressedFormatter : BaseFormatter
    {

        // TODO:
        /*
        Store path names in a book and use index instead of URL
        byte[1, URL <- index]

        and write the book at the end of the serialized array
          
        To slightly compress everything   
        */

        public CompressedFormatter(Encoding stringEncoding) : base(stringEncoding)
        {
            throw new System.NotImplementedException();
        }

        public CompressedFormatter()
        {
            throw new System.NotImplementedException();
        }

        public override byte[] Serialize(object clazz)
        {
            throw new System.NotImplementedException();
        }

        public override T Deserialize<T>(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }
    }
}