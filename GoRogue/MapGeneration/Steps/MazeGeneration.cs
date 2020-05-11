using System;
using Troschuetz.Random;
using System.Collections.Generic;
using SadRogue.Primitives;
using System.Runtime.CompilerServices;
using System.Linq;
using GoRogue.MapViews;

namespace GoRogue.MapGeneration.Steps
{
    /// <summary>
    /// Generates a maze in the wall areas of a map, using crawlers that walk the map carving tunnels.
    ///
    /// Context Components Required:
    ///     - None
    ///
    /// Context Components Created:
    ///     - <see cref="ContextComponents.WallFloor"/> (if none is present -- existing one is used if one already exists)
    ///     - <see cref="ContextComponents.TunnelsList"/> (if none is present -- existing one is used if one already exists)
    /// </summary>
    /// <remarks>
    /// This generation steps generates mazes, and adds the tunnels made to the <see cref="ContextComponents.TunnelsList"/> context component of the
    /// <see cref="GenerationContext"/>.  If no such component exists, one is created.  It also sets the tunnels to true in the map's
    /// <see cref="ContextComponents.WallFloor"/> context component.  If the GenerationContext has an existing WallFloor context component, that component
    /// is used.  If not, a WallFloor component is created and added to the map context, with it's <see cref="ContextComponents.WallFloor.View"/> property
    /// being an <see cref="MapViews.ArrayMap{T}"/> whose width/height match <see cref="GenerationContext.Width"/>/<see cref="GenerationContext.Height"/>.
    /// </remarks>
    public class MazeGeneration : GenerationStep
    {
        /// <summary>
        /// RNG to use for maze generation.
        /// </summary>
        public IGenerator? RNG = null;

        /// <summary>
        /// Out of 100, how much to increase the chance of a crawler changing direction each step.  Once it changes direction, it resets to 0 and increases
        /// by this amount.  Defaults to 10.
        /// </summary>
        public ushort CrawlerChangeDirectionImprovement = 10;

        /// <summary>
        /// Creates a new maze generation step.
        /// </summary>
        /// <param name="name">The name of the generation step.  Defaults to <see cref="MazeGeneration"/>.</param>
        public MazeGeneration(string? name = null)
            : base(name) { }

        /// <inheritdoc/>
        protected override void OnPerform(GenerationContext context)
        {
            // Use proper RNG
            if (RNG == null)
                RNG = Random.SingletonRandom.DefaultRNG;

            // Validate configuration
            if (CrawlerChangeDirectionImprovement > 100)
                throw new Exception("Crawler direction change chance must be in range [0, 100].");

            // Logic implemented from http://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/

            // Get or create/add a wall-floor context component
            var wallFloorContext = context.GetComponentOrNew(() => new ContextComponents.WallFloor(context));

            // Get or create/add a tunnel list context component
            var tunnelList = context.GetComponentOrNew(() => new ContextComponents.TunnelsList());


            var crawlers = new List<Crawler>();
            Point empty = FindEmptySquare(wallFloorContext.View, RNG);
            
            while (empty != Point.None)
            {
                var crawler = new Crawler();
                crawlers.Add(crawler);
                crawler.MoveTo(empty);
                var startedCrawler = true;
                var percentChangeDirection = 0;

                while (crawler.Path.Count != 0)
                {
                    // Dig this position
                    wallFloorContext.View[crawler.CurrentPosition] = true;

                    // Get valid directions (basically is any position outside the map or not?)
                    var points = AdjacencyRule.Cardinals.NeighborsClockwise(crawler.CurrentPosition).ToArray();
                    var directions = AdjacencyRule.Cardinals.DirectionsOfNeighborsClockwise(Direction.None).ToList();

                    var valids = new bool[4];

                    // Rule out any valids based on their position. Only process NSEW, do not use diagonals
                    for (var i = 0; i < 4; i++)
                        valids[i] = IsPointWallsExceptSource(wallFloorContext.View, points[i], directions[i] + 4);

                    // If not a new crawler, exclude where we came from
                    if (!startedCrawler)
                        valids[directions.IndexOf(crawler.Facing + 4)] = false;

                    // Do we have any valid direction to go?
                    if (valids[0] || valids[1] || valids[2] || valids[3])
                    {
                        var index = 0;

                        // Are we just starting this crawler? OR Is the current crawler facing
                        // direction invalid?
                        if (startedCrawler || valids[directions.IndexOf(crawler.Facing)] == false)
                        {
                            // Just get anything
                            index = GetDirectionIndex(valids, RNG);
                            crawler.Facing = directions[index];
                            percentChangeDirection = 0;
                            startedCrawler = false;
                        }
                        else
                        {
                            // Increase probablity we change direction
                            percentChangeDirection += CrawlerChangeDirectionImprovement;

                            if (PercentageCheck(percentChangeDirection, RNG))
                            {
                                index = GetDirectionIndex(valids, RNG);
                                crawler.Facing = directions[index];
                                percentChangeDirection = 0;
                            }
                            else
                                index = directions.IndexOf(crawler.Facing);
                        }

                        crawler.MoveTo(points[index]);
                    }
                    else
                        crawler.Backtrack();
                }

                empty = FindEmptySquare(wallFloorContext.View, RNG);
            }

            // Add appropriate items to the tunnels list
            tunnelList.AddItems(crawlers.Select(c => c.AllPositions).Where(a => a.Count != 0), Name);
        }

        private static Point FindEmptySquare(IMapView<bool> map, IGenerator rng)
        {
            // Try random positions first
            for (int i = 0; i < 100; i++)
            {
                var location = map.RandomPosition(false, rng);

                if (IsPointConsideredEmpty(map, location))
                    return location;
            }

            // Start looping through every single one
            for (int i = 0; i < map.Width * map.Height; i++)
            {
                var location = Point.FromIndex(i, map.Width);

                if (IsPointConsideredEmpty(map, location))
                    return location;
            }

            return Point.None;
        }

        private static int GetDirectionIndex(bool[] valids, IGenerator rng)
        {
            // 10 tries to find random ok valid
            bool randomSuccess = false;
            int tempDirectionIndex = 0;

            for (int randomCounter = 0; randomCounter < 10; randomCounter++)
            {
                tempDirectionIndex = rng.Next(4);
                if (valids[tempDirectionIndex])
                {
                    randomSuccess = true;
                    break;
                }
            }

            // Couldn't find an active valid, so just run through each
            if (!randomSuccess)
            {
                if (valids[0])
                    tempDirectionIndex = 0;
                else if (valids[1])
                    tempDirectionIndex = 1;
                else if (valids[2])
                    tempDirectionIndex = 2;
                else
                    tempDirectionIndex = 3;
            }

            return tempDirectionIndex;
        }

        // TODO: Create random position function that has a fallback for if random fails after max retries
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsPointConsideredEmpty(IMapView<bool> map, Point location) => !IsPointMapEdge(map, location) &&  // exclude outer ridge of map
                   location.X % 2 != 0 && location.Y % 2 != 0 && // check is odd number position
                   IsPointSurroundedByWall(map, location) && // make sure is surrounded by a wall.
                   !map[location]; // The location is a wall

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsPointMapEdge(IMapView<bool> map, Point location, bool onlyEdgeTest = false)
        {
            if (onlyEdgeTest)
                return location.X == 0 || location.X == map.Width - 1 || location.Y == 0 || location.Y == map.Height - 1;
            return location.X <= 0 || location.X >= map.Width - 1 || location.Y <= 0 || location.Y >= map.Height - 1;
        }

        private static bool IsPointSurroundedByWall(IMapView<bool> map, Point location)
        {
            var points = AdjacencyRule.EightWay.Neighbors(location);

            var mapBounds = map.Bounds();
            foreach (var point in points)
            {
                if (!mapBounds.Contains(point))
                    return false;

                if (map[point])
                    return false;
            }

            return true;
        }

        private static bool IsPointWallsExceptSource(IMapView<bool> map, Point location, Direction sourceDirection)
        {
            // exclude the outside of the map
            var mapInner = map.Bounds().Expand(-1, -1);

            if (!mapInner.Contains(location))
                // Shortcut out if this location is part of the map edge.
                return false;

            // Get map indexes for all surrounding locations
            var index = AdjacencyRule.EightWay.DirectionsOfNeighborsClockwise().ToArray();

            Direction[] skipped;

            if (sourceDirection == Direction.Right)
                skipped = new[] { sourceDirection, Direction.UpRight, Direction.DownRight };
            else if (sourceDirection == Direction.Left)
                skipped = new[] { sourceDirection, Direction.UpLeft, Direction.DownLeft };
            else if (sourceDirection == Direction.Up)
                skipped = new[] { sourceDirection, Direction.UpRight, Direction.UpLeft };
            else
                skipped = new[] { sourceDirection, Direction.DownRight, Direction.DownLeft };

            for (int i = 0; i < index.Length; i++)
            {
                if (skipped[0] == index[i] || skipped[1] == index[i] || skipped[2] == index[i])
                    continue;

                if (!map.Bounds().Contains(location + index[i]) || map[location + index[i]])
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool PercentageCheck(int outOfHundred, IGenerator rng) => outOfHundred > 0 && rng.Next(101) < outOfHundred;

        private class Crawler
        {
            public Area AllPositions = new Area();
            public Point CurrentPosition = new Point(0, 0);
            public Direction Facing = Direction.Up;
            public bool IsActive = true;
            public Stack<Point> Path = new Stack<Point>();

            public void Backtrack()
            {
                if (Path.Count != 0)
                    CurrentPosition = Path.Pop();
            }

            public void MoveTo(Point position)
            {
                Path.Push(position);
                AllPositions.Add(position);
                CurrentPosition = position;
            }
        }
    }
}
