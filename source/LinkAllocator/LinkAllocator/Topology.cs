using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkAllocator
{
    public class Topology
    {
        public List<Device> devices = new List<Device>();
        public List<Link> links = new List<Link>();
        public List<Connection> connections = new List<Connection>();

        public Device GetDevice(string name)
        {
            var devs = devices.FindAll(x => x.name == name);
            if(devs.Count != 1)
            {
                throw new ApplicationException("topology (devies) broken");
            }

            return devs[0];
        }

        public Link GetLink(string name)
        {
            var l = links.FindAll(x => x.name == name);
            if(l.Count != 1)
            {
                throw new ApplicationException("topology (links) broken");
            }

            return l[0];
        }

        public void AddDevice(string name)
        {
            devices.Add(new Device(name));
        }

        public void AddConnection(string dev1, string dev2, int capacity)
        {
            Device d1 = GetDevice(dev1);
            Device d2 = GetDevice(dev2);

            Connection c = new Connection(d1, d2, capacity);
            connections.Add(c);

            d1.outgoingConnections.Add(c);
            d2.incomingConnections.Add(c);
        }

        public void AddLink(string name, string dev1, string dev2, int capacityNeeded)
        {
            links.Add(new Link(name, GetDevice(dev1), GetDevice(dev2), capacityNeeded));
        }

        public void AllocateLinksPaths()
        {
            links.ForEach(AllocateLinkPath);
        }

        private void ResetMarks(int val)
        {
            devices.ForEach(x => x.mark = val);
        }

        private void AllocateLinkPath(Link link)
        {
            BFSForLink(link);
            FindShortestPathAndAllocateResourcesForLink(link);
        }

        private void FindShortestPathAndAllocateResourcesForLink(Link link)
        {
            Device currDev = link.dst;
            List<Connection> path = new List<Connection>();
            while (currDev != link.src)
            {
                Connection currPath = currDev.incomingConnections.Find(x => x.source.mark == currDev.mark - 1);
                currPath.AllocatePath(link);
                path.Add(currPath);

                currDev = currPath.source;
            }

            path.Reverse();
            link.path = path;
        }

        private void BFSForLink(Link link)
        {
            const int NOT_SEEN = -1;
            const int START_POINT = 0;

            //BFS
            Queue<Device> frontier = new Queue<Device>();
            frontier.Enqueue(link.src);
            ResetMarks(NOT_SEEN);
            link.src.mark = START_POINT;

            while (frontier.Count != 0)
            {
                Device curr = frontier.Dequeue();

                foreach (Connection c in curr.outgoingConnections) //can be optimized by stopping when dst is found
                {
                    if (c.destination.mark == NOT_SEEN && c.CanAllocateLink(link))
                    {
                        c.destination.mark = c.source.mark + 1;
                        frontier.Enqueue(c.destination);
                    }
                }
            }

            if (link.dst.mark == NOT_SEEN)
            {
                throw new ApplicationException("dst is not found");
            }
        }

        /// <summary>
        /// pre-req - paths have to be allocated
        /// </summary>
        public void AllocateSlots()
        {

        }
    }
}
