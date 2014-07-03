using System;
using LinkAllocator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace LinkAllocatorTests
{
    [TestClass]
    public class SlotAllocationTests
    {
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

            Assert.AreEqual(1, t.GetLink("L1").mainPath.Count);
            Assert.AreEqual(t.GetDevice("D1"), t.GetLink("L1").mainPath[0].source);
            Assert.AreEqual(t.GetDevice("D2"), t.GetLink("L1").mainPath[0].destination);
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

            Assert.AreEqual(1, t.GetLink("L1").mainPath.Count);
            Assert.AreEqual(t.GetDevice("D1"), t.GetLink("L1").mainPath[0].source);
            Assert.AreEqual(t.GetDevice("D2"), t.GetLink("L1").mainPath[0].destination);
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
        ///      O1
        ///      |
        ///      |8x
        ///   1x |    1x
        /// I1---D1------I4
        ///     /  \
        /// 4x /    \ 2x
        ///   /      \
        ///  I2       I3
        /// </summary>
        [TestMethod]
        public void SlotAllocationWithDemultiplexing_ComplexCase()
        {
            Topology t = new Topology();

            t.AddDevice("I1");
            t.AddDevice("I2");
            t.AddDevice("I3");
            t.AddDevice("I4");
            t.AddDevice("D1");
            t.AddDevice("O1");

            t.AddConnection("C1", "I1", "D1", 10);
            t.AddConnection("C2", "I2", "D1", 40);
            t.AddConnection("C3", "I3", "D1", 20);
            t.AddConnection("C4", "I4", "D1", 10);
            t.AddConnection("C5", "D1", "O1", 80);

            t.AddLink("L1", "I1", "O1", 10);

            t.AddLink("L2", "I2", "O1", 20);
            t.AddLink("L3", "I2", "O1", 10);
            t.AddLink("L4", "I2", "O1", 10);

            t.AddLink("L5", "I3", "O1", 5);
            t.AddLink("L6", "I3", "O1", 5);
            t.AddLink("L7", "I3", "O1", 10);

            t.AddLink("L8", "I4", "O1", 5);
            t.AddLink("L9", "I4", "O1", 5);

            t.AllocateLinksPaths();
            t.AllocateSlots();
        }

        /// <summary>
        ///      I1
        ///      |
        ///      |8x
        ///   1x |    1x
        /// O1---D1------O4
        ///     /  \
        /// 4x /    \ 2x
        ///   /      \
        ///  O2       O3
        /// </summary>
        [TestMethod]
        public void SlotAllocationWithMultiplexing_ComplexCase()
        {
            Topology t = new Topology();

            t.AddDevice("O1");
            t.AddDevice("O2");
            t.AddDevice("O3");
            t.AddDevice("O4");
            t.AddDevice("D1");
            t.AddDevice("I1");

            t.AddConnection("C1", "D1", "O1", 10);
            t.AddConnection("C2", "D1", "O2", 40);
            t.AddConnection("C3", "D1", "O3", 20);
            t.AddConnection("C4", "D1", "O4", 10);
            t.AddConnection("C5", "I1", "D1", 80);

            t.AddLink("L1", "I1", "O1", 10);
                                   
            t.AddLink("L2", "I1", "O2", 20);
            t.AddLink("L3", "I1", "O2", 10);
            t.AddLink("L4", "I1", "O2", 10);
                                   
            t.AddLink("L5", "I1", "O3", 5);
            t.AddLink("L6", "I1", "O3", 5);
            t.AddLink("L7", "I1", "O3", 10);
                                   
            t.AddLink("L8", "I1", "O4", 5);
            t.AddLink("L9", "I1", "O4", 5);

            t.AllocateLinksPaths();
            t.AllocateSlots();
        }

        public void SlotAllocationWithDemultiplexing_EvenMoreComplexCase()
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

        /// <summary>
        ///      D3
        ///     /  \
        ///    /    \ 
        ///   /      \
        ///  D1       D2
        /// </summary>
        [TestMethod]
        public void FindSlotsForPathWith2Sources()
        {
            Topology t = new Topology();

            t.AddDevice("D1");
            t.AddDevice("D2");
            t.AddDevice("D3");

            t.AddConnection("C1", "D1", "D3", 10);
            t.AddConnection("C2", "D2", "D3", 10);

            List<string> sources = new List<string>() { "D1", "D2" };
            List<string> destinations = new List<string>() { "D3" };

            t.AddLink("L1", sources, destinations, 10);

            t.AllocateLinksPaths();
            t.AllocateSlots();
        }


        /// <summary>
        ///  D2       D3
        ///   \      /
        ///    \    /
        ///     \  /
        ///      D1
        /// </summary>
        [TestMethod]
        public void FindSlotsForPathWith2Destinations()
        {
            Topology t = new Topology();

            t.AddDevice("D1");
            t.AddDevice("D2");
            t.AddDevice("D3");

            t.AddConnection("C1", "D1", "D2", 10);
            t.AddConnection("C2", "D1", "D3", 10);

            List<string> sources = new List<string>() { "D1" };
            List<string> destinations = new List<string>() { "D2", "D3" };

            t.AddLink("L1", sources, destinations, 10);

            t.AllocateLinksPaths();
            t.AllocateSlots();
        }
    }
}
