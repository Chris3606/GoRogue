using GoRogue.Random;
using GoRogue.MapViews;
using Troschuetz.Random;
using System;

namespace GoRogue.MapGeneration.Generators
{
    /// <summary>
    /// Implements a cellular automata genereation algorithm to generate a cave-like map.
    /// </summary>
    /// <remarks>
    /// Generates a map by randomly filling the map surface with floor or wall values (true and false
    /// respectively) based on a probability given, then iteratively smoothing it via the process
    /// outlined in the cited roguebasin article. /// After generate is called, the passed in map
    /// will have had a value of true set to all floor tiles, and a value of false set to all wall
    /// tiles. Based on the C# roguelike library RogueSharp's implementation, and the roguebasin article below: http://www.roguebasin.com/index.php?title=Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels.
    ///
    /// It is guaranteed that the "set" function of the ISettableMapView passed in will only be called
    /// once per tile, unless the type is ArrayMap of bool, in which case the operation is inexpensive 
    /// and calling it multiples times costs little extra, and saves an internal allocation.
    /// </remarks>
    static public class CellularAutomataGenerator
    {
        /// <summary>
        /// Generates the map. Floor tiles will be set to true in the provided map, and wall tiles
        /// will be set to false.
        /// </summary>
        /// <param name="map">The map to fill with values when generate is called.</param>
        /// <param name="rng">
        /// The RNG to use to initially fill the map. If null is specified, the default RNG is used.
        /// </param>
        /// <param name="fillProbability">
        /// Represents the percent chance that a given cell will be a floor cell when the map is
        /// initially randomly filled. Recommended to be in range [40, 60] (40 is used in the
        /// roguebasin article).
        /// </param>
        /// <param name="totalIterations">
        /// Total number of times the cellular automata-based smoothing algorithm is executed.
        /// Recommended to be in range [2, 10] (7 is used on roguebasin article).
        /// </param>
        /// <param name="cutoffBigAreaFill">
        /// Total number of times the cellular automata smoothing variation that is more likely to
        /// result in "breaking up" large areas will be run before switching to the more standard
        /// nearest neighbors version. Recommended to be in range [2, 7] (4 is used in roguebasin article).
        /// </param>
        /// <param name="connectUsingDefault">
        /// Whether or not to ensure all areas generated are connected. If this is true,
        /// ClosestMapAreaConnector.Connect will be used to connect the areas, with
        /// Distance.MANHATTAN distance used, the RNG given, a RandomConnectionPointSelector that
        /// uses the RNG specified to this function, and default values for all other optional
        /// parameters of ClosestMapAreaConnector.Connect.
        /// </param>
        static public void Generate(ISettableMapView<bool> map, IGenerator rng = null, int fillProbability = 40, int totalIterations = 7, int cutoffBigAreaFill = 4,
            bool connectUsingDefault = true)
        {
            if (rng == null) rng = SingletonRandom.DefaultRNG;

            var tempMap = map as ArrayMap<bool>;
            bool wasArrayMap = tempMap != null;

            randomlyFillCells(tempMap, rng, fillProbability);

            for (int i = 0; i < totalIterations; i++)
            {
                if (i < cutoffBigAreaFill)
                    cellAutoBigAreaAlgo(tempMap);
                else
                    cellAutoNearestNeighborsAlgo(tempMap);
            }

            // Ensure it's enclosed before we try to connect, so we can't possibly connect a path
            // that ruins the enclosure. Doing this before connection ensures that filling it can't
            // kill the path to an area.
            fillToRectangle(tempMap);

            if (connectUsingDefault)
                Connectors.ClosestMapAreaConnector.Connect(tempMap, Distance.MANHATTAN, new Connectors.RandomConnectionPointSelector(rng));

            if (!wasArrayMap)
            {
                for (int x = 0; x < tempMap.Width; x++)
                    for (int y = 0; y < tempMap.Height; y++)
                        map[x, y] = tempMap[x, y];
            }
        }

        static private void cellAutoBigAreaAlgo(ISettableMapView<bool> map)
        {
            var oldMap = new ArrayMap<bool>(map.Width, map.Height);

            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                    oldMap[x, y] = map[x, y];

            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                {
                    if (x == 0 || y == 0 || x == map.Width - 1 || y == map.Height - 1)
                        continue;

                    if (countWallsNear(oldMap, x, y, 1) >= 5 || countWallsNear(oldMap, x, y, 2) <= 2)
                        map[x, y] = false;
                    else
                        map[x, y] = true;
                }
        }

        static private void cellAutoNearestNeighborsAlgo(ISettableMapView<bool> map)
        {
            var oldMap = new ArrayMap<bool>(map.Width, map.Height);

            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                    oldMap[x, y] = map[x, y];

            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                {
                    if (x == 0 || y == 0 || x == map.Width - 1 || y == map.Height - 1)
                        continue;

                    if (countWallsNear(oldMap, x, y, 1) >= 5)
                        map[x, y] = false;
                    else
                        map[x, y] = true;
                }
        }

        static private int countWallsNear(ISettableMapView<bool> mapToUse, int posX, int posY, int distance)
        {
            int count = 0;
            int xMin = Math.Max(posX - distance, 0);
            int xMax = Math.Min(posX + distance, mapToUse.Width - 1);
            int yMin = Math.Max(posY - distance, 0);
            int yMax = Math.Min(posY + distance, mapToUse.Height - 1);

            for (int x = xMin; x <= xMax; x++)
                for (int y = yMin; y <= yMax; y++)
                {
                    if (x == posX && y == posY)
                        continue;

                    if (!mapToUse[x, y])
                        ++count;
                }

            return count;
        }

        static private void fillToRectangle(ISettableMapView<bool> map)
        {
            for (int x = 0; x < map.Width; x++)
            {
                map[x, 0] = false;
                map[x, map.Height - 1] = false;
            }

            for (int y = 0; y < map.Height; y++)
            {
                map[0, y] = false;
                map[map.Width - 1, y] = false;
            }
        }

        static private void randomlyFillCells(ISettableMapView<bool> map, IGenerator rng, int fillProbability)
        {
            for (int x = 0; x < map.Width; x++)
                for (int y = 0; y < map.Height; y++)
                {
                    if (x == 0 || y == 0 || x == map.Width - 1 || y == map.Height - 1) // Borders are always walls
                        map[x, y] = false;
                    else if (rng.Next(100) < fillProbability)
                        map[x, y] = true;
                    else
                        map[x, y] = false;
                }
        }
    }
}