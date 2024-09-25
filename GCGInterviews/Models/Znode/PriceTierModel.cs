using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Models
{
    public class PriceTierModel
    {
        public int PriceTierId { get; set; }

        public int PriceListId { get; set; }

        public string SKU { get; set; }

        public decimal? Price { get; set; }

        public decimal? Quantity { get; set; }

        public decimal? MinQuantity { get; set; }

        public decimal? MaxQuantity { get; set; }
    }
}
