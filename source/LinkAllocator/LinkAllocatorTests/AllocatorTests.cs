using System;
using LinkAllocator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LinkAllocatorTests
{
    [TestClass]
    public class AllocatorTests
    {
        [TestMethod]
        public void TolopogyCreation()
        {
            Topology t = new Topology();
            t.AddDevice("D1");
            t.AddDevice("D2");

            t.AddConnection("C1", "D1", "D2", 10);

            t.AddLink("L1", "D1", "D2", 10);
        }

        [TestMethod]
        public void SimplePathFind()
        {
            Topology t = new Topology();

            t.AddDevice("D1");
            t.AddDevice("D2");

            t.AddConnection("C1", "D1", "D2", 10);

            t.AddLink("L1", "D1", "D2", 10);

            t.AllocateLinksPaths();

            Assert.AreEqual(1, t.GetLink("L1").path.Count);
            Assert.AreEqual(t.GetDevice("D1"), t.GetLink("L1").path[0].source);
            Assert.AreEqual(t.GetDevice("D2"), t.GetLink("L1").path[0].destination);
        }

        [TestMethod]
        public void SplittedPathFind()
        {
            Topology t = new Topology();

            t.AddDevice("D1");
            t.AddDevice("D2");
            t.AddDevice("D3");

            // path 1
            t.AddConnection("C1", "D1", "D2", 10);

            // path 2
            t.AddConnection("C2", "D1", "D3", 10);
            t.AddConnection("C3", "D3", "D2", 10);

            t.AddLink("L1", "D1", "D2", 10);
            t.AddLink("L2", "D1", "D2", 10);

            t.AllocateLinksPaths();

            Assert.AreEqual(1, t.GetLink("L1").path.Count);
            Assert.AreEqual(t.GetDevice("D1"), t.GetLink("L1").path[0].source);
            Assert.AreEqual(t.GetDevice("D2"), t.GetLink("L1").path[0].destination);

            Assert.AreEqual(2, t.GetLink("L2").path.Count);
            Assert.AreEqual(t.GetDevice("D1"), t.GetLink("L2").path[0].source);
            Assert.AreEqual(t.GetDevice("D3"), t.GetLink("L2").path[0].destination);
            Assert.AreEqual(t.GetDevice("D3"), t.GetLink("L2").path[1].source);
            Assert.AreEqual(t.GetDevice("D2"), t.GetLink("L2").path[1].destination);
        }

        public void TwoWayPathFind()
        {
            Topology t = new Topology();

            t.AddDevice("D1");
            t.AddDevice("D2");

            t.AddConnection("C1", "D1", "D2", 10);
            t.AddConnection("C2", "D1", "D2", 10);

            t.AddLink("L1", "D1", "D2", 10);
            t.AddLink("L2", "D2", "D1", 10);

            Assert.AreEqual(1, t.GetLink("L1").path.Count);
            Assert.AreEqual(t.GetDevice("D1"), t.GetLink("L1").path[0].source);
            Assert.AreEqual(t.GetDevice("D2"), t.GetLink("L1").path[0].destination);

            Assert.AreEqual(1, t.GetLink("L2").path.Count);
            Assert.AreEqual(t.GetDevice("D2"), t.GetLink("L2").path[0].source);
            Assert.AreEqual(t.GetDevice("D1"), t.GetLink("L2").path[0].destination);
        }

        [TestMethod]
        public void SimpleSlotFind()
        {
            Topology t = new Topology();

            t.AddDevice("D1");
            t.AddDevice("D2");

            t.AddConnection("C1", "D1", "D2", 10);

            t.AddLink("L1", "D1", "D2", 10);

            t.AllocateLinksPaths();
            t.AllocateSlots();

            Assert.AreEqual(1, t.GetLink("L1").path.Count);
            Assert.AreEqual(t.GetDevice("D1"), t.GetLink("L1").path[0].source);
            Assert.AreEqual(t.GetDevice("D2"), t.GetLink("L1").path[0].destination);
            Assert.AreEqual(1, t.GetConnection("C1").slots.Count);
            Assert.AreEqual(t.GetLink("L1"), t.GetConnection("C1").slots[0].slotOWner);
        }

        [TestMethod]
        public void Find8SlotsSameSize()
        {
            Topology t = new Topology();

            t.AddDevice("D1");
            t.AddDevice("D2");

            t.AddConnection("C1", "D1", "D2", 80);

            t.AddLink("L1", "D1", "D2", 10);
            t.AddLink("L2", "D1", "D2", 10);
            t.AddLink("L3", "D1", "D2", 10);
            t.AddLink("L4", "D1", "D2", 10);
            t.AddLink("L5", "D1", "D2", 10);
            t.AddLink("L6", "D1", "D2", 10);
            t.AddLink("L7", "D1", "D2", 10);
            t.AddLink("L8", "D1", "D2", 10);

            t.AllocateLinksPaths();
            t.AllocateSlots();

            Assert.AreEqual(1, t.GetLink("L1").path.Count);
            Assert.AreEqual(t.GetDevice("D1"), t.GetLink("L1").path[0].source);
            Assert.AreEqual(t.GetDevice("D2"), t.GetLink("L1").path[0].destination);
            Assert.AreEqual(8, t.GetConnection("C1").slots.Count);
            Assert.AreEqual(t.GetLink("L1"), t.GetConnection("C1").slots[0].slotOWner);
            Assert.AreEqual(t.GetLink("L2"), t.GetConnection("C1").slots[1].slotOWner);
            Assert.AreEqual(t.GetLink("L3"), t.GetConnection("C1").slots[2].slotOWner);
            Assert.AreEqual(t.GetLink("L4"), t.GetConnection("C1").slots[3].slotOWner);
            Assert.AreEqual(t.GetLink("L5"), t.GetConnection("C1").slots[4].slotOWner);
            Assert.AreEqual(t.GetLink("L6"), t.GetConnection("C1").slots[5].slotOWner);
            Assert.AreEqual(t.GetLink("L7"), t.GetConnection("C1").slots[6].slotOWner);
            Assert.AreEqual(t.GetLink("L8"), t.GetConnection("C1").slots[7].slotOWner);
        }

        /// <summary>
        /// after first allocation of L1 on C1 it has to be deallocated 
        /// (or L2 allocation wont be possible at all) and L1 should be allocated C2
        /// </summary>
        [TestMethod]
        public void PathAllocationNeedsSorting()
        {
            Topology t = new Topology();

            t.AddDevice("D1");
            t.AddDevice("D2");
            t.AddDevice("D3");

            t.AddConnection("C1", "D1", "D2", 20);
            t.AddConnection("C2", "D1", "D3", 10);
            t.AddConnection("C3", "D3", "D2", 10);

            t.AddLink("L1", "D1", "D2", 10);
            t.AddLink("L2", "D1", "D2", 20);

            t.AllocateLinksPaths();
            t.AllocateSlots();
            
            //ASSERIONS NOT NEEDED - NO EXCEPTION SHOULD BE ENOUGH!
        }

        /// <summary>
        ///      D4
        ///      |
        ///      |2x
        ///      |
        ///      D3
        ///     /  \
        /// 1x /    \ 1x
        ///   /      \
        ///  D1       D2
        /// </summary>
        [TestMethod]
        public void SlotAllocationWithDemultiplexing()
        {
            Topology t = new Topology();

            t.AddDevice("D1");
            t.AddDevice("D2");
            t.AddDevice("D3");
            t.AddDevice("D4");

            t.AddConnection("C1", "D1", "D3", 10);
            t.AddConnection("C2", "D2", "D3", 10);
            t.AddConnection("C3", "D3", "D4", 20);

            t.AddLink("L1", "D1", "D4", 10);
            t.AddLink("L2", "D2", "D4", 10);

            t.AllocateLinksPaths();
            t.AllocateSlots();
        }

        /// <summary>
        ///      D4
        ///      |
        ///      |8x
        ///      |
        ///      D3
        ///     /  \
        /// 4x /    \ 1x
        ///   /      \
        ///  D1       D2
        /// </summary>
        [TestMethod]
        public void SlotAllocationWithDemultiplexing_ComplexCase()
        {
            Topology t = new Topology();

            t.AddDevice("D1");
            t.AddDevice("D2");
            t.AddDevice("D3");
            t.AddDevice("D4");

            t.AddConnection("C1", "D1", "D3", 40);
            t.AddConnection("C2", "D2", "D3", 10);
            t.AddConnection("C3", "D3", "D4", 80);

            t.AddLink("L1", "D1", "D4", 10);
            t.AddLink("L2", "D1", "D4", 20);
            t.AddLink("L3", "D1", "D4", 10);
            t.AddLink("L4", "D2", "D4", 10);

            t.AllocateLinksPaths();
            t.AllocateSlots();
        }

        /// <summary>
        ///   D3       D4
        ///    \       /
        ///  1x \     / 1x
        ///      \   /
        ///       \ /
        ///        D2
        ///        |
        ///        | 2x
        ///        |
        ///        D1
        /// </summary>
        [TestMethod]
        public void SlotAllocationWithMultiplexing()
        {
            Topology t = new Topology();

            t.AddDevice("D1");
            t.AddDevice("D2");
            t.AddDevice("D3");
            t.AddDevice("D4");

            t.AddConnection("C1", "D1", "D2", 20);
            t.AddConnection("C2", "D2", "D3", 10);
            t.AddConnection("C3", "D2", "D4", 10);

            t.AddLink("L1", "D1", "D3", 10);
            t.AddLink("L2", "D1", "D4", 10);

            t.AllocateLinksPaths();
            t.AllocateSlots();
        }
    }
}
