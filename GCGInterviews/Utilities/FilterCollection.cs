using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Utilities
{
    public class FilterCollection : List<FilterTuple>
    {
        public void Add(string filterName, string filterOperator, string filterValue)
        {
            Add(new FilterTuple(filterName, filterOperator, filterValue));
        }
    }
}
