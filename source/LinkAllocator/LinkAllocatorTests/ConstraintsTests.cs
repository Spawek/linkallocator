using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LinkAllocator;

namespace LinkAllocatorTests
{
    [TestClass]
    public class ConstraintsTests
    {
        [TestMethod]
        public void ForbiddenSlotConstraintTest()
        {
            Topology t = new Topology();

            t.AddDevice("D1");
            t.AddDevice("D2");

            t.AddConnection("C1", "D1", "D2", 20);

            t.AddLink("L1", "D1", "D2", 10);

            t.AddForbiddenSlotConstraint("Constraint-1", "D1", 0, 2);

            t.AllocateLinksPaths();
            t.AllocateSlots();

            Assert.AreEqual(1, t.GetLink("L1").mainPath.Count);
            Assert.AreEqual(t.GetDevice("D1"), t.GetLink("L1").mainPath[0].source);
            Assert.AreEqual(t.GetDevice("D2"), t.GetLink("L1").mainPath[0].destination);
            Assert.AreEqual(2, t.GetConnection("C1").slots.Count);
            Assert.AreEqual(Slot.State.FORBIDDEN, t.GetConnection("C1").slots[0].state);
            Assert.AreEqual("Constraint-1", t.GetConnection("C1").slots[0].constraintName);
            Assert.AreEqual(Slot.State.TAKEN, t.GetConnection("C1").slots[1].state);
            Assert.AreEqual(t.GetLink("L1"), t.GetConnection("C1").slots[1].slotOWner);
        }
    }
}
