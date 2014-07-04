using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LinkAllocator;

namespace LinkAllocatorTests
{
    [TestClass]
    public class RoutingTablesTests
    {

        /// <summary>
        ///      P/O
        ///      |
        ///      | 4x
        ///      |
        ///     RT/O     
        ///      |
        ///      | 8x
        ///      |
        ///     RT/I
        ///      |
        ///      | 2x
        ///      |
        ///      D/I
        /// </summary>
        [TestMethod]
        public void SimpleRoutingCalculation()
        {
            Topology t = new Topology();

            t.AddDevice("D/I");
            t.AddDevice("P/O");
            t.AddDevice("RT/I");
            t.AddDevice("RT/O");

            t.AddConnection("C1", "D/I", "RT/I", 20);
            t.AddConnection("C2", "RT/I", "RT/O", 80);
            t.AddConnection("C3", "RT/O", "P/O", 40);

            t.AddLink("L1", "D/I", "P/O", 10);
            t.AddLink("L2", "D/I", "P/O", 10);

            t.AllocateLinksPaths();
            t.AllocateSlots();

            RoutingTable rt = t.CalculateRoutingTableForDevice("RT");

            Assert.AreEqual(2, rt.routingEntities.Count);
        }
    }
}
