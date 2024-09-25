using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Models.HawkSearch
{
    public class FieldDefinition
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public bool IncludeInResults { get; set; } = true;
    }
}
