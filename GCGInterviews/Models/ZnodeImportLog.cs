using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Models
{
    public class ZnodeImportLog
    {
        public int ImportLogId { get; set; }

        public int ImportProcessLogId { get; set; }

        public string ErrorDescription { get; set; }

        public string ColumnName { get; set; }

        public string Data { get; set; }

        public string DefaultErrorValue { get; set; }

        public long? RowNumber { get; set; }

        public string Guid { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }
    }
}
