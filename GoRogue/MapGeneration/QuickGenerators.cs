using GoRogue.MapViews;
using GoRogue.Random;
using System.Collections.Generic;
using System.Linq;
using Troschuetz.Random;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
	/// <summary>
	/// Collection of algorithms that put map generation pieces together, in ways that allow you to
	/// quickly and easily generate a given type of map in a single function call. The implementation
	/// of these functions may also be used as the basis for implementing more customized generation processes.
	/// </summary>
	public static class QuickGenerators
	{
		/// <summary>
		/// Generates a cave-like map using the cellular automata algorithm here:
		/// http://www.roguebasin.com/index.php?title=Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels.
		/// See <see cref="Generators.CellularAutomataAreaGenerator"/> for details. This algorithm is identical, except that
		/// it connects the areas automatically afterward.
		/// </summary>
		/// <param name="map"></param>
		/// <param name="rng"></param>
		/// <param name="fillProbability"></param>
		/// <param name="totalIterations"></param>
		/// <param name="cutoffBigAreaFill"></param>
		/// <returns>Collection of areas representing the areas of the map before they were connected.</returns>
		public static IEnumerable<Area> GenerateCellularAutomataMap(ISettableMapView<bool> map, IGenerator? rng = null, int fillProbability = 40,
																	   int totalIterations = 7, int cutoffBigAreaFill = 4)
		{
			if (rng == null) rng = SingletonRandom.DefaultRNG;

			// Optimization to allow us to use the existing map if the existing map is an ArrayMap.
			// otherwise we allocate a temporary ArrayMap and copy it onto the original at the end,
			// to make sure that we set each value in the final array map only once in the case that
			// it is something less "quick" than an ArrayMap.
			(ArrayMap<bool> tempMap, bool wasArrayMap) = getStartingArray(map);

			// TODO: The above optimization causes an extra arraymap copy in the case of CellularAutomata -- it may be useful to
			// attempt to eliminate this copy for performance reasons

			// Generate map
			Generators.CellularAutomataAreaGenerator.Generate(tempMap, rng, fillProbability, totalIterations, cutoffBigAreaFill);
			// Calculate connected areas and store before we connect different rooms
			var areas = MapAreaFinder.MapAreasFor(tempMap, AdjacencyRule.Cardinals).ToList();

			// Connect randomly
			Connectors.ClosestMapAreaConnector.Connect(tempMap, Distance.Manhattan, new Connectors.RandomConnectionPointSelector(rng));

			if (!wasArrayMap)
				map.ApplyOverlay(tempMap);

			return areas;
		}

		/// <summary>
		/// Generates a dungeon map based on the process outlined here: http://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/.
		/// </summary>
		/// <remarks>
		/// First, non-overlapping rooms are randomly placed using <see cref="Generators.RoomsGenerator"/>. Then, a maze is
		/// generated into the remaining space using a <see cref="Generators.MazeGenerator"/>. Those mazes are then connected.
		/// The rooms are connected to the maze using a <see cref="Connectors.RoomDoorConnector"/>, and finally, small dead ends
		/// are trimmed out.
		/// </remarks>
		/// <param name="map">The map to set values to.</param>
		/// <param name="minRooms">Minimum amount of rooms to generate.</param>
		/// <param name="maxRooms">Maximum amount of rooms to generate.</param>
		/// <param name="roomMinSize">The minimum size of the room. Forces an odd number.</param>
		/// <param name="roomMaxSize">The maximum size of the room. Forces an odd number.</param>
		/// <param name="roomSizeRatioX">
		/// The ratio of the room width to the height. Defaults to 1.0.
		/// </param>
		/// <param name="roomSizeRatioY">
		/// The ratio of the room height to the width. Defaults to 1.0.
		/// </param>
		/// <param name="maxCreationAttempts">
		/// The max times to re-generate a room that cannot be placed before giving up on placing
		/// that room. Defaults to 10.
		/// </param>
		/// <param name="maxPlacementAttempts">
		/// The max times to attempt to place a room in a map without intersection, before giving up
		/// and re-generating that room. Defaults to 10.
		/// </param>
		/// <param name="crawlerChangeDirectionImprovement">
		/// Out of 100, how much to increase the chance of the crawler changing direction each step
		/// during maze generation. Once it changes direction, the chance resets to 0 and increases
		/// by this amount. Defaults to 10.
		/// </param>
		/// <param name="minSidesToConnect">Minimum sides of the room to process. Defaults to 1.</param>
		/// <param name="maxSidesToConnect">Maximum sides of the room to process. Defaults to 4.</param>
		/// <param name="cancelSideConnectionSelectChance">
		/// A chance out of 100 to cancel selecting sides to process (per room) while we are
		/// connecting them. Defaults to 50.
		/// </param>
		/// <param name="cancelConnectionPlacementChance">
		/// A chance out of 100 to cancel placing a door on a side after one has already been placed
		/// (per room) during connection.Defaults to 70.
		/// </param>
		/// <param name="cancelConnectionPlacementChanceIncrease">
		/// Increase the <paramref name="cancelConnectionPlacementChance"/> value by this amount each
		/// time a door is placed (per room) during the connection process. Defaults to 10.
		/// </param>
		/// <param name="saveDeadEndChance">
		/// After the maze generation finishes, the small dead ends will be trimmed out. This value
		/// indicates the chance out of 100 that the dead end remains. Defaults to 0.
		/// </param>
		/// <param name="maxTrimIterations">
		/// Maximum number of passes to make looking for dead ends when trimming.  Defaults to infinity.
		/// </param>
		/// <returns>A list of the interior of rooms generated, and the connections placed.</returns>
		public static IEnumerable<(Rectangle Room, Point[][] Connections)> GenerateDungeonMazeMap(ISettableMapView<bool> map, int minRooms,
			int maxRooms, int roomMinSize, int roomMaxSize, float roomSizeRatioX = 1f, float roomSizeRatioY = 1f, int maxCreationAttempts = 10,
			int maxPlacementAttempts = 10, int crawlerChangeDirectionImprovement = 10, int minSidesToConnect = 1, int maxSidesToConnect = 4,
			int cancelSideConnectionSelectChance = 50, int cancelConnectionPlacementChance = 70, int cancelConnectionPlacementChanceIncrease = 10,
			int saveDeadEndChance = 0, int maxTrimIterations = -1)
			=> GenerateDungeonMazeMap(map, null, minRooms, maxRooms, roomMinSize, roomMaxSize, roomSizeRatioX, roomSizeRatioY, maxCreationAttempts, maxPlacementAttempts,
									  crawlerChangeDirectionImprovement, minSidesToConnect, maxSidesToConnect, cancelSideConnectionSelectChance,
									  cancelConnectionPlacementChance, cancelConnectionPlacementChanceIncrease, saveDeadEndChance, maxTrimIterations);

		/// <summary>
		/// Generates a dungeon map based on the process outlined here: http://journal.stuffwithstuff.com/2014/12/21/rooms-and-mazes/.
		/// </summary>
		/// <remarks>
		/// First, non-overlapping rooms are randomly placed using <see cref="Generators.RoomsGenerator"/>. Then, a maze is
		/// generated into the remaining space using a <see cref="Generators.MazeGenerator"/>. Those mazes are then connected.
		/// The rooms are connected to the maze using a <see cref="Connectors.RoomDoorConnector"/>, and finally, small dead ends
		/// are trimmed out.
		/// </remarks>
		/// <param name="map">The map to set values to.</param>
		/// <param name="rng">The RNG to use. If null is specified, the default RNG is used.</param>
		/// <param name="minRooms">Minimum amount of rooms to generate.</param>
		/// <param name="maxRooms">Maximum amount of rooms to generate.</param>
		/// <param name="roomMinSize">The minimum size of the room. Forces an odd number.</param>
		/// <param name="roomMaxSize">The maximum size of the room. Forces an odd number.</param>
		/// <param name="roomSizeRatioX">
		/// The ratio of the room width to the height. Defaults to 1.0.
		/// </param>
		/// <param name="roomSizeRatioY">
		/// The ratio of the room height to the width. Defaults to 1.0.
		/// </param>
		/// <param name="maxCreationAttempts">
		/// The max times to re-generate a room that cannot be placed before giving up on placing
		/// that room. Defaults to 10.
		/// </param>
		/// <param name="maxPlacementAttempts">
		/// The max times to attempt to place a room in a map without intersection, before giving up
		/// and re-generating that room. Defaults to 10.
		/// </param>
		/// <param name="crawlerChangeDirectionImprovement">
		/// Out of 100, how much to increase the chance of the crawler changing direction each step
		/// during maze generation. Once it changes direction, the chance resets to 0 and increases
		/// by this amount. Defaults to 10.
		/// </param>
		/// <param name="minSidesToConnect">Minimum sides of the room to process. Defaults to 1.</param>
		/// <param name="maxSidesToConnect">Maximum sides of the room to process. Defaults to 4.</param>
		/// <param name="cancelSideConnectionSelectChance">
		/// A chance out of 100 to cancel selecting sides to process (per room) while we are
		/// connecting them. Defaults to 50.
		/// </param>
		/// <param name="cancelConnectionPlacementChance">
		/// A chance out of 100 to cancel placing a door on a side after one has already been placed
		/// (per room) during connection.Defaults to 70.
		/// </param>
		/// <param name="cancelConnectionPlacementChanceIncrease">
		/// Increase the <paramref name="cancelConnectionPlacementChance"/> value by this amount each
		/// time a door is placed (per room) during the connection process. Defaults to 10.
		/// </param>
		/// <param name="saveDeadEndChance">
		/// After the connection finishes, the small dead ends will be trimmed out. This value
		/// indicates the chance out of 100 that a given dead end remains. Defaults to 0.
		/// </param>
		/// <param name="maxTrimIterations">Maximum number of passes to make looking for dead ends when trimming.  Defaults to infinity.</param>
		/// <returns>A list of the interior of rooms generated and the connections placed.</returns>
		public static IEnumerable<(Rectangle Room, Point[][] Connections)> GenerateDungeonMazeMap(ISettableMapView<bool> map, IGenerator? rng, int minRooms,
			int maxRooms, int roomMinSize, int roomMaxSize, float roomSizeRatioX = 1f, float roomSizeRatioY = 1f, int maxCreationAttempts = 10, int maxPlacementAttempts = 10,
			int crawlerChangeDirectionImprovement = 10, int minSidesToConnect = 1, int maxSidesToConnect = 4, int cancelSideConnectionSelectChance = 50,
			int cancelConnectionPlacementChance = 70, int cancelConnectionPlacementChanceIncrease = 10, int saveDeadEndChance = 0, int maxTrimIterations = -1)
		{
			if (rng == null) rng = SingletonRandom.DefaultRNG;

			// Optimization to allow us to use the existing map if the existing map is an ArrayMap.
			// otherwise we allocate a temporary ArrayMap and copy it onto the original at the end,
			// to make sure that we set each value in the final array map only once in the case that
			// it is something less "quick" than an ArrayMap.
			(ArrayMap<bool> tempMap, bool wasArrayMap) = getStartingArray(map);

			// Generate rooms
			var mapRooms = Generators.RoomsGenerator.Generate(tempMap, rng, minRooms, maxRooms, roomMinSize, roomMaxSize, roomSizeRatioX, roomSizeRatioY,
															  maxCreationAttempts, maxPlacementAttempts);

			// Generate maze
			var mazes = Generators.MazeGenerator.Generate(tempMap, rng, crawlerChangeDirectionImprovement).ToList();
			// Connect random points in each maze to the next closest one, with a horizontal-vertical tunnel, so we have 1 maze
			Connectors.ClosestMapAreaConnector.Connect(map, mazes, Distance.Manhattan, areaConnector: new Connectors.ClosestConnectionPointSelector(Distance.Manhattan), tunnelCreator: new Connectors.HorizontalVerticalTunnelCreator(rng));

			// Connect rooms to maze
			var connectionResult = Connectors.RoomDoorConnector.ConnectRooms(tempMap, rng, mapRooms, minSidesToConnect, maxSidesToConnect,
																			 cancelSideConnectionSelectChance, cancelConnectionPlacementChance,
																			 cancelConnectionPlacementChanceIncrease);

			Connectors.DeadEndTrimmer.Trim(tempMap, mazes, saveDeadEndChance, maxTrimIterations, rng);

			if (!wasArrayMap)
				map.ApplyOverlay(tempMap);

			return connectionResult;
		}

		/// <summary>
		/// Generates a map by attempting to randomly place the specified number of rooms, ranging in
		/// size between the specified min size and max size, trying the specified number of times to
		/// position a room without overlap before discarding the room entirely. The given map will
		/// have a value of false set to all non-passable tiles, and true set to all passable ones.
		/// </summary>
		/// <param name="map">The map to set values to.</param>
		/// <param name="maxRooms">The maximum number of rooms to attempt to place on the map.</param>
		/// <param name="roomMinSize">The minimum size in width and height of each room.</param>
		/// <param name="roomMaxSize">The maximum size in width and height of each room.</param>
		/// <param name="attemptsPerRoom">
		/// The maximum number of times the position of a room will be generated to try to position
		/// it properly (eg. without overlapping with other rooms), before simply discarding the
		/// room. Defaults to 10.
		/// </param>
		/// <returns>Rectangles representing the interior of each room generated.</returns>
		public static IEnumerable<Rectangle> GenerateRandomRoomsMap(ISettableMapView<bool> map, int maxRooms, int roomMinSize, int roomMaxSize, int attemptsPerRoom = 10)
			=> GenerateRandomRoomsMap(map, null, maxRooms, roomMinSize, roomMaxSize, attemptsPerRoom);

		/// <summary>
		/// Generates a map by attempting to randomly place the specified number of rooms, ranging in
		/// size between the specified min size and max size, trying the specified number of times to
		/// position a room without overlap before discarding the room entirely. The given map will
		/// have a value of false set to all non-passable tiles, and true set to all passable ones.
		/// </summary>
		/// <param name="map">The map to set values to.</param>
		/// <param name="rng">
		/// The RNG to use to place rooms and determine room size. If null is specified, the default
		/// RNG is used.
		/// </param>
		/// <param name="maxRooms">The maximum number of rooms to attempt to place on the map.</param>
		/// <param name="roomMinSize">The minimum size in width and height of each room.</param>
		/// <param name="roomMaxSize">The maximum size in width and height of each room.</param>
		/// <param name="attemptsPerRoom">
		/// The maximum number of times the position of a room will be generated to try to position
		/// it properly (eg. without overlapping with other rooms), before simply discarding the
		/// room. Defaults to 10.
		/// </param>
		/// <returns>Rectangles representing the interor of each room generated.</returns>
		public static IEnumerable<Rectangle> GenerateRandomRoomsMap(ISettableMapView<bool> map, IGenerator? rng, int maxRooms, int roomMinSize, int roomMaxSize,
																	  int attemptsPerRoom = 10)
		{
			if (rng == null) rng = SingletonRandom.DefaultRNG;

			// Optimization to allow us to use the existing map if the existing map is an ArrayMap.
			// otherwise we allocate a temporary ArrayMap and copy it onto the original at the end,
			// to make sure that we set each value in the final array map only once in the case that
			// it is something less "quick" than an ArrayMap.
			(ArrayMap<bool> tempMap, bool wasArrayMap) = getStartingArray(map);

			// Generate map
			var rooms = Generators.BasicRoomsGenerator.Generate(tempMap, rng, maxRooms, roomMinSize, roomMaxSize, attemptsPerRoom);
			Connectors.OrderedMapAreaConnector.Connect(tempMap, AdjacencyRule.Cardinals, new Connectors.CenterBoundsConnectionPointSelector(), rng: rng);

			if (!wasArrayMap)
				map.ApplyOverlay(tempMap);

			return rooms;
		}

		/// <summary>
		/// Generates a map, as a simple rectangular box, setting the map given as a "walkability
		/// map". Wall tiles (the edges of the map) will have a value of false set in the given map,
		/// whereas true will be set to all non-wall tiles.
		/// </summary>
		/// <param name="map">The map to set values to.</param>
		public static void GenerateRectangleMap(ISettableMapView<bool> map)
		{
			for (int x = 0; x < map.Width; x++)
				for (int y = 0; y < map.Height; y++)
				{
					if (x == 0 || y == 0 || x == map.Width - 1 || y == map.Height - 1)
						map[x, y] = false;
					else
						map[x, y] = true;
				}
		}

		private static (ArrayMap<bool> tempMap, bool wasArrayMap) getStartingArray(ISettableMapView<bool> startingMap)
		{
			// Create map to generate on (or use the existing one), and reset to all full of walls
			var tempMap = startingMap as ArrayMap<bool>;

			bool wasArrayMap = true;
			if (tempMap == null)
			{
				wasArrayMap = false;
				tempMap = new ArrayMap<bool>(startingMap.Width, startingMap.Height);
			}
			else // Clear existing map properly
				tempMap.SetToDefault();

			return (tempMap, wasArrayMap);
		}
	}
}
