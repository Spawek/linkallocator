using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkAllocator
{
    public class Link
    {
        public string name; //channel?
        public Device src;
        public Device dst;
        public int capacityNeeded;
        public List<Connection> path = new List<Connection>();
        public List<List<int>> availableSlotSets = null;

        public Link(string _name, Device _src, Device _dst, int _capacityNeeded)
        {
            name = _name;
            src = _src;
            dst = _dst;
            capacityNeeded = _capacityNeeded;
        }

        public void FindAvailableSlotSets()
        {
            if (path.Count == 0) throw new ApplicationException("empty path!");

            // noone cares about demultiplexing in here - can be implemented but i dont know if its needed

            int slotsNeededOnFirstConnection = capacityNeeded / path[0].CapacityPerSlot;

            List<List<int>> slotSets = new List<List<int>>();
            for (int firstSlot = 0; firstSlot < path[0].slots.Count; firstSlot += slotsNeededOnFirstConnection)
            {
                slotSets.Add(Enumerable.Range(firstSlot, slotsNeededOnFirstConnection).ToList());
            }

            availableSlotSets = slotSets;
        }

        public bool TryAllocateSlotSet(List<int> slotSet)
        {
            if (slotSet.All(CanAllocateSlot))
            {
                slotSet.ForEach(AllocateSlot);
                return true;
            }

            return false;
        }

        private void AllocateSlot(int slotNo)
        {
            path.ForEach(x => x.AllocateSlot(this, slotNo));
        }

        private bool CanAllocateSlot(int slotNo)
        {
            return path.All(x => x.CanAllocateSlot(slotNo));
        }

        public void DeallocateSlotSet(List<int> slotSet)
        {
            slotSet.ForEach(DeallocateSlot);
        }

        private void DeallocateSlot(int slotNo)
        {
            path.ForEach(x => x.DeallocateSlot(slotNo));
        }
        
        public override string ToString()
        {
            return name;
        }
    }
}
