using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dell.OrderHold.Infrastructure.Logging
{
    public interface IDBLogItemRepository
    {
        LogItem Create(LogItem item);
        LogItem Get(string id, bool includeProperties = false);
        void Delete(string id);
        RepositoryCollection<LogItem> GetAllBySource(string source, DateTime startDate, DateTime endDate, IPaging paging, ISorting sorting, bool includeProperties = false);
        RepositoryCollection<LogItem> GetAllBySeverity(string severity, DateTime startDate, DateTime endDate, IPaging paging, ISorting sorting, bool includeProperties = false);
        RepositoryCollection<LogItem> GetAllByCorrelationId(string correlationId, IPaging paging, ISorting sorting, bool includeProperties = false);
        RepositoryCollection<LogItem> GetAllByText(string text, DateTime startDate, DateTime endDate, IPaging paging, ISorting sorting, bool includeProperties = false);
    }

    public class RepositoryCollection<T>
    {
        /// <summary>
        /// Items returned from the repository.
        /// </summary>
        public List<T> Items { get; set; }
        /// <summary>
        /// Total number of records in the system of record.
        /// </summary>
        public int Total { get; set; }
    }
}
