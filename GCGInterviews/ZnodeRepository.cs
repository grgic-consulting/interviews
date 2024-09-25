using GCGInterviews.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews
{
    // Simplified for test purposes
    public class ZnodeRepository<T> : IZnodeRepository<T> where T : class
    {
        // Simplified for test purposes
        public IQueryable<T> Table { get; }
        public T Insert(T model)
        {
            // Simplified for test purposes
            return model;
        }

        public bool Update(T model)
        {
            // Simplified for test purposes
            return true;
        }

        public bool Delete(string whereClause)
        {
            // Simplified for test purposes
            return true;
        }

        public bool Delete(T model)
        {
            // Simplified for test purposes
            return true;
        }
    }
}
