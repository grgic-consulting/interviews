using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Models
{
    public class ZnodeImportProcessLog
    {
        public int ImportProcessLogId { get; set; }

        public int? ImportTemplateId { get; set; }

        public string Status { get; set; }

        public DateTime ProcessStartedDate { get; set; }

        public DateTime? ProcessCompletedDate { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }

        public int? ERPTaskSchedulerId { get; set; }

        public long? SuccessRecordCount { get; set; }

        public long? FailedRecordcount { get; set; }

        public long? TotalProcessedRecords { get; set; }
    }
}
