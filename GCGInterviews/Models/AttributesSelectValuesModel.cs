using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Models
{
    public class AttributesSelectValuesModel
    {
        public string Value { get; set; }

        public string Code { get; set; }

        public int DisplayOrder { get; set; }

        public string SwatchText { get; set; }

        public string Path { get; set; }

        public int VariantDisplayOrder { get; set; }

        public string VariantImagePath { get; set; }

        public string VariantSKU { get; set; }
    }
}
