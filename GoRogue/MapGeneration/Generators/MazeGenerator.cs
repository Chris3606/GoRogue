using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GoRogue.MapViews;
using GoRogue.Random;
using SadRogue.Primitives;
using Troschuetz.Random;

namespace GoRogue.MapGeneration.Generators
{
    /// <summary>
    /// Generates a maze, and adds it to the given map.
    /// </summary>
    public static class MazeGenerator
    {
        /// <summary>
        /// Generates a maze in map using crawlers that walk the map carving tunnels.
        /// </summary>
        /// <param name="map">The map to modify.</param>
        /// <param name="crawlerChangeDirectionImprovement">
        /// Out of 100, how much to increase the chance of the crawler changing direction each step.
        /// Once it changes direction, the chance resets to 0 and increases by this amount. Defaults
        /// to 10.
        /// </param>
        /// <returns>A list of mazes that were generated.</returns>
        public static IEnumerable<Area> Generate(ISettableMapView<bool> map, int crawlerChangeDirectionImprovement = 10)
            => Generate(map, null, crawlerChangeDirectionImprovement);

        /// <summary>
        /// Generates a maze in map using crawlers that walk the map carving tunnels.
        /// </summary>
        /// <param name="map">The map to modify.</param>
        /// <param name="rng">The RNG to use.</param>
        /// <param name="crawlerChangeDirectionImprovement">
        /// Out of 100, how much to increase the chance of the crawler changing direction each step.
        /// Once it changes direction, the chance resets to 0 and increases by this amount. Defaults
        /// to 10.
        /// </param>
        /// <returns>A list of mazes that were generated.</returns>
        public static IEnumerable<Area> Generate(ISettableMapView<bool> map, IGenerator? rng, int crawlerChangeDirectionImprovement = 10)
        {
            // Implemented the logic from http://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/

            if (rng == null) rng = SingletonRandom.DefaultRNG;

            var crawlers = new List<Crawler>();

            var empty = FindEmptySquare(map, rng);

            while (empty != Point.None)
            {
                Crawler crawler = new Crawler();
                crawlers.Add(crawler);
                crawler.MoveTo(empty);
                var startedCrawler = true;
                var percentChangeDirection = 0;

                while (crawler.Path.Count != 0)
                {
                    // Dig this position
                    map[crawler.CurrentPosition] = true;

                    // Get valid directions (basically is any position outside the map or not?)
                    var points = AdjacencyRule.Cardinals.NeighborsClockwise(crawler.CurrentPosition).ToArray();
                    var directions = AdjacencyRule.Cardinals.DirectionsOfNeighborsClockwise(Direction.None).ToList();
                    var valids = new bool[4];

                    // Rule out any valids based on their position. Only process NSEW, do not use diagonals
                    for (var i = 0; i < 4; i++)
                        valids[i] = IsPointWallsExceptSource(map, points[i], directions[i] + 4);

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
                            index = GetDirectionIndex(valids, rng);
                            crawler.Facing = directions[index];
                            percentChangeDirection = 0;
                            startedCrawler = false;
                        }
                        else
                        {
                            // Increase probablity we change direction
                            percentChangeDirection += crawlerChangeDirectionImprovement;

                            if (PercentageCheck(percentChangeDirection, rng))
                            {
                                index = GetDirectionIndex(valids, rng);
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

                empty = FindEmptySquare(map, rng);
            }

            return crawlers.Select(c => c.AllPositions).Where(a => a.Count != 0);
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
