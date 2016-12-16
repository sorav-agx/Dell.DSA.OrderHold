using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Logging
{
    public static class Extensions
    {
        public static Dictionary<string, string> ToDictionary(this IEnumerable<KeyValuePair<string, string>> items)
        {
            if (items == null)
                return null;
            Dictionary<string, string> d = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var item in items)
                try
                {
                    d.Add(item.Key, item.Value);
                }
                catch { }

            return d;
        }
    }
}
