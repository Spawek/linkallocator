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
    ///     - add expects to tests for multi receiver/transmitter
    ///     - fix empty list returing bug in better way (nicer)
    ///     - 15MHz - split to 3 links with different names (/LN-1, LN-2, LN-3)
    ///     - fixed slot constraint
    ///     - forbidden slot constraint
    ///     - fixed path constraint (has to go throught?)
    /// </summary>
    public class Topology
    {
        public List<Device> devices = new List<Device>();
        public List<Link> links = new List<Link>();
        public List<Connection> connections = new List<Connection>();

        private const int NOT_SEEN = -1;
        private const int START_POINT = 0;

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

        public void AddLink(string name, string source, string destination, int capacityNeeded)
        {
            if (links.Any(x=>x.name == name))
                throw new ArgumentException("there cannot be 2 links with the same name");
            if (source == destination)
                throw new ArgumentException("link source cannot be link destination!");
            if (capacityNeeded <= 0)
                throw new ArgumentException("link capacity should be > 0");

            links.Add(new Link(name, GetDevice(source), GetDevice(destination), capacityNeeded));
        }

        public void AddLink(string name, List<string> sources, List<string> destinaitons, int capacityNeeded)
        {
            if (links.Any(x => x.name == name))
                throw new ArgumentException("there cannot be 2 links with the same name");
            if (sources.Any(x=>destinaitons.Any(y => x == y)))
                throw new ArgumentException("link source cannot be link destination!");
            if (capacityNeeded <= 0)
                throw new ArgumentException("link capacity should be > 0");

            List<Device> sourceDevices = sources.Select(x=>GetDevice(x)).ToList();
            List<Device> destinationDevices = destinaitons.Select(x=>GetDevice(x)).ToList();

            links.Add(new Link(name, sourceDevices, destinationDevices, capacityNeeded));
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
            AllocateMainPath(link);
            AllocateAdditionalPaths(link);

            ValidateLinkPaths(link);
        }

        private void AllocateAdditionalPaths(Link link)
        {
            AllocateAdditionalSourcesPaths(link);
            AllocateAdditionalDestinationPaths(link);
        }

        private void AllocateAdditionalDestinationPaths(Link link)
        {
            foreach (Device destination in link.additionalDestinations)
            {
                CreateMarksWithReversedBFS(link, destination);
                int minMark = link.devicesOnWholePath.Min(x => (x.mark != NOT_SEEN) ? x.mark : int.MaxValue);
                if (minMark == int.MaxValue)
                    throw new ApplicationException("Path allocation algorithm cannot allocate additional destination for link: " + link.name);

                Device devWithMinMark = link.devicesOnWholePath.Find(x => x.mark == minMark);

                List<Connection> additionalDestinationPath =
                    FindShortestReversedPathForLinkAndGivenDestination(link, destination, devWithMinMark);
                additionalDestinationPath.ForEach(x => x.AllocatePath(link));

                link.additionalDestinationPaths.Add(additionalDestinationPath);
            }
        }

        private void AllocateAdditionalSourcesPaths(Link link)
        {
            foreach (Device source in link.additionalSources)
            {
                CreateMarksWithBFS(link, source);
                int minMark = link.devicesOnWholePath.Min(x => (x.mark != NOT_SEEN) ? x.mark : int.MaxValue);
                if (minMark == int.MaxValue)
                    throw new ApplicationException("Path allocation algoritithm cannot allocate additional source for link: " + link.name);

                Device devWithMinMark = link.devicesOnWholePath.Find(x => x.mark == minMark);

                List<Connection> additionalSourcePath =
                    FindShortestPathForLinkAndGivenDestination(link, source, devWithMinMark);
                additionalSourcePath.ForEach(x => x.AllocatePath(link));

                link.additionalSourcePaths.Add(additionalSourcePath);
            }
        }

        private void AllocateMainPath(Link link)
        {
            CreateMarksWithBFS(link, link.mainSource);
            if (link.mainDestination.mark == NOT_SEEN)
                throw new ApplicationException("Path allocation algirithm cannot allocate a link: " + link.name);

            if (link.mainPath != null)
                throw new ApplicationException("main path should be empty in this moment!");
            List<Connection> path = FindShortestPathForLinkAndGivenDestination(link, link.mainSource, link.mainDestination);
            path.ForEach(x => x.AllocatePath(link));
            link.mainPath = path;
        }

        private void ValidateLinkPaths(Link link)
        {
            VerifyPathsSize(link);
            VerifyPathsSourceAndDestination(link);
            CheckPathsConsistency(link);
            CheckIfLinkIsAllocatedOnAllItsPathConnections(link);
            CheckIfEveryConnectionInWholePathIsUnique(link);
        }

        private static void VerifyPathsSourceAndDestination(Link link)
        {
            VerifyPathSource(link.mainPath, link.mainSource);
            VerifyPathDestination(link.mainPath, link.mainDestination);
            for (int i = 0; i < link.additionalSources.Count; i++)
            {
                VerifyPathSource(link.additionalSourcePaths[i], link.additionalSources[i]);
            }
            for (int i = 0; i < link.additionalDestinations.Count; i++)
            {
                VerifyPathDestination(link.additionalDestinationPaths[i], link.additionalDestinations[i]);
            }
        }

        private static void VerifyPathSource(List<Connection> path, Device expectedSource)
        {
            if (path[0].source != expectedSource)
                throw new PathValidationException("Path source is wrong!");
        }

        private static void VerifyPathDestination(List<Connection> path, Device expectedDestination)
        {
            if (path[path.Count - 1].destination != expectedDestination)
                throw new PathValidationException("Path destination is wrong!");
        }

        private static void VerifyPathsSize(Link link)
        {
            if (link.mainPath.Count == 0)
                throw new PathValidationException("Path size is 0!");

            if (link.additionalDestinationPaths.Count != link.additionalDestinations.Count)
                throw new PathValidationException("Not all additional destination paths are calculated");
            if (link.additionalDestinationPaths.Any(x => x.Count == 0))
                throw new PathValidationException("Additional destination path size is 0!");
            
            if (link.additionalSourcePaths.Count != link.additionalSourcePaths.Count)
                throw new PathValidationException("Not all additional source paths are calculated");
            if (link.additionalSourcePaths.Any(x => x.Count == 0))
                throw new PathValidationException("Additional source path size is 0!");
        }

        private void CheckIfEveryConnectionInWholePathIsUnique(Link link)
        {
            if (link.wholePath.Distinct().ToList().Count != link.wholePath.Count)
                throw new ApplicationException("Whole path connections are not distinct!");
        }

        private static void CheckIfLinkIsAllocatedOnAllItsPathConnections(Link link)
        {
            if (!link.wholePath.All(x => x.IsLinkAllocated(link)))
                throw new PathValidationException("Path is not allocated on all its connections!");
        }

        private static void CheckPathsConsistency(Link link)
        {
            CheckPathConsistency(link.mainPath);
            link.additionalSourcePaths.ForEach(CheckPathConsistency);
            link.additionalDestinationPaths.ForEach(CheckPathConsistency);
        }

        private static void CheckPathConsistency(List<Connection> path)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                if (path[i].destination != path[i + 1].source)
                    throw new PathValidationException("Path is not consistent!");
            }
        }

        private List<Connection> FindShortestReversedPathForLinkAndGivenDestination(Link link, Device source, Device destination)
        {
            Device currDev = destination;
            List<Connection> path = new List<Connection>();
            while (currDev != source)
            {
                Connection currPath = currDev.outgoingConnections.Find(x => x.destination.mark == currDev.mark - 1);
                path.Add(currPath);

                currDev = currPath.destination;
            }

            path.Reverse();

            return path;
        }
        private List<Connection> FindShortestPathForLinkAndGivenDestination(Link link, Device source, Device destination)
        {
            Device currDev = destination;
            List<Connection> path = new List<Connection>();
            while (currDev != source)
            {
                Connection currPath = currDev.incomingConnections.Find(x => x.source.mark == currDev.mark - 1);
                path.Add(currPath);

                currDev = currPath.source;
            }

            path.Reverse();

            return path;
        }

        private void CreateMarksWithBFS(Link link, Device startPoint)
        {
            Queue<Device> frontier = new Queue<Device>();
            frontier.Enqueue(startPoint);
            ResetMarks(NOT_SEEN);
            startPoint.mark = START_POINT;

            while (frontier.Count != 0)
            {
                Device curr = frontier.Dequeue();

                foreach (Connection c in curr.outgoingConnections)
                {
                    if (c.destination.mark == NOT_SEEN && c.CanAllocateLink(link))
                    {
                        c.destination.mark = curr.mark + 1;
                        frontier.Enqueue(c.destination);
                    }
                }
            }
        }

        /// <summary>
        /// reversed BFS uses one-way links in reversed direction
        /// (uses incoming connections instead of outgoing connections)
        /// </summary>
        /// <param name="link"></param>
        /// <param name="destination"></param>
        private void CreateMarksWithReversedBFS(Link link, Device startPoint)
        {
            Queue<Device> frontier = new Queue<Device>();
            frontier.Enqueue(startPoint);
            ResetMarks(NOT_SEEN);
            startPoint.mark = START_POINT;

            while (frontier.Count != 0)
            {
                Device curr = frontier.Dequeue();

                foreach (Connection c in curr.incomingConnections)
                {
                    if (c.source.mark == NOT_SEEN && c.CanAllocateLink(link))
                    {
                        c.source.mark = curr.mark + 1;
                        frontier.Enqueue(c.source);
                    }
                }
            }
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
