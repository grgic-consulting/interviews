using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Utilities
{
    public class FilterTuple : Tuple<string, string, string>
    {
        public string FilterName => base.Item1;

        public string FilterOperator => base.Item2;

        public string FilterValue => base.Item3;

        public FilterTuple(string filterName, string filterOperator, string filterValue)
            : base(filterName, filterOperator, filterValue)
        {
        }
    }
}
