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
        public Device mainSource = null;
        public List<Device> additionalSources = new List<Device>();
        public Device mainDestination = null;
        public List<Device> additionalDestinations = new List<Device>();
        public int capacityNeeded;
        public List<Connection> mainPath = new List<Connection>();
        public List<Device> additionalPaths = new List<Device>();
        public List<int> availableBegins = null;
        public int maxCapacityOnPath = -1;

        public Link(string _name, Device _src, Device _dst, int _capacityNeeded)
        {
            if (_capacityNeeded <= 0)
                throw new ArgumentException("capacityNeeded has to be > 0");

            name = _name;
            mainSource = _src;
            mainDestination = _dst;
            capacityNeeded = _capacityNeeded;
        }

        public Link(string _name, List<Device> _src, List<Device> _dst, int _capacityNeeded)
        {
            if (_capacityNeeded <= 0)
                throw new ArgumentException("capacityNeeded has to be > 0");
            if (_src.Count == 0)
                throw new ApplicationException("no source in link");
            if (_dst.Count == 0)
                throw new ApplicationException("no destinationn in link");

            name = _name;
            mainSource = _src[0];
            additionalSources = _src.GetRange(1, _src.Count - 1);
            mainDestination = _dst[0];
            additionalDestinations = _dst.GetRange(1, _dst.Count - 1);
            capacityNeeded = _capacityNeeded;
        }

        public void FindAvailableBeginPositions()
        {
            if (mainPath.Count == 0) throw new ApplicationException("empty path!");

            maxCapacityOnPath = mainPath.Max(x => x.maxCapacity);

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
            mainPath.ForEach(x => x.AllocateSlot(this, beginPos));
        }

        private bool CanAllocateSlot(int beginPos)
        {
            return mainPath.All(x => x.CanAllocateSlot(this, beginPos));
        }

        public void DeallocateSlot(int beginPost)
        {
            mainPath.ForEach(x => x.DeallocateSlot(this, beginPost));
        }
        
        public override string ToString()
        {
            return name;
        }
    }
}
