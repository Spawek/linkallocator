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
        public List<int> availableBegins = null;
        public int maxCapacityOnPath = -1;

        public Link(string _name, Device _src, Device _dst, int _capacityNeeded)
        {
            name = _name;
            src = _src;
            dst = _dst;
            capacityNeeded = _capacityNeeded;
        }

        public void FindAvailableBeginPositions()
        {
            if (path.Count == 0) throw new ApplicationException("empty path!");

            maxCapacityOnPath = path.Max(x => x.maxCapacity);

            List<int> availableBeginSlots = Enumerable.Range(0, maxCapacityOnPath / capacityNeeded).ToList();

            //when there are 4 slots of size 10 and link size will be 10, available begins will be 0,10,20,30
            availableBegins = availableBeginSlots.ConvertAll(x => x * capacityNeeded);
        }

        public bool TryAllocateSlot(int beginPos)
        {
            if (CanAllocateSlot(beginPos))
            {
                AllocateSlot(beginPos);
                return true;
            }

            return false;
        }

        private void AllocateSlot(int beginPos)
        {
            path.ForEach(x => x.AllocateSlot(this, beginPos));
        }

        private bool CanAllocateSlot(int beginPos)
        {
            return path.All(x => x.CanAllocateSlot(this, beginPos));
        }

        public void DeallocateSlot(int beginPost)
        {
            path.ForEach(x => x.DeallocateSlot(this, beginPost));
        }
        
        public override string ToString()
        {
            return name;
        }
    }
}
