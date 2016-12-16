using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Instrumentation
{
    public interface IDBInstrumentationItemRepository
    {
        void Create(InstrumentationItem item);
        InstrumentationItem Get(Guid id);
    }
}
