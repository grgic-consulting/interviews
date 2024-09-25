using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Znode.Engine.ERPConnector;

namespace GCGInterviews
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var indexer = new HawkSearchIndexHelper();
            indexer.ReindexProducts();
        }
    }
}
