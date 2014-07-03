using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkAllocator
{
    public class FixedSlotConstraint
    {
        public int index;
        public int modulo;

        public FixedSlotConstraint(int _index, int _modulo)
        {
            index = _index;
            modulo = _modulo;
        }
    }
}
