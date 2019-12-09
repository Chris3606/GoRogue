using GoRogue;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using GoRogue.Pathing;
using GoRogue.Random;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using CA = EMK.Cartography;

namespace GoRogue_UnitTests
{
	[TestClass]
	public class PathingTests
	{
		static private readonly Point END = (17, 14);
		static private readonly int ITERATIONS = 100;
		static private readonly int MAP_HEIGHT = 30;
		static private readonly int MAP_WIDTH = 30;
		static private readonly Point START = (1, 2);

		[TestMethod]
		public void AStarAssumeWalkable()
		{
			var walkabilityMap = new ArrayMap<bool>(MAP_WIDTH, MAP_HEIGHT);
			QuickGenerators.GenerateRectangleMap(walkabilityMap);

			var pather = new AStar(walkabilityMap, Distance.CHEBYSHEV);

			walkabilityMap[START] = false;
			walkabilityMap[END] = true;

			var path = pather.ShortestPath(START, END, true);
			Assert.AreEqual(true, path != null);

			walkabilityMap[START] = true;
			walkabilityMap[END] = false;
			path = pather.ShortestPath(START, END, true);
			Assert.AreEqual(true, path != null);

			walkabilityMap[START] = false;
			walkabilityMap[END] = false;
			path = pather.ShortestPath(START, END, true);
			Assert.AreEqual(true, path != null);

			walkabilityMap[START] = true;
			walkabilityMap[END] = true;
			path = pather.ShortestPath(START, END, true);
			Assert.AreEqual(true, path != null);
		}

		[TestMethod]
		public void AStarMatchesCorrectChebyshev() => aStarMatches(Distance.CHEBYSHEV);

		[TestMethod]
		public void AStarMatchesCorrectEuclidean() => aStarMatches(Distance.EUCLIDEAN);

		[TestMethod]
		public void AStarMatchesCorrectManhattan() => aStarMatches(Distance.MANHATTAN);

		[TestMethod]
		public void AStarNoAssumeWalkable()
		{
			var walkabilityMap = new ArrayMap<bool>(MAP_WIDTH, MAP_HEIGHT);
			QuickGenerators.GenerateRectangleMap(walkabilityMap);

			var pather = new AStar(walkabilityMap, Distance.CHEBYSHEV);

			walkabilityMap[START] = false;
			walkabilityMap[END] = true;

			var path = pather.ShortestPath(START, END, false);
			Assert.AreEqual(false, path != null);

			walkabilityMap[START] = true;
			walkabilityMap[END] = false;
			path = pather.ShortestPath(START, END, false);
			Assert.AreEqual(false, path != null);

			walkabilityMap[START] = false;
			walkabilityMap[END] = false;
			path = pather.ShortestPath(START, END, false);
			Assert.AreEqual(false, path != null);

			walkabilityMap[START] = true;
			walkabilityMap[END] = true;
			path = pather.ShortestPath(START, END, false);
			Assert.AreEqual(true, path != null);
		}

		[TestMethod]
		public void GoalMapBasicResult()
		{
			Point goal = (5, 5);
			var map = new ArrayMap<bool>(50, 50);
			QuickGenerators.GenerateRectangleMap(map);

			var goalMapData = new ArrayMap<GoalState>(map.Width, map.Height);
			goalMapData.ApplyOverlay(new LambdaTranslationMap<bool, GoalState>(map, i => i ? GoalState.Clear : GoalState.Obstacle));
			goalMapData[goal] = GoalState.Goal;

			var goalMap = new GoalMap(goalMapData, Distance.CHEBYSHEV);
			goalMap.Update();

			foreach (var startPos in goalMap.Positions().Where(p => map[p] && p != goal))
			{
				Point pos = startPos;
				while (true)
				{
					var dir = goalMap.GetDirectionOfMinValue(pos);
					if (dir == Direction.NONE)
						break;
					pos += dir;
				}

				bool success = pos == goal;
				if (!success)
				{
					Console.WriteLine($"Failed starting from {startPos} on foal map.  Found min {goalMap.GetDirectionOfMinValue(pos)} when calculating from {pos}:");
					Console.WriteLine(goalMap);
				}
				Assert.AreEqual(true, success);
			}
		}

		[TestMethod]
		public void FleeMapBasicResult()
		{
			Point goal = (5, 5);
			var map = new ArrayMap<bool>(50, 50);
			QuickGenerators.GenerateRectangleMap(map);

			var goalMapData = new ArrayMap<GoalState>(map.Width, map.Height);
			goalMapData.ApplyOverlay(new LambdaTranslationMap<bool, GoalState>(map, i => i ? GoalState.Clear : GoalState.Obstacle));
			goalMapData[goal] = GoalState.Goal;

			var goalMap = new GoalMap(goalMapData, Distance.CHEBYSHEV);
			var fleeMap = new FleeMap(goalMap);
			goalMap.Update();

			foreach (var startPos in fleeMap.Positions().Where(p => map[p] && p != goal))
			{
				Point pos = startPos;
				int moves = 0;
				while (moves < map.Width * map.Height)
				{
					var dir = fleeMap.GetDirectionOfMinValue(pos);
					if (dir == Direction.NONE)
						break;
					pos += dir;

					moves++;
				}

				bool success = pos != goal;
				if (!success)
				{
					Console.WriteLine($"Failed starting from {startPos} on flee map.  Found min {fleeMap.GetDirectionOfMinValue(pos)} when calculating from {pos}:");
					Console.WriteLine(fleeMap);
				}
				Assert.AreEqual(true, success);
			}
		}

		[TestMethod]
		public void ManualAStarChebyshevTest()
		{
			var map = new ArrayMap<bool>(MAP_WIDTH, MAP_HEIGHT);
			QuickGenerators.GenerateRectangleMap(map);

			var pather = new AStar(map, Distance.CHEBYSHEV, Distance.EUCLIDEAN.Calculate);
			var path = pather.ShortestPath(START, END);

			Utility.PrintHightlightedPoints(map, path.StepsWithStart);

			foreach (var point in path.StepsWithStart)
				Console.WriteLine(point);
		}

		[TestMethod]
		public void ManualAStarEuclidianTest()
		{
			var map = new ArrayMap<bool>(MAP_WIDTH, MAP_HEIGHT);
			QuickGenerators.GenerateRectangleMap(map);

			var pather = new AStar(map, Distance.EUCLIDEAN);
			var path = pather.ShortestPath(START, END);

			Utility.PrintHightlightedPoints(map, path.StepsWithStart);

			foreach (var point in path.StepsWithStart)
				Console.WriteLine(point);
		}

		[TestMethod]
		public void ManualAStarManhattanTest()
		{
			var map = new ArrayMap<bool>(MAP_WIDTH, MAP_HEIGHT);
			QuickGenerators.GenerateRectangleMap(map);

			var pather = new AStar(map, Distance.MANHATTAN);
			var path = pather.ShortestPath(START, END);

			Utility.PrintHightlightedPoints(map, path.StepsWithStart);

			foreach (var point in path.StepsWithStart)
				Console.WriteLine(point);
		}

		[TestMethod]
		public void ManualGoalMapTest()
		{
			var map = new ArrayMap<bool>(MAP_WIDTH, MAP_HEIGHT);
			QuickGenerators.GenerateRectangleMap(map);

			var stateMap = new ArrayMap<GoalState>(map.Width, map.Height);
			foreach (var pos in stateMap.Positions())
				stateMap[pos] = map[pos] ? GoalState.Clear : GoalState.Obstacle;

			stateMap[MAP_WIDTH / 2, MAP_WIDTH / 2] = GoalState.Goal;
			stateMap[MAP_WIDTH / 2 + 5, MAP_HEIGHT / 2 + 5] = GoalState.Goal;

			var goalMap = new GoalMap(stateMap, Distance.EUCLIDEAN);
			goalMap.Update();

			Assert.AreEqual(null, goalMap[0, 0]);

			Console.Write(goalMap.ToString(5, "0.00"));
		}

		[TestMethod]
		public void OpenMapPathing()
		{
			var map = new ArrayMap<bool>(10, 10);
			for (int x = 0; x < map.Width; x++)
				for (int y = 0; y < map.Height; y++)
					map[x, y] = true;

			Point start = (1, 6);
			Point end = (0, 1);
			var pather = new AStar(map, Distance.CHEBYSHEV);

			try
			{
				pather.ShortestPath(start, end);
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[TestMethod]
		public void PathInitReversing()
		{
			Point start = (1, 1);
			Point end = (6, 6);
			// Because Path constructor is internal to avoid confusion, we use AStar to return a
			// (simple) known path
			var map = new ArrayMap<bool>(10, 10);
			QuickGenerators.GenerateRectangleMap(map);
			var pather = new AStar(map, Distance.CHEBYSHEV);

			var actualPath = pather.ShortestPath(start, end);
			var expectedPath = new List<Point>();

			for (int i = start.X; i <= end.X; i++)
				expectedPath.Add((i, i));

			Console.WriteLine("Pre-Reverse:");
			printExpectedAndActual(expectedPath, actualPath);

			checkAgainstPath(expectedPath, actualPath, start, end);

			expectedPath.Reverse();
			actualPath.Reverse();

			Console.WriteLine("\nPost-Reverse:");
			printExpectedAndActual(expectedPath, actualPath);

			checkAgainstPath(expectedPath, actualPath, end, start);
		}

		private static void checkAdjacency(Path path, Distance distanceCalc)
		{
			if (path.LengthWithStart == 1)
				return;

			for (int i = 0; i < path.LengthWithStart - 2; i++)
			{
				bool isAdjacent = false;
				foreach (var neighbor in ((AdjacencyRule)distanceCalc).Neighbors(path.GetStepWithStart(i)))
				{
					if (neighbor == path.GetStepWithStart(i + 1))
					{
						isAdjacent = true;
						break;
					}
				}

				Assert.AreEqual(true, isAdjacent);
			}
		}

		private static void checkWalkable(Path path, IMapView<bool> map)
		{
			foreach (var pos in path.StepsWithStart)
				Assert.AreEqual(true, map[pos]);
		}

		private static IMapView<GoalState> createGoalStateMap(IMapView<bool> walkabilityMap, IEnumerable<Point> goals)
		{
			var mapGoals = new ArrayMap<GoalState>(walkabilityMap.Width, walkabilityMap.Height);
			for (int x = 0; x < walkabilityMap.Width; x++)
				for (int y = 0; y < walkabilityMap.Height; y++)
					mapGoals[x, y] = walkabilityMap[x, y] ? GoalState.Clear : GoalState.Obstacle;

			foreach (var goal in goals)
				mapGoals[goal] = GoalState.Goal;

			return mapGoals;
		}

		private static CA.Heuristic distanceHeuristic(Distance distanceCalc)
		{
			switch (distanceCalc.Type)
			{
				case Distance.Types.CHEBYSHEV:
					return CA.AStar.MaxAlongAxisHeuristic;

				case Distance.Types.EUCLIDEAN:
					return CA.AStar.EuclidianHeuristic;

				case Distance.Types.MANHATTAN:
					return CA.AStar.ManhattanHeuristic;

				default:
					throw new Exception("Should not occur");
			}
		}

		// Initialize graph for control-case AStar, based on a GoRogue IMapView
		private static GraphReturn initGraph(IMapView<bool> map, Distance connectivity)
		{
			var returnVal = new GraphReturn();
			returnVal.Graph = new CA.Graph();

			returnVal.Nodes = new CA.Node[map.Width, map.Height]; // So we can add arcs easier

			for (int x = 0; x < map.Width; x++)
				for (int y = 0; y < map.Height; y++)
					if (map[x, y])
					{
						returnVal.Nodes[x, y] = new CA.Node(x, y, 0);
						returnVal.Graph.AddNode(returnVal.Nodes[x, y]);
					}

			for (int x = 0; x < map.Width; x++)
				for (int y = 0; y < map.Height; y++)
				{
					if (map[x, y])
					{
						foreach (var neighbor in ((AdjacencyRule)connectivity).Neighbors(x, y))
						{
							// Out of bounds of map
							if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= map.Width || neighbor.Y >= map.Height)
								continue;

							if (!map[neighbor]) // Not walkable, so no node exists for it
								continue;

							returnVal.Graph.AddArc(new CA.Arc(returnVal.Nodes[x, y], returnVal.Nodes[neighbor.X, neighbor.Y]));
						}
					}
				}

			return returnVal;
		}

		private void aStarMatches(Distance distanceCalc)
		{
			var map = new ArrayMap<bool>(MAP_WIDTH, MAP_HEIGHT);
			QuickGenerators.GenerateCellularAutomataMap(map);
			var graphTuple = initGraph(map, distanceCalc);

			var pather = new AStar(map, distanceCalc);
			var controlPather = new CA.AStar(graphTuple.Graph);
			controlPather.ChoosenHeuristic = distanceHeuristic(distanceCalc);

			for (int i = 0; i < ITERATIONS; i++)
			{
				Point start = (SingletonRandom.DefaultRNG.Next(map.Width - 1), SingletonRandom.DefaultRNG.Next(map.Height - 1));
				while (!map[start])
					start = (SingletonRandom.DefaultRNG.Next(map.Width - 1), SingletonRandom.DefaultRNG.Next(map.Height - 1));

				Point end = (SingletonRandom.DefaultRNG.Next(map.Width - 1), SingletonRandom.DefaultRNG.Next(map.Height - 1));
				while (end == start || !map[end])
					end = (SingletonRandom.DefaultRNG.Next(map.Width - 1), SingletonRandom.DefaultRNG.Next(map.Height - 1));

				var path1 = pather.ShortestPath(start, end);
				controlPather.SearchPath(graphTuple.Nodes[start.X, start.Y], graphTuple.Nodes[end.X, end.Y]);
				var path2 = controlPather.PathByNodes;

				if (path2.Length != path1.LengthWithStart)
				{
					Console.WriteLine($"Error: Control got {path2.Length}, but custom AStar got {path1.LengthWithStart}");
					Console.WriteLine("Control: ");
					Utility.PrintHightlightedPoints(map, Utility.ToPoints(path2));
					Console.WriteLine("AStar  :");
					Utility.PrintHightlightedPoints(map, path1.StepsWithStart);
				}

				bool lengthGood = (path1.LengthWithStart <= path2.Length);
				Assert.AreEqual(true, lengthGood);
				Assert.AreEqual(path1.Start, start);
				Assert.AreEqual(path1.End, end);
				checkWalkable(path1, map);
				checkAdjacency(path1, distanceCalc);
			}
		}

		private void checkAgainstPath(IReadOnlyList<Point> expectedPath, Path actual, Point start, Point end)
		{
			var actualList = actual.StepsWithStart.ToList();

			checkListsMatch(expectedPath, actualList);

			for (int i = 0; i < expectedPath.Count; i++)
				Assert.AreEqual(expectedPath[i], actual.GetStepWithStart(i));

			for (int i = 1; i < expectedPath.Count; i++)
				Assert.AreEqual(expectedPath[i], actual.GetStep(i - 1));

			Assert.AreEqual(actual.Start, start);
			Assert.AreEqual(actual.End, end);

			Assert.AreEqual(actual.Length, expectedPath.Count - 1);
			Assert.AreEqual(actual.LengthWithStart, expectedPath.Count);
		}

		private void checkListsMatch(IReadOnlyList<Point> expected, IReadOnlyList<Point> actual)
		{
			Assert.AreEqual(expected.Count, actual.Count);

			for (int i = 0; i < expected.Count; i++)
				Assert.AreEqual(expected[i], actual[i]);
		}

		private void printExpectedAndActual(IReadOnlyList<Point> expected, Path actual)
		{
			Console.WriteLine("Expected:");
			foreach (var i in expected)
				Console.Write(i + ",");
			Console.WriteLine();

			Console.WriteLine("Actual");
			foreach (var i in actual.StepsWithStart)
				Console.Write(i + ",");
			Console.WriteLine();

			Console.WriteLine("Actual by index: ");
			for (int i = 0; i < expected.Count; i++)
				Console.Write(actual.GetStepWithStart(i) + ",");
			Console.WriteLine();
		}
	}

	internal class GraphReturn
	{
		public CA.Graph Graph;
		public CA.Node[,] Nodes;
	}
}
