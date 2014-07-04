using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkAllocator
{
    public class RoutingTable
    {
        public string device;
        public List<RoutingEntity> routingEntities = new List<RoutingEntity>();

        public RoutingTable(string dev)
        {
            device = dev;   
        }
    }
}
