using System;
using LinkAllocator;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace LinkAllocatorTests
{
    [TestClass]
    public class PathAllocationTests
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

            Assert.AreEqual(1, t.GetLink("L1").mainPath.Count);
            Assert.AreEqual(t.GetDevice("D1"), t.GetLink("L1").mainPath[0].source);
            Assert.AreEqual(t.GetDevice("D2"), t.GetLink("L1").mainPath[0].destination);
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

            Assert.AreEqual(1, t.GetLink("L1").mainPath.Count);
            Assert.AreEqual(t.GetDevice("D1"), t.GetLink("L1").mainPath[0].source);
            Assert.AreEqual(t.GetDevice("D2"), t.GetLink("L1").mainPath[0].destination);

            Assert.AreEqual(2, t.GetLink("L2").mainPath.Count);
            Assert.AreEqual(t.GetDevice("D1"), t.GetLink("L2").mainPath[0].source);
            Assert.AreEqual(t.GetDevice("D3"), t.GetLink("L2").mainPath[0].destination);
            Assert.AreEqual(t.GetDevice("D3"), t.GetLink("L2").mainPath[1].source);
            Assert.AreEqual(t.GetDevice("D2"), t.GetLink("L2").mainPath[1].destination);
        }

        [TestMethod]
        public void TwoWayPathFind()
        {
            Topology t = new Topology();

            t.AddDevice("D1");
            t.AddDevice("D2");

            t.AddConnection("C1", "D1", "D2", 10);
            t.AddConnection("C2", "D2", "D1", 10);

            t.AddLink("L1", "D1", "D2", 10);
            t.AddLink("L2", "D2", "D1", 10);

            t.AllocateLinksPaths();

            Assert.AreEqual(1, t.GetLink("L1").mainPath.Count);
            Assert.AreEqual(t.GetDevice("D1"), t.GetLink("L1").mainPath[0].source);
            Assert.AreEqual(t.GetDevice("D2"), t.GetLink("L1").mainPath[0].destination);

            Assert.AreEqual(1, t.GetLink("L2").mainPath.Count);
            Assert.AreEqual(t.GetDevice("D2"), t.GetLink("L2").mainPath[0].source);
            Assert.AreEqual(t.GetDevice("D1"), t.GetLink("L2").mainPath[0].destination);
        }
    
        /// <summary>
        ///      D3
        ///     /  \
        ///    /    \ 
        ///   /      \
        ///  D1       D2
        /// </summary>
        [TestMethod]
        public void FindPathWith2Sources()
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
        }

        /// <summary>
        ///  D2       D3
        ///   \      /
        ///    \    /
        ///     \  /
        ///      D1
        /// </summary>
        [TestMethod]
        public void FindPathWith2Destinations()
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
        }

        /// <summary>
        ///  D2       D3-----D4
        ///   \      /
        ///    \    /
        ///     \  /
        ///      D1
        /// </summary>
        [TestMethod]
        public void FindPathWith3Destinations()
        {
            Topology t = new Topology();

            t.AddDevice("D1");
            t.AddDevice("D2");
            t.AddDevice("D3");
            t.AddDevice("D4");

            t.AddConnection("C1", "D1", "D2", 10);
            t.AddConnection("C2", "D1", "D3", 10);
            t.AddConnection("C3", "D3", "D4", 10);

            List<string> sources = new List<string>() { "D1" };
            List<string> destinations = new List<string>() { "D2", "D3", "D4" };

            t.AddLink("L1", sources, destinations, 10);

            t.AllocateLinksPaths();
        }
    }
}
