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
        public Connection(string _name, Device _src, Device _dst, int _capacity)
        {
            name = _name;
            source = _src;
            destination = _dst;
            currCapacity = _capacity;
            maxCapacity = _capacity;
        }

        public string name;
        public Device source;
        public Device destination;
        public List<Slot> slots = null;
        private List<Link> allocatedLinks = new List<Link>();

        public bool IsLinkAllocated(Link link)
        {
            return allocatedLinks.Any(x => x == link);
        }
        public bool CanAllocateLink(Link link)
        {
            return currCapacity >= link.capacityNeeded;
        }
        public void AllocatePath(Link link)
        {
            if (currCapacity < link.capacityNeeded)
                throw new ApplicationException("cannot allocate link!");
            if (allocatedLinks.Exists(x => x == link))
                throw new ApplicationException("link already exists!");

            allocatedLinks.Add(link);
            currCapacity -= link.capacityNeeded;
        }
        public void DeallocatePath(Link link)
        {
            if (!allocatedLinks.Remove(link))
                throw new ApplicationException("link not exists!");
            
            currCapacity += link.capacityNeeded;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="link"></param>
        /// <param name="slot"></param>
        /// <returns>EMPTY LIST MEANS IMPOSSIBLE - BUGFIX</returns>
        public List<int> GetNeededSlotsForLinkAndBeginPosition(Link link, int slot)
        {
            int currSlot = slot * maxCapacity / link.maxCapacityOnPath;
            int currModulo = slots.Count;
            int currConnectionModuloForLink = maxCapacity / link.capacityNeeded;
            int noOfAllocatedSlots = currModulo / currConnectionModuloForLink;

            List<int> neededSlots = new List<int>();
            for (int i = currSlot; i < currModulo; i += currConnectionModuloForLink)
            {
                neededSlots.Add(i);
            }

            return neededSlots;
        }
        public bool CanAllocateSlot(Link link, int slot)
        {
            List<int> neededSlots = GetNeededSlotsForLinkAndBeginPosition(link, slot);

            return CanAllocateSlots(neededSlots);
        }
        private bool CanAllocateSlots(List<int> slotsNumbers)
        {
            return slotsNumbers.All(x => slots[x].IsAvailable());
        }
        public void AllocateSlot(Link link, int slot)
        {
            List<int> neededSlots = GetNeededSlotsForLinkAndBeginPosition(link, slot);

            foreach(int slotNo in neededSlots) //TODO: refactor - use .foreach
            {
                if (!slots[slotNo].IsAvailable())
                    throw new ApplicationException("cannot allocate taken slot");

                slots[slotNo].Allocate(link);
            }
        }
        public void DeallocateSlot(Link link, int slot)
        {
            List<int> neededSlots = GetNeededSlotsForLinkAndBeginPosition(link, slot);

            neededSlots.ForEach(x => slots[x].Deallocate());
        }
        public void CreateSlots()
        {
            if (allocatedLinks.Count > 0)
            {
                int minNeededCapacity = allocatedLinks.Min(x => x.capacityNeeded);

                if (maxCapacity % minNeededCapacity != 0)
                    throw new ApplicationException("Cannot calculate slots number");

                int slotsNumbers = maxCapacity / minNeededCapacity;

                if (allocatedLinks.Any(x => maxCapacity % x.capacityNeeded != 0))
                    throw new ApplicationException("Correct slots number cannot be calculated");

                slots = new List<Slot>();
                for (int i = 0; i < slotsNumbers; i++)
                {
                    slots.Add(new Slot());
                }
            }
        }

        private int currCapacity;
        public readonly int maxCapacity;
        public int CapacityPerSlot { get { return maxCapacity / slots.Count(); } }

        public override string ToString()
        {
            return name;
        }
    }
}
