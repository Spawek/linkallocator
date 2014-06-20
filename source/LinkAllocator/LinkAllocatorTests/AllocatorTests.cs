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

            t.AddConnection("D1", "D2", 80);

            t.AddLink("L1", "D1", "D2", 10);
        }

        [TestMethod]
        public void SimplePathFind()
        {
            Topology t = new Topology();

            t.AddDevice("D1");
            t.AddDevice("D2");

            t.AddConnection("D1", "D2", 80);

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
            t.AddConnection("D1", "D2", 10);

            // path 2
            t.AddConnection("D1", "D3", 10);
            t.AddConnection("D3", "D2", 10);

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
    }
}
