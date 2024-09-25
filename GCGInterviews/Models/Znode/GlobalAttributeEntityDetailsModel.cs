using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Models
{
    public class  GlobalAttributeEntityDetailsModel
    {
        public int EntityId { get; set; }

        public string EntityType { get; set; }

        public bool HasChildAccount { get; set; }

        public List<GlobalAttributeValuesModel> Attributes { get; set; }

        public string FamilyCode { get; set; }
    }
}
