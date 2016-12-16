using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Serialization
{
    public interface ISerializationProvider
    {

        Stream Serialize(object obj);
        object Deserialize(Stream stream, Type type);
    }
}
