using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Models
{
    public class CategoryProductModel
    {
        public int PimCategoryProductId { get; set; }

        public int? PimCategoryId { get; set; }

        public int? PimCatalogId { get; set; }

        public int PimProductId { get; set; }

        public int? DisplayOrder { get; set; }

        public bool Status { get; set; }

        public string ProductName { get; set; }

        public string ImagePath { get; set; }

        public string ProductType { get; set; }

        public string AttributeFamily { get; set; }

        public string SKU { get; set; }

        public bool? IsActive { get; set; }

        public string Assortment { get; set; }

        public string CategoryName { get; set; }

        public string CategoryCode { get; set; }

        public int? Categoryid { get; set; }

        public int? UserId { get; set; }
    }
}
