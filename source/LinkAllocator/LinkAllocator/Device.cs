using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkAllocator
{
    public class Device
    {
        public string name;
        public List<Connection> outgoingConnections = new List<Connection>();
        public List<Connection> incomingConnections = new List<Connection>();
        public int mark = 0;  // needed for path finding
        
        public Device(string _name)
        {
            name = _name;
        }

        public override string ToString()
        {
            return name + ": " + mark.ToString();
        }

    }
}
