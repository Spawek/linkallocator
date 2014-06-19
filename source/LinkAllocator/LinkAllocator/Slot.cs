using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkAllocator
{
    class Slot
    {
        State state;
        Link slotOWner = null;

        public enum State
        {
            FREE, TAKEN, FORBIDDEN
        }

        public Slot(State _state)
        {
            state = _state;
        }
    }
}
