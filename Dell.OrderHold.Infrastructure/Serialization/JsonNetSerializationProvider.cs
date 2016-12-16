using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Serialization
{
    public class JsonNetSerializationProvider : ISerializationProvider
    {
        public System.IO.Stream Serialize(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            var str = JsonConvert.SerializeObject(obj);
            return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(str));
        }

        public object Deserialize(System.IO.Stream stream, Type type)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (type == null)
                throw new ArgumentNullException("type");

            byte[] buffer = new byte[16 * 1024];
            byte[] streamBytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                streamBytes = ms.ToArray();
            }

            string str = System.Text.Encoding.UTF8.GetString(streamBytes);
            return JsonConvert.DeserializeObject(str, type);
        }
    }
}
