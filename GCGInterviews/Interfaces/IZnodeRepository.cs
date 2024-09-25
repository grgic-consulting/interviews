using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Interfaces
{
    public interface IZnodeRepository<T>
    {
        bool Update(T model);

        T Insert(T model);

        bool Delete(string whereClause);

        bool Delete(T entity);

        IQueryable<T> Table { get; }
    }
}
