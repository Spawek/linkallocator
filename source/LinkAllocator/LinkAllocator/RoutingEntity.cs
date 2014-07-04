using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkAllocator
{
    public struct RoutingEntity
    {
        public string linkName;
        public string deviceConnectedOnOutput;
        public int inputModulo;
        public int outputModulo;
        public int inputIndex;
        public int outputIndex;
        public string inputDevice;
        public string outputDevice;
    }
}
