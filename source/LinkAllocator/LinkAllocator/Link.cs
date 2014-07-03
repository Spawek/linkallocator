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
        public List<List<Connection>> additionalSourcePaths = new List<List<Connection>>();
        public Device mainDestination = null;
        public List<Device> additionalDestinations = new List<Device>();
        public List<List<Connection>> additionalDestinationPaths = new List<List<Connection>>();
        public int capacityNeeded;
        public List<Connection> mainPath = null;
        public List<Connection> wholePath
        {
            get 
            {   
                var wholePath = mainPath.GetRange(0, mainPath.Count);
                additionalSourcePaths.ForEach(x => wholePath.AddRange(x));
                additionalDestinationPaths.ForEach(x => wholePath.AddRange(x));

                return wholePath;
            }
        }
        public List<Device> devicesOnWholePath
        {
            get
            { //TODO: use set
                List<Device> devicesOnPath = new List<Device>();
                foreach (Connection conn in wholePath)
                {
                    if (!devicesOnPath.Exists(x => x == conn.source))
                        devicesOnPath.Add(conn.source);
                    if (!devicesOnPath.Exists(x => x == conn.destination)) 
                        devicesOnPath.Add(conn.destination);
                }

                return devicesOnPath;
            }
        }
        public List<int> availableSlots = null;
        public int modulo = -777;
        public int maxCapacityOnPath = -1;
        public const int NOT_ALLOCATED = -666;
        public int allocatedSlot = NOT_ALLOCATED;
        public FixedSlotConstraint fixedSlotConstraint = null;

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
            if (fixedSlotConstraint == null)
            {
                modulo = maxCapacityOnPath / capacityNeeded;
                availableSlots = Enumerable.Range(0, modulo).ToList();
            }
            else // constraint has to be applied
            {
                int maxCapacityOnConstraint = capacityNeeded * fixedSlotConstraint.modulo;
                if (maxCapacityOnConstraint > maxCapacityOnPath)
                    throw new ApplicationException("it shouldnt be like that... just check it");
                modulo = fixedSlotConstraint.modulo;
                int numberOfAvailableSlots = maxCapacityOnPath / maxCapacityOnConstraint;
                availableSlots = Enumerable.Range(fixedSlotConstraint.index, numberOfAvailableSlots).ToList(); 
            }
        }

        public bool TryAllocateSlot(int slot)
        {
            if (CanAllocateSlot(slot))
            {
                AllocateSlot(slot);
                return true;
            }

            return false;
        }

        private void AllocateSlot(int slot)
        {
            wholePath.ForEach(x => x.AllocateSlot(this, slot));
            allocatedSlot = slot;
        }

        private bool CanAllocateSlot(int slot)
        {
            return wholePath.All(x => x.CanAllocateSlot(this, slot));
        }

        public void DeallocateSlot(int slot)
        {
            wholePath.ForEach(x => x.DeallocateSlot(this, slot));
            allocatedSlot = NOT_ALLOCATED;
        }
        
        public override string ToString()
        {
            return name;
        }

        public void SetFixedSlotConstraint(FixedSlotConstraint constraint)
        {
            if (fixedSlotConstraint != null)
                throw new ApplicationException("fixed slot constraint cannot be applied twice!");

            fixedSlotConstraint = constraint;
        }
    }
}
