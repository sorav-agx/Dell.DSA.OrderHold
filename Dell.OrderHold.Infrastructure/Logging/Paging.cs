using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Logging
{
    public class Paging : IPaging
    {
        public Paging() { }
        public Paging(int pageSize, int pageIndex)
        {
            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
        }
        public int? PageSize { get; set; }

        public int? PageIndex { get; set; }
    }
}
