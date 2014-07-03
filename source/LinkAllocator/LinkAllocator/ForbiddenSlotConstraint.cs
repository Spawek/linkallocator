using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkAllocator
{
    public struct ForbiddenSlotConstraint
    {
        public string name;
        public int index;
        public int modulo;

        public ForbiddenSlotConstraint(string _name, int _index, int _modulo)
        {
            name = _name;
            index = _index;
            modulo = _modulo;
        }
    }
}
