using GCGInterviews.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews.Interfaces
{
    public interface IGlobalAttributeGroupEntityService
    {
        GlobalAttributeEntityDetailsModel GetEntityAttributeDetails(int entityId, string entityType);
    }
}
