using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkAllocator
{
    /* one way connection */
    public class Connection
    {
        public Connection(Device _src, Device _dst, int _capacity)
        {
            source = _src;
            destination = _dst;
            currCapacity = _capacity;
            maxCapacity = _capacity;
        }

        public Device source;
        public Device destination;
        public List<Slot> slots = null;
        private List<Link> allocatedLinks = new List<Link>();
        public bool CanAllocateLink(Link link)
        {
            return currCapacity >= link.capacityNeeded;
        }
        public void AllocateLink(Link link)
        {
            if(currCapacity < link.capacityNeeded)
            {
                throw new ApplicationException("cannot allocate link!");
            }
            if (allocatedLinks.Exists(x => x == link))
            {
                throw new ApplicationException("link already exists!");
            }
            allocatedLinks.Add(link);
            currCapacity -= link.capacityNeeded;
        }
        public void DeallocateLink(Link link)
        {
            if (!allocatedLinks.Remove(link))
            {
                throw new ApplicationException("link not exists!");
            }
            currCapacity += link.capacityNeeded;
        }
        public int FreeSlots { get { return slots.Count(x => x.state == Slot.State.FREE); } }

        private int currCapacity;
        private readonly int maxCapacity;
    }
}
