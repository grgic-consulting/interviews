using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Models.HawkSearch
{
    public class SubmitField
    {
        public SubmitField()
        {
            this.Values = new List<string>();
        }

        public string Name { get; set; }

        public List<string> Values { get; set; }
    }
}
