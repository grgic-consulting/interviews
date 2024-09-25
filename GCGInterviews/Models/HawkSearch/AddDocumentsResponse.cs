using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Models.HawkSearch
{
    public class AddDocumentsResponse
    {
        public string Status { get; set; }
        public SummaryResponse Summary { get; set; }
    }

    public class SummaryResponse
    {
        public int Total { get; set; }
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public int Warnings { get; set; }
    }
}
