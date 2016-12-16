using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Rest
{
    public class Cookie
    {
        public Cookie(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("key");

            this.Key = key;
            this.Value = value;
        }
        public string Key { get; internal set; }
        public string Value { get; internal set; }
    }
}
