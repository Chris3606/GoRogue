using System;
using System.Collections.Generic;
using Priority_Queue;

namespace GoRogue.Pathing
{
    // Node for AStar, stores all values and integrates with priority queue implementation
    class AStarNode : FastPriorityQueueNode
    {
        public AStarNode Parent;
        public readonly Coord Position;
        public bool Closed; // Whether or not the node has been closed
        public float F; // (Partly estimated) distance to end point going thru this node
        public float G; // (Known) distance from start to this node, by shortest known path

        public AStarNode(Coord position, AStarNode parent = null)
        {
            Parent = parent;
            Position = position;
            Closed = false;
            F = G = float.MaxValue;
        }
    }


    public class Path
    {
        private bool inOriginalOrder;

        public Coord Start
        {
            get
            {
                if (inOriginalOrder)
                    return _steps[_steps.Count - 1];

                return _steps[0];
            }
        }
        public Coord End
        {
            get
            {
                if (inOriginalOrder)
                    return _steps[0];

                return _steps[_steps.Count - 1];
            }
        }

        private IList<Coord> _steps;
        public IEnumerable<Coord> Steps
        {
            get
            {
                if (inOriginalOrder)
                {
                    for (int i = _steps.Count - 2; i >= 0; i--)
                        yield return _steps[i];
                }
                else
                {
                    for (int i = 1; i < _steps.Count; i++)
                        yield return _steps[i];
                }
            }
        }

        public IEnumerable<Coord> StepsWithStart
        {
            get
            {
                if (inOriginalOrder)
                {
                    for (int i = _steps.Count - 1; i >= 0; i--)
                        yield return _steps[i];
                }
                else
                {
                    for (int i = 0; i < _steps.Count; i++)
                        yield return _steps[i];
                }
                
            }
        }

        public int Length { get => _steps.Count - 1; }
        public int LengthWithStart { get => _steps.Count; }


        internal Path(IList<Coord> steps)
        {
            _steps = steps;
            inOriginalOrder = true;
        }

        public Coord GetStep(int stepNum)
        {
            if (inOriginalOrder)
                return _steps[(_steps.Count - 2) - stepNum];

            return _steps[stepNum + 1];
        }

        public Coord GetStepWithStart(int stepNum)
        {
            if (inOriginalOrder)
                return _steps[(_steps.Count - 1) - stepNum];

            return _steps[stepNum];
        }
    }

    public class AStar
    {
        public IMapOf<bool> WalkabilityMap { get; private set; }

        private Distance _heuristic;
        public Distance Heuristic
        {
            get => _heuristic;
            set
            {
                _heuristic = value;
                if (_heuristic == Distance.MANHATTAN)
                    neighborFunc = cardinalNeighbors;
                else
                    neighborFunc = neighbors;
            }
        }

        private Func<int, int, IEnumerable<Direction>> neighborFunc;
        private AStarNode[] nodes;
        private FastPriorityQueue<AStarNode> openNodes;
        private int nodesWidth;
        private int nodesHeight;

        public AStar(IMapOf<bool> walkabilityMap, Distance heuristic)
        {
            WalkabilityMap = walkabilityMap;
            Heuristic = heuristic;

            int maxSize = walkabilityMap.Width * walkabilityMap.Height;
            nodes = new AStarNode[maxSize];
            for (int i = 0; i < maxSize; i++)
                nodes[i] = new AStarNode(Coord.ToCoord(i, walkabilityMap.Width), null);
            nodesWidth = walkabilityMap.Width;
            nodesHeight = walkabilityMap.Height;

            openNodes = new FastPriorityQueue<AStarNode>(maxSize);
        }

        public Path ShortestPath(Coord start, Coord end)
        {
            // Don't waste initialization time if there is definately no path
            if (!WalkabilityMap[start] || !WalkabilityMap[end])
                return null; // There is no path

            // If the path is simply the start, don't bother with graph initialization and such
            if (start == end)
            {
                var retVal = new List<Coord>();
                retVal.Add(start);
                return new Path(retVal);
            }

            // Clear nodes to beginning state
            if (nodesWidth != WalkabilityMap.Width || nodesHeight != WalkabilityMap.Height)
            {
                int length = WalkabilityMap.Width * WalkabilityMap.Height;
                nodes = new AStarNode[length];
                openNodes = new FastPriorityQueue<AStarNode>(length);
                //openNodes = new TestPQ(length);
                for (int i = 0; i < length; i++)
                    nodes[i] = new AStarNode(Coord.ToCoord(i, WalkabilityMap.Width), null);

                nodesWidth = WalkabilityMap.Width;
                nodesHeight = WalkabilityMap.Height;
            }
            else
            {
                foreach (var node in nodes)
                {
                    node.Parent = null;
                    node.Closed = false;
                    node.F = node.G = float.MaxValue;
                }
            }

            var result = new List<Coord>();
            int index = start.ToIndex(WalkabilityMap.Width);
            nodes[index].G = 0;
            nodes[index].F = (float)_heuristic.DistanceBetween(start, end); // Completely heuristic for first node
            openNodes.Enqueue(nodes[index], nodes[index].F);

            while (openNodes.Count != 0)
            {
                var current = openNodes.Dequeue();
                current.Closed = true; // We are evaluating this node, no need for it to ever end up in open nodes queue again
                if (current.Position == end) // We found the end, cleanup and return the path
                {
                    openNodes.Clear();

                    do
                    {
                        result.Add(current.Position);
                        current = current.Parent;
                    } while (current != null);

                    return new Path(result);
                }

                foreach (var dir in neighborFunc(end.X - current.Position.X, end.Y - current.Position.Y))
                {
                    Coord neighborPos = current.Position + dir;

                    if (!WalkabilityMap[neighborPos]) // Not part of walkable node "graph", ignore
                        continue;

                    // Not a valid map position, ignore
                    if (neighborPos.X < 0 || neighborPos.Y < 0 || neighborPos.X >= WalkabilityMap.Width || neighborPos.Y >= WalkabilityMap.Height)
                        continue;

                    int neighborIndex = neighborPos.ToIndex(WalkabilityMap.Width);
                    var neighbor = nodes[neighborIndex];
                    
                    if (neighbor.Closed) // This neighbor has already been evaluated at shortest possible path, don't re-add
                        continue;


                    float newDistance = current.G + (float)_heuristic.DistanceBetween(current.Position, neighbor.Position);
                    if (newDistance >= neighbor.G) // Not a better path
                        continue;

                    // We found a best path, so record and update
                    neighbor.Parent = current;
                    neighbor.G = newDistance; // (Known) distance to this node via shortest path
                    neighbor.F = newDistance + (float)_heuristic.DistanceBetween(neighbor.Position, end); // Heuristic distance to end (priority in queue)
                    // If it's already in the queue, update priority to new F
                    if (openNodes.Contains(neighbor))
                        openNodes.UpdatePriority(neighbor, neighbor.F);
                    else // Otherwise, add it with proper priority
                        openNodes.Enqueue(neighbor, neighbor.F);
                }
            }

            return null; // No path found
        }

        public Path ShortestPath(int startX, int startY, int endX, int endY) => ShortestPath(Coord.Get(startX, startY), Coord.Get(endX, endY));

        // These neighbor functions are special in that they return (approximately) the closest directions to the end goal first.
        // This is intended to "prioritize" more direct-looking paths, in the case that one or more paths are equally short
        private static IEnumerable<Direction> cardinalNeighbors(int dx, int dy)
        {
            
            Direction left, right;
            // Intentional inversion of dx and dy sign, because of the order in which our priority queue returns values (we want the one that
            // we ideally use to be last-in, not first-in)
            left = right = Direction.GetCardinalDirection(-dx, -dy);
            yield return right; // Return first direction

            left -= 2;
            right += 2;
            yield return left;
            yield return right;

            // Return last direction
            right += 2;
            yield return right;
            

            //return Direction.CardinalsClockwise();
        }

        private static IEnumerable<Direction> neighbors(int dx, int dy)
        {
            
            Direction left, right;
            // Intentional inversion of dx and dy sign, because of the order in which our priority queue returns values
            left = right = Direction.GetDirection(-dx, -dy);
            yield return right; // Return first direction

            for (int i = 0; i < 3; i++)
            {
                left--;
                right++;

                yield return left;
                yield return right;
            }

            // Return last direction
            right++;
            yield return right;
            

            //return Direction.Outwards();
        }
    }
}
