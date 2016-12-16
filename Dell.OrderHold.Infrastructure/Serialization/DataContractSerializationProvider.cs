using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Serialization
{
    public class DataContractSerializationProvider : ISerializationProvider
    {
        public System.IO.Stream Serialize(object obj)
        {
            DataContractSerializer ser = new DataContractSerializer(obj.GetType());
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, obj);
            return ms;
        }

        public object Deserialize(System.IO.Stream stream, Type type)
        {
            DataContractSerializer ser = new DataContractSerializer(type);
            return ser.ReadObject(stream);
        }
    }
}
