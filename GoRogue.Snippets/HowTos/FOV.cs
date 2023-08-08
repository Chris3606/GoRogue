using GoRogue.FOV;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Snippets.HowTos
{
    public static class FOVExample
    {
        #region MapStructure
        // Simple structure which records some attributes about the terrain at each location.
        record struct Tile(bool IsWalkable, bool IsTransparent);

        // Map class which contains some terrain and an FOV instance for us to use
        class Map
        {
            // Array of terrain at each location
            public readonly ArrayView<Tile> Terrain;

            // Grid view we'll pass to FOV, which represents the transparency of terrain
            public readonly IGridView<bool> TransparencyView;

            // FOV instance we'll use
            public readonly IFOV FOV;

            public Map(int width, int height)
            {
                Terrain = new ArrayView<Tile>(width, height);

                // Note that a using lambda-based grid view may have performance implications; if
                // the performance is not sufficient for your use case, you could instead use any
                // other grid view (an ArrayView of cached boolean values representing
                // transparency, for example).  For simplicity, we'll do it this way; and it's fast
                // enough as-is for many use cases.
                TransparencyView = new LambdaTranslationGridView<Tile, bool>(
                    Terrain, t => t.IsTransparent);
                FOV = new RecursiveShadowcastingFOV(TransparencyView);
            }
        }
        #endregion

        public static void ExampleCode()
        {
            #region MapCreation
            // Creates a rectangle map with walls around the outside and floor in the middle using
            // GoRogue's map generation system, and retrieves a grid view representing the
            // generated map where "false" represents walls and "true" represents floors.
            var wallFloor = new Generator(11, 11)
                .ConfigAndGenerateSafe(gen => gen.AddSteps(DefaultAlgorithms.RectangleMapSteps()))
                .Context
                .GetFirst<IGridView<bool>>("WallFloor");

            // We then use this generated grid view to create our Map
            var map = new Map(wallFloor.Width, wallFloor.Height);
            map.Terrain.ApplyOverlay(
                pos => wallFloor[pos]
                    ? new Tile(true, true)
                    : new Tile(false, false));

            // In the printed result, "#" represents a wall (opaque cell), and "." represents a
            // floor (transparent cell).
            Console.WriteLine("Map:");
            Console.WriteLine(
                map.Terrain.ExtendToString(elementStringifier: t => t.IsTransparent ? "." : "#"));
            #endregion

            #region CalculateFOV
            map.FOV.Calculate(map.Terrain.Bounds().Center, 5, Radius.Square);
            #endregion

            #region BooleanResultView
            // "*" in the printed output represents true; "-" represents false.
            Console.WriteLine("\nBooleanResultView FOV:");
            Console.WriteLine(
                map.FOV.BooleanResultView.ExtendToString(elementStringifier: v => v ? "*" : "-"));
            #endregion

            #region IntroduceBlockingAndRecalc
            // Since our grid view gets its data from the map, we can simply change the map data and
            // recalculate the FOV.
            map.Terrain[5, 4] = new Tile(false, false);
            map.FOV.Calculate(map.Terrain.Bounds().Center, 5, Radius.Square);

            Console.WriteLine("\nFOV with blocking object:");
            Console.WriteLine(
                map.FOV.BooleanResultView.ExtendToString(elementStringifier: v => v ? "*" : "-"));
            #endregion

            #region AngleRestrictedFOV
            // Here, we restrict the calculated FOV to a cone.  The cone is defined by two parameters:
            // - An angle, which specifies the direction the cone is facing.  0 is up, and the angle
            // increases clockwise.
            // - A "span", which specifies the width of the cone.  A span of 360 degrees is a full
            // circle
            map.FOV.Calculate(
                map.Terrain.Bounds().Center, 5, Radius.Square, 90, 60);
            Console.WriteLine("\nFOV Restricted to Cone:");
            Console.WriteLine(
                map.FOV.BooleanResultView.ExtendToString(elementStringifier: v => v ? "*" : "-"));
            #endregion

            #region DoubleResultView
            // Output rounds the double values to 2 decimal places for readability.
            Console.WriteLine("\nDoubleResultView FOV:");
            Console.WriteLine(
                map.FOV.DoubleResultView.ExtendToString(
                    4, elementStringifier: i => i.ToString("0.00")));
            #endregion

            #region NewlySeenAndUnseen
            // Turn the FOV to face south, and recalculate, so that we have some newly seen and
            // unseen cells.
            map.FOV.Calculate(
                map.Terrain.Bounds().Center, 5, Radius.Square, 180, 60);
            Console.WriteLine("\nFOV After recalculate:");
            Console.WriteLine(
                map.FOV.BooleanResultView.ExtendToString(elementStringifier: v => v ? "*" : "-"));

            // Newly seen cells are those that are in the FOV, but were not in the FOV the last
            // time Calculate was called.
            Console.WriteLine($"\nNewly seen: {map.FOV.NewlySeen.ExtendToString()}");

            // Newly unseen cells are those that are NOT in the FOV, but WERE in the FOV the last
            // time calculate was called.
            Console.WriteLine($"Newly unseen: {map.FOV.NewlyUnseen.ExtendToString()}");
            #endregion

            #region CurrentFOV
            Console.WriteLine($"\nCurrent FOV: {map.FOV.CurrentFOV.ExtendToString()}");
            #endregion

            #region CalculateAppend
            // CalculateAppend provides overloads taking all the same parameters as Calculate.
            // Here, we'll use the angle-restricted overload.
            map.FOV.CalculateAppend(
                map.Terrain.Bounds().Center, 5, Radius.Square, 270, 60);
            Console.WriteLine("\nFOV After append:");
            Console.WriteLine(
                map.FOV.BooleanResultView.ExtendToString(elementStringifier: v => v ? "*" : "-"));

            // Note that in this case, the newly seen and unseen cells represent the entirety of cells
            // that were newly seen prior to the last call to Reset(); eg. the newly seen cells from
            // the last call to CalculateAppend as well as the newly seen cells from the last call to
            // Calculate.
            Console.WriteLine($"Newly seen: {map.FOV.NewlySeen.ExtendToString()}");
            Console.WriteLine($"Newly unseen: {map.FOV.NewlyUnseen.ExtendToString()}");
            #endregion
        }
    }
}
