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

        /// <summary>
        ///    D3
        ///     |
        ///     | 8x
        ///     |
        ///    D2
        ///     |
        ///     | 2x
        ///     |
        ///    D1
        /// </summary>
        [TestMethod]
        public void FixedSlotConstraintTest()
        {
            Topology t = new Topology();

            t.AddDevice("D1");
            t.AddDevice("D2");
            t.AddDevice("D3");

            t.AddConnection("C1", "D1", "D2", 20);
            t.AddConnection("C2", "D2", "D3", 80);

            t.AddLink("L1", "D1", "D3", 10);

            t.AddFixedSlotConstraint("L1", 3, 4);

            t.AllocateLinksPaths();
            t.AllocateSlots();
        }

        /// <summary>
        ///    O1
        ///     |
        ///     | 8x
        ///     |     2x
        ///    D1--------I2
        ///     |
        ///     | 2x
        ///     |
        ///    I1
        /// </summary>
        [TestMethod]
        public void FixedSlotConstraintTest_complexCase()
        {
            Topology t = new Topology();

            t.AddDevice("O1");
            t.AddDevice("D1");
            t.AddDevice("I1");
            t.AddDevice("I2");

            t.AddConnection("C1", "I1", "D1", 20);
            t.AddConnection("C2", "I2", "D1", 20);
            t.AddConnection("C3", "D1", "O1", 80);

            t.AddLink("L1", "I1", "O1", 10);
            t.AddLink("L2", "I2", "O1", 10);

            t.AddFixedSlotConstraint("L1", 3, 4);
            t.AddFixedSlotConstraint("L2", 3, 4);

            t.AllocateLinksPaths();
            t.AllocateSlots();
        }
    }
}
