// ReSharper disable NotAccessedPositionalProperty.Local
// ReSharper disable RedundantNameQualifier

namespace GoRogue.Snippets.HowTos
{
    #region LambdaGridViewExample
    using SadRogue.Primitives.GridViews;

    public static class LambdaGridViewExample
    {
        // Represents some attributes about the terrain at each location.  A real Tile
        // structure might have different values, or you might use an enum instead of a struct
        // for each location; but this suffices for a simple example.
        record struct Tile(bool IsWalkable, bool IsTransparent);

        public static void ExampleCode()
        {
            var tiles = new ArrayView<Tile>(10, 10);

            // In this example, suppose we want a grid view where "true" indicates a tile can
            // be passed through (like a floor), and "false" indicates a tile is impassable
            // (like a wall); this is exactly the type of grid view AStar expects, for example.
            // We can use a LambdaGridView to do this, given an array of tile structures.
            var gridView = new LambdaGridView<bool>(
                tiles.Width,
                tiles.Height,
                p => tiles[p].IsWalkable);
            Console.WriteLine(gridView.ExtendToString(elementStringifier: b => b ? "T" : "F"));
        }
    }
    #endregion
}


namespace GoRogue.Snippets.HowTos
{
    #region LambdaTranslationGridViewExample
    using SadRogue.Primitives.GridViews;

    public static class LambdaTranslationGridViewExample
    {
        record struct Tile(bool IsWalkable, bool IsTransparent);

        public static void ExampleCode()
        {
            var tiles = new ArrayView<Tile>(10, 10);

            // Similar to last time, the grid view we create should have a value of "true" for
            // passable tiles, and false otherwise.  We only need a "tile" value, not a
            // position, to determine if a tile is passable; but if we wanted the position as
            // well, we could change the function we pass to take two parameters; a position
            // and a value.
            var gridView = new LambdaTranslationGridView<Tile, bool>(
                tiles,
                t => t.IsWalkable);
            Console.WriteLine(gridView.ExtendToString(elementStringifier: b => b ? "T" : "F"));
        }
    }
    #endregion
}

namespace GoRogue.Snippets.HowTos
{
    #region SubclassTranslationGridViewExample
    using SadRogue.Primitives.GridViews;

    public static class SubclassTranslationGridViewExample
    {
        record struct Tile(bool IsWalkable, bool IsTransparent);

        class TerrainWalkabilityView : TranslationGridView<Tile, bool>
        {
            public TerrainWalkabilityView(IGridView<Tile> baseGrid)
                : base(baseGrid)
            { }

            // Again, in this case, we don't need the position; but we could override
            // TranslateGet(Point position, Tile value) instead if we did.
            protected override bool TranslateGet(Tile value) => value.IsWalkable;
        }

        public static void ExampleCode()
        {
            var tiles = new ArrayView<Tile>(10, 10);

            var gridView = new TerrainWalkabilityView(tiles);
            Console.WriteLine(gridView.ExtendToString(elementStringifier: b => b ? "T" : "F"));
        }
    }
    #endregion
}

namespace GoRogue.Snippets.HowTos
{
    #region ViewportDemonstration
    using GoRogue.Pathing;
    using SadRogue.Primitives;
    using SadRogue.Primitives.GridViews;

    public static class ViewportDemonstration
    {
        private static readonly Point s_goal = (5, 6);

        public static void ExampleCode()
        {
            // Create a large view representing an entire map, from which we will use a subset
            // of values.  All of the edges will be obstacles; everything else will be clear.
            var largeView = new ArrayView<GoalState>(300, 300);
            foreach (var pos in largeView.Bounds().Expand(-1, -1).Positions())
                largeView[pos] = GoalState.Clear;

            // Add a goal
            largeView[s_goal] = GoalState.Goal;

            // Create a viewport that will show a 10x10 area of the large view, starting at the
            // top left corner (position (0, 0))
            var viewport = new Viewport<GoalState>(
                largeView,
                new Rectangle(0, 0, 10, 10));

            // Create goal map using the viewport and calculate it.
            var goalMap = new GoalMap(viewport, Distance.Chebyshev);

            Console.WriteLine("Initial Goal Map:");
            Console.WriteLine(goalMap.ToString(5));

            // Move the viewport to the right and re-calculate the goal map
            viewport.SetViewArea(viewport.ViewArea.WithPosition((3, 0)));
            goalMap.Update();

            Console.WriteLine("\nGoal Map After Move:");
            Console.WriteLine(goalMap.ToString(5));

            // Note that, if you access positions in the goal map, they will be relative to the
            // viewport, not the original grid view.  For example, the following will print
            // "3", rather than "0":
            Console.WriteLine($"\nGoalMap {s_goal}: {goalMap[s_goal]}");

            // To get the value of a global coordinate, we must perform the translation
            // ourself.
            Console.WriteLine(
                $"Global {s_goal}: {goalMap[s_goal - viewport.ViewArea.Position]}");
        }
    }
    #endregion
}

namespace GoRogue.Snippets.HowTos
{
    #region GridViewBaseExample
    using SadRogue.Primitives;
    using SadRogue.Primitives.GridViews;

    public static class GridViewBaseExample
    {
        // A tile structure representing some attributes about the terrain at each location.
        record struct Tile(bool IsWalkable, bool IsTransparent);

        // Simple map class which contains some terrain for reference.
        class Map
        {
            public readonly ArrayView<Tile> Terrain;

            public Map(int width, int height)
            {
                Terrain = new ArrayView<Tile>(width, height);
            }

        }

        // A grid view that represents the walkability of the terrain in a map.
        class TerrainWalkabilityView : GridViewBase<bool>
        {
            private readonly Map _map;

            public override int Height => _map.Terrain.Height;
            public override int Width => _map.Terrain.Width;

            public override bool this[Point pos] => _map.Terrain[pos].IsWalkable;

            public TerrainWalkabilityView(Map map)
            {
                _map = map;
            }
        }

        public static void ExampleCode()
        {
            var map = new Map(10, 10);
            var gridView = new TerrainWalkabilityView(map);
            Console.WriteLine(gridView.ExtendToString(elementStringifier: b => b ? "T" : "F"));
        }
    }
    #endregion
}

namespace GoRogue.Snippets.HowTos
{
    #region GridView1DIndexBaseExample
    using SadRogue.Primitives.GridViews;

    public static class GridView1DIndexBaseExample
    {
        // A tile structure representing some attributes about the terrain at each location.
        record struct Tile(bool IsWalkable, bool IsTransparent);

        // Simple map class which contains some terrain for reference.
        class Map
        {
            public readonly ArrayView<Tile> Terrain;

            public Map(int width, int height)
            {
                Terrain = new ArrayView<Tile>(width, height);
            }

        }

        // A grid view that represents the walkability of the terrain in a map.
        // This is identical to the grid view above, except the indexer we
        // implement is different; it functions identically to the above
        // example.
        class TerrainWalkabilityView : GridView1DIndexBase<bool>
        {
            private readonly Map _map;

            public override int Height => _map.Terrain.Height;
            public override int Width => _map.Terrain.Width;

            public override bool this[int index] => _map.Terrain[index].IsWalkable;

            public TerrainWalkabilityView(Map map)
            {
                _map = map;
            }
        }

        public static void ExampleCode()
        {
            var map = new Map(10, 10);
            var gridView = new TerrainWalkabilityView(map);
            Console.WriteLine(gridView.ExtendToString(elementStringifier: b => b ? "T" : "F"));
        }
    }
    #endregion
}
