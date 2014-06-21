using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkAllocator
{
    public class Slot
    {
        public State state;
        public Link slotOWner = null;

        public enum State
        {
            FREE, TAKEN, FORBIDDEN
        }

        public Slot(State _state)
        {
            state = _state;
        }

        public override string ToString()
        {
            string ownerString = slotOWner != null ? " : " + slotOWner.ToString() : String.Empty;

            return state.ToString() + ownerString;
        }
    }
}
