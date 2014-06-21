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
            List<Device> devs = devices.FindAll(x => x.name == name);
            if(devs.Count != 1)
            {
                throw new ApplicationException("topology (devies) broken");
            }

            return devs[0];
        }

        public Link GetLink(string name)
        {
            List<Link> l = links.FindAll(x => x.name == name);
            if(l.Count != 1)
            {
                throw new ApplicationException("topology (links) broken");
            }

            return l[0];
        }

        public Connection GetConnection(string name)
        {
            List<Connection> c = connections.FindAll(x => x.name == name);
            if (c.Count != 1)
            {
                throw new ApplicationException("topology (connections) broken");
            }

            return c[0];
        }

        public void AddDevice(string name)
        {
            if (devices.Any(x=> x.name == name))
            {
                throw new ApplicationException("there cannot be 2 devices with the same name");
            }

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
            {
                throw new ApplicationException("there cannnot be 2 connections in same direction between 2 devices");
            }

            if (connections.Any(x => x.name == name))
            {
                throw new ApplicationException("there cannot be 2 connection with the sane name");
            }

            Device d1 = GetDevice(dev1);
            Device d2 = GetDevice(dev2);

            Connection c = new Connection(name, d1, d2, capacity);
            connections.Add(c);

            d1.outgoingConnections.Add(c);
            d2.incomingConnections.Add(c);
        }

        public void AddLink(string name, string dev1, string dev2, int capacityNeeded)
        {
            if(links.Any(x=>x.name == name))
            {
                throw new ApplicationException("there cannot be 2 links with the same name");
            }

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

            return String.Compare(lhs.dst.name, rhs.dst.name);
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
            connections.ForEach(x => x.CreateSlots());
            links.ForEach(x => x.FindAvailableSlotSets()); //TODO: sort by linkCapacity

            TryAllocateLinks(links);
        }

        private bool TryAllocateLinks(List<Link> links)
        {
            if (links.Count == 0)
                return true;

            Link currLink = links[0];

            foreach(List<int> slotSet in currLink.availableSlotSets)
            {
                if (currLink.TryAllocateSlotSet(slotSet))
                {
                    if (TryAllocateLinks(links.GetRange(1, links.Count - 1)))
                    {
                        return true;
                    }
                    currLink.DeallocateSlotSet(slotSet); //test it!
                }
            }

            return false;
        }
    }
}
