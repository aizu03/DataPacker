using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataPacker.Serialization
{
    public interface IFormatter
    {
        public byte[] Serialize(object clazz);
        public T Deserialize<T>(byte[] bytes);
    }
}
