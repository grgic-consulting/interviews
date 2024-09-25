using GCGInterviews.Models.HawkSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Models
{
    public class SubmitDocument
    {
        public SubmitDocument()
        {
            this.IdentityField = new Dictionary<string, string>();
            this.Fields = new List<SubmitField>();
        }

        public Dictionary<string, string> IdentityField { get; set; }

        public List<SubmitField> Fields { get; set; }
    }
}
