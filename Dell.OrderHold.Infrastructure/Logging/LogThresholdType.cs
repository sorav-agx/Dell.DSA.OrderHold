using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Logging
{
    public enum LogThresholdType
    {
        Uri,
        HttpMethod,
        Headers,
        Cookies,
        StatusCode
        /*Body // not yet */
    }
}
