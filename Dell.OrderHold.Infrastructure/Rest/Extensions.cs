using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Rest
{
    public static class Extensions
    {
        public static string GetHeaderValue(this IEnumerable<Header> headers, string key)
        {
            if (headers == null || string.IsNullOrWhiteSpace(key))
                return null;
            var header = headers.FirstOrDefault(d => d.Key.ToLower().Equals(key.ToLower()));

            if (header == null)
                return null;

            return header.Value;
        }

        public static string GetCookieValue(this IEnumerable<Cookie> cookies, string key)
        {
            if (cookies == null || string.IsNullOrWhiteSpace(key))
                return null;
            var cookie = cookies.FirstOrDefault(d => d.Key.ToLower().Equals(key.ToLower()));

            if (cookie == null)
                return null;

            return cookie.Value;
        }
    }
}
