using System.Text;

namespace DataPacker.Serialization
{
    public class CompactFormatter
    {
        public static byte[] Serialize(object clazz)
        {
            using var formatter = new BasicFormatter();
            return formatter.Serialize(clazz);
        }    

        public static byte[] Serialize(object clazz, Encoding stringEncoding)
        {
            using var formatter = new BasicFormatter(stringEncoding);
            return formatter.Serialize(clazz);
        }

        public static T Deserialize<T>(byte[] bytes)
        {
            using var formatter = new BasicFormatter();
            return formatter.Deserialize<T>(bytes);
        } 
        
        public static T Deserialize<T>(byte[] bytes, Encoding stringEncoding)
        {
            using var formatter = new BasicFormatter(stringEncoding);
            return formatter.Deserialize<T>(bytes);
        }
    }
}
