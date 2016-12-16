using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Logging
{
    public class Sorting : ISorting
    {
        public Sorting() { }
        public Sorting(string sortBy)
        {
            this.Sort = sortBy;
        }

        public string Sort { get; set; }
    }
}
