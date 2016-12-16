using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Rest
{
    public interface IMediaTypeSerializer
    {
        string Serialize(object obj);
        T DeSerialize<T>(string objString);
        IEnumerable<string> MediaTypes { get; }
    }
}
