using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkAllocator
{
    /* one way connection */
    class Connection
    {
        public Connection(Device _src, Device _dst, int slotsNo)
        {
            source = _src;
            destination = _dst;
            slots = new List<Slot>(slotsNo);
            for (int i = 0; i < slotsNo; i++)
            {
                slots.Add(new Slot(Slot.State.FREE));
            }
        }

        public Device source;
        public Device destination;
        public List<Slot> slots;
    }
}
