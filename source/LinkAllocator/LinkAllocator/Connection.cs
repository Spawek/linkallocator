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
        /// <param name="linkBeginPos"></param>
        /// <returns>EMPTY LIST MEANS IMPOSSIBLE - BUGFIX</returns>
        public List<int> GetNeededSlotsForLinkAndBeginPosition(Link link, int linkBeginPos)
        {
            int beginPos = -1;
            if (linkBeginPos == 0)
            {
                beginPos = 0;
            }
            else
            {
                beginPos = linkBeginPos / (link.maxCapacityOnPath / maxCapacity);
            }

            int firstSlot = beginPos / CapacityPerSlot;
            int noOfSlotsNeeded = link.capacityNeeded / CapacityPerSlot;

            /// BUGFIX: it can return slots that not exists when there are more than 1 slots needed and
            ///      it tries to allocated on last slot (coz before ones failed)
            if (firstSlot + noOfSlotsNeeded > slots.Count)
            {
                return new List<int>();
            }

            List<int> neededSlots = Enumerable.Range(firstSlot, noOfSlotsNeeded).ToList();

            return neededSlots;
        }
        public bool CanAllocateSlot(Link link, int linkBeginPos)
        {
            List<int> neededSlots = GetNeededSlotsForLinkAndBeginPosition(link, linkBeginPos);

            if (neededSlots.Count == 0) return false; //BUGFIX

            return CanAllocateSlots(neededSlots);
        }
        private bool CanAllocateSlots(List<int> slotsNumbers)
        {
            return slotsNumbers.All(x => slots[x].state == Slot.State.FREE);
        }
        public void AllocateSlot(Link link, int beginPos)
        {
            List<int> neededSlots = GetNeededSlotsForLinkAndBeginPosition(link, beginPos);

            foreach(int slotNo in neededSlots) //TODO: refactor - use .foreach
            {
                if (slots[slotNo].state != Slot.State.FREE)
                    throw new ApplicationException("cannot allocate taken slot");

                slots[slotNo].state = Slot.State.TAKEN;
                slots[slotNo].slotOWner = link;
            }
        }
        public void DeallocateSlot(Link link, int beginPos)
        {
            List<int> neededSlots = GetNeededSlotsForLinkAndBeginPosition(link, beginPos);

            foreach (int slotNo in neededSlots) //TODO: refactor - use .foreach
            {
                if (slots[slotNo].state != Slot.State.TAKEN)
                    throw new ApplicationException("cannot deallocate not taken slot");

                slots[slotNo].state = Slot.State.FREE;
                slots[slotNo].slotOWner = null;
            }
        }
        public int FreeSlots { get { return slots.Count(x => x.state == Slot.State.FREE); } }
        public void CreateSlots()
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
                slots.Add(new Slot(Slot.State.FREE));
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
