using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Models.HawkSearch
{
    public class RebuildIndexResponseModel
    {
        public string IndexName { get; set; }
        public bool IsSuccess { get; set; }
        public IndexingResponse[] IndexingResponses { get; set; }
    }

    public class IndexingResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }
}
