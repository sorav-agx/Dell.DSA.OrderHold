using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Logging
{
    public interface IPaging
    {
        int? PageSize { get; set; }
        int? PageIndex { get; set; }
    }
}
