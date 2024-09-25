using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Models
{
    public class ZnodePriceListPortal
    {
        public int PriceListPortalId { get; set; }

        public int PriceListId { get; set; }

        public int PortalId { get; set; }

        public int? Precedence { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public int ModifiedBy { get; set; }

        public DateTime ModifiedDate { get; set; }
    }
}
