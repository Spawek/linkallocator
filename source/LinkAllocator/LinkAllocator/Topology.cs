using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkAllocator
{
    [Serializable]
    public class PathValidationException : Exception
    {
        public PathValidationException() { }
        public PathValidationException(string message) : base(message) { }
        public PathValidationException(string message, Exception inner) : base(message, inner) { }
        protected PathValidationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
    /// <summary>
    /// TODO:
    ///     - many receivers/transmitters (find path from next device to first path)
    ///     - fix empty list returing bug in better way (nicer)
    ///     - 15MHz - split to 3 links with different names (/LN-1, LN-2, LN-3)
    /// </summary>
    public class Topology
    {
        public List<Device> devices = new List<Device>();
        public List<Link> links = new List<Link>();
        public List<Connection> connections = new List<Connection>();

        public Device GetDevice(string name)
        {
            List<Device> devs = devices.FindAll(x => x.name == name);
            if (devs.Count != 1)
                throw new ApplicationException("topology (devies) broken");

            return devs[0];
        }

        public Link GetLink(string name)
        {
            List<Link> l = links.FindAll(x => x.name == name);
            if (l.Count != 1)
                throw new ApplicationException("topology (links) broken");

            return l[0];
        }

        public Connection GetConnection(string name)
        {
            List<Connection> c = connections.FindAll(x => x.name == name);
            if (c.Count != 1)
                throw new ApplicationException("topology (connections) broken");

            return c[0];
        }

        public void AddDevice(string name)
        {
            if (devices.Any(x=> x.name == name))
                throw new ApplicationException("there cannot be 2 devices with the same name");

            devices.Add(new Device(name));
        }

        /// <summary>
        /// NOTE: for now there is an assumption that there can be only one connecton (in same direction) between 2 devices
        /// (can be fixed in future, but BFS algorithm will need to mark connections instead devices)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dev1"></param>
        /// <param name="dev2"></param>
        /// <param name="capacity"></param>
        public void AddConnection(string name, string dev1, string dev2, int capacity)
        {
            if (connections.Any(x => x.source.name == dev1 && x.destination.name == dev2))
                throw new ApplicationException("there cannnot be 2 connections in same direction between 2 devices");
            if (connections.Any(x => x.name == name))
                throw new ApplicationException("there cannot be 2 connection with the sane name");

            Device d1 = GetDevice(dev1);
            Device d2 = GetDevice(dev2);

            Connection c = new Connection(name, d1, d2, capacity);
            connections.Add(c);

            d1.outgoingConnections.Add(c);
            d2.incomingConnections.Add(c);
        }

        public void AddLink(string name, string dev1, string dev2, int capacityNeeded)
        {
            if (links.Any(x=>x.name == name))
                throw new ApplicationException("there cannot be 2 links with the same name");
            if (dev1 == dev2)
                throw new ApplicationException("link cannot go from point A to point A!");
            if (capacityNeeded <= 0)
                throw new ApplicationException("link capacity should be > 0");

            links.Add(new Link(name, GetDevice(dev1), GetDevice(dev2), capacityNeeded));
        }


        /// <summary>
        /// Comparation in decreasing order of capacity needed and then in dst.name order.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public int LinkCmp(Link lhs, Link rhs)
        {
            if (lhs.capacityNeeded > rhs.capacityNeeded) return -1;
            if (lhs.capacityNeeded < rhs.capacityNeeded) return 1;

            return String.Compare(lhs.mainDestination.name, rhs.mainDestination.name);
        }

        public void AllocateLinksPaths()
        {
            links.Sort(LinkCmp);
            links.ForEach(AllocateLinkPath);
        }

        private void ResetMarks(int val)
        {
            devices.ForEach(x => x.mark = val);
        }

        private void AllocateLinkPath(Link link)
        {
            CreateMarksWithBFS(link, link.mainSource);
            FindShortestPathAndAllocateResourcesForLinkAndGivenDestination(link, link.mainDestination);
            ValidateLinkMainPath(link);
        }

        private void ValidateLinkMainPath(Link link)
        {
            if (link.mainPath.Count == 0)
                throw new PathValidationException("Path size is 0!");
            if (link.mainPath[0].source != link.mainSource)
                throw new PathValidationException("Path source is wrong!");
            if (link.mainPath[link.mainPath.Count - 1].destination != link.mainDestination)
                throw new PathValidationException("Path destination is wrong!");
            CheckPathConsistency(link);
            CheckIfLinkIsAllocatedOnAllItsPathConnections(link);
        }

        private static void CheckIfLinkIsAllocatedOnAllItsPathConnections(Link link)
        {
            if (!link.mainPath.All(x => x.IsLinkAllocated(link)))
                throw new PathValidationException("Path is not allocated on all its connections!");
        }

        private static void CheckPathConsistency(Link link)
        {
            for (int i = 0; i < link.mainPath.Count - 1; i++)
            {
                if (link.mainPath[i].destination != link.mainPath[i + 1].source)
                    throw new PathValidationException("Path is not consistent!");
            }
        }

        private void FindShortestPathAndAllocateResourcesForLinkAndGivenDestination(Link link, Device destination)
        {
            Device currDev = destination;
            List<Connection> path = new List<Connection>();
            while (currDev != link.mainSource)
            {
                Connection currPath = currDev.incomingConnections.Find(x => x.source.mark == currDev.mark - 1);
                currPath.AllocatePath(link);
                path.Add(currPath);

                currDev = currPath.source;
            }

            path.Reverse();
            link.mainPath = path;
        }

        private void CreateMarksWithBFS(Link link, Device startPoint)
        {
            const int NOT_SEEN = -1;
            const int START_POINT = 0;

            Queue<Device> frontier = new Queue<Device>();
            frontier.Enqueue(link.mainSource);
            ResetMarks(NOT_SEEN);
            startPoint.mark = START_POINT;

            while (frontier.Count != 0)
            {
                Device curr = frontier.Dequeue();

                foreach (Connection c in curr.outgoingConnections)
                {
                    if (c.destination.mark == NOT_SEEN && c.CanAllocateLink(link))
                    {
                        c.destination.mark = c.source.mark + 1;
                        frontier.Enqueue(c.destination);
                    }
                }
            }

            if (link.mainDestination.mark == NOT_SEEN)
                throw new ApplicationException("Path allocation algirithm cannot allocate a link: " + link.name);
        }

        /// <summary>
        /// pre-req - paths have to be allocated
        /// </summary>
        public void AllocateSlots()
        {
            connections.ForEach(x => x.CreateSlots());
            links.ForEach(x => x.FindAvailableBeginPositions());

            if (!TryAllocateLinks(links))
                throw new ApplicationException("Cannot allocate slots!");

            ValidateSlotsAllocation();
        }

        private void ValidateSlotsAllocation()
        {
            links.ForEach(CheckNumberOfAllocatedSlots);
        }

        private void CheckNumberOfAllocatedSlots(Link link)
        {
            foreach (Connection connection in link.mainPath)
            {
                int neededSlotsOnConnection = link.capacityNeeded / connection.CapacityPerSlot;
                int slotsAllocatedOnConnection = connection.slots.Count(x => x.slotOWner == link);
                if (neededSlotsOnConnection != slotsAllocatedOnConnection)
                    throw new PathValidationException("Slots number allocated on connection is wrong");
            }
        }

        private bool TryAllocateLinks(List<Link> links)
        {
            if (links.Count == 0)
                return true;

            Link currLink = links[0];

            foreach(int beginPos in currLink.availableBegins)
            {
                if (currLink.TryAllocateSlot(beginPos))
                {
                    if (TryAllocateLinks(links.GetRange(1, links.Count - 1)))
                    {
                        return true;
                    }
                    currLink.DeallocateSlot(beginPos);
                }
            }

            return false;
        }
    }
}
