using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkAllocator
{
    class Link
    {
        public string name; //channel?
        public Device src;
        public Device dst;
        public int slotsNeeded;

        public Link(string _name, Device _src, Device _dst, int _slotsNeeded)
        {
            name = _name;
            src = _src;
            dst = _dst;
            slotsNeeded = _slotsNeeded;
        }
    }
}
