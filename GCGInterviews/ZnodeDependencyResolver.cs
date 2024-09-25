using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCGInterviews
{
    public static class ZnodeDependencyResolver
    {
        public static T GetService<T>() where T : class
        {
            // Simplified for test purposes
            return null;
        }
    }
}
