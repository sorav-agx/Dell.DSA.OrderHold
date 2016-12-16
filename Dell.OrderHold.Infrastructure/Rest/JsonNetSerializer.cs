using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Rest
{
    internal class JsonNetSerializer : IMediaTypeSerializer
    {
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public T DeSerialize<T>(string objString)
        {
            return JsonConvert.DeserializeObject<T>(objString);
        }

        public IEnumerable<string> MediaTypes
        {
            get
            {
                return new List<string>()
                {
                    "application/json",
                    "text/json",
                    "application/vnd.collection+json",
                    "application/x-javascript",
                    "text/javascript",
                    "text/x-javascript",
                    "text/x-json"
                };
            }
        }
    }
}
