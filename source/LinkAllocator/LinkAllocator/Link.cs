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

        public Link(string _name, Device _src, Device _dst, int _capacityNeeded)
        {
            name = _name;
            src = _src;
            dst = _dst;
            capacityNeeded = _capacityNeeded;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
