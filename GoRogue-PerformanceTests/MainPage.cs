using EasyConsole;
using GoRogue;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using System;
using System.Collections.Generic;

namespace GoRogue_PerformanceTests
{
	internal class MainPage : MenuPage
	{
		public MainPage(Program program)
			: base("Main Menu", program,
				  new Option("Dice Notation Tests", DiceNotation),
				  new Option("Effects Tests", Effects), // 0.0025-0.0032
				  new Option("LambdaTranslationMap Tests", LambdaTranslationMap),
				  new Option("LayerMask Tests", LayerMask),
				  new Option("Lighting/FOV Tests", LightingFOV),
				  new Option("Line Tests", Line),
				  new Option("Pathing Tests", Pathing),
				  new Option("Quit", Quit))
		{ }

		private static void DiceNotation()
		{
			var timeForSmallDiceRoll = DiceNotationTests.TimeForDiceRoll("1d6", Runner.ITERATIONS_FOR_TIMING * 10);
			var timeForMediumDiceRoll = DiceNotationTests.TimeForDiceRoll("2d6+3", Runner.ITERATIONS_FOR_TIMING * 10);
			var timeForLargeDiceRoll = DiceNotationTests.TimeForDiceRoll("1d(1d12+4)+3", Runner.ITERATIONS_FOR_TIMING * 10);

			var timeForSmallDiceExpr = DiceNotationTests.TimeForDiceExpression("1d6", Runner.ITERATIONS_FOR_TIMING * 10);
			var timeForMediumDiceExpr = DiceNotationTests.TimeForDiceExpression("2d6+3", Runner.ITERATIONS_FOR_TIMING * 10);
			var timeForLargeDiceExpr = DiceNotationTests.TimeForDiceExpression("1d(1d12+4)+3", Runner.ITERATIONS_FOR_TIMING * 10);

			var timeForKeepRoll = DiceNotationTests.TimeForDiceRoll("5d6k2+3", Runner.ITERATIONS_FOR_TIMING * 10);
			var timeForKeepExpr = DiceNotationTests.TimeForDiceExpression("5d6k2+3", Runner.ITERATIONS_FOR_TIMING * 10);

			Console.WriteLine();
			Console.WriteLine($"Time to roll 1d6, 2d6+3, and 5d6k2+3, 1d(1d12+4)+3 dice {Runner.ITERATIONS_FOR_TIMING * 10} times: ");
			Console.WriteLine("\tRoll Method:");
			Console.WriteLine($"\t\t1d6         : {timeForSmallDiceRoll}");
			Console.WriteLine($"\t\t2d6+3       : {timeForMediumDiceRoll}");
			Console.WriteLine($"\t\t5d6k2+3      : {timeForKeepRoll}");
			Console.WriteLine($"\t\t1d(1d12+4)+3: {timeForLargeDiceRoll}");

			Console.WriteLine("\tParse Method:");
			Console.WriteLine($"\t\t1d6         : {timeForSmallDiceExpr}");
			Console.WriteLine($"\t\t2d6+3       : {timeForMediumDiceExpr}");
			Console.WriteLine($"\t\t5d6k2+3      : {timeForKeepExpr}");
			Console.WriteLine($"\t\t1d(1d12+4)+3: {timeForLargeDiceExpr}");
			Console.WriteLine();
		}

		private static void LightingFOV()
		{
			// Doesn't work properly in release mode, only in debug mode.
			/*
			long lightingMem = LightingFOVTests.MemorySingleLightSourceLighting(MAP_WIDTH, MAP_HEIGHT, LIGHT_RADIUS);
			long fovMem = LightingFOVTests.MemorySingleLightSourceFOV(MAP_WIDTH, MAP_HEIGHT, LIGHT_RADIUS);
			Console.WriteLine($"Memory for {MAP_WIDTH}x{MAP_HEIGHT}, Radius {LIGHT_RADIUS}:");
			Console.WriteLine($"\tLighting: {lightingMem} bytes");
			Console.WriteLine($"\tFOV     : {fovMem} bytes");
			*/

			var timeSingleLighting = LightingFOVTests.TimeForSingleLightSourceLighting(Runner.MAP_WIDTH, Runner.MAP_HEIGHT, Runner.SOURCE_TYPE,
																					   Runner.LIGHT_RADIUS, Runner.RADIUS_STRATEGY, Runner.ITERATIONS_FOR_TIMING);
			var timeSingleFOV = LightingFOVTests.TimeForSingleLightSourceFOV(Runner.MAP_WIDTH, Runner.MAP_HEIGHT,
																			 Runner.LIGHT_RADIUS, Runner.ITERATIONS_FOR_TIMING);
			Console.WriteLine();
			Console.WriteLine($"Time for {Runner.ITERATIONS_FOR_TIMING} calculates, single source, {Runner.MAP_WIDTH}x{Runner.MAP_HEIGHT} map, Radius {Runner.LIGHT_RADIUS}:");
			Console.WriteLine($"\tSenseMap: {timeSingleLighting.ToString()}");
			Console.WriteLine($"\tFOV     : {timeSingleFOV.ToString()}");

			Console.WriteLine();
			TestLightingNSource(2);

			Console.WriteLine();
			TestLightingNSource(3);

			Console.WriteLine();
			TestLightingNSource(4);
		}

		private static void Effects()
		{
			var timeNoEventHandler = EffectsTests.TestEffectManagerNoExpiredHandler(Runner.NUM_EFFECTS, Runner.ITERATIONS_FOR_TIMING);
			var timeWithEventHandler = EffectsTests.TestEffectManagerWithExpiredHandler(Runner.NUM_EFFECTS, Runner.ITERATIONS_FOR_TIMING);

			Console.WriteLine();
			Console.WriteLine($"Time for processing {Runner.NUM_EFFECTS} {Runner.ITERATIONS_FOR_TIMING} times:");
			Console.WriteLine($"\tNo Event Handler:   {timeNoEventHandler}");
			Console.WriteLine($"\tWith Event Handler: {timeWithEventHandler}");
		}

		private static void Line()
		{
			var timeBres = LineTests.TimeForLineGeneration(Runner.LINE_START, Runner.LINE_END, Lines.Algorithm.BRESENHAM, Runner.ITERATIONS_FOR_TIMING);
			var timeBresOrdered = LineTests.TimeForLineGeneration(Runner.LINE_START, Runner.LINE_END, Lines.Algorithm.BRESENHAM_ORDERED, Runner.ITERATIONS_FOR_TIMING);
			var timeDDA = LineTests.TimeForLineGeneration(Runner.LINE_START, Runner.LINE_END, Lines.Algorithm.DDA, Runner.ITERATIONS_FOR_TIMING);
			var timeOrtho = LineTests.TimeForLineGeneration(Runner.LINE_START, Runner.LINE_END, Lines.Algorithm.ORTHO, Runner.ITERATIONS_FOR_TIMING);

			Console.WriteLine();
			Console.WriteLine($"Time for {Runner.ITERATIONS_FOR_TIMING} generations of line from {Runner.LINE_START} to {Runner.LINE_END}:");
			Console.WriteLine($"\tBresenham        : {timeBres}");
			Console.WriteLine($"\tBresenham Ordered: {timeBresOrdered}");
			Console.WriteLine($"\tDDA              : {timeDDA}");
			Console.WriteLine($"\tOrtho            : {timeOrtho}");
		}

		private static void Pathing()
		{
			/* AStar */
			var timeAStar = PathingTests.TimeForAStar(Runner.MAP_WIDTH, Runner.MAP_HEIGHT, Runner.ITERATIONS_FOR_TIMING);
			Console.WriteLine();
			Console.WriteLine($"Time for {Runner.ITERATIONS_FOR_TIMING} paths, on {Runner.MAP_WIDTH}x{Runner.MAP_HEIGHT} map:");
			Console.WriteLine($"\tAStar: {timeAStar}");

			/* Single-Goal GoalMap */
			var map = new ArrayMap<bool>(Runner.MAP_WIDTH, Runner.MAP_HEIGHT);
			QuickGenerators.GenerateCellularAutomataMap(map);
			Coord goal = map.RandomPosition(true);

			var timeGoalMap = PathingTests.TimeForGoalMap(map, goal.Yield(), Runner.ITERATIONS_FOR_TIMING);
			var timeFleeMap = PathingTests.TimeForFleeMap(map, goal.Yield(), Runner.ITERATIONS_FOR_TIMING);

			Console.WriteLine();
			Console.WriteLine($"Time to calculate single-source goal map on {Runner.MAP_WIDTH}x{Runner.MAP_HEIGHT} map {Runner.ITERATIONS_FOR_TIMING} times:");
			Console.WriteLine($"\tGoal-Map    : {timeGoalMap}");
			Console.WriteLine($"\tFlee-Map    : {timeFleeMap}");

			/* Multi-Goal GoalMap */
			var goals = new List<Coord>();

			for (int i = 0; i < Runner.NUM_GOALS; i++)
				goals.Add(map.RandomPosition(true));

			var timeMGoalMap = PathingTests.TimeForGoalMap(map, goals, Runner.ITERATIONS_FOR_TIMING);
			var timeMFleeMap = PathingTests.TimeForFleeMap(map, goals, Runner.ITERATIONS_FOR_TIMING);

			Console.WriteLine();
			Console.WriteLine($"Time to calculate multi-source goal map on {Runner.MAP_WIDTH}x{Runner.MAP_HEIGHT} map {Runner.ITERATIONS_FOR_TIMING} times:");
			Console.WriteLine($"\tGoal-Map    : {timeMGoalMap}");
			Console.WriteLine($"\tFlee-Map    : {timeMFleeMap}");
		}

		private static void LayerMask()
		{
			var timeForRetrieveLayers = LayerMaskTests.TimeForGetLayers(Runner.ITERATIONS_FOR_TIMING);
			var timeForRetrieve1Layer = LayerMaskTests.TimeForGetRandomLayers(Runner.ITERATIONS_FOR_TIMING, 1);
			var timeForRetrieve3Layers = LayerMaskTests.TimeForGetRandomLayers(Runner.ITERATIONS_FOR_TIMING, 3);
			var timeForRetrieve7Layers = LayerMaskTests.TimeForGetRandomLayers(Runner.ITERATIONS_FOR_TIMING, 7);
			var timeForRetrieve32Layers = LayerMaskTests.TimeForGetRandomLayers(Runner.ITERATIONS_FOR_TIMING, 32);

			Console.WriteLine($"Time for retrieving layers from mask {Runner.ITERATIONS_FOR_TIMING} times:");
			Console.WriteLine($"\tLayers [0, 2, 5]      : {timeForRetrieveLayers}");
			Console.WriteLine("\tTime for random layers :");
			Console.WriteLine($"\t\t1 Layer  : {timeForRetrieve1Layer}");
			Console.WriteLine($"\t\t3 Layers : {timeForRetrieve3Layers}");
			Console.WriteLine($"\t\t7 Layers : {timeForRetrieve7Layers}");
			Console.WriteLine($"\t\t32 Layers: {timeForRetrieve32Layers}");
		}

		private static void LambdaTranslationMap()
		{
			var timeFor1ParamAccess = MapViewTests.TimeForLambdaTranslationMap1ParamAccess(Runner.MAP_WIDTH, Runner.MAP_HEIGHT, Runner.ITERATIONS_FOR_TIMING);
			var timeFor2ParamAccess = MapViewTests.TimeForLambdaTranslationMap2ParamAccess(Runner.MAP_WIDTH, Runner.MAP_HEIGHT, Runner.ITERATIONS_FOR_TIMING);

			Console.WriteLine($"Time for accessing {Runner.MAP_WIDTH}x{Runner.MAP_HEIGHT} LambdaTranslationMap {Runner.ITERATIONS_FOR_TIMING} times:");
			Console.WriteLine($"\tSingle-param: {timeFor1ParamAccess}");
			Console.WriteLine($"\tDouble-param: {timeFor2ParamAccess}");
		}

		private static void Quit()
		{
			Runner.Quit = true;
		}

		private static void TestLightingNSource(int sources)
		{
			var timeMultipleLighting = LightingFOVTests.TimeForNSourcesLighting(Runner.MAP_WIDTH, Runner.MAP_HEIGHT, Runner.LIGHT_RADIUS,
																				Runner.ITERATIONS_FOR_TIMING, sources);
			Console.WriteLine($"Time for {Runner.ITERATIONS_FOR_TIMING}, calc fov's, {sources} sources, {Runner.MAP_WIDTH}x{Runner.MAP_HEIGHT} map, Radius {Runner.LIGHT_RADIUS}:");
			Console.WriteLine($"\tLighting: {timeMultipleLighting.ToString()}");
		}
	}
}