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
        private List<ForbiddenSlotConstraint> forbiddenSlotConstraints = new List<ForbiddenSlotConstraint>();

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

                if (allocatedLinks.Any(x => maxCapacity % x.capacityNeeded != 0))
                    throw new ApplicationException("Correct slots number cannot be calculated");

                int slotsNumberNeededForTheSmallestLink = maxCapacity / minNeededCapacity;
                int slotsNumberNeeded;
                if(forbiddenSlotConstraints.Count != 0)
                {
                    int slotsNumberNeededForConstraints = forbiddenSlotConstraints.Max(x => x.modulo);
                    slotsNumberNeeded = Math.Max(slotsNumberNeededForTheSmallestLink, slotsNumberNeededForConstraints);
                }
                else
                {
                    slotsNumberNeeded = slotsNumberNeededForTheSmallestLink;
                }

                CreateSlots(slotsNumberNeeded);
                forbiddenSlotConstraints.ForEach(x => ApplyConstraint(x));
            }
        }

        private void ApplyConstraint(ForbiddenSlotConstraint constraint)
        {
            if (slots == null)
                throw new ApplicationException("Cannot apply constraint when slots are not created");
            if (slots.Count == 0)
                throw new ApplicationException("Cannot apply constraint when slots are empty");
            if (slots.Count < constraint.index)
                throw new ApplicationException("Slots number cannot be smaller than constraint index");
            if (slots.Count < constraint.modulo)
                throw new ApplicationException("Slots number cannot be smaller than constraint modulo");

            for (int i = constraint.index; i < slots.Count; i += constraint.modulo)
            {
                slots[i].Forbid(constraint.name);
            }
        }

        private void CreateSlots(int slotsNumber)
        {
            slots = new List<Slot>();
            for (int i = 0; i < slotsNumber; i++)
            {
                slots.Add(new Slot());
            }
        }

        private int currCapacity;
        public readonly int maxCapacity;
        public int CapacityPerSlot { get { return maxCapacity / slots.Count(); } }

        public override string ToString()
        {
            return name;
        }

        public void AddForbiddenSlotConstraint(ForbiddenSlotConstraint constraint)
        {
            if (currCapacity < maxCapacity / constraint.modulo)
                throw new ApplicationException("more constraint cannot be applied to this device");
            if (forbiddenSlotConstraints.Any(x => x.index == constraint.index && x.modulo == constraint.modulo))
                throw new ApplicationException("this constraint alearedy exists in current device");
            currCapacity -= maxCapacity / constraint.modulo;

            forbiddenSlotConstraints.Add(constraint);
        }
    
    }
}
