using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Models.HawkSearch
{
    public class RemoveDocumentsResponse
    {
        public string Status { get; set; }
        public SummaryResponse Summary { get; set; }
        public IReadOnlyList<ItemStatusResponse> Items { get; set; }
    }

    public class ItemStatusResponse
    {
        public string Item { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
