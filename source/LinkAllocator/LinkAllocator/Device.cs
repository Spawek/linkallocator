using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkAllocator
{
    class Device
    {
        public string name;
        public List<Connection> connections = new List<Connection>();
        
        public Device(string _name)
        {
            name = _name;
        }

        public void AddConnection(Device dev, int slotsNo)
        {
            connections.Add(new Connection(this, dev, slotsNo));
        }
    }
}
